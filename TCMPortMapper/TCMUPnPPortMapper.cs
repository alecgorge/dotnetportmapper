using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;


namespace TCMPortMapper
{
	public class ExistingUPnPPortMapping
	{
		private IPAddress localAddress;
		private UInt16 localPort;
		private UInt16 externalPort;
		private PortMappingTransportProtocol transportProtocol;
		private String description;

		public ExistingUPnPPortMapping(IPAddress localAddress, UInt16 localPort, UInt16 externalPort,
		                               PortMappingTransportProtocol transportProtocol, String description)
		{
			this.localAddress = localAddress;
			this.localPort = localPort;
			this.externalPort = externalPort;
			this.transportProtocol = transportProtocol;
			this.description = description;
		}

		public IPAddress LocalAddress
		{
			get { return localAddress; }
		}

		public UInt16 LocalPort
		{
			get { return localPort; }
		}

		public UInt16 ExternalPort
		{
			get { return externalPort; }
		}

		public PortMappingTransportProtocol TransportProtocol
		{
			get { return transportProtocol; }
		}

		public String Description
		{
			get { return description; }
		}
	}

	class UPnPPortMapper
	{
		public delegate void PMDidFail(UPnPPortMapper sender);
		public delegate void PMDidGetExternalIPAddress(UPnPPortMapper sender, IPAddress ip);
		public delegate void PMDidBeginWorking(UPnPPortMapper sender);
		public delegate void PMDidEndWorking(UPnPPortMapper sender);

		public event PMDidFail DidFail;
		public event PMDidGetExternalIPAddress DidGetExternalIPAddress;
		public event PMDidBeginWorking DidBeginWorking;
		public event PMDidEndWorking DidEndWorking;

		private Object multiThreadLock = new Object();
		private Object singleThreadLock = new Object();

		private volatile ThreadID threadID;
		private volatile ThreadFlags refreshExternalIPThreadFlags;
		private volatile ThreadFlags updatePortMappingsThreadFlags;

		private List<ExistingUPnPPortMapping> existingUPnPPortMappings;
		private Dictionary<UInt16, ExistingUPnPPortMapping> existingUPnPPortMappingsUdpDict;
		private Dictionary<UInt16, ExistingUPnPPortMapping> existingUPnPPortMappingsTcpDict;

		private MiniUPnP.UPNPUrls urls = new MiniUPnP.UPNPUrls();
		private MiniUPnP.IGDdatas igddata = new MiniUPnP.IGDdatas();

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

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Public API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public UPnPPortMapper()
		{
			// Nothing to do here
		}

		public void Refresh()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (Monitor.TryEnter(multiThreadLock))
				{
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

				DoUpdateExistingUPnPPortMappings();

				List<PortMapping> mappingsToRemove = PortMapper.SharedInstance.PortMappingsToRemove;
				lock (mappingsToRemove)
				{
					while (mappingsToRemove.Count > 0)
					{
						PortMapping pm = mappingsToRemove[0];

						if (pm.MappingStatus == PortMappingStatus.Mapped)
						{
							RemovePortMapping(pm);
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
							RemovePortMapping(pm);
						}
					}
				}

				Monitor.Exit(multiThreadLock);
			}
		}

