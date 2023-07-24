using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Web.Hosting;

namespace Nexus.Data
{
    /// <summary>
    /// Tøída pro zajištìní pøipojení k databazi.
    /// </summary>
    internal sealed class SqlConnectionHolder
    {
        internal DbConnection _Connection;
        private bool _Opened;


        internal DbConnection Connection
        {
            get { return _Connection; }
        }

        internal SqlConnectionHolder(string connectionString)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("SqlConnectionHolder(" + connectionString + ")", System.Reflection.MethodBase.GetCurrentMethod());
            try
            {
                _Connection = new SqlConnection(connectionString);
            }
            catch (ArgumentException e)
            {
                e = new ArgumentException("SqlError_Connection_String", "connectionString", e);
                Nexus.Diagnostics.Log4NetWrapper.Info(e, System.Reflection.MethodBase.GetCurrentMethod());
            }
        }


        internal void Open()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Open()", System.Reflection.MethodBase.GetCurrentMethod());
            Open(false);
        }

        internal void Open(bool revertImpersonate)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Open(" + revertImpersonate + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (_Opened)
                return; // Already opened

            if (revertImpersonate)
            {
                using (HostingEnvironment.Impersonate())
                {
                    Connection.Open();
                }
            }
            else
            {
                Connection.Open();
            }

            _Opened = true; // Open worked!
        }

        internal void Close()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("Close()", System.Reflection.MethodBase.GetCurrentMethod());
            if (!_Opened) // Not open!
                return;
            // Close connection
            Connection.Close();
            _Opened = false;
        }
    }
}
