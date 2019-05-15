using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositorch.Data.VersionControl
{
	public interface IVcsData
	{
		string RevisionByNumber(int revisionNumber);
		string[] ParentRevisions(string revision);

		Log Log(string revision);
	}
}
