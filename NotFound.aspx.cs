using System;

public partial class NotFoundPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.TrySkipIisCustomErrors = true;
        Response.StatusCode = 404;
    }
}
