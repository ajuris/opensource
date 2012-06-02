using System;
using System.Linq.Expressions;

namespace Geta.Community.EntityAttributeBuilder
{
    public class ExpressionHelper
    {
        public string ExtractMemberAccess<T, R>(Expression<Func<T, R>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            string propertyName = null;
            Expression expressionBody = expression.Body;
            if (expressionBody.NodeType == ExpressionType.MemberAccess)
            {
                propertyName = ((MemberExpression)expressionBody).Member.Name;
            }

            // if using PageType builders actual expression type to access PageType property will be substituted by Castle proxy
            if (expressionBody.NodeType == ExpressionType.Convert)
            {
                if (((UnaryExpression)expressionBody).Operand.NodeType == ExpressionType.MemberAccess)
                {
                    propertyName = ((MemberExpression)((UnaryExpression)expressionBody).Operand).Member.Name;
                }
            }

            return propertyName;
        }
    }
}
