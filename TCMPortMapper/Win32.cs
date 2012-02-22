using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace TCMPortMapper
{
	class Win32
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct FileDescriptorSet
		{
			//
			// how many are set?
			//
			public int Count;
			//
			// an array of Socket handles
			//
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=MaxCount)]
			public IntPtr[] Array;

			public static readonly int Size = Marshal.SizeOf(typeof(FileDescriptorSet));
			public static readonly FileDescriptorSet Empty = new FileDescriptorSet(0);
			public const int MaxCount = 64;

			public FileDescriptorSet(int count)
			{
				Count = count;
				Array = count == 0 ? null : new IntPtr[MaxCount];
			}
		}

		//
		// Structure used in select() call, taken from the BSD file sys/time.h.
		//
		[StructLayout(LayoutKind.Sequential)]
		public struct TimeValue
		{
			public int Seconds;  // seconds
			public int Microseconds; // and microseconds
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WSAData
		{
			public short wVersion;
			public short wHighVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=257)]
			public string szDescription;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=129)]
			public string szSystemStatus;
			public short iMaxSockets;
			public short iMaxUdpDg;
			public int lpVendorInfo;
		}

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
		public static extern SocketError WSAStartup([In] short wVersionRequested, [Out] out WSAData lpWSAData);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int WSACleanup();

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In, Out] ref FileDescriptorSet readfds,
		                                [In, Out] ref FileDescriptorSet writefds,
		                                [In, Out] ref FileDescriptorSet exceptfds,
		                                [In] ref TimeValue timeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In, Out] ref FileDescriptorSet readfds,
		                                [In, Out] ref FileDescriptorSet writefds,
		                                [In, Out] ref FileDescriptorSet exceptfds,
		                                [In] IntPtr nullTimeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In, Out] ref FileDescriptorSet readfds,
		                                [In] IntPtr ignoredA,
		                                [In] IntPtr ignoredB,
		                                [In] ref TimeValue timeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In, Out] ref FileDescriptorSet readfds,
		                                [In] IntPtr ignoredA,
		                                [In] IntPtr ignoredB,
		                                [In] IntPtr nullTimeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In] IntPtr ignoredA,
		                                [In, Out] ref FileDescriptorSet writefds,
		                                [In] IntPtr ignoredB,
		                                [In] ref TimeValue timeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In] IntPtr ignoredA,
		                                [In, Out] ref FileDescriptorSet writefds,
		                                [In] IntPtr ignoredB,
		                                [In] IntPtr nullTimeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In] IntPtr ignoredA,
		                                [In] IntPtr ignoredB,
		                                [In, Out] ref FileDescriptorSet exceptfds,
		                                [In] ref TimeValue timeout);

		[DllImport("wsock32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
		public static extern int select([In] int ignoredParameter,
		                                [In] IntPtr ignoredA,
		                                [In] IntPtr ignoredB,
		                                [In, Out] ref FileDescriptorSet exceptfds,
		                                [In] IntPtr nullTimeout);

		[DllImport("iphlpapi.dll")]
		public static extern Int32 SendARP([In] UInt32 destIpAddress,
		                                   [In] UInt32 srcIpAddress,
		                                   [In, Out] byte[] macAddress,
		                                   [In, Out] ref Int32 macAddressLength);
	}
}
