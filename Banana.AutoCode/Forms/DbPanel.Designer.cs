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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cbConnStrings = new System.Windows.Forms.ComboBox();
            this.tvDb = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pboxDbRefresh = new System.Windows.Forms.PictureBox();
            this.pboxDbDelete = new System.Windows.Forms.PictureBox();
            this.pboxDbEdit = new System.Windows.Forms.PictureBox();
            this.pboxDbAdd = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbRefresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbDelete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbAdd)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.cbConnStrings, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvDb, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // cbConnStrings
            // 
            resources.ApplyResources(this.cbConnStrings, "cbConnStrings");
            this.cbConnStrings.FormattingEnabled = true;
            this.cbConnStrings.Name = "cbConnStrings";
            this.cbConnStrings.SelectedIndexChanged += new System.EventHandler(this.cbConnStrings_SelectedIndexChanged);
            // 
            // tvDb
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tvDb, 2);
            resources.ApplyResources(this.tvDb, "tvDb");
            this.tvDb.Name = "tvDb";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.pboxDbRefresh);
            this.panel1.Controls.Add(this.pboxDbDelete);
            this.panel1.Controls.Add(this.pboxDbEdit);
            this.panel1.Controls.Add(this.pboxDbAdd);
            this.panel1.Name = "panel1";
            // 
            // pboxDbRefresh
            // 
            this.pboxDbRefresh.Image = global::Banana.AutoCode.Properties.Resources.Icons_16x16_DatabaseRefresh;
            resources.ApplyResources(this.pboxDbRefresh, "pboxDbRefresh");
            this.pboxDbRefresh.Name = "pboxDbRefresh";
            this.pboxDbRefresh.TabStop = false;
            this.pboxDbRefresh.Click += new System.EventHandler(this.pboxDbRefresh_Click);
            // 
            // pboxDbDelete
            // 
            this.pboxDbDelete.Image = global::Banana.AutoCode.Properties.Resources.Icons_16x16_DatabaseDelete;
            resources.ApplyResources(this.pboxDbDelete, "pboxDbDelete");
            this.pboxDbDelete.Name = "pboxDbDelete";
            this.pboxDbDelete.TabStop = false;
            this.pboxDbDelete.Click += new System.EventHandler(this.pboxDbDelete_Click);
            // 
            // pboxDbEdit
            // 
            this.pboxDbEdit.Image = global::Banana.AutoCode.Properties.Resources.Icons_16x16_DatabaseEdit;
            resources.ApplyResources(this.pboxDbEdit, "pboxDbEdit");
            this.pboxDbEdit.Name = "pboxDbEdit";
            this.pboxDbEdit.TabStop = false;
            this.pboxDbEdit.Click += new System.EventHandler(this.pboxDbEdit_Click);
            // 
            // pboxDbAdd
            // 
            this.pboxDbAdd.Image = global::Banana.AutoCode.Properties.Resources.Icons_16x16_DatabaseAdd;
            resources.ApplyResources(this.pboxDbAdd, "pboxDbAdd");
            this.pboxDbAdd.Name = "pboxDbAdd";
            this.pboxDbAdd.TabStop = false;
            this.pboxDbAdd.Click += new System.EventHandler(this.pboxDbAdd_Click);
            // 
            // DbPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CloseButtonVisible = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft;
            this.Name = "DbPanel";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbRefresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbDelete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pboxDbAdd)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cbConnStrings;
        private System.Windows.Forms.TreeView tvDb;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pboxDbEdit;
        private System.Windows.Forms.PictureBox pboxDbAdd;
        private System.Windows.Forms.PictureBox pboxDbDelete;
        private System.Windows.Forms.PictureBox pboxDbRefresh;
    }
}