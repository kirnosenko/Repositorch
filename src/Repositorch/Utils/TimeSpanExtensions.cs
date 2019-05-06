namespace System
{
	public static class TimeSpanExtensions
	{
		public static string ToFormatedString(this TimeSpan span)
		{
			return
				span.Hours.ToString("D2") + ":" +
				span.Minutes.ToString("D2") + ":" +
				span.Seconds.ToString("D2") + ":" +
				span.Milliseconds.ToString("D3");
		}
	}
}
