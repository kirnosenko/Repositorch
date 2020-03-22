using System;
using Newtonsoft.Json.Linq;
using Repositorch.Data;

namespace Repositorch.Web
{
	public interface IMetric
	{
		object GetDefaultSettings(IRepository repository);
		object GetFormData(IRepository repository);
		object Calculate(IRepository repository, JObject settings);
	}

	public abstract class Metric : IMetric
	{
		public virtual object GetDefaultSettings(IRepository repository)
		{
			return null;
		}
		public virtual object GetFormData(IRepository repository)
		{
			return null;
		}
		public abstract object Calculate(IRepository repository, JObject settings);
	}
}
