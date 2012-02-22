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
	public partial class AddMappingForm : Form
	{
		public AddMappingForm()
		{
			InitializeComponent();
		}

		private bool CheckForm()
		{
			UInt16 port;

			if(!UInt16.TryParse(localPortTextBox.Text, out port)) return false;
			if(!UInt16.TryParse(publicPortTextBox.Text, out port)) return false;

			if(!tcpCheckBox.Checked && !udpCheckBox.Checked) return false;

			return true;
		}

		private void ClearForm()
		{
			localPortTextBox.Clear();
			publicPortTextBox.Clear();
			tcpCheckBox.Checked = true;
			udpCheckBox.Checked = false;
			descriptionTextBox.Clear();
		}

		private void localPortTextBox_Leave(object sender, EventArgs e)
		{
			UInt16 port;
			if (!UInt16.TryParse(localPortTextBox.Text, out port))
			{
				localPortTextBox.Text = "";
			}
			else
			{
				publicPortTextBox.Text = localPortTextBox.Text;
			}

			okButton.Enabled = CheckForm();
		}

		private void publicPortTextBox_Leave(object sender, EventArgs e)
		{
			UInt16 port;
			if (!UInt16.TryParse(publicPortTextBox.Text, out port))
			{
				publicPortTextBox.Text = "";
			}

			okButton.Enabled = CheckForm();
		}

		private void tcpCheckBox_Click(object sender, EventArgs e)
		{
			okButton.Enabled = CheckForm();
		}

		private void udpCheckBox_Click(object sender, EventArgs e)
		{
			okButton.Enabled = CheckForm();
		}

		private void descriptionTextBox_Enter(object sender, EventArgs e)
		{
			if (descriptionTextBox.ForeColor == Color.Gray)
			{
				descriptionTextBox.Clear();
				descriptionTextBox.ForeColor = Color.Black;
			}
		}

		private void descriptionTextBox_Leave(object sender, EventArgs e)
		{
			if (descriptionTextBox.Text.Trim().Length == 0)
			{
				descriptionTextBox.Text = "Optional";
				descriptionTextBox.ForeColor = Color.Gray;
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			UInt16 localPort;
			UInt16.TryParse(localPortTextBox.Text, out localPort);

			UInt16 publicPort;
			UInt16.TryParse(publicPortTextBox.Text, out publicPort);

			PortMappingTransportProtocol protocol;
			if (tcpCheckBox.Checked)
			{
				if (udpCheckBox.Checked)
					protocol = PortMappingTransportProtocol.Both;
				else
					protocol = PortMappingTransportProtocol.TCP;
			}
			else
			{
				protocol = PortMappingTransportProtocol.UDP;
			}

			PortMapping pm = new PortMapping(localPort, publicPort, protocol);

			String description;
			if (descriptionTextBox.ForeColor == Color.Gray)
				description = "";
			else
				description = descriptionTextBox.Text;

			WindowManager.GetMainForm().AddPortMapping(pm, description);

			ClearForm();
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			ClearForm();
			Close();
		}
	}
}