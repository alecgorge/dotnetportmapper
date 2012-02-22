using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using TCMPortMapper;


namespace PortMap
{
	public partial class Form1 : Form
	{
		/// <summary>
		/// Stored font and text formatting rules for ListView.
		/// </summary>
		private Font mlvTxtFont;
		private StringFormat mlvTxtFormat;

		/// <summary>
		/// Used to determine which list items to invalidate when the selection changes.
		/// Areas to the right of the last column must be invalidated to allow for full row selection.
		/// This hack required thanks to Microsoft.
		/// </summary>
		private List<int> mlvPreviousSelectedIndexes;

		/// <summary>
		/// Stores the ListViewItems that correspond to the TCMPortMappings.
		/// </summary>
		private List<ListViewItem> mappings;


		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			mlvTxtFont = new Font(mappingsListView.Font.FontFamily, 8);

			mlvTxtFormat = new StringFormat(StringFormatFlags.NoWrap);
			mlvTxtFormat.Trimming = StringTrimming.EllipsisCharacter;
			mlvTxtFormat.LineAlignment = StringAlignment.Center;

			mlvPreviousSelectedIndexes = new List<int>();

			mappings = new List<ListViewItem>();

			externalIPAddressLabel.Text = "";
			internalIPAddressLabel.Text = "";

			PortMapper pm = PortMapper.SharedInstance;
			pm.DidStartWork += new PortMapper.PMDidStartWork(PortMapper_DidStartWork);
			pm.DidFinishWork += new PortMapper.PMDidFinishWork(PortMapper_DidFinishWork);
			pm.WillStartSearchForRouter += new PortMapper.PMWillStartSearchForRouter(PortMapper_WillStartSearchForRouter);
			pm.DidFinishSearchForRouter += new PortMapper.PMDidFinishSearchForRouter(PortMapper_DidFinishSearchForRouter);
			pm.DidChangeMappingStatus += new PortMapper.PMDidChangeMappingStatus(PortMapper_DidChangeMappingStatus);
			pm.ExternalIPAddressDidChange += new PortMapper.PMExternalIPAddressDidChange(PortMapper_ExternalIPAddressDidChange);

			pm.Start();
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
		//	DebugLog.WriteLine("Form1: Form1_Shown");
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			PortMapper pm = PortMapper.SharedInstance;
			pm.DidStartWork -= new PortMapper.PMDidStartWork(PortMapper_DidStartWork);
			pm.DidFinishWork -= new PortMapper.PMDidFinishWork(PortMapper_DidFinishWork);
			pm.WillStartSearchForRouter -= new PortMapper.PMWillStartSearchForRouter(PortMapper_WillStartSearchForRouter);
			pm.DidFinishSearchForRouter -= new PortMapper.PMDidFinishSearchForRouter(PortMapper_DidFinishSearchForRouter);
			pm.DidChangeMappingStatus -= new PortMapper.PMDidChangeMappingStatus(PortMapper_DidChangeMappingStatus);
			pm.ExternalIPAddressDidChange -= new PortMapper.PMExternalIPAddressDidChange(PortMapper_ExternalIPAddressDidChange);

