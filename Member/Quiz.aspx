<%@ Page Title="Quiz" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Quiz.aspx.cs" Inherits="QuizPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap"><div class="lesson quizpage">
<p class="hint"><asp:HyperLink ID="lnkCourse" runat="server" /></p>
<h1 class="lesson-title"><asp:Label ID="lblTitle" runat="server" /></h1>
<asp:Label ID="lblBest" runat="server" CssClass="badge ok" Visible="false" />

<asp:Panel ID="pnlQuiz" runat="server" CssClass="quiz">
<p class="hint">Answer every question, then press Submit. You can retake the quiz as many times as you like; your best score is kept. Pass mark: <%= Db.PassMark %>%.</p>
<asp:Repeater ID="rpQ" runat="server" OnItemDataBound="rpQ_ItemDataBound">
<ItemTemplate>
<div class="q" role="group" aria-labelledby="qt<%# Container.ItemIndex + 1 %>">
<p class="qt" id="qt<%# Container.ItemIndex + 1 %>"><span class="qn"><%# Container.ItemIndex + 1 %></span> <%#: Eval("QuestionText") %></p>
<asp:HiddenField ID="hfId" runat="server" Value='<%# Eval("QuestionId") %>' />
<asp:RadioButtonList ID="rbl" runat="server" RepeatLayout="UnorderedList" CssClass="choices" />
</div>
</ItemTemplate>
</asp:Repeater>
<asp:Label ID="lblWarn" runat="server" CssClass="alert bad" data-toast="err" />
<asp:Button ID="btnSubmit" runat="server" Text="Submit answers" OnClick="btnSubmit_Click" CausesValidation="false" />
</asp:Panel>

<asp:Panel ID="pnlResult" runat="server" Visible="false">
<div class="panel result">
<asp:Literal ID="litRing" runat="server" />
<div>
<p class="result-score"><asp:Label ID="lblScore" runat="server" /></p>
<p><asp:Label ID="lblVerdict" runat="server" /></p>
<div class="actions">
<a class="btn" href="<%= ResolveUrl("~/Member/Quiz.aspx") %>?course=<%= courseIdValue %>">Retake quiz</a>
<a class="btn ghost" href="<%= ResolveUrl("~/Course.aspx") %>?id=<%= courseIdValue %>">Back to course</a>
<asp:HyperLink ID="lnkCert" runat="server" CssClass="btn" Visible="false">Get your certificate</asp:HyperLink>
</div>
<asp:Label ID="lblCertHint" runat="server" CssClass="hint" />
</div>
</div>
<h2>Review</h2>
<asp:Repeater ID="rpR" runat="server">
<ItemTemplate>
<div class="q <%# (bool)Eval("Right") ? "right" : "wrong" %>">
<p class="qt"><span class="qn"><%# Container.ItemIndex + 1 %></span> <%#: Eval("QuestionText") %></p>
<p>Your answer: <b><%#: Eval("Chosen") %></b> <span class="badge <%# (bool)Eval("Right") ? "ok" : "bad" %>"><%# (bool)Eval("Right") ? "Correct" : "Incorrect" %></span></p>
<p class="hint" style="display:<%# (bool)Eval("Right") ? "none" : "block" %>">Correct answer: <%#: Eval("Correct") %></p>
</div>
</ItemTemplate>
</asp:Repeater>
</asp:Panel>

<asp:Panel ID="pnlNone" runat="server" Visible="false" CssClass="empty">
<p><b>No quiz yet</b></p>
<p>This course does not have quiz questions yet. Check back soon, or carry on with the lessons.</p>
</asp:Panel>

<h2 style="margin-top:34px">Your attempts</h2>
<asp:Repeater ID="rpH" runat="server">
<HeaderTemplate><div class="scrollx"><table class="t"><tr><th>Date</th><th>Score</th><th>Result</th></tr></HeaderTemplate>
<ItemTemplate><tr><td><%#: Eval("TakenAt", "{0:dd MMM yyyy, HH:mm}") %></td><td><%# Eval("Score") %> / <%# Eval("Total") %></td><td><span class="badge <%# (int)Eval("Pct") >= Db.PassMark ? "ok" : "warn" %>"><%# Eval("Pct") %>%</span></td></tr></ItemTemplate>
<FooterTemplate></table></div></FooterTemplate>
</asp:Repeater>
<asp:Label ID="lblNoH" runat="server" CssClass="hint" Visible="false" Text="No attempts yet. Your scores will show up here." />
</div></section>
</asp:Content>
