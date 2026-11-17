<%@ Page Title="Page not found" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="NotFound.aspx.cs" Inherits="NotFoundPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap"><div class="panel" style="max-width:640px;margin:40px auto;text-align:center">
<h1 class="ptitle">404 &ndash; Page not found</h1>
<p>The page or course you are looking for does not exist or has been removed.</p>
<br />
<a class="btn" href="<%= ResolveUrl("~/Default.aspx") %>">Browse courses</a>
</div></section>
</asp:Content>
