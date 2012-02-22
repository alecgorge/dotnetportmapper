namespace PortMap
{
	partial class AddMappingForm
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
			this.localPortTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.publicPortTextBox = new System.Windows.Forms.TextBox();
			this.tcpCheckBox = new System.Windows.Forms.CheckBox();
			this.udpCheckBox = new System.Windows.Forms.CheckBox();
			this.descriptionTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// localPortTextBox
			// 
			this.localPortTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.localPortTextBox.Location = new System.Drawing.Point(128, 12);
			this.localPortTextBox.Name = "localPortTextBox";
			this.localPortTextBox.Size = new System.Drawing.Size(100, 23);
			this.localPortTextBox.TabIndex = 0;
			this.localPortTextBox.Leave += new System.EventHandler(this.localPortTextBox_Leave);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Local Port:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(110, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Desired Public Port:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(12, 71);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(110, 15);
			this.label3.TabIndex = 3;
			this.label3.Text = "Protocol:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(12, 98);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(110, 15);
			this.label4.TabIndex = 4;
			this.label4.Text = "Description:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// publicPortTextBox
			// 
			this.publicPortTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.publicPortTextBox.Location = new System.Drawing.Point(128, 41);
			this.publicPortTextBox.Name = "publicPortTextBox";
			this.publicPortTextBox.Size = new System.Drawing.Size(100, 23);
			this.publicPortTextBox.TabIndex = 5;
			this.publicPortTextBox.Leave += new System.EventHandler(this.publicPortTextBox_Leave);
			// 
			// tcpCheckBox
			// 
			this.tcpCheckBox.AutoSize = true;
			this.tcpCheckBox.Checked = true;
			this.tcpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tcpCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tcpCheckBox.Location = new System.Drawing.Point(128, 70);
			this.tcpCheckBox.Name = "tcpCheckBox";
			this.tcpCheckBox.Size = new System.Drawing.Size(48, 19);
			this.tcpCheckBox.TabIndex = 6;
			this.tcpCheckBox.Text = "TCP";
			this.tcpCheckBox.UseVisualStyleBackColor = true;
			this.tcpCheckBox.Click += new System.EventHandler(this.tcpCheckBox_Click);
			// 
			// udpCheckBox
			// 
			this.udpCheckBox.AutoSize = true;
			this.udpCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.udpCheckBox.Location = new System.Drawing.Point(182, 70);
			this.udpCheckBox.Name = "udpCheckBox";
			this.udpCheckBox.Size = new System.Drawing.Size(49, 19);
			this.udpCheckBox.TabIndex = 7;
			this.udpCheckBox.Text = "UDP";
			this.udpCheckBox.UseVisualStyleBackColor = true;
			this.udpCheckBox.Click += new System.EventHandler(this.udpCheckBox_Click);
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.descriptionTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.descriptionTextBox.ForeColor = System.Drawing.Color.Gray;
			this.descriptionTextBox.Location = new System.Drawing.Point(128, 95);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = new System.Drawing.Size(206, 23);
			this.descriptionTextBox.TabIndex = 8;
			this.descriptionTextBox.Text = "Optional";
			this.descriptionTextBox.Leave += new System.EventHandler(this.descriptionTextBox_Leave);
			this.descriptionTextBox.Enter += new System.EventHandler(this.descriptionTextBox_Enter);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(234, 44);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(68, 15);
			this.label5.TabIndex = 9;
			this.label5.Text = "( 1 - 65535 )";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(259, 145);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(152, 145);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(101, 23);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "Add Mapping";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// AddMappingForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(346, 180);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.descriptionTextBox);
			this.Controls.Add(this.udpCheckBox);
			this.Controls.Add(this.tcpCheckBox);
			this.Controls.Add(this.publicPortTextBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.localPortTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AddMappingForm";
			this.Text = "AddMappingForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox localPortTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox publicPortTextBox;
		private System.Windows.Forms.CheckBox tcpCheckBox;
		private System.Windows.Forms.CheckBox udpCheckBox;
		private System.Windows.Forms.TextBox descriptionTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}