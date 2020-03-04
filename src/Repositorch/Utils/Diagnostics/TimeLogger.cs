namespace System.Diagnostics
{
	public class TimeLogger : IDisposable
	{
		public static TimeLogger Start(Action<string> finalAction = null)
		{
			return new TimeLogger(finalAction);
		}

		private Stopwatch timer;
		private Action<string> finalAction;

		public TimeLogger(Action<string> finalAction)
		{
			timer = Stopwatch.StartNew();
			this.finalAction = finalAction;
		}
		public void Dispose()
		{
			timer.Stop();
			if (finalAction != null)
			{
				finalAction(FormatedTime);
			}
		}
		public string FormatedTime
		{
			get
			{
				return timer.Elapsed.ToFormatedString();
			}
		}
	}
}
