using System;
using System.Data;
using System.Text;
using System.Web;

/// <summary>
/// Administrator-only CSV downloads: ?what=users | results | progress. The folder is protected by web.config
/// (Admin role only). Password hashes are never exported.
/// </summary>
public partial class AdminExport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string what = (Request.QueryString["what"] ?? "").ToLowerInvariant();
        var sb = new StringBuilder();
        switch (what)
        {
            case "users":
                sb.Append(Csv.Row("User ID", "Full name", "Email", "Role", "Joined", "Enrolments"));
                foreach (DataRow r in Db.Query("SELECT u.UserId,u.FullName,u.Email,u.Role,u.CreatedAt,(SELECT COUNT(*) FROM Enrollments e WHERE e.UserId=u.UserId) AS Enrolments FROM Users u ORDER BY u.UserId").Rows)
                    sb.Append(Csv.Row(r["UserId"], r["FullName"], r["Email"], r["Role"], r["CreatedAt"], r["Enrolments"]));
                break;
            case "results":
                sb.Append(Csv.Row("Full name", "Email", "Course", "Score", "Out of", "Percent", "Taken at"));
                foreach (DataRow r in Db.Query("SELECT u.FullName,u.Email,c.Title,a.Score,a.Total,CASE WHEN a.Total=0 THEN 0 ELSE 100*a.Score/a.Total END AS Pct,a.TakenAt FROM QuizAttempts a JOIN Users u ON u.UserId=a.UserId JOIN Courses c ON c.CourseId=a.CourseId ORDER BY a.TakenAt DESC").Rows)
                    sb.Append(Csv.Row(r["FullName"], r["Email"], r["Title"], r["Score"], r["Total"], r["Pct"], r["TakenAt"]));
                break;
            case "progress":
                sb.Append(Csv.Row("Full name", "Email", "Course", "Enrolled", "Lessons done", "Lessons total", "Progress %", "Best quiz %", "Completed"));
                var t = Db.Query(
                    @"SELECT u.FullName,u.Email,c.Title,e.EnrolledAt,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=e.CourseId) AS Lessons,
 (SELECT COUNT(*) FROM LessonProgress p JOIN Lessons l2 ON l2.LessonId=p.LessonId WHERE p.UserId=e.UserId AND l2.CourseId=e.CourseId) AS Done,
 (SELECT COUNT(*) FROM Questions q WHERE q.CourseId=e.CourseId) AS Qs,
 (SELECT MAX(CASE WHEN a.Total=0 THEN 0 ELSE 100*a.Score/a.Total END) FROM QuizAttempts a WHERE a.UserId=e.UserId AND a.CourseId=e.CourseId) AS Best
 FROM Enrollments e JOIN Users u ON u.UserId=e.UserId JOIN Courses c ON c.CourseId=e.CourseId ORDER BY c.Title,u.FullName");
                foreach (DataRow r in t.Rows)
                {
                    int lessons = Convert.ToInt32(r["Lessons"]), done = Convert.ToInt32(r["Done"]);
                    int best = r["Best"] == DBNull.Value ? -1 : Convert.ToInt32(r["Best"]);
                    sb.Append(Csv.Row(r["FullName"], r["Email"], r["Title"], r["EnrolledAt"], done, lessons,
                        lessons == 0 ? 0 : Math.Min(100, done * 100 / lessons), best < 0 ? "" : (object)best,
                        Db.IsComplete(lessons, done, Convert.ToInt32(r["Qs"]), best) ? "Yes" : "No"));
                }
                break;
            default:
                throw new HttpException(404, "Unknown export");
        }
        Response.Clear();
        Response.ContentType = "text/csv";
        Response.AddHeader("Content-Disposition", "attachment; filename=onyx-" + what + "-" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.BinaryWrite(new UTF8Encoding(true).GetPreamble()); // BOM so Excel reads UTF-8 names correctly
        Response.BinaryWrite(Encoding.UTF8.GetBytes(sb.ToString()));
        Response.End();
    }
}
