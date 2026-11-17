using System;
using System.Collections.Generic;
using System.Data;

public partial class AdminStats : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        lblMembers.Text = Db.Scalar("SELECT COUNT(*) FROM Users WHERE Role='Member'").ToString();
        lblEnrol.Text = Db.Scalar("SELECT COUNT(*) FROM Enrollments").ToString();
        lblLessonsDone.Text = Db.Scalar("SELECT COUNT(*) FROM LessonProgress").ToString();
        lblAttempts.Text = Db.Scalar("SELECT COUNT(*) FROM QuizAttempts").ToString();

        // Who finished what: one row per enrolment, judged with the same rule the certificate uses.
        var en = Db.Query(
            @"SELECT e.CourseId,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=e.CourseId) AS Lessons,
 (SELECT COUNT(*) FROM LessonProgress p JOIN Lessons l2 ON l2.LessonId=p.LessonId WHERE p.UserId=e.UserId AND l2.CourseId=e.CourseId) AS Done,
 (SELECT COUNT(*) FROM Questions q WHERE q.CourseId=e.CourseId) AS Qs,
 (SELECT MAX(CASE WHEN a.Total=0 THEN 0 ELSE 100*a.Score/a.Total END) FROM QuizAttempts a WHERE a.UserId=e.UserId AND a.CourseId=e.CourseId) AS Best
 FROM Enrollments e");
        var completed = new Dictionary<int, int>();
        var doneByCourse = new Dictionary<int, int>();
        int certs = 0;
        foreach (DataRow r in en.Rows)
        {
            int cid = Convert.ToInt32(r["CourseId"]), lessons = Convert.ToInt32(r["Lessons"]), done = Convert.ToInt32(r["Done"]);
            int best = r["Best"] == DBNull.Value ? -1 : Convert.ToInt32(r["Best"]);
            int d; doneByCourse.TryGetValue(cid, out d); doneByCourse[cid] = d + done;
            if (Db.IsComplete(lessons, done, Convert.ToInt32(r["Qs"]), best))
            {
                int c; completed.TryGetValue(cid, out c); completed[cid] = c + 1;
                certs++;
            }
        }
        lblCerts.Text = certs.ToString();

        var t = Db.Query(
            @"SELECT c.CourseId,c.Title,
 (SELECT COUNT(*) FROM Enrollments e WHERE e.CourseId=c.CourseId) AS Enrolled,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=c.CourseId) AS Lessons,
 (SELECT COUNT(*) FROM QuizAttempts a WHERE a.CourseId=c.CourseId) AS Attempts,
 (SELECT AVG(CASE WHEN a2.Total=0 THEN 0 ELSE 100*a2.Score/a2.Total END) FROM QuizAttempts a2 WHERE a2.CourseId=c.CourseId) AS AvgScore
 FROM Courses c ORDER BY c.Title");
        t.Columns.Add("Progress", typeof(int));
        t.Columns.Add("CompletedCount", typeof(int));
        foreach (DataRow r in t.Rows)
        {
            int cid = Convert.ToInt32(r["CourseId"]), enrolled = Convert.ToInt32(r["Enrolled"]), lessons = Convert.ToInt32(r["Lessons"]);
            int d; doneByCourse.TryGetValue(cid, out d);
            r["Progress"] = (enrolled == 0 || lessons == 0) ? 0 : Math.Min(100, d * 100 / (enrolled * lessons));
            int c; completed.TryGetValue(cid, out c);
            r["CompletedCount"] = c;
            if (r["AvgScore"] != DBNull.Value) r["AvgScore"] = Convert.ToInt32(Convert.ToDouble(r["AvgScore"]));
        }
        rpCourses.DataSource = t;
        rpCourses.DataBind();
        rpCourses.Visible = t.Rows.Count > 0;
        pnlNoCourses.Visible = t.Rows.Count == 0;

        double maxEnrol = 1;
        foreach (DataRow r in t.Rows) maxEnrol = Math.Max(maxEnrol, Convert.ToDouble(r["Enrolled"]));
        litEnrol.Text = Media.HBars(t, "Title", "Enrolled", maxEnrol, "", 0, "Bar chart of enrolments per course. The same numbers are in the table above.");
        litScore.Text = Media.HBars(t, "Title", "AvgScore", 100, "%", Db.PassMark, "Bar chart of the average quiz score per course. The same numbers are in the table above.");

        var top = Db.Query(
            @"SELECT TOP 5 u.FullName,u.Email,
 (SELECT COUNT(*) FROM LessonProgress p WHERE p.UserId=u.UserId) AS Done,
 (SELECT COUNT(*) FROM QuizAttempts a WHERE a.UserId=u.UserId) AS Attempts
 FROM Users u WHERE u.Role='Member' ORDER BY Done DESC, Attempts DESC, u.FullName");
        rpTop.DataSource = top;
        rpTop.DataBind();
        rpTop.Visible = top.Rows.Count > 0;
    }
}
