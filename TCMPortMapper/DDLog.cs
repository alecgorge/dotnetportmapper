using System;
using System.Diagnostics;
using System.IO;

namespace TCMPortMapper
{
	public abstract class OutputLog
	{
		protected const String F = "yyyy-MM-dd HH:mm:ss:fff";

		protected static String Timestamp()
		{
			return DateTime.Now.ToString(F);
		}
	}

	public class DebugLog : OutputLog
	{
		[Conditional("DEBUG")]
		public static void Write(String format, params Object[] args)
		{
			String partMessage = String.Format(format, args);
			String fullMessage = String.Format("{0}:  {1}", Timestamp(), partMessage);
			Debug.Write(fullMessage);
		}

		[Conditional("DEBUG")]
		public static void WriteIf(bool flag, String format, params Object[] args)
		{
			if (flag)
			{
				String partMessage = String.Format(format, args);
				String fullMessage = String.Format("{0}:  {1}", Timestamp(), partMessage);
				Debug.Write(fullMessage);
			}
		}

		[Conditional("DEBUG")]
		public static void WriteLine(String format, params Object[] args)
		{
			String partMessage = String.Format(format, args);
			String fullMessage = String.Format("{0}:  {1}", Timestamp(), partMessage);
			Debug.WriteLine(fullMessage);
		}

		[Conditional("DEBUG")]
		public static void WriteLineIf(bool flag, String format, params Object[] args)
		{
			if (flag)
			{
				String partMessage = String.Format(format, args);
				String fullMessage = String.Format("{0}:  {1}", Timestamp(), partMessage);
				Debug.WriteLine(fullMessage);
			}
		}
	}
}
