using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Nexus.Reflection
{
    /// <summary>
    /// Pomocn� t��da pro pr�ci s reflex� pomoc� n� lze p�istupovat k private a internal polo�k�m i v ci�ch assembly.
    /// </summary>
    internal class ReflectionHelper
    {
        /// <summary>
        /// Vyhled�v� metody v ur�it�m objektu.
        /// </summary>
        /// <param name="objectType">Datov� typ objektu.</param>
        /// <param name="name">N�zev metody.</param>
        /// <param name="paramsTypes">Datov� typy jednotliv�ch argument�.</param>
        /// <param name="flags">Omezen� vyhled�v�n� dle r�zn�ch vlastnost�.</param>
        /// <returns>Objekt obsahuj�c� pot�ebn� informace a p��stup k metod�.</returns>
        internal static MethodInfo FindMethod(Type objectType, String name, Type[] paramsTypes, BindingFlags flags)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindMethod(" + objectType + ", " + name + ", " + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(paramsTypes) + ", " + flags + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetMethod(name, flags, null, paramsTypes, null);
        }
        /// <summary>
        /// Vyhled�v� vlastnosti v ur�it�m objektu.
        /// </summary>
        /// <param name="objectType">Datov� typ objektu.</param>
        /// <param name="name">N�zev vlastnosti.</param>
        /// <returns>Objekt obsahuj�c� pot�ebn� informace a p��stup k vlastnosti.</returns>
        internal static PropertyInfo FindProperty(Type objectType, String name)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindProperty(" + objectType + ", " + name + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetProperty(name);
        }
        /// <summary>
        /// Vyhled�v� field v ur�it�m objektu.
        /// </summary>
        /// <param name="objectType">Datov� typ objektu.</param>
        /// <param name="name">N�zev fieldu.</param>
        /// <returns>Objekt obsahuj�c� pot�ebn� informace a p��stup k fieldu.</returns>
        internal static FieldInfo FindField(Type objectType, String name)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("FindField(" + objectType + ", " + name + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return objectType.GetField(name);
        }

        /// <summary>
        /// Pokus� se sestavit z p�edan�ho pole objekt� pole jejich datov�ch typ�. Pokud je n�jak� null tak se vyhod� vyj�mka.
        /// </summary>
        /// <param name="parameters">Pole objekt�.</param>
        /// <returns>Pole datov�ch typ�.</returns>
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
        /// Poskytuje monost na��st n�jakou assembly dle jej�cho cel�ho n�zvu a v n� obsa�en� objekt. P�es dal�� metody se n�sledn� lze dostat k metod�m, vlastnostem a field�m objektu.
        /// </summary>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <param name="fullTypeName">�pln� n�zev po�adovan�ho objektu.</param>
        /// <returns>Datov� typ po�adovan�ho objektu.</returns>
        internal static Type GetFrameworkTypeAssembly(String assemblyName, String fullTypeName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkTypeAssembly(" + assemblyName + ", " + fullTypeName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkTypeAssembly(AppDomain.CurrentDomain, assemblyName, fullTypeName);
        }
        /// <summary>
        /// Poskytuje monost na��st n�jakou assembly dle jej�cho cel�ho n�zvu a v n� obsa�en� objekt. P�es dal�� metody se n�sledn� lze dostat k metod�m, vlastnostem a field�m objektu.
        /// </summary>
        /// <param name="appDomain">Aplika�n� dom�na po�adovan� assembly.</param>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <param name="fullTypeName">�pln� n�zev po�adovan�ho objektu.</param>
        /// <returns>Datov� typ po�adovan�ho objektu.</returns>
        internal static Type GetFrameworkTypeAssembly(AppDomain appDomain, String assemblyName, String fullTypeName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkTypeAssembly(" + appDomain + ", " + assemblyName + ", " + fullTypeName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkAssembly(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(appDomain, assemblyName)).GetType(fullTypeName);
        }

        /// <summary>
        /// Z�sk� objekt <c>Assembly</c> po�dovan�ho typu.
        /// </summary>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <returns>Objekt <c>Assembly</c> s obsa�en�mi objekty.</returns>
        internal static Assembly GetFrameworkAssembly(String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssembly(" + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return Assembly.LoadFile(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(AppDomain.CurrentDomain, assemblyName));
        }
        /// <summary>
        /// Z�sk� objekt <c>Assembly</c> po�dovan�ho typu.
        /// </summary>
        /// <param name="appDomain">Aplika�n� dom�na po�adovan� assembly.</param>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <returns>Objekt <c>Assembly</c> s obsa�en�mi objekty.</returns>
        internal static Assembly GetFrameworkAssembly(AppDomain appDomain, String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssembly(" + appDomain + ", " + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return Assembly.LoadFile(Nexus.Reflection.ReflectionHelper.GetFrameworkAssemblyPath(appDomain, assemblyName));
        }

        /// <summary>
        /// Z�sk� �pln� n�zev souboru po�adovan� assembly.
        /// </summary>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <returns>�pln� n�zev souboru po�adovan� assembly.</returns>
        internal static String GetFrameworkAssemblyPath(String assemblyName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetFrameworkAssemblyPath(" + assemblyName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetFrameworkAssemblyPath(AppDomain.CurrentDomain, assemblyName);
        }
        /// <summary>
        /// Z�sk� �pln� n�zev souboru po�adovan� assembly.
        /// </summary>
        /// <param name="appDomain">Aplika�n� dom�na po�adovan� assembly.</param>
        /// <param name="assemblyName">N�zev assembly bez p��pony souboru.</param>
        /// <returns>�pln� n�zev souboru po�adovan� assembly.</returns>
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
    /// Pomocn� t��da pro zji��ov�n� informac� o datov�ch typech.
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
    /// Pomocn� t��da pro vol�n� metod objekt� s b�vrativiz hodnotou.
    /// </summary>
    /// <typeparam name="T">Objekt se kter�m budou metody pracovat.</typeparam>
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
    /// Pomocn� t��da pro vol�n� metod bez n�vratov�h� hodnoty.
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
    /// Pomocn� t��da pro vol�n� vlastnost�.
    /// </summary>
    /// <typeparam name="T">Objekt se kter�m budou metody pracovat.</typeparam>
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
    /// Pomocn� t��da pro vol�n� field�.
    /// </summary>
    /// <typeparam name="T">Objekt se kter�m budou metody pracovat.</typeparam>
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
