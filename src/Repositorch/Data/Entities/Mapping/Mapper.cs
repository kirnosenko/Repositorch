using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.VersionControl;

namespace Repositorch.Data.Entities.Mapping
{
	public abstract class Mapper<IME, OME>
		where IME : IRepositoryMappingExpression
		where OME : IRepositoryMappingExpression
	{
		protected IVcsData vcsData;

		public Mapper(IVcsData vcsData)
		{
			this.vcsData = vcsData;
		}
		public abstract IEnumerable<OME> Map(IME expression);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IEnumerable<OME> SingleExpression(OME expression)
		{
			return Enumerable.Repeat(expression, 1);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IEnumerable<OME> NoExpressions()
		{
			return Enumerable.Empty<OME>();
		}
	}
}
