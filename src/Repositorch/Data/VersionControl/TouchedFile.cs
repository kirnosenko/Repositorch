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
			/// Addition of a new file.
			/// </summary>
			ADDED,
			/// <summary>
			/// Modification of an existent file.
			/// </summary>
			MODIFIED,
			/// <summary>
			/// Removing of an existent file.
			/// </summary>
			DELETED
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
	}
}
