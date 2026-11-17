<%@ Page Title="Manage Quizzes" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Questions.aspx.cs" Inherits="AdminQuestions" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Manage Quiz Questions</h1>
<div class="tools">
<div>
<asp:Label runat="server" AssociatedControlID="ddlCourse" Text="Course" />
<asp:DropDownList ID="ddlCourse" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" DataTextField="Title" DataValueField="CourseId" />
</div>
</div>
<asp:Label ID="lblMsg" runat="server" CssClass="alert" EnableViewState="false" />
<div class="scrollx">
<asp:GridView ID="gv" runat="server" CssClass="gv" AutoGenerateColumns="false" DataKeyNames="QuestionId" GridLines="None"
 AllowPaging="true" PageSize="6" PagerStyle-CssClass="pg" EmptyDataText="This course has no quiz questions yet. Add the first one below."
 OnRowEditing="gv_RowEditing" OnRowCancelingEdit="gv_RowCancelingEdit" OnRowUpdating="gv_RowUpdating" OnRowDeleting="gv_RowDeleting" OnPageIndexChanging="gv_PageIndexChanging">
<Columns>
<asp:BoundField DataField="QuestionId" HeaderText="ID" ReadOnly="true" />
<asp:TemplateField HeaderText="Question">
<ItemTemplate><b><%#: Eval("QuestionText") %></b></ItemTemplate>
<EditItemTemplate>
<asp:TextBox ID="txtQ" runat="server" Text='<%# Bind("QuestionText") %>' TextMode="MultiLine" Rows="3" data-max="300" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtQ" ValidationGroup="edit" ErrorMessage="Question is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtQ" ValidationGroup="edit" ValidationExpression="^[\s\S]{1,300}$" ErrorMessage="Maximum 300 characters." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Options">
<ItemTemplate>A. <%#: Eval("OptionA") %><br />B. <%#: Eval("OptionB") %><br />C. <%#: Eval("OptionC") %><br />D. <%#: Eval("OptionD") %></ItemTemplate>
<EditItemTemplate>
A <asp:TextBox ID="txtA" runat="server" Text='<%# Bind("OptionA") %>' MaxLength="200" /><asp:RequiredFieldValidator runat="server" ControlToValidate="txtA" ValidationGroup="edit" ErrorMessage="Option A required." CssClass="err" Display="Dynamic" /><br />
B <asp:TextBox ID="txtB" runat="server" Text='<%# Bind("OptionB") %>' MaxLength="200" /><asp:RequiredFieldValidator runat="server" ControlToValidate="txtB" ValidationGroup="edit" ErrorMessage="Option B required." CssClass="err" Display="Dynamic" /><br />
C <asp:TextBox ID="txtC" runat="server" Text='<%# Bind("OptionC") %>' MaxLength="200" /><asp:RequiredFieldValidator runat="server" ControlToValidate="txtC" ValidationGroup="edit" ErrorMessage="Option C required." CssClass="err" Display="Dynamic" /><br />
D <asp:TextBox ID="txtD" runat="server" Text='<%# Bind("OptionD") %>' MaxLength="200" /><asp:RequiredFieldValidator runat="server" ControlToValidate="txtD" ValidationGroup="edit" ErrorMessage="Option D required." CssClass="err" Display="Dynamic" />
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Correct">
<ItemTemplate><span class="badge ok"><%# Eval("CorrectOption") %></span></ItemTemplate>
<EditItemTemplate>
<asp:DropDownList ID="ddlK" runat="server" SelectedValue='<%# Bind("CorrectOption") %>'>
<asp:ListItem>A</asp:ListItem><asp:ListItem>B</asp:ListItem><asp:ListItem>C</asp:ListItem><asp:ListItem>D</asp:ListItem>
</asp:DropDownList>
</EditItemTemplate>
</asp:TemplateField>
<asp:TemplateField HeaderText="Actions">
<ItemTemplate>
<asp:LinkButton runat="server" CommandName="Edit" CausesValidation="false">Edit</asp:LinkButton>
<asp:LinkButton runat="server" CommandName="Delete" CausesValidation="false" OnClientClick="return confirm('Delete this question?');">Delete</asp:LinkButton>
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
<h2>Add Question</h2>
<asp:Label runat="server" AssociatedControlID="txtQuestion" Text="Question" />
<asp:TextBox ID="txtQuestion" runat="server" TextMode="MultiLine" Rows="3" data-max="300" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtQuestion" ValidationGroup="add" ErrorMessage="Question is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtQuestion" ValidationGroup="add" ValidationExpression="^[\s\S]{1,300}$" ErrorMessage="Maximum 300 characters." CssClass="err" Display="Dynamic" />
<div class="row2">
<div>
<asp:Label runat="server" AssociatedControlID="txtOA" Text="Option A" /><asp:TextBox ID="txtOA" runat="server" MaxLength="200" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOA" ValidationGroup="add" ErrorMessage="Option A is required." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtOB" Text="Option B" /><asp:TextBox ID="txtOB" runat="server" MaxLength="200" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOB" ValidationGroup="add" ErrorMessage="Option B is required." CssClass="err" Display="Dynamic" />
</div>
<div>
<asp:Label runat="server" AssociatedControlID="txtOC" Text="Option C" /><asp:TextBox ID="txtOC" runat="server" MaxLength="200" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOC" ValidationGroup="add" ErrorMessage="Option C is required." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtOD" Text="Option D" /><asp:TextBox ID="txtOD" runat="server" MaxLength="200" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOD" ValidationGroup="add" ErrorMessage="Option D is required." CssClass="err" Display="Dynamic" />
</div>
</div>
<asp:Label runat="server" AssociatedControlID="ddlKey" Text="Correct answer" />
<asp:DropDownList ID="ddlKey" runat="server"><asp:ListItem>A</asp:ListItem><asp:ListItem>B</asp:ListItem><asp:ListItem>C</asp:ListItem><asp:ListItem>D</asp:ListItem></asp:DropDownList>
<br /><br />
<asp:Button ID="btnAdd" runat="server" Text="Add question" ValidationGroup="add" OnClick="btnAdd_Click" />
</div>
</section>
</asp:Content>
