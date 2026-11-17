using System;

/// <summary>
/// Sample learning content (lessons, quiz questions, cover images) inserted the first time the site runs.
/// Idempotent: a course that already has lessons or questions is left alone.
/// </summary>
public static class Seed
{
    static void Cover(string title, string url)
    {
        Db.Exec("UPDATE Courses SET CoverUrl=@u WHERE Title=@t AND CoverUrl IS NULL", Db.P("@u", url), Db.P("@t", title));
    }

    static int CourseId(string title)
    {
        object o = Db.Scalar("SELECT CourseId FROM Courses WHERE Title=@t", Db.P("@t", title));
        return o == null ? -1 : Convert.ToInt32(o);
    }

    static void Lesson(int cid, int order, string title, string body, string type, string url)
    {
        Db.Exec("INSERT INTO Lessons(CourseId,Title,Body,MediaType,MediaUrl,SortOrder) VALUES(@c,@t,@b,@mt,@mu,@o)",
            Db.P("@c", cid), Db.P("@t", title), Db.P("@b", body), Db.P("@mt", type), Db.P("@mu", url), Db.P("@o", order));
    }

    static void Question(int cid, string q, string a, string b, string c, string d, string correct)
    {
        Db.Exec("INSERT INTO Questions(CourseId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption) VALUES(@c,@q,@a,@b,@cc,@d,@k)",
            Db.P("@c", cid), Db.P("@q", q), Db.P("@a", a), Db.P("@b", b), Db.P("@cc", c), Db.P("@d", d), Db.P("@k", correct));
    }

    static bool Empty(string table, int cid)
    {
        return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM " + table + " WHERE CourseId=@c", Db.P("@c", cid))) == 0;
    }

    public static void Run()
    {
        Cover("Web Development Fundamentals", "~/Media/covers/web-dev.svg");
        Cover("Database Design with SQL", "~/Media/covers/sql.svg");
        Cover("ASP.NET Web Forms in Practice", "~/Media/covers/aspnet.svg");
        Cover("Interface Design Principles", "~/Media/covers/design.svg");
        Cover("Cybersecurity Essentials", "~/Media/covers/security.svg");

        SeedWeb(CourseId("Web Development Fundamentals"));
        SeedSql(CourseId("Database Design with SQL"));
        SeedAsp(CourseId("ASP.NET Web Forms in Practice"));
        SeedDesign(CourseId("Interface Design Principles"));
        SeedSecurity(CourseId("Cybersecurity Essentials"));
    }

