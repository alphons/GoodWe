namespace GoodWe.WinForm
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			userControl11 = new UserControl1();
			SuspendLayout();
			// 
			// userControl11
			// 
			userControl11.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			userControl11.Location = new Point(12, 12);
			userControl11.Name = "userControl11";
			userControl11.Size = new Size(384, 495);
			userControl11.TabIndex = 0;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(398, 508);
			Controls.Add(userControl11);
			Name = "Form1";
			Text = "GoodWe 1.0";
			ResumeLayout(false);
		}

		#endregion

		private UserControl1 userControl11;
	}
}
