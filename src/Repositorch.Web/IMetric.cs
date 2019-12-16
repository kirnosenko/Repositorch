using System;
using Newtonsoft.Json.Linq;
using Repositorch.Data;

namespace Repositorch.Web
{
	public interface IMetric
	{
		object Calculate(IDataStore data, JObject input);
	}
}
