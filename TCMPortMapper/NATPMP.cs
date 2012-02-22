using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TCMPortMapper
{
	class NATPMP
	{
		public const int RESPTYPE_PUBLICADDRESS  = 0;
		public const int RESPTYPE_UDPPORTMAPPING = 1;
		public const int RESPTYPE_TCPPORTMAPPING = 2;

		public const int PROTOCOL_UDP = 1;
		public const int PROTOCOL_TCP = 2;

		public const int ERR_INVALIDARGS        = -1;
		public const int ERR_SOCKETERROR        = -2;
		public const int ERR_CANNOTGETGATEWAY   = -3;
		public const int ERR_CLOSEERR           = -4;
		public const int ERR_RECVFROM           = -5;
		public const int ERR_NOPENDINGREQ       = -6;
		public const int ERR_NOGATEWAYSUPPORT   = -7;
		public const int ERR_CONNECTERR         = -8;
		public const int ERR_WRONGPACKETSOURCE  = -9;
		public const int ERR_SENDERR            = -10;
		public const int ERR_FCNTLERROR         = -11;
		public const int ERR_GETTIMEOFDAYERR    = -12;
		public const int ERR_UNSUPPORTEDVERSION = -14;
		public const int ERR_UNSUPPORTEDOPCODE  = -15;
		public const int ERR_UNDEFINEDERROR     = -49;
		public const int ERR_NOTAUTHORIZED      = -51;
		public const int ERR_NETWORKFAILURE     = -52;
		public const int ERR_OUTOFRESOURCES     = -53;
		public const int ERR_TRYAGAIN           = -100;

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct natpmp_t
		{
			public Int32 s;
			public UInt32 gateway;
			public Int32 has_pending_request;
			public fixed byte pending_request[12];
			public Int32 pending_request_len;
			public Int32 try_number;
			public Win32.TimeValue retry_time;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct natpmpresp_t
		{
			[FieldOffset(0)]
			public UInt16 type;

			[FieldOffset(2)]
			public UInt16 resultcode;

			[FieldOffset(4)]
			public UInt32 epoch;

			[StructLayout(LayoutKind.Sequential)]
			public struct publicaddress
			{
				public UInt32 addr;
			}

			[FieldOffset(8)]
			public publicaddress pnu_publicaddress;

			[StructLayout(LayoutKind.Sequential)]
			public struct newportmapping
			{
				public UInt16 privateport;
				public UInt16 mappedpublicport;
				public UInt32 lifetime;
			}

			[FieldOffset(8)]
			public newportmapping pnu_newportmapping;
		}

	//	[DllImport("natpmp.dll")]
	//	public static extern int getdefaultgateway([In, Out] ref UInt32 addr);

		[DllImport("natpmp.dll")]
		public static extern int initnatpmp([In, Out] ref natpmp_t p);

		[DllImport("natpmp.dll")]
		public static extern int closenatpmp([In, Out] ref natpmp_t p);

		[DllImport("natpmp.dll")]
		public static extern int sendpublicaddressrequest([In, Out] ref natpmp_t p);

		[DllImport("natpmp.dll")]
		public static extern int sendnewportmappingrequest([In, Out] ref natpmp_t p,
		                                                   [In] int protocol,
		                                                   [In] UInt16 privateport,
		                                                   [In] UInt16 publicport,
		                                                   [In] UInt32 lifetime);

		[DllImport("natpmp.dll")]
		public static extern int getnatpmprequesttimeout([In, Out] ref natpmp_t p,
		                                                 [In, Out] ref Win32.TimeValue timeout);

		[DllImport("natpmp.dll")]
		public static extern int readnatpmpresponseorretry([In, Out] ref natpmp_t p,
		                                                   [In, Out] ref natpmpresp_t response);

		[DllImport("natpmp.dll")]
		public static extern String strnatpmperr([In] int t);
	}
}
