namespace GoodWe.WinForm
{
	partial class UserControl1
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ListViewItem listViewItem1 = new ListViewItem(new string[] { "Model", "-" }, -1);
			ListViewItem listViewItem2 = new ListViewItem(new string[] { "Serial", "-" }, -1);
			ListViewItem listViewItem3 = new ListViewItem(new string[] { "Rated Power", "-" }, -1);
			comboBox1 = new ComboBox();
			label2 = new Label();
			textBox1 = new TextBox();
			label1 = new Label();
			button1 = new Button();
			button2 = new Button();
			groupBox1 = new GroupBox();
			groupBox2 = new GroupBox();
			listView1 = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			groupBox3 = new GroupBox();
			listViewData = new ListView();
			columnHeader3 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			labelStatus = new Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox3.SuspendLayout();
			SuspendLayout();
			// 
			// comboBox1
			// 
			comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox1.FormattingEnabled = true;
			comboBox1.Items.AddRange(new object[] { "TCP (502)", "UDP (8899)" });
			comboBox1.Location = new Point(267, 23);
			comboBox1.Name = "comboBox1";
			comboBox1.Size = new Size(94, 23);
			comboBox1.TabIndex = 7;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(198, 28);
			label2.Name = "label2";
			label2.Size = new Size(52, 15);
			label2.TabIndex = 6;
			label2.Text = "Protocol";
			// 
			// textBox1
			// 
			textBox1.Location = new Point(85, 22);
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(100, 23);
			textBox1.TabIndex = 5;
			textBox1.Text = "192.168.74.33";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(5, 28);
			label1.Name = "label1";
			label1.Size = new Size(62, 15);
			label1.TabIndex = 4;
			label1.Text = "IP-address";
			// 
			// button1
			// 
			button1.AllowDrop = true;
			button1.Location = new Point(395, 30);
			button1.Name = "button1";
			button1.Size = new Size(75, 23);
			button1.TabIndex = 8;
			button1.Text = "Start";
			button1.UseVisualStyleBackColor = true;
			button1.Click += Start_Click;
			// 
			// button2
			// 
			button2.Enabled = false;
			button2.Location = new Point(395, 59);
			button2.Name = "button2";
			button2.Size = new Size(75, 23);
			button2.TabIndex = 9;
			button2.Text = "Stop";
			button2.UseVisualStyleBackColor = true;
			button2.Click += Stop_Click;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(textBox1);
			groupBox1.Controls.Add(label1);
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(comboBox1);
			groupBox1.Location = new Point(16, 21);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(373, 59);
			groupBox1.TabIndex = 10;
			groupBox1.TabStop = false;
			groupBox1.Text = "connection settings";
			// 
			// groupBox2
			// 
			groupBox2.Controls.Add(listView1);
			groupBox2.Location = new Point(16, 98);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new Size(373, 122);
			groupBox2.TabIndex = 11;
			groupBox2.TabStop = false;
			groupBox2.Text = "Device Information";
			// 
			// listView1
			// 
			listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
			listView1.FullRowSelect = true;
			listView1.GridLines = true;
			listView1.HeaderStyle = ColumnHeaderStyle.None;
			listView1.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3 });
			listView1.Location = new Point(21, 27);
			listView1.Name = "listView1";
			listView1.Size = new Size(340, 76);
			listView1.TabIndex = 0;
			listView1.UseCompatibleStateImageBehavior = false;
			listView1.View = View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Width = 150;
			// 
			// columnHeader2
			// 
			columnHeader2.TextAlign = HorizontalAlignment.Right;
			columnHeader2.Width = 150;
			// 
			// groupBox3
			// 
			groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			groupBox3.Controls.Add(listViewData);
			groupBox3.Location = new Point(17, 255);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new Size(373, 241);
			groupBox3.TabIndex = 12;
			groupBox3.TabStop = false;
			groupBox3.Text = "Data";
			// 
			// listViewData
			// 
			listViewData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			listViewData.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader4 });
			listViewData.FullRowSelect = true;
			listViewData.GridLines = true;
			listViewData.HeaderStyle = ColumnHeaderStyle.None;
			listViewData.Location = new Point(21, 27);
			listViewData.Name = "listViewData";
			listViewData.Size = new Size(340, 208);
			listViewData.TabIndex = 0;
			listViewData.UseCompatibleStateImageBehavior = false;
			listViewData.View = View.Details;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Sensor";
			columnHeader3.Width = 150;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Data";
			columnHeader4.TextAlign = HorizontalAlignment.Right;
			columnHeader4.Width = 150;
			// 
			// labelStatus
			// 
			labelStatus.AutoSize = true;
			labelStatus.Location = new Point(29, 225);
			labelStatus.Name = "labelStatus";
			labelStatus.Size = new Size(51, 15);
			labelStatus.TabIndex = 13;
			labelStatus.Text = "Stopped";
			// 
			// UserControl1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(labelStatus);
			Controls.Add(groupBox3);
			Controls.Add(groupBox2);
			Controls.Add(groupBox1);
			Controls.Add(button2);
			Controls.Add(button1);
			Name = "UserControl1";
			Size = new Size(481, 499);
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox3.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ComboBox comboBox1;
		private Label label2;
		private TextBox textBox1;
		private Label label1;
		private Button button1;
		private Button button2;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private ListView listView1;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private GroupBox groupBox3;
		private ListView listViewData;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private Label labelStatus;
	}
}
