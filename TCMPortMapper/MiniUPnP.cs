using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TCMPortMapper
{
	class MiniUPnP
	{
		public const int UPNPCOMMAND_SUCCESS = 0;
		public const int UPNPCOMMAND_UNKNOWN_ERROR = -1;
		public const int UPNPCOMMAND_INVALID_ARGS = -2;

		public const int MINIUPNPC_URL_MAXSIZE = 128;

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct UPNPDev
		{
			public IntPtr pNext;
			public String descURL;
			public String st;
			public fixed byte buffer[2];

			public UPNPDev Next
			{
				get { return (UPNPDev)Marshal.PtrToStructure(pNext, typeof(UPNPDev)); }
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct UPNPUrls
		{
			public String controlURL;
			public String ipcondescURL;
			public String controlURL_CIF;
		}

		/// <summary>
		/// This structure is used to avoid a SystemAccessViolation,
		/// and to prevent corruption of the process' memory.
		/// See issue #2 for further discussion.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct UPNPUrls_2
		{
			public char *controlURL;
			public char *ipcondescURL;
			public char *controlURL_CIF;
		}

		[StructLayout(LayoutKind.Sequential, Size = 1796, CharSet = CharSet.Ansi)]
		public struct IGDdatas
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String cureltname;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			public String urlbase;

			private Int32 level;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			public String controlurl_CIF;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String eventsuburl_CIF;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String scpdurl_CIF;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String servicetype_CIF;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			public String controlurl;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String eventsuburl;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			public String scpdurl;
			
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String serviceType;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String controlurl_tmp;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String eventsuburl_tmp;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String scpdurl_tmp;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MINIUPNPC_URL_MAXSIZE)]
			private String servicetype_tmp;

			public String ServiceType
			{
				get { return serviceType; }
				set { serviceType = value; }
			}
		}

		/// <summary>
		/// Discover UPnP devices on the network.
		/// The discovered devices are returned as a chained list.
		/// It is up to the caller to free the list with freeUPNPDevlist().
		/// If available, device list will be obtained from MiniSSDPd.
		/// </summary>
		/// <param name="delay">
		///		Delay (in milliseconds) is the maximum time for waiting any device response.
		/// </param>
		/// <param name="multicastif">
		///		If NULL, default multicast interface for sending SSDP discover packets will be used.
		/// </param>
		/// <param name="minissdpdsock">
		///		If null, default path for minissdpd socket will be used.
		/// </param>
		/// <returns>
		///		A pointer to a UPNPDev structure. Free this when done.
		///		Use the PtrToUPNPDev method to convert to a UPNPDev structure.
		/// </returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr upnpDiscover([In] int delay,
												 [In] String multicastif,
												 [In] String minissdpdsock);

		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr upnpDiscover([In] int delay,
												 [In] IntPtr multicastif,
												 [In] String minissdpdsock);

		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr upnpDiscover([In] int delay,
												 [In] String multicastif,
												 [In] IntPtr minissdpdsock);

		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr upnpDiscover([In] int delay,
												 [In] IntPtr multicastif,
												 [In] IntPtr minissdpdsock);

		/// <summary>
		/// frees list returned by upnpDiscover()
		/// </summary>
		/// <param name="devlistP">
		///		IntPtr (pointer to UPNPDev structure) as returned by upnpDiscover().
		/// </param>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void freeUPNPDevlist([In] IntPtr devlistP);

		/// <summary>
		/// Used when skipping the discovery process.
		/// Return value:
		/// 0 - Not OK
		/// 1 - OK
		/// </summary>
		/// <param name="rootDescUrl"></param>
		/// <param name="urls"></param>
		/// <param name="data"></param>
		/// <param name="lanAddr"></param>
		/// <param name="lanAddrLength"></param>
		/// <returns></returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern int UPNP_GetIGDFromUrl([In] String rootDescUrl,
		                                                   [In] UPNPUrls_2 *urls,
		                                                   [In, Out] ref IGDdatas datas,
		                                                   [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] lanAddr,
		                                                   [In] int lanAddrLength);

		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern void FreeUPNPUrls([In] UPNPUrls_2 *urls);

		/// <summary>
		/// Extracts the external IP address.
		/// </summary>
		/// <param name="controlURL"></param>
		/// <param name="serviceType"></param>
		/// <param name="externalIPAddr">
		///		The array to copy the external IP address bytes into.
		///		The array must be 16 bytes in length.
		/// </param>
		/// <returns>
		///		0: SUCCESS
		///		NON ZERO: ERROR Either a UPnP error code or an unknown error.
		/// 
		///		Possible UPnP Errors:
		///		402 Invalid Args - See UPnP Device Architecture section on Control.
		///		501 Action Failed - See UPnP Device Architecture section on Control.
		/// </returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int UPNP_GetExternalIPAddress([In] String controlURL,
														   [In] String serviceType,
														   [In, Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] externalIPAddr);

		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int UPNP_GetExternalIPAddress([In] String controlURL,
														   [In, MarshalAs(UnmanagedType.LPArray, SizeConst = MINIUPNPC_URL_MAXSIZE)] byte[] serviceType,
														   [In, Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] externalIPAddr);

		/// <summary>
		/// Description forthcoming...
		/// </summary>
		/// <param name="controlURL"></param>
		/// <param name="serviceType"></param>
		/// <param name="index"></param>
		/// <param name="extPort"></param>
		/// <param name="intClient"></param>
		/// <param name="intPort"></param>
		/// <param name="protocol"></param>
		/// <param name="desc"></param>
		/// <param name="enabled"></param>
		/// <param name="rHost"></param>
		/// <param name="duration"></param>
		/// <returns>
		///		UPNPCOMMAND_SUCCESS, UPNPCOMMAND_INVALID_ARGS, UPNPCOMMAND_UNKNOWN_ERROR or a UPnP Error Code.
		///		
		///		Possible UPNP Error codes:
		///		402 Invalid Args - See UPnP Device Architecture section on Control
		///		713 SpecifiedArrayIndexInvalid - The specified array index is out of bounds
		/// </returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int UPNP_GetGenericPortMappingEntry([In] String controlURL,
																 [In] String serviceType,
																 [In] [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] byte[] index,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] byte[] extPort,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] intClient,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] byte[] intPort,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] byte[] protocol,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 80)] byte[] desc,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] byte[] enabled,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 64)] byte[] rHost,
																 [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] duration);

		/// <summary>
		/// Adds a UPnP port mapping using the given information.
		/// </summary>
		/// <param name="controlURL"></param>
		/// <param name="serviceType"></param>
		/// <param name="externalPort"></param>
		/// <param name="internalPort"></param>
		/// <param name="internalClient"></param>
		/// <param name="description"></param>
		/// <param name="protocol"></param>
		/// <returns>
		///		Returns values:
		///		Zero: SUCCESS
		///		Non-Zero: ERROR. Either a UPnP error code or an unknown error.
		///		
		///		List of possible UPnP errors:
		///		402 - Invalid Args
		///		501 - Action Failed
		///		715 - WildCardNotPermittedInSrcIP
		///		716 - WildCardNotPermittedInExtPort
		///		718 - ConflictInMappingEntry
		///           The port mapping entry specified conflicts with a mapping assigned previously to another client
		///		724 - SamePortValueRequired
		///		      Internal and External port values must be the same
		///		725 - OnlyPermanentLeasesSupported
		///		      The NAT implementation only supports permanent lease times on port mappings
		///		726 - RemoteHostOnlySupportsWildcard
		///		      RemoteHost must be a wildcard and cannot be a specific IP address or DNS name
		///		727 - ExternalPortOnlySupportsWildcard
		///		      ExternalPort must be a wildcard and cannot be a specific port value
		/// </returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int UPNP_AddPortMapping([In] String controlURL,
													 [In] String serviceType,
													 [In] String externalPort,
													 [In] String internalPort,
													 [In] String internalClient,
													 [In] String description,
													 [In] String protocol);

		/// <summary>
		/// Deletes a UPnP port mapping with the given information.
		/// </summary>
		/// <param name="controlURL"></param>
		/// <param name="serviceType"></param>
		/// <param name="externalPort"></param>
		/// <param name="protocol"></param>
		/// <returns>
		///		Return values:
		///		Zero: SUCCESS
		///		Non-Zero: ERROR. Either a UPnP error code or an undefined error.
		///		
		///		List of possible UPnP errors:
		///		402 - Invalid Args
		///		714 - NoSuchEntryInArray (Port Mapping doesn't exist)
		/// </returns>
		[DllImport("miniupnp.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int UPNP_DeletePortMapping([In] String controlURL,
														[In] String serviceType,
														[In] String externalPort,
														[In] String protocol);


		#region Utility Methods

		public static UPNPDev PtrToUPNPDev(IntPtr devlistP)
		{
			return (UPNPDev)Marshal.PtrToStructure(devlistP, typeof(UPNPDev));
		}

		public static String NullTerminatedArrayToString(byte[] nullTerminatedStr)
		{
			for (int i = 0; i < nullTerminatedStr.Length; i++)
			{
				if (nullTerminatedStr[i] == 0)
				{
					return System.Text.Encoding.ASCII.GetString(nullTerminatedStr, 0, i);
				}
			}

			return System.Text.Encoding.ASCII.GetString(nullTerminatedStr, 0, nullTerminatedStr.Length);
		}

		#endregion
	}
}
