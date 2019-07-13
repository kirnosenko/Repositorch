using System;

namespace Repositorch.Data.VersionControl
{
	/// <summary>
	/// Touched file in commit.
	/// </summary>
	public class TouchedFile
	{
		/// <summary>
		/// Action on touched path.
		/// </summary>
		public enum TouchedFileAction
		{
			/// <summary>
			/// Modification of an existent file.
			/// </summary>
			MODIFIED,
			/// <summary>
			/// Addition of a new file.
			/// </summary>
			ADDED,
			/// <summary>
			/// Removing of an existent file.
			/// </summary>
			REMOVED
		}
		
		/// <summary>
		/// Path to the touched file.
		/// </summary>
		public string Path;
		/// <summary>
		/// Action on the touched file.
		/// </summary>
		public TouchedFileAction Action;
		/// <summary>
		/// The source path for a copied file.
		/// Null for a file created from scratch.
		/// </summary>
		public string SourcePath;
		/// <summary>
		/// The source revision for a copied file.
		/// Null for a file created from scratch.
		/// Null for a file copied from the previous revision.
		/// </summary>
		public string SourceRevision;
	}
}
