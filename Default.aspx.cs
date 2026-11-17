using System;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblCourses.Text = Db.Scalar("SELECT COUNT(*) FROM Courses WHERE IsPublished=1").ToString();
            lblLessons.Text = Db.Scalar("SELECT COUNT(*) FROM Lessons l JOIN Courses c ON c.CourseId=l.CourseId WHERE c.IsPublished=1").ToString();
            lblLearners.Text = Db.Scalar("SELECT COUNT(*) FROM Users WHERE Role='Member'").ToString();
            Bind();
        }
    }

    void Bind()
    {
        int uid = User.Identity.IsAuthenticated ? Db.UserId(User.Identity.Name) : -1;
        var t = Db.Query(
            @"SELECT c.CourseId,c.Title,c.Category,c.Level,c.Description,c.CoverUrl,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=c.CourseId) AS Lessons,
 (SELECT COUNT(*) FROM Enrollments x WHERE x.CourseId=c.CourseId) AS Learners,
 (SELECT COUNT(*) FROM Enrollments m WHERE m.CourseId=c.CourseId AND m.UserId=@u) AS Enrolled
 FROM Courses c
 WHERE c.IsPublished=1 AND (c.Title LIKE @q OR c.Category LIKE @q) AND (@lvl='' OR c.Level=@lvl)
 ORDER BY c.Title",
            Db.P("@q", "%" + txtSearch.Text.Trim() + "%"), Db.P("@lvl", ddlLevel.SelectedValue), Db.P("@u", uid));
        rp.DataSource = t;
        rp.DataBind();
        lblMsg.Text = t.Rows.Count == 0 ? "No courses match your search. Try a different word or level." : "";
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        Bind();
    }

    protected void rp_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "enroll") return;
        int cid = Db.ToInt((string)e.CommandArgument, -1);
        if (!User.Identity.IsAuthenticated)
        {
            Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(ResolveUrl("~/Course.aspx") + "?id=" + cid));
            return;
        }
        int uid = Db.UserId(User.Identity.Name);
        Db.Exec("IF NOT EXISTS(SELECT 1 FROM Enrollments WHERE UserId=@u AND CourseId=@c) AND EXISTS(SELECT 1 FROM Courses WHERE CourseId=@c AND IsPublished=1) INSERT INTO Enrollments(UserId,CourseId) VALUES(@u,@c)",
            Db.P("@u", uid), Db.P("@c", cid));
        Response.Redirect("~/Course.aspx?id=" + cid + "&enrolled=1");
    }
}
