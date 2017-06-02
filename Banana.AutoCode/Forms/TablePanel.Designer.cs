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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.txtClazz = new System.Windows.Forms.TextBox();
            this.lblTable = new System.Windows.Forms.Label();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.lblComment = new System.Windows.Forms.Label();
            this.lblClazz = new System.Windows.Forms.Label();
            this.txtTable = new System.Windows.Forms.TextBox();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RawType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DbType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsPrimaryKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsForeignKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsUnique = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsNullAble = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.txtComment, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtClazz, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTable, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvColumns, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblComment, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblClazz, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtTable, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(942, 521);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtComment
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.txtComment, 3);
            this.txtComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtComment.Location = new System.Drawing.Point(103, 33);
            this.txtComment.Name = "txtComment";
            this.txtComment.ReadOnly = true;
            this.txtComment.Size = new System.Drawing.Size(836, 21);
            this.txtComment.TabIndex = 10;
            // 
            // txtClazz
            // 
            this.txtClazz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtClazz.Location = new System.Drawing.Point(574, 3);
            this.txtClazz.Name = "txtClazz";
            this.txtClazz.ReadOnly = true;
            this.txtClazz.Size = new System.Drawing.Size(365, 21);
            this.txtClazz.TabIndex = 9;
            // 
            // lblTable
            // 
            this.lblTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTable.AutoSize = true;
            this.lblTable.Location = new System.Drawing.Point(26, 8);
            this.lblTable.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(71, 12);
            this.lblTable.TabIndex = 6;
            this.lblTable.Text = "Table Name:";
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
            this.dgvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumns.Location = new System.Drawing.Point(3, 63);
            this.dgvColumns.MultiSelect = false;
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.ReadOnly = true;
            this.dgvColumns.RowHeadersVisible = false;
            this.dgvColumns.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgvColumns.RowTemplate.Height = 23;
            this.dgvColumns.Size = new System.Drawing.Size(936, 455);
            this.dgvColumns.TabIndex = 3;
            // 
            // lblComment
            // 
            this.lblComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(44, 38);
            this.lblComment.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(53, 12);
            this.lblComment.TabIndex = 5;
            this.lblComment.Text = "Comment:";
            // 
            // lblClazz
            // 
            this.lblClazz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClazz.AutoSize = true;
            this.lblClazz.Location = new System.Drawing.Point(497, 8);
            this.lblClazz.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblClazz.Name = "lblClazz";
            this.lblClazz.Size = new System.Drawing.Size(71, 12);
            this.lblClazz.TabIndex = 7;
            this.lblClazz.Text = "Class Name:";
            // 
            // txtTable
            // 
            this.txtTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTable.Location = new System.Drawing.Point(103, 3);
            this.txtTable.Name = "txtTable";
            this.txtTable.ReadOnly = true;
            this.txtTable.Size = new System.Drawing.Size(365, 21);
            this.txtTable.TabIndex = 8;
            // 
            // ColumnName
            // 
            this.ColumnName.DataPropertyName = "Name";
            this.ColumnName.FillWeight = 17.7665F;
            this.ColumnName.HeaderText = "列名";
            this.ColumnName.MinimumWidth = 100;
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            this.ColumnName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // RawType
            // 
            this.RawType.DataPropertyName = "RawType";
            this.RawType.FillWeight = 17.7665F;
            this.RawType.HeaderText = "Sql Type";
            this.RawType.MinimumWidth = 100;
            this.RawType.Name = "RawType";
            this.RawType.ReadOnly = true;
            this.RawType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // DbType
            // 
            this.DbType.DataPropertyName = "DataType";
            this.DbType.HeaderText = "Db Type";
            this.DbType.MinimumWidth = 100;
            this.DbType.Name = "DbType";
            this.DbType.ReadOnly = true;
            this.DbType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Type
            // 
            this.Type.DataPropertyName = "TypeName";
            this.Type.FillWeight = 593.401F;
            this.Type.HeaderText = "Csharp Type";
            this.Type.MinimumWidth = 100;
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Comment
            // 
            this.Comment.DataPropertyName = "Comment";
            this.Comment.FillWeight = 17.7665F;
            this.Comment.HeaderText = "列说明";
            this.Comment.MinimumWidth = 180;
            this.Comment.Name = "Comment";
            this.Comment.ReadOnly = true;
            // 
            // IsPrimaryKey
            // 
            this.IsPrimaryKey.DataPropertyName = "IsPrimaryKey";
            this.IsPrimaryKey.FillWeight = 17.7665F;
            this.IsPrimaryKey.HeaderText = "是否主键";
            this.IsPrimaryKey.MinimumWidth = 80;
            this.IsPrimaryKey.Name = "IsPrimaryKey";
            this.IsPrimaryKey.ReadOnly = true;
            this.IsPrimaryKey.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsForeignKey
            // 
            this.IsForeignKey.DataPropertyName = "IsForeignKey";
            this.IsForeignKey.FillWeight = 17.7665F;
            this.IsForeignKey.HeaderText = "是否外键";
            this.IsForeignKey.MinimumWidth = 80;
            this.IsForeignKey.Name = "IsForeignKey";
            this.IsForeignKey.ReadOnly = true;
            this.IsForeignKey.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsUnique
            // 
            this.IsUnique.DataPropertyName = "IsUnique";
            this.IsUnique.HeaderText = "是否唯一";
            this.IsUnique.MinimumWidth = 80;
            this.IsUnique.Name = "IsUnique";
            this.IsUnique.ReadOnly = true;
            this.IsUnique.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // IsNullAble
            // 
            this.IsNullAble.DataPropertyName = "IsNullAble";
            this.IsNullAble.FillWeight = 17.7665F;
            this.IsNullAble.HeaderText = "是否可为空";
            this.IsNullAble.MinimumWidth = 80;
            this.IsNullAble.Name = "IsNullAble";
            this.IsNullAble.ReadOnly = true;
            this.IsNullAble.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // TablePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 521);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
            this.Name = "TablePanel";
            this.Text = "TablePanel";
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