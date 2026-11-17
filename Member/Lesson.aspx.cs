using System;
using System.Data;
using System.Web;

public partial class LessonPage : System.Web.UI.Page
{
    int uid, lessonId, courseId, idx;
    DataTable outline;

    protected void Page_Load(object sender, EventArgs e)
    {
        uid = Db.UserId(User.Identity.Name);
        lessonId = Db.ToInt(Request.QueryString["id"], -1);
        var t = Db.Query("SELECT l.LessonId,l.CourseId,l.Title,l.Body,l.MediaType,l.MediaUrl,l.SortOrder,c.Title AS CourseTitle,c.IsPublished FROM Lessons l JOIN Courses c ON c.CourseId=l.CourseId WHERE l.LessonId=@l", Db.P("@l", lessonId));
        if (t.Rows.Count == 0) throw new HttpException(404, "Lesson not found");
        var r = t.Rows[0];
        courseId = (int)r["CourseId"];
        bool admin = User.IsInRole("Admin");
        if (!admin && (!(bool)r["IsPublished"] || !Db.IsEnrolled(uid, courseId)))
        {
            Response.Redirect("~/Course.aspx?id=" + courseId + "&msg=enrol");
            return;
        }

        Page.Title = Server.HtmlEncode((string)r["Title"]);
        if (IsPostBack) return;

        string body = (string)r["Body"];
        lblTitle.Text = Server.HtmlEncode((string)r["Title"]);
        lnkCourse.Text = Server.HtmlEncode((string)r["CourseTitle"]);
        lnkCourse.NavigateUrl = "~/Course.aspx?id=" + courseId;
        litMedia.Text = Media.Render((string)r["MediaType"], r["MediaUrl"] as string, (string)r["Title"]);
        litBody.Text = Media.FormatBody(body);
        lblRead.Text = Media.ReadingTime(body, (string)r["MediaType"]);

        bool quiz = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Questions WHERE CourseId=@c", Db.P("@c", courseId))) > 0;
        liQuiz.Visible = quiz;
        lnkQuizSide.NavigateUrl = "~/Member/Quiz.aspx?course=" + courseId;

        BindOutline();
        lblPos.Text = "Lesson " + (idx + 1) + " of " + outline.Rows.Count;
        lnkPrev.Visible = idx > 0;
        if (idx > 0) lnkPrev.NavigateUrl = "~/Member/Lesson.aspx?id=" + outline.Rows[idx - 1]["LessonId"];
        if (idx < outline.Rows.Count - 1)
        {
            lnkNext.NavigateUrl = "~/Member/Lesson.aspx?id=" + outline.Rows[idx + 1]["LessonId"];
        }
        else
        {
            lnkNext.Text = quiz ? "Take the quiz &rarr;" : "Back to course &rarr;";
            lnkNext.NavigateUrl = quiz ? "~/Member/Quiz.aspx?course=" + courseId : "~/Course.aspx?id=" + courseId;
        }
        ShowState();
    }

    // Course outline (sidebar): every lesson of this course in order, with a tick for the ones the member finished.
    void BindOutline()
    {
        outline = Db.Query(
            @"SELECT l.LessonId,l.Title,
 (SELECT COUNT(*) FROM LessonProgress p WHERE p.LessonId=l.LessonId AND p.UserId=@u) AS Done
 FROM Lessons l WHERE l.CourseId=@c ORDER BY l.SortOrder,l.LessonId",
            Db.P("@u", uid), Db.P("@c", courseId));
        outline.Columns.Add("Num", typeof(int));
        outline.Columns.Add("Cur", typeof(bool));
        int done = 0;
        for (int i = 0; i < outline.Rows.Count; i++)
        {
            bool cur = Convert.ToInt32(outline.Rows[i]["LessonId"]) == lessonId;
            outline.Rows[i]["Num"] = i + 1;
            outline.Rows[i]["Cur"] = cur;
            if (cur) idx = i;
            if (Convert.ToInt32(outline.Rows[i]["Done"]) > 0) done++;
        }
        rpOutline.DataSource = outline;
        rpOutline.DataBind();
        int total = outline.Rows.Count;
        barFill.Style["width"] = (total == 0 ? 0 : 100 * done / total) + "%";
        lblProg.Text = done + " of " + Media.Plural(total, "lesson") + " completed";
    }

    bool IsDone()
    {
        return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM LessonProgress WHERE UserId=@u AND LessonId=@l", Db.P("@u", uid), Db.P("@l", lessonId))) > 0;
    }

    void ShowState()
    {
        bool done = IsDone();
        lblDone.Visible = done;
        btnDone.Text = done ? "Mark as not completed" : "Mark as completed";
    }

    // Toggles progress: inserts a LessonProgress row, or deletes it again.
    protected void btnDone_Click(object sender, EventArgs e)
    {
        if (IsDone())
        {
            Db.Exec("DELETE FROM LessonProgress WHERE UserId=@u AND LessonId=@l", Db.P("@u", uid), Db.P("@l", lessonId));
            lblMsg.Text = "Progress removed.";
        }
        else
        {
            Db.Exec("INSERT INTO LessonProgress(UserId,LessonId) VALUES(@u,@l)", Db.P("@u", uid), Db.P("@l", lessonId));
            lblMsg.Text = "Nice work! Progress saved.";
        }
        ShowState();
        BindOutline(); // refresh the ticks and the progress bar in the sidebar
    }
}
