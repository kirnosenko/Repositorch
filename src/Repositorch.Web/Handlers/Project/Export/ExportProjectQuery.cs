using System.IO;
using MediatR;

namespace Repositorch.Web.Handlers.Project.Export
{
	public class ExportProjectQuery : IRequest
	{
		public string ProjectName { get; set; }
		public Stream Output { get; set; }
	}
}
