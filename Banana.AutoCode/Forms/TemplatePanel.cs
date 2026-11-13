using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.Core;
using WeifenLuo.WinFormsUI.Docking;
using Banana.AutoCode.Resources;

namespace Banana.AutoCode.Forms
{
    public partial class TemplatePanel : DockContent, IDisposable
    {
        const String DIRECTORY_ICON = "dir";
        const Int32 CHECK_BOX_SIZE = 13;
        const Int32 CHECK_BOX_GAP = 4;
        protected Hashtable FileMap;

        private String _basePath;
        private readonly Dictionary<TreeNode, CheckState> _checkStateCache = new Dictionary<TreeNode, CheckState>();
        private readonly Dictionary<TreeNode, Boolean> _checkableCache = new Dictionary<TreeNode, Boolean>();
        private readonly Dictionary<TreeNode, Int32> _textWidthCache = new Dictionary<TreeNode, Int32>();
        private readonly System.Windows.Forms.Timer _persistTimer = new System.Windows.Forms.Timer();
        private Boolean _persistPending;

        public TemplatePanel()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            FileMap = new Hashtable();
            var iconList = new ImageList();
            var basePath = GetBasePath();
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                Trace.WriteLine(string.Format(Localization.Templates_Directory_Not_Exists_Auto_Created, basePath));
            }

            InitImageList(iconList, basePath);
            tvTemplates.ImageList = iconList;

            // 原生 CheckBoxes 会给每个节点强制绘制复选框，无法用 StateImageIndex 隐藏；
            // 因此关闭原生勾选框，改为 OwnerDrawText 自绘：仅可勾选节点绘制三态复选框。
            tvTemplates.CheckBoxes = false;
            tvTemplates.DrawMode = TreeViewDrawMode.OwnerDrawText;
            // 整行都可命中/选中，不再要求精确点在文字上。
            tvTemplates.FullRowSelect = true;
            tvTemplates.DrawNode += new DrawTreeNodeEventHandler(tvTemplates_DrawNode);
            tvTemplates.MouseDown += new MouseEventHandler(tvTemplates_MouseDown);
            tvTemplates.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(tvTemplates_NodeMouseDoubleClick);

            // 勾选状态延迟批量保存：点击时只更新界面，停止点击后再写一次配置。
            _persistTimer.Interval = 400;
            _persistTimer.Tick += new EventHandler(PersistTimer_Tick);
            this.FormClosing += new FormClosingEventHandler(TemplatePanel_FormClosing);

