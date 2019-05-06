using System;
using System.IO;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLog : ILog
	{
		public GitLog(Stream log)
		{
			TextReader reader = new StreamReader(log);

			Revision = reader.ReadLine();
			Author = reader.ReadLine();
			Date = DateTime.Parse(reader.ReadLine()).ToUniversalTime();
			Message = reader.ReadLine();
		}

		public string Revision { get; protected set; }
		public string Author { get; protected set; }
		public DateTime Date { get; protected set; }
		public string Message { get; protected set; }
	}
}