			pm.StopBlocking();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Private API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void UpdateTagLine()
		{
			if (PortMapper.SharedInstance.IsRunning)
			{
				System.Net.IPAddress externalIP = PortMapper.SharedInstance.ExternalIPAddress;

				if (externalIP != null)
				{
					externalIPAddressLabel.Text = externalIP.ToString();

					String text = String.Format("{0} - {1} - {2}",
												PortMapper.SharedInstance.MappingProtocolName,
												PortMapper.SharedInstance.RouterManufacturer,
												PortMapper.SharedInstance.LocalIPAddress);

					internalIPAddressLabel.Text = text;
				}
				else
				{
					externalIPAddressLabel.Text = "No Port Mapping Protocol Found";
					internalIPAddressLabel.Text = "Router Information Unknown";
				}
			}
			else
			{
				internalIPAddressLabel.Text = "PortMapper Stopped";
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Interface Actions
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void AddPortMapping(PortMapping pm, String description)
		{
			mappingsListView.BeginUpdate();
			{
				ListViewItem lvi = new ListViewItem(description);
				lvi.SubItems.Add(pm.LocalPort.ToString());
				lvi.SubItems.Add(pm.DesiredExternalPort.ToString());
				lvi.SubItems.Add("");
				lvi.SubItems.Add(pm.ExternalPort.ToString());
				lvi.Tag = pm;

				mappings.Add(lvi);
				mappingsListView.VirtualListSize = mappings.Count;
			}
			mappingsListView.EndUpdate();

			PortMapper.SharedInstance.AddPortMapping(pm);
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			WindowManager.ShowAddMappingForm();
		}

		private void removeButton_Click(object sender, EventArgs e)
		{
			if (mappingsListView.SelectedIndices.Count > 0)
			{
				int selectedIndex = mappingsListView.SelectedIndices[0];
				ListViewItem selectedItem = mappings[selectedIndex];
				PortMapping pm = (PortMapping)selectedItem.Tag;

				mappingsListView.BeginUpdate();
				{
					mappings.Remove(selectedItem);
					mappingsListView.VirtualListSize = mappings.Count;
				}
				mappingsListView.EndUpdate();

				PortMapper.SharedInstance.RemovePortMapping(pm);
			}
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			PortMapper.SharedInstance.Refresh();
		}

		private void allUpnpButton_Click(object sender, EventArgs e)
		{
			WindowManager.ShowAllMappingsForm();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region PortMapper Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void PortMapper_WillStartSearchForRouter(PortMapper sender)
		{
			DebugLog.WriteLine("Form1: PortMapper_WillStartSearchForRouter");

			progressPictureBox.Visible = true;

			externalIPAddressLabel.Text = "Searching for Router...";
			internalIPAddressLabel.Text = "";
		}

		private void PortMapper_DidFinishSearchForRouter(PortMapper sender)
		{
			DebugLog.WriteLine("Form1: PortMapper_DidFinishSearchForRouter");

			progressPictureBox.Visible = false;

			if (PortMapper.SharedInstance.MappingProtocolName == "UPnP")
			{
				allUpnpButton.Enabled = true;
			}
			else
			{
				allUpnpButton.Enabled = false;
			}

			UpdateTagLine();
		}

		private void PortMapper_DidStartWork(PortMapper sender)
		{
			DebugLog.WriteLine("Form1: PortMapper_DidStartWork");
		}

		private void PortMapper_DidFinishWork(PortMapper sender)
		{
			DebugLog.WriteLine("Form1: PortMapper_DidFinishWork");
		}

		private void PortMapper_DidChangeMappingStatus(PortMapper sender, PortMapping pm)
		{
			DebugLog.WriteLine("Form1: PortMapper_DidChangeMappingStatus");

			foreach (ListViewItem lvi in mappings)
			{
				PortMapping lvi_pm = (PortMapping)lvi.Tag;
				if (lvi_pm == pm)
				{
					mappingsListView.RedrawItems(lvi.Index, lvi.Index, false);
				}
			}
		}

		private void PortMapper_ExternalIPAddressDidChange(PortMapper sender, System.Net.IPAddress ip)
		{
			DebugLog.WriteLine("Form1: PortMapper_ExternalIPAddressDidChange");

			UpdateTagLine();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region MappingsListView Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void mappingsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = mappingsListView_RetrieveVirtualItem(e.ItemIndex);
		}

		private ListViewItem mappingsListView_RetrieveVirtualItem(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= mappings.Count)
			{
				return null;
			}
			else
			{
				return mappings[rowIndex];
			}
		}

		private void mappingsListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void mappingsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			Rectangle fillRectangle = new Rectangle();
			fillRectangle.X = 0;
			fillRectangle.Y = e.Bounds.Y;
			fillRectangle.Width = mappingsListView.Bounds.Width;
			fillRectangle.Height = e.Bounds.Height;
			
			if (e.Item.Selected)
			{
				// Draw the background for a selected item.
				SolidBrush brush = new SolidBrush(SystemColors.Highlight);
				e.Graphics.FillRectangle(brush, fillRectangle);
			}
		}

		private void mappingsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			// Setup the drawing bounds for the item by adding appropriate padding
			Rectangle lvsiRect = new Rectangle();
			if (e.Header.TextAlign == HorizontalAlignment.Left)
			{
				lvsiRect.X = e.Bounds.X + 4;
				lvsiRect.Y = e.Bounds.Y;
				lvsiRect.Width = e.Bounds.Width - 12;
				lvsiRect.Height = e.Bounds.Height;
			}
			else if (e.Header.TextAlign == HorizontalAlignment.Right)
			{
				lvsiRect.X = e.Bounds.X + 8;
				lvsiRect.Y = e.Bounds.Y;
				lvsiRect.Width = e.Bounds.Width - 12;
				lvsiRect.Height = e.Bounds.Height;
			}
			else
			{
				lvsiRect.X = e.Bounds.X;
				lvsiRect.Y = e.Bounds.Y;
				lvsiRect.Width = e.Bounds.Width;
				lvsiRect.Height = e.Bounds.Height;
			}

			// Get the actual TCMPortMapping
			PortMapping pm = (PortMapping)e.Item.Tag;

			if (e.Header == statusColumn)
			{
				Image img;

				if (pm.MappingStatus == PortMappingStatus.Unmapped)
				{
					img = Properties.Resources.DotRed;
				}
				else if (pm.MappingStatus == PortMappingStatus.Trying)
				{
					img = Properties.Resources.DotYellow;
				}
				else
				{
					img = Properties.Resources.DotGreen;
				}

				Rectangle imgRect = new Rectangle();
				imgRect.X = e.Bounds.X + ((e.Bounds.Width  - img.Width)  / 2);
				imgRect.Y = e.Bounds.Y + ((e.Bounds.Height - img.Height) / 2);
				imgRect.Width  = img.Width;
				imgRect.Height = img.Height;
				
				e.Graphics.DrawImage(img, imgRect);
			}
			else
			{
				Color txtColor;
				if (e.Item.Selected)
					txtColor = SystemColors.HighlightText;
				else
					txtColor = e.Item.ForeColor;

				SolidBrush txtBrush = new SolidBrush(txtColor);

				if (e.Header.TextAlign == HorizontalAlignment.Left)
					mlvTxtFormat.Alignment = StringAlignment.Near;
				else
					mlvTxtFormat.Alignment = StringAlignment.Far;

				String text = e.SubItem.Text;
				if (e.Header == publicPortColumn)
				{
					text = pm.ExternalPort.ToString();
				}

				e.Graphics.DrawString(text, mlvTxtFont, txtBrush, lvsiRect, mlvTxtFormat);
			}
		}

