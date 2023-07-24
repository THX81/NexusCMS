namespace Nexus.Data.AdaptiveDAL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Serialization;

    using Nexus.Data;
    using Nexus.Data.Linq;
    using Nexus.Data.AdaptiveDAL;
    using Nexus.Reflection;

    /// <summary>
    /// Zajišťuje persistenci předaných objektů v Sql databázi a jejich znovuzískávání.
    /// </summary>
    public static class BusinessObjectManager<T>
        where T : BusinessObjectBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select(Expression<Func<T, bool>> filter)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Select(" + filter + ")", System.Reflection.MethodBase.GetCurrentMethod());

            //BusinessObjectsSqlStorageManager.EnsureTableForObject(typeof(T));

            QueryTranslator tr = new QueryTranslator();

            HttpContext.Current.Response.Write("tr.Translate(filter)=" + tr.Translate(filter) + "<br>");


            List<T> result = new List<T>();
            result.Add(new ClientTest() { Name = "yyy" } as T);
            result.Add(new ClientTest() { Name = "yyy" } as T);
            result.Add(new ClientTest() { Name = "xxx" } as T);
            result.Add(new ClientTest() { Name = "xxx" } as T);
            result.Add(new ClientTest() { Name = "aaa" } as T);
            result.Add(new ClientTest() { Name = "ccc" } as T);
            result.Add(new ClientTest() { Name = "bbb" } as T);
            return result;
        }

        /// <summary>
        /// Vrátí z objektového úložiště všechny objekty požadovaného typu objektů.
        /// </summary>
        /// <returns>Všechny vyhovující typy objektů.</returns>
        public static Collection<T> GetAll()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetAll()", System.Reflection.MethodBase.GetCurrentMethod());

            BusinessObjectsSqlStorageManager.EnsureTableForObject(typeof(T));

            SqlCommander sqlCommander = new SqlCommander(ConfigurationManager.ConnectionStrings[BusinessObjectManagerConfigSection.GetConfig().ConnectionStringName].ConnectionString); ;
            Collection<T> result = new Collection<T>();
            DbDataReader reader = null;
            DbCommand cmd = new SqlCommand();
            StringBuilder selectQuerySB = new StringBuilder();

            try
            {
                selectQuerySB.Append("select ObjectId, TypeName, ObjectData, CreateDate, ModifyDate from BusinessObjects where TypeName=@TypeName");

                cmd.CommandText = selectQuerySB.ToString();
                (cmd as SqlCommand).Parameters.AddWithValue("@TypeName", typeof(T).FullName);

                reader = sqlCommander.ExecuteReader(ref cmd, CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    ////////////T newObject = BusinessObjectBase.Deserialize<T>(reader.GetString(2));
                    ////////////if (newObject != null)
                    ////////////{
                    ////////////    newObject.objectId = reader.GetGuid(0);
                    ////////////    newObject.createDate = reader.GetDateTime(3);
                    ////////////    newObject.modifyDate = reader.GetDateTime(4);
                    ////////////    result.Add(newObject);
                    ////////////}
                }
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
                if (cmd != null)
                    cmd.Dispose();
                if (sqlCommander != null)
                    sqlCommander.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Vrátí z objektového úložiště všechny objekty požadovaného typu objektů.
        /// </summary>
        /// <param name="pageIndex">Index požadované stránky dat.</param>
        /// <param name="pageCount">Počet objektů v jedné stránce.</param>
        /// <param name="totalCount">Navrácená hodnota celkového počtu objektů.</param>
        /// <returns>Všechny vyhovující typy objektů.</returns>
        public static Collection<T> GetAll(int pageIndex, int pageCount, out int totalCount)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetAll(" + pageIndex + ", " + pageCount + ", out totalCount)", System.Reflection.MethodBase.GetCurrentMethod());

            BusinessObjectsSqlStorageManager.EnsureTableForObject(typeof(T));

            SqlCommander sqlCommander = new SqlCommander(ConfigurationManager.ConnectionStrings[BusinessObjectManagerConfigSection.GetConfig().ConnectionStringName].ConnectionString); ;
            Collection<T> result = new Collection<T>();
            DbDataReader reader = null;
            DbCommand cmd = null;
            StringBuilder selectQuerySB = new StringBuilder();
            totalCount = 0;

            cmd = new SqlCommand();

            try
            {
                selectQuerySB.Append("select ObjectId, TypeName, ObjectData, CreateDate, ModifyDate from ");
                selectQuerySB.Append("(select ROW_NUMBER() over(order by ObjectId) as RowNum, ObjectId, TypeName, ObjectData, CreateDate, ModifyDate from BusinessObjects where TypeName=@TypeName) PagedBusinessObjects ");
                selectQuerySB.Append("where RowNum between " + (pageIndex * pageCount) + " and " + ((pageIndex * pageCount) + pageCount));

                cmd.CommandText = selectQuerySB.ToString();
                (cmd as SqlCommand).Parameters.AddWithValue("@TypeName", typeof(T).FullName);

                reader = sqlCommander.ExecuteReader(ref cmd, CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    ////////////T newObject = BusinessObjectBase.Deserialize<T>(reader.GetString(2));
                    ////////////if (newObject != null)
                    ////////////{
                    ////////////    newObject.objectId = reader.GetGuid(0);
                    ////////////    newObject.createDate = reader.GetDateTime(3);
                    ////////////    newObject.modifyDate = reader.GetDateTime(4);
                    ////////////    result.Add(newObject);
                    ////////////}
                }
                if (reader.NextResult() && reader.Read())
                {
                    totalCount = reader.GetInt32(0);
                }
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
                if (cmd != null)
                    cmd.Dispose();
                if (sqlCommander != null)
                    sqlCommander.Dispose();
            }

            return result;
        }
    }

    [Serializable]
    public class ClientTest : BusinessObjectBase
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public Int16 Age { get; set; }
        public string Address { get; set; }
    }
}
