namespace Banana.AutoCode.Forms
{
    partial class OptionsPanel
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtRemovePrefix = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRemoveSuffix = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Remove Prefix";
            // 
            // txtRemovePrefix
            // 
            this.txtRemovePrefix.Location = new System.Drawing.Point(101, 16);
            this.txtRemovePrefix.Name = "txtRemovePrefix";
            this.txtRemovePrefix.Size = new System.Drawing.Size(277, 21);
            this.txtRemovePrefix.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Remove Suffix";
            // 
            // txtRemoveSuffix
            // 
            this.txtRemoveSuffix.Location = new System.Drawing.Point(101, 53);
            this.txtRemoveSuffix.Name = "txtRemoveSuffix";
            this.txtRemoveSuffix.Size = new System.Drawing.Size(277, 21);
            this.txtRemoveSuffix.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(101, 89);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 135);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtRemoveSuffix);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRemovePrefix);
            this.Controls.Add(this.label1);
            this.Name = "OptionsPanel";
            this.Text = "OptionsPanel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRemovePrefix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRemoveSuffix;
        private System.Windows.Forms.Button btnSave;

    }
}