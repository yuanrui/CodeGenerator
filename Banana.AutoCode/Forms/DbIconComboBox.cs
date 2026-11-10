//------------------------------------------------------------------------------
// <copyright file="DbIconComboBox.cs">
//    Copyright (c) 2025, https://github.com/yuanrui All rights reserved.
// </copyright>
// <author>Yuan Rui</author>
// <date>2026-11-10 11:00:00</date>
//------------------------------------------------------------------------------

using Banana.AutoCode.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Banana.AutoCode.Forms
{
    /// <summary>
    /// 带数据库类型图标的 ComboBox owner-draw 绘制与下拉宽度计算。
    /// 图标取自 <see cref="DbProviderIcons"/> 的静态缓存，这里不创建位图副本。
    /// </summary>
    /// <remarks>
    /// 只适用于 <see cref="ComboBoxStyle.DropDownList"/>：
    /// 可编辑样式的编辑区是原生 EDIT 窗口，WinForms 不会为它触发 DrawItem，
    /// 也无法用子控件稳定覆盖；DropDownList 的收起区域会以
    /// <see cref="DrawItemState.ComboBoxEdit"/> 状态回到 DrawItem，图标与列表同源绘制。
    /// 注意收起状态的 State 里始终带 <see cref="DrawItemState.Selected"/>，
    /// 因此编辑区不能调用 <see cref="DrawItemEventArgs.DrawBackground"/>，
    /// 否则会被填成 SystemColors.Highlight 的蓝底。
    /// </remarks>
    internal static class DbIconComboBox
    {
        internal const Int32 IconSize = 16;
        internal const Int32 IconPadding = 2;
        internal const Int32 ItemHeight = 20;

        /// <summary>
        /// 启用 owner-draw 并统一下拉行高。
        /// </summary>
        internal static void Apply(ComboBox combo)
        {
            if (combo == null)
            {
                return;
            }

            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.ItemHeight = ItemHeight;
        }

        /// <summary>
        /// 绘制「图标 + 文本」。编辑区按控件背景色绘制，列表项保留系统选中/悬停高亮。
        /// </summary>
        internal static void Draw(DrawItemEventArgs e, ComboBox combo, String text, String providerName)
        {
            if (IsEditArea(e))
            {
                FillEditBackground(e, combo);

                // 无选中项：只留正常背景，不画图标、文字与焦点框。
                if (e.Index < 0 || String.IsNullOrEmpty(text))
                {
                    return;
                }

                DrawIcon(e, providerName);
                DrawText(e, text, combo.ForeColor);
                DrawFocus(e);
                return;
            }

            e.DrawBackground();

            if (e.Index < 0 || String.IsNullOrEmpty(text))
            {
                return;
            }

            DrawIcon(e, providerName);
            DrawText(e, text, e.ForeColor);
            DrawFocus(e);
        }

        /// <summary>
        /// 设计时或数据源为空时的降级绘制：只铺背景、画文本与焦点框，不取图标。
        /// </summary>
        internal static void DrawPlain(DrawItemEventArgs e, ComboBox combo, String text)
        {
            if (IsEditArea(e))
            {
                FillEditBackground(e, combo);

                if (e.Index < 0 || String.IsNullOrEmpty(text))
                {
                    return;
                }

                DrawText(e, text, combo.ForeColor);
                DrawFocus(e);
                return;
            }

            e.DrawBackground();

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            DrawText(e, text, e.ForeColor);
            DrawFocus(e);
        }

        /// <summary>
        /// 让下拉宽度容纳图标、最长文本与滚动条，避免文字被截断。
        /// </summary>
        internal static void FitDropDownWidth(ComboBox combo)
        {
            if (combo == null)
            {
                return;
            }

            var width = combo.Width;

            foreach (var item in combo.Items)
            {
                var text = combo.GetItemText(item);
                if (String.IsNullOrEmpty(text))
                {
                    continue;
                }

                var textWidth = TextRenderer.MeasureText(text, combo.Font).Width;
                width = Math.Max(width, textWidth + IconSize + IconPadding * 3 + SystemInformation.VerticalScrollBarWidth);
            }

            combo.DropDownWidth = width;
        }

        private static Boolean IsEditArea(DrawItemEventArgs e)
        {
            return (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
        }

        /// <summary>
        /// 编辑区背景固定用控件自身的语义色，避免 <see cref="DrawItemEventArgs.DrawBackground"/>
        /// 在 Selected 状态下填系统高亮。
        /// </summary>
        private static void FillEditBackground(DrawItemEventArgs e, ComboBox combo)
        {
            var backColor = combo == null ? SystemColors.Window : combo.BackColor;

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
        }

        private static void DrawIcon(DrawItemEventArgs e, String providerName)
        {
            var image = DbProviderIcons.GetImage(providerName);
            var bounds = e.Bounds;
            var iconTop = bounds.Top + Math.Max(0, (bounds.Height - IconSize) / 2);
            var iconRect = new Rectangle(bounds.Left + IconPadding, iconTop, IconSize, IconSize);

            e.Graphics.DrawImage(image, iconRect);
        }

        private static void DrawText(DrawItemEventArgs e, String text, Color foreColor)
        {
            var bounds = e.Bounds;
            var textLeft = bounds.Left + IconPadding + IconSize + IconPadding;
            var textRect = new Rectangle(textLeft, bounds.Top,
                Math.Max(0, bounds.Right - textLeft - IconPadding), bounds.Height);

            TextRenderer.DrawText(e.Graphics, text, e.Font, textRect, foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static void DrawFocus(DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus
                && (e.State & DrawItemState.NoFocusRect) == 0)
            {
                e.DrawFocusRectangle();
            }
        }
    }
}
