using System.IO;
using MediatR;

namespace Repositorch.Web.Handlers.Project.Export
{
	public class ExportProjectQuery : IRequest, ICustomLogObject
	{
		public string ProjectName { get; set; }
		public Stream Output { get; set; }

		public object CustomLogObject => ProjectName;
	}
}
