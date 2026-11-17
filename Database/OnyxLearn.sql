/* ============================================================================
   Onyx Learn - database script (SQL Server / LocalDB)
   CT050-3-2-WAPP Web Applications

   The website creates and seeds this database automatically on first run, so you
   normally do NOT need this file. Run it in SSMS / Visual Studio (SQL Server Object
   Explorer) only if you want to create the database manually.

   Logins created:   admin@onyx.local  / Admin@123   (Admin)
                     member@onyx.local / Member@123  (Member)
   ============================================================================ */
IF DB_ID('OnyxLearn') IS NULL CREATE DATABASE OnyxLearn;
GO
USE OnyxLearn;
GO

-- 1. Core tables
IF OBJECT_ID('Users') IS NULL CREATE TABLE Users(
 UserId INT IDENTITY PRIMARY KEY, FullName NVARCHAR(100) NOT NULL, Email NVARCHAR(150) NOT NULL UNIQUE,
 PasswordHash NVARCHAR(200) NOT NULL, Role NVARCHAR(20) NOT NULL DEFAULT 'Member', CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
 FailedLogins INT NOT NULL DEFAULT 0, LockoutUntil DATETIME NULL);
IF OBJECT_ID('Courses') IS NULL CREATE TABLE Courses(
 CourseId INT IDENTITY PRIMARY KEY, Title NVARCHAR(150) NOT NULL, Category NVARCHAR(60) NOT NULL,
 Level NVARCHAR(30) NOT NULL, Description NVARCHAR(500) NOT NULL, IsPublished BIT NOT NULL DEFAULT 1, CoverUrl NVARCHAR(300) NULL);
IF OBJECT_ID('Enrollments') IS NULL CREATE TABLE Enrollments(
 EnrollmentId INT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
 CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE, EnrolledAt DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT UQ_Enroll UNIQUE(UserId, CourseId));
GO

-- 2. Learning module tables
IF OBJECT_ID('Lessons') IS NULL CREATE TABLE Lessons(
 LessonId INT IDENTITY PRIMARY KEY, CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE,
 Title NVARCHAR(150) NOT NULL, Body NVARCHAR(MAX) NOT NULL, MediaType NVARCHAR(10) NOT NULL DEFAULT 'None',
 MediaUrl NVARCHAR(300) NULL, SortOrder INT NOT NULL DEFAULT 1);
IF OBJECT_ID('LessonProgress') IS NULL CREATE TABLE LessonProgress(
 UserId INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
 LessonId INT NOT NULL REFERENCES Lessons(LessonId) ON DELETE CASCADE,
 CompletedAt DATETIME NOT NULL DEFAULT GETDATE(), CONSTRAINT PK_LessonProgress PRIMARY KEY(UserId, LessonId));
IF OBJECT_ID('Questions') IS NULL CREATE TABLE Questions(
 QuestionId INT IDENTITY PRIMARY KEY, CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE,
 QuestionText NVARCHAR(300) NOT NULL, OptionA NVARCHAR(200) NOT NULL, OptionB NVARCHAR(200) NOT NULL,
 OptionC NVARCHAR(200) NOT NULL, OptionD NVARCHAR(200) NOT NULL,
 CorrectOption CHAR(1) NOT NULL CONSTRAINT CK_Questions_Correct CHECK (CorrectOption IN ('A','B','C','D')));
IF OBJECT_ID('QuizAttempts') IS NULL CREATE TABLE QuizAttempts(
 AttemptId INT IDENTITY PRIMARY KEY, UserId INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
 CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE,
 Score INT NOT NULL, Total INT NOT NULL, TakenAt DATETIME NOT NULL DEFAULT GETDATE());
IF OBJECT_ID('CourseComments') IS NULL CREATE TABLE CourseComments(
 CommentId INT IDENTITY PRIMARY KEY, CourseId INT NOT NULL REFERENCES Courses(CourseId) ON DELETE CASCADE,
 UserId INT NOT NULL REFERENCES Users(UserId) ON DELETE CASCADE,
 Body NVARCHAR(500) NOT NULL, PostedAt DATETIME NOT NULL DEFAULT GETDATE());
GO

-- 3. Indexes
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lessons_Course') CREATE INDEX IX_Lessons_Course ON Lessons(CourseId, SortOrder);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Questions_Course') CREATE INDEX IX_Questions_Course ON Questions(CourseId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Attempts_UserCourse') CREATE INDEX IX_Attempts_UserCourse ON QuizAttempts(UserId, CourseId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Comments_Course') CREATE INDEX IX_Comments_Course ON CourseComments(CourseId, PostedAt);
GO

