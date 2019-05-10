using System.IO;
using System.Text;

namespace System
{
	public static class StringExtension
	{
		public static Stream ToStream(this string text)
		{
			return new MemoryStream(new UTF8Encoding().GetBytes(text));
		}
	}
}
