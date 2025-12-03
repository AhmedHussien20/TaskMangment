using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByDynamicSafe<T>(
            this IQueryable<T> query,
            string column,
            string direction)
        {
            if (string.IsNullOrWhiteSpace(column))
                return query;

            try
            {
                direction = direction?.ToUpper() ?? "ASC";

                return direction == "ASC"
                    ? query.OrderBy(e => EF.Property<object>(e, column))
                    : query.OrderByDescending(e => EF.Property<object>(e, column));
            }
            catch
            {
                // لو العمود غلط -> نرجع Id كـ default
                return query.OrderBy(e => EF.Property<object>(e, "Id"));
            }
        }
    }

}
