namespace VPKInstaller
{
  partial class InstallWindow
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
      this.PathTextBox = new System.Windows.Forms.TextBox();
      this.SelectFolderBT = new System.Windows.Forms.Button();
      this.InstallBT = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 186);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(89, 12);
      this.label1.TabIndex = 0;
      this.label1.Text = "选择安装路径：";
      // 
      // PathTextBox
      // 
      this.PathTextBox.Location = new System.Drawing.Point(18, 201);
      this.PathTextBox.Multiline = true;
      this.PathTextBox.Name = "PathTextBox";
      this.PathTextBox.Size = new System.Drawing.Size(311, 21);
      this.PathTextBox.TabIndex = 1;
      this.PathTextBox.TextChanged += new System.EventHandler(this.PathTextBox_TextChanged);
      // 
      // SelectFolderBT
      // 
      this.SelectFolderBT.Location = new System.Drawing.Point(344, 201);
      this.SelectFolderBT.Name = "SelectFolderBT";
      this.SelectFolderBT.Size = new System.Drawing.Size(75, 23);
      this.SelectFolderBT.TabIndex = 2;
      this.SelectFolderBT.Text = "浏览";
      this.SelectFolderBT.UseVisualStyleBackColor = true;
      this.SelectFolderBT.Click += new System.EventHandler(this.SelectFolderBT_Click);
      // 
      // InstallBT
      // 
      this.InstallBT.Enabled = false;
      this.InstallBT.Location = new System.Drawing.Point(597, 326);
      this.InstallBT.Name = "InstallBT";
      this.InstallBT.Size = new System.Drawing.Size(75, 23);
      this.InstallBT.TabIndex = 3;
      this.InstallBT.Text = "安装";
      this.InstallBT.UseVisualStyleBackColor = true;
      this.InstallBT.Click += new System.EventHandler(this.InstallBT_Click);
      // 
      // InstallWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(684, 361);
      this.Controls.Add(this.InstallBT);
      this.Controls.Add(this.SelectFolderBT);
      this.Controls.Add(this.PathTextBox);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.Name = "InstallWindow";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "InstallWindow";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox PathTextBox;
    private System.Windows.Forms.Button SelectFolderBT;
    private System.Windows.Forms.Button InstallBT;
  }
}