using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using TCMPortMapper;

namespace PortMap
{
	public partial class AllMappingsForm : Form
	{
		private List<ListViewItem> mappings;


		public AllMappingsForm()
		{
			InitializeComponent();
		}

		private void AllMappingsForm_Load(object sender, EventArgs e)
		{
			mappings = new List<ListViewItem>();

			localIPLabel.Text = PortMapper.SharedInstance.LocalIPAddress.ToString();

			PortMapper.SharedInstance.DidReceiveUPNPMappingTable += new PortMapper.PMDidReceiveUPNPMappingTable(PortMapper_DidReceiveUPNPMappingTable);
			DoRefresh();
		}

		private void AllMappingsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			PortMapper.SharedInstance.DidReceiveUPNPMappingTable -= new PortMapper.PMDidReceiveUPNPMappingTable(PortMapper_DidReceiveUPNPMappingTable);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Private API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void DoRefresh()
		{
			progressPictureBox.Visible = true;
			refreshButton.Enabled = false;
			PortMapper.SharedInstance.RequestUPnPMappingTable();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Interface Actions
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void removeButton_Click(object sender, EventArgs e)
		{
			if (mappingsListView.SelectedIndices.Count > 0)
			{
				int selectedIndex = mappingsListView.SelectedIndices[0];
				ListViewItem selectedItem = mappings[selectedIndex];
				ExistingUPnPPortMapping pm = (ExistingUPnPPortMapping)selectedItem.Tag;

				PortMapper.SharedInstance.RemovePortMapping(pm);

				// Note: No need to call DoRefresh() here.
				// The RemovePortMapping method will automatically refresh the
				// upnp mapping table and call our delegate.
			}
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			DoRefresh();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region PortMapper Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void PortMapper_DidReceiveUPNPMappingTable(PortMapper sender, List<ExistingUPnPPortMapping> existingMappings)
		{
			mappingsListView.BeginUpdate();
			{
				mappings.Clear();

				foreach (ExistingUPnPPortMapping pm in existingMappings)
				{
					String protocol;
					if(pm.TransportProtocol == PortMappingTransportProtocol.UDP)
						protocol = "UDP";
					else
						protocol = "TCP";

					ListViewItem lvi = new ListViewItem(protocol);
					lvi.SubItems.Add(pm.ExternalPort.ToString());
					lvi.SubItems.Add(pm.LocalAddress.ToString());
					lvi.SubItems.Add(pm.LocalPort.ToString());
					lvi.SubItems.Add(pm.Description);
					lvi.Tag = pm;

					mappings.Add(lvi);
				}

				mappingsListView.VirtualListSize = mappings.Count;
			}
			mappingsListView.EndUpdate();

			progressPictureBox.Visible = false;
			refreshButton.Enabled = true;
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

		private void mappingsListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			removeButton.Enabled = (mappingsListView.SelectedIndices.Count > 0);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}