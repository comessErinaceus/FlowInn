//using Microsoft.AspNetCore.Http.Features;
using System.Linq.Expressions;
//using System.Runtime.CompilerServices;

namespace FlowInn.API.Models
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> OrderByCustom<TEntity>(this IQueryable<TEntity> items, string sortBy, string sortOrder) 
        { 
            var type = typeof(TEntity);
            var expressoin2 = Expression.Parameter(type, "t");
            var property = type.GetProperty(sortBy);
            var expression1 = Expression.MakeMemberAccess(expressoin2, property);
            var lambda = Expression.Lambda(expression1, expressoin2);
            var result = Expression.Call(
                typeof(Queryable),
                sortOrder == "desc" ? "OrderByDescending" : "OrderBy",
                new Type[] { type, property.PropertyType },
                items.Expression,
                Expression.Quote(lambda));

            return items.Provider.CreateQuery<TEntity>(result);
        }
    }
}
