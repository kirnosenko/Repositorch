using System;
using System.Collections.Generic;
using Repositorch.Data;
using Repositorch.Data.VersionControl;

namespace Repositorch.Web.Projects
{
	public interface IProjectManager
	{
		IEnumerable<string> GetProjectNames();
		ProjectSettings GetProject(string projectName);
		void AddProject(ProjectSettings projectSettings);
		void UpdateProject(ProjectSettings projectSettings);
		void RemoveProject(string projectName);
		IDataStore GetProjectDataStore(string projectName);
		IDataStore GetProjectDataStore(ProjectSettings projectSettings);
		IVcsData GetProjectVcsData(string projectName);
		IVcsData GetProjectVcsData(ProjectSettings projectSettings);
	}
}
