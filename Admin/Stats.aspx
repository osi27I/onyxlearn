<%@ Page Title="Statistics" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Stats.aspx.cs" Inherits="AdminStats" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Statistics</h1>
<p class="hint">A quick look at how learners are using the platform.</p>

<div class="stats">
<div class="stat"><b><asp:Label ID="lblMembers" runat="server" /></b><span>Members</span></div>
<div class="stat"><b><asp:Label ID="lblEnrol" runat="server" /></b><span>Enrolments</span></div>
<div class="stat"><b><asp:Label ID="lblLessonsDone" runat="server" /></b><span>Lessons completed</span></div>
<div class="stat"><b><asp:Label ID="lblAttempts" runat="server" /></b><span>Quiz attempts</span></div>
<div class="stat"><b><asp:Label ID="lblCerts" runat="server" /></b><span>Courses completed</span></div>
</div>

<h2>Per course</h2>
<div class="scrollx">
<asp:Repeater ID="rpCourses" runat="server">
<HeaderTemplate><table class="t"><tr><th>Course</th><th>Enrolled</th><th>Average progress</th><th>Quiz attempts</th><th>Average score</th><th>Completed</th></tr></HeaderTemplate>
<ItemTemplate><tr>
<td><b><%#: Eval("Title") %></b></td>
<td><%# Eval("Enrolled") %></td>
<td><%# Eval("Progress") %>%</td>
<td><%# Eval("Attempts") %></td>
<td><%# Eval("AvgScore") == DBNull.Value ? "-" : Eval("AvgScore") + "%" %></td>
<td><%# Eval("CompletedCount") %></td>
</tr></ItemTemplate>
<FooterTemplate></table></FooterTemplate>
</asp:Repeater>
</div>
<asp:Panel ID="pnlNoCourses" runat="server" CssClass="empty" Visible="false">No courses yet.</asp:Panel>

<div class="row2">
<div class="panel">
<h2>Enrolments per course</h2>
<div class="scrollx"><asp:Literal ID="litEnrol" runat="server" /></div>
</div>
<div class="panel">
<h2>Average quiz score</h2>
<div class="scrollx"><asp:Literal ID="litScore" runat="server" /></div>
<p class="hint">The dashed line is the pass mark.</p>
</div>
</div>

<h2>Most active learners</h2>
<div class="scrollx">
<asp:Repeater ID="rpTop" runat="server">
<HeaderTemplate><table class="t"><tr><th>Learner</th><th>Lessons completed</th><th>Quiz attempts</th></tr></HeaderTemplate>
<ItemTemplate><tr><td><b><%#: Eval("FullName") %></b><br /><small class="hint"><%#: Eval("Email") %></small></td><td><%# Eval("Done") %></td><td><%# Eval("Attempts") %></td></tr></ItemTemplate>
<FooterTemplate></table></FooterTemplate>
</asp:Repeater>
</div>

<h2>Download data (CSV)</h2>
<div class="panel actions">
<a class="btn" href="<%= ResolveUrl("~/Admin/Export.aspx") %>?what=users">Users</a>
<a class="btn" href="<%= ResolveUrl("~/Admin/Export.aspx") %>?what=progress">Course progress</a>
<a class="btn" href="<%= ResolveUrl("~/Admin/Export.aspx") %>?what=results">Quiz results</a>
<span class="hint">Opens in Excel. Passwords are never included.</span>
</div>
</section>
</asp:Content>
