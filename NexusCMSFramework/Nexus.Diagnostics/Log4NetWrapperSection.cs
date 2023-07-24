    using  System;
    using  System.Configuration;

namespace Nexus.Diagnostics
{
    /// <summary>
    /// Konfiguraèní sekce pro wrapper log4net.
    /// </summary>
    internal class Log4NetWrapperSection : ConfigurationSection
    {
        /// <summary>
        /// Ovlivòuje logavání vyjímek metodou <c>Log4NetWrapper.Error()</c>, zda se následnì vyhodí i v public metodách. 
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
