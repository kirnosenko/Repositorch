using System;
using System.IO;
using MediatR;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.Import
{
	public class ImportProjectCommand : IRequest
	{
		public ProjectData Data { get; set; }
	}
}
