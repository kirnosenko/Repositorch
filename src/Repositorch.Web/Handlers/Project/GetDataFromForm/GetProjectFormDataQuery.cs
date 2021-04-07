﻿using System;
using System.IO;
using MediatR;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.GetDataFromForm
{
	public class GetProjectDataFromFormQuery : IRequest<ProjectData>, ICustomLogObject
	{
		public ProjectSettings Settings { get; set; }
		public Stream File { get; set; }

		public object CustomLogObject => Settings;
	}
}
