<%@ Page Title="Certificate" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Certificate.aspx.cs" Inherits="CertificatePage" %>
<asp:Content ContentPlaceHolderID="Main" runat="server">
<section class="wrap">
<div class="actions noprint">
<button type="button" class="btn" data-print>Print or save as PDF</button>
<a class="btn ghost" href="<%= ResolveUrl("~/Member/Dashboard.aspx") %>">Back to my dashboard</a>
</div>
<p class="hint noprint">In the print window choose &ldquo;Save as PDF&rdquo; as the printer, landscape, and turn off headers and footers.</p>
<article class="cert" aria-label="Certificate of completion">
<div class="cert-in">
<p class="cert-brand">ONYX<span>LEARN</span></p>
<h1 class="cert-title">Certificate of Completion</h1>
<p class="cert-line">This certifies that</p>
<p class="cert-name"><asp:Label ID="lblName" runat="server" /></p>
<p class="cert-line">has completed every lesson of the course</p>
<p class="cert-course"><asp:Label ID="lblCourse" runat="server" /></p>
<p class="cert-line"><asp:Label ID="lblScore" runat="server" /></p>
<div class="cert-foot">
<div><span class="cert-lab">Date</span><asp:Label ID="lblDate" runat="server" /></div>
<div class="cert-seal" aria-hidden="true">&#10003;</div>
<div><span class="cert-lab">Reference</span><asp:Label ID="lblRef" runat="server" /></div>
</div>
</div>
</article>
</section>
</asp:Content>