            RefreshTreeView();
        }

        void tvTemplates_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var tuple = e.Node.Tag as Tuple<Boolean, String>;

            if (tuple == null || !tuple.Item1)
            {
                return;
            }

            var path = tuple.Item2;
            var outputPanel = FileMap[path] as OutputPanel;
            if (outputPanel == null)
            {
                outputPanel = new OutputPanel(true, false, true);
                outputPanel.Text = GetFileName(path);
                var text = File.ReadAllText(path);
                outputPanel.AppendText(text, Color.BlueViolet);
                outputPanel.Show(((Main)this.ParentForm).MainDockPanel, DockState.Document);
                
                FileMap[path] = outputPanel;
            }
            else
            {
                outputPanel.Activate();
            }
        }
        
        private string GetBasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigConstants.TEMPLATES_DIR);
        }

        protected void RefreshTreeView()
        {
            FlushSelection();

            tvTemplates.Nodes.Clear();
            _checkStateCache.Clear();
            _checkableCache.Clear();
            _textWidthCache.Clear();

            foreach (DictionaryEntry item in FileMap)
            {
                var form = item.Value as OutputPanel;
                if (form == null)
                {
                    continue;
                }

                form.Close();
            }

            FileMap.Clear();

            var basePath = GetBasePath();
            _basePath = basePath;

            if (!Directory.Exists(basePath))
            {
                Trace.WriteLine(String.Format(Localization.Templates_Directory_Not_Exists, basePath));
                return;
            }

            var rootNode = new TreeNode();

            // 读取独立配置节点（含旧 appSettings 键迁移）；异常时内部回退为默认全选。
            TemplateSelection.Initialize();
            BuildFileTreeNode(rootNode, basePath);

            foreach (TreeNode node in rootNode.Nodes)
            {
                tvTemplates.Nodes.Add(node);
            }

            // 启动/刷新后保持折叠：只展示第一层节点，三态仍按整棵子树计算。
            tvTemplates.Invalidate();
        }

        private void BuildFileTreeNode(TreeNode currentNode, string path)
        {
            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                var node = new TreeNode(GetFileName(dir));
                node.Tag = Tuple.Create(false, dir);
                node.ImageKey = DIRECTORY_ICON;
                node.SelectedImageKey = node.ImageKey;

                BuildFileTreeNode(node, dir);

                currentNode.Nodes.Add(node);
            }

            var files = Directory.GetFiles(path);
            
            foreach (var filePath in files)
            {
                var node = new TreeNode(GetFileName(filePath));
                node.Tag = Tuple.Create(true, filePath);
                node.ImageKey = GetExtension(filePath);
                node.SelectedImageKey = node.ImageKey;

                if (TemplateSelection.IsTemplateFile(filePath))
                {
                    // 有记录按记录回填；没有记录的模板默认勾选，避免新增模板被静默漏掉。
                    var relativePath = TemplateSelection.GetRelativePath(_basePath, filePath);
                    node.Checked = TemplateSelection.IsEnabled(relativePath);
                }

                currentNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// 三态基数只统计可勾选的 .tt 节点；.ttinclude、.thrift 等不可勾选节点不参与。
        /// </summary>
        private void AggregateSelection(TreeNode node, ref Boolean hasTemplate, ref Boolean allChecked, ref Boolean anyChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                if (IsTemplateNode(child))
                {
                    hasTemplate = true;
                    allChecked &= child.Checked;
                    anyChecked |= child.Checked;
                }
                else if (IsDirectoryNode(child))
                {
                    AggregateSelection(child, ref hasTemplate, ref allChecked, ref anyChecked);
                }
            }
        }

        private CheckState GetCheckState(TreeNode node)
        {
            if (IsTemplateNode(node))
            {
                return node.Checked ? CheckState.Checked : CheckState.Unchecked;
            }

            CheckState cached;
            if (_checkStateCache.TryGetValue(node, out cached))
            {
                return cached;
            }

            Boolean hasTemplate = false;
            Boolean allChecked = true;
            Boolean anyChecked = false;
            AggregateSelection(node, ref hasTemplate, ref allChecked, ref anyChecked);

            CheckState state;
            if (!hasTemplate)
            {
                state = CheckState.Unchecked;
            }
            else if (allChecked)
            {
                state = CheckState.Checked;
            }
            else
            {
                state = anyChecked ? CheckState.Indeterminate : CheckState.Unchecked;
            }

            _checkStateCache[node] = state;
            return state;
        }

        private void ApplySelection(TreeNode node, Boolean value)
        {
            foreach (TreeNode child in node.Nodes)
            {
                if (IsTemplateNode(child))
                {
                    child.Checked = value;
                }
                else if (IsDirectoryNode(child))
                {
                    ApplySelection(child, value);
                }
            }
        }

        /// <summary>
        /// 目录：全选时点击变全不选，半勾选/全不选时点击变全选；文件：切换自身。
        /// </summary>
        private void ToggleNode(TreeNode node)
        {
            if (IsDirectoryNode(node))
            {
                ApplySelection(node, GetCheckState(node) != CheckState.Checked);
            }
            else if (IsTemplateNode(node))
            {
                node.Checked = !node.Checked;
            }
            else
            {
                return;
            }

            // 点击路径上不做全树三态汇总：清空缓存，由下次绘制按需计算。
            _checkStateCache.Clear();
            PersistSelection();
            tvTemplates.Invalidate();
        }

        private void tvTemplates_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            var node = tvTemplates.GetNodeAt(e.X, e.Y);

            if (node == null)
            {
                return;
            }

            // 文字、图标、行尾空白都落在同一行内：统一显式选中，避免依赖系统对自绘文本的命中判断。
            if (tvTemplates.SelectedNode != node)
            {
                tvTemplates.SelectedNode = node;
            }

            // 勾选切换只在点击复选框本身时发生，点行内其他区域不会改变勾选。
            if (!IsCheckable(node))
            {
                return;
            }

            var checkBox = GetCheckBoxBounds(node.Bounds);
            if (checkBox.Contains(e.X, e.Y))
            {
                ToggleNode(node);
            }
        }

        private void tvTemplates_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = false;

            var node = e.Node;
            var textLeft = e.Bounds.Left;

            if (IsCheckable(node))
            {
                var checkBox = GetCheckBoxBounds(e.Bounds);
                DrawCheckBox(e.Graphics, checkBox, GetCheckState(node));
                textLeft = checkBox.Right + CHECK_BOX_GAP;
            }

            var selected = (e.State & TreeNodeStates.Selected) != 0;
            var color = selected && tvTemplates.Focused ? SystemColors.HighlightText : node.ForeColor;
            var font = node.NodeFont ?? tvTemplates.Font;

            // 自绘文本不能依赖 e.Bounds 的宽度（它按不含复选框的布局测量），
            // 也不使用 EndEllipsis，否则获得/失去焦点时会截断成省略号。
            // 文本宽度按节点缓存，避免每次重绘都重新测量。
            Int32 textWidth;
            if (!_textWidthCache.TryGetValue(node, out textWidth))
            {
                textWidth = TextRenderer.MeasureText(
                    e.Graphics,
                    node.Text,
                    font,
                    new Size(10000, e.Bounds.Height),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width;
                _textWidthCache[node] = textWidth;
            }

            var width = Math.Max(textWidth + 2, tvTemplates.ClientSize.Width - textLeft);
            var textBounds = new Rectangle(textLeft, e.Bounds.Top, width, e.Bounds.Height);

            // 系统只在 e.Bounds 范围内绘制选中背景，文字宽于该范围时高亮文字会落在白色背景上而“消失”；
            // 这里按实际文字区域补画选中背景，保证获得焦点时文本同样完整可见。
            if (selected && tvTemplates.Focused)
            {
                using (var highlight = new SolidBrush(SystemColors.Highlight))
                {
                    e.Graphics.FillRectangle(highlight, textBounds);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                node.Text,
                font,
                textBounds,
                color,
                TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        private static Rectangle GetCheckBoxBounds(Rectangle labelBounds)
        {
            var top = labelBounds.Top + (labelBounds.Height - CHECK_BOX_SIZE) / 2;
            return new Rectangle(labelBounds.Left + 1, top, CHECK_BOX_SIZE, CHECK_BOX_SIZE);
        }

        private static void DrawCheckBox(Graphics graphics, Rectangle checkBox, CheckState state)
        {
            using (var background = new SolidBrush(Color.White))
            using (var border = new Pen(Color.FromArgb(122, 122, 122)))
            {
                graphics.FillRectangle(background, checkBox);
                graphics.DrawRectangle(border, checkBox.X, checkBox.Y, checkBox.Width - 1, checkBox.Height - 1);
            }

            if (state == CheckState.Checked)
            {
                using (var mark = new Pen(Color.FromArgb(0, 120, 215), 2f))
                {
                    graphics.DrawLines(mark, new[]
                    {
                        new Point(checkBox.Left + 3, checkBox.Top + 6),
                        new Point(checkBox.Left + 5, checkBox.Top + 9),
                        new Point(checkBox.Left + 10, checkBox.Top + 3)
                    });
                }
            }
            else if (state == CheckState.Indeterminate)
            {
                // 半勾选：实心方块，与对勾和空框都能明显区分。
                using (var fill = new SolidBrush(Color.FromArgb(120, 120, 120)))
                {
                    graphics.FillRectangle(fill, checkBox.Left + 3, checkBox.Top + 3, 7, 7);
                }
            }
        }

        private Boolean IsCheckable(TreeNode node)
        {
            if (IsTemplateNode(node))
            {
                return true;
            }

            if (!IsDirectoryNode(node))
            {
                return false;
            }

            Boolean checkable;
            if (_checkableCache.TryGetValue(node, out checkable))
            {
                return checkable;
            }

            Boolean hasTemplate = false;
            Boolean allChecked = true;
            Boolean anyChecked = false;
            AggregateSelection(node, ref hasTemplate, ref allChecked, ref anyChecked);

            _checkableCache[node] = hasTemplate;
            return hasTemplate;
        }

        private static Boolean IsDirectoryNode(TreeNode node)
        {
            var tuple = node == null ? null : node.Tag as Tuple<Boolean, String>;
            return tuple != null && !tuple.Item1;
        }

        private static Boolean IsTemplateNode(TreeNode node)
        {
            var tuple = node == null ? null : node.Tag as Tuple<Boolean, String>;
            return tuple != null && tuple.Item1 && TemplateSelection.IsTemplateFile(tuple.Item2);
        }

        /// <summary>
        /// 标记勾选状态已变化，延迟合并写入配置，避免每次点击都同步写文件阻塞 UI。
        /// </summary>
        private void PersistSelection()
        {
            _persistPending = true;
            _persistTimer.Stop();
            _persistTimer.Start();
        }

        private void PersistTimer_Tick(object sender, EventArgs e)
        {
            FlushSelection();
        }

        private void TemplatePanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            FlushSelection();
        }

        /// <summary>
        /// 立即写入尚未保存的勾选状态；关闭窗口、刷新树或程序退出前调用，保证状态不丢。
        /// </summary>
        internal void FlushPendingSelection()
        {
            FlushSelection();
        }

        private void FlushSelection()
        {
            _persistTimer.Stop();

            if (!_persistPending)
            {
                return;
            }

            _persistPending = false;

            // 树已清空/控件已释放时不要用空状态覆盖已保存的勾选记录。
            if (tvTemplates.Nodes.Count == 0)
            {
                return;
            }

            var states = new List<KeyValuePair<String, Boolean>>();
            CollectSelection(tvTemplates.Nodes, states);
            TemplateSelection.Save(states);
        }

        private void CollectSelection(TreeNodeCollection nodes, List<KeyValuePair<String, Boolean>> states)
        {
            foreach (TreeNode node in nodes)
            {
                if (IsTemplateNode(node))
                {
                    var tuple = node.Tag as Tuple<Boolean, String>;
                    var relativePath = TemplateSelection.GetRelativePath(_basePath, tuple.Item2);
                    states.Add(new KeyValuePair<String, Boolean>(relativePath, node.Checked));
                }
                else if (IsDirectoryNode(node))
                {
                    CollectSelection(node.Nodes, states);
                }
            }
        }

        private void InitImageList(ImageList iconList, string path)
        {
            if (! Directory.Exists(path))
            {
                return;
            }
            
            var baseIcon = IconUtils.GetIcon(path);
            if (baseIcon != null)
            {
                iconList.Images.Add(DIRECTORY_ICON, baseIcon);
            }
            
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var ext = GetExtension(filePath);
                if (! iconList.Images.ContainsKey(ext))
                {
                    iconList.Images.Add(ext, Icon.ExtractAssociatedIcon(filePath));
                }
            }
        }

        private string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        private string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public override void Refresh()
        {
            base.Refresh();

            RefreshTreeView();
        }

        void IDisposable.Dispose()
        {
            this.FileMap.Clear();
            this.FileMap = null;

            this.Dispose(true);
        }
    }
}
