using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class CoursePage : System.Web.UI.Page
{
    int courseId;
    int uid = -1;
    bool isAdmin;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Browsers post new lines as CR+LF; count them as one character so the 500 limit matches the counter.
        if (IsPostBack) txtComment.Text = txtComment.Text.Replace("\r\n", "\n");
        courseId = Db.ToInt(Request.QueryString["id"], -1);
        isAdmin = User.IsInRole("Admin");
        if (User.Identity.IsAuthenticated) uid = Db.UserId(User.Identity.Name);

        var c = Db.Query("SELECT CourseId,Title,Category,Level,Description,IsPublished,CoverUrl FROM Courses WHERE CourseId=@c", Db.P("@c", courseId));
        if (c.Rows.Count == 0 || (!(bool)c.Rows[0]["IsPublished"] && !isAdmin))
            throw new HttpException(404, "Course not found");
        var r = c.Rows[0];
        Page.Title = Server.HtmlEncode((string)r["Title"]);

        if (!IsPostBack)
        {
            lblCat.Text = Server.HtmlEncode((string)r["Category"] + " | " + (string)r["Level"]);
            lblTitle.Text = Server.HtmlEncode((string)r["Title"]);
            lblDesc.Text = Server.HtmlEncode((string)r["Description"]);
            imgCover.Src = HttpUtility.HtmlDecode(Media.CoverOrDefault(r["CoverUrl"]));
            if (Request.QueryString["enrolled"] == "1") lblFlash.Text = "You are now enrolled. Start with the first lesson below.";
            else if (Request.QueryString["msg"] == "enrol") { lblFlash.CssClass = "alert"; lblFlash.Text = "Enrol in this course to open its lessons and quiz."; }
            else if (Request.QueryString["msg"] == "cert") { lblFlash.CssClass = "alert"; lblFlash.Text = "Finish every lesson and pass the quiz to unlock your certificate."; }
            BindAll();
        }
    }

    // Shows or hides the delete link: authors can delete their own comments and admins can delete any.
    protected bool CanDelete(int commentUserId)
    {
        return isAdmin || (uid > 0 && commentUserId == uid);
    }

    void BindAll()
    {
        bool enrolled = uid > 0 && Db.IsEnrolled(uid, courseId);
        int lessons = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Lessons WHERE CourseId=@c", Db.P("@c", courseId)));
        int questions = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Questions WHERE CourseId=@c", Db.P("@c", courseId)));
        int learners = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Enrollments WHERE CourseId=@c", Db.P("@c", courseId)));
        lblFacts.Text = Media.Plural(lessons, "lesson") + " | " + Media.Plural(questions, "quiz question") + " | " + Media.Plural(learners, "learner");

        btnEnrol.Visible = !enrolled;
        lnkQuiz.Visible = enrolled && questions > 0;
        lnkQuiz.NavigateUrl = "~/Member/Quiz.aspx?course=" + courseId;

        var t = Db.Query(
            @"SELECT l.LessonId,l.Title,l.MediaType,l.SortOrder,
 CASE WHEN EXISTS(SELECT 1 FROM LessonProgress p WHERE p.LessonId=l.LessonId AND p.UserId=@u) THEN 1 ELSE 0 END AS Done
 FROM Lessons l WHERE l.CourseId=@c ORDER BY l.SortOrder,l.LessonId", Db.P("@c", courseId), Db.P("@u", uid));
        rpLessons.DataSource = t;
        rpLessons.DataBind();
        lblNoLessons.Visible = t.Rows.Count == 0;

        lnkCert.Visible = enrolled && Db.CourseCompleted(uid, courseId);
        lnkCert.NavigateUrl = "~/Member/Certificate.aspx?course=" + courseId;
        lnkStart.Visible = false;
        pnlProgress.Visible = enrolled && t.Rows.Count > 0;
        if (enrolled && t.Rows.Count > 0)
        {
            int done = 0, next = -1;
            foreach (DataRow row in t.Rows)
            {
                if ((int)row["Done"] == 1) done++;
                else if (next < 0) next = (int)row["LessonId"];
            }
            int pct = done * 100 / t.Rows.Count;
            barProgress.Style["width"] = pct + "%";
            lblProgress.Text = done + " of " + Media.Plural(t.Rows.Count, "lesson") + " completed (" + pct + "%)";
            lnkStart.Visible = next > 0;
            lnkStart.NavigateUrl = "~/Member/Lesson.aspx?id=" + next;
            lnkStart.Text = done == 0 ? "Start learning" : "Continue learning";
        }

        var cm = Db.Query("SELECT m.CommentId,m.UserId,m.Body,m.PostedAt,u.FullName FROM CourseComments m JOIN Users u ON u.UserId=m.UserId WHERE m.CourseId=@c ORDER BY m.PostedAt DESC", Db.P("@c", courseId));
        rpComments.DataSource = cm;
        rpComments.DataBind();
        lblNoComments.Visible = cm.Rows.Count == 0;

        pnlPost.Visible = uid > 0;
        pnlGuest.Visible = uid <= 0;
    }

    protected void btnEnrol_Click(object sender, EventArgs e)
    {
        if (uid <= 0)
        {
            Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl));
            return;
        }
        Db.Exec("IF NOT EXISTS(SELECT 1 FROM Enrollments WHERE UserId=@u AND CourseId=@c) INSERT INTO Enrollments(UserId,CourseId) VALUES(@u,@c)",
            Db.P("@u", uid), Db.P("@c", courseId));
        Response.Redirect("~/Course.aspx?id=" + courseId + "&enrolled=1");
    }

    protected void btnPost_Click(object sender, EventArgs e)
    {
        if (!IsValid || uid <= 0) return;
        string body = txtComment.Text.Trim();
        if (body.Length < 2) return;
        Db.Exec("INSERT INTO CourseComments(CourseId,UserId,Body) VALUES(@c,@u,@b)",
            Db.P("@c", courseId), Db.P("@u", uid), Db.P("@b", body));
        txtComment.Text = "";
        lblMsg.Text = "Comment posted.";
        BindAll();
    }

    protected void rpComments_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "del") return;
        int id = Db.ToInt((string)e.CommandArgument, -1);
        Db.Exec("DELETE FROM CourseComments WHERE CommentId=@id AND CourseId=@c AND (UserId=@u OR @admin=1)",
            Db.P("@id", id), Db.P("@c", courseId), Db.P("@u", uid), Db.P("@admin", isAdmin ? 1 : 0));
        BindAll();
    }
}
