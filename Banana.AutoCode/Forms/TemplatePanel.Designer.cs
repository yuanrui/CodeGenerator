namespace Banana.AutoCode.Forms
{
    partial class TemplatePanel
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
            this.tvTemplates = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // tvTemplates
            // 
            this.tvTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTemplates.Location = new System.Drawing.Point(0, 0);
            this.tvTemplates.Name = "tvTemplates";
            this.tvTemplates.Size = new System.Drawing.Size(312, 603);
            this.tvTemplates.TabIndex = 0;
            // 
            // TemplatePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 603);
            this.Controls.Add(this.tvTemplates);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight;
            this.Name = "TemplatePanel";
            this.Text = "TemplatePanel";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvTemplates;

    }
}