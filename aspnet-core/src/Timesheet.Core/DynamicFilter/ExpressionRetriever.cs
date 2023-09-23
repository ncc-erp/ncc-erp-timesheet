using Timesheet.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Timesheet.DynamicFilter
{
    public static class ExpressionRetriever
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", 0, new Type[] { typeof(string) });
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        public static Expression GetExpression<T>(ParameterExpression param, ExpressionFilter filter, Type propertyType)
        {
            MemberExpression member = Expression.Property(param, filter.ActualPropertyName);
            ConstantExpression constant = Expression.Constant(filter.ActualValue, propertyType);
            string s = string.Empty;
            switch (filter.Comparison)
            {
                case ComparisonOperator.Equal:
                    return Expression.Equal(member, constant);
                case ComparisonOperator.GreaterThan:
                    return Expression.GreaterThan(member, constant);
                case ComparisonOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);
                case ComparisonOperator.LessThan:
                    return Expression.LessThan(member, constant);
                case ComparisonOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
                case ComparisonOperator.NotEqual:
                    return Expression.NotEqual(member, constant);
                case ComparisonOperator.Contains:
                    return Expression.Call(member, containsMethod, constant);
                case ComparisonOperator.StartsWith:
                    return Expression.Call(member, startsWithMethod, constant);
                case ComparisonOperator.EndsWith:
                    return Expression.Call(member, endsWithMethod, constant);
                case ComparisonOperator.In:
                    return constant.ListContains(member);
                default:
                    return null;
            }
        }
    }
}
