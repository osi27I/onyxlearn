using System;

public partial class ErrorPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.TrySkipIisCustomErrors = true;
        if (Request.QueryString["code"] == "input")
        {
            Response.StatusCode = 400;
            lblText.Text = "Your input contained characters that look like HTML or script (such as the less-than sign). Please remove them and try again.";
        }
        else
        {
            Response.StatusCode = 500;
            lblText.Text = "An unexpected problem occurred. Please try again, and contact an administrator if it keeps happening.";
        }
    }
}