    // ------------------------------------------------------------------ Web Development Fundamentals
    static void SeedWeb(int c)
    {
        if (c < 0) return;
        if (Empty("Lessons", c))
        {
            Lesson(c, 1, "How the web works",
                "Every time you open a web page, your browser and a web server hold a short conversation. The browser sends an HTTP request that names the page it wants. The server finds or builds that page and sends back an HTTP response, normally containing HTML.\n\n" +
                "On a dynamic site such as Onyx Learn the server does extra work before replying. ASP.NET runs the code behind the page, asks the database for the data it needs and merges the rows into HTML. The browser then downloads the CSS, JavaScript and images that the HTML points to and draws the finished page.\n\n" +
                "## Key terms\n\n" +
                "- Client: the browser running on your device.\n- Server: the computer that runs the web application and the database.\n- Request and response: one message each way, carrying a method (GET or POST), a status code and headers.\n- Status codes: 200 means success, 404 means not found and 500 means a server error.\n\n" +
                "Watch the short animation above, then open your browser's developer tools (F12), choose the Network tab and reload this page to see the real requests.",
                "Video", "~/Media/lessons/request-lifecycle.mp4");
            Lesson(c, 2, "Structuring pages with HTML5",
                "HTML5 introduced semantic elements that describe the purpose of each part of a page instead of just its appearance. A page normally has a header for the logo and title, a nav for the main links, a main area that holds the unique content and a footer for credits. Inside main, a section groups related content and an article holds a self-contained item such as a course card.\n\n" +
                "## Why semantics matter\n\n" +
                "- Screen readers can jump straight to the navigation or the main content.\n- Search engines understand which text is the heading and which is supporting content.\n- Code is easier to read and maintain than a page made only of div tags.\n\n" +
                "HTML5 also adds the video and audio elements, form input types such as email and number, and attributes like required and placeholder that give basic validation without any script. Look at the diagram: every region has one clear job.",
                "Image", "~/Media/lessons/html5-layout.png");
            Lesson(c, 3, "Styling with CSS",
                "CSS separates presentation from content. A rule has a selector, which says which elements to style, and declarations, which say how. You can place CSS in an external file linked from the head, in a style element on one page, or inline on a single element. External files are best because every page shares one file, which keeps the look consistent and the site easy to change.\n\n" +
                "## The box model\n\n" +
                "Every element is a rectangular box made of content, padding, border and margin. Padding is the space inside the border and margin is the space outside it. Setting box-sizing: border-box makes the width include padding and border, which makes layouts far easier to predict.\n\n" +
                "## Layout tools\n\n" +
                "- Flexbox arranges items in one direction and is ideal for navigation bars.\n- Grid lays out rows and columns together and is used for the course cards on the home page.\n- Media queries change the layout on small screens so the site stays usable on a phone.",
                "Image", "~/Media/lessons/box-model.png");
        }
        if (Empty("Questions", c))
        {
            Question(c, "Which HTTP status code means the requested page was not found?", "200", "301", "404", "500", "C");
            Question(c, "Which HTML5 element should hold the main navigation links?", "<nav>", "<div>", "<aside>", "<footer>", "A");
            Question(c, "In the CSS box model, which area lies outside the border and separates the box from its neighbours?", "Content", "Padding", "Outline", "Margin", "D");
            Question(c, "Which is the best place for styles that every page of a site should share?", "Inline style attributes", "A style element on each page", "An external stylesheet", "A JavaScript file", "C");
            Question(c, "What does the HTML5 required attribute do on an input field?", "Hides the field", "Stops the browser submitting the form while the field is empty", "Encrypts the value", "Sets a default value", "B");
        }
    }

    // ------------------------------------------------------------------ Database Design with SQL
    static void SeedSql(int c)
    {
        if (c < 0) return;
        if (Empty("Lessons", c))
        {
            Lesson(c, 1, "Entities and relationships",
                "Good database design starts with a conceptual model. An entity is something the system needs to store facts about, for example a user or a course. Each entity becomes a table, each fact about it becomes an attribute (column) and each instance becomes a row. A primary key uniquely identifies every row.\n\n" +
                "Entities are connected by relationships. One user can enrol in many courses and one course can have many learners, so the relationship is many-to-many. A relational database cannot store that directly, so we add a linking table, here called Enrollments, that holds a foreign key to each side. The diagram shows the result.\n\n" +
                "## Checklist for an ERD\n\n" +
                "- Give every table a primary key.\n- Use a foreign key wherever one table refers to another.\n- Mark the cardinality (one-to-one, one-to-many, many-to-many) on every line.\n- Name tables and columns clearly and consistently.",
                "Image", "~/Media/lessons/erd-example.png");
            Lesson(c, 2, "Normalisation",
                "Normalisation organises data so that every fact is stored in one place only. It prevents three classic problems: insertion anomalies, update anomalies and deletion anomalies.\n\n" +
                "## The first three normal forms\n\n" +
                "- First normal form (1NF): every column holds a single value and there are no repeating groups.\n- Second normal form (2NF): every non-key column depends on the whole primary key, not just part of it.\n- Third normal form (3NF): non-key columns depend only on the key and not on other non-key columns.\n\n" +
                "For example, if the course title were copied into every enrolment row, renaming a course would mean updating hundreds of rows. Storing the title once in Courses and linking to it by CourseId avoids that. Press play on the audio summary for a quick recap.",
                "Audio", "~/Media/lessons/normalisation-summary.mp3");
            Lesson(c, 3, "Querying with SELECT and JOIN",
                "SELECT chooses the columns, FROM names the table and WHERE filters the rows. ORDER BY sorts the result. Most real queries also combine tables, which is what JOIN is for.\n\n" +
                "## Examples\n\n" +
                "- SELECT Title, Level FROM Courses WHERE IsPublished = 1 ORDER BY Title;\n- SELECT u.FullName, c.Title FROM Users u JOIN Enrollments e ON e.UserId = u.UserId JOIN Courses c ON c.CourseId = e.CourseId;\n\n" +
                "An INNER JOIN keeps only rows that have a match on both sides, so a user with no enrolments does not appear. A LEFT JOIN keeps every row from the left table and fills the missing side with NULL. Aggregate functions such as COUNT and MAX, together with GROUP BY, summarise data, for example the number of learners per course.\n\n" +
                "## Safe SQL in an application\n\n" +
                "Never build a query by joining user input into the text. Pass values as parameters (such as @name) so the database treats them as data, not as code.",
                "Video", "~/Media/lessons/sql-join.mp4");
        }
        if (Empty("Questions", c))
        {
            Question(c, "What uniquely identifies each row in a table?", "A foreign key", "A primary key", "A view", "An index name", "B");
            Question(c, "How is a many-to-many relationship implemented in a relational database?", "By storing a comma-separated list in one column", "By copying the data into both tables", "With a linking table that holds a foreign key to each side", "By adding a second primary key", "C");
            Question(c, "Third normal form removes which kind of problem?", "Repeating groups", "Partial dependency on part of a composite key", "Dependencies between non-key columns", "Missing primary keys", "C");
            Question(c, "Which join returns only the rows that have a match in both tables?", "LEFT JOIN", "INNER JOIN", "CROSS JOIN", "FULL OUTER JOIN", "B");
            Question(c, "Why should an application use parameterised queries?", "They are quicker to type", "They make SELECT statements optional", "They encrypt the database file", "They stop user input being run as SQL code", "D");
        }
    }

