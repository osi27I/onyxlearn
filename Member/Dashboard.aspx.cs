using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

public partial class MemberDashboard : System.Web.UI.Page
{
    int uid;

    protected void Page_Load(object sender, EventArgs e)
    {
        uid = Db.UserId(User.Identity.Name);
        if (!IsPostBack)
        {
            txtName.Text = (string)Db.Scalar("SELECT FullName FROM Users WHERE UserId=@u", Db.P("@u", uid));
            txtEmailRo.Text = User.Identity.Name;
            lblName.Text = Server.HtmlEncode(txtName.Text);
            Bind();
        }
    }

    void Bind()
    {
        var t = Db.Query(
            @"SELECT c.CourseId,c.Title,c.Category,e.EnrolledAt,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=c.CourseId) AS TotalLessons,
 (SELECT COUNT(*) FROM LessonProgress p JOIN Lessons l2 ON l2.LessonId=p.LessonId WHERE p.UserId=@u AND l2.CourseId=c.CourseId) AS Done,
 (SELECT COUNT(*) FROM Questions qq WHERE qq.CourseId=c.CourseId) AS Qs,
 (SELECT MAX(CASE WHEN q.Total=0 THEN 0 ELSE 100*q.Score/q.Total END) FROM QuizAttempts q WHERE q.UserId=@u AND q.CourseId=c.CourseId) AS BestScore
 FROM Enrollments e JOIN Courses c ON c.CourseId=e.CourseId WHERE e.UserId=@u ORDER BY e.EnrolledAt DESC", Db.P("@u", uid));
        t.Columns.Add("Pct", typeof(int));
        t.Columns.Add("Completed", typeof(bool));
        int doneTotal = 0, bestSum = 0, bestCount = 0;
        foreach (DataRow r in t.Rows)
        {
            int total = Convert.ToInt32(r["TotalLessons"]), done = Convert.ToInt32(r["Done"]);
            r["Pct"] = total == 0 ? 0 : Math.Min(100, done * 100 / total);
            int best = r["BestScore"] == DBNull.Value ? -1 : Convert.ToInt32(r["BestScore"]);
            r["Completed"] = Db.IsComplete(total, done, Convert.ToInt32(r["Qs"]), best);
            doneTotal += done;
            if (r["BestScore"] != DBNull.Value) { bestSum += Convert.ToInt32(r["BestScore"]); bestCount++; }
        }
        rp.DataSource = t;
        rp.DataBind();
        lblNone.Visible = t.Rows.Count == 0;
        lblCount.Text = t.Rows.Count.ToString();
        lblDoneLessons.Text = doneTotal.ToString();
        lblAvg.Text = bestCount == 0 ? "-" : (bestSum / bestCount) + "%";
        lblQuizzes.Text = Db.Scalar("SELECT COUNT(*) FROM QuizAttempts WHERE UserId=@u", Db.P("@u", uid)).ToString();

        var q = Db.Query("SELECT TOP 10 c.Title,a.Score,a.Total,a.TakenAt,CASE WHEN a.Total=0 THEN 0 ELSE 100*a.Score/a.Total END AS Pct FROM QuizAttempts a JOIN Courses c ON c.CourseId=a.CourseId WHERE a.UserId=@u ORDER BY a.TakenAt DESC", Db.P("@u", uid));
        // the table shows the newest 5, the chart shows up to 10 with the oldest on the left
        var recent = q.Clone();
        for (int i = 0; i < q.Rows.Count && i < 5; i++) recent.ImportRow(q.Rows[i]);
        rpQuiz.DataSource = recent;
        rpQuiz.DataBind();
        rpQuiz.Visible = q.Rows.Count > 0;
        lblNoQuiz.Visible = q.Rows.Count == 0;
        pnlChart.Visible = q.Rows.Count > 0;
        if (q.Rows.Count > 0)
        {
            var chrono = q.Clone();
            for (int i = q.Rows.Count - 1; i >= 0; i--) chrono.ImportRow(q.Rows[i]);
            litChart.Text = Media.ScoreChart(chrono, Db.PassMark);
        }
        BindResume(t);
    }

    // "Continue where you left off": the next unfinished lesson, starting with the course the member worked on last.
    void BindResume(DataTable courses)
    {
        pnlResume.Visible = false;
        if (courses.Rows.Count == 0) return;
        var order = new List<int>();
        object last = Db.Scalar("SELECT TOP 1 l.CourseId FROM LessonProgress p JOIN Lessons l ON l.LessonId=p.LessonId JOIN Enrollments e ON e.CourseId=l.CourseId AND e.UserId=p.UserId WHERE p.UserId=@u ORDER BY p.CompletedAt DESC", Db.P("@u", uid));
        if (last != null && last != DBNull.Value) order.Add(Convert.ToInt32(last));
        foreach (DataRow r in courses.Rows)
        {
            int id = Convert.ToInt32(r["CourseId"]);
            if (!order.Contains(id)) order.Add(id);
        }
        foreach (int cid in order)
        {
            var nx = Db.Query(
                @"SELECT TOP 1 l.LessonId,l.Title,c.Title AS CourseTitle FROM Lessons l JOIN Courses c ON c.CourseId=l.CourseId
 WHERE l.CourseId=@c AND c.IsPublished=1 AND NOT EXISTS(SELECT 1 FROM LessonProgress p WHERE p.LessonId=l.LessonId AND p.UserId=@u)
 ORDER BY l.SortOrder,l.LessonId", Db.P("@c", cid), Db.P("@u", uid));
            if (nx.Rows.Count == 0) continue;
            pnlResume.Visible = true;
            lblResumeTitle.Text = Server.HtmlEncode((string)nx.Rows[0]["Title"]);
            lblResumeCourse.Text = Server.HtmlEncode((string)nx.Rows[0]["CourseTitle"]);
            lnkResume.Text = "Resume lesson";
            lnkResume.NavigateUrl = "~/Member/Lesson.aspx?id=" + nx.Rows[0]["LessonId"];
            return;
        }
        pnlResume.Visible = true;
        lblResumeTitle.Text = "You are all caught up";
        lblResumeCourse.Text = "Every lesson in your courses is done. Ready for something new?";
        lnkResume.Text = "Browse courses";
        lnkResume.NavigateUrl = "~/Default.aspx";
    }

    protected void rp_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "drop") return;
        Db.Exec("DELETE FROM Enrollments WHERE UserId=@u AND CourseId=@c", Db.P("@u", uid), Db.P("@c", Db.ToInt((string)e.CommandArgument, -1)));
        Bind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        Db.Exec("UPDATE Users SET FullName=@n WHERE UserId=@u", Db.P("@n", txtName.Text.Trim()), Db.P("@u", uid));
        lblName.Text = Server.HtmlEncode(txtName.Text.Trim());
        lblMsg.Text = "Profile updated.";
    }

    protected void btnPw_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        string stored = (string)Db.Scalar("SELECT PasswordHash FROM Users WHERE UserId=@u", Db.P("@u", uid));
        if (!Db.Verify(txtOld.Text, stored))
        {
            lblPwMsg.Text = "Your current password is not correct.";
            return;
        }
        Db.Exec("UPDATE Users SET PasswordHash=@h WHERE UserId=@u", Db.P("@h", Db.Hash(txtNew.Text)), Db.P("@u", uid));
        lblPwMsg.Text = "Password updated.";
    }
}
