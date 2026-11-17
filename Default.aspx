<%@ Page Title="Courses" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="hero">
<h1>Learn without limits.</h1>
<p>A digital learning platform with structured courses, video and audio lessons, self-assessment quizzes and a community of learners. Browse freely, join to enrol.</p>
</section>
<section class="wrap">
<div class="stats">
<div class="stat"><b><asp:Label ID="lblCourses" runat="server" /></b><span>Published courses</span></div>
<div class="stat"><b><asp:Label ID="lblLessons" runat="server" /></b><span>Lessons</span></div>
<div class="stat"><b><asp:Label ID="lblLearners" runat="server" /></b><span>Learners</span></div>
</div>
<h2>Course Catalogue</h2>
<div class="tools bar">
<div>
<asp:Label runat="server" AssociatedControlID="txtSearch" Text="Search" />
<asp:TextBox ID="txtSearch" runat="server" placeholder="Search by title or category" MaxLength="60" />
</div>
<div>
<asp:Label runat="server" AssociatedControlID="ddlLevel" Text="Level" />
<asp:DropDownList ID="ddlLevel" runat="server">
<asp:ListItem Value="">All levels</asp:ListItem>
<asp:ListItem>Beginner</asp:ListItem>
<asp:ListItem>Intermediate</asp:ListItem>
<asp:ListItem>Advanced</asp:ListItem>
</asp:DropDownList>
</div>
<asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CausesValidation="false" />
</div>
<asp:Label ID="lblMsg" runat="server" CssClass="note" />
<div class="grid">
<asp:Repeater ID="rp" runat="server" OnItemCommand="rp_ItemCommand">
<ItemTemplate>
<article class="card">
<img class="cover" src="<%# Media.CoverOrDefault(Eval("CoverUrl")) %>" alt="" loading="lazy" />
<small><%#: Eval("Category") %> &middot; <%#: Eval("Level") %></small>
<h3><a href="<%# ResolveUrl("~/Course.aspx") %>?id=<%# Eval("CourseId") %>"><%#: Eval("Title") %></a></h3>
<p><%#: Eval("Description") %></p>
<div class="meta"><span><%# Media.Plural((int)Eval("Lessons"), "lesson") %></span><span><%# Media.Plural((int)Eval("Learners"), "learner") %></span></div>
<div class="actions">
<asp:LinkButton runat="server" CssClass="btn" CommandName="enroll" CommandArgument='<%# Eval("CourseId") %>' CausesValidation="false" Visible='<%# (int)Eval("Enrolled") == 0 %>'>Enrol</asp:LinkButton>
<asp:HyperLink runat="server" CssClass="btn" NavigateUrl='<%# "~/Course.aspx?id=" + Eval("CourseId") %>' Visible='<%# (int)Eval("Enrolled") > 0 %>'>Continue</asp:HyperLink>
<a class="btn ghost" href="<%# ResolveUrl("~/Course.aspx") %>?id=<%# Eval("CourseId") %>">Details</a>
</div>
</article>
</ItemTemplate>
</asp:Repeater>
</div>
</section>
</asp:Content>
