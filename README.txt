ONYX LEARN - Web-based Learning System (version 3)
CT050-3-2-WAPP Web Applications (ASP.NET Web Forms 4.8, C#, SQL Server LocalDB)

HOW TO RUN
1. Visual Studio -> File > Open > Web Site -> select this folder -> Ctrl+F5.
2. The database "OnyxLearn", all tables, sample courses, lessons and quiz questions are
   created automatically on first run (requires SQL Server LocalDB, installed with Visual Studio).
   A manual script is also provided: Database/OnyxLearn.sql.
3. If you used an older version of this project, nothing needs to be deleted: new tables and
   columns are added to the existing OnyxLearn database automatically.

LOGINS
  Admin   : admin@onyx.local  / Admin@123
  Member  : member@onyx.local / Member@123   (or register a new account)
  Five failed sign-ins lock an account for 10 minutes (an admin can unlock it in Manage Users).

BEFORE SUBMITTING
- Edit the team names, TP numbers and responsibilities in About.aspx (Project Team table).
- Optionally set <compilation debug="false"> in web.config.

FEATURES
Guest     : browse/search/filter courses, course details, lessons list, read discussions
Member    : register, sign in, enrol/unenrol, text/image/audio/video lessons, mark lessons complete,
            self-assessment quizzes with scores and history, discussion comments, progress dashboard,
            edit profile, change password
            - lesson page with a course outline (ticks for finished lessons) and reading time
            - quiz shown one question at a time with a progress bar (all questions at once without JavaScript)
            - dashboard: "continue where you left off", progress ring per course, quiz score chart
            - certificate of completion (all lessons done + quiz passed), print or save as PDF
Admin     : manage courses (add/edit/delete/publish), lessons (with media upload), quiz questions,
            users (role, unlock, delete)
            - Stats page: totals, per-course table, charts, most active learners
            - CSV downloads (users, course progress, quiz results); passwords are never exported

WHAT IS NEW IN VERSION 3 (compared with version 2)
- Colour contrast and keyboard focus reviewed against WCAG 2.1 AA; one <h1> per page; labelled charts.
- No database changes: version 3 runs on the same OnyxLearn database as version 2.
- New pages: Member/Certificate.aspx, Admin/Stats.aspx, Admin/Export.aspx (CSV).
- Pass mark (60%) is defined once in Db.PassMark and used by quizzes, dashboard, certificate and stats.

FOLDERS
  Admin/    administrator pages (Admin role only)      Member/  pages for signed-in members
  App_Code/ Db.cs (data access, schema, hashing), Media.cs (HTML5 media, charts), Csv.cs (CSV export), Seed.cs (sample content)
  Media/    covers/ lessons/ uploads/                   Database/ OnyxLearn.sql
  Site.master, Site.css, Site.js, Web.sitemap, web.config, Global.asax
