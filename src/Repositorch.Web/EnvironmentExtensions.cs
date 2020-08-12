using System;
using System.Collections.Generic;
using System.IO;

namespace Repositorch.Web
{
	public static class EnvironmentExtensions
	{
		public static Dictionary<string, string> GetVariables()
		{
			return new Dictionary<string, string>()
			{
				{ "HOME", GetHomePath() },
				{ "REPO", GetRepoPath() },
				{ "DB", GetDbPath() }
			};
		}

		public static string GetHomePath()
		{
			return GetEnvHomePath() ?? GetOsHomePath();
		}

		public static string GetDbPath()
		{
			var fileName = GetEnvDbFile() ?? "repositorch.db";
			return Path.Combine(GetHomePath(), fileName);
		}

		public static string GetRepoPath()
		{
			return GetEnvRepoPath() ?? GetOsHomePath();
		}

		private static string GetEnvHomePath()
		{
			return GetEnvPath("REPOSITORCH_HOME_PATH");
		}

		private static string GetEnvRepoPath()
		{
			return GetEnvPath("REPOSITORCH_REPO_PATH");
		}

		private static string GetEnvPath(string name)
		{
			var envPath = Environment.GetEnvironmentVariable(name);
			if (envPath != null && !Directory.Exists(envPath))
			{
				envPath = null;
			}
			return envPath;
		}

		private static string GetEnvDbFile()
		{
			return Environment.GetEnvironmentVariable("REPOSITORCH_DB_FILE");
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
