<%@ Page Title="Course" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Course.aspx.cs" Inherits="CoursePage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<asp:Label ID="lblFlash" runat="server" CssClass="alert ok" />
<div class="course-head">
<div>
<small class="badge"><asp:Label ID="lblCat" runat="server" /></small>
<h1><asp:Label ID="lblTitle" runat="server" /></h1>
<p><asp:Label ID="lblDesc" runat="server" /></p>
<p class="hint"><asp:Label ID="lblFacts" runat="server" /></p>
<div class="actions">
<asp:Button ID="btnEnrol" runat="server" Text="Enrol in this course" OnClick="btnEnrol_Click" CausesValidation="false" />
<asp:HyperLink ID="lnkQuiz" runat="server" CssClass="btn ghost">Take the quiz</asp:HyperLink>
<asp:HyperLink ID="lnkStart" runat="server" CssClass="btn">Continue learning</asp:HyperLink>
<asp:HyperLink ID="lnkCert" runat="server" CssClass="btn" Visible="false">Get your certificate</asp:HyperLink>
</div>
<asp:Panel ID="pnlProgress" runat="server" CssClass="panel" Visible="false">
<b>Your progress</b>
<div class="progress"><i id="barProgress" runat="server"></i></div>
<span class="pct"><asp:Label ID="lblProgress" runat="server" /></span>
</asp:Panel>
</div>
<img id="imgCover" runat="server" alt="Course cover illustration" />
</div>

<h2 style="margin-top:34px">Lessons</h2>
<asp:Label ID="lblNoLessons" runat="server" CssClass="empty" Visible="false" Text="Lessons for this course are being prepared." />
<ol class="lessons">
<asp:Repeater ID="rpLessons" runat="server">
<ItemTemplate>
<li>
<span class="no <%# (int)Eval("Done") == 1 ? "done" : "" %>"><%# (int)Eval("Done") == 1 ? "&#10003;" : Eval("SortOrder") %></span>
<span class="tt"><a href="<%# ResolveUrl("~/Member/Lesson.aspx") %>?id=<%# Eval("LessonId") %>"><%#: Eval("Title") %></a>
<small class="hint"><%# Eval("MediaType").ToString() == "None" ? "Reading" : Eval("MediaType").ToString() == "Embed" ? "Video" : Eval("MediaType") %></small></span>
<span class="badge <%# (int)Eval("Done") == 1 ? "ok" : "" %>"><%# (int)Eval("Done") == 1 ? "Completed" : "Open" %></span>
</li>
</ItemTemplate>
</asp:Repeater>
</ol>

<h2 style="margin-top:34px">Discussion</h2>
<p class="hint">Ask questions and share what you learned. Please keep it respectful.</p>
<asp:Label ID="lblNoComments" runat="server" CssClass="empty" Visible="false" Text="No comments yet. Be the first to start the discussion." />
<asp:Repeater ID="rpComments" runat="server" OnItemCommand="rpComments_ItemCommand">
<ItemTemplate>
<article class="comment">
<header><span><b><%#: Eval("FullName") %></b> &middot; <%#: Eval("PostedAt", "{0:dd MMM yyyy, HH:mm}") %></span>
<asp:LinkButton runat="server" CommandName="del" CommandArgument='<%# Eval("CommentId") %>' CausesValidation="false" Visible='<%# CanDelete((int)Eval("UserId")) %>' OnClientClick="return confirm('Delete this comment?');">Delete</asp:LinkButton></header>
<p><%#: Eval("Body") %></p>
</article>
</ItemTemplate>
</asp:Repeater>

<asp:Panel ID="pnlPost" runat="server" CssClass="panel">
<asp:Label runat="server" AssociatedControlID="txtComment" Text="Add a comment" />
<asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="3" data-max="500" ValidationGroup="c" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtComment" ValidationGroup="c" ErrorMessage="Write something first." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtComment" ValidationGroup="c" ValidationExpression="^[\s\S]{2,500}$" ErrorMessage="A comment must be between 2 and 500 characters." CssClass="err" Display="Dynamic" />
<br /><br />
<asp:Button ID="btnPost" runat="server" Text="Post comment" ValidationGroup="c" OnClick="btnPost_Click" />
<asp:Label ID="lblMsg" runat="server" CssClass="note" data-toast="ok" />
</asp:Panel>
<asp:Panel ID="pnlGuest" runat="server" CssClass="panel" Visible="false">
<a href="<%= ResolveUrl("~/Login.aspx") %>?ReturnUrl=<%= Server.UrlEncode(Request.RawUrl) %>"><b>Sign in</b></a> to join the discussion.
</asp:Panel>
</section>
</asp:Content>
