using System;
using System.IO;
using System.Web.UI.WebControls;

public partial class AdminLessons : System.Web.UI.Page
{
    int CourseId { get { return Db.ToInt(ddlCourse.SelectedValue, -1); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        ddlCourse.DataSource = Db.Query("SELECT CourseId,Title FROM Courses ORDER BY Title");
        ddlCourse.DataBind();
        string pick = Request.QueryString["course"];
        if (pick != null && ddlCourse.Items.FindByValue(pick) != null) ddlCourse.SelectedValue = pick;
        Bind();
    }

    protected static string Short(object text, int n)
    {
        string s = text as string ?? "";
        s = s.Replace("\r", " ").Replace("\n", " ");
        return s.Length <= n ? s : s.Substring(0, n) + "...";
    }

    void Bind()
    {
        gv.DataSource = Db.Query("SELECT LessonId,Title,Body,MediaType,MediaUrl,SortOrder FROM Lessons WHERE CourseId=@c ORDER BY SortOrder,LessonId", Db.P("@c", CourseId));
        gv.DataBind();
        if (!IsPostBack || txtOrder.Text.Length == 0)
        {
            object max = Db.Scalar("SELECT MAX(SortOrder) FROM Lessons WHERE CourseId=@c", Db.P("@c", CourseId));
            txtOrder.Text = ((max == null || max == DBNull.Value) ? 1 : Convert.ToInt32(max) + 1).ToString();
        }
    }

    void Say(string text, bool ok)
    {
        lblMsg.CssClass = ok ? "alert ok" : "alert bad";
        lblMsg.Text = Server.HtmlEncode(text);
    }

    protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
    {
        gv.EditIndex = -1;
        gv.PageIndex = 0;
        txtOrder.Text = "";
        Bind();
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
        string body = ((string)(e.NewValues["Body"] ?? "")).Trim();
        string type = (string)(e.NewValues["MediaType"] ?? "None");
        string url = ((string)(e.NewValues["MediaUrl"] ?? "")).Trim();
        int order = Db.ToInt(((string)(e.NewValues["SortOrder"] ?? "")).Trim(), -1);
        if (title.Length == 0 || title.Length > 150 || body.Length == 0 || order < 1 || order > 999)
        {
            Say("Title (up to 150 characters), lesson text and an order from 1 to 999 are required.", false);
            e.Cancel = true;
            return;
        }
        if (type == "None") url = "";
        if (!Media.IsValid(type, url))
        {
            Say("That media URL is not valid for the selected type. Use a file under ~/Media/ or an http(s) link (YouTube/Vimeo embed links for Embed).", false);
            e.Cancel = true;
            return;
        }
        Db.Exec("UPDATE Lessons SET Title=@t,Body=@b,MediaType=@mt,MediaUrl=@mu,SortOrder=@o WHERE LessonId=@id",
            Db.P("@t", title), Db.P("@b", body), Db.P("@mt", type), Db.P("@mu", url.Length == 0 ? null : url), Db.P("@o", order), Db.P("@id", id));
        Say("Lesson updated.", true);
        gv.EditIndex = -1;
        Bind();
    }

    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int id = (int)gv.DataKeys[e.RowIndex].Value;
        string url = Db.Scalar("SELECT MediaUrl FROM Lessons WHERE LessonId=@id", Db.P("@id", id)) as string;
        Db.Exec("DELETE FROM Lessons WHERE LessonId=@id", Db.P("@id", id));
        RemoveUploadIfUnused(url);
        Say("Lesson deleted.", true);
        gv.EditIndex = -1;
        if (gv.Rows.Count == 1 && gv.PageIndex > 0) gv.PageIndex--;
        Bind();
    }

    // Deletes an uploaded file from disk once no lesson refers to it any more.
    void RemoveUploadIfUnused(string url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("~/Media/uploads/") || !Media.IsSafeUrl(url)) return;
        if (Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Lessons WHERE MediaUrl=@u", Db.P("@u", url))) > 0) return;
        try
        {
            string path = Server.MapPath(url);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        if (CourseId < 0) { Say("Create a course first.", false); return; }
        string type = ddlType.SelectedValue, url = txtUrl.Text.Trim();
        string saved = null;

        if (fu.HasFile)
        {
            string ext = Path.GetExtension(fu.FileName).ToLowerInvariant();
            string detected = Media.TypeFromExtension(ext);
            if (detected == null) { Say("That file type is not allowed. Use JPG, PNG, GIF, WEBP, MP4, WEBM, MP3, OGG or WAV.", false); return; }
            if (fu.PostedFile.ContentLength > Media.MaxUploadBytes) { Say("The file is larger than 30 MB.", false); return; }
            string dir = Server.MapPath("~/Media/uploads");
            Directory.CreateDirectory(dir);
            string name = Guid.NewGuid().ToString("N") + ext;   // random name: never trust the uploaded file name
            fu.SaveAs(Path.Combine(dir, name));
            saved = "~/Media/uploads/" + name;
            url = saved;
            type = detected;
        }
        if (type == "None") url = "";
        if (!Media.IsValid(type, url))
        {
            Say("Please give a valid media URL (or upload a file) for the selected type.", false);
            return;
        }
        int order = Db.ToInt(txtOrder.Text.Trim(), 1);
        Db.Exec("INSERT INTO Lessons(CourseId,Title,Body,MediaType,MediaUrl,SortOrder) VALUES(@c,@t,@b,@mt,@mu,@o)",
            Db.P("@c", CourseId), Db.P("@t", txtTitle.Text.Trim()), Db.P("@b", txtBody.Text.Trim()), Db.P("@mt", type),
            Db.P("@mu", url.Length == 0 ? null : url), Db.P("@o", order));
        txtTitle.Text = txtBody.Text = txtUrl.Text = "";
        txtOrder.Text = "";
        ddlType.SelectedIndex = 0;
        Say("Lesson added.", true);
        Bind();
    }
}
