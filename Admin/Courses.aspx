<%@ Page Title="Manage Courses" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Courses.aspx.cs" Inherits="AdminCourses" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Manage Courses</h1>
<div class="stats">
<div class="stat"><b><asp:Label ID="lblUsers" runat="server" /></b><span>Registered users</span></div>
<div class="stat"><b><asp:Label ID="lblCourses" runat="server" /></b><span>Courses</span></div>
<div class="stat"><b><asp:Label ID="lblLessons" runat="server" /></b><span>Lessons</span></div>
<div class="stat"><b><asp:Label ID="lblEnroll" runat="server" /></b><span>Enrolments</span></div>
</div>
<asp:Label ID="lblMsg" runat="server" CssClass="alert" EnableViewState="false" />
<div class="scrollx">
<asp:GridView ID="gv" runat="server" CssClass="gv" AutoGenerateColumns="false" DataKeyNames="CourseId" GridLines="None"
 AllowPaging="true" PageSize="6" PagerStyle-CssClass="pg" EmptyDataText="No courses yet."
 OnRowEditing="gv_RowEditing" OnRowCancelingEdit="gv_RowCancelingEdit" OnRowUpdating="gv_RowUpdating" OnRowDeleting="gv_RowDeleting" OnPageIndexChanging="gv_PageIndexChanging">
<Columns>
<asp:BoundField DataField="CourseId" HeaderText="ID" ReadOnly="true" />
<asp:TemplateField HeaderText="Title">
<ItemTemplate><b><%#: Eval("Title") %></b></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtT" runat="server" Text='<%# Bind("Title") %>' MaxLength="150" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtT" ValidationGroup="edit" ErrorMessage="Title is required." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Category">
<ItemTemplate><%#: Eval("Category") %></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtC" runat="server" Text='<%# Bind("Category") %>' MaxLength="60" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtC" ValidationGroup="edit" ErrorMessage="Category is required." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Level">
<ItemTemplate><%#: Eval("Level") %></ItemTemplate>
<EditItemTemplate>
<asp:DropDownList ID="ddlL" runat="server" SelectedValue='<%# Bind("Level") %>'>
<asp:ListItem>Beginner</asp:ListItem><asp:ListItem>Intermediate</asp:ListItem><asp:ListItem>Advanced</asp:ListItem>
</asp:DropDownList>
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Description">
<ItemTemplate><div class="cell-wide"><%#: Eval("Description") %></div></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtD" runat="server" Text='<%# Bind("Description") %>' TextMode="MultiLine" Rows="3" data-max="500" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtD" ValidationGroup="edit" ErrorMessage="Description is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtD" ValidationGroup="edit" ValidationExpression="^[\s\S]{1,500}$" ErrorMessage="Maximum 500 characters." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Cover">
<ItemTemplate><small><%#: Eval("CoverUrl") %></small></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtU" runat="server" Text='<%# Bind("CoverUrl") %>' MaxLength="300" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtU" ValidationGroup="edit" ValidationExpression="^(~/Media/[A-Za-z0-9_\-./]+|https?://\S+)$" ErrorMessage="Use ~/Media/... or an http(s) link." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Published">
<ItemTemplate><span class="badge <%# (bool)Eval("IsPublished") ? "ok" : "warn" %>"><%# (bool)Eval("IsPublished") ? "Live" : "Draft" %></span></ItemTemplate>
<EditItemTemplate><asp:CheckBox ID="chkP" runat="server" Checked='<%# Bind("IsPublished") %>' /></EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Content">
<ItemTemplate>
<a href="Lessons.aspx?course=<%# Eval("CourseId") %>"><%# Media.Plural((int)Eval("Lessons"), "lesson") %></a><br />
<a href="Questions.aspx?course=<%# Eval("CourseId") %>"><%# Media.Plural((int)Eval("Questions"), "question") %></a>
</ItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Actions">
<ItemTemplate>
<asp:LinkButton runat="server" CommandName="Edit" CausesValidation="false">Edit</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this course? Its lessons, quiz questions, enrolments and comments will also be deleted.');">Delete</asp:LinkButton>
</ItemTemplate>
<EditItemTemplate>
<asp:LinkButton runat="server" CommandName="Update" ValidationGroup="edit">Save</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="Cancel" CausesValidation="false">Cancel</asp:LinkButton>
</EditItemTemplate>
</asp:TemplateField>
</Columns>
</asp:GridView>
</div>

<div class="panel">
<h2>Add Course</h2>
<div class="row2">
<div>
<asp:Label runat="server" AssociatedControlID="txtTitle" Text="Title" />
<asp:TextBox ID="txtTitle" runat="server" MaxLength="150" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ValidationGroup="add" ErrorMessage="Title is required." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtCat" Text="Category" />
<asp:TextBox ID="txtCat" runat="server" MaxLength="60" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtCat" ValidationGroup="add" ErrorMessage="Category is required." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="ddlLevel" Text="Level" />
<asp:DropDownList ID="ddlLevel" runat="server"><asp:ListItem>Beginner</asp:ListItem><asp:ListItem>Intermediate</asp:ListItem><asp:ListItem>Advanced</asp:ListItem></asp:DropDownList>
</div>
<div>
<asp:Label runat="server" AssociatedControlID="txtDesc" Text="Description" />
<asp:TextBox ID="txtDesc" runat="server" TextMode="MultiLine" Rows="3" data-max="500" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtDesc" ValidationGroup="add" ErrorMessage="Description is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtDesc" ValidationGroup="add" ValidationExpression="^[\s\S]{1,500}$" ErrorMessage="Maximum 500 characters." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtCover" Text="Cover image URL (optional)" />
<asp:TextBox ID="txtCover" runat="server" MaxLength="300" placeholder="~/Media/covers/web-dev.svg" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtCover" ValidationGroup="add" ValidationExpression="^(~/Media/[A-Za-z0-9_\-./]+|https?://\S+)$" ErrorMessage="Use ~/Media/... or an http(s) link." CssClass="err" Display="Dynamic" />
<div class="chk"><asp:CheckBox ID="chkPub" runat="server" Checked="true" Text="Publish immediately" /></div>
</div>
</div>
<br />
<asp:Button ID="btnAdd" runat="server" Text="Add course" ValidationGroup="add" OnClick="btnAdd_Click" />
</div>
</section>
</asp:Content>
