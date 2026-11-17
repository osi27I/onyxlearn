<%@ Page Title="Join" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="RegisterPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap"><div class="form">
<h1 class="ptitle">Create your account</h1>
<asp:Label runat="server" AssociatedControlID="txtName" Text="Full name" />
<asp:TextBox ID="txtName" runat="server" MaxLength="100" autocomplete="name" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ErrorMessage="Name is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtName" ValidationExpression="^.{2,100}$" ErrorMessage="Name must be between 2 and 100 characters." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" />
<asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="150" autocomplete="email" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Enter a valid email." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtPw" Text="Password" />
<asp:TextBox ID="txtPw" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="new-password" />
<div class="meter" aria-hidden="true"><i id="pwBar"></i></div><span id="pwText" class="hint" aria-live="polite"></span>
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPw" ErrorMessage="Password is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtPw" ValidationExpression="^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$" ErrorMessage="Min 8 characters with upper, lower and a digit." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtPw2" Text="Confirm password" />
<asp:TextBox ID="txtPw2" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="new-password" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPw2" ErrorMessage="Please confirm your password." CssClass="err" Display="Dynamic" />
<asp:CompareValidator runat="server" ControlToValidate="txtPw2" ControlToCompare="txtPw" ErrorMessage="Passwords do not match." CssClass="err" Display="Dynamic" />
<div class="chk"><input type="checkbox" id="showPw" /><label for="showPw">Show passwords</label></div>
<br />
<asp:Button ID="btnReg" runat="server" Text="Create account" OnClick="btnReg_Click" />
<asp:Label ID="lblMsg" runat="server" CssClass="note" />
<p class="hint">Already a member? <a href="<%= ResolveUrl("~/Login.aspx") %>"><b>Sign in</b></a></p>
</div></section>
</asp:Content>
