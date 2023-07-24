namespace Nexus.Data.AdaptiveDAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Nexus.Data;
    using Nexus.Reflection;

    /// <summary>
    /// Pomocná třída pro vytváření a modifikaci databázových tabulek.
    /// </summary>
    public static class BusinessObjectsSqlStorageManager // internal
    {

        /// <summary>
        /// Zajistí že správnou tabulku pro požadovaný objekt.
        /// </summary>
        /// <param name="type">Typ objektu pro který se má zajistit úložiště.</param>
        internal static void EnsureTableForObject(Type type)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("EnsureTableForObject(" + type + ")", System.Reflection.MethodBase.GetCurrentMethod());

            if (!TableExists(type))
            {
                CreateTableForObject(type);
            }
            else
            {
                UpdateTable(type);
            }
        }

        /// <summary>
        /// Vytvoří novou tabulku v databázi se odpovídající strukturou předaného typu.
        /// </summary>
        /// <param name="type">Typ objektu pro který se má vytvořit tabulka.</param>
        public static void CreateTableForObject(Type type) // internal
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("CreateTableForObject(" + type + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (type == null)
                throw new ArgumentNullException("type");
            String fullName = type.FullName;
            if (type.BaseType.FullName != typeof(BusinessObjectBase).FullName)
                throw new ArgumentException("Type must derive from base class BusinessObjectBase!");

            StringBuilder sbTable = new StringBuilder(256);
            sbTable.Append("CREATE TABLE [" + fullName + "](" + Environment.NewLine);
            sbTable.Append(" [ObjectId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_" + fullName + "_ObjectId]  DEFAULT (newid())," + Environment.NewLine);
            foreach (FieldInfo field in type.GetFields(BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                if (field.Name != "objectId" && field.Name != "createDate" && field.Name != "modifyDate")
                {
                    SqlTypeInfo info = SqlDataTypesHelper.GetSqlTypeInfo(field.FieldType, null, null);
                    sbTable.Append(" [" + field.Name + "] " + GetSqlTypeDefinition(info) + "," + Environment.NewLine);
                }
            }
            sbTable.Append(" [CreateDate] [datetime] NULL," + Environment.NewLine);
            sbTable.Append(" [ModifyDate] [datetime] NULL CONSTRAINT [DF_" + fullName + "_ModifyDate]  DEFAULT (getutcdate())," + Environment.NewLine);
            sbTable.Append(" CONSTRAINT [PK_" + fullName + "] PRIMARY KEY CLUSTERED ([ObjectId] ASC)" + Environment.NewLine);
            sbTable.Append(") ON [PRIMARY]" + Environment.NewLine);

            StringBuilder sbIndexes = new StringBuilder(256);
            sbIndexes.Append("CREATE NONCLUSTERED INDEX [idx_CreateDate] ON [" + fullName + "] ([CreateDate] ASC);" + Environment.NewLine);
            sbIndexes.Append("CREATE NONCLUSTERED INDEX [idx_ModifyDate] ON [" + fullName + "] ([ModifyDate] ASC);" + Environment.NewLine);
            foreach (FieldInfo field in type.GetFields(BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                SqlTypeInfo info = SqlDataTypesHelper.GetSqlTypeInfo(field.FieldType, null, null);
                if (field.Name != "objectId" && field.Name != "createDate" && field.Name != "modifyDate" && info.IsIndexable)
                    sbIndexes.Append(GetQueryCreateIndex(fullName, field.Name, info) + Environment.NewLine);
            }

            StringBuilder sbTrigger1 = new StringBuilder(256);
            sbTrigger1.Append("CREATE TRIGGER [" + fullName + "UpdateModifyDate] ON  [" + fullName + "] AFTER UPDATE" + Environment.NewLine);
            sbTrigger1.Append("AS " + Environment.NewLine);
            sbTrigger1.Append("BEGIN" + Environment.NewLine);
            sbTrigger1.Append("    SET NOCOUNT ON;" + Environment.NewLine);
            sbTrigger1.Append("    update [" + fullName + "] set ModifyDate=getutcdate() where ObjectId=(select ObjectId from Inserted);" + Environment.NewLine);
            sbTrigger1.Append("END" + Environment.NewLine);

            StringBuilder sbTrigger2 = new StringBuilder(256);
            sbTrigger2.Append("CREATE TRIGGER [" + fullName + "InsertCreateDateModifyDate] ON  [" + fullName + "] AFTER INSERT" + Environment.NewLine);
            sbTrigger2.Append("AS " + Environment.NewLine);
            sbTrigger2.Append("BEGIN" + Environment.NewLine);
            sbTrigger2.Append("    SET NOCOUNT ON;" + Environment.NewLine);
            sbTrigger2.Append("    update [" + fullName + "] set CreateDate=getutcdate(), ModifyDate=getutcdate() where ObjectId=(select ObjectId from Inserted);" + Environment.NewLine);
            sbTrigger2.Append("END" + Environment.NewLine);
            
            SqlCommander cmd = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            try
            {
                //System.Web.HttpContext.Current.Response.Write(sbTable.ToString() + Environment.NewLine);
                //System.Web.HttpContext.Current.Response.Write(sbIndexes.ToString() + Environment.NewLine);
                //System.Web.HttpContext.Current.Response.Write(sbTrigger1.ToString() + Environment.NewLine);
                //System.Web.HttpContext.Current.Response.Write(sbTrigger2.ToString() + Environment.NewLine);

                cmd.ExecuteNonQuery(sbTable.ToString());
                cmd.ExecuteNonQuery(sbIndexes.ToString());
                cmd.ExecuteNonQuery(sbTrigger1.ToString());
                cmd.ExecuteNonQuery(sbTrigger2.ToString());
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private static void UpdateTable(Type type)
        {
            DataTable schemaTbl = new DataTable(type.FullName);
            SqlCommander cmd = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            try
            {
                schemaTbl = cmd.ExecuteDataTable("select top 0 * from [" + type.FullName + "]");
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                if (schemaTbl != null)
                    schemaTbl.Dispose();
            }


            foreach (PropertyInfo prop in type.GetProperties())
            {
                bool exists = false;
                Type columnType = null;
                foreach (DataColumn col in schemaTbl.Columns)
                {
                    if (col.ColumnName == prop.Name)
                    {
                        exists = true;
                        columnType = col.DataType;
                    }
                }

                if (!TypesHelper.IsCollection(prop.PropertyType) && !TypesHelper.IsNonPrimitiveNonCollectionType(prop.PropertyType))
                {
                    if (!exists)
                    {
                        AddColumnToTable(prop);
                    }
                    else
                    {
                        if (columnType != prop.PropertyType)
                        {
                            //System.Web.HttpContext.Current.Response.Write("(" + columnType + " != " + prop.PropertyType + ")<br>");
                            ModifyColumnInTable(prop);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Zjistí zda existuje tabulka v databázi.
        /// </summary>
        /// <param name="type">Typ objektu pro který se má zjistit existence tabulky.</param>
        /// <returns>Pokud existuje vrací se <c>true</c> jinak <c>false</c>.</returns>
        internal static bool TableExists(Type type)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TableExists(" + type + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.BaseType.FullName != typeof(BusinessObjectBase).FullName)
                throw new ArgumentException("Type must derive from base class \"BusinessObjectBase\"!");
            return TableExists(type.FullName);
        }

        /// <summary>
        /// Zjistí zda existuje tabulka v databázi.
        /// </summary>
        /// <param name="objectFullName">Plný název objektu pro který se má zjistit existence tabulky.</param>
        /// <returns>Pokud existuje vrací se <c>true</c> jinak <c>false</c>.</returns>
        private static bool TableExists(String objectFullName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TableExists(" + objectFullName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (String.IsNullOrEmpty(objectFullName))
                throw new ArgumentNullException("objectFullName");

            using (SqlCommander cmd = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString()))
            {
                return (cmd.ExecuteScalar("select name from sysobjects where xtype='u' and name = '" + objectFullName + "'") != null);
            }
        }

        /// <summary>
        /// Přidá k tabulce objektu nový sloupec
        /// </summary>
        /// <param name="prop"></param>
        private static void AddColumnToTable(PropertyInfo prop)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("AddColumnToTable(" + prop + ")", System.Reflection.MethodBase.GetCurrentMethod());
            SqlCommander cmd = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            try
            {
                SqlTypeInfo info = SqlDataTypesHelper.GetSqlTypeInfo(prop.PropertyType, null, null);
                cmd.ExecuteDataTable("ALTER TABLE [" + prop.DeclaringType.FullName + "] ADD [" + prop.Name + "] " + GetSqlTypeDefinition(info));
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
        }

        /// <summary>
        /// Upraví existující sloupec v tabulce objektu
        /// </summary>
        /// <param name="prop"></param>
        private static void ModifyColumnInTable(PropertyInfo prop)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ModifyColumnInTable(" + prop + ")", System.Reflection.MethodBase.GetCurrentMethod());
            SqlCommander cmd = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            try
            {
                SqlTypeInfo info = SqlDataTypesHelper.GetSqlTypeInfo(prop.PropertyType, null, null);
                cmd.ExecuteDataTable("ALTER TABLE [" + prop.DeclaringType.FullName + "] ALTER COLUMN [" + prop.Name + "] " + GetSqlTypeDefinition(info));
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
        }

        /// <summary>
        /// Vytvoří SQL zápis datového typu pro tabulkové sloupce.
        /// </summary>
        /// <param name="typeInfo">Informace o datovém typu.</param>
        /// <returns></returns>
        internal static String GetSqlTypeDefinition(SqlTypeInfo typeInfo)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetSqlTypeDefinition(" + typeInfo + ")", System.Reflection.MethodBase.GetCurrentMethod());

            StringBuilder result = new StringBuilder();
            result.Append(typeInfo.SqlTypeName);
            if (!String.IsNullOrEmpty(typeInfo.SqlTypeLength))
            {
                result.Append("(" + typeInfo.SqlTypeLength);
                if (!String.IsNullOrEmpty(typeInfo.SqlTypePrecision))
                    result.Append("," + typeInfo.SqlTypePrecision);
                result.Append(")");
            }
            if (!String.IsNullOrEmpty(typeInfo.SqlDefaultValue))
                result.Append(" DEFAULT " + typeInfo.SqlDefaultValue);
            if (!typeInfo.IsNullable)
                result.Append(" NOT");
            result.Append(" NULL");

            return result.ToString();
        }

        /// <summary>
        /// Vrátí SQL příkaz pro vytvoření indexu nad sloupcem tabulky.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static String GetQueryCreateIndex(String tableName, String columnName, SqlTypeInfo typeInfo)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryCreateIndex(" + tableName + ", " + columnName + ", " + typeInfo + ")", System.Reflection.MethodBase.GetCurrentMethod());

            if (typeInfo.IsIndexable)
                return "CREATE NONCLUSTERED INDEX [idx_" + columnName + "] ON [" + tableName + "] ([" + columnName + "] ASC);";
            return String.Empty;
        }













        /// <summary>
        /// Vrátí SQL dotaz pro získání prázdného schéma tabulky.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <returns>SQL dotaz který vrátí prázdný výsledek z požadované tabulky. Využije se například pro vytvoření prázdného objektu <c>DataTable</c>.</returns>
        internal static String GetQueryTableSchema(String tableName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryTableSchema(" + tableName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            return "select top 0 * from " + tableName;
        }


        /// <summary>
        /// Vytvoří příkaz pro přidání nového sloupce do požadvané tabulky.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název nového sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryAddColumn(String tableName, String columnName, Type type, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryAddColumn(" + tableName + ", " + columnName + ", " + type + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetQueryAddColumn(tableName, columnName, type, null, null, canByNull, defaultValue, isValue);
        }
        /// <summary>
        /// Vytvoří příkaz pro přidání nového sloupce do požadvané tabulky.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název nového sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="typeLength">Délkové či hodnotové omezení datového typu.</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryAddColumn(String tableName, String columnName, Type type, Int32? typeLength, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryAddColumn(" + tableName + ", " + columnName + ", " + type + ", " + typeLength + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetQueryAddColumn(tableName, columnName, type, typeLength, null, canByNull, defaultValue, isValue);
        }
        /// <summary>
        /// Vytvoří příkaz pro přidání nového sloupce do požadvané tabulky.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název nového sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="typeLength">Délkové či hodnotové omezení datového typu.</param>
        /// <param name="typePrecision">Délka desetinných míst v případě datového typu <c>Decimal</c>, <c>Single</c> nebo <c>Double</c>.</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryAddColumn(String tableName, String columnName, Type type, Int32? typeLength, Int32? typePrecision, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryAddColumn(" + tableName + ", " + columnName + ", " + type + ", " + typeLength + ", " + typePrecision + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            return "ALTER TABLE " + tableName + " ADD " + columnName + " " + SqlDataTypesHelper.GetSqlTypeRepresentation(type, typeLength, typePrecision) +
                        " DEFAULT " + SqlDataTypesHelper.GetSqlValueRepresentation(type, defaultValue, isValue) + " " + (canByNull ? String.Empty : "NOT ") + "NULL";
        }

        /// <summary>
        /// Vytvoří příkaz pro modifikaci existujícího sloupce v požadvané tabulce.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název existujícího sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryModifyColumn(String tableName, String columnName, Type type, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryModifyColumn(" + tableName + ", " + columnName + ", " + type + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetQueryModifyColumn(tableName, columnName, type, null, null, canByNull, defaultValue, isValue);
        }
        /// <summary>
        /// Vytvoří příkaz pro modifikaci existujícího sloupce v požadvané tabulce.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název existujícího sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="typeLength">Délkové či hodnotové omezení datového typu.</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryModifyColumn(String tableName, String columnName, Type type, Int32? typeLength, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryModifyColumn(" + tableName + ", " + columnName + ", " + type + ", " + typeLength + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return GetQueryModifyColumn(tableName, columnName, type, typeLength, null, canByNull, defaultValue, isValue);
        }
        /// <summary>
        /// Vytvoří příkaz pro modifikaci existujícího sloupce v požadvané tabulce.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název existujícího sloupce.</param>
        /// <param name="type">.NET datový typ</param>
        /// <param name="typeLength">Délkové či hodnotové omezení datového typu.</param>
        /// <param name="typePrecision">Délka desetinných míst v případě datového typu <c>Decimal</c>, <c>Single</c> nebo <c>Double</c>.</param>
        /// <param name="canByNull">Zda může sloupcec obsahovat null hodnoty.</param>
        /// <param name="defaultValue">V případě ukládání null hodnoty se do sloupce vloží předaná hodnota nebo hodnota nějaká SQL funkce.</param>
        /// <param name="isValue">Zda je hodnota argumentu <paramref name="defaultValue"/> hodnotou či názvem SQL funkce.</param>
        /// <returns></returns>
        internal static String GetQueryModifyColumn(String tableName, String columnName, Type type, Int32? typeLength, Int32? typePrecision, Boolean canByNull, Object defaultValue, Boolean isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryModifyColumn(" + tableName + ", " + columnName + ", " + type + ", " + typeLength + ", " + typePrecision + ", " + canByNull + ", " + defaultValue + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            String defaultStatement = SqlDataTypesHelper.GetSqlValueRepresentation(type, defaultValue, isValue);
            return "ALTER TABLE " + tableName + " ALTER COLUMN " + columnName + " " + SqlDataTypesHelper.GetSqlTypeRepresentation(type, typeLength, typePrecision) +
                        (defaultStatement == "NULL" && canByNull ? String.Empty : " DEFAULT " + defaultStatement) + (canByNull ? String.Empty : " NOT") + " NULL";
        }


        /// <summary>
        /// Vytvoří příkaz pro odstranění existujícího sloupce z požadvané tabulky.
        /// </summary>
        /// <param name="tableName">Název tabulky.</param>
        /// <param name="columnName">Název existujícího sloupce.</param>
        /// <returns></returns>
        internal static String GetQueryDropColumn(String tableName, String columnName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetQueryDropColumn(" + tableName + ", " + columnName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");

            return "ALTER TABLE " + tableName + " DROP COLUMN " + columnName;
        }
    }
}