    // ------------------------------------------------------------------ ASP.NET Web Forms in Practice
    static void SeedAsp(int c)
    {
        if (c < 0) return;
        if (Empty("Lessons", c))
        {
            Lesson(c, 1, "The page life cycle",
                "An ASP.NET Web Forms page is processed in a fixed sequence on every request. Knowing the order explains many common bugs.\n\n" +
                "On the first request the page is built, Page_Load runs and the HTML is rendered. When the user clicks a button the browser posts the form back to the same page, ASP.NET rebuilds the controls, restores their values from ViewState, runs Page_Load again and then runs the button's event handler.\n\n" +
                "## Practical rules\n\n" +
                "- Wrap data binding in if (!IsPostBack) so that it runs only once and does not overwrite what the user typed.\n- Re-bind the data after you insert, update or delete, otherwise the page shows old values.\n- Event handlers run after Page_Load, so do not rely on values changed in an event handler inside Page_Load.\n- A master page wraps every page with the shared header, navigation and footer.",
                "Image", "~/Media/lessons/page-lifecycle.png");
            Lesson(c, 2, "Validation controls",
                "Validation stops bad data before it reaches the database. ASP.NET provides validator controls that check input on the client with JavaScript for fast feedback and then check it again on the server.\n\n" +
                "## Common validators\n\n" +
                "- RequiredFieldValidator: the field must not be empty.\n- RegularExpressionValidator: the value must match a pattern, for example a valid email or a strong password.\n- CompareValidator: two values must match, such as password and confirm password.\n- RangeValidator and CustomValidator: numeric ranges and your own rules.\n\n" +
                "Client-side checks can be bypassed, so always test IsValid on the server before saving. Use ValidationGroup to separate forms that share a page, for instance the profile form and the change password form on the same dashboard. Finally, encode anything you display (Server.HtmlEncode or the encoded data-binding syntax) so that user input cannot inject script.",
                "None", null);
            Lesson(c, 3, "Authentication and roles",
                "Authentication proves who a user is; authorisation decides what that user may do. Forms authentication stores the result of a successful sign-in in an encrypted cookie.\n\n" +
                "In Onyx Learn the password is never stored. Only a salted PBKDF2 hash is saved, and the sign-in page compares hashes. The cookie also carries the user's role, and Web.config protects folders: everything in the Member folder needs a signed-in user and everything in the Admin folder needs the Admin role.\n\n" +
                "## Good practice\n\n" +
                "- Protect pages on the server. Hiding a menu link does not secure a page.\n- Limit failed attempts. Onyx Learn locks an account for ten minutes after five wrong passwords.\n- Mark the cookie HttpOnly so scripts cannot read it, and use HTTPS in production.\n- Re-check the user's role on the server so a demoted or deleted account loses access immediately.",
                "Audio", "~/Media/lessons/web-auth-summary.mp3");
        }
        if (Empty("Questions", c))
        {
            Question(c, "Where should initial data binding be placed so that it does not overwrite user input on a postback?", "Inside if (IsPostBack)", "Inside if (!IsPostBack)", "In the Page_Unload event", "In the Render method", "B");
            Question(c, "Which validator checks that two fields, such as password and confirm password, hold the same value?", "RangeValidator", "RequiredFieldValidator", "RegularExpressionValidator", "CompareValidator", "D");
            Question(c, "Which file defines the shared header, navigation and footer for all pages?", "Global.asax", "The master page", "Web.config", "App_Code", "B");
            Question(c, "How does Onyx Learn restrict the Admin folder to administrators?", "An authorization rule in Web.config that allows the Admin role", "By hiding the menu link", "With a CSS rule", "With a JavaScript alert", "A");
            Question(c, "Why must input also be validated on the server and not only in the browser?", "Server validation is always faster", "Client-side checks can be bypassed", "ASP.NET cannot run JavaScript", "HTML5 forbids client-side validation", "B");
        }
    }

