using System;
using System.Web;
using System.Web.Security;

public partial class LoginPage : System.Web.UI.Page
{
    const int MaxFailures = 5;
    const int LockMinutes = 10;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        if (Request.QueryString["registered"] == "1")
            lblInfo.Text = "Account created. Please sign in.";
        else if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
            lblInfo.Text = "Your account does not have access to that page.";
    }

    // Only allow redirects back to pages on this site (prevents open-redirect attacks).
    static bool IsLocal(string url)
    {
        return !string.IsNullOrEmpty(url) && url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        string email = txtEmail.Text.Trim().ToLower();
        var t = Db.Query("SELECT UserId,PasswordHash,Role,FailedLogins,CASE WHEN LockoutUntil IS NOT NULL AND LockoutUntil > GETDATE() THEN 1 ELSE 0 END AS Locked FROM Users WHERE Email=@e", Db.P("@e", email));
        if (t.Rows.Count == 0)
        {
            lblMsg.Text = "Invalid email or password.";
            return;
        }
        var row = t.Rows[0];
        int id = Convert.ToInt32(row["UserId"]);
        if (Convert.ToInt32(row["Locked"]) == 1)
        {
            lblMsg.Text = "Too many failed attempts. This account is locked for a few minutes; please try again later.";
            return;
        }
        if (!Db.Verify(txtPw.Text, (string)row["PasswordHash"]))
        {
            int fails = Convert.ToInt32(row["FailedLogins"]) + 1;
            if (fails >= MaxFailures)
                Db.Exec("UPDATE Users SET FailedLogins=0, LockoutUntil=DATEADD(MINUTE,@m,GETDATE()) WHERE UserId=@id", Db.P("@m", LockMinutes), Db.P("@id", id));
            else
                Db.Exec("UPDATE Users SET FailedLogins=@f WHERE UserId=@id", Db.P("@f", fails), Db.P("@id", id));
            lblMsg.Text = "Invalid email or password.";
            return;
        }
        Db.Exec("UPDATE Users SET FailedLogins=0, LockoutUntil=NULL WHERE UserId=@id", Db.P("@id", id));

        string role = (string)row["Role"];
        var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now, DateTime.Now.AddMinutes(60), false, role);
        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
        {
            HttpOnly = true,
            Secure = Request.IsSecureConnection
        };
        Response.Cookies.Add(cookie);

        string ret = Request.QueryString["ReturnUrl"];
        if (IsLocal(ret) && (role == "Admin" || !ret.ToLower().Contains("/admin/")))
            Response.Redirect(ret);
        else
            Response.Redirect(role == "Admin" ? "~/Admin/Courses.aspx" : "~/Member/Dashboard.aspx");
    }
}
