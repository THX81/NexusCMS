using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Nexus.Data.Linq
{
    internal static class SqlMethodCallConverter
    {
        internal static MethodCallExpression TranslateMethodCall(MethodCallExpression e, ref StringBuilder sb)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (sb == null)
                throw new ArgumentNullException("sb");

            switch (e.Method.DeclaringType.ToString())
            {
                case "System.String":
                    TranslateStringMethod(e, ref sb);
                    return e;
                default:
                    throw new NotSupportedException(String.Format("The type '{0}' is not supported", e.Method.DeclaringType));
            }
        }

        private static void TranslateStringMethod(MethodCallExpression e, ref StringBuilder sb)
        {
            QueryTranslator tr = new QueryTranslator();
            switch (e.Method.Name)
            {
                case "Contains":
                    sb.Append("(" + tr.Translate(e.Object) + " LIKE '%" + tr.Translate(e.Arguments[0]).Replace("'", "") + "%')");
                    break;
                case "StartsWith":
                    sb.Append("(" + tr.Translate(e.Object) + " LIKE '" + tr.Translate(e.Arguments[0]).Replace("'", "") + "%')");
                    break;
                case "EndsWith":
                    sb.Append("(" + tr.Translate(e.Object) + " LIKE '%" + tr.Translate(e.Arguments[0]).Replace("'", "") + "')");
                    break;
                default:
                    throw new NotSupportedException(String.Format("The method '{0}' is not supported", e.Method.Name));
            }
        }
    }
}
