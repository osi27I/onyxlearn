<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="System.Security.Principal" %>
<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        Db.Init();
    }

    // Builds the user from the encrypted forms-authentication cookie.
    // The role and the existence of the account are re-checked in the database on every page request,
    // so a demoted, deleted or locked-out user loses access immediately instead of when the cookie expires.
    void Application_AuthenticateRequest(object sender, EventArgs e)
    {
        var c = Request.Cookies[FormsAuthentication.FormsCookieName];
        if (c == null) return;
        try
        {
            var t = FormsAuthentication.Decrypt(c.Value);
            if (t == null || t.Expired) return;
            string role = t.UserData;
            if (Request.CurrentExecutionFilePathExtension == ".aspx")
            {
                var r = Db.Query("SELECT Role FROM Users WHERE Email=@e", Db.P("@e", t.Name));
                if (r.Rows.Count == 0)
                {
                    FormsAuthentication.SignOut();
                    Context.User = new GenericPrincipal(new GenericIdentity(string.Empty), new string[0]);
                    return;
                }
                role = (string)r.Rows[0]["Role"];
            }
            Context.User = new GenericPrincipal(new FormsIdentity(t), new[] { role });
        }
        catch { }
    }

    // ASP.NET request validation rejects input that looks like HTML or script (for example "<b>").
    // Show a friendly message instead of the generic error page.
    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError();
        if (ex == null) return;
        if (ex is HttpRequestValidationException || ex.InnerException is HttpRequestValidationException)
        {
            Server.ClearError();
            Response.Redirect("~/Error.aspx?code=input", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
</script>
