<%@ Page Title="Manage Lessons" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Lessons.aspx.cs" Inherits="AdminLessons" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Manage Lessons</h1>
<div class="tools">
<div>
<asp:Label runat="server" AssociatedControlID="ddlCourse" Text="Course" />
<asp:DropDownList ID="ddlCourse" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" DataTextField="Title" DataValueField="CourseId" />
</div>
</div>
<asp:Label ID="lblMsg" runat="server" CssClass="alert" EnableViewState="false" />
<div class="scrollx">
<asp:GridView ID="gv" runat="server" CssClass="gv" AutoGenerateColumns="false" DataKeyNames="LessonId" GridLines="None"
 AllowPaging="true" PageSize="6" PagerStyle-CssClass="pg" EmptyDataText="This course has no lessons yet. Add the first one below."
 OnRowEditing="gv_RowEditing" OnRowCancelingEdit="gv_RowCancelingEdit" OnRowUpdating="gv_RowUpdating" OnRowDeleting="gv_RowDeleting" OnPageIndexChanging="gv_PageIndexChanging">
<Columns>
<asp:TemplateField HeaderText="Order">
<ItemTemplate><%# Eval("SortOrder") %></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtO" runat="server" Text='<%# Bind("SortOrder") %>' MaxLength="3" />
<asp:RangeValidator runat="server" ControlToValidate="txtO" ValidationGroup="edit" Type="Integer" MinimumValue="1" MaximumValue="999" ErrorMessage="1 to 999." CssClass="err" Display="Dynamic" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtO" ValidationGroup="edit" ErrorMessage="Required." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Title">
<ItemTemplate><b><%#: Eval("Title") %></b><br /><small class="hint"><%#: Short(Eval("Body"), 90) %></small></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtT" runat="server" Text='<%# Bind("Title") %>' MaxLength="150" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtT" ValidationGroup="edit" ErrorMessage="Title is required." CssClass="err" Display="Dynamic" />
<asp:TextBox ID="txtB" runat="server" Text='<%# Bind("Body") %>' TextMode="MultiLine" Rows="6" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtB" ValidationGroup="edit" ErrorMessage="Lesson text is required." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Media">
<ItemTemplate><span class="badge"><%#: Eval("MediaType") %></span><br /><small class="hint"><%#: Eval("MediaUrl") %></small></ItemTemplate>
<EditItemTemplate>
<asp:DropDownList ID="ddlM" runat="server" SelectedValue='<%# Bind("MediaType") %>'>
<asp:ListItem>None</asp:ListItem><asp:ListItem>Image</asp:ListItem><asp:ListItem>Video</asp:ListItem><asp:ListItem>Audio</asp:ListItem><asp:ListItem>Embed</asp:ListItem>
</asp:DropDownList>
<asp:TextBox ID="txtM" runat="server" Text='<%# Bind("MediaUrl") %>' MaxLength="300" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Actions">
<ItemTemplate>
<a href="<%# ResolveUrl("~/Member/Lesson.aspx") %>?id=<%# Eval("LessonId") %>">View</a>
<asp:LinkButton runat="server" CommandName="Edit" CausesValidation="false">Edit</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this lesson? Learner progress for it is also removed.');">Delete</asp:LinkButton>
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
<h2>Add Lesson</h2>
<div class="row2">
<div>
<asp:Label runat="server" AssociatedControlID="txtTitle" Text="Title" />
<asp:TextBox ID="txtTitle" runat="server" MaxLength="150" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ValidationGroup="add" ErrorMessage="Title is required." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtOrder" Text="Order in course" />
<asp:TextBox ID="txtOrder" runat="server" MaxLength="3" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOrder" ValidationGroup="add" ErrorMessage="Order is required." CssClass="err" Display="Dynamic" />
<asp:RangeValidator runat="server" ControlToValidate="txtOrder" ValidationGroup="add" Type="Integer" MinimumValue="1" MaximumValue="999" ErrorMessage="Enter a number from 1 to 999." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtBody" Text="Lesson text" />
<asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" Rows="8" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtBody" ValidationGroup="add" ErrorMessage="Lesson text is required." CssClass="err" Display="Dynamic" />
<span class="hint">Blank line = new paragraph. Start a line with &ldquo;## &rdquo; for a heading, or with &ldquo;- &rdquo; for bullet points.</span>
</div>
<div>
<asp:Label runat="server" AssociatedControlID="ddlType" Text="Media type" />
<asp:DropDownList ID="ddlType" runat="server"><asp:ListItem>None</asp:ListItem><asp:ListItem>Image</asp:ListItem><asp:ListItem>Video</asp:ListItem><asp:ListItem>Audio</asp:ListItem><asp:ListItem>Embed</asp:ListItem></asp:DropDownList>
<asp:Label runat="server" AssociatedControlID="txtUrl" Text="Media URL (optional)" />
<asp:TextBox ID="txtUrl" runat="server" MaxLength="300" placeholder="~/Media/lessons/file.mp4 or https://..." />
<span class="hint">Embed accepts YouTube (/embed/) and Vimeo player links only.</span>
<asp:Label runat="server" AssociatedControlID="fu" Text="...or upload a file" />
<asp:FileUpload ID="fu" runat="server" accept=".jpg,.jpeg,.png,.gif,.webp,.mp4,.webm,.mp3,.ogg,.wav" />
<span class="hint">Allowed: JPG, PNG, GIF, WEBP, MP4, WEBM, MP3, OGG, WAV. Maximum 30 MB. The media type is detected automatically.</span>
</div>
</div>
<br />
<asp:Button ID="btnAdd" runat="server" Text="Add lesson" ValidationGroup="add" OnClick="btnAdd_Click" />
</div>
</section>
</asp:Content>
