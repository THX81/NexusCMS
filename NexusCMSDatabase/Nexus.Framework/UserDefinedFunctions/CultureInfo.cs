using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using Microsoft.SqlServer.Server;

/// <summary>
/// Funkce pro zjistování informací o kulturách z .NET Frameworku
/// </summary>
public partial class LegacySQLFunctions
{
    /// <summary>
    /// Vrátí všechny dostupné kultury systému a .NET Frameworku
    /// </summary>
    /// <returns>Seznam dostupných kultur</returns>
    [Microsoft.SqlServer.Server.SqlFunction(
        FillRowMethodName = "ParseCultureInfoRow",
        TableDefinition = "CultureId INT, CultureName NVARCHAR(16), CultureEnglishName NVARCHAR(64)")]
    public static System.Collections.IEnumerable GetAllCultures()
    {
        DataTable tbl = new DataTable("Cultures");
        tbl.Columns.Add("CultureId", typeof(Int32));
        tbl.Columns.Add("CultureName", typeof(String));
        tbl.Columns.Add("CultureEnglishName", typeof(String));
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        foreach(CultureInfo info in cultures)
        {
            DataRow newRow = tbl.NewRow();
            newRow[0] = info.LCID;
            newRow[1] = info.Name;
            newRow[2] = info.EnglishName;
            tbl.Rows.Add(newRow);
        }
        tbl.AcceptChanges();
        return (System.Collections.IEnumerable)tbl.Select("", "CultureEnglishName ASC");
    }
    /// <summary>
    /// Plnící metoda pro metodu <c>GetAllCultures()</c> pro plnění jednotlivých řádků daty .
    /// </summary>
    /// <param name="item">Objekt řádku</param>
    /// <param name="cultureId">Identifikátor kultury</param>
    /// <param name="cultureName">Systémový název kultury</param>
    /// <param name="cultureEnglishName">Anglický název kultury</param>
    public static void ParseCultureInfoRow(object item, out Int32 cultureId, out String cultureName, out String cultureEnglishName)
    {
        DataRow row = (DataRow)item;

        cultureId = Convert.ToInt32(row["CultureId"]);
        cultureName = row["CultureName"].ToString();
        cultureEnglishName = row["CultureEnglishName"].ToString();
    }


    /// <summary>
    /// Zjišťuje identifikátor kultury dle jejího systémového názvu.
    /// </summary>
    /// <param name="cultureName">Systémový název kultury</param>
    /// <returns>Identifikátor kultury, pokud není nalezena kultura vrací se identifikátor neutrální kultury.</returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static Int32 GetCultureId(SqlString cultureName)
    {
        return CultureInfo.GetCultureInfo((cultureName.IsNull ? String.Empty : cultureName.Value.ToString())).LCID;
    }
    /// <summary>
    /// Zjišťuje systémový název kultury dle jejího identifikátoru.
    /// </summary>
    /// <param name="cultureId">Identifikátor kultury</param>
    /// <returns>Systémový název kultury, pokud není nalezena kultura vrací se prázdný název neutrální kultury.</returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static String GetCultureName(SqlInt32 cultureId)
    {
        return CultureInfo.GetCultureInfo((cultureId.IsNull ? 127 : Convert.ToInt32(cultureId.Value))).Name;
    }
    /// <summary>
    /// Zjišťuje nativní název kultury dle jejího identifikátoru.
    /// </summary>
    /// <param name="cultureId">Identifikátor kultury</param>
    /// <returns>Nativní název kultury, pokud není nalezena kultura vrací se prázdný název neutrální kultury.</returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static String GetCultureNativeName(SqlInt32 cultureId)
    {
        return CultureInfo.GetCultureInfo((cultureId.IsNull ? 127 : Convert.ToInt32(cultureId.Value))).NativeName;
    }
    /// <summary>
    /// Zjišťuje anglický název kultury dle jejího identifikátoru.
    /// </summary>
    /// <param name="cultureId">Identifikátor kultury</param>
    /// <returns>Anglický název kultury, pokud není nalezena kultura vrací se prázdný název neutrální kultury.</returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static String GetCultureEnglishName(SqlInt32 cultureId)
    {
        return CultureInfo.GetCultureInfo((cultureId.IsNull ? 127 : Convert.ToInt32(cultureId.Value))).EnglishName;
    }
};

