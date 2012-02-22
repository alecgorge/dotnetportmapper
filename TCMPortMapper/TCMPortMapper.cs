using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

namespace TCMPortMapper
{
	/// <summary>
	/// Status of PortMapping object.
	/// </summary>
	public enum PortMappingStatus
	{
		Unmapped = 0,
		Trying   = 1,
		Mapped   = 2
	}

	/// <summary>
	/// Protocol used for PortMapping object.
	/// </summary>
	public enum PortMappingTransportProtocol
	{
		UDP  = 1,
		TCP  = 2,
		Both = 3
	}

	public class PortMapping
	{
		private UInt16 localPort;
		private UInt16 externalPort;
		private UInt16 desiredExternalPort;
		private PortMappingStatus mappingStatus;
		private PortMappingTransportProtocol transportProtocol;

		public PortMapping(UInt16 localPort, UInt16 desiredExternalPort, PortMappingTransportProtocol protocol)
		{
			this.localPort = localPort;
			this.desiredExternalPort = desiredExternalPort;
			this.transportProtocol = protocol;

			this.mappingStatus = PortMappingStatus.Unmapped;
		}

		public UInt16 LocalPort
		{
			get { return localPort; }
		}

		public UInt16 DesiredExternalPort
		{
			get { return desiredExternalPort; }
		}

		public PortMappingTransportProtocol TransportProtocol
		{
			get { return transportProtocol; }
		}

		public UInt16 ExternalPort
		{
			get { return externalPort; }
		}

		public PortMappingStatus MappingStatus
		{
			get { return mappingStatus; }
		}

		internal void SetExternalPort(UInt16 port)
		{
			externalPort = port;
		}

		internal void SetMappingStatus(PortMappingStatus newMappingStatus)
		{
			if (mappingStatus != newMappingStatus)
			{
				mappingStatus = newMappingStatus;
				if (mappingStatus == PortMappingStatus.Unmapped)
				{
					externalPort = 0;
				}
				PortMapper.SharedInstance.OnDidChangeMappingStatus(this);
			}
		}
	}

	public class PortMapper
	{
		/// <summary>
		/// Singleton instance of class
		/// </summary>
		private static PortMapper sharedInstance;

		/// <summary>
		/// Static constructor.
		/// - executes before any instance of the class is created.
		/// - executes before any of the static members for the class are referenced.
		/// - executes after the static field initializers (if any) for the class.
		/// - executes at most one time during a single program instantiation.
		/// - called automatically to initialize the class before the first instance is created or any static members are referenced.
		/// </summary>
		static PortMapper()
		{
			sharedInstance = new PortMapper();
		}

		/// <summary>
		/// Returns the sharedInstance of the PortMapper.
		/// This is the sole instance that is to be used throughout the library.
		/// </summary>
		public static PortMapper SharedInstance
		{
			get { return sharedInstance; }
		}

		public delegate void PMExternalIPAddressDidChange(PortMapper sender, IPAddress ip);
		public delegate void PMWillStartSearchForRouter(PortMapper sender);
		public delegate void PMDidFinishSearchForRouter(PortMapper sender);
		public delegate void PMDidStartWork(PortMapper sender);
		public delegate void PMDidFinishWork(PortMapper sender);
		public delegate void PMDidReceiveUPNPMappingTable(PortMapper sender, List<ExistingUPnPPortMapping> mappings);
		public delegate void PMDidChangeMappingStatus(PortMapper sender, PortMapping pm);

		public event PMExternalIPAddressDidChange ExternalIPAddressDidChange;
		public event PMWillStartSearchForRouter WillStartSearchForRouter;
		public event PMDidFinishSearchForRouter DidFinishSearchForRouter;
		public event PMDidStartWork DidStartWork;
		public event PMDidFinishWork DidFinishWork;
		public event PMDidReceiveUPNPMappingTable DidReceiveUPNPMappingTable;
		public event PMDidChangeMappingStatus DidChangeMappingStatus;

		private enum MappingProtocol
		{
			None = 0,
			NATPMP = 1,
			UPnP = 2
		}

		private enum MappingStatus
		{
			Failed = 0,
			Trying = 1,
			Works = 2
		}

		private NATPMPPortMapper natpmpPortMapper;
		private UPnPPortMapper upnpPortMapper;

		private List<PortMapping> portMappings;         // Active mappings, and mappings to add
		private List<PortMapping> portMappingsToRemove; // Active mappings that should be removed

		private List<ExistingUPnPPortMapping> existingUPnPPortMappingsToRemove;

		private volatile bool isRunning;

		private MappingStatus natpmpStatus;
		private MappingStatus upnpStatus;
		private MappingProtocol mappingProtocol;

