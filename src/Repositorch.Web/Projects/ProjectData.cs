using System;
using System.IO;
using Newtonsoft.Json;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Projects
{
	public class ProjectData
	{
		public ProjectSettings Settings { get; set; }
		public Commit[] Commits { get; set; }
		public Tag[] Tags { get; set; }
		public Author[] Authors { get; set; }
		public Branch[] Branches { get; set; }
		public BugFix[] Fixes { get; set; }
		public CodeFile[] Files { get; set; }
		public Modification[] Modifications { get; set; }
		public CodeBlock[] Blocks { get; set; }

		public Stream ToStream()
		{
			var stream = new MemoryStream();
			
			var writer = new StreamWriter(stream);
			var jsonWriter = new JsonTextWriter(writer);
			var serializer = new JsonSerializer() { Formatting = Formatting.Indented };
			serializer.Serialize(jsonWriter, this);
			jsonWriter.Flush();
			stream.Seek(0, SeekOrigin.Begin);

			return stream;
		}

		public static ProjectData FromStream(Stream stream)
		{
			var reader = new StreamReader(stream);
			var jsonReader = new JsonTextReader(reader);
			var serializer = new JsonSerializer();

			return serializer.Deserialize<ProjectData>(jsonReader);
		}
	}
}