    // ------------------------------------------------------------------ Interface Design Principles
    static void SeedDesign(int c)
    {
        if (c < 0) return;
        if (Empty("Lessons", c))
        {
            Lesson(c, 1, "Layout and visual hierarchy",
                "Visual hierarchy tells the user where to look first. Size, weight, colour and spacing all signal importance. A large bold headline is read first, supporting text second and the main button last, as numbered in the diagram.\n\n" +
                "## Principles\n\n" +
                "- Proximity: group related items together and leave space between groups.\n- Alignment: align elements to a shared grid so the page feels ordered.\n- Repetition: reuse the same colours, fonts and spacing so pages feel like one product.\n- Whitespace: empty space is not wasted; it gives content room to breathe.\n\n" +
                "A good test is the squint test. Blur your eyes and look at the page: the most important element should still stand out.",
                "Image", "~/Media/lessons/visual-hierarchy.png");
            Lesson(c, 2, "Typography and contrast",
                "Most of the interface is text, so readable typography is the biggest usability win. Choose one or two typefaces, a body size of at least 16 pixels and a line height of about 1.5 so that paragraphs are comfortable to read.\n\n" +
                "Contrast is the difference in brightness between text and its background. The Web Content Accessibility Guidelines require a ratio of at least 4.5 to 1 for normal text. The grey text in the diagram looks stylish but fails, while the dark navy passes easily.\n\n" +
                "## Accessibility checklist\n\n" +
                "- Check contrast with a free tool before you finalise colours.\n- Do not rely on colour alone. Add text or an icon to error and success messages.\n- Give every image meaningful alt text.\n- Make every control reachable with the keyboard and show a visible focus outline.",
                "Image", "~/Media/lessons/contrast-demo.png");
            Lesson(c, 3, "Navigation patterns",
                "Navigation lets users answer three questions: where am I, where can I go and how do I get back. A consistent main menu placed in the same position on every page answers the second question.\n\n" +
                "## Patterns used in this site\n\n" +
                "- A top navigation bar from the master page, with different links for guests, members and administrators.\n- Breadcrumbs that show the path, such as Courses, then Course, then Lesson.\n- Prominent calls to action, for example Enrol and Take quiz.\n- A friendly 404 page that offers a way back to the course list.\n\n" +
                "Keep menu labels short and use words your learners already know. Limit the main menu to about seven items. Mark the current page and make sure every page can be reached in three clicks or fewer.",
                "None", null);
        }
        if (Empty("Questions", c))
        {
            Question(c, "Which design principle groups related items by placing them close together?", "Proximity", "Contrast", "Repetition", "Alignment", "A");
            Question(c, "What is the minimum WCAG AA contrast ratio for normal body text?", "2 to 1", "3 to 1", "10 to 1", "4.5 to 1", "D");
            Question(c, "What do breadcrumbs mainly help users to know?", "How fast the page loaded", "Where they are within the site", "How strong their password is", "Which browser they use", "B");
            Question(c, "Which is the most accessible way to show an error message?", "Red colour only", "Hide it after two seconds", "Clear text, with colour or an icon as extra support", "A tooltip that appears on hover", "C");
            Question(c, "What does the squint test help you check?", "Page loading speed", "Which element stands out most (visual hierarchy)", "Spelling mistakes", "Database performance", "B");
        }
    }