		private String routerManufacturer;
		private IPAddress routerIPAddress;
		private IPAddress localIPAddress;
		private IPAddress externalIPAddress;

		private bool localIPOnRouterSubnet;

		private int workCount;

		private Object multiThreadLock = new Object();
		private Object singleThreadLock = new Object();
		private Object workLock = new Object();

		private bool isGoingToSleep;
		private bool isNetworkAvailable;

		private bool requestedUPnPMappingTable;

		private PortMapper()
		{
			natpmpPortMapper = new NATPMPPortMapper();
			upnpPortMapper = new UPnPPortMapper();

			portMappings = new List<PortMapping>();
			portMappingsToRemove = new List<PortMapping>();

			existingUPnPPortMappingsToRemove = new List<ExistingUPnPPortMapping>();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		protected virtual void OnExternalIPAddressDidChange()
		{
			if (ExternalIPAddressDidChange != null)
			{
				Invoke(ExternalIPAddressDidChange, this, externalIPAddress);
			}
		}

		protected virtual void OnWillStartSearchForRouter()
		{
			if (WillStartSearchForRouter != null)
			{
				Invoke(WillStartSearchForRouter, this);
			}
		}

		protected virtual void OnDidFinishSearchForRouter()
		{
			if (DidFinishSearchForRouter != null)
			{
				Invoke(DidFinishSearchForRouter, this);
			}
		}

		protected virtual void OnDidStartWork()
		{
			if (DidStartWork != null)
			{
				Invoke(DidStartWork, this);
			}
		}

		protected virtual void OnDidFinishWork()
		{
			if (DidFinishWork != null)
			{
				Invoke(DidFinishWork, this);
			}
		}

		protected virtual void OnDidReceiveUPNPMappingTable(List<ExistingUPnPPortMapping> mappings)
		{
			if (DidReceiveUPNPMappingTable != null)
			{
				Invoke(DidReceiveUPNPMappingTable, this, mappings);
			}
		}

		internal virtual void OnDidChangeMappingStatus(PortMapping pm)
		{
			// It is vitally important that we Invoke this on a background thread!
			// The mapping status is generally changed within the context of a lock on the mappings list.
			// This lock is also used within public API methods,
			// so if we don't use a background thread, there's a potential for deadlock.
			Thread bgThread = new Thread(new ParameterizedThreadStart(OnDidChangeMappingStatusThread));
			bgThread.IsBackground = true;
			bgThread.Start(pm);
		}

		protected virtual void OnDidChangeMappingStatusThread(Object pm)
		{
			if (DidChangeMappingStatus != null)
			{
				Invoke(DidChangeMappingStatus, this, (PortMapping)pm);
			}
		}

		private System.ComponentModel.ISynchronizeInvoke mSynchronizingObject = null;
		/// <summary>
		/// Set the <see cref="System.ComponentModel.ISynchronizeInvoke">ISynchronizeInvoke</see>
		/// object to use as the invoke object. When returning results from asynchronous calls,
		/// the Invoke method on this object will be called to pass the results back
		/// in a thread safe manner.
		/// </summary>
		/// <remarks>
		/// If using in conjunction with a form, it is highly recommended
		/// that you pass your main <see cref="System.Windows.Forms.Form">form</see> (window) in.
		/// </remarks>
		public System.ComponentModel.ISynchronizeInvoke SynchronizingObject
		{
			get { return mSynchronizingObject; }
			set { mSynchronizingObject = value; }
		}

		private bool mAllowApplicationForms = true;
		/// <summary>
		/// Allows the application to attempt to post async replies over the
		/// application "main loop" by using the message queue of the first available
		/// open form (window). This is retrieved through
		/// <see cref="System.Windows.Forms.Application.OpenForms">Application.OpenForms</see>.
		/// 
		/// Note: This is true by default.
		/// </summary>
		public bool AllowApplicationForms
		{
			get { return mAllowApplicationForms; }
			set { mAllowApplicationForms = value; }
		}

		private bool mAllowMultithreadedCallbacks = false;
		/// <summary>
		/// If set to true, <see cref="AllowApplicationForms">AllowApplicationForms</see>
		/// is set to false and <see cref="SynchronizingObject">SynchronizingObject</see> is set
		/// to null. Any time an asynchronous method needs to invoke a delegate method
		/// it will run the method in its own thread.
		/// </summary>
		/// <remarks>
		/// If set to true, you will have to handle any synchronization needed.
		/// If your application uses Windows.Forms or any other non-thread safe
		/// library, then you will have to do your own invoking.
		/// </remarks>
		public bool AllowMultithreadedCallbacks
		{
			get { return mAllowMultithreadedCallbacks; }
			set
			{
				mAllowMultithreadedCallbacks = value;
				if (mAllowMultithreadedCallbacks)
				{
					mAllowApplicationForms = false;
					mSynchronizingObject = null;
				}
			}
		}

		/// <summary>
		/// Helper method to obtain a proper invokeable object.
		/// If an invokeable object is set, it's immediately returned.
		/// Otherwise, an open windows form is returned if available.
		/// </summary>
		/// <returns>An invokeable object, or null if none available.</returns>
		private System.ComponentModel.ISynchronizeInvoke GetInvokeObject()
		{
			if (mSynchronizingObject != null) return mSynchronizingObject;

			if (mAllowApplicationForms)
			{
				// Need to post it over control thread
				System.Windows.Forms.FormCollection forms = System.Windows.Forms.Application.OpenForms;

				if (forms != null && forms.Count > 0)
				{
					System.Windows.Forms.Control control = forms[0];
					return control;
				}
			}
			return null;
		}

		/// <summary>
		/// Calls a method using the objects invokable object (if provided).
		/// Otherwise, it simply invokes the method normally.
		/// </summary>
		/// <param name="method">
		///		The method to call.
		/// </param>
		/// <param name="args">
		///		The arguments to call the method with.
		/// </param>
		/// <returns>
		///		The result returned from method, or null if the method could not be invoked.
		///	</returns>
		internal object Invoke(Delegate method, params object[] args)
		{
			System.ComponentModel.ISynchronizeInvoke invokeable = GetInvokeObject();

			try
			{
				if (invokeable != null)
				{
					return invokeable.Invoke(method, args);
				}

				if (mAllowMultithreadedCallbacks)
				{
					return method.DynamicInvoke(args);
				}
			}
			catch { }

			return null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Public Properties
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool IsRunning
		{
			get { return isRunning; }
		}

		public IPAddress LocalIPAddress
		{
			get { return localIPAddress; }
		}

		public IPAddress ExternalIPAddress
		{
			get { return externalIPAddress; }
		}

		public String RouterManufacturer
		{
			get { return routerManufacturer; }
		}

		public IPAddress RouterIPAddress
		{
			get { return routerIPAddress; }
		}

		public String MappingProtocolName
		{
			get
			{
				if (mappingProtocol == MappingProtocol.NATPMP)
					return "NAT-PMP";
				else if (mappingProtocol == MappingProtocol.UPnP)
					return "UPnP";
				else
					return "None";
			}
		}

		public String DefaultLocalBonjourHostName
		{
			get { return System.Net.Dns.GetHostName() + ".local"; }
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Internal Properties
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		internal List<PortMapping> PortMappings
		{
			get { return portMappings; }
		}

		internal List<PortMapping> PortMappingsToRemove
		{
			get { return portMappingsToRemove; }
		}

		internal List<ExistingUPnPPortMapping> ExistingUPnPPortMappingsToRemove
		{
			get { return existingUPnPPortMappingsToRemove; }
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Public Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Starts the PortMapper.
		/// This will immediately begin the search for the router and external ip address.
		/// </summary>
		public void Start()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (!isRunning)
				{
					// Initialize winsock
					Win32.WSAData d;
					Win32.WSAStartup(2, out d);

					AddSystemEventDelegates();
					AddNATPMPDelegates();
					AddUPnPDelegates();

					isRunning = true;
				}
				Refresh();
			}
		}

		/// <summary>
		/// Asynchronously adds the given port mapping.
		/// </summary>
		/// <param name="pm">
		///		The port mapping to add.
		///		Note: Many UPnP routers only support port mappings where localPort == externalPort.
		///	</param>
		public void AddPortMapping(PortMapping pm)
		{
			if(pm == null) return;

			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				lock (portMappings)
				{
					portMappings.Add(pm);
				}
				if (isRunning) UpdatePortMappings();
			}
		}

		/// <summary>
		/// Asynchronously removes the given port mapping.
		/// </summary>
		/// <param name="pm">
		///		The port mapping to remove.
		/// </param>
		public void RemovePortMapping(PortMapping pm)
		{
			if(pm == null) return;

			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				lock (portMappings)
				{
					portMappings.Remove(pm);
				}
				lock (portMappingsToRemove)
				{
					if (pm.MappingStatus != PortMappingStatus.Unmapped)
					{
						portMappingsToRemove.Add(pm);
					}
				}
				if (isRunning) UpdatePortMappings();
			}
		}

