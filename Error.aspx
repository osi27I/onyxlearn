<%@ Page Title="Something went wrong" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="ErrorPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap"><div class="panel" style="max-width:640px;margin:40px auto;text-align:center">
<h1 class="ptitle">Oops, something went wrong</h1>
<p><asp:Label ID="lblText" runat="server" /></p>
<br />
<a class="btn" href="<%= ResolveUrl("~/Default.aspx") %>">Back to courses</a>
</div></section>
</asp:Content>
