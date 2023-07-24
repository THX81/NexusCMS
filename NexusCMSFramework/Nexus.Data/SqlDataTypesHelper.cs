using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Nexus.Data
{
    /// <summary>
    /// Pomocná třída pro porovnávání a převody datových typů mezi .NET a DB platformami.
    /// </summary>
    internal static class SqlDataTypesHelper
    {
        /// <summary>
        /// Předanou hodnotu upraví tak aby byla použitelná pro SQL dotaz.
        /// </summary>
        /// <param name="dbType">Typ databáze.</param>
        /// <param name="type">Datový typ</param>
        /// <param name="value">Hodnota která se má použít v SQL příkazu. Pokud se jedná o hodnotu tak se v případě nutnosti obalí apostrofy nebo upraví desedinný odděovač.</param>
        /// <param name="isValue">Indikuje zda parametr <paramref name="value"/> obsahuje opravdovou hodnotu nebo jen název nějaké funkce či příkazu.</param>
        internal static String GetSqlValueRepresentation(Type type, Object value, bool isValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetSqlValueRepresentation(" + type + ", " + value + ", " + isValue + ")", System.Reflection.MethodBase.GetCurrentMethod());
            if (value == null || value == DBNull.Value)
            {
                if (type == typeof(String))
                    return "''";
                else
                    return "NULL";
            }

            if (!isValue) return value.ToString();

            Decimal decimalValue = 0;
            Int32 int32Value = 0;
            Int64 int64Value = 0;
            DateTime dateTimeValue = new DateTime(1753, 1, 1);
            TimeSpan timeSpanValue = new TimeSpan(0, 0, 0);
            if (TryConvertToDecimal(value, out decimalValue) && (type == typeof(Decimal) || type == typeof(Single) || type == typeof(Double)))
                return decimalValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (TryConvertToInt32(value, out int32Value) && type == typeof(Int32))
                return int32Value.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (TryConvertToInt64(value, out int64Value) && type == typeof(Int64))
                return int64Value.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (TryConvertToDateTime(value, out dateTimeValue) && type == typeof(DateTime))
                return "'" + dateTimeValue.ToString(CultureInfo.InvariantCulture.NumberFormat) + "'";

            if (TryConvertToTimeSpan(value, out timeSpanValue) && type == typeof(DateTime))
                return "'" + timeSpanValue.ToString() + "'";

            return "'" + value +"'";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static String GetSqlDefaultValue(Type type)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetDefaultValueForSqlNull(" + type + ")", System.Reflection.MethodBase.GetCurrentMethod());

            if (type == typeof(String))
                return "''";
            if (type == typeof(Int16?) || type == typeof(Int32?) || type == typeof(Int64?) || type == typeof(Decimal?) || type == typeof(Single?) || type == typeof(Double?))
                return "NULL";
            if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) || type == typeof(Decimal) || type == typeof(Single) || type == typeof(Double))
                return "0";
            if (type == typeof(DateTime))
                return new DateTime(1753, 1, 1).ToString();
            if (type == typeof(TimeSpan))
                return new TimeSpan(0, 0, 0).ToString();
            if (type == typeof(Guid))
                return "newid()";

            return "NULL";
        }

        internal static Boolean TryConvertToDecimal(Object value, out Decimal convertedValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TryConvertToDecimal(" + value + ", out convertedValue)", System.Reflection.MethodBase.GetCurrentMethod());
            convertedValue = 0;

            if (value == null || value == DBNull.Value) return false;
            if (Decimal.TryParse(value.ToString(), out convertedValue)) return true;
            if (Decimal.TryParse(value.ToString(), NumberStyles.Float | NumberStyles.Currency, Thread.CurrentThread.CurrentCulture.NumberFormat, out convertedValue)) return true;
            if (Decimal.TryParse(value.ToString(), NumberStyles.Float | NumberStyles.Currency, CultureInfo.InvariantCulture.NumberFormat, out convertedValue)) return true;

            return false;
        }

        internal static Boolean TryConvertToInt32(Object value, out Int32 convertedValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TryConvertToInt32(" + value + ", out convertedValue)", System.Reflection.MethodBase.GetCurrentMethod());
            convertedValue = 0;

            if (value == null || value == DBNull.Value) return false;
            if (Int32.TryParse(value.ToString(), out convertedValue)) return true;
            if (Int32.TryParse(value.ToString(), NumberStyles.Integer | NumberStyles.Number, Thread.CurrentThread.CurrentCulture.NumberFormat, out convertedValue)) return true;
            if (Int32.TryParse(value.ToString(), NumberStyles.Integer | NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out convertedValue)) return true;

            return false;
        }

        internal static Boolean TryConvertToInt64(Object value, out Int64 convertedValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TryConvertToInt64(" + value + ", out convertedValue)", System.Reflection.MethodBase.GetCurrentMethod());
            convertedValue = 0;

            if (value == null || value == DBNull.Value) return false;
            if (Int64.TryParse(value.ToString(), out convertedValue)) return true;
            if (Int64.TryParse(value.ToString(), NumberStyles.Integer | NumberStyles.Number, Thread.CurrentThread.CurrentCulture.NumberFormat, out convertedValue)) return true;
            if (Int64.TryParse(value.ToString(), NumberStyles.Integer | NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out convertedValue)) return true;

            return false;
        }

        internal static Boolean TryConvertToDateTime(Object value, out DateTime convertedValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TryConvertToDateTime(" + value + ", out convertedValue)", System.Reflection.MethodBase.GetCurrentMethod());
            convertedValue = new DateTime(1753, 1, 1);

            if (value == null || value == DBNull.Value) return false;
            if (DateTime.TryParse(value.ToString(), out convertedValue)) return true;
            if (DateTime.TryParse(value.ToString(), Thread.CurrentThread.CurrentCulture.DateTimeFormat, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces, out convertedValue)) return true;
            if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces, out convertedValue)) return true;

            return false;
        }

        internal static Boolean TryConvertToTimeSpan(Object value, out TimeSpan convertedValue)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("TryConvertToTimeSpan(" + value + ", out convertedValue)", System.Reflection.MethodBase.GetCurrentMethod());
            convertedValue = new TimeSpan(0, 0, 0);

            if (value == null || value == DBNull.Value) return false;
            if (TimeSpan.TryParse(value.ToString(), out convertedValue)) return true;

            return false;
        }



        /// <summary>
        /// Vytvoří objekt pro uchovavani informaci o .NET typu a korespondujícím SQL typu.
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="length"></param>
        /// <param name="precision"></param>
        /// <returns>Objekt obsahujici informace o .NET a SQL typu.</returns>
        internal static SqlTypeInfo GetSqlTypeInfo(Type systemType, Int32? length, Int32? precision)
        {
            Nexus.Diagnostics.Log4NetWrapper.Info("GetSqlTypeInfo(" + systemType + ", " + length + ", " + precision + ")", System.Reflection.MethodBase.GetCurrentMethod());

            string typeName = systemType.ToString();

            bool nullable = false;
            if (systemType.ToString().StartsWith("System.Nullable`1["))
            {
                nullable = true;
                typeName = typeName.Remove(0, 18);
                typeName = typeName.Remove(typeName.Length - 1);
            }

            switch (typeName)
            {
                case "System.Byte":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "tinyint", String.Empty, String.Empty);
                case "System.SByte":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "smallint", String.Empty, String.Empty);
                case "System.Byte[]":
                    return new SqlTypeInfo(systemType, nullable, false, String.Empty, "varbinary(max)", String.Empty, String.Empty);
                case "System.Int16":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "smallint", String.Empty, String.Empty);
                case "System.Int32":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "int", String.Empty, String.Empty);
                case "System.Int64":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "bigint", String.Empty, String.Empty);
                case "System.UInt16":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "int", String.Empty, String.Empty);
                case "System.UInt32":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "bigint", String.Empty, String.Empty);
                case "System.UInt64":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "bigint", String.Empty, String.Empty);// potencionální problém s přetečením
                case "System.Decimal":
                    if (length.HasValue)
                        if (length.Value > 38)
                            throw new ArgumentOutOfRangeException("Maximum length of type \"Decimal\" is 38!");
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "decimal", (length.HasValue ? length.Value.ToString() : "38"), (precision.HasValue ? precision.Value.ToString() : "9"));// potencionální problém s přetečením
                case "System.Double":
                    throw new NotSupportedException("Type \"Double\" is not supported!");
                case "System.float":
                    throw new NotSupportedException("Type \"float\" is not supported!");
                case "System.DateTime":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "datetime", String.Empty, String.Empty);
                case "System.TimeSpan":
                    return new SqlTypeInfo(systemType, nullable, true, String.Empty, "timestamp", String.Empty, String.Empty);
                case "System.String":
                    return new SqlTypeInfo(systemType, nullable, false, String.Empty, "nvarchar", (length.HasValue ? length.Value.ToString() : "max"), String.Empty);
                default:
                    if (systemType.GetNestedType("XmlNode") != null || systemType == typeof(XmlNode))
                        return new SqlTypeInfo(systemType, nullable, false, String.Empty, "xml", String.Empty, String.Empty);
                    else
                        return new SqlTypeInfo(systemType, nullable, false, String.Empty, "nvarchar", "4000", String.Empty);
            }
        }
    }

    /// <summary>
    /// Uchovává informace o SQL typu a .NET typu.
    /// </summary>
    internal class SqlTypeInfo
    {
        private Type netType;
        private bool isNullable;
        private bool isIndexable;
        private String sqlDefaultValue;
        private String sqlTypeName;
        private String sqlTypeLength;
        private String sqlTypePrecision;

        internal SqlTypeInfo(Type netType, bool isNullable, bool isIndexable, String sqlDefaultValue, String sqlTypeName, String sqlTypeLength, String sqlTypePrecision)
        {
            this.netType = netType;
            this.isNullable = isNullable;
            this.isIndexable = isIndexable;
            this.sqlDefaultValue = sqlDefaultValue;
            this.sqlTypeName = sqlTypeName;
            this.sqlTypeLength = sqlTypeLength;
            this.sqlTypePrecision = sqlTypePrecision;
        }

        internal Type NetType
        {
            get { return this.netType; }
        }
        internal bool IsNullable
        {
            get { return this.isNullable; }
        }
        internal bool IsIndexable
        {
            get { return this.isIndexable; }
        }
        internal String SqlDefaultValue
        {
            get { return this.sqlDefaultValue; }
        }
        internal String SqlTypeName
        {
            get { return this.sqlTypeName; }
        }
        internal String SqlTypeLength
        {
            get { return this.sqlTypeLength; }
        }
        internal String SqlTypePrecision
        {
            get { return this.sqlTypePrecision; }
        }
    }
}
