using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace ChuntianCms.DAL
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)  
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first  
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression   
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        ///  Lambda表达式拼接，And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        /// <summary>
        /// Lambda表达式拼接，Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public static IOrderedQueryable<T> Ex_OrderBy<T>(this IQueryable<T> source, params KeyValuePair<string, bool>[] OrderByPropertyList) where T : class
        {
            if (OrderByPropertyList.Count() == 1) return source.Ex_OrderBy(OrderByPropertyList[0].Key, OrderByPropertyList[0].Value);
            if (OrderByPropertyList.Count() > 1)
            {
                var type = typeof(T);
                var param = Expression.Parameter(type, type.Name);
                Func<string, dynamic> KeySelectorFunc = _PropertyName =>
                {
                    return Expression.Lambda(Expression.Property(param, _PropertyName), param);
                };
                IOrderedQueryable<T> OrderedQueryable = OrderByPropertyList[0].Value
                    ? Queryable.OrderBy(source, KeySelectorFunc(OrderByPropertyList[0].Key))
                    : Queryable.OrderByDescending(source, KeySelectorFunc(OrderByPropertyList[0].Key));
                for (int i = 1; i < OrderByPropertyList.Length; i++)
                {
                    OrderedQueryable = OrderByPropertyList[i].Value
                        ? Queryable.ThenBy(OrderedQueryable, KeySelectorFunc(OrderByPropertyList[i].Key))
                        : Queryable.ThenByDescending(OrderedQueryable, KeySelectorFunc(OrderByPropertyList[i].Key));
                }
                return OrderedQueryable;
            }
            return null;
        }
        public static IOrderedQueryable<T> Ex_OrderBy<T>(this IQueryable<T> source, string OrderByPropertyName, bool IsOrderByAsc = true) where T : class
        {
            string command = IsOrderByAsc ? "OrderBy" : "OrderByDescending";
            var type = typeof(T);
            var property = type.GetProperty(OrderByPropertyName);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<T>(resultExpression) as IOrderedQueryable<T>;

        }
        public static IOrderedQueryable<T> Ex_ThenBy<T>(this IOrderedQueryable<T> OrderedQueryable, params KeyValuePair<string, bool>[] OrderByPropertyList) where T : class
        {
            if (OrderByPropertyList.Count() > 0)
            {
                var type = typeof(T);
                var param = Expression.Parameter(type, type.Name);
                Func<string, dynamic> KeySelectorFunc = _PropertyName =>
                {
                    return Expression.Lambda(Expression.Property(param, _PropertyName), param);
                };
                for (int i = 0; i < OrderByPropertyList.Length; i++)
                {
                    OrderedQueryable = OrderByPropertyList[i].Value
                        ? Queryable.ThenBy(OrderedQueryable, KeySelectorFunc(OrderByPropertyList[i].Key))
                        : Queryable.ThenByDescending(OrderedQueryable, KeySelectorFunc(OrderByPropertyList[i].Key));
                }
                return OrderedQueryable;
            }
            return null;
        }
        public static IOrderedQueryable<T> Ex_ThenBy<T>(this IOrderedQueryable<T> OrderedQueryable, string OrderByPropertyName, bool IsOrderByAsc = true) where T : class
        {
            var type = typeof(T);
           

            var param = Expression.Parameter(type, type.Name);
            var body = Expression.Property(param, OrderByPropertyName);
            dynamic KeySelector = Expression.Lambda(body, param);

            return IsOrderByAsc ? Queryable.ThenBy(OrderedQueryable, KeySelector) : Queryable.ThenByDescending(OrderedQueryable, KeySelector);
        }

    }
    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;
        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
    }
}
