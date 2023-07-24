using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Nexus.Reflection
{
    /// <summary>
    /// Pomocn· t¯Ìda pro pr·ci s reflexÌ pomocÌ nÌû lze p¯istupovat k private a internal poloûk·m i v ciÌch assembly.
    /// </summary>
    internal class ReflectionHelper
    {
        /// <summary>
        /// Vyhled·v· metody v urËitÈm objektu.
        /// </summary>
        /// <param name="objectType">Datov˝ typ objektu.</param>
        /// <param name="name">N·zev metody.</param>
        /// <param name="paramsTypes">DatovÈ typy jednotliv˝ch argument˘.</param>
        /// <param name="flags">OmezenÌ vyhled·v·nÌ dle r˘zn˝ch vlastnostÌ.</param>
        /// <returns>Objekt obsahujÌcÌ pot¯ebnÈ informace a p¯Ìstup k metodÏ.</returns>
        internal static MethodInfo FindMethod(Type objectType, String name, Type[] paramsTypes, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindMethod(" + objectType + ", " + name + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetMethod(name, flags, null, paramsTypes, null);
        }
        /// <summary>
        /// Vyhled·v· vlastnosti v urËitÈm objektu.
        /// </summary>
        /// <param name="objectType">Datov˝ typ objektu.</param>
        /// <param name="name">N·zev vlastnosti.</param>
        /// <returns>Objekt obsahujÌcÌ pot¯ebnÈ informace a p¯Ìstup k vlastnosti.</returns>
        internal static PropertyInfo FindProperty(Type objectType, String name)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindProperty(" + objectType + ", " + name + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetProperty(name);
        }
        /// <summary>
        /// Vyhled·v· field v urËitÈm objektu.
        /// </summary>
        /// <param name="objectType">Datov˝ typ objektu.</param>
        /// <param name="name">N·zev fieldu.</param>
        /// <returns>Objekt obsahujÌcÌ pot¯ebnÈ informace a p¯Ìstup k fieldu.</returns>
        internal static FieldInfo FindField(Type objectType, String name)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindField(" + objectType + ", " + name + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetField(name);
        }

        /// <summary>
        /// PokusÌ se sestavit z p¯edanÈho pole objekt˘ pole jejich datov˝ch typ˘. Pokud je nÏjak· null tak se vyhodÌ vyjÌmka.
        /// </summary>
        /// <param name="parameters">Pole objekt˘.</param>
        /// <returns>Pole datov˝ch typ˘.</returns>
        internal static Type[] GetTypeArrFromObjectArr(Object[] parameters)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetTypeArrFromObjectArr(" + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (parameters == null) return new Type[0];

            Type[] returnArr = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                    throw new ArgumentNullException("Parameters can not be null. Parameter index " + i + ". Use another overloaded method.");

                returnArr[i] = parameters[i].GetType();
            }
            return returnArr;
        }

        /// <summary>
        /// Poskytuje monost naËÌst nÏjakou assembly dle jejÌcho celÈho n·zvu a v nÌ obsaûen˝ objekt. P¯es dalöÌ metody se n·slednÏ lze dostat k metod·m, vlastnostem a field˘m objektu.
        /// </summary>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <param name="fullTypeName">⁄pln˝ n·zev poûadovanÈho objektu.</param>
        /// <returns>Datov˝ typ poûadovanÈho objektu.</returns>
        internal static Type GetFrameworkTypeAssembly(String assemblyName, String fullTypeName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkTypeAssembly(" + assemblyName + ", " + fullTypeName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkTypeAssembly(AppDomain.CurrentDomain, assemblyName, fullTypeName);
        }
        /// <summary>
        /// Poskytuje monost naËÌst nÏjakou assembly dle jejÌcho celÈho n·zvu a v nÌ obsaûen˝ objekt. P¯es dalöÌ metody se n·slednÏ lze dostat k metod·m, vlastnostem a field˘m objektu.
        /// </summary>
        /// <param name="appDomain">AplikaËnÌ domÈna poûadovanÈ assembly.</param>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <param name="fullTypeName">⁄pln˝ n·zev poûadovanÈho objektu.</param>
        /// <returns>Datov˝ typ poûadovanÈho objektu.</returns>
        internal static Type GetFrameworkTypeAssembly(AppDomain appDomain, String assemblyName, String fullTypeName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkTypeAssembly(" + appDomain + ", " + assemblyName + ", " + fullTypeName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkAssembly(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(appDomain, assemblyName)).GetType(fullTypeName);
        }

        /// <summary>
        /// ZÌsk· objekt <c>Assembly</c> poûdovanÈho typu.
        /// </summary>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <returns>Objekt <c>Assembly</c> s obsaûen˝mi objekty.</returns>
        internal static Assembly GetFrameworkAssembly(String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssembly(" + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return Assembly.LoadFile(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(AppDomain.CurrentDomain, assemblyName));
        }
        /// <summary>
        /// ZÌsk· objekt <c>Assembly</c> poûdovanÈho typu.
        /// </summary>
        /// <param name="appDomain">AplikaËnÌ domÈna poûadovanÈ assembly.</param>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <returns>Objekt <c>Assembly</c> s obsaûen˝mi objekty.</returns>
        internal static Assembly GetFrameworkAssembly(AppDomain appDomain, String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssembly(" + appDomain + ", " + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return Assembly.LoadFile(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(appDomain, assemblyName));
        }

        /// <summary>
        /// ZÌsk· ˙pln˝ n·zev souboru poûadovanÈ assembly.
        /// </summary>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <returns>⁄pln˝ n·zev souboru poûadovanÈ assembly.</returns>
        internal static String GetFrameworkAssemblyPath(String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssemblyPath(" + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkAssemblyPath(AppDomain.CurrentDomain, assemblyName);
        }
        /// <summary>
        /// ZÌsk· ˙pln˝ n·zev souboru poûadovanÈ assembly.
        /// </summary>
        /// <param name="appDomain">AplikaËnÌ domÈna poûadovanÈ assembly.</param>
        /// <param name="assemblyName">N·zev assembly bez p¯Ìpony souboru.</param>
        /// <returns>⁄pln˝ n·zev souboru poûadovanÈ assembly.</returns>
        internal static String GetFrameworkAssemblyPath(AppDomain appDomain, String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssemblyPath(" + appDomain + ", " + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (!assemblyName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)) assemblyName += ".dll";

            Assembly[] a = appDomain.GetAssemblies();
            for (int i = 0; i < a.Length; i++)
                if (a[i].Location.EndsWith(assemblyName))
                    return a[i].Location;
            return assemblyName;
        }
    }

    /// <summary>
    /// Pomocn· t¯Ìda pro zjiöùov·nÌ informacÌ o datov˝ch typech.
    /// </summary>
    internal static class TypesHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static bool IsCollection(PropertyInfo propertyInfo)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("IsCollection(" + propertyInfo + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return IsCollection(propertyInfo.PropertyType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsCollection(Type type)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("IsCollection(" + type + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return type.GetInterface("ICollection") != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        internal static bool IsNonPrimitiveNonCollectionType(Type propertyType)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("IsNonPrimitiveNonCollectionType(" + propertyType + ")", System.Reflection.MethodBase.GetCurrentMethod());
            bool result = (!propertyType.IsPrimitive && !IsCollection(propertyType));
            switch (propertyType.FullName)
            {
                case "System.String":
                case "System.DateTime":
                case "System.Decimal":
                case "System.Double":
                case "System.Long":
                case "System.Guid":
                    return false;
            }

            return result;
        }
    }

    /// <summary>
    /// Pomocn· t¯Ìda pro vol·nÌ metod objekt˘ s b˝vrativiz hodnotou.
    /// </summary>
    /// <typeparam name="T">Objekt se kter˝m budou metody pracovat.</typeparam>
    public static class MethodInvokerWithReturn<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T InvokeAndReturn(Object reference, String methodName, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("InvokeAndReturn(" + reference + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Type[] paramsTypes = ReflectionHelper.GetTypeArrFromObjectArr(parameters);
            return InvokeAndReturn(reference, methodName, paramsTypes, parameters, flags);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="methodName"></param>
        /// <param name="paramsTypes"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T InvokeAndReturn(Object reference, String methodName, Type[] paramsTypes, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("InvokeAndReturn(" + reference + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            MethodInfo method = ReflectionHelper.FindMethod(reference.GetType(), methodName, paramsTypes, flags);
            if (method != null)
                returnObject = (T)method.Invoke(reference, parameters);

            return returnObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T InvokeAndReturn(Type objType, String methodName, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("InvokeAndReturn(" + objType + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Type[] paramsTypes = ReflectionHelper.GetTypeArrFromObjectArr(parameters);
            return InvokeAndReturn(objType, methodName, paramsTypes, parameters, flags);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="methodName"></param>
        /// <param name="paramsTypes"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T InvokeAndReturn(Type objType, String methodName, Type[] paramsTypes, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("InvokeAndReturn(" + objType + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            MethodInfo method = ReflectionHelper.FindMethod(objType, methodName, paramsTypes, flags);
            if (method != null)
                returnObject = (T)method.Invoke(null, parameters);

            return returnObject;
        }
    }

    /// <summary>
    /// Pomocn· t¯Ìda pro vol·nÌ metod bez n·vratovÈhÈ hodnoty.
    /// </summary>
    public static class MethodInvoker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        public static void Invoke(Object reference, String methodName, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Invoke(" + reference + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Type[] paramsTypes = ReflectionHelper.GetTypeArrFromObjectArr(parameters);
            Invoke(reference, methodName, paramsTypes, parameters, flags);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="methodName"></param>
        /// <param name="paramsTypes"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        public static void Invoke(Object reference, String methodName, Type[] paramsTypes, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Invoke(" + reference + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            MethodInfo method = ReflectionHelper.FindMethod(reference.GetType(), methodName, paramsTypes, flags);
            if (method != null)
                method.Invoke(reference, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        public static void Invoke(Type objType, String methodName, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Invoke(" + objType + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Type[] paramsTypes = ReflectionHelper.GetTypeArrFromObjectArr(parameters);
            Invoke(objType, methodName, paramsTypes, parameters, flags);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="methodName"></param>
        /// <param name="paramsTypes"></param>
        /// <param name="parameters"></param>
        /// <param name="flags"></param>
        public static void Invoke(Type objType, String methodName, Type[] paramsTypes, Object[] parameters, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Invoke(" + objType + ", " + methodName + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(parameters) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            MethodInfo method = ReflectionHelper.FindMethod(objType, methodName, paramsTypes, flags);
            if (method != null)
                method.Invoke(null, parameters);
        }
    }

    /// <summary>
    /// Pomocn· t¯Ìda pro vol·nÌ vlastnostÌ.
    /// </summary>
    /// <typeparam name="T">Objekt se kter˝m budou metody pracovat.</typeparam>
    public static class PropertyInvoker<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetValue(Object reference, String propertyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetValue(" + reference + ", " + propertyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            PropertyInfo property = ReflectionHelper.FindProperty(reference.GetType(), propertyName);
            if (property != null)
            {
                //if (property.CanRead) throw new Exception("Property \"" + reference.GetType() + "." + property.Name + "\" can not be readed.");
                returnObject = (T)property.GetValue(reference, null);
            }
            return returnObject;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetValue(Type objType, String propertyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetValue(" + objType + ", " + propertyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            PropertyInfo property = ReflectionHelper.FindProperty(objType, propertyName);
            if (property != null)
            {
                //if (property.CanRead) throw new Exception("Property \"" + objType + "." + property.Name + "\" can not be readed.");
                returnObject = (T)property.GetValue(objType, null);
            }
            return returnObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="propertyName"></param>
        /// <param name="data"></param>
        public static void SetValue(Object reference, String propertyName, T data)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SetValue(" + reference + ", " + propertyName + ", " + data + ")", System.Reflection.MethodBase.GetCurrentMethod());
            PropertyInfo property = ReflectionHelper.FindProperty(reference.GetType(), propertyName);
            if (property != null)
            {
                if (property.CanWrite) throw new Exception("Property \"" + reference.GetType() + "." + property.Name + "\" can not be writed.");
                property.SetValue(reference, data, null);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyName"></param>
        /// <param name="data"></param>
        public static void SetValue(Type objType, String propertyName, T data)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SetValue(" + objType + ", " + propertyName + ", " + data + ")", System.Reflection.MethodBase.GetCurrentMethod());
            PropertyInfo property = ReflectionHelper.FindProperty(objType, propertyName);
            if (property != null)
            {
                if (property.CanWrite) throw new Exception("Property \"" + objType + "." + property.Name + "\" can not be writed.");
                property.SetValue(objType, data, null);
            }
        }
    }

    /// <summary>
    /// Pomocn· t¯Ìda pro vol·nÌ field˘.
    /// </summary>
    /// <typeparam name="T">Objekt se kter˝m budou metody pracovat.</typeparam>
    public static class FieldInvoker<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetValue(Object reference, String fieldName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetValue(" + reference + ", " + fieldName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            FieldInfo field = ReflectionHelper.FindField(reference.GetType(), fieldName);
            if (field != null)
                returnObject = (T)field.GetValue(reference);
            return returnObject;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetValue(Type objType, String fieldName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetValue(" + objType + ", " + fieldName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            T returnObject = default(T);
            FieldInfo field = ReflectionHelper.FindField(objType, fieldName);
            if (field != null)
                returnObject = (T)field.GetValue(objType);
            return returnObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="fieldName"></param>
        /// <param name="data"></param>
        public static void SetValue(Object reference, String fieldName, T data)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SetValue(" + reference + ", " + fieldName + ", " + data + ")", System.Reflection.MethodBase.GetCurrentMethod());
            FieldInfo field = ReflectionHelper.FindField(reference.GetType(), fieldName);
            if (field != null)
                field.SetValue(reference, data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="fieldName"></param>
        /// <param name="data"></param>
        public static void SetValue(Type objType, String fieldName, T data)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SetValue(" + objType + ", " + fieldName + ", " + data + ")", System.Reflection.MethodBase.GetCurrentMethod());
            FieldInfo field = ReflectionHelper.FindField(objType, fieldName);
            if (field != null)
                field.SetValue(objType, data);
        }
    }
}
