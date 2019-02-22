namespace Banana.AutoCode.Forms
{
    partial class TablePanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TablePanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.txtClazz = new System.Windows.Forms.TextBox();
            this.lblTable = new System.Windows.Forms.Label();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RawType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DbType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsPrimaryKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsForeignKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsUnique = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsNullAble = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblComment = new System.Windows.Forms.Label();
            this.lblClazz = new System.Windows.Forms.Label();
            this.txtTable = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.txtComment, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtClazz, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTable, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvColumns, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblComment, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblClazz, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtTable, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // txtComment
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.txtComment, 3);
            resources.ApplyResources(this.txtComment, "txtComment");
            this.txtComment.Name = "txtComment";
            this.txtComment.ReadOnly = true;
            // 
            // txtClazz
            // 
            resources.ApplyResources(this.txtClazz, "txtClazz");
            this.txtClazz.Name = "txtClazz";
            this.txtClazz.ReadOnly = true;
            this.txtClazz.ShortcutsEnabled = false;
            // 
            // lblTable
            // 
            resources.ApplyResources(this.lblTable, "lblTable");
            this.lblTable.Name = "lblTable";
            // 
            // dgvColumns
            // 
            this.dgvColumns.AllowUserToAddRows = false;
            this.dgvColumns.AllowUserToDeleteRows = false;
            this.dgvColumns.AllowUserToResizeColumns = false;
            this.dgvColumns.AllowUserToResizeRows = false;
            this.dgvColumns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnName,
            this.RawType,
            this.DbType,
            this.Type,
            this.Comment,
            this.IsPrimaryKey,
            this.IsForeignKey,
            this.IsUnique,
            this.IsNullAble});
            this.tableLayoutPanel1.SetColumnSpan(this.dgvColumns, 4);
            resources.ApplyResources(this.dgvColumns, "dgvColumns");
            this.dgvColumns.MultiSelect = false;
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.ReadOnly = true;
            this.dgvColumns.RowHeadersVisible = false;
            this.dgvColumns.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgvColumns.RowTemplate.Height = 23;
            // 
            // ColumnName
            // 
            this.ColumnName.DataPropertyName = "Name";
            this.ColumnName.FillWeight = 17.7665F;
            resources.ApplyResources(this.ColumnName, "ColumnName");
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            this.ColumnName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // RawType
            // 
            this.RawType.DataPropertyName = "RawType";
            this.RawType.FillWeight = 17.7665F;
            resources.ApplyResources(this.RawType, "RawType");
            this.RawType.Name = "RawType";
            this.RawType.ReadOnly = true;
            this.RawType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // DbType
            // 
            this.DbType.DataPropertyName = "DataType";
            resources.ApplyResources(this.DbType, "DbType");
            this.DbType.Name = "DbType";
            this.DbType.ReadOnly = true;
            this.DbType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Type
            // 
            this.Type.DataPropertyName = "TypeName";
            this.Type.FillWeight = 593.401F;
            resources.ApplyResources(this.Type, "Type");
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Comment
            // 
            this.Comment.DataPropertyName = "Comment";
            this.Comment.FillWeight = 17.7665F;
            resources.ApplyResources(this.Comment, "Comment");
            this.Comment.Name = "Comment";
            this.Comment.ReadOnly = true;
            // 
            // IsPrimaryKey
            // 
            this.IsPrimaryKey.DataPropertyName = "IsPrimaryKey";
            this.IsPrimaryKey.FillWeight = 17.7665F;
            resources.ApplyResources(this.IsPrimaryKey, "IsPrimaryKey");
            this.IsPrimaryKey.Name = "IsPrimaryKey";
            this.IsPrimaryKey.ReadOnly = true;
            this.IsPrimaryKey.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsForeignKey
            // 
            this.IsForeignKey.DataPropertyName = "IsForeignKey";
            this.IsForeignKey.FillWeight = 17.7665F;
            resources.ApplyResources(this.IsForeignKey, "IsForeignKey");
            this.IsForeignKey.Name = "IsForeignKey";
            this.IsForeignKey.ReadOnly = true;
            this.IsForeignKey.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsUnique
            // 
            this.IsUnique.DataPropertyName = "IsUnique";
            resources.ApplyResources(this.IsUnique, "IsUnique");
            this.IsUnique.Name = "IsUnique";
            this.IsUnique.ReadOnly = true;
            this.IsUnique.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsNullAble
            // 
            this.IsNullAble.DataPropertyName = "IsNullAble";
            this.IsNullAble.FillWeight = 17.7665F;
            resources.ApplyResources(this.IsNullAble, "IsNullAble");
            this.IsNullAble.Name = "IsNullAble";
            this.IsNullAble.ReadOnly = true;
            this.IsNullAble.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // lblComment
            // 
            resources.ApplyResources(this.lblComment, "lblComment");
            this.lblComment.Name = "lblComment";
            // 
            // lblClazz
            // 
            resources.ApplyResources(this.lblClazz, "lblClazz");
            this.lblClazz.Name = "lblClazz";
            // 
            // txtTable
            // 
            resources.ApplyResources(this.txtTable, "txtTable");
            this.txtTable.Name = "txtTable";
            this.txtTable.ReadOnly = true;
            // 
            // TablePanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
            this.Name = "TablePanel";
            this.ShowIcon = false;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvColumns;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.Label lblClazz;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.TextBox txtClazz;
        private System.Windows.Forms.TextBox txtTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn RawType;
        private System.Windows.Forms.DataGridViewTextBoxColumn DbType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsPrimaryKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsForeignKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsUnique;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsNullAble;

    }
}