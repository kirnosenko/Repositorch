using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Repositorch.Data.Entities.Selection
{
	public class CommitSlice
	{
		private CommitSlice()
		{
		}

		public string Label { get; init; }
		public Expression<Func<Commit, bool>> Condition { get; init; }
		public Expression<Func<Commit, bool>> BeginCondition { get; init; }
		public Expression<Func<Commit, bool>> EndCondition { get; init; }

		public static CommitSlice Create(
			string label,
			Expression<Func<Commit, bool>> condition,
			Expression<Func<Commit, bool>> beginCondition,
			Expression<Func<Commit, bool>> endCondition)
		{
			return new CommitSlice()
			{
				Label = label,
				Condition = condition,
				BeginCondition = beginCondition,
				EndCondition = endCondition,
			};
		}
	}

	public static class SliceExtensions
	{
		public static CommitSlice[] GetDaySlices(this IRepository repository)
		{
			return repository.GetDateSlices(
				dateFrom => dateFrom.ToString("yyyy-MM-dd"),
				dateFrom =>
				{
					var nextDay = dateFrom.AddDays(1);
					return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
				});
		}

		public static CommitSlice[] GetWeekSlices(this IRepository repository, DayOfWeek firstWeekDay)
		{
			return repository.GetDateSlices(
				dateFrom =>
				{
					var year = dateFrom.Year.ToString();
					var week = CultureInfo.InvariantCulture.Calendar
						.GetWeekOfYear(dateFrom, CalendarWeekRule.FirstDay, firstWeekDay)
						.ToString("D2");
					return $"{year}-{week}";
				},
				dateFrom =>
				{
					var nextMonday = dateFrom;
					do
					{
						nextMonday = nextMonday.AddDays(1);
					} while (nextMonday.DayOfWeek != firstWeekDay && nextMonday.Year == dateFrom.Year);
					return nextMonday;
				});
		}

		public static CommitSlice[] GetMonthSlices(this IRepository repository)
		{
			return repository.GetDateSlices(
				dateFrom => dateFrom.ToString("yyyy-MM"),
				dateFrom =>
					dateFrom.Month < 12
						? new DateTime(dateFrom.Year, dateFrom.Month + 1, 1, 0, 0, 0)
						: new DateTime(dateFrom.Year + 1, 1, 1, 0, 0, 0));
		}

		public static CommitSlice[] GetYearSlices(this IRepository repository)
		{
			return repository.GetDateSlices(
				dateFrom => dateFrom.Year.ToString(),
				dateFrom => new DateTime(dateFrom.Year + 1, 1, 1, 0, 0, 0));
		}

		public static CommitSlice[] GetDateSlices(
			this IRepository repository,
			Func<DateTime, string> sliceLabel,
			Func<DateTime, DateTime> nextSliceDate)
		{
			var dateStart = repository.Get<Commit>().Min(x => x.Date);
			var dateStop = repository.Get<Commit>().Max(x => x.Date);

			List<CommitSlice> slices = new List<CommitSlice>();
			while (dateStart <= dateStop)
			{
				var dateFrom = dateStart;
				var dateTo = nextSliceDate(dateFrom);
				var slice = CommitSlice.Create(
					sliceLabel(dateFrom),
					c => c.Date >= dateFrom && c.Date < dateTo,
					c => c.Date >= dateFrom,
					c => c.Date < dateTo);
				slices.Add(slice);
				dateStart = dateTo;
			}

			return slices.ToArray();
		}

		public static CommitSlice[] GetTagSlices(this IRepository repository)
		{
			var tags = (
				from t in repository.Get<CommitAttribute>().Where(a => a.Type == CommitAttribute.TAG)
				join c in repository.Get<Commit>() on t.CommitNumber equals c.Number
				select new
				{
					tag = t.Data,
					number = c.Number
				}).OrderBy(x => x.number).ToArray();

			List<CommitSlice> slices = new List<CommitSlice>();
			int from = 0;
			foreach (var tag in tags)
			{
				var tagFrom = from;
				slices.Add(CommitSlice.Create(
					tag.tag,
					c => c.Number > tagFrom && c.Number <= tag.number,
					c => c.Number > tagFrom,
					c => c.Number <= tag.number
				));
				from = tag.number;
			}

			return slices.ToArray();
		}
	}
}
