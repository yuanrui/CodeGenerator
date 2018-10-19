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
            this.btnRefresh = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.cbConnStrings, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvDb, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnRefresh, 1, 0);
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
            // btnRefresh
            // 
            resources.ApplyResources(this.btnRefresh, "btnRefresh");
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cbConnStrings;
        private System.Windows.Forms.TreeView tvDb;
        private System.Windows.Forms.Button btnRefresh;

    }
}