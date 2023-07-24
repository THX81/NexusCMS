namespace Nexus.Data.AdaptiveDAL
{
    using System;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Základní objekt pro bussines objekty které můžou být ukládány do uložiště objektů.
    /// </summary>
    [Serializable]
    public abstract class BusinessObjectBase
    {
        /// <summary>
        /// Prázdný konstruktor.
        /// </summary>
        public BusinessObjectBase()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("BusinessObjectBase()", System.Reflection.MethodBase.GetCurrentMethod());
            //foreach (PropertyInfo prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    System.Web.HttpContext.Current.Response.Write(prop.Name + "<br>\n");
            //}
        }

        /// <summary>
        /// Uloží nebo aktualizuje objekt v úložišti objektů.
        /// </summary>
        /// <param name="obj">Objekt k uložení.</param>
        /// <returns>Pokud se nevyskytnou problémy tak se vrátí počet ovlivněných objektů.</returns>
        public Int32 Save()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Save()", System.Reflection.MethodBase.GetCurrentMethod());

            BusinessObjectsSqlStorageManager.EnsureTableForObject(this.GetType());

            SqlCommander sqlCommander = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            Int32 result = 0;
            if (sqlCommander.ExecuteScalar("select ObjectId from [" + this.GetType().FullName + "] where ObjectId='" + this.ObjectId + "'") != null)
            {
                //result = sqlCommander.ExecuteNonQuery("update [" + this.GetType().FullName + "] set ObjectData = '" + this.Serialize() + "' where TypeName='" + this.GetType().FullName + "' and ObjectId='" + this.ObjectId + "'");
            }
            else
            {
                StringBuilder sbInsert = new StringBuilder(256);
                sbInsert.Append("DECLARE @NewId uniqueidentifier;");
                sbInsert.Append("SET @NewId = newid();");
                sbInsert.Append("insert into [" + this.GetType().FullName + "]([ObjectId]");
                foreach (PropertyInfo prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        sbInsert.Append(", [" + prop.Name + "]");
                    }
                }
                sbInsert.Append(") values(@NewId");
                foreach (PropertyInfo prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        sbInsert.Append(", " + SqlDataTypesHelper.GetSqlValueRepresentation(prop.PropertyType, prop.GetValue(this, null), true));
                    }
                }
                sbInsert.Append(");");
                sbInsert.Append("SELECT ObjectId,CreateDate,ModifyDate FROM [" + this.GetType().FullName + "] WHERE ObjectId=@NewId;");
                DbCommand cmd = new SqlCommand(sbInsert.ToString());
                DbDataReader reader = null;
                try
                {
                    reader = sqlCommander.ExecuteReader(ref cmd);
                    if (reader.FieldCount == 3 && reader.Read())
                    {
                        this.objectId = reader.GetGuid(0);
                        this.createDate = reader.GetDateTime(1);
                        this.modifyDate = reader.GetDateTime(2);
                    }
                    result = reader.RecordsAffected;
                }
                catch (Exception ex)
                {
                    Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
                }
                finally
                {
                    if (reader != null)
                        reader.Dispose();
                    sbInsert = null;
                    if (sqlCommander != null)
                        sqlCommander.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Vymaže objekt v úložišti objektů.
        /// </summary>
        /// <returns>Pokud se nevyskytnou problémy tak se vrátí počet ovlivněných objektů.</returns>
        public int Delete()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Delete()", System.Reflection.MethodBase.GetCurrentMethod());
            SqlCommander sqlCommander = new SqlCommander(BusinessObjectManagerConfigSection.GetConnectionString());
            Int32 result = 0;
            if (this.ObjectId.ToString() != "00000000-0000-0000-0000-000000000000")
            {

                BusinessObjectsSqlStorageManager.EnsureTableForObject(this.GetType());

                try
                {
                    result = sqlCommander.ExecuteNonQuery("delete from [" + this.GetType().FullName + "] where ObjectId='" + this.ObjectId + "'");
                }
                catch (Exception ex)
                {
                    Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
                }
                finally
                {
                    if (sqlCommander != null)
                        sqlCommander.Dispose();
                }
            }
            return result;
        }


        /// <summary>
        /// Používá se pro jedinečnou identifikaci objektu.
        /// </summary>
        internal Guid objectId;
        /// <summary>
        /// Datum vytvoření objektu.
        /// </summary>
        internal DateTime createDate;
        /// <summary>
        /// Datum posledního uložení objektu.
        /// </summary>
        internal DateTime modifyDate;

        /// <summary>
        /// Používá se pro jedinečnou identifikaci objektu v databázi i v .NET prostředí.
        /// </summary>
        public Guid ObjectId
        {
            get { return this.objectId; }
        }
        /// <summary>
        /// Datum vytvoření objektu.
        /// </summary>
        public DateTime CreateDate
        {
            get { return this.createDate; }
        }
        /// <summary>
        /// Datum posledního uložení objektu.
        /// </summary>
        public DateTime ModifyDate
        {
            get { return this.modifyDate; }
        }
    }
}
