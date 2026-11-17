<%@ Page Title="Lesson" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Lesson.aspx.cs" Inherits="LessonPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<div class="lesson-layout">

<details class="outline" open>
<summary>Course outline</summary>
<p class="outline-course"><asp:HyperLink ID="lnkCourse" runat="server" /></p>
<div class="progress"><i id="barFill" runat="server"></i></div>
<span class="pct"><asp:Label ID="lblProg" runat="server" /></span>
<ol class="olist">
<asp:Repeater ID="rpOutline" runat="server">
<ItemTemplate>
<li class="<%# (bool)Eval("Cur") ? "cur" : "" %> <%# (int)Eval("Done") > 0 ? "done" : "" %>">
<a href="<%# ResolveUrl("~/Member/Lesson.aspx") %>?id=<%# Eval("LessonId") %>" <%# (bool)Eval("Cur") ? "aria-current=\"page\"" : "" %>>
<span class="n" aria-hidden="true"><%# (int)Eval("Done") > 0 ? "&#10003;" : Eval("Num") %></span>
<span><%#: Eval("Title") %><span class="vh"><%# (int)Eval("Done") > 0 ? " (completed)" : "" %></span></span>
</a>
</li>
</ItemTemplate>
</asp:Repeater>
<li id="liQuiz" runat="server" class="quizlink"><asp:HyperLink ID="lnkQuizSide" runat="server"><span class="n" aria-hidden="true">?</span><span>Course quiz</span></asp:HyperLink></li>
</ol>
</details>

<article class="lesson">
<p class="hint"><asp:Label ID="lblPos" runat="server" /> &middot; <asp:Label ID="lblRead" runat="server" /></p>
<h1 class="lesson-title"><asp:Label ID="lblTitle" runat="server" /></h1>
<asp:Literal ID="litMedia" runat="server" />
<div class="prose"><asp:Literal ID="litBody" runat="server" /></div>

<div class="panel actions">
<asp:Button ID="btnDone" runat="server" Text="Mark as completed" OnClick="btnDone_Click" CausesValidation="false" />
<asp:Label ID="lblDone" runat="server" CssClass="badge ok" Visible="false" Text="Completed" />
<asp:Label ID="lblMsg" runat="server" CssClass="hint" data-toast="ok" />
</div>

<div class="pager">
<asp:HyperLink ID="lnkPrev" runat="server" CssClass="btn ghost">&larr; Previous lesson</asp:HyperLink>
<asp:HyperLink ID="lnkNext" runat="server" CssClass="btn">Next lesson &rarr;</asp:HyperLink>
</div>
</article>

</div>
</section>
</asp:Content>