		/// <summary>
		/// Even though we specifically draw/fill the areas we want, Windows decides to ignore
		/// drawing outside the node bounds.  Thus we have to specifically invalidate the
		/// regions to the right of the last column to allow us to have Mac style row selection.
		/// 
		/// Invalidates the bounds to the right of the given list item.
		/// </summary>
		private void mappingsListView_InvalidateOuterBounds(ListViewItem item)
		{
			if (mappingsListView.Bounds.Width > item.Bounds.Width)
			{
				Rectangle redrawMe = new Rectangle();
				redrawMe.X = item.Bounds.X + item.Bounds.Width;
				redrawMe.Y = item.Bounds.Y;
				redrawMe.Width = mappingsListView.Bounds.Width - item.Bounds.Width;
				redrawMe.Height = item.Bounds.Height;
				
				mappingsListView.Invalidate(redrawMe);
			}
		}

		private void mappingsListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			ColumnHeader column = mappingsListView.Columns[e.ColumnIndex];

			if (column == descriptionColumn)
			{
				if (e.NewWidth < 40 || e.NewWidth > 1000) e.Cancel = true;
			}
			else if (column == localPortColumn)
			{
				e.Cancel = true;
			}
			else if (column == desiredPortColumn)
			{
				e.Cancel = true;
			}
			else if (column == statusColumn)
			{
				e.Cancel = true;
			}
			else if (column == publicPortColumn)
			{
				e.Cancel = true;
			}
		}

		private void mappingsListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			ColumnHeader column = mappingsListView.Columns[e.ColumnIndex];

			if (column == descriptionColumn)
			{
				if(column.Width < 40)
					column.Width = 40;
				else if (column.Width > 1000)
					column.Width = 1000;
			}
			else if (column == localPortColumn)
			{
				if (column.Width != 80)
					column.Width = 80;
			}
			else if (column == desiredPortColumn)
			{
				if (column.Width != 80)
					column.Width = 80;
			}
			else if (column == statusColumn)
			{
				if (column.Width != 18)
					column.Width = 18;
			}
			else if (column == publicPortColumn)
			{
				if (column.Width != 80)
					column.Width = 80;
			}
		}

		private void mappingsListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection newSelectedIndexes = mappingsListView.SelectedIndices;
			
			for (int i = mlvPreviousSelectedIndexes.Count - 1; i >= 0; i--)
			{
				int index = mlvPreviousSelectedIndexes[i];
			
				if (index >= mappingsListView.VirtualListSize)
				{
					// Remove it now, because of stupid windows behavior
					// If we call newSelectedIndexes.Contains(index) it will actually crash
					mlvPreviousSelectedIndexes.RemoveAt(i);
				}
				else if (!newSelectedIndexes.Contains(index))
				{
					// Index is no longer selected
					mappingsListView_InvalidateOuterBounds(mappingsListView_RetrieveVirtualItem(index));
					mlvPreviousSelectedIndexes.RemoveAt(i);
				}
			}
			
			foreach (int index in newSelectedIndexes)
			{
				if (!mlvPreviousSelectedIndexes.Contains(index))
				{
					// Index is now selected
					mappingsListView_InvalidateOuterBounds(mappingsListView_RetrieveVirtualItem(index));
					mlvPreviousSelectedIndexes.Add(index);
				}
			}
			
			removeButton.Enabled = (mappingsListView.SelectedIndices.Count > 0);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}