using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace TCMPortMapper
{
	class NATPMPPortMapper
	{
		public delegate void PMDidFail(NATPMPPortMapper sender);
		public delegate void PMDidGetExternalIPAddress(NATPMPPortMapper sender, IPAddress ip);
		public delegate void PMDidBeginWorking(NATPMPPortMapper sender);
		public delegate void PMDidEndWorking(NATPMPPortMapper sender);
		public delegate void PMDidReceiveBroadcastExternalIPChange(NATPMPPortMapper sender, IPAddress ip, IPAddress senderIP);

		public event PMDidFail DidFail;
		public event PMDidGetExternalIPAddress DidGetExternalIPAddress;
		public event PMDidBeginWorking DidBeginWorking;
		public event PMDidEndWorking DidEndWorking;
		public event PMDidReceiveBroadcastExternalIPChange DidReceiveBroadcastExternalIPChange;

		private Object multiThreadLock = new Object();
		private Object singleThreadLock = new Object();

		private volatile ThreadID threadID;
		private volatile ThreadFlags refreshExternalIPThreadFlags;
		private volatile ThreadFlags updatePortMappingsThreadFlags;

		private Timer updateTimer;
		private uint updateInterval;

		private UdpClient udpClient;

		private IPAddress lastBroadcastExternalIP;

		private enum ThreadID
		{
			None = 0,
			RefreshExternalIP = 1,
			UpdatePortMappings = 2
		}

		[Flags]
		private enum ThreadFlags
		{
			None = 0,
			ShouldQuit = 1,
			ShouldRestart = 2
		}

		// Standard routine:
		// 
		// Refresh -> triggers RefreshExternalIPThread -> Upon completion of thread, UpdatePortMappings is called.
		// Case 1: No threads are running -> perfect, trigger thread as planned
		// Case 2: RefreshExternalIPThread is running -> this thread is aborted, and then Refresh is called again
		// Case 3: UpdatePortMappingsThread is running -> this thread is aborted, and then Refresh is called again
		// 
		// UpdatePortMappings -> triggers UpdatePortMappingsThread -> Upon completion of thread, AdjustUpdateTimer is called
		// Case 1: No threads are running -> perfect, trigger thread as planned
		// Case 2: RefreshExternalIPInThread is running -> That's fine, we do nothing
		// Case 3: UpdatePortMappingsInThread is running -> this thread is aborted, and restarted from beginning

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Public API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public NATPMPPortMapper()
		{
			// Until I find a way around this bug, there's no reason to setup the udp client...

			// Add UDP listener for public ip update packets
			// udpClient = new UdpClient(5351);

			// Note: The following code throws an exception for some reason.
			// The JoinMulticastGroup works fine for every multicast address except 224.0.0.1
			// Another reason why windows sucks.
			// udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.0.1"));

			// So basically, the udpClient won't be receiving anything.
			// I consider this to be a bug in Windows and/or .Net.
			
			// udpClient.BeginReceive(new AsyncCallback(udpClient_DidReceive), null);
		}

		public void Refresh()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (Monitor.TryEnter(multiThreadLock))
				{
					updateInterval = 3600 / 2;
					refreshExternalIPThreadFlags = ThreadFlags.None;
					updatePortMappingsThreadFlags = ThreadFlags.None;

					threadID = ThreadID.RefreshExternalIP;

					Thread bgThread = new Thread(new ThreadStart(RefreshExternalIPThread));
					bgThread.Start();

					Monitor.Exit(multiThreadLock);
				}
				else
				{
					if (threadID == ThreadID.RefreshExternalIP)
					{
						refreshExternalIPThreadFlags = ThreadFlags.ShouldQuit | ThreadFlags.ShouldRestart;
					}
					else if (threadID == ThreadID.UpdatePortMappings)
					{
						updatePortMappingsThreadFlags = ThreadFlags.ShouldQuit;
					}
				}
			}
		}

		public void UpdatePortMappings()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (Monitor.TryEnter(multiThreadLock))
				{
					updatePortMappingsThreadFlags = ThreadFlags.None;

					threadID = ThreadID.UpdatePortMappings;

					Thread bgThread = new Thread(new ThreadStart(UpdatePortMappingsThread));
					bgThread.Start();

					Monitor.Exit(multiThreadLock);
				}
				else
				{
					if (threadID == ThreadID.UpdatePortMappings)
					{
						updatePortMappingsThreadFlags = ThreadFlags.ShouldQuit | ThreadFlags.ShouldRestart;
					}
				}
			}
		}

		public void Stop()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (updateTimer != null)
				{
					updateTimer.Dispose();
					updateTimer = null;
				}
				if (Monitor.TryEnter(multiThreadLock))
				{
					Monitor.Exit(multiThreadLock);

					// Restart update to remove mappings before stopping
					UpdatePortMappings();
				}
				else if (threadID == ThreadID.RefreshExternalIP)
				{
					// Stop the RefreshExternalIPThread
					refreshExternalIPThreadFlags = ThreadFlags.ShouldQuit;
				}
			}
		}

		public void StopBlocking()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				refreshExternalIPThreadFlags = ThreadFlags.ShouldQuit;
				updatePortMappingsThreadFlags = ThreadFlags.ShouldQuit;

				Monitor.Enter(multiThreadLock);

				NATPMP.natpmp_t natpmp = new NATPMP.natpmp_t();
				NATPMP.initnatpmp(ref natpmp);

				List<PortMapping> mappingsToRemove = PortMapper.SharedInstance.PortMappingsToRemove;
				lock (mappingsToRemove)
				{
					while (mappingsToRemove.Count > 0)
					{
						PortMapping pm = mappingsToRemove[0];

						if (pm.MappingStatus == PortMappingStatus.Mapped)
						{
							RemovePortMapping(pm, ref natpmp);
						}

						mappingsToRemove.RemoveAt(0);
					}
				}

				List<PortMapping> mappingsToStop = PortMapper.SharedInstance.PortMappings;
				lock (mappingsToStop)
				{
					for (int i = 0; i < mappingsToStop.Count; i++)
					{
						PortMapping pm = mappingsToStop[i];

						if (pm.MappingStatus == PortMappingStatus.Mapped)
						{
							RemovePortMapping(pm, ref natpmp);
						}
					}
				}

				Monitor.Exit(multiThreadLock);
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		protected virtual void OnDidFail()
		{
			if (DidFail != null)
			{
				PortMapper.SharedInstance.Invoke(DidFail, this);
			}
		}

		protected virtual void OnDidGetExternalIPAddress(IPAddress ip)
		{
			if (DidGetExternalIPAddress != null)
			{
				PortMapper.SharedInstance.Invoke(DidGetExternalIPAddress, this, ip);
			}
		}

		protected virtual void OnDidBeginWorking()
		{
			if (DidBeginWorking != null)
			{
				// This is thread safe, so there's no need to Invoke it
				DidBeginWorking(this);
			}
		}

		protected virtual void OnDidEndWorking()
		{
			if (DidEndWorking != null)
			{
				// This is thread safe, so there's no need to Invoke it
				DidEndWorking(this);
			}
		}

		protected virtual void OnDidReceiveBroadcastExternalIPChange(IPAddress externalIP, IPAddress senderIP)
		{
			if (lastBroadcastExternalIP == null)
			{
				lastBroadcastExternalIP = externalIP;
			}
			else
			{
				if (lastBroadcastExternalIP == externalIP)
				{
					// To accommodate packet loss, the NAT-PMP protocol may broadcast
					// an external IP address change up to 10 times.
					// We only need to broadcast it once.
					return;
				}
			}

			if (DidReceiveBroadcastExternalIPChange != null)
			{
				PortMapper.SharedInstance.Invoke(DidReceiveBroadcastExternalIPChange, this, externalIP, senderIP);
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Private API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private bool AddPortMapping(PortMapping portMapping, ref NATPMP.natpmp_t natpmp)
		{
			return ApplyPortMapping(portMapping, false, ref natpmp);
		}

		private bool RefreshPortMapping(PortMapping portMapping, ref NATPMP.natpmp_t natpmp)
		{
			return ApplyPortMapping(portMapping, false, ref natpmp);
		}

		private bool RemovePortMapping(PortMapping portMapping, ref NATPMP.natpmp_t natpmp)
		{
			return ApplyPortMapping(portMapping, true, ref natpmp);
		}

		private bool ApplyPortMapping(PortMapping portMapping, bool remove, ref NATPMP.natpmp_t natpmp)
		{
			NATPMP.natpmpresp_t response = new NATPMP.natpmpresp_t();
			int r;
			Win32.TimeValue timeout = new Win32.TimeValue();
			Win32.FileDescriptorSet fds = new Win32.FileDescriptorSet(1);

			if (!remove)
			{
				portMapping.SetMappingStatus(PortMappingStatus.Trying);
			}
			PortMappingTransportProtocol protocol = portMapping.TransportProtocol;

			for (int i = 1; i <= 2; i++)
			{
				PortMappingTransportProtocol currentProtocol;
				if(i == 1)
					currentProtocol = PortMappingTransportProtocol.UDP;
				else
					currentProtocol = PortMappingTransportProtocol.TCP;

				if (protocol == currentProtocol || protocol == PortMappingTransportProtocol.Both)
				{
					r = NATPMP.sendnewportmappingrequest(ref natpmp,
						(i == 1) ? NATPMP.PROTOCOL_UDP : NATPMP.PROTOCOL_TCP,
						portMapping.LocalPort, portMapping.DesiredExternalPort, (uint)(remove ? 0 : 3600));

					do
					{
						fds.Count = 1;
						fds.Array[0] = (IntPtr)natpmp.s;
						NATPMP.getnatpmprequesttimeout(ref natpmp, ref timeout);

						Win32.select(0, ref fds, IntPtr.Zero, IntPtr.Zero, ref timeout);

						r = NATPMP.readnatpmpresponseorretry(ref natpmp, ref response);
					}
					while(r == NATPMP.ERR_TRYAGAIN);

					if (r < 0)
					{
						portMapping.SetMappingStatus(PortMappingStatus.Unmapped);
						return false;
					}
				}
			}

			if (remove)
			{
				portMapping.SetMappingStatus(PortMappingStatus.Unmapped);
			}
			else
			{
				updateInterval = Math.Min(updateInterval, response.pnu_newportmapping.lifetime / 2);
				if (updateInterval < 60)
				{
					DebugLog.WriteLine("NAT-PMP: ApplyPortMapping: Caution - new port mapping had a lifetime < 120 ({0})",
						response.pnu_newportmapping.lifetime);

					updateInterval = 60;
				}
				portMapping.SetExternalPort(response.pnu_newportmapping.mappedpublicport);
				portMapping.SetMappingStatus(PortMappingStatus.Mapped);
			}

			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Refresh External IP Thread
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void RefreshExternalIPThread()
		{
			Monitor.Enter(multiThreadLock);
			OnDidBeginWorking();

			NATPMP.natpmp_t natpmp = new NATPMP.natpmp_t();
			NATPMP.natpmpresp_t response = new NATPMP.natpmpresp_t();
			int r;
			Win32.TimeValue timeout = new Win32.TimeValue();
			Win32.FileDescriptorSet fds = new Win32.FileDescriptorSet(1);
			bool didFail = false;

			r = NATPMP.initnatpmp(ref natpmp);
			if (r < 0)
			{
				didFail = true;
			}
			else
			{
				r = NATPMP.sendpublicaddressrequest(ref natpmp);
				if (r < 0)
				{
					didFail = true;
				}
				else
				{
					do
					{
						fds.Count = 1;
						fds.Array[0] = (IntPtr)natpmp.s;
						NATPMP.getnatpmprequesttimeout(ref natpmp, ref timeout);

						Win32.select(0, ref fds, IntPtr.Zero, IntPtr.Zero, ref timeout);

						r = NATPMP.readnatpmpresponseorretry(ref natpmp, ref response);
						if (refreshExternalIPThreadFlags != ThreadFlags.None)
						{
							DebugLog.WriteLine("NAT-PMP: RefreshExternalIPThread quit prematurely (1)");

							Monitor.Exit(multiThreadLock);
							if ((refreshExternalIPThreadFlags & ThreadFlags.ShouldRestart) > 0)
							{
								Refresh();
							}
							NATPMP.closenatpmp(ref natpmp);
							OnDidEndWorking();
							return;
						}
					}
					while (r == NATPMP.ERR_TRYAGAIN);

					if (r < 0)
					{
						didFail = true;
						DebugLog.WriteLine("NAT-PMP: IP refresh did time out");
					}
					else
					{
						IPAddress ipaddr = new IPAddress((long)response.pnu_publicaddress.addr);
						OnDidGetExternalIPAddress(ipaddr);
					}
				}
			}

			NATPMP.closenatpmp(ref natpmp);
			Monitor.Exit(multiThreadLock);

			if (refreshExternalIPThreadFlags != ThreadFlags.None)
			{
				DebugLog.WriteLine("NAT-PMP: RefreshExternalIPThread quit prematurely (2)");

				if ((refreshExternalIPThreadFlags & ThreadFlags.ShouldRestart) > 0)
				{
					Refresh();
				}
			}
			else
			{
				if (didFail)
				{
					OnDidFail();
				}
				else
				{
					UpdatePortMappings();
				}
			}
			OnDidEndWorking();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Update Port Mappings Thread
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void UpdatePortMappingsThread()
		{
			Monitor.Enter(multiThreadLock);
			OnDidBeginWorking();

			NATPMP.natpmp_t natpmp = new NATPMP.natpmp_t();
			NATPMP.initnatpmp(ref natpmp);

			// Remove mappings scheduled for removal

			List<PortMapping> mappingsToRemove = PortMapper.SharedInstance.PortMappingsToRemove;
			lock (mappingsToRemove)
			{
				while ((mappingsToRemove.Count > 0) && (updatePortMappingsThreadFlags == ThreadFlags.None))
				{
					PortMapping mappingToRemove = mappingsToRemove[0];

					if (mappingToRemove.MappingStatus == PortMappingStatus.Mapped)
					{
						RemovePortMapping(mappingToRemove, ref natpmp);
					}

					mappingsToRemove.RemoveAt(0);
				}
			}

			// If the port mapper is running:
			//   -Refresh existing mappings
			//   -Add new mappings
			// If the port mapper is stopped:
			//   -Remove any existing mappings

			List<PortMapping> mappings = PortMapper.SharedInstance.PortMappings;
			lock (mappings)
			{
				for (int i = 0; i < mappings.Count && updatePortMappingsThreadFlags == ThreadFlags.None; i++)
				{
					PortMapping existingMapping = mappings[i];
					bool isRunning = PortMapper.SharedInstance.IsRunning;

					if (existingMapping.MappingStatus == PortMappingStatus.Mapped)
					{
						if (isRunning)
						{
							RefreshPortMapping(existingMapping, ref natpmp);
						}
						else
						{
							RemovePortMapping(existingMapping, ref natpmp);
						}
					}
				}

				for (int i = 0; i < mappings.Count && updatePortMappingsThreadFlags == ThreadFlags.None; i++)
				{
					PortMapping mappingToAdd = mappings[i];
					bool isRunning = PortMapper.SharedInstance.IsRunning;

					if (mappingToAdd.MappingStatus == PortMappingStatus.Unmapped && isRunning)
					{
						AddPortMapping(mappingToAdd, ref natpmp);
					}
				}
			}

			NATPMP.closenatpmp(ref natpmp);
			Monitor.Exit(multiThreadLock);

			if (PortMapper.SharedInstance.IsRunning)
			{
				if ((updatePortMappingsThreadFlags & ThreadFlags.ShouldRestart) > 0)
				{
					UpdatePortMappings();
				}
				else if ((updatePortMappingsThreadFlags & ThreadFlags.ShouldQuit) > 0)
				{
					Refresh();
				}
				else
				{
					AdjustUpdateTimer();
				}
			}

			OnDidEndWorking();
		}

		private void AdjustUpdateTimer()
		{
			// This method is also locked with the single thread lock
			// This is because it alters variables altered within the public API
			lock (singleThreadLock)
			{
				if (updateTimer != null)
				{
					updateTimer.Dispose();
					updateTimer = null;
				}
				updateTimer = new Timer(new TimerCallback(UpdatePortMappings), null, (updateInterval * 1000), Timeout.Infinite);
			}
		}

		private void UpdatePortMappings(object state)
		{
			// Called via timer (on background thread)
			UpdatePortMappings();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void udpClient_DidReceive(IAsyncResult ar)
		{
			// When the public address changes, the NAT gateway will send a notification on the
			// multicast group 224.0.0.1 port 5351 with the format of a public address response.
			// 
			// Public address response:
			// 
			//  0                   1                   2                   3
			//  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
			// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			// | Vers = 0      | OP = 128 + 0  | Result Code                   |
			// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			// | Seconds Since Start of Epoch                                  |
			// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			// | Public IPv4 Address (a.b.c.d)                                 |
			// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

			DebugLog.WriteLine("NAT-PMP: udpClient_DidReceive");

			try
			{
				IPEndPoint ep = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 5351);
				byte[] data = udpClient.EndReceive(ar, ref ep);

				if (data.Length == 12)
				{
					byte[] rawIP = new byte[4];
					Buffer.BlockCopy(data, 8, rawIP, 0, 4);

					IPAddress newIP = new IPAddress(rawIP);

					OnDidReceiveBroadcastExternalIPChange(newIP, ep.Address);
				}
			}
			catch (Exception e)
			{
				DebugLog.WriteLine("NAT-PMP: udpClient_DidReceive: Exception: {0}", e);
			}

			udpClient.BeginReceive(new AsyncCallback(udpClient_DidReceive), null);
		}
	}
}