    // ------------------------------------------------------------------ Cybersecurity Essentials
    static void SeedSecurity(int c)
    {
        if (c < 0) return;
        if (Empty("Lessons", c))
        {
            Lesson(c, 1, "Threat modelling",
                "Before you defend a system you must understand what can go wrong. Threat modelling is a structured way to do that. STRIDE is a popular checklist with six categories: Spoofing, Tampering, Repudiation, Information disclosure, Denial of service and Elevation of privilege.\n\n" +
                "Draw the data flow from the user to the browser, the web server and the database, then ask which STRIDE threat applies at each step. A stolen password is spoofing, a modified hidden field is tampering and a member opening an admin page is elevation of privilege.\n\n" +
                "## From threat to defence\n\n" +
                "- Spoofing: strong passwords, hashing and account lockout.\n- Tampering: validate on the server and never trust form data.\n- Elevation of privilege: role checks on every protected page.\n- Information disclosure: parameterised queries and custom error pages.",
                "Image", "~/Media/lessons/threat-model.png");
            Lesson(c, 2, "Protecting passwords",
                "Passwords are the most valuable data a learning platform holds, because many people reuse them on other sites. If the database leaks, plain-text passwords expose every account.\n\n" +
                "## What to do instead\n\n" +
                "- Never store the password. Store a salted hash.\n- Use a slow algorithm such as PBKDF2, bcrypt or Argon2 so that guessing is expensive.\n- Use a unique random salt for every user so identical passwords have different hashes.\n- Enforce a minimum length and a mix of characters, and limit the number of failed sign-in attempts.\n\n" +
                "As a user, choose a long passphrase, use a different one for each site and turn on two-step verification wherever it is offered. Never send passwords by email.",
                "None", null);
            Lesson(c, 3, "Injection and cross-site scripting",
                "Injection happens when untrusted input is treated as code. In SQL injection an attacker types a fragment of SQL into a login box. If the application joins that text into its query, the database obeys it and may reveal or destroy data.\n\n" +
                "Cross-site scripting (XSS) is the same idea in the browser. If a comment containing a script tag is saved and later displayed without encoding, every visitor's browser runs the attacker's script.\n\n" +
                "## Defences\n\n" +
                "- Use parameterised queries for every value, as the Db helper class in this project does.\n- HTML-encode all output. Encoding turns angle brackets into harmless text.\n- Validate the length and format of input on the server. ASP.NET request validation also rejects form values that look like markup.\n- Show generic custom error pages so attackers cannot read stack traces.",
                "None", null);
        }
        if (Empty("Questions", c))
        {
            Question(c, "What should a database store for a user's password?", "The plain text password", "A reversible encrypted copy with the key in the code", "The first three characters", "A salted hash", "D");
            Question(c, "Which STRIDE category describes a member opening an administrator page?", "Spoofing", "Elevation of privilege", "Tampering", "Repudiation", "B");
            Question(c, "Which defence is the most effective against SQL injection?", "Larger fonts", "Longer page titles", "Parameterised queries", "Hiding the sign-in button", "C");
            Question(c, "Output encoding mainly protects against which attack?", "Cross-site scripting (XSS)", "Brute-force guessing", "SQL injection", "Packet loss", "A");
            Question(c, "Why does a site lock an account after several failed sign-ins?", "To save disk space", "To slow down password guessing", "To log administrators out", "To reset passwords automatically", "B");
        }
    }
}