		/// <summary>
		/// Asynchronously removes the given port mapping.
		/// 
		/// This method will also automatically refresh the UPnP mapping table,
		/// and call the DidReceiveUPNPMappingTable delegate.
		/// </summary>
		/// <param name="pm">
		///		The port mapping to remove.
		///	</param>
		public void RemovePortMapping(ExistingUPnPPortMapping pm)
		{
			if(pm == null) return;

			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (upnpStatus == MappingStatus.Works)
				{
					lock (existingUPnPPortMappingsToRemove)
					{
						existingUPnPPortMappingsToRemove.Add(pm);
					}

					if (isRunning)
					{
						requestedUPnPMappingTable = true;
						UpdatePortMappings();
					}
				}
			}
		}

		/// <summary>
		/// Refreshes all port mapping information, and all port mappings.
		/// </summary>
		public void Refresh()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (isRunning)
				{
					Thread bgThread = new Thread(new ThreadStart(RefreshThread));
					bgThread.Start();
				}
			}
		}

		public void RequestUPnPMappingTable()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (isRunning)
				{
					if (upnpStatus == MappingStatus.Works)
					{
						requestedUPnPMappingTable = true;
						upnpPortMapper.UpdateExistingUPnPPortMappings();
					}
				}
			}
		}

		/// <summary>
		/// Asynchronously stops the port mapper.
		/// All added port mappings will be removed.
		/// </summary>
		public void Stop()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (isRunning)
				{
					RemoveSystemEventDelegates();
					RemoveNATPMPDelegates();
					RemoveUPnPDelegates();
					isRunning = false;

					if (natpmpStatus == MappingStatus.Works)
					{
						natpmpPortMapper.Stop();
					}
					if (upnpStatus == MappingStatus.Works)
					{
						upnpPortMapper.Stop();
					}

					Win32.WSACleanup();
				}
			}
		}

		/// <summary>
		/// Synchronously stops the port mapper.
		/// All added port mappings will be removed.
		/// </summary>
		public void StopBlocking()
		{
			// All public API methods are wrapped in a single thread lock.
			// This frees users to invoke the public API from multiple threads, but provides us a bit of sanity.
			lock (singleThreadLock)
			{
				if (isRunning)
				{
					RemoveSystemEventDelegates();
					RemoveNATPMPDelegates();
					RemoveUPnPDelegates();
					isRunning = false;

					if (natpmpStatus == MappingStatus.Works)
					{
						natpmpPortMapper.StopBlocking();
					}
					if (upnpStatus == MappingStatus.Works)
					{
						upnpPortMapper.StopBlocking();
					}

					Win32.WSACleanup();
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Refresh Thread
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void RefreshThread()
		{
			lock(multiThreadLock)
			{
				IncreaseWorkCount();

				mappingProtocol = MappingProtocol.None;
				externalIPAddress = null;

				lock (portMappings)
				{
					for (int i = 0; i < portMappings.Count; i++)
					{
						PortMapping pm = (PortMapping)portMappings[i];
						if (pm.MappingStatus == PortMappingStatus.Mapped)
						{
							pm.SetMappingStatus(PortMappingStatus.Unmapped);
						}
					}
				}

				OnWillStartSearchForRouter();

				DebugLog.WriteLine("RefreshThread");

				// Update all the following variables:
				// - routerIPAddress
				// - routerManufacturer
				// - localIPAddress
				// - localIPAddressOnSubnet
				// 
				// Note: The order in which we call the following methods matters.
				// We must update the routerIPAddress before the others.
				UpdateRouterIPAddress();
				UpdateRouterManufacturer();
				UpdateLocalIPAddress();

				DebugLog.WriteLine("routerIPAddress       : {0}", routerIPAddress);
				DebugLog.WriteLine("routerManufacturer    : {0}", routerManufacturer);
				DebugLog.WriteLine("localIPAddress        : {0}", localIPAddress);
				DebugLog.WriteLine("localIPAddressOnSubnet: {0}", localIPOnRouterSubnet);

				if (routerIPAddress != null)
				{
					if ((localIPAddress != null) && localIPOnRouterSubnet)
					{
						externalIPAddress = null;

						if (IsIPv4AddressInPrivateSubnet(routerIPAddress))
						{
							natpmpStatus = MappingStatus.Trying;
							upnpStatus = MappingStatus.Trying;

							natpmpPortMapper.Refresh();
							upnpPortMapper.Refresh();
						}
						else
						{
							natpmpStatus = MappingStatus.Failed;
							upnpStatus = MappingStatus.Failed;
							externalIPAddress = localIPAddress;
							mappingProtocol = MappingProtocol.None;

							// Set all mappings to be mapped with their local port number being the external one
							lock (portMappings)
							{
								for (int i = 0; i < portMappings.Count; i++)
								{
									PortMapping pm = (PortMapping)portMappings[i];
									pm.SetExternalPort(pm.LocalPort);
									pm.SetMappingStatus(PortMappingStatus.Mapped);
								}
							}

							OnDidFinishSearchForRouter();
						}
					}
					else
					{
						OnDidFinishSearchForRouter();
					}
				}
				else
				{
					OnDidFinishSearchForRouter();
				}

				// If we call DecreaseWorkCount right now, then it will likely result in a call to DidFinishWork.
				// This is because we the background threads that get called likely haven't started yet.
				// Therefore we delay this call, so as to allow the background threads to start first.
				new Timer(new TimerCallback(DecreaseWorkCount), null, (1 * 1000), Timeout.Infinite);
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Private API
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Updates the routerIPAddress variable.
		/// </summary>
		private void UpdateRouterIPAddress()
		{
			routerIPAddress = null;

			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

			bool found = false;
			for (int i = 0; i < networkInterfaces.Length && !found; i++)
			{
				NetworkInterface networkInterface = networkInterfaces[i];

				if (networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					GatewayIPAddressInformationCollection gateways;
					gateways = networkInterface.GetIPProperties().GatewayAddresses;

					for (int j = 0; j < gateways.Count && !found; j++)
					{
						GatewayIPAddressInformation gatewayInfo = gateways[j];

						if (gatewayInfo.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							if (!gatewayInfo.Address.Equals(IPAddress.Any))
							{
								routerIPAddress = gatewayInfo.Address;
								found = true;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Updates the routerManufacturer variable.
		/// This is done by getting the MAC address of the router,
		/// and then looking up the corresponding manufacturer in the OUI list.
		/// 
		/// The routerIPAddress variable should be set prior to calling this method.
		/// </summary>
		private void UpdateRouterManufacturer()
		{
			DebugLog.WriteLine("UpdateRouterManufacturer()");

			routerManufacturer = "Unknown";

			if (routerIPAddress == null)
			{
				return;
			}

			Exception e;

			PhysicalAddress routerMac = GetHardwareAddressForIPv4Address(routerIPAddress, out e);
			if (routerMac == null)
			{
				DebugLog.WriteLine("PortMapper: Error getting router mac address: {0}", e);
				return;
			}

			String result = GetManufacturerForHardwareAddress(routerMac, out e);
			if (result == null)
			{
				if (e == null)
					DebugLog.WriteLine("PortMapper: Router MAC address not in OUI list");
				else
					DebugLog.WriteLine("PortMapper: Error getting router manufacturer: {0}", e);
			}
			else
			{
				routerManufacturer = result;
			}
		}

		/// <summary>
		/// Updates the localIPAddress variable, and the related localIPOnRouterSubnet variable.
		/// 
		/// The routerIPAddress variable should be set prior to calling this method.
		/// </summary>
		private void UpdateLocalIPAddress()
		{
			localIPAddress = null;
			
			IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in localhost.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIPAddress = ip;
					break;
				}
			}

			if (routerIPAddress != null)
			{
				localIPOnRouterSubnet = IsIPv4AddressInPrivateSubnet(localIPAddress);
			}
			else
			{
				localIPOnRouterSubnet = false;
			}
		}

		/// <summary>
		/// Registers for notifications of power and network events.
		/// </summary>
		private void AddSystemEventDelegates()
		{
			try
			{
				SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
				NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
				NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
			}
			catch (Exception e)
			{
				// I have no idea why this throws exceptions on some computers.
				// As a windows developer, it doesn't really surprise me though.
				DebugLog.WriteLine("PortMapper: AddSystemEventDelegates: {0}", e);
			}
		}

		/// <summary>
		/// Unregisters for notifications of power and network events.
		/// </summary>
		private void RemoveSystemEventDelegates()
		{
			try
			{
				SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
				NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
				NetworkChange.NetworkAvailabilityChanged -= new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
			}
			catch (Exception e)
			{
				// I have no idea why this throws exceptions on some computers.
				// As a windows developer, it doesn't really surprise me though.
				DebugLog.WriteLine("PortMapper: RemoveSystemEventDelegates: {0}", e);
			}
		}

		private void AddNATPMPDelegates()
		{
			natpmpPortMapper.DidBeginWorking += new NATPMPPortMapper.PMDidBeginWorking(natpmpPortMapper_DidBeginWorking);
			natpmpPortMapper.DidEndWorking += new NATPMPPortMapper.PMDidEndWorking(natpmpPortMapper_DidEndWorking);
			natpmpPortMapper.DidGetExternalIPAddress += new NATPMPPortMapper.PMDidGetExternalIPAddress(natpmpPortMapper_DidGetExternalIPAddress);
			natpmpPortMapper.DidFail += new NATPMPPortMapper.PMDidFail(natpmpPortMapper_DidFail);
			natpmpPortMapper.DidReceiveBroadcastExternalIPChange += new NATPMPPortMapper.PMDidReceiveBroadcastExternalIPChange(natpmpPortMapper_DidReceiveBroadcastExternalIPChange);
		}

		private void RemoveNATPMPDelegates()
		{
			natpmpPortMapper.DidBeginWorking -= new NATPMPPortMapper.PMDidBeginWorking(natpmpPortMapper_DidBeginWorking);
			natpmpPortMapper.DidEndWorking -= new NATPMPPortMapper.PMDidEndWorking(natpmpPortMapper_DidEndWorking);
			natpmpPortMapper.DidGetExternalIPAddress -= new NATPMPPortMapper.PMDidGetExternalIPAddress(natpmpPortMapper_DidGetExternalIPAddress);
			natpmpPortMapper.DidFail -= new NATPMPPortMapper.PMDidFail(natpmpPortMapper_DidFail);
			natpmpPortMapper.DidReceiveBroadcastExternalIPChange -= new NATPMPPortMapper.PMDidReceiveBroadcastExternalIPChange(natpmpPortMapper_DidReceiveBroadcastExternalIPChange);
		}

		private void AddUPnPDelegates()
		{
			upnpPortMapper.DidBeginWorking += new UPnPPortMapper.PMDidBeginWorking(upnpPortMapper_DidBeginWorking);
			upnpPortMapper.DidEndWorking += new UPnPPortMapper.PMDidEndWorking(upnpPortMapper_DidEndWorking);
			upnpPortMapper.DidGetExternalIPAddress += new UPnPPortMapper.PMDidGetExternalIPAddress(upnpPortMapper_DidGetExternalIPAddress);
			upnpPortMapper.DidFail += new UPnPPortMapper.PMDidFail(upnpPortMapper_DidFail);
		}

		private void RemoveUPnPDelegates()
		{
			upnpPortMapper.DidBeginWorking -= new UPnPPortMapper.PMDidBeginWorking(upnpPortMapper_DidBeginWorking);
			upnpPortMapper.DidEndWorking -= new UPnPPortMapper.PMDidEndWorking(upnpPortMapper_DidEndWorking);
			upnpPortMapper.DidGetExternalIPAddress -= new UPnPPortMapper.PMDidGetExternalIPAddress(upnpPortMapper_DidGetExternalIPAddress);
			upnpPortMapper.DidFail -= new UPnPPortMapper.PMDidFail(upnpPortMapper_DidFail);
		}

		private void UpdatePortMappings()
		{
			// This method is called from either AddPortMapping or RemovePortMapping

			if (mappingProtocol == MappingProtocol.NATPMP)
			{
				natpmpPortMapper.UpdatePortMappings();
			}
			else if (mappingProtocol == MappingProtocol.UPnP)
			{
				upnpPortMapper.UpdatePortMappings();
			}
		}

		/// <summary>
		/// Called from:
		///  - RefreshThread
		///  - UpdateLocalIPAddress - RefreshThread
		///  - GetRouterPhysicalAddress - GetRouterManufacturer - RefreshThread
		/// 
		///  - RouterIPAddress property
		/// 
		///  - natpmpPortMapper_DidReceiveBroadcastExternalIPChange
		/// </summary>
		/// <returns></returns>
//		private IPAddress GetRouterIPAddress()
//		{
//			UInt32 routerAddr = 0;
//			if (NATPMP.getdefaultgateway(ref routerAddr) < 0)
//			{
//				DebugLog.WriteLine("PortMapper: Unable to get router ip address");
//				return null;
//			}
//			
//			try
//			{
//				return new IPAddress((long)routerAddr);
//			}
//			catch (Exception e)
//			{
//				DebugLog.WriteLine("PortMapper: Unable to get router ip address: {0}", e);
//				return null;
//			}
//		}

		private void IncreaseWorkCount()
		{
			lock (workLock)
			{
				if (workCount == 0)
				{
					OnDidStartWork();
				}
				workCount++;
			}
		}

		private void DecreaseWorkCount()
		{
			lock (workLock)
			{
				workCount--;
				if (workCount == 0)
				{
					if (upnpStatus == MappingStatus.Works && requestedUPnPMappingTable)
					{
						OnDidReceiveUPNPMappingTable(upnpPortMapper.ExistingUPnPPortMappings);
						requestedUPnPMappingTable = false;
					}

					OnDidFinishWork();
				}
			}
		}

		private void DecreaseWorkCount(Object state)
		{
			// Called via timer (on a background thread)
			DecreaseWorkCount();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region NATPMPPortMapper Delegate Methods
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

		private void natpmpPortMapper_DidBeginWorking(NATPMPPortMapper sender)
		{
			IncreaseWorkCount();
		}

		private void natpmpPortMapper_DidEndWorking(NATPMPPortMapper sender)
		{
			DecreaseWorkCount();
		}

		private void natpmpPortMapper_DidGetExternalIPAddress(NATPMPPortMapper sender, System.Net.IPAddress ip)
		{
			bool shouldNotify = false;

			if (natpmpStatus == MappingStatus.Trying)
			{
				natpmpStatus = MappingStatus.Works;
				mappingProtocol = MappingProtocol.NATPMP;
				shouldNotify = true;
			}

			externalIPAddress = ip;
			OnExternalIPAddressDidChange();

			if (shouldNotify)
			{
				OnDidFinishSearchForRouter();
			}
		}

		private void natpmpPortMapper_DidFail(NATPMPPortMapper sender)
		{
			DebugLog.WriteLine("natpmpPortMapper_DidFail");

			if (natpmpStatus == MappingStatus.Trying)
			{
				natpmpStatus = MappingStatus.Failed;
			}
			else if (natpmpStatus == MappingStatus.Works)
			{
				externalIPAddress = null;
			}

			if (upnpStatus == MappingStatus.Failed)
			{
				OnDidFinishSearchForRouter();
			}
		}

		private void natpmpPortMapper_DidReceiveBroadcastExternalIPChange(NATPMPPortMapper sender, IPAddress ip, IPAddress senderIP)
		{
			if (isRunning)
			{
				DebugLog.WriteLine("natpmpPortMapper_DidReceiveBroadcastExternalIPChange");

				if (senderIP == localIPAddress)
				{
					DebugLog.WriteLine("Refreshing because of NAT-PMP device external IP broadcast");
					Refresh();
				}
				else
				{
					DebugLog.WriteLine("Got information from rogue NAT-PMP device");
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region UPnPPortMapper Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void upnpPortMapper_DidBeginWorking(UPnPPortMapper sender)
		{
			IncreaseWorkCount();
		}

		private void upnpPortMapper_DidEndWorking(UPnPPortMapper sender)
		{
			DecreaseWorkCount();
		}

		private void upnpPortMapper_DidGetExternalIPAddress(UPnPPortMapper sender, IPAddress ip)
		{
			DebugLog.WriteLine("upnpPortMapper_DidGetExternalIPAddress: {0}", ip);

			bool shouldNotify = false;

			if (upnpStatus == MappingStatus.Trying)
			{
				upnpStatus = MappingStatus.Works;
				mappingProtocol = MappingProtocol.UPnP;
				shouldNotify = true;
			}

			externalIPAddress = ip;
			OnExternalIPAddressDidChange();

			if (shouldNotify)
			{
				OnDidFinishSearchForRouter();
			}
		}

		private void upnpPortMapper_DidFail(UPnPPortMapper sender)
		{
			DebugLog.WriteLine("upnpPortMapper_DidFail");

			if (upnpStatus == MappingStatus.Trying)
			{
				upnpStatus = MappingStatus.Failed;
			}
			else if (upnpStatus == MappingStatus.Works)
			{
				externalIPAddress = null;
			}

			if (natpmpStatus == MappingStatus.Failed)
			{
				OnDidFinishSearchForRouter();
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region SystemEvent Delegate Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Suspend)
			{
				// System is going to sleep
				isGoingToSleep = true;

				if (isRunning)
				{
					if (natpmpStatus == MappingStatus.Works)
					{
						natpmpPortMapper.StopBlocking();
					}
					if (upnpStatus == MappingStatus.Works)
					{
						upnpPortMapper.StopBlocking();
					}
				}
			}
			else if (e.Mode == PowerModes.Resume)
			{
				// System is waking up (but may not have an internet connection yet)
				// Wait for network information to refresh
				isGoingToSleep = false;
			}
		}

		private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			isNetworkAvailable = e.IsAvailable;
		}

		private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
		{
			if (!isGoingToSleep)
			{
				if (isNetworkAvailable)
				{
					DebugLog.WriteLine("Refreshing because of network change");
					Refresh();
				}
				else
				{
					// Ignore - System does not yet have network restored
				}
			}
			else
			{
				// Ignore - System is going to sleep
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Utility Methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private bool IsIPv4AddressInPrivateSubnet(IPAddress myAddr)
		{
			if(myAddr == null) return false;

			// Private subnets as defined in http://tools.ietf.org/html/rfc1918
			// Loopback address 127.0.0.1/8 http://tools.ietf.org/html/rfc3330
			// Zeroconf/bonjour self assigned addresses 169.254.0.0/16 http://tools.ietf.org/html/rfc3927

			String[] netAddrs = {"192.168.0.0",  "10.0.0.0",  "172.16.0.0", "127.0.0.1", "169.254.0.0"};
			String[] netMasks = {"255.255.0.0", "255.0.0.0", "255.240.0.0", "255.0.0.0", "255.255.0.0"};

			UInt32 myIP = BitConverter.ToUInt32(myAddr.GetAddressBytes(), 0);

			for (int i = 0; i < netMasks.Length; i++)
			{
				IPAddress netAddr = IPAddress.Parse(netAddrs[i]);
				UInt32 netIP = BitConverter.ToUInt32(netAddr.GetAddressBytes(), 0);

				IPAddress maskAddr = IPAddress.Parse(netMasks[i]);
				UInt32 maskIP = BitConverter.ToUInt32(maskAddr.GetAddressBytes(), 0);

				if ((myIP & maskIP) == (netIP & maskIP))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Only called from GetRouterPhysicalAddress()
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		private PhysicalAddress GetHardwareAddressForIPv4Address(IPAddress ip, out Exception e)
		{
			if (ip == null)
			{
				e = new ArgumentNullException();
				return null;
			}

			if (ip.AddressFamily != AddressFamily.InterNetwork)
			{
				e = new ArgumentException("Only supports IPv4 addresses");
				return null;
			}

			UInt32 dstAddrInt = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
			UInt32 srcAddrInt = 0;

			byte[] mac = new byte[6]; // 48 bit
			int length = mac.Length;
			int reply = Win32.SendARP(dstAddrInt, srcAddrInt, mac, ref length);

			if (reply != 0)
			{
				e = new System.ComponentModel.Win32Exception(reply);
				return null;
			}

			e = null;
			return new PhysicalAddress(mac);
		}

		/// <summary>
		/// Searches for a match for the given mac address in the oui.txt file.
		/// If a match is found, the corresponding company name is returned.
		/// If a match is not found, null is returned.
		/// If an error occurs, null is returned, and the exception is set.
		/// 
		/// Note: The oui list may contain missing names, so an empty string may be returned.
		/// </summary>
		private String GetManufacturerForHardwareAddress(PhysicalAddress macAddr, out Exception e)
		{
			if (macAddr == null)
			{
				e = new ArgumentNullException();
				return null;
			}

			String macAddrPrefix = macAddr.ToString().Substring(0, 6);

			StreamReader streamReader = null;
			String result = null;

			try
			{
				// OUI - Organizationally Unique Identifier
				// 
				// If you wish to update the list of OUI's, you can get the latest version here:
				// http://standards.ieee.org/regauth/oui/index.shtml
				// Then format the list using the ReformatOUI method below.
				// Ensure that the oui file is in UTF-8.

				streamReader = File.OpenText("oui.txt");
				String line = streamReader.ReadLine();

				while ((line != null) && (result == null))
				{
					if (line.StartsWith(macAddrPrefix))
					{
						result = line.Substring(6).Trim();
					}
					line = streamReader.ReadLine();
				}
			}
			catch (Exception ex)
			{
				e = ex;
			}
			finally
			{
				if(streamReader != null) streamReader.Close();
			}

			e = null;
			return result;
		}

		/// <summary>
		/// This method is not used in the framework, but may be used by developers to properly
		/// format the most recently obtained list of OUI's.
		/// The list can be obtained from IEEE:
		/// http://standards.ieee.org/regauth/oui/index.shtml
		/// </summary>
		/// <param name="inFilePath">
		///		Path to downloaded oui text file.
		///		This file should be encoded in UTF-8 format.
		/// </param>
		/// <param name="outFilePath">
		///		Path to output formatted out file.
		///		This file will be created or overwritten.
		///	</param>
		private void ReformatOUI(String inFilePath, String outFilePath)
		{
			StreamReader streamReader = File.OpenText(inFilePath);
			String line = streamReader.ReadLine();

			StreamWriter streamWriter = File.CreateText(outFilePath);

			uint lineCount = 0;
			uint badLineCount = 0;

			while (line != null)
			{
				if (line.Contains("(base 16)"))
				{
					String[] separators = {"(base 16)"};
					String[] tokens = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

					if (tokens.Length == 2)
					{
						String mac = tokens[0].Trim();
						String company = tokens[1].Trim();

						String outLine = mac +" "+ company;

						lineCount++;
						streamWriter.WriteLine(outLine);
					}
					else
					{
						badLineCount++;
					}
				}

				line = streamReader.ReadLine();
			}

			streamReader.Close();
			streamWriter.Close();

			Console.WriteLine("Number of lines: {0}", lineCount);
			Console.WriteLine("Number of bad lines: {0}", badLineCount);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#endregion
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
}
