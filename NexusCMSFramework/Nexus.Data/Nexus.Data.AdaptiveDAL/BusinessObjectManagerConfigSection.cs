namespace Nexus.Data.AdaptiveDAL
{
    using  System;
    using  System.Configuration;

    internal class BusinessObjectManagerConfigSection : ConfigurationSection
    {
        public BusinessObjectManagerConfigSection()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("BusinessObjectManagerConfigSection()", System.Reflection.MethodBase.GetCurrentMethod());
        }

        public BusinessObjectManagerConfigSection(String connectionStringName)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("BusinessObjectManagerConfigSection(" + connectionStringName + ")", System.Reflection.MethodBase.GetCurrentMethod());
            ConnectionStringName = connectionStringName;
        }

        /// <summary>
        /// Naète konfiguraci pro 
        /// </summary>
        /// <returns></returns>
        public static BusinessObjectManagerConfigSection GetConfig()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetConfig()", System.Reflection.MethodBase.GetCurrentMethod());
            BusinessObjectManagerConfigSection config = ConfigurationManager.GetSection("BusinessObjectManager") as BusinessObjectManagerConfigSection;

            if (config == null)
                Nexus.Diagnostics.Log4NetWrapper.Error(new ArgumentNullException("config"), System.Reflection.MethodBase.GetCurrentMethod());

            return config;
        }

        /// <summary>
        /// Naète žetìzec pro pøipojení k databázi dle aplikaèní konfigurace pro objekt <c>BusinessObjectManager<T></c>.
        /// </summary>
        /// <returns>Pøipojovací øetìzec k SQL databázi.</returns>
        internal static String GetConnectionString()
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetConnectionString()", System.Reflection.MethodBase.GetCurrentMethod());
            return ConfigurationManager.ConnectionStrings[GetConfig().ConnectionStringName].ConnectionString;
        }

        [ConfigurationProperty("connectionStringName", DefaultValue = "", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
        public String ConnectionStringName
        {
            get
            { return (String)this["connectionStringName"]; }
            set
            { this["connectionStringName"] = value; }
        }
    }
}
