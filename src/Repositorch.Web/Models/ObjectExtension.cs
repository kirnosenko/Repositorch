﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Repositorch.Web.Models
{
	public static class ObjectExtension
	{
		public static ExpandoObject ToExpando(this object anonymousObject)
		{
			IDictionary<string, object> anonymousDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(anonymousObject);
			IDictionary<string, object> expando = new ExpandoObject();
			foreach (var item in anonymousDictionary)
				expando.Add(item);
			return (ExpandoObject)expando;
		}
	}
}
