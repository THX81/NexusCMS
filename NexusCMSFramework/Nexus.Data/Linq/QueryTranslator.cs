using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Nexus.Data.Linq
{
    internal class QueryTranslator : ExpressionVisitor
    {
        StringBuilder sb;

        internal QueryTranslator()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("QueryTranslator()", System.Reflection.MethodBase.GetCurrentMethod());
        }


        internal string Translate(Expression expression)
        {
            
            Nexus.Diagnostics.Log4NetWrapper.Info("Translate(" + expression + ")", System.Reflection.MethodBase.GetCurrentMethod());
            this.sb = new StringBuilder();
            expression = Evaluator.PartialEval(expression);
            this.Visit(expression);
            return this.sb.ToString();
        }


        private static Expression StripQuotes(Expression e)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("StripQuotes(" + e + ")", System.Reflection.MethodBase.GetCurrentMethod());
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }


        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("VisitMethodCall(" + m + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                sb.Append("SELECT * FROM (");
                this.Visit(m.Arguments[0]);
                sb.Append(") AS T WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            else
            {
                return SqlMethodCallConverter.TranslateMethodCall(m, ref sb);
            }
        }


        protected override Expression VisitUnary(UnaryExpression u)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("VisitUnary(" + u + ")", System.Reflection.MethodBase.GetCurrentMethod());
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }


        protected override Expression VisitBinary(BinaryExpression b)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("VisitBinary(" + b + ")", System.Reflection.MethodBase.GetCurrentMethod());
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }


        protected override Expression VisitConstant(ConstantExpression c)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("VisitConstant(" + c + ")", System.Reflection.MethodBase.GetCurrentMethod());
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                sb.Append("SELECT * FROM ");
                sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }


        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("VisitMemberAccess(" + m + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
