using System;

public partial class RegisterPage : System.Web.UI.Page
{
    protected void btnReg_Click(object sender, EventArgs e)
    {
        if (!IsValid) return;
        string email = txtEmail.Text.Trim().ToLower();
        if (Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Users WHERE Email=@e", Db.P("@e", email))) > 0)
        {
            lblMsg.Text = "An account with this email already exists.";
            return;
        }
        Db.Exec("INSERT INTO Users(FullName,Email,PasswordHash,Role) VALUES(@n,@e,@h,'Member')",
            Db.P("@n", txtName.Text.Trim()), Db.P("@e", email), Db.P("@h", Db.Hash(txtPw.Text)));
        Response.Redirect("~/Login.aspx?registered=1");
    }
}
