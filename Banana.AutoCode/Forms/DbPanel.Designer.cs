namespace Banana.AutoCode.Forms
{
    partial class DbPanel
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
            this.cbConnStrings = new System.Windows.Forms.ComboBox();
            this.tvDb = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 59.85915F));
            this.tableLayoutPanel1.Controls.Add(this.cbConnStrings, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvDb, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 604F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(390, 650);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // cbConnStrings
            // 
            this.cbConnStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbConnStrings.FormattingEnabled = true;
            this.cbConnStrings.Location = new System.Drawing.Point(3, 3);
            this.cbConnStrings.Name = "cbConnStrings";
            this.cbConnStrings.Size = new System.Drawing.Size(384, 20);
            this.cbConnStrings.TabIndex = 1;
            this.cbConnStrings.SelectedIndexChanged += new System.EventHandler(this.cbConnStrings_SelectedIndexChanged);
            // 
            // tvDb
            // 
            this.tvDb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDb.Location = new System.Drawing.Point(3, 29);
            this.tvDb.Name = "tvDb";
            this.tvDb.Size = new System.Drawing.Size(384, 618);
            this.tvDb.TabIndex = 2;
            // 
            // DbPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 650);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft;
            this.Name = "DbPanel";
            this.Text = "DatabaseExplorer";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cbConnStrings;
        private System.Windows.Forms.TreeView tvDb;

    }
}