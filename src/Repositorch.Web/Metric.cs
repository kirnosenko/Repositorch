using System;
using Newtonsoft.Json.Linq;
using Repositorch.Data;

namespace Repositorch.Web
{
	public interface IMetric
	{
		object GetSettings(IRepository repository);
		object Calculate(IRepository repository, JObject input);
	}

	public abstract class Metric : IMetric
	{
		public virtual object GetSettings(IRepository repository)
		{
			return null;
		}
		public abstract object Calculate(IRepository repository, JObject input);
	}
}
