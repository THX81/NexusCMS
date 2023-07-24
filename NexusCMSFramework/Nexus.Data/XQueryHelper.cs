namespace Nexus.Data
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    internal static class XQueryHelper
    {
        internal static String GetXQueryForProperty(Object obj, String propertyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetXQueryForProperty(" + obj + ", " + propertyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            
            String result = String.Empty;
            try
            {
                if (obj == null || obj == DBNull.Value)
                    throw new ArgumentNullException("obj");
                if (String.IsNullOrEmpty(propertyName))
                    throw new ArgumentNullException("propertyName");

                PropertyInfo prop = obj.GetType().GetProperty(propertyName);

                if (prop == null)
                    throw new Exception("Object '" + obj + "' dont have property '" + propertyName + "'!");


                result = "/" + (prop.DeclaringType.FullName + "." + prop.Name).Replace('.', '/');
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            
            return result;
        }
    }
}
