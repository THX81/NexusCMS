using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;

namespace Nexus.Diagnostics
{
    /// <summary>
    /// Zabaluje nejpoužívanejší metody log4net aby staèilo volat pouze jedinou metodu.
    /// </summary>
    public static class Log4NetWrapper
    {
        /// <summary>
        /// Log a message object with the log4net.Core.Level.Debug level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="currentMethod"></param>
        /// <remarks>
        ///      This method first checks if this logger is DEBUG enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Debug level. If this logger
        ///     is DEBUG enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Debug(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Debug(object message, MethodBase currentMethod)
        {
            Debug(message, currentMethod.DeclaringType);
        }

        /// <summary>
        /// Log a message object with the log4net.Core.Level.Debug level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="caller"></param>
        /// <remarks>
        ///      This method first checks if this logger is DEBUG enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Debug level. If this logger
        ///     is DEBUG enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Debug(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Debug(object message, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Debug(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="caller"></param>
        /// <remarks>
        /// </remarks>
        public static void Debug(object message, Exception exception, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Debug(message, exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// </remarks>
        public static void Debug(ILog logger, object message)
        {
            Configure();
            logger.Debug(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <remarks>
        /// </remarks>
        public static void Debug(ILog logger, object message, Exception exception)
        {
            Configure();
            logger.Debug(message, exception);
        }







        /// <summary>
        /// Zapíše do logu informace o vyjímce a v pøípadì že se nejedná o <c>public</c> metodu, vyhodí pøedanou vyjímku.
        /// Pokud je v konfiguraci <c>Log4NetWrapper</c> nastavený parametr <c>throwAllErrorsAfterLoged</c>, budou vyhazovány i vyjímky pro <c>public</c> memtody.
        /// </summary>
        /// <param name="ex">Vyjímka která se má zpracovat.</param>
        /// <param name="currentMethod">Odkaz na právì bìžící metodu.</param>
        /// <example>
        /// 
        /// </example>
        public static void Error(Exception ex, MethodBase currentMethod)
        {
            Error(ex, currentMethod.DeclaringType);

            if (!currentMethod.IsPublic)
                throw ex;

            if (GetLog4NetWrapperConfig() != null)
                if (currentMethod.IsPublic && GetLog4NetWrapperConfig().ThrowAllErrorsAfterLoged)
                    throw ex;
        }

        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Error level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="caller"></param>
        /// <remarks>
        ///      This method first checks if this logger is ERROR enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Error level. If this logger
        ///     is ERROR enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Error(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Error(object message, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Error(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="caller"></param>
        /// <remarks>
        /// </remarks>
        public static void Error(object message, Exception exception, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Error(message, exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// </remarks>
        public static void Error(ILog logger, object message)
        {
            Configure();
            logger.Error(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <remarks>
        /// </remarks>
        public static void Error(ILog logger, object message, Exception exception)
        {
            Configure();
            logger.Error(message, exception);
        }







        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Fatal level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="currentMethod"></param>
        /// <remarks>
        ///      This method first checks if this logger is FATAL enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Fatal level. If this logger
        ///     is FATAL enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Fatal(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Fatal(object message, MethodBase currentMethod)
        {
            Fatal(message, currentMethod.DeclaringType);
        }

        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Fatal level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="caller"></param>
        /// <remarks>
        ///      This method first checks if this logger is FATAL enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Fatal level. If this logger
        ///     is FATAL enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Fatal(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Fatal(object message, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Fatal(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="caller"></param>
        /// <remarks>
        /// </remarks>
        public static void Fatal(object message, Exception exception, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Fatal(message, exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// </remarks>
        public static void Fatal(ILog logger, object message)
        {
            Configure();
            logger.Fatal(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <remarks>
        /// </remarks>
        public static void Fatal(ILog logger, object message, Exception exception)
        {
            Configure();
            logger.Fatal(message, exception);
        }







        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Info level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="currentMethod"></param>
        /// <remarks>
        ///      This method first checks if this logger is INFO enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Info level. If this logger
        ///     is INFO enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Info(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Info(object message, MethodBase currentMethod)
        {
            Info(message, currentMethod.DeclaringType);
        }

        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Info level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="caller"></param>
        /// <remarks>
        ///      This method first checks if this logger is INFO enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Info level. If this logger
        ///     is INFO enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Info(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Info(object message, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Info(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="caller"></param>
        /// <remarks>
        /// </remarks>
        public static void Info(object message, Exception exception, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Info(message, exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// </remarks>
        public static void Info(ILog logger, object message)
        {
            Configure();
            logger.Info(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <remarks>
        /// </remarks>
        public static void Info(ILog logger, object message, Exception exception)
        {
            Configure();
            logger.Info(message, exception);
        }







        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Warn level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="currentMethod"></param>
        /// <remarks>
        ///      This method first checks if this logger is WARN enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Warn level. If this logger
        ///     is WARN enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Warn(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Warn(object message, MethodBase currentMethod)
        {
            Warn(message, currentMethod.DeclaringType);
        }

        /// <summary>
        /// Logs a message object with the log4net.Core.Level.Warn level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="caller"></param>
        /// <remarks>
        ///      This method first checks if this logger is WARN enabled by comparing the
        ///     level of this logger with the log4net.Core.Level.Warn level. If this logger
        ///     is WARN enabled, then it converts the message object (passed as parameter)
        ///     to a string by invoking the appropriate log4net.ObjectRenderer.IObjectRenderer.
        ///     It then proceeds to call all the registered appenders in this logger and
        ///     also higher in the hierarchy depending on the value of the additivity flag.
        ///     WARNING Note that passing an System.Exception to this method will print the
        ///     name of the System.Exception but no stack trace. To print a stack trace use
        ///     the log4net.ILog.Warn(System.Object,System.Exception) form instead.
        /// </remarks>
        public static void Warn(object message, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Warn(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="caller"></param>
        /// <remarks>
        /// </remarks>
        public static void Warn(object message, Exception exception, Type caller)
        {
            Configure();
            LogManager.GetLogger(caller).Warn(message, exception);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// </remarks>
        public static void Warn(ILog logger, object message)
        {
            Configure();
            logger.Warn(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <remarks>
        /// </remarks>
        public static void Warn(ILog logger, object message, Exception exception)
        {
            Configure();
            logger.Warn(message, exception);
        }





        /// <summary>
        /// Projde pripadne pole a vypise vsechny jeho hodnoty do retezce.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String GetCollectionData(Object data)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetCollectionData(" + data + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (data == null) return String.Empty;

            ICollection dataCollection = data as ICollection;
            StringBuilder result = new StringBuilder();
            if (dataCollection != null)
            {
                foreach (Object item in dataCollection)
                {
                    if (item != null)
                        result.Append(HttpUtility.UrlEncode(item.ToString()));
                }
            }
            else
            {
                result.Append(HttpUtility.UrlEncode(data.ToString()));
            }

            return result.ToString();
        }



        /// <summary>
        ///     Automatically configures the log4net system based on the application's configuration
        ///     settings.
        /// </summary>
        /// <remarks>
        ///      Each application has a configuration file. This has the same name as the
        ///     application with '.config' appended.  This file is XML and calling this function
        ///     prompts the configurator to look in that file for a section called log4net
        ///     that contains the configuration data.
        ///      To use this method to configure log4net you must specify the log4net.Config.Log4NetConfigurationSectionHandler
        ///     section handler for the log4net configuration section. See the log4net.Config.Log4NetConfigurationSectionHandler
        ///     for an example.
        /// </remarks>
        public static void Configure()
        {
            String cacheKey = "Log4NetConfigured";
            bool callConf = true;

            if (HttpContext.Current != null)
            {
                object configured = (object)HttpRuntime.Cache[cacheKey];

                if (configured == null)
                {
                    configured = new object();
                    HttpRuntime.Cache.Insert(cacheKey, configured, null, DateTime.Now.AddSeconds(10), TimeSpan.Zero);
                }
                else
                    callConf = false;
            }

            if (callConf)
                log4net.Config.XmlConfigurator.Configure();
        }

        internal static Log4NetWrapperSection GetLog4NetWrapperConfig()
        {
            String cacheKey = "Log4NetWrapperConfiguration";
            Log4NetWrapperSection config = HttpRuntime.Cache[cacheKey] as Log4NetWrapperSection;

            if (config == null)
            {
                config = (Log4NetWrapperSection)ConfigurationManager.GetSection("Log4NetWrapper");
                if (config != null)
                    HttpRuntime.Cache.Insert(cacheKey, config, null, DateTime.Now.AddSeconds(5), TimeSpan.Zero);
            }

            return config;
        }
    }
}
