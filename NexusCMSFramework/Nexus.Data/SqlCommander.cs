namespace Nexus.Data
{
    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;

    /// <summary>
    /// T��da obsahuje pomocn� metody pro manimulaci s daty. Nahrazuje funk�nost objekt� <c>MySqlConnection</c>, <c>MySqlCommand</c> a <c>MySqlDataAdapter</c>.
    /// Je pot�eba p�idat do konfigurace aplikace sekci "SqlCommander". 'section name="SqlCommander" type="Nexus.Data.SqlCommanderSection, Nexus.Data, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" allowLocation="true" allowDefinition="Everywhere"'
    /// </summary>
    public class SqlCommander : IDisposable
    {
        internal SqlConnectionHolder holder;

        /// <summary>
        /// Konstruktor. Typ datab�ze je MSSql2005.
        /// </summary>
        /// <param name="connectionString">P�ipojovac� �et�zec.</param>
        public SqlCommander(string connectionString)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SqlCommander(" + connectionString + ")", System.Reflection.MethodBase.GetCurrentMethod());
            holder = new SqlConnectionHolder(connectionString);
        }



        /// <summary>
        /// Pomocn� funkce pro vytvo�en� vstupn�ch parametr�.
        /// </summary>
        /// <param name="paramName">nazev parametru</param>
        /// <param name="dataType">MySql typ</param>
        /// <param name="objValue">hodnota</param>
        /// <returns>objekt parametru</returns>
        public static DbParameter CreateInputParam(string paramName, DbType dataType, object objValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("CreateInputParam(" + paramName + ", " + dataType + ", " + objValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DbParameter param = null;
            param = new SqlParameter(paramName, dataType);
            param.Value = objValue;
            param.Direction = ParameterDirection.Input;
            return param;
        }

        /// <summary>
        /// Pomocn� funkce pro vytvo�en� vstupne vystupnich parametr�.
        /// </summary>
        /// <param name="paramName">nazev parametru</param>
        /// <param name="dataType">Sql typ</param>
        /// <param name="objValue">hodnota</param>
        /// <returns>objekt parametru</returns>
        public static DbParameter CreateInputOutputParam(string paramName, DbType dataType, object objValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("CreateInputOutputParam(" + paramName + ", " + dataType + ", " + objValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DbParameter param = null;
            param = new SqlParameter(paramName, dataType);
            param.Value = objValue;
            param.Direction = ParameterDirection.InputOutput;
            return param;
        }

        /// <summary>
        /// Pomocn� funkce pro vytvo�en� v�stupn�ch parametr�.
        /// </summary>
        /// <param name="paramName">nazev parametru</param>
        /// <param name="dataType">Sql typ</param>
        /// <returns>objekt parametru</returns>
        public static DbParameter CreateOutputParam(string paramName, DbType dataType)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("CreateOutputParam(" + paramName + ", " + dataType + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DbParameter param = null;
            param = new SqlParameter(paramName, dataType);
            param.Direction = ParameterDirection.Output;
            return param;
        }

        /// <summary>
        /// Pomocn� funkce pro vytvo�en� navratovych parametru
        /// </summary>
        /// <param name="paramName">nazev parametru</param>
        /// <param name="dataType">Sql typ</param>
        /// <returns>objekt parametru</returns>
        public static DbParameter CreateReturnValueParam(string paramName, DbType dataType)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("CreateReturnValueParam(" + paramName + ", " + dataType + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DbParameter param = null;
            param = new SqlParameter(paramName, dataType);
            param.Direction = ParameterDirection.ReturnValue;
            return param;
        }

        /// <summary>
        /// Pomocn� metoda pou��van� k z�sk�n� hodnoty v�stupn�ho parametru po vykon�n� dotazu.
        /// Typicky se jedn� o parametr s celkov�m mno�stv�m z�znam� v datab�zi.
        /// </summary>
        /// <param name="pars">kolekce parametr� pou�it� p�i dotazu.</param>
        /// <returns>pokud kolekce obsahuje hledan� parametr tak vrac� po�et z�znam�, v p��pad� chyby -1.</returns>
        public static Int32 GetReturnValue(DbParameterCollection pars)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetReturnValue(" + Nexus.Diagnostics.Log4NetWrapper.GetCollectionData(pars) + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (pars != null)
                foreach (DbParameter param in pars)
                {
                    if (param.Direction == ParameterDirection.Output && param.Value != null && param.Value is Int32)
                        return (Int32)param.Value;
                }

            return -1;
        }


        
        /// <summary>
        /// Vykon�v� dotazy bez n�vratov�ch dat.
        /// Bez parametr� a CommandType jako "Text"
        /// </summary>
        /// <param name="sql">SQL dotaz</param>
        /// <returns>V p��pad� delete, update a insert vr�t� po�et ovlivn�n�ch �adk�</returns>
        public Int32 ExecuteNonQuery(string sql)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteNonQuery(" + sql + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Int32 result = 0;
            DbCommand cmd = null;
            cmd = new SqlCommand(sql);
            try
            {
                cmd.CommandType = CommandType.Text;
                result = ExecuteNonQuery(ref cmd);
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Vykon�v� dotazy bez n�vratov�ch dat.
        /// </summary>
        /// <param name="cmd">Objekt by m�l minim�ln� obsahovat <c>DbCommand.CommandText</c>.
        /// P��padn� se daj� d�le p�ed�vat data v jeho vlatnostech jako nap��klad <c>DbCommand.CommandType</c> nebo <c>DbCommand.Parameters</c></param>
        /// <returns>V p��pad� delete, update a insert vr�t� po�et ovlivn�n�ch �adk�</returns>
        public Int32 ExecuteNonQuery(ref DbCommand cmd)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteNonQuery(ref " + cmd + ")", System.Reflection.MethodBase.GetCurrentMethod());
            int result = 0;
            
            try
            {
                holder.Close();
                holder.Open();
                cmd.Connection = holder.Connection;

                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                holder.Close();
            }
            return result;
        }

        /// <summary>
        /// Metoda vrac� DataTable napln�n� daty z SQL dotazu.
        /// Bez parametr� a CommandType jako "Text"
        /// </summary>
        /// <param name="sql">SQL dotaz</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteDataTable(" + sql + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DataTable result = new DataTable();
            DbCommand cmd = null;
            cmd = new SqlCommand(sql);
            try
            {
                cmd.CommandType = CommandType.Text;
                result = ExecuteDataTable(ref cmd);
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Metoda vrac� DataTable napln�n� daty z SQL dotazu.
        /// </summary>
        /// <param name="cmd">Objekt by m�l minim�ln� obsahovat <c>DbCommand.CommandText</c>.
        /// P��padn� se daj� d�le p�ed�vat data v jeho vlatnostech jako nap��klad <c>DbCommand.CommandType</c> nebo <c>DbCommand.Parameters</c></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(ref DbCommand cmd)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteDataTable(ref " + cmd + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DataTable dt = new DataTable();
            DbDataAdapter ad = new SqlDataAdapter();
            
            try
            {
                holder.Open();
                cmd.Connection = holder.Connection;
                ad.SelectCommand = cmd;

                if (!String.IsNullOrEmpty(cmd.CommandText))
                    ad.Fill(dt);

            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (holder != null)
                    holder.Close();
                if (ad != null)
                    ad.Dispose();
            }
            return dt;
        }

        /// <summary>
        /// Umo��uje zpracovavat dotazy nap��klad pro bin�rn� data.
        /// Bez parametr� a CommandType jako "Text"
        /// </summary>
        /// <param name="sql">SQL dotaz</param>
        /// <returns></returns>
        public Object ExecuteScalar(string sql)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteScalar(" + sql + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Object result = new Object();
            DbCommand cmd = null;
            cmd = new SqlCommand(sql);
            try
            {
                cmd.CommandType = CommandType.Text;
                result = ExecuteScalar(ref cmd);
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Umo��uje zpracovavat dotazy nap��klad pro bin�rn� data.
        /// </summary>
        /// <param name="cmd">Objekt by m�l minim�ln� obsahovat <c>DbCommand.CommandText</c>.
        /// P��padn� se daj� d�le p�ed�vat data v jeho vlatnostech jako nap��klad <c>DbCommand.CommandType</c> nebo <c>DbCommand.Parameters</c></param>
        /// <returns></returns>
        public Object ExecuteScalar(ref DbCommand cmd)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteScalar(ref " + cmd + ")", System.Reflection.MethodBase.GetCurrentMethod());
            Object result = new Object();
            try
            {
                holder.Open();
                cmd.Connection = holder.Connection;

                if (!String.IsNullOrEmpty(cmd.CommandText))
                    result = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (holder != null)
                    holder.Close();
            }

            return result;
        }

        /// <summary>
        /// Umo��uje vykon�vat SQL p��kazy pomoc� objektu <c>DbDataReader</c>.
        /// CommandBehavior na "CloseConnection"
        /// </summary>
        /// <param name="cmd">Objekt by m�l minim�ln� obsahovat <c>DbCommand.CommandText</c>.
        /// P��padn� se daj� d�le p�ed�vat data v jeho vlatnostech jako nap��klad <c>DbCommand.CommandType</c> nebo <c>DbCommand.Parameters</c></param>
        /// <returns>P��stup k datum vysledku p�es objekt <c>DbDataReader</c></returns>
        public DbDataReader ExecuteReader(ref DbCommand cmd)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteReader(ref " + cmd + ")", System.Reflection.MethodBase.GetCurrentMethod());
            return ExecuteReader(ref cmd, CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Umo��uje vykon�vat SQL p��kazy pomoc� objektu <c>DbDataReader</c>.
        /// </summary>
        /// <param name="cmd">Objekt by m�l minim�ln� obsahovat <c>DbCommand.CommandText</c>.
        /// P��padn� se daj� d�le p�ed�vat data v jeho vlatnostech jako nap��klad <c>DbCommand.CommandType</c> nebo <c>DbCommand.Parameters</c></param>
        /// <param name="cmdBehavior">Modifik�tor pro chovani DataReaderu</param>
        /// <returns>P��stup k datum vysledku p�es objekt <c>DbDataReader</c></returns>
        public DbDataReader ExecuteReader(ref DbCommand cmd, CommandBehavior cmdBehavior)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteReader(ref " + cmd + ", " + cmdBehavior + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DbDataReader reader = null;

            try
            {
                cmd.Connection = holder.Connection;
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();

                if (!String.IsNullOrEmpty(cmd.CommandText))
                    reader = cmd.ExecuteReader(cmdBehavior);
            }
            catch (Exception ex)
            {
                if (holder != null)
                    holder.Close();
                if (cmd != null)
                    cmd.Dispose();
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            return reader;
        }




        /// <summary>
        /// Executes a SQL command(s) and returns the resultset in a System.Data.DataSet.
        /// A new SqlConnection object is created, opened, and closed during this method.
        /// </summary>
        /// <param name="sql">Command(s) to execute without parameters</param>
        /// <returns>System.Data.DataSet containing the resultset</returns>
        public DataSet ExecuteDataset(string sql)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteDataset(" + sql + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DataSet result = new DataSet();
            DbCommand cmd = new SqlCommand(sql);

            try
            {
                cmd.CommandType = CommandType.Text;
                result = ExecuteDataset(ref cmd);
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

            return result;
        }

        /// <summary>
        /// Executes a SQL command(s) and returns the resultset in a System.Data.DataSet.
        /// A new MySqlConnection object is created, opened, and closed during this method.
        /// </summary>
        /// <param name="cmd">Command to execute, parameters to use for the command and commandType</param>
        /// <returns>System.Data.DataSet containing the resultset</returns>
        public DataSet ExecuteDataset(ref DbCommand cmd)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("ExecuteDataset(" + cmd + ")", System.Reflection.MethodBase.GetCurrentMethod());
            DataSet result = new DataSet();
            cmd.Connection = holder.Connection;
            SqlDataAdapter da = new SqlDataAdapter(cmd as SqlCommand);
            try
            {
                da.Fill(result);
            }
            catch (Exception ex)
            {
                Nexus.Diagnostics.Log4NetWrapper.Error(ex, System.Reflection.MethodBase.GetCurrentMethod());
            }
            finally
            {
                if (da != null)
                    da.Dispose();
            }
            return result;
        }




        /// <summary>
        /// Zajist� uzav�en� p�ipojen� k datab�zi
        /// </summary>
        public void Dispose()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Dispose()", System.Reflection.MethodBase.GetCurrentMethod());
            if (holder != null)
            {
                if (holder.Connection.State != ConnectionState.Closed)
                    holder.Close();
            }
        }
    }
    
}

