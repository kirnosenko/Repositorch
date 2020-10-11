using System;

namespace Repositorch.Data.Entities
{
	/// <summary>
	/// Some additional information about a commit.
	/// May be anything with help of custom mappers.
	/// </summary>
	public class CommitAttribute
	{
		public const string TAG = nameof(TAG);
		public const string FIX = nameof(FIX);
		public const string MERGE = nameof(MERGE);
		public const string SPLIT = nameof(SPLIT);

		public int Id { get; set; }
		/// <summary>
		/// Attribute type.
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// Attribute data.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// Related commit.
		/// </summary>
		public int CommitNumber { get; set; }
		public Commit Commit { get; set; }
	}
}
