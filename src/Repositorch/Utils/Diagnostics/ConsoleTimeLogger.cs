namespace System.Diagnostics
{
	public class ConsoleTimeLogger : TimeLogger
	{
		public static ConsoleTimeLogger Start(string taskTitle)
		{
			return new ConsoleTimeLogger(taskTitle);
		}

		public ConsoleTimeLogger(string taskTitle)
			: base(
				taskTitle,
				x => Console.WriteLine(x.FormatedTime)
			)
		{
		}
	}
}
