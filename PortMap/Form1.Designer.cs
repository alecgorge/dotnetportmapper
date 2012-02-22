namespace PortMap
{
	partial class Form1
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
			this.addButton = new System.Windows.Forms.Button();
			this.removeButton = new System.Windows.Forms.Button();
			this.externalIPAddressLabel = new System.Windows.Forms.Label();
			this.internalIPAddressLabel = new System.Windows.Forms.Label();
			this.allUpnpButton = new System.Windows.Forms.Button();
			this.mappingsListView = new System.Windows.Forms.ListView();
			this.descriptionColumn = new System.Windows.Forms.ColumnHeader();
			this.localPortColumn = new System.Windows.Forms.ColumnHeader();
			this.desiredPortColumn = new System.Windows.Forms.ColumnHeader();
			this.statusColumn = new System.Windows.Forms.ColumnHeader();
			this.publicPortColumn = new System.Windows.Forms.ColumnHeader();
			this.progressPictureBox = new System.Windows.Forms.PictureBox();
			this.refreshButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.progressPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.addButton.Image = global::PortMap.Properties.Resources.plus;
			this.addButton.Location = new System.Drawing.Point(12, 275);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(27, 23);
			this.addButton.TabIndex = 0;
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.removeButton.Enabled = false;
			this.removeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.removeButton.Image = global::PortMap.Properties.Resources.minus;
			this.removeButton.Location = new System.Drawing.Point(45, 275);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(27, 23);
			this.removeButton.TabIndex = 1;
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// externalIPAddressLabel
			// 
			this.externalIPAddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.externalIPAddressLabel.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.externalIPAddressLabel.Location = new System.Drawing.Point(12, 9);
			this.externalIPAddressLabel.Name = "externalIPAddressLabel";
			this.externalIPAddressLabel.Size = new System.Drawing.Size(436, 37);
			this.externalIPAddressLabel.TabIndex = 2;
			this.externalIPAddressLabel.Text = "[External IP]";
			this.externalIPAddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// internalIPAddressLabel
			// 
			this.internalIPAddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.internalIPAddressLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.internalIPAddressLabel.Location = new System.Drawing.Point(12, 46);
			this.internalIPAddressLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.internalIPAddressLabel.Name = "internalIPAddressLabel";
			this.internalIPAddressLabel.Size = new System.Drawing.Size(436, 20);
			this.internalIPAddressLabel.TabIndex = 3;
			this.internalIPAddressLabel.Text = "[Protocol] - [Router Name] - [Internal IP]";
			this.internalIPAddressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// allUpnpButton
			// 
			this.allUpnpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.allUpnpButton.Enabled = false;
			this.allUpnpButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.allUpnpButton.Location = new System.Drawing.Point(316, 275);
			this.allUpnpButton.Name = "allUpnpButton";
			this.allUpnpButton.Size = new System.Drawing.Size(132, 23);
			this.allUpnpButton.TabIndex = 4;
			this.allUpnpButton.Text = "All UPnP Mappings";
			this.allUpnpButton.UseVisualStyleBackColor = true;
			this.allUpnpButton.Click += new System.EventHandler(this.allUpnpButton_Click);
			// 
			// mappingsListView
			// 
			this.mappingsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mappingsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.descriptionColumn,
            this.localPortColumn,
            this.desiredPortColumn,
            this.statusColumn,
            this.publicPortColumn});
			this.mappingsListView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mappingsListView.FullRowSelect = true;
			this.mappingsListView.Location = new System.Drawing.Point(12, 74);
			this.mappingsListView.Name = "mappingsListView";
			this.mappingsListView.OwnerDraw = true;
			this.mappingsListView.Size = new System.Drawing.Size(436, 195);
			this.mappingsListView.TabIndex = 5;
			this.mappingsListView.UseCompatibleStateImageBehavior = false;
			this.mappingsListView.View = System.Windows.Forms.View.Details;
			this.mappingsListView.VirtualMode = true;
			this.mappingsListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.mappingsListView_DrawColumnHeader);
			this.mappingsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.mappingsListView_DrawItem);
			this.mappingsListView.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.mappingsListView_ColumnWidthChanged);
			this.mappingsListView.SelectedIndexChanged += new System.EventHandler(this.mappingsListView_SelectedIndexChanged);
			this.mappingsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.mappingsListView_RetrieveVirtualItem);
			this.mappingsListView.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.mappingsListView_ColumnWidthChanging);
			this.mappingsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.mappingsListView_DrawSubItem);
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Text = "Description";
			this.descriptionColumn.Width = 145;
			// 
			// localPortColumn
			// 
			this.localPortColumn.Text = "Local Port";
			this.localPortColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.localPortColumn.Width = 80;
			// 
			// desiredPortColumn
			// 
			this.desiredPortColumn.Text = "Desired Port";
			this.desiredPortColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.desiredPortColumn.Width = 80;
			// 
			// statusColumn
			// 
			this.statusColumn.Text = "";
			this.statusColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.statusColumn.Width = 18;
			// 
			// publicPortColumn
			// 
			this.publicPortColumn.Text = "Public Port";
			this.publicPortColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.publicPortColumn.Width = 80;
			// 
			// progressPictureBox
			// 
			this.progressPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.progressPictureBox.Image = global::PortMap.Properties.Resources.progress;
			this.progressPictureBox.Location = new System.Drawing.Point(213, 279);
			this.progressPictureBox.Name = "progressPictureBox";
			this.progressPictureBox.Size = new System.Drawing.Size(16, 16);
			this.progressPictureBox.TabIndex = 6;
			this.progressPictureBox.TabStop = false;
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.refreshButton.Location = new System.Drawing.Point(235, 275);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(75, 23);
			this.refreshButton.TabIndex = 7;
			this.refreshButton.Text = "Refresh";
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(460, 310);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.progressPictureBox);
			this.Controls.Add(this.mappingsListView);
			this.Controls.Add(this.allUpnpButton);
			this.Controls.Add(this.internalIPAddressLabel);
			this.Controls.Add(this.externalIPAddressLabel);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.addButton);
			this.MinimumSize = new System.Drawing.Size(350, 250);
			this.Name = "Form1";
			this.Text = "Port Map";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.progressPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.Label externalIPAddressLabel;
		private System.Windows.Forms.Label internalIPAddressLabel;
		private System.Windows.Forms.Button allUpnpButton;
		private System.Windows.Forms.ListView mappingsListView;
		private System.Windows.Forms.ColumnHeader descriptionColumn;
		private System.Windows.Forms.ColumnHeader localPortColumn;
		private System.Windows.Forms.ColumnHeader desiredPortColumn;
		private System.Windows.Forms.ColumnHeader statusColumn;
		private System.Windows.Forms.ColumnHeader publicPortColumn;
		private System.Windows.Forms.PictureBox progressPictureBox;
		private System.Windows.Forms.Button refreshButton;
	}
}

