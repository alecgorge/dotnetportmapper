namespace PortMap
{
	partial class AllMappingsForm
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
			this.mappingsListView = new System.Windows.Forms.ListView();
			this.protocolColumn = new System.Windows.Forms.ColumnHeader();
			this.publicPortColumn = new System.Windows.Forms.ColumnHeader();
			this.localAddressColumn = new System.Windows.Forms.ColumnHeader();
			this.localPortColumn = new System.Windows.Forms.ColumnHeader();
			this.descriptionColumn = new System.Windows.Forms.ColumnHeader();
			this.label2 = new System.Windows.Forms.Label();
			this.localIPLabel = new System.Windows.Forms.Label();
			this.refreshButton = new System.Windows.Forms.Button();
			this.progressPictureBox = new System.Windows.Forms.PictureBox();
			this.removeButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.progressPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(259, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "The current UPnP mapping table of your router:";
			// 
			// mappingsListView
			// 
			this.mappingsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mappingsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.protocolColumn,
            this.publicPortColumn,
            this.localAddressColumn,
            this.localPortColumn,
            this.descriptionColumn});
			this.mappingsListView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mappingsListView.FullRowSelect = true;
			this.mappingsListView.Location = new System.Drawing.Point(12, 28);
			this.mappingsListView.Name = "mappingsListView";
			this.mappingsListView.Size = new System.Drawing.Size(490, 195);
			this.mappingsListView.TabIndex = 1;
			this.mappingsListView.UseCompatibleStateImageBehavior = false;
			this.mappingsListView.View = System.Windows.Forms.View.Details;
			this.mappingsListView.VirtualMode = true;
			this.mappingsListView.SelectedIndexChanged += new System.EventHandler(this.mappingsListView_SelectedIndexChanged);
			this.mappingsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.mappingsListView_RetrieveVirtualItem);
			// 
			// protocolColumn
			// 
			this.protocolColumn.Text = "Protocol";
			// 
			// publicPortColumn
			// 
			this.publicPortColumn.Text = "Public Port";
			this.publicPortColumn.Width = 75;
			// 
			// localAddressColumn
			// 
			this.localAddressColumn.Text = "Local IP Address";
			this.localAddressColumn.Width = 100;
			// 
			// localPortColumn
			// 
			this.localPortColumn.Text = "Local Port";
			this.localPortColumn.Width = 75;
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Text = "Description";
			this.descriptionColumn.Width = 150;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(48, 233);
			this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(121, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Your local IP Address:";
			// 
			// localIPLabel
			// 
			this.localIPLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.localIPLabel.AutoSize = true;
			this.localIPLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.localIPLabel.Location = new System.Drawing.Point(169, 233);
			this.localIPLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.localIPLabel.Name = "localIPLabel";
			this.localIPLabel.Size = new System.Drawing.Size(88, 15);
			this.localIPLabel.TabIndex = 3;
			this.localIPLabel.Text = "255.255.255.255";
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.refreshButton.Location = new System.Drawing.Point(427, 229);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(75, 23);
			this.refreshButton.TabIndex = 5;
			this.refreshButton.Text = "Refresh";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// progressPictureBox
			// 
			this.progressPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.progressPictureBox.Image = global::PortMap.Properties.Resources.progress;
			this.progressPictureBox.Location = new System.Drawing.Point(403, 231);
			this.progressPictureBox.Name = "progressPictureBox";
			this.progressPictureBox.Size = new System.Drawing.Size(18, 18);
			this.progressPictureBox.TabIndex = 6;
			this.progressPictureBox.TabStop = false;
			this.progressPictureBox.Visible = false;
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.removeButton.Enabled = false;
			this.removeButton.Image = global::PortMap.Properties.Resources.minus;
			this.removeButton.Location = new System.Drawing.Point(12, 229);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(27, 23);
			this.removeButton.TabIndex = 7;
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// AllMappingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(514, 264);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.progressPictureBox);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.localIPLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.mappingsListView);
			this.Controls.Add(this.label1);
			this.MinimumSize = new System.Drawing.Size(400, 150);
			this.Name = "AllMappingsForm";
			this.Text = "All UPnP Port Mappings";
			this.Load += new System.EventHandler(this.AllMappingsForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AllMappingsForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.progressPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView mappingsListView;
		private System.Windows.Forms.ColumnHeader protocolColumn;
		private System.Windows.Forms.ColumnHeader publicPortColumn;
		private System.Windows.Forms.ColumnHeader localAddressColumn;
		private System.Windows.Forms.ColumnHeader localPortColumn;
		private System.Windows.Forms.ColumnHeader descriptionColumn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label localIPLabel;
		private System.Windows.Forms.Button refreshButton;
		private System.Windows.Forms.PictureBox progressPictureBox;
		private System.Windows.Forms.Button removeButton;
	}
}