-- 4. Seed data: users
IF NOT EXISTS(SELECT 1 FROM Users WHERE Email='admin@onyx.local')
 INSERT INTO Users(FullName,Email,PasswordHash,Role) VALUES(N'Administrator','admin@onyx.local','Vk+Cg27G2wLfS19lxbyL6w==:BqYkpUofW22kj0+dexG3tKAXtRt4XiCkGb3bHu9Cg6c=','Admin');
IF NOT EXISTS(SELECT 1 FROM Users WHERE Email='member@onyx.local')
 INSERT INTO Users(FullName,Email,PasswordHash,Role) VALUES(N'Demo Learner','member@onyx.local','Xjgv1rE322wOzvNG+VdREA==:uccao/1bnfglmzoICzINeA/nM98aloPubijQIIZ8Rew=','Member');
GO

-- 5. Seed data: courses (ids 1-5 on a fresh database)
IF NOT EXISTS(SELECT 1 FROM Courses)
INSERT INTO Courses(Title,Category,Level,Description,CoverUrl) VALUES
(N'Web Development Fundamentals',N'Programming',N'Beginner',N'HTML5, CSS3 and JavaScript from first principles with hands-on exercises.',N'~/Media/covers/web-dev.svg'),
(N'Database Design with SQL',N'Data',N'Intermediate',N'Entity relationship modelling, normalisation and practical SQL querying.',N'~/Media/covers/sql.svg'),
(N'ASP.NET Web Forms in Practice',N'Programming',N'Intermediate',N'Server controls, validation, state management and secure authentication.',N'~/Media/covers/aspnet.svg'),
(N'Interface Design Principles',N'Design',N'Beginner',N'Layout, typography, contrast and navigation patterns for usable interfaces.',N'~/Media/covers/design.svg'),
(N'Cybersecurity Essentials',N'Security',N'Beginner',N'Threat models, secure coding habits and protecting user credentials.',N'~/Media/covers/security.svg');
GO

