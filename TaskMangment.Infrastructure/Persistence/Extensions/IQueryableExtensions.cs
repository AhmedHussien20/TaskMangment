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

            var property = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.Equals(column, StringComparison.OrdinalIgnoreCase));

            var propertyName = property?.Name ?? "Id";
            direction = direction?.ToUpper() ?? "ASC";

            return direction == "ASC"
                ? query.OrderBy(e => EF.Property<object>(e, propertyName))
                : query.OrderByDescending(e => EF.Property<object>(e, propertyName));
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchKey)
        {
            if (string.IsNullOrWhiteSpace(searchKey))
                return query;

            searchKey = searchKey.Trim();

            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSearchableProperty(p))
                .ToList();

            if (!properties.Any())
                return query;

            Expression? finalExpression = null;
            var parameter = Expression.Parameter(typeof(T), "x");
            var searchValue = Expression.Constant(searchKey);
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            foreach (var property in properties)
            {
                var propertyAccess = Expression.Property(parameter, property);
                Expression? condition = null;

                if (property.PropertyType == typeof(string))
                {
                    condition = Expression.Call(propertyAccess, containsMethod, searchValue);
                }
                else
                {
                    // Id (and other numeric key fields): convert to string then Contains
                    condition = BuildNumericContainsExpression(propertyAccess, property.PropertyType, containsMethod, searchValue);
                }

                if (condition == null)
                    continue;

                finalExpression = finalExpression == null
                    ? condition
                    : Expression.OrElse(finalExpression, condition);
            }

            if (finalExpression == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
            return query.Where(lambda);
        }

        private static bool IsSearchableProperty(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
                return true;

            // Allow searching by primary Id (int/long/guid)
            if (!property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                return false;

            var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            return underlying == typeof(int)
                || underlying == typeof(long)
                || underlying == typeof(short)
                || underlying == typeof(byte)
                || underlying == typeof(Guid);
        }

        private static Expression? BuildNumericContainsExpression(
            Expression propertyAccess,
            Type propertyType,
            MethodInfo containsMethod,
            ConstantExpression searchValue)
        {
            var underlying = Nullable.GetUnderlyingType(propertyType);

            if (underlying != null)
            {
                var hasValue = Expression.Property(propertyAccess, nameof(Nullable<int>.HasValue));
                var value = Expression.Property(propertyAccess, nameof(Nullable<int>.Value));
                var toString = Expression.Call(value, underlying.GetMethod(nameof(ToString), Type.EmptyTypes)!);
                var contains = Expression.Call(toString, containsMethod, searchValue);
                return Expression.AndAlso(hasValue, contains);
            }

            var toStringNonNull = Expression.Call(
                propertyAccess,
                propertyType.GetMethod(nameof(ToString), Type.EmptyTypes)!);
            return Expression.Call(toStringNonNull, containsMethod, searchValue);
        }
    }
}
