using System;
using MediatR;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.Import
{
	public class ImportProjectCommand : IRequest, ICustomLogObject
	{
		public ProjectData Data { get; set; }

		public object CustomLogObject => Data.Settings;
	}
}