-- 6. Seed data: lessons
IF NOT EXISTS(SELECT 1 FROM Lessons)
INSERT INTO Lessons(CourseId,Title,Body,MediaType,MediaUrl,SortOrder) VALUES
(1,N'How the web works',N'Every time you open a web page, your browser and a web server hold a short conversation. The browser sends an HTTP request that names the page it wants. The server finds or builds that page and sends back an HTTP response, normally containing HTML.

On a dynamic site such as Onyx Learn the server does extra work before replying. ASP.NET runs the code behind the page, asks the database for the data it needs and merges the rows into HTML. The browser then downloads the CSS, JavaScript and images that the HTML points to and draws the finished page.

## Key terms

- Client: the browser running on your device.
- Server: the computer that runs the web application and the database.
- Request and response: one message each way, carrying a method (GET or POST), a status code and headers.
- Status codes: 200 means success, 404 means not found and 500 means a server error.

Watch the short animation above, then open your browser''s developer tools (F12), choose the Network tab and reload this page to see the real requests.',N'Video',N'~/Media/lessons/request-lifecycle.mp4',1),
(1,N'Structuring pages with HTML5',N'HTML5 introduced semantic elements that describe the purpose of each part of a page instead of just its appearance. A page normally has a header for the logo and title, a nav for the main links, a main area that holds the unique content and a footer for credits. Inside main, a section groups related content and an article holds a self-contained item such as a course card.

## Why semantics matter

- Screen readers can jump straight to the navigation or the main content.
- Search engines understand which text is the heading and which is supporting content.
- Code is easier to read and maintain than a page made only of div tags.

HTML5 also adds the video and audio elements, form input types such as email and number, and attributes like required and placeholder that give basic validation without any script. Look at the diagram: every region has one clear job.',N'Image',N'~/Media/lessons/html5-layout.png',2),
(1,N'Styling with CSS',N'CSS separates presentation from content. A rule has a selector, which says which elements to style, and declarations, which say how. You can place CSS in an external file linked from the head, in a style element on one page, or inline on a single element. External files are best because every page shares one file, which keeps the look consistent and the site easy to change.

## The box model

Every element is a rectangular box made of content, padding, border and margin. Padding is the space inside the border and margin is the space outside it. Setting box-sizing: border-box makes the width include padding and border, which makes layouts far easier to predict.

## Layout tools

- Flexbox arranges items in one direction and is ideal for navigation bars.
- Grid lays out rows and columns together and is used for the course cards on the home page.
- Media queries change the layout on small screens so the site stays usable on a phone.',N'Image',N'~/Media/lessons/box-model.png',3),
(2,N'Entities and relationships',N'Good database design starts with a conceptual model. An entity is something the system needs to store facts about, for example a user or a course. Each entity becomes a table, each fact about it becomes an attribute (column) and each instance becomes a row. A primary key uniquely identifies every row.

Entities are connected by relationships. One user can enrol in many courses and one course can have many learners, so the relationship is many-to-many. A relational database cannot store that directly, so we add a linking table, here called Enrollments, that holds a foreign key to each side. The diagram shows the result.

## Checklist for an ERD

- Give every table a primary key.
- Use a foreign key wherever one table refers to another.
- Mark the cardinality (one-to-one, one-to-many, many-to-many) on every line.
- Name tables and columns clearly and consistently.',N'Image',N'~/Media/lessons/erd-example.png',1),
(2,N'Normalisation',N'Normalisation organises data so that every fact is stored in one place only. It prevents three classic problems: insertion anomalies, update anomalies and deletion anomalies.

## The first three normal forms

- First normal form (1NF): every column holds a single value and there are no repeating groups.
- Second normal form (2NF): every non-key column depends on the whole primary key, not just part of it.
- Third normal form (3NF): non-key columns depend only on the key and not on other non-key columns.

For example, if the course title were copied into every enrolment row, renaming a course would mean updating hundreds of rows. Storing the title once in Courses and linking to it by CourseId avoids that. Press play on the audio summary for a quick recap.',N'Audio',N'~/Media/lessons/normalisation-summary.mp3',2),
(2,N'Querying with SELECT and JOIN',N'SELECT chooses the columns, FROM names the table and WHERE filters the rows. ORDER BY sorts the result. Most real queries also combine tables, which is what JOIN is for.

## Examples

- SELECT Title, Level FROM Courses WHERE IsPublished = 1 ORDER BY Title;
- SELECT u.FullName, c.Title FROM Users u JOIN Enrollments e ON e.UserId = u.UserId JOIN Courses c ON c.CourseId = e.CourseId;

An INNER JOIN keeps only rows that have a match on both sides, so a user with no enrolments does not appear. A LEFT JOIN keeps every row from the left table and fills the missing side with NULL. Aggregate functions such as COUNT and MAX, together with GROUP BY, summarise data, for example the number of learners per course.

## Safe SQL in an application

Never build a query by joining user input into the text. Pass values as parameters (such as @name) so the database treats them as data, not as code.',N'Video',N'~/Media/lessons/sql-join.mp4',3),
(3,N'The page life cycle',N'An ASP.NET Web Forms page is processed in a fixed sequence on every request. Knowing the order explains many common bugs.

On the first request the page is built, Page_Load runs and the HTML is rendered. When the user clicks a button the browser posts the form back to the same page, ASP.NET rebuilds the controls, restores their values from ViewState, runs Page_Load again and then runs the button''s event handler.

## Practical rules

- Wrap data binding in if (!IsPostBack) so that it runs only once and does not overwrite what the user typed.
- Re-bind the data after you insert, update or delete, otherwise the page shows old values.
- Event handlers run after Page_Load, so do not rely on values changed in an event handler inside Page_Load.
- A master page wraps every page with the shared header, navigation and footer.',N'Image',N'~/Media/lessons/page-lifecycle.png',1),
(3,N'Validation controls',N'Validation stops bad data before it reaches the database. ASP.NET provides validator controls that check input on the client with JavaScript for fast feedback and then check it again on the server.

## Common validators

- RequiredFieldValidator: the field must not be empty.
- RegularExpressionValidator: the value must match a pattern, for example a valid email or a strong password.
- CompareValidator: two values must match, such as password and confirm password.
- RangeValidator and CustomValidator: numeric ranges and your own rules.

Client-side checks can be bypassed, so always test IsValid on the server before saving. Use ValidationGroup to separate forms that share a page, for instance the profile form and the change password form on the same dashboard. Finally, encode anything you display (Server.HtmlEncode or the encoded data-binding syntax) so that user input cannot inject script.',N'None',NULL,2),
(3,N'Authentication and roles',N'Authentication proves who a user is; authorisation decides what that user may do. Forms authentication stores the result of a successful sign-in in an encrypted cookie.

In Onyx Learn the password is never stored. Only a salted PBKDF2 hash is saved, and the sign-in page compares hashes. The cookie also carries the user''s role, and Web.config protects folders: everything in the Member folder needs a signed-in user and everything in the Admin folder needs the Admin role.

## Good practice

- Protect pages on the server. Hiding a menu link does not secure a page.
- Limit failed attempts. Onyx Learn locks an account for ten minutes after five wrong passwords.
- Mark the cookie HttpOnly so scripts cannot read it, and use HTTPS in production.
- Re-check the user''s role on the server so a demoted or deleted account loses access immediately.',N'Audio',N'~/Media/lessons/web-auth-summary.mp3',3),
(4,N'Layout and visual hierarchy',N'Visual hierarchy tells the user where to look first. Size, weight, colour and spacing all signal importance. A large bold headline is read first, supporting text second and the main button last, as numbered in the diagram.

## Principles

- Proximity: group related items together and leave space between groups.
- Alignment: align elements to a shared grid so the page feels ordered.
- Repetition: reuse the same colours, fonts and spacing so pages feel like one product.
- Whitespace: empty space is not wasted; it gives content room to breathe.

A good test is the squint test. Blur your eyes and look at the page: the most important element should still stand out.',N'Image',N'~/Media/lessons/visual-hierarchy.png',1),
(4,N'Typography and contrast',N'Most of the interface is text, so readable typography is the biggest usability win. Choose one or two typefaces, a body size of at least 16 pixels and a line height of about 1.5 so that paragraphs are comfortable to read.

Contrast is the difference in brightness between text and its background. The Web Content Accessibility Guidelines require a ratio of at least 4.5 to 1 for normal text. The grey text in the diagram looks stylish but fails, while the dark navy passes easily.

## Accessibility checklist

- Check contrast with a free tool before you finalise colours.
- Do not rely on colour alone. Add text or an icon to error and success messages.
- Give every image meaningful alt text.
- Make every control reachable with the keyboard and show a visible focus outline.',N'Image',N'~/Media/lessons/contrast-demo.png',2),
(4,N'Navigation patterns',N'Navigation lets users answer three questions: where am I, where can I go and how do I get back. A consistent main menu placed in the same position on every page answers the second question.

## Patterns used in this site

- A top navigation bar from the master page, with different links for guests, members and administrators.
- Breadcrumbs that show the path, such as Courses, then Course, then Lesson.
- Prominent calls to action, for example Enrol and Take quiz.
- A friendly 404 page that offers a way back to the course list.

Keep menu labels short and use words your learners already know. Limit the main menu to about seven items. Mark the current page and make sure every page can be reached in three clicks or fewer.',N'None',NULL,3),
(5,N'Threat modelling',N'Before you defend a system you must understand what can go wrong. Threat modelling is a structured way to do that. STRIDE is a popular checklist with six categories: Spoofing, Tampering, Repudiation, Information disclosure, Denial of service and Elevation of privilege.

Draw the data flow from the user to the browser, the web server and the database, then ask which STRIDE threat applies at each step. A stolen password is spoofing, a modified hidden field is tampering and a member opening an admin page is elevation of privilege.

## From threat to defence

- Spoofing: strong passwords, hashing and account lockout.
- Tampering: validate on the server and never trust form data.
- Elevation of privilege: role checks on every protected page.
- Information disclosure: parameterised queries and custom error pages.',N'Image',N'~/Media/lessons/threat-model.png',1),
(5,N'Protecting passwords',N'Passwords are the most valuable data a learning platform holds, because many people reuse them on other sites. If the database leaks, plain-text passwords expose every account.

## What to do instead

- Never store the password. Store a salted hash.
- Use a slow algorithm such as PBKDF2, bcrypt or Argon2 so that guessing is expensive.
- Use a unique random salt for every user so identical passwords have different hashes.
- Enforce a minimum length and a mix of characters, and limit the number of failed sign-in attempts.

As a user, choose a long passphrase, use a different one for each site and turn on two-step verification wherever it is offered. Never send passwords by email.',N'None',NULL,2),
(5,N'Injection and cross-site scripting',N'Injection happens when untrusted input is treated as code. In SQL injection an attacker types a fragment of SQL into a login box. If the application joins that text into its query, the database obeys it and may reveal or destroy data.

Cross-site scripting (XSS) is the same idea in the browser. If a comment containing a script tag is saved and later displayed without encoding, every visitor''s browser runs the attacker''s script.

## Defences

- Use parameterised queries for every value, as the Db helper class in this project does.
- HTML-encode all output. Encoding turns angle brackets into harmless text.
- Validate the length and format of input on the server. ASP.NET request validation also rejects form values that look like markup.
- Show generic custom error pages so attackers cannot read stack traces.',N'None',NULL,3);
GO

-- 7. Seed data: quiz questions
IF NOT EXISTS(SELECT 1 FROM Questions)
INSERT INTO Questions(CourseId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption) VALUES
(1,N'Which HTTP status code means the requested page was not found?',N'200',N'301',N'404',N'500',N'C'),
(1,N'Which HTML5 element should hold the main navigation links?',N'<nav>',N'<div>',N'<aside>',N'<footer>',N'A'),
(1,N'In the CSS box model, which area lies outside the border and separates the box from its neighbours?',N'Content',N'Padding',N'Outline',N'Margin',N'D'),
(1,N'Which is the best place for styles that every page of a site should share?',N'Inline style attributes',N'A style element on each page',N'An external stylesheet',N'A JavaScript file',N'C'),
(1,N'What does the HTML5 required attribute do on an input field?',N'Hides the field',N'Stops the browser submitting the form while the field is empty',N'Encrypts the value',N'Sets a default value',N'B'),
(2,N'What uniquely identifies each row in a table?',N'A foreign key',N'A primary key',N'A view',N'An index name',N'B'),
(2,N'How is a many-to-many relationship implemented in a relational database?',N'By storing a comma-separated list in one column',N'By copying the data into both tables',N'With a linking table that holds a foreign key to each side',N'By adding a second primary key',N'C'),
(2,N'Third normal form removes which kind of problem?',N'Repeating groups',N'Partial dependency on part of a composite key',N'Dependencies between non-key columns',N'Missing primary keys',N'C'),
(2,N'Which join returns only the rows that have a match in both tables?',N'LEFT JOIN',N'INNER JOIN',N'CROSS JOIN',N'FULL OUTER JOIN',N'B'),
(2,N'Why should an application use parameterised queries?',N'They are quicker to type',N'They make SELECT statements optional',N'They encrypt the database file',N'They stop user input being run as SQL code',N'D'),
(3,N'Where should initial data binding be placed so that it does not overwrite user input on a postback?',N'Inside if (IsPostBack)',N'Inside if (!IsPostBack)',N'In the Page_Unload event',N'In the Render method',N'B'),
(3,N'Which validator checks that two fields, such as password and confirm password, hold the same value?',N'RangeValidator',N'RequiredFieldValidator',N'RegularExpressionValidator',N'CompareValidator',N'D'),
(3,N'Which file defines the shared header, navigation and footer for all pages?',N'Global.asax',N'The master page',N'Web.config',N'App_Code',N'B'),
(3,N'How does Onyx Learn restrict the Admin folder to administrators?',N'An authorization rule in Web.config that allows the Admin role',N'By hiding the menu link',N'With a CSS rule',N'With a JavaScript alert',N'A'),
(3,N'Why must input also be validated on the server and not only in the browser?',N'Server validation is always faster',N'Client-side checks can be bypassed',N'ASP.NET cannot run JavaScript',N'HTML5 forbids client-side validation',N'B'),
(4,N'Which design principle groups related items by placing them close together?',N'Proximity',N'Contrast',N'Repetition',N'Alignment',N'A'),
(4,N'What is the minimum WCAG AA contrast ratio for normal body text?',N'2 to 1',N'3 to 1',N'10 to 1',N'4.5 to 1',N'D'),
(4,N'What do breadcrumbs mainly help users to know?',N'How fast the page loaded',N'Where they are within the site',N'How strong their password is',N'Which browser they use',N'B'),
(4,N'Which is the most accessible way to show an error message?',N'Red colour only',N'Hide it after two seconds',N'Clear text, with colour or an icon as extra support',N'A tooltip that appears on hover',N'C'),
(4,N'What does the squint test help you check?',N'Page loading speed',N'Which element stands out most (visual hierarchy)',N'Spelling mistakes',N'Database performance',N'B'),
(5,N'What should a database store for a user''s password?',N'The plain text password',N'A reversible encrypted copy with the key in the code',N'The first three characters',N'A salted hash',N'D'),
(5,N'Which STRIDE category describes a member opening an administrator page?',N'Spoofing',N'Elevation of privilege',N'Tampering',N'Repudiation',N'B'),
(5,N'Which defence is the most effective against SQL injection?',N'Larger fonts',N'Longer page titles',N'Parameterised queries',N'Hiding the sign-in button',N'C'),
(5,N'Output encoding mainly protects against which attack?',N'Cross-site scripting (XSS)',N'Brute-force guessing',N'SQL injection',N'Packet loss',N'A'),
(5,N'Why does a site lock an account after several failed sign-ins?',N'To save disk space',N'To slow down password guessing',N'To log administrators out',N'To reset passwords automatically',N'B');
GO
