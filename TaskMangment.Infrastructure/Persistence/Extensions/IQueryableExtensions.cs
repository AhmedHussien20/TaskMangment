using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

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
                return query.OrderBy(e => EF.Property<object>(e, "Id"));
            }
        }
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchKey)
        {
            if (string.IsNullOrWhiteSpace(searchKey))
                return query;

            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string));

            if (!properties.Any())
                return query;

            Expression? finalExpression = null;
            var parameter = Expression.Parameter(typeof(T), "x");

            foreach (var property in properties)
            {
                var propertyAccess = Expression.Property(parameter, property);
                var searchValue = Expression.Constant(searchKey);

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                var containsExpression = Expression.Call(propertyAccess, containsMethod!, searchValue);

                finalExpression = finalExpression == null
                    ? containsExpression
                    : Expression.OrElse(finalExpression, containsExpression);
            }

            if (finalExpression == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, parameter);

            return query.Where(lambda);
        }
    }
}
