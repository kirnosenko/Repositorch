using System;

namespace Repositorch.Data
{
	/// <summary>
	/// Action on touched file.
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
}