		public void UpdateExistingUPnPPortMappings()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				Thread bgThread = new Thread(new ThreadStart(UpdateExistingUPnPMappingsThread));
				bgThread.Start();
			}
		}

		public List<ExistingUPnPPortMapping> ExistingUPnPPortMappings
		{
			get { return existingUPnPPortMappings; }
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

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Private API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private String GetPortMappingDescription()
		{
			String machineName = Environment.MachineName;
			String userName = Environment.UserName;

			System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

			String processName = currentProcess.ProcessName;
			int processId = currentProcess.Id;


			return machineName + "/" + userName + "/" + processName + "/" + processId;
		}

		private void DoUpdateExistingUPnPPortMappings()
		{
			List<ExistingUPnPPortMapping> existingMappings = new List<ExistingUPnPPortMapping>();

			Dictionary<UInt16, ExistingUPnPPortMapping> existingMappingsUdpDict =
				new Dictionary<UInt16, ExistingUPnPPortMapping>();
			Dictionary<UInt16, ExistingUPnPPortMapping> existingMappingsTcpDict =
				new Dictionary<UInt16, ExistingUPnPPortMapping>();

			int r = 0;
			int i = 0;
			byte[] index = new byte[6];
			byte[] intClient = new byte[16];
			byte[] intPort = new byte[6];
			byte[] extPort = new byte[6];
			byte[] protocol = new byte[4];
			byte[] desc = new byte[80];
			byte[] enabled = new byte[6];
			byte[] rHost = new byte[64];
			byte[] duration = new byte[16];

			do
			{
				// Convert "int i" to a null-terminated char array
				String iStr = i.ToString();
				int maxCount = Math.Min(iStr.Length, 5);
				System.Text.Encoding.ASCII.GetBytes(iStr, 0, maxCount, index, 0);

				// Reset all the other null-terminated char arrays
				intClient[0] = 0;
				intPort[0] = 0;
				extPort[0] = 0;
				protocol[0] = 0;  // Warning - not in Cocoa version
				desc[0] = 0;
				enabled[0] = 0;
				rHost[0] = 0;
				duration[0] = 0;

				r = MiniUPnP.UPNPCOMMAND_UNKNOWN_ERROR;
				try
				{
					r = MiniUPnP.UPNP_GetGenericPortMappingEntry(urls.controlURL, igddata.ServiceType,
																 index,
																 extPort, intClient, intPort,
																 protocol, desc, enabled,
																 rHost, duration);
				}
				catch (AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that all the data gets marshaled over and back properly.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_GetGenericPortMappingEntry");
					
					r = MiniUPnP.UPNPCOMMAND_SUCCESS;
				}

				if (r == MiniUPnP.UPNPCOMMAND_SUCCESS)
				{
					IPAddress iAddr;
					IPAddress.TryParse(MiniUPnP.NullTerminatedArrayToString(intClient), out iAddr);
					
					UInt16 iPort;
					UInt16.TryParse(MiniUPnP.NullTerminatedArrayToString(intPort), out iPort);

					UInt16 ePort;
					UInt16.TryParse(MiniUPnP.NullTerminatedArrayToString(extPort), out ePort);
					
					PortMappingTransportProtocol transportProtocol = 0;
					String protocolStr = MiniUPnP.NullTerminatedArrayToString(protocol);
					if (protocolStr.Equals("UDP", StringComparison.OrdinalIgnoreCase))
					{
						transportProtocol |= PortMappingTransportProtocol.UDP;
					}
					if (protocolStr.Equals("TCP", StringComparison.OrdinalIgnoreCase))
					{
						transportProtocol |= PortMappingTransportProtocol.TCP;
					}

					String description = MiniUPnP.NullTerminatedArrayToString(desc);

					ExistingUPnPPortMapping existingPM;
					existingPM = new ExistingUPnPPortMapping(iAddr, iPort, ePort, transportProtocol, description);

					existingMappings.Add(existingPM);

					if ((transportProtocol & PortMappingTransportProtocol.UDP) > 0)
					{
						existingMappingsUdpDict[ePort] = existingPM;
					}
					if ((transportProtocol & PortMappingTransportProtocol.TCP) > 0)
					{
						existingMappingsTcpDict[ePort] = existingPM;
					}

					DebugLog.WriteLine("Existing UPnP: {0}: {1} {2}->{3}:{4} ({5})",
						                                i, protocolStr, ePort, iAddr, iPort, description);
				}

				i++;
			} while ((r == MiniUPnP.UPNPCOMMAND_SUCCESS) && (updatePortMappingsThreadFlags == ThreadFlags.None));

			// Update stored list of existing mappings
			existingUPnPPortMappings = existingMappings;
			existingUPnPPortMappingsUdpDict = existingMappingsUdpDict;
			existingUPnPPortMappingsTcpDict = existingMappingsTcpDict;
		}

		private bool AddPortMapping(PortMapping portMapping)
		{
			portMapping.SetMappingStatus(PortMappingStatus.Trying);

			String intPortStr = portMapping.LocalPort.ToString();
			String intClient = PortMapper.SharedInstance.LocalIPAddress.ToString();
			String description = GetPortMappingDescription();

			bool done = false;
			int attemptCount = 0;
			do
			{
				int udpErrCode = 0;
				int tcpErrCode = 0;

				bool udpResult = true;
				bool tcpResult = true;

				UInt16 extPort;
				if(portMapping.DesiredExternalPort < (65535 - 40))
					extPort = (UInt16)(portMapping.DesiredExternalPort + attemptCount);
				else
					extPort = (UInt16)(portMapping.DesiredExternalPort - attemptCount);

				String extPortStr = extPort.ToString();

				if ((portMapping.TransportProtocol & PortMappingTransportProtocol.UDP) > 0)
				{
					ExistingUPnPPortMapping existingPM;
					if (existingUPnPPortMappingsUdpDict.TryGetValue(extPort, out existingPM))
					{
						udpErrCode = 718;
						DebugLog.WriteLine("UPnP: AddPortMapping: UDP: mapping already exists");
					}
					else
					{
						udpErrCode = MiniUPnP.UPNP_AddPortMapping(urls.controlURL, igddata.ServiceType,
																  extPortStr, intPortStr, intClient, description, "UDP");
						DebugLog.WriteLine("UPnP: AddPortMapping: UDP: result = {0}", udpErrCode);
					}

					udpResult = (udpErrCode == MiniUPnP.UPNPCOMMAND_SUCCESS);
				}
				if ((portMapping.TransportProtocol & PortMappingTransportProtocol.TCP) > 0)
				{
					ExistingUPnPPortMapping existingPM;
					if (existingUPnPPortMappingsTcpDict.TryGetValue(extPort, out existingPM))
					{
						tcpErrCode = 718;
						DebugLog.WriteLine("UPnP: AddPortMapping: TCP: mapping already exists");
					}
					else
					{
						tcpErrCode = MiniUPnP.UPNP_AddPortMapping(urls.controlURL, igddata.ServiceType,
																  extPortStr, intPortStr, intClient, description, "TCP");
						DebugLog.WriteLine("UPnP: AddPortMapping: TCP: result = {0}", tcpErrCode);
					}

					tcpResult = (tcpErrCode == MiniUPnP.UPNPCOMMAND_SUCCESS);
				}

				if (udpResult && !tcpResult)
				{
					DebugLog.WriteLine("Deleting UDP mapping");
					try
					{
						MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "UDP");
					}
					catch(AccessViolationException)
					{
						// I have no idea why the above method sometimes throws an AccessException.
						// The odd part about it is that it works perfect, except for the stupid exception.
						// So the exception can safely be ignored, it just bugs me because it feels like a hack.
						DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
					}
				}
				if (tcpResult && !udpResult)
				{
					DebugLog.WriteLine("Deleting TCP mapping");
					try
					{
						MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "TCP");
					}
					catch(AccessViolationException)
					{
						// I have no idea why the above method sometimes throws an AccessException.
						// The odd part about it is that it works perfect, except for the stupid exception.
						// So the exception can safely be ignored, it just bugs me because it feels like a hack.
						DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
					}
				}

				if (udpResult && tcpResult)
				{
					// All attempted port mappings were successful
					portMapping.SetExternalPort(extPort);
					portMapping.SetMappingStatus(PortMappingStatus.Mapped);
					return true;
				}

				attemptCount++;
				if(attemptCount >= 10)
				{
					// We've tried 10 different mappings and still no success
					done = true;
				}
				else if (!udpResult && udpErrCode != 718)
				{
					// We received non-conflict error
					done = true;
				}
				else if (!tcpResult && tcpErrCode != 718)
				{
					// We received non-conflict error
					done = true;
				}

			} while (!done);

			portMapping.SetMappingStatus(PortMappingStatus.Unmapped);
			return false;
		}

		private bool RemovePortMapping(PortMapping portMapping)
		{
			// Make sure the mapping still belongs to us
			IPAddress ourIP = PortMapper.SharedInstance.LocalIPAddress;
			String ourDescription = GetPortMappingDescription();

			bool udpMappingStolen = false;
			bool tcpMappingStolen = false;

			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.UDP) > 0)
			{
				ExistingUPnPPortMapping existingPM;
				if (existingUPnPPortMappingsUdpDict.TryGetValue(portMapping.ExternalPort, out existingPM))
				{
					if (!existingPM.LocalAddress.Equals(ourIP) || !existingPM.Description.Equals(ourDescription))
					{
						// The mapping was stolen by another machine or process
						// Do not remove it, but for our purposes we can consider it removed

						DebugLog.WriteLine("UPnP: RemovePortMapping: UDP mapping stolen");
						udpMappingStolen = true;
					}
				}
			}
			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.TCP) > 0)
			{
				ExistingUPnPPortMapping existingPM;
				if (existingUPnPPortMappingsTcpDict.TryGetValue(portMapping.ExternalPort, out existingPM))
				{
					if (!existingPM.LocalAddress.Equals(ourIP) || !existingPM.Description.Equals(ourDescription))
					{
						// The mapping was stolen by another machine or process
						// Do not remove it, but for our purposes we can consider it removed

						DebugLog.WriteLine("UPnP: RemovePortMapping: TCM mapping stolen");
						tcpMappingStolen = true;
					}
				}
			}

			int result = MiniUPnP.UPNPCOMMAND_SUCCESS;

			bool udpResult = true;
			bool tcpResult = true;

			String extPortStr = portMapping.ExternalPort.ToString();

			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.UDP) > 0 && !udpMappingStolen)
			{
				try
				{
					result = MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "UDP");
				}
				catch(AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that it works perfect, except for the stupid exception.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
				}
				
				DebugLog.WriteLine("UPnP: RemovePortMapping: UDP: result = {0}", result);
				udpResult = (result == MiniUPnP.UPNPCOMMAND_SUCCESS);
			}
			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.TCP) > 0 && !tcpMappingStolen)
			{
				try
				{
					result = MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "TCP");
				}
				catch(AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that it works perfect, except for the stupid exception.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
				}

				DebugLog.WriteLine("UPnP: RemovePortMapping: TCP: result = {0}", result);
				tcpResult = (result == MiniUPnP.UPNPCOMMAND_SUCCESS);
			}

			portMapping.SetMappingStatus(PortMappingStatus.Unmapped);

			return (udpResult && tcpResult);
		}

		private bool RemovePortMapping(ExistingUPnPPortMapping portMapping)
		{
			int result = MiniUPnP.UPNPCOMMAND_SUCCESS;

			bool udpResult = true;
			bool tcpResult = true;

			String extPortStr = portMapping.ExternalPort.ToString();

			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.UDP) > 0)
			{
				try
				{
					result = MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "UDP");
				}
				catch(AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that it works perfect, except for the stupid exception.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
				}

				DebugLog.WriteLine("UPnP: RemovePortMapping: UDP: result = {0}", result);
				udpResult = (result == MiniUPnP.UPNPCOMMAND_SUCCESS);
			}
			if ((portMapping.TransportProtocol & PortMappingTransportProtocol.TCP) > 0)
			{
				try
				{
					result = MiniUPnP.UPNP_DeletePortMapping(urls.controlURL, igddata.ServiceType, extPortStr, "TCP");
				}
				catch (AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that it works perfect, except for the stupid exception.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_DeletePortMapping");
				}

				DebugLog.WriteLine("UPnP: RemovePortMapping: TCP: result = {0}", result);
				tcpResult = (result == MiniUPnP.UPNPCOMMAND_SUCCESS);
			}

			return (udpResult && tcpResult);
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

			IntPtr devlistP = IntPtr.Zero;
			byte[] lanAddr = new byte[16];
			byte[] externalAddr = new byte[16];
			
			bool didFail = false;

			devlistP = MiniUPnP.upnpDiscover(2500, IntPtr.Zero, IntPtr.Zero);
			if (devlistP == IntPtr.Zero)
			{
				DebugLog.WriteLine("UPnP: No IDG Device found on the network (1)");
				didFail = true;
			}
			else
			{
				MiniUPnP.UPNPDev devlist = MiniUPnP.PtrToUPNPDev(devlistP);
				
				// Check all of the devices for reachability
				bool foundIDGDevice = false;
				MiniUPnP.UPNPDev device = devlist;

				IPAddress routerIP = PortMapper.SharedInstance.RouterIPAddress;

				List<String> descURLs = new List<String>();

				bool done = false;
				while (!done)
				{
					try
					{
						Uri uri = new Uri(device.descURL);

						if (routerIP != null)
						{
							if (uri.Host == routerIP.ToString())
							{
								descURLs.Insert(0, device.descURL);
							}
							else
							{
								descURLs.Add(device.descURL);
							}
						}
						else
						{
							descURLs.Add(device.descURL);
						}
					}
					catch(Exception e)
					{
						DebugLog.WriteLine("UPnP: Error while inspecting url: {0}", device.descURL);
						DebugLog.WriteLine("UPnP: Exception: {0}", e);
					}

					if(device.pNext == IntPtr.Zero)
						done = true;
					else
						device = device.Next;
				}

				for (int i = 0; i < descURLs.Count && !foundIDGDevice; i++)
				{
					String url = descURLs[i];
					DebugLog.WriteLine("UPnP: Trying URL: {0}", url);

					// Reset service type.
					// This will help us determine if the exception below can safely be ignored.
					igddata.ServiceType = null;

					int r = 0;
					try
					{
					//	r = MiniUPnP.UPNP_GetIGDFromUrl(url, ref urls, ref igddata, lanAddr, lanAddr.Length);

						MiniUPnP.UPNPUrls_2 urls_2 = new MiniUPnP.UPNPUrls_2();
						unsafe
						{
							r = MiniUPnP.UPNP_GetIGDFromUrl(url, &urls_2, ref igddata, lanAddr, lanAddr.Length);
							MiniUPnP.FreeUPNPUrls(&urls_2);
						}

						// Find urls
						GetUPNPUrls(url);
					}
					catch(AccessViolationException)
					{
						// I have no idea why the above method sometimes throws an AccessException.
						// The odd part about it is that all the data gets marshaled over and back properly.
						// So the exception can safely be ignored, it just bugs me because it feels like a hack.
						DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_GetIGDFromUrl");

						if (igddata.ServiceType != null)
						{
							r = 1;
						}
					}

					if (r == 1)
					{
						r = MiniUPnP.UPNPCOMMAND_UNKNOWN_ERROR;
						try
						{
							r = MiniUPnP.UPNP_GetExternalIPAddress(urls.controlURL, igddata.ServiceType, externalAddr);
						}
						catch(AccessViolationException)
						{
							// I have no idea why the above method sometimes throws an AccessException.
							// The odd part about it is that all the data gets marshaled over and back properly.
							// So the exception can safely be ignored, it just bugs me because it feels like a hack.
							DebugLog.WriteLine("Ignoring exception from method MiniUPnP.UPNP_GetExternalIPAddress");

							if (externalAddr[0] != 0)
							{
								r = MiniUPnP.UPNPCOMMAND_SUCCESS;
							}
						}

						if (r != MiniUPnP.UPNPCOMMAND_SUCCESS)
						{
							DebugLog.WriteLine("UPnP: GetExternalIPAddress returned {0}", r);
						}
						else
						{
							IPAddress externalIP;
							IPAddress.TryParse(MiniUPnP.NullTerminatedArrayToString(externalAddr), out externalIP);

							if(externalIP != null)
							{
								OnDidGetExternalIPAddress(externalIP);

								foundIDGDevice = true;
								didFail = false;
							}
						}
					}
				}

				if (!foundIDGDevice)
				{
					DebugLog.WriteLine("UPnP: No IDG Device found on the network (2)");
					didFail = true;
				}

				try
				{
					MiniUPnP.freeUPNPDevlist(devlistP);
				}
				catch(AccessViolationException)
				{
					// I have no idea why the above method sometimes throws an AccessException.
					// The odd part about it is that all the data gets marshaled over and back properly.
					// So the exception can safely be ignored, it just bugs me because it feels like a hack.
					DebugLog.WriteLine("Ignoring exception from method MiniUPnP.freeUPNPDevlist");
				}
			}

			Monitor.Exit(multiThreadLock);

			if (refreshExternalIPThreadFlags != ThreadFlags.None)
			{
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

		private void GetUPNPUrls(String url)
		{
			if (String.IsNullOrEmpty(igddata.urlbase))
				urls.ipcondescURL = url;
			else
				urls.ipcondescURL = igddata.urlbase;

			int index_fin_url = urls.ipcondescURL.IndexOf('/', 7); // 7 = http://

			if (index_fin_url >= 0)
			{
				urls.ipcondescURL = urls.ipcondescURL.Substring(0, index_fin_url);
			}

			urls.controlURL = urls.ipcondescURL + igddata.controlurl;
			urls.controlURL_CIF = urls.ipcondescURL + igddata.controlurl_CIF;
			urls.ipcondescURL += igddata.scpdurl;
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

			// Remove existing mappings scheduled for removal.
			// These are mappings that weren't created by us, but have been explicity set for removal.

			List<ExistingUPnPPortMapping> existingMappingsToRemove;
			existingMappingsToRemove = PortMapper.SharedInstance.ExistingUPnPPortMappingsToRemove;
			lock (existingMappingsToRemove)
			{
				while ((existingMappingsToRemove.Count > 0) && (updatePortMappingsThreadFlags == ThreadFlags.None))
				{
					ExistingUPnPPortMapping existingMappingToRemove = existingMappingsToRemove[0];

					RemovePortMapping(existingMappingToRemove);

					existingMappingsToRemove.RemoveAt(0);
				}
			}

			// We need to safeguard mappings that others might have made.
			// UPnP is quite generous in giving us what we want,
			// even if other mappings are there, especially from the same local machine.

			DoUpdateExistingUPnPPortMappings();

			// Remove mappings scheduled for removal

			List<PortMapping> mappingsToRemove = PortMapper.SharedInstance.PortMappingsToRemove;
			lock (mappingsToRemove)
			{
				while ((mappingsToRemove.Count > 0) && (updatePortMappingsThreadFlags == ThreadFlags.None))
				{
					PortMapping mappingToRemove = mappingsToRemove[0];

					if (mappingToRemove.MappingStatus == PortMappingStatus.Mapped)
					{
						RemovePortMapping(mappingToRemove);
					}

					mappingsToRemove.RemoveAt(0);
				}
			}

			// If the port mapper is running:
			//   -Add new mappings
			// If the port mapper is stopped:
			//   -Remove any existing mappings

			List<PortMapping> mappings = PortMapper.SharedInstance.PortMappings;
			lock (mappings)
			{
				for (int i = 0; i < mappings.Count && updatePortMappingsThreadFlags == ThreadFlags.None; i++)
				{
					PortMapping currentMapping = mappings[i];
					bool isRunning = PortMapper.SharedInstance.IsRunning;

					if (currentMapping.MappingStatus == PortMappingStatus.Unmapped && isRunning)
					{
						AddPortMapping(currentMapping);
					}
					else if (currentMapping.MappingStatus == PortMappingStatus.Mapped && !isRunning)
					{
						RemovePortMapping(currentMapping);
					}
				}
			}

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
			}

			OnDidEndWorking();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Update Existing UPnP Port Mappings Thread
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void UpdateExistingUPnPMappingsThread()
		{
			Monitor.Enter(multiThreadLock);
			OnDidBeginWorking();

			DoUpdateExistingUPnPPortMappings();

			Monitor.Exit(multiThreadLock);
			OnDidEndWorking();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}
