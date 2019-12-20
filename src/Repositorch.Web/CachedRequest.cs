using System;
using LiteDB;

namespace Repositorch.Web
{
	public class CachedRequest
	{
		[BsonId]
		public string Url { get; set; }
		public string Content { get; set; }
	}
}
