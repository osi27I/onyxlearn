using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Helpers for multimedia (HTML5 img / video / audio / iframe) and for safely displaying lesson text.
/// Everything that reaches the page is HTML-encoded.
/// </summary>
public static class Media
{
    public static readonly string[] Types = { "None", "Image", "Video", "Audio", "Embed" };
    public const int MaxUploadBytes = 30 * 1024 * 1024;

    static readonly string[] EmbedPrefixes =
    {
        "https://www.youtube.com/embed/",
        "https://www.youtube-nocookie.com/embed/",
        "https://player.vimeo.com/video/"
    };

    static readonly Regex LocalPath = new Regex(@"^~/Media/[A-Za-z0-9_\-./]+$", RegexOptions.Compiled);

    /// <summary>Allowed file types for upload: returns the media type for an extension, or null if not allowed.</summary>
    public static string TypeFromExtension(string ext)
    {
        switch ((ext ?? "").ToLowerInvariant())
        {
            case ".jpg": case ".jpeg": case ".png": case ".gif": case ".webp": return "Image";
            case ".mp4": case ".webm": return "Video";
            case ".mp3": case ".ogg": case ".wav": return "Audio";
            default: return null;
        }
    }

    static string MimeFromExtension(string ext)
    {
        switch ((ext ?? "").ToLowerInvariant())
        {
            case ".mp4": return "video/mp4";
            case ".webm": return "video/webm";
            case ".mp3": return "audio/mpeg";
            case ".ogg": return "audio/ogg";
            case ".wav": return "audio/wav";
            default: return "";
        }
    }

