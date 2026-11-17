using System;
using System.Globalization;
using System.Web;

/// <summary>A printable certificate. It is only shown for courses the member has really completed.</summary>
public partial class CertificatePage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int uid = Db.UserId(User.Identity.Name);
        int courseId = Db.ToInt(Request.QueryString["course"], -1);
        var c = Db.Query("SELECT Title FROM Courses WHERE CourseId=@c", Db.P("@c", courseId));
        if (c.Rows.Count == 0) throw new HttpException(404, "Course not found");
        if (!Db.CourseCompleted(uid, courseId))
        {
            Response.Redirect("~/Course.aspx?id=" + courseId + "&msg=cert");
            return;
        }

        // Date = the day the last requirement was met: the last lesson ticked, or the first passing quiz attempt.
        DateTime when = DateTime.MinValue;
        object d1 = Db.Scalar("SELECT MAX(p.CompletedAt) FROM LessonProgress p JOIN Lessons l ON l.LessonId=p.LessonId WHERE p.UserId=@u AND l.CourseId=@c", Db.P("@u", uid), Db.P("@c", courseId));
        if (d1 != null && d1 != DBNull.Value) when = Convert.ToDateTime(d1);
        object d2 = Db.Scalar("SELECT MIN(TakenAt) FROM QuizAttempts WHERE UserId=@u AND CourseId=@c AND Total>0 AND 100*Score/Total>=@p", Db.P("@u", uid), Db.P("@c", courseId), Db.P("@p", Db.PassMark));
        if (d2 != null && d2 != DBNull.Value && Convert.ToDateTime(d2) > when) when = Convert.ToDateTime(d2);
        if (when == DateTime.MinValue) when = DateTime.Now;

        object best = Db.Scalar("SELECT MAX(CASE WHEN Total=0 THEN 0 ELSE 100*Score/Total END) FROM QuizAttempts WHERE UserId=@u AND CourseId=@c", Db.P("@u", uid), Db.P("@c", courseId));

        lblName.Text = Server.HtmlEncode((string)Db.Scalar("SELECT FullName FROM Users WHERE UserId=@u", Db.P("@u", uid)));
        lblCourse.Text = Server.HtmlEncode((string)c.Rows[0]["Title"]);
        lblScore.Text = (best == null || best == DBNull.Value) ? "" : "and passed the course quiz with a best score of " + best + "%.";
        lblDate.Text = when.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        lblRef.Text = "ONYX-" + courseId.ToString("000") + "-" + uid.ToString("0000") + "-" + when.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        Page.Title = "Certificate";
    }
}
