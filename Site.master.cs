using System;
using System.Web.Security;

public partial class SiteMaster : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        bool auth = Page.User.Identity.IsAuthenticated;
        phGuest.Visible = !auth;
        phUser.Visible = auth;
        lnkOut.Visible = auth;
        phAdmin.Visible = auth && Page.User.IsInRole("Admin");

        // Breadcrumbs only make sense below the home page.
        var node = System.Web.SiteMap.CurrentNode;
        navCrumbs.Visible = node != null && node.ParentNode != null;

        if (!Page.Title.EndsWith("Onyx Learn")) Page.Title = Page.Title + " | Onyx Learn";
    }

    protected void lnkOut_Click(object sender, EventArgs e)
    {
        FormsAuthentication.SignOut();
        Response.Redirect("~/Default.aspx");
    }
}
