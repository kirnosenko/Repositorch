using System;
using System.Collections.Generic;
using System.Linq;
using Repositorch.Data;
using Repositorch.Data.Entities;

namespace Repositorch
{
	class Program
	{
		static void Main(string[] args)
		{
			IDataStore data = new SqlCeDataStore("d:/123.sdf");
			using (var s = data.OpenSession())
			{
				s.Add(new Branch());
				s.SubmitChanges();
			}
			Console.ReadKey();
		}
	}
}
