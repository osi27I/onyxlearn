using System;
using System.Web.UI.WebControls;

public partial class AdminQuestions : System.Web.UI.Page
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

    void Bind()
    {
        gv.DataSource = Db.Query("SELECT QuestionId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption FROM Questions WHERE CourseId=@c ORDER BY QuestionId", Db.P("@c", CourseId));
        gv.DataBind();
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

    static string V(System.Collections.Specialized.IOrderedDictionary d, string key)
    {
        return ((string)(d[key] ?? "")).Trim();
    }

    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        if (!IsValid) { e.Cancel = true; return; }
        int id = (int)gv.DataKeys[e.RowIndex].Value;
        string q = V(e.NewValues, "QuestionText"), a = V(e.NewValues, "OptionA"), b = V(e.NewValues, "OptionB"), c = V(e.NewValues, "OptionC"), d = V(e.NewValues, "OptionD"), k = V(e.NewValues, "CorrectOption");
        if (q.Length == 0 || q.Length > 300 || a.Length == 0 || b.Length == 0 || c.Length == 0 || d.Length == 0 || k.Length != 1 || "ABCD".IndexOf(k) < 0)
        {
            Say("Every field is required (question up to 300 characters) and the correct answer must be A, B, C or D.", false);
            e.Cancel = true;
            return;
        }
        Db.Exec("UPDATE Questions SET QuestionText=@q,OptionA=@a,OptionB=@b,OptionC=@c,OptionD=@d,CorrectOption=@k WHERE QuestionId=@id",
            Db.P("@q", q), Db.P("@a", a), Db.P("@b", b), Db.P("@c", c), Db.P("@d", d), Db.P("@k", k), Db.P("@id", id));
        Say("Question updated.", true);
        gv.EditIndex = -1;
        Bind();
    }

    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Db.Exec("DELETE FROM Questions WHERE QuestionId=@id", Db.P("@id", (int)gv.DataKeys[e.RowIndex].Value));
        Say("Question deleted.", true);
        gv.EditIndex = -1;
        if (gv.Rows.Count == 1 && gv.PageIndex > 0) gv.PageIndex--;
        Bind();
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        if (CourseId < 0) { Say("Create a course first.", false); return; }
        Db.Exec("INSERT INTO Questions(CourseId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption) VALUES(@c,@q,@a,@b,@cc,@d,@k)",
            Db.P("@c", CourseId), Db.P("@q", txtQuestion.Text.Trim()), Db.P("@a", txtOA.Text.Trim()), Db.P("@b", txtOB.Text.Trim()),
            Db.P("@cc", txtOC.Text.Trim()), Db.P("@d", txtOD.Text.Trim()), Db.P("@k", ddlKey.SelectedValue));
        txtQuestion.Text = txtOA.Text = txtOB.Text = txtOC.Text = txtOD.Text = "";
        ddlKey.SelectedIndex = 0;
        Say("Question added.", true);
        Bind();
    }
}
