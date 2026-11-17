<%@ Page Title="Sign in" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="LoginPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap"><div class="form">
<h1 class="ptitle">Sign in</h1>
<asp:Label ID="lblInfo" runat="server" CssClass="note" />
<asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" />
<asp:TextBox ID="txtEmail" runat="server" TextMode="Email" MaxLength="150" autocomplete="username" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Enter a valid email." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtPw" Text="Password" />
<asp:TextBox ID="txtPw" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="current-password" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPw" ErrorMessage="Password is required." CssClass="err" Display="Dynamic" />
<div class="chk"><input type="checkbox" id="showPw" /><label for="showPw">Show password</label></div>
<br />
<asp:Button ID="btnLogin" runat="server" Text="Sign in" OnClick="btnLogin_Click" />
<asp:Label ID="lblMsg" runat="server" CssClass="note" />
<p class="hint">New here? <a href="<%= ResolveUrl("~/Register.aspx") %>"><b>Create an account</b></a></p>
</div></section>
</asp:Content>
