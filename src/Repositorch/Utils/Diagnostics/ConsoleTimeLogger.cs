namespace System.Diagnostics
{
	public class ConsoleTimeLogger : TimeLogger
	{
		public static ConsoleTimeLogger Start(string taskTitle)
		{
			return new ConsoleTimeLogger(taskTitle);
		}

		public ConsoleTimeLogger(string taskTitle)
			: base(time => Console.WriteLine(string.Format(
				"{0}: {1}", taskTitle, time)))
		{
		}
	}
}
