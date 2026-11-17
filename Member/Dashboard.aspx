<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Dashboard.aspx.cs" Inherits="MemberDashboard" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">Welcome, <asp:Label ID="lblName" runat="server" /></h1>
<div class="stats">
<div class="stat"><b><asp:Label ID="lblCount" runat="server" /></b><span>Enrolled courses</span></div>
<div class="stat"><b><asp:Label ID="lblDoneLessons" runat="server" /></b><span>Lessons completed</span></div>
<div class="stat"><b><asp:Label ID="lblQuizzes" runat="server" /></b><span>Quiz attempts</span></div>
<div class="stat"><b><asp:Label ID="lblAvg" runat="server" /></b><span>Average best score</span></div>
</div>

<asp:Panel ID="pnlResume" runat="server" CssClass="resume" Visible="false">
<div>
<span class="eyebrow">Continue where you left off</span>
<p class="resume-t"><asp:Label ID="lblResumeTitle" runat="server" /></p>
<p class="hint"><asp:Label ID="lblResumeCourse" runat="server" /></p>
</div>
<asp:HyperLink ID="lnkResume" runat="server" CssClass="btn">Resume</asp:HyperLink>
</asp:Panel>

<h2>My Courses</h2>
<asp:Panel ID="lblNone" runat="server" CssClass="empty" Visible="false">
<svg class="empty-ic" viewBox="0 0 64 64" aria-hidden="true"><path d="M8 14c8-3 16-3 24 2v34c-8-5-16-5-24-2z" /><path d="M56 14c-8-3-16-3-24 2v34c8-5 16-5 24-2z" /></svg>
<p><b>No courses yet</b></p>
<p>Pick a course from the catalogue and it will show up here with your progress.</p>
<p><a class="btn" href="<%= ResolveUrl("~/Default.aspx") %>">Browse the catalogue</a></p>
</asp:Panel>
<div class="mycourses">
<asp:Repeater ID="rp" runat="server" OnItemCommand="rp_ItemCommand">
<ItemTemplate>
<article class="mc">
<%# Media.Ring((int)Eval("Pct"), (bool)Eval("Completed") ? "pass" : "prog", "Progress " + Eval("Pct") + " percent") %>
<div class="mc-body">
<h3><a href="<%# ResolveUrl("~/Course.aspx") %>?id=<%# Eval("CourseId") %>"><%#: Eval("Title") %></a></h3>
<p class="hint"><%#: Eval("Category") %> &middot; <%# Eval("Done") %> of <%# Media.Plural((int)Eval("TotalLessons"), "lesson") %> done</p>
<p class="hint">Best quiz: <%# Eval("BestScore") == DBNull.Value ? "not taken yet" : Eval("BestScore") + "%" %><%# (bool)Eval("Completed") ? " &middot; <span class=\"badge ok\">Completed</span>" : "" %></p>
<div class="actions">
<a class="btn" href="<%# ResolveUrl("~/Course.aspx") %>?id=<%# Eval("CourseId") %>">Open</a>
<asp:HyperLink runat="server" CssClass="btn ghost" NavigateUrl='<%# "~/Member/Certificate.aspx?course=" + Eval("CourseId") %>' Visible='<%# (bool)Eval("Completed") %>'>Certificate</asp:HyperLink>
<asp:LinkButton runat="server" CssClass="linkbtn" CommandName="drop" CommandArgument='<%# Eval("CourseId") %>' CausesValidation="false" OnClientClick="return confirm('Remove this course? Your lesson progress is kept if you enrol again.');">Unenrol</asp:LinkButton>
</div>
</div>
</article>
</ItemTemplate>
</asp:Repeater>
</div>

<h2 style="margin-top:40px">Quiz scores</h2>
<asp:Panel ID="pnlChart" runat="server" CssClass="panel" Visible="false">
<div class="scrollx"><asp:Literal ID="litChart" runat="server" /></div>
<p class="hint">Your last quiz attempts, oldest on the left. Blue bars reached the pass mark, amber bars did not.</p>
</asp:Panel>
<asp:Panel ID="lblNoQuiz" runat="server" CssClass="empty" Visible="false">
<svg class="empty-ic" viewBox="0 0 64 64" aria-hidden="true"><path d="M10 52V30M26 52V14M42 52V36M58 52H6" /></svg>
<p><b>No quiz attempts yet</b></p>
<p>Finish a lesson, take a quiz, and your scores will be charted here.</p>
</asp:Panel>
<asp:Repeater ID="rpQuiz" runat="server">
<HeaderTemplate><div class="scrollx"><table class="t"><tr><th>Course</th><th>Score</th><th>Date</th></tr></HeaderTemplate>
<ItemTemplate><tr><td><%#: Eval("Title") %></td><td><%# Eval("Score") %> / <%# Eval("Total") %></td><td><%#: Eval("TakenAt", "{0:dd MMM yyyy, HH:mm}") %></td></tr></ItemTemplate>
<FooterTemplate></table></div></FooterTemplate>
</asp:Repeater>

<div class="row2">
<div class="panel">
<h2>Profile</h2>
<asp:Label runat="server" AssociatedControlID="txtName" Text="Full name" />
<asp:TextBox ID="txtName" runat="server" MaxLength="100" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ValidationGroup="p" ErrorMessage="Name is required." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtName" ValidationGroup="p" ValidationExpression="^.{2,100}$" ErrorMessage="Name must be between 2 and 100 characters." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtEmailRo" Text="Email (cannot be changed)" />
<asp:TextBox ID="txtEmailRo" runat="server" ReadOnly="true" />
<br /><br />
<asp:Button ID="btnSave" runat="server" Text="Save changes" ValidationGroup="p" OnClick="btnSave_Click" />
<asp:Label ID="lblMsg" runat="server" CssClass="note" data-toast="ok" />
</div>
<div class="panel">
<h2>Change password</h2>
<asp:Label runat="server" AssociatedControlID="txtOld" Text="Current password" />
<asp:TextBox ID="txtOld" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="current-password" />
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtOld" ValidationGroup="pw" ErrorMessage="Enter your current password." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtNew" Text="New password" />
<asp:TextBox ID="txtNew" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="new-password" />
<div class="meter" aria-hidden="true"><i id="pwBar"></i></div><span id="pwText" class="hint" aria-live="polite"></span>
<asp:RequiredFieldValidator runat="server" ControlToValidate="txtNew" ValidationGroup="pw" ErrorMessage="Enter a new password." CssClass="err" Display="Dynamic" />
<asp:RegularExpressionValidator runat="server" ControlToValidate="txtNew" ValidationGroup="pw" ValidationExpression="^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$" ErrorMessage="Min 8 characters with upper, lower and a digit." CssClass="err" Display="Dynamic" />
<asp:Label runat="server" AssociatedControlID="txtNew2" Text="Confirm new password" />
<asp:TextBox ID="txtNew2" runat="server" TextMode="Password" ClientIDMode="Static" autocomplete="new-password" />
<asp:CompareValidator runat="server" ControlToValidate="txtNew2" ControlToCompare="txtNew" ValidationGroup="pw" ErrorMessage="Passwords do not match." CssClass="err" Display="Dynamic" />
<div class="chk"><input type="checkbox" id="showPw" /><label for="showPw">Show passwords</label></div>
<br />
<asp:Button ID="btnPw" runat="server" Text="Update password" ValidationGroup="pw" OnClick="btnPw_Click" />
<asp:Label ID="lblPwMsg" runat="server" CssClass="note" data-toast="ok" />
</div>
</div>
</section>
</asp:Content>
