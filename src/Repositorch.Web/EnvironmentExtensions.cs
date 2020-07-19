using System;

namespace Repositorch.Web
{
	public static class EnvironmentExtensions
	{
		public static string GetHomePath()
		{
			return Environment.GetEnvironmentVariable("REPOSITORCH_HOME_PATH") ?? GetOsHomePath();
		}

		public static string GetSrcPath()
		{
			return Environment.GetEnvironmentVariable("REPOSITORCH_SRC_PATH") ?? GetOsHomePath();
		}

		private static string GetOsHomePath()
		{
			var homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable("HOME")
				: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

			return homePath;
		}
	}
}
