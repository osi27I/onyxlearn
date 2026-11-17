using System;
using System.Globalization;
using System.Text;

/// <summary>
/// CSV helpers for the admin exports. Every field is quoted. Text that starts with = + - @ (or a tab / line break)
/// gets a leading apostrophe, so a spreadsheet can never run something a user typed as a formula.
/// </summary>
public static class Csv
{
    public static string Field(object value)
    {
        if (value == null || value == DBNull.Value) return "\"\"";
        if (value is DateTime) return "\"" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + "\"";
        bool numeric = value is int || value is long || value is short || value is decimal || value is double || value is float;
        string s = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (!numeric && s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0) s = "'" + s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    public static string Row(params object[] values)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(Field(values[i]));
        }
        return sb.Append("\r\n").ToString();
    }
}
