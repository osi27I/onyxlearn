using System;
using System.Web.UI.WebControls;

public partial class AdminUsers : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) Bind();
    }

    void Bind()
    {
        gv.DataSource = Db.Query(
            @"SELECT u.UserId,u.FullName,u.Email,u.Role,u.CreatedAt,
 (SELECT COUNT(*) FROM Enrollments e WHERE e.UserId=u.UserId) AS Enrolments,
 CASE WHEN u.LockoutUntil IS NOT NULL AND u.LockoutUntil > GETDATE() THEN 1 ELSE 0 END AS Locked
 FROM Users u WHERE u.FullName LIKE @q OR u.Email LIKE @q ORDER BY u.UserId",
            Db.P("@q", "%" + txtFind.Text.Trim() + "%"));
        gv.DataBind();
    }

    void Say(string text, bool ok)
    {
        lblMsg.CssClass = ok ? "alert ok" : "alert bad";
        lblMsg.Text = Server.HtmlEncode(text);
    }

    protected void btnFind_Click(object sender, EventArgs e)
    {
        gv.PageIndex = 0;
        Bind();
    }

    protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gv.PageIndex = e.NewPageIndex;
        Bind();
    }

    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id = Db.ToInt(Convert.ToString(e.CommandArgument), -1);
        if (e.CommandName == "role")
        {
            // An administrator cannot change their own role, so the site always keeps at least one admin.
            int n = Db.Exec("UPDATE Users SET Role=CASE WHEN Role='Admin' THEN 'Member' ELSE 'Admin' END WHERE UserId=@id AND Email<>@me",
                Db.P("@id", id), Db.P("@me", User.Identity.Name));
            if (n == 0) Say("You cannot change your own role.", false); else Say("Role updated.", true);
            Bind();
        }
        else if (e.CommandName == "unlock")
        {
            Db.Exec("UPDATE Users SET FailedLogins=0, LockoutUntil=NULL WHERE UserId=@id", Db.P("@id", id));
            Say("Account unlocked.", true);
            Bind();
        }
    }

    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int n = Db.Exec("DELETE FROM Users WHERE UserId=@id AND Email<>@me",
            Db.P("@id", (int)gv.DataKeys[e.RowIndex].Value), Db.P("@me", User.Identity.Name));
        if (n == 0) Say("You cannot delete your own account.", false); else Say("User deleted.", true);
        Bind();
    }
}
