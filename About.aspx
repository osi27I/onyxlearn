<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="About.aspx.cs" Inherits="AboutPage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<h1 class="ptitle">About Onyx Learn</h1>
<p class="prose">Onyx Learn is a web-based learning system that gives registered and non-registered users access to structured digital learning resources. Our mission is to make focused, distraction-free learning available to everyone.</p>

<h2 style="margin-top:40px">What you can do</h2>
<div class="grid">
<article class="card"><h3>Guests</h3><p>Browse and search the course catalogue, read course details and see the community discussion without signing in.</p></article>
<article class="card"><h3>Members</h3><p>Enrol in courses, work through text, image, audio and video lessons, track progress, take self-assessment quizzes and join discussions.</p></article>
<article class="card"><h3>Administrators</h3><p>Manage courses, lessons (including media uploads), quiz questions and user accounts from a protected admin area.</p></article>
</div>

<h2 style="margin-top:40px">Technology</h2>
<div class="scrollx"><table class="t">
<tr><th>Layer</th><th>Used in this project</th></tr>
<tr><td>Server-side</td><td>ASP.NET Web Forms 4.8, C#, master page, forms authentication with roles</td></tr>
<tr><td>Database</td><td>SQL Server LocalDB, created and seeded automatically on first run</td></tr>
<tr><td>Client-side</td><td>HTML5 semantic elements, external CSS3, JavaScript (validation helpers, animation)</td></tr>
<tr><td>Multimedia</td><td>HTML5 image, video and audio elements, optional embedded video</td></tr>
</table></div>

<h2 style="margin-top:40px">Project Team</h2>
<div class="scrollx"><table class="t">
<tr><th>No.</th><th>Name</th><th>Student ID</th><th>Responsibility</th></tr>
<tr><td>1</td><td>[Member 1 Name]</td><td>[TP Number]</td><td>[Responsibility]</td></tr>
<tr><td>2</td><td>[Member 2 Name]</td><td>[TP Number]</td><td>[Responsibility]</td></tr>
<tr><td>3</td><td>[Member 3 Name]</td><td>[TP Number]</td><td>[Responsibility]</td></tr>
<tr><td>4</td><td>[Member 4 Name]</td><td>[TP Number]</td><td>[Responsibility]</td></tr>
</table></div>
</section>
</asp:Content>
