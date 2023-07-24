    using  System;
    using  System.Configuration;

namespace Nexus.Diagnostics
{
    /// <summary>
    /// Konfigura�n� sekce pro wrapper log4net.
    /// </summary>
    internal class Log4NetWrapperSection : ConfigurationSection
    {
        /// <summary>
        /// Ovliv�uje logav�n� vyj�mek metodou <c>Log4NetWrapper.Error()</c>, zda se n�sledn� vyhod� i v public metod�ch. 
        /// </summary>
        [ConfigurationProperty("throwAllErrorsAfterLoged", DefaultValue = false, IsRequired = true)]
        public Boolean ThrowAllErrorsAfterLoged
        {
            get
            { return (Boolean)this["throwAllErrorsAfterLoged"]; }
            set
            { this["throwAllErrorsAfterLoged"] = value; }
        }

    }
}
