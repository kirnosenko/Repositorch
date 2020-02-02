using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Metrics.Subfolder
{
	public class Submetric : IMetric
	{
		public object Calculate(IDataStore data, JObject input)
		{
			using (var s = data.OpenSession())
			{
				int commitsCount = s.GetReadOnly<Commit>().Count();
				
				return commitsCount;
			}
		}
	}
}
