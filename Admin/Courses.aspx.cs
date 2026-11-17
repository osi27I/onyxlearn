using System;
using System.Web.UI.WebControls;

public partial class AdminCourses : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) Bind();
    }

    void Bind()
    {
        gv.DataSource = Db.Query(
            @"SELECT c.CourseId,c.Title,c.Category,c.Level,c.Description,c.IsPublished,c.CoverUrl,
 (SELECT COUNT(*) FROM Lessons l WHERE l.CourseId=c.CourseId) AS Lessons,
 (SELECT COUNT(*) FROM Questions q WHERE q.CourseId=c.CourseId) AS Questions
 FROM Courses c ORDER BY c.CourseId");
        gv.DataBind();
        lblUsers.Text = Db.Scalar("SELECT COUNT(*) FROM Users").ToString();
        lblCourses.Text = Db.Scalar("SELECT COUNT(*) FROM Courses").ToString();
        lblLessons.Text = Db.Scalar("SELECT COUNT(*) FROM Lessons").ToString();
        lblEnroll.Text = Db.Scalar("SELECT COUNT(*) FROM Enrollments").ToString();
    }

    void Say(string text, bool ok)
    {
        lblMsg.CssClass = ok ? "alert ok" : "alert bad";
        lblMsg.Text = Server.HtmlEncode(text);
    }

    bool TitleTaken(string title, int exceptId)
    {
        return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Courses WHERE Title=@t AND CourseId<>@id", Db.P("@t", title), Db.P("@id", exceptId))) > 0;
    }

    protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gv.EditIndex = -1;
        gv.PageIndex = e.NewPageIndex;
        Bind();
    }

    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gv.EditIndex = e.NewEditIndex;
        Bind();
    }

    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gv.EditIndex = -1;
        Bind();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        if (!IsValid) { e.Cancel = true; return; }
        int id = (int)gv.DataKeys[e.RowIndex].Value;
        string title = ((string)(e.NewValues["Title"] ?? "")).Trim();
        string cat = ((string)(e.NewValues["Category"] ?? "")).Trim();
        string desc = ((string)(e.NewValues["Description"] ?? "")).Trim();
        string cover = ((string)(e.NewValues["CoverUrl"] ?? "")).Trim();
        if (title.Length == 0 || cat.Length == 0 || desc.Length == 0 || desc.Length > 500)
        {
            Say("Title, category and description are required (description up to 500 characters).", false);
            e.Cancel = true;
            return;
        }
        if (cover.Length > 0 && !Media.IsSafeUrl(cover))
        {
            Say("The cover URL must be a file under ~/Media/ or an http(s) link.", false);
            e.Cancel = true;
            return;
        }
        if (TitleTaken(title, id))
        {
            Say("Another course already uses that title.", false);
            e.Cancel = true;
            return;
        }
        try
        {
            Db.Exec("UPDATE Courses SET Title=@t,Category=@c,Level=@l,Description=@d,IsPublished=@p,CoverUrl=@u WHERE CourseId=@id",
                Db.P("@t", title), Db.P("@c", cat), Db.P("@l", e.NewValues["Level"]), Db.P("@d", desc),
                Db.P("@p", Convert.ToBoolean(e.NewValues["IsPublished"])), Db.P("@u", cover.Length == 0 ? null : cover), Db.P("@id", id));
            Say("Course updated.", true);
        }
        catch (System.Data.SqlClient.SqlException)
        {
            Say("The course could not be saved. Please check the values and try again.", false);
            e.Cancel = true;
            return;
        }
        gv.EditIndex = -1;
        Bind();
    }

    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Db.Exec("DELETE FROM Courses WHERE CourseId=@id", Db.P("@id", (int)gv.DataKeys[e.RowIndex].Value));
        Say("Course deleted.", true);
        gv.EditIndex = -1;
        if (gv.Rows.Count == 1 && gv.PageIndex > 0) gv.PageIndex--;
        Bind();
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        string title = txtTitle.Text.Trim(), cover = txtCover.Text.Trim();
        if (TitleTaken(title, -1)) { Say("A course with that title already exists.", false); return; }
        if (cover.Length > 0 && !Media.IsSafeUrl(cover)) { Say("The cover URL must be a file under ~/Media/ or an http(s) link.", false); return; }
        Db.Exec("INSERT INTO Courses(Title,Category,Level,Description,IsPublished,CoverUrl) VALUES(@t,@c,@l,@d,@p,@u)",
            Db.P("@t", title), Db.P("@c", txtCat.Text.Trim()), Db.P("@l", ddlLevel.SelectedValue), Db.P("@d", txtDesc.Text.Trim()),
            Db.P("@p", chkPub.Checked), Db.P("@u", cover.Length == 0 ? null : cover));
        txtTitle.Text = txtCat.Text = txtDesc.Text = txtCover.Text = "";
        Say("Course added.", true);
        Bind();
    }
}