    public static bool IsSafeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        if (url.StartsWith("~/")) return LocalPath.IsMatch(url) && !url.Contains("..");
        Uri u;
        return Uri.TryCreate(url, UriKind.Absolute, out u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsEmbedUrl(string url)
    {
        foreach (string p in EmbedPrefixes)
            if (url != null && url.StartsWith(p, StringComparison.OrdinalIgnoreCase) && IsSafeUrl(url)) return true;
        return false;
    }

    /// <summary>True when the type/url pair is consistent and safe. "None" never needs a URL.</summary>
    public static bool IsValid(string type, string url)
    {
        if (type == "None") return true;
        if (Array.IndexOf(Types, type) < 0) return false;
        if (type == "Embed") return IsEmbedUrl(url);
        return IsSafeUrl(url);
    }

    static string Resolve(string url)
    {
        return url.StartsWith("~/") ? VirtualPathUtility.ToAbsolute(url) : url;
    }

    static string A(string s)
    {
        return HttpUtility.HtmlAttributeEncode(s);
    }

    /// <summary>Returns the HTML5 element for a lesson's media. The caption is the lesson title.</summary>
    public static string Render(string type, string url, string title)
    {
        if (type == "None" || string.IsNullOrEmpty(url) || !IsValid(type, url)) return "";
        string src = Resolve(url);
        string ext = Path.GetExtension(url.Split('?')[0]);
        var sb = new StringBuilder();
        switch (type)
        {
            case "Image":
                sb.Append("<figure class=\"media\"><img src=\"").Append(A(src)).Append("\" alt=\"").Append(A(title))
                  .Append("\" loading=\"lazy\" /><figcaption>").Append(HttpUtility.HtmlEncode(title)).Append("</figcaption></figure>");
                break;
            case "Video":
                string poster = "";
                if (url.StartsWith("~/") && ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    string pv = url.Substring(0, url.Length - ext.Length) + "-poster.jpg";
                    if (File.Exists(HttpContext.Current.Server.MapPath(pv))) poster = " poster=\"" + A(Resolve(pv)) + "\"";
                }
                sb.Append("<figure class=\"media\"><video controls preload=\"metadata\"").Append(poster).Append("><source src=\"").Append(A(src))
                  .Append("\" type=\"").Append(MimeFromExtension(ext)).Append("\" />Your browser does not support the video element. <a href=\"")
                  .Append(A(src)).Append("\">Download the video</a>.</video><figcaption>Video: ").Append(HttpUtility.HtmlEncode(title)).Append("</figcaption></figure>");
                break;
            case "Audio":
                sb.Append("<figure class=\"media\"><audio controls preload=\"none\"><source src=\"").Append(A(src)).Append("\" type=\"")
                  .Append(MimeFromExtension(ext)).Append("\" />Your browser does not support the audio element. <a href=\"").Append(A(src))
                  .Append("\">Download the audio</a>.</audio><figcaption>Audio: ").Append(HttpUtility.HtmlEncode(title)).Append("</figcaption></figure>");
                break;
            case "Embed":
                sb.Append("<div class=\"embed\"><iframe src=\"").Append(A(src)).Append("\" title=\"").Append(A(title))
                  .Append("\" loading=\"lazy\" allowfullscreen referrerpolicy=\"strict-origin-when-cross-origin\"></iframe></div>");
                break;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Turns plain lesson text into safe HTML. Blank line = new paragraph. "## text" = sub-heading.
    /// A block where every line starts with "- " becomes a bulleted list. All text is HTML-encoded.
    /// </summary>
    public static string FormatBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return "";
        var sb = new StringBuilder();
        string[] blocks = Regex.Split(body.Replace("\r\n", "\n"), "\n\\s*\n");
        foreach (string raw in blocks)
        {
            string blk = raw.Trim();
            if (blk.Length == 0) continue;
            if (blk.StartsWith("## "))
            {
                sb.Append("<h2>").Append(HttpUtility.HtmlEncode(blk.Substring(3).Trim())).Append("</h2>");
                continue;
            }
            string[] lines = blk.Split('\n');
            bool list = true;
            foreach (string l in lines) if (!l.TrimStart().StartsWith("- ")) { list = false; break; }
            if (list)
            {
                sb.Append("<ul>");
                foreach (string l in lines) sb.Append("<li>").Append(HttpUtility.HtmlEncode(l.TrimStart().Substring(2).Trim())).Append("</li>");
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<p>").Append(HttpUtility.HtmlEncode(blk).Replace("\n", "<br />")).Append("</p>");
            }
        }
        return sb.ToString();
    }

    /// <summary>"About 3 min read" (180 words a minute), with "Video lesson" / "Audio lesson" in front when there is media.</summary>
    public static string ReadingTime(string body, string mediaType)
    {
        int words = string.IsNullOrEmpty(body) ? 0 : body.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        int mins = Math.Max(1, (words + 179) / 180);
        string kind = mediaType == "Video" ? "Video lesson &middot; " : mediaType == "Audio" ? "Audio lesson &middot; " : "";
        return kind + "about " + mins + " min read";
    }

    /// <summary>Progress ring as inline SVG (no images, no scripts). cls is "pass", "fail" or "prog".</summary>
    public static string Ring(int pct, string cls, string label)
    {
        if (pct < 0) pct = 0;
        if (pct > 100) pct = 100;
        double circ = 2 * Math.PI * 52;
        string dash = (circ * pct / 100).ToString("0.##", CultureInfo.InvariantCulture);
        string full = circ.ToString("0.##", CultureInfo.InvariantCulture);
        return "<svg class=\"ring " + cls + "\" viewBox=\"0 0 120 120\" role=\"img\" aria-label=\"" + HttpUtility.HtmlAttributeEncode(label) + "\">"
            + "<circle class=\"trk\" cx=\"60\" cy=\"60\" r=\"52\" />"
            + (pct == 0 ? "" : "<circle class=\"val\" cx=\"60\" cy=\"60\" r=\"52\" stroke-dasharray=\"" + dash + " " + full + "\" transform=\"rotate(-90 60 60)\" />")
            + "<text x=\"60\" y=\"68\" text-anchor=\"middle\">" + pct + "%</text></svg>";
    }

    static string F(double d)
    {
        return d.ToString("0.#", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Bar chart of quiz scores as inline SVG, oldest on the left, with the pass mark as a dashed line.
    /// Expects the columns Pct (int), Title (string) and TakenAt (DateTime). Every piece of text is HTML-encoded.
    /// </summary>
    public static string ScoreChart(DataTable t, int passMark)
    {
        int n = t.Rows.Count;
        if (n == 0) return "";
        const int W = 640, H = 230, padL = 38, padR = 78, padT = 20, padB = 44;
        double plotW = W - padL - padR, plotH = H - padT - padB, slot = plotW / n, bw = Math.Min(56, slot * 0.62);
        var sb = new StringBuilder();
        sb.Append("<svg class=\"chart\" viewBox=\"0 0 " + W + " " + H + "\" role=\"img\" aria-label=\"Bar chart of your last " + n + " quiz scores. The same numbers are in the table below.\">");
        foreach (int g in new[] { 0, 50, 100 })
        {
            double y = padT + plotH * (1 - g / 100.0);
            sb.Append("<line class=\"grid\" x1=\"" + padL + "\" x2=\"" + (W - padR) + "\" y1=\"" + F(y) + "\" y2=\"" + F(y) + "\" />");
            sb.Append("<text class=\"ax\" x=\"" + (padL - 6) + "\" y=\"" + F(y + 4) + "\" text-anchor=\"end\">" + g + "%</text>");
        }
        double py = padT + plotH * (1 - passMark / 100.0);
        sb.Append("<line class=\"passline\" x1=\"" + padL + "\" x2=\"" + (W - padR) + "\" y1=\"" + F(py) + "\" y2=\"" + F(py) + "\" />");
        sb.Append("<text class=\"passtxt\" x=\"" + (W - padR + 8) + "\" y=\"" + F(py + 4) + "\" text-anchor=\"start\">Pass " + passMark + "%</text>");
        for (int i = 0; i < n; i++)
        {
            DataRow r = t.Rows[i];
            int pct = Math.Max(0, Math.Min(100, Convert.ToInt32(r["Pct"])));
            double h = plotH * pct / 100.0, x = padL + slot * i + (slot - bw) / 2, y = padT + plotH - h;
            string when = Convert.ToDateTime(r["TakenAt"]).ToString("d MMM", CultureInfo.InvariantCulture);
            sb.Append("<rect class=\"bar " + (pct >= passMark ? "pass" : "fail") + "\" x=\"" + F(x) + "\" y=\"" + F(y) + "\" width=\"" + F(bw) + "\" height=\"" + F(Math.Max(h, 1)) + "\" rx=\"6\">"
                + "<title>" + HttpUtility.HtmlEncode(Convert.ToString(r["Title"]) + ": " + pct + "% on " + when) + "</title></rect>");
            sb.Append("<text class=\"val\" x=\"" + F(x + bw / 2) + "\" y=\"" + F(y - 6) + "\" text-anchor=\"middle\">" + pct + "%</text>");
            sb.Append("<text class=\"ax\" x=\"" + F(x + bw / 2) + "\" y=\"" + (H - 22) + "\" text-anchor=\"middle\">" + when + "</text>");
        }
        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Horizontal bar chart as inline SVG: one bar per row, label on the left, value at the end of the bar.
    /// Values are drawn against max (a mark line is drawn at markAt when it is above 0). All text is HTML-encoded.
    /// </summary>
    public static string HBars(DataTable t, string labelCol, string valueCol, double max, string suffix, double markAt, string ariaLabel)
    {
        int n = t.Rows.Count;
        if (n == 0) return "";
        if (max <= 0) max = 1;
        const int W = 480, rowH = 36, padT = 8, labelW = 168, valueW = 52;
        double barMax = W - labelW - valueW;
        int H = padT * 2 + rowH * n;
        var sb = new StringBuilder();
        sb.Append("<svg class=\"chart hbars\" viewBox=\"0 0 " + W + " " + H + "\" role=\"img\" aria-label=\"" + HttpUtility.HtmlAttributeEncode(ariaLabel) + "\">");
        for (int i = 0; i < n; i++)
        {
            DataRow r = t.Rows[i];
            string label = Convert.ToString(r[labelCol]);
            bool cut = label.Length > 22;
            string shown = cut ? label.Substring(0, 21) : label;
            double v = r[valueCol] == DBNull.Value ? 0 : Convert.ToDouble(r[valueCol], CultureInfo.InvariantCulture);
            double w = Math.Max(0, Math.Min(1, v / max)) * barMax, y = padT + i * rowH;
            sb.Append("<text class=\"ax\" x=\"" + (labelW - 10) + "\" y=\"" + F(y + rowH / 2 + 4) + "\" text-anchor=\"end\">" + HttpUtility.HtmlEncode(shown) + (cut ? "&hellip;" : "") + "<title>" + HttpUtility.HtmlEncode(label) + "</title></text>");
            sb.Append("<rect class=\"track\" x=\"" + labelW + "\" y=\"" + F(y + 7) + "\" width=\"" + F(barMax) + "\" height=\"" + (rowH - 14) + "\" rx=\"7\" />");
            if (w > 0) sb.Append("<rect class=\"bar pass\" x=\"" + labelW + "\" y=\"" + F(y + 7) + "\" width=\"" + F(w) + "\" height=\"" + (rowH - 14) + "\" rx=\"7\" />");
            sb.Append("<text class=\"val\" x=\"" + F(labelW + barMax + 10) + "\" y=\"" + F(y + rowH / 2 + 4) + "\">" + F(v) + HttpUtility.HtmlEncode(suffix) + "</text>");
        }
        if (markAt > 0)
        {
            double mx = labelW + Math.Min(1, markAt / max) * barMax;
            sb.Append("<line class=\"passline\" x1=\"" + F(mx) + "\" x2=\"" + F(mx) + "\" y1=\"" + (padT - 2) + "\" y2=\"" + (H - padT + 2) + "\" />");
        }
        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>"1 lesson", "3 lessons".</summary>
    public static string Plural(int n, string word)
    {
        return n + " " + word + (n == 1 ? "" : "s");
    }

    public static string CoverOrDefault(object cover)
    {
        string c = cover as string;
        return A(Resolve(string.IsNullOrEmpty(c) || !IsSafeUrl(c) ? "~/Media/covers/default.svg" : c));
    }
}
