<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Users.aspx.cs" Inherits="AdminUsers" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Manage Users</h1>
<div class="tools bar">
<div>
<asp:Label runat="server" AssociatedControlID="txtFind" Text="Search name or email" />
<asp:TextBox ID="txtFind" runat="server" MaxLength="60" />
</div>
<asp:Button ID="btnFind" runat="server" Text="Search" OnClick="btnFind_Click" CausesValidation="false" />
<a class="btn ghost" href="<%= ResolveUrl("~/Admin/Export.aspx") %>?what=users">Export CSV</a>
</div>
<asp:Label ID="lblMsg" runat="server" CssClass="alert" EnableViewState="false" />
<div class="scrollx">
<asp:GridView ID="gv" runat="server" CssClass="gv" AutoGenerateColumns="false" DataKeyNames="UserId" GridLines="None"
 AllowPaging="true" PageSize="8" PagerStyle-CssClass="pg" EmptyDataText="No users found."
 OnRowCommand="gv_RowCommand" OnRowDeleting="gv_RowDeleting" OnPageIndexChanging="gv_PageIndexChanging">
<Columns>
<asp:BoundField DataField="UserId" HeaderText="ID" />
<asp:BoundField DataField="FullName" HeaderText="Name" />
<asp:BoundField DataField="Email" HeaderText="Email" />
<asp:TemplateField HeaderText="Role"><ItemTemplate><span class="badge <%# Eval("Role").ToString() == "Admin" ? "warn" : "" %>"><%#: Eval("Role") %></span></ItemTemplate></asp:TemplateField>
<asp:BoundField DataField="Enrolments" HeaderText="Courses" />
<asp:BoundField DataField="CreatedAt" HeaderText="Joined" DataFormatString="{0:dd MMM yyyy}" />
<asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge <%# (int)Eval("Locked") == 1 ? "bad" : "ok" %>"><%# (int)Eval("Locked") == 1 ? "Locked" : "Active" %></span></ItemTemplate></asp:TemplateField>
<asp:TemplateField HeaderText="Actions">
<ItemTemplate>
<asp:LinkButton runat="server" CommandName="role" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" OnClientClick="return confirm('Change the role of this user?');">Toggle role</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="unlock" CommandArgument='<%# Eval("UserId") %>' CausesValidation="false" Visible='<%# (int)Eval("Locked") == 1 %>'>Unlock</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this user and all of their enrolments, progress and comments?');">Delete</asp:LinkButton>
</ItemTemplate>
</asp:TemplateField>
</Columns>
</asp:GridView>
</div>
</section>
</asp:Content>
