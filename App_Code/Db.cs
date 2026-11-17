using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

/// <summary>
/// Data-access helper. Every query uses SQL parameters (never string concatenation of user input).
/// Also creates and seeds the database on first run.
/// </summary>
public static class Db
{
    static string Cs { get { return ConfigurationManager.ConnectionStrings["Db"].ConnectionString; } }

    public static SqlParameter P(string name, object value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    public static DataTable Query(string sql, params SqlParameter[] p)
    {
        using (var c = new SqlConnection(Cs))
        using (var a = new SqlDataAdapter(sql, c))
        {
            a.SelectCommand.Parameters.AddRange(p);
            var t = new DataTable();
            a.Fill(t);
            return t;
        }
    }

    public static int Exec(string sql, params SqlParameter[] p)
    {
        using (var c = new SqlConnection(Cs))
        using (var m = new SqlCommand(sql, c))
        {
            m.Parameters.AddRange(p);
            c.Open();
            return m.ExecuteNonQuery();
        }
    }

    public static object Scalar(string sql, params SqlParameter[] p)
    {
        using (var c = new SqlConnection(Cs))
        using (var m = new SqlCommand(sql, c))
        {
            m.Parameters.AddRange(p);
            c.Open();
            return m.ExecuteScalar();
        }
    }

    /// <summary>Returns the UserId for an e-mail address, or -1 when the account no longer exists.</summary>
    public static int UserId(string email)
    {
        object o = Scalar("SELECT UserId FROM Users WHERE Email=@e", P("@e", email));
        return (o == null || o == DBNull.Value) ? -1 : Convert.ToInt32(o);
    }

    public static bool IsEnrolled(int userId, int courseId)
    {
        return Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Enrollments WHERE UserId=@u AND CourseId=@c",
            P("@u", userId), P("@c", courseId))) > 0;
    }

    public const int PassMark = 60;

    >
    public static bool IsComplete(int lessons, int done, int questions, int best)
    {
        if (lessons == 0 || done < lessons) return false;
        return questions == 0 || best >= PassMark;
    }

    public static bool CourseCompleted(int userId, int courseId)
    {
        int lessons = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Lessons WHERE CourseId=@c", P("@c", courseId)));
        int done = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM LessonProgress p JOIN Lessons l ON l.LessonId=p.LessonId WHERE p.UserId=@u AND l.CourseId=@c",
            P("@u", userId), P("@c", courseId)));
        int questions = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Questions WHERE CourseId=@c", P("@c", courseId)));
        object b = Scalar("SELECT MAX(CASE WHEN Total=0 THEN 0 ELSE 100*Score/Total END) FROM QuizAttempts WHERE UserId=@u AND CourseId=@c",
            P("@u", userId), P("@c", courseId));
        int best = (b == null || b == DBNull.Value) ? -1 : Convert.ToInt32(b);
        return IsComplete(lessons, done, questions, best);
    }

    /// <summary>Parses an integer safely (query strings come from the user).</summary>
    public static int ToInt(string s, int fallback)
    {
        int n;
        return int.TryParse(s, out n) ? n : fallback;
    }

    public static string Hash(string pw)
    {
        var salt = new byte[16];
        using (var r = RandomNumberGenerator.Create()) r.GetBytes(salt);
        using (var k = new Rfc2898DeriveBytes(pw, salt, 10000))
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(k.GetBytes(32));
    }

    public static bool Verify(string pw, string stored)
    {
        var s = stored.Split(':');
        if (s.Length != 2) return false;
        using (var k = new Rfc2898DeriveBytes(pw, Convert.FromBase64String(s[0]), 10000))
            return Convert.ToBase64String(k.GetBytes(32)) == s[1];
    }

    const string SchemaCore = @"
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
 CONSTRAINT UQ_Enroll UNIQUE(UserId, CourseId));";

    const string AlterFailed = "IF COL_LENGTH('Users','FailedLogins') IS NULL ALTER TABLE Users ADD FailedLogins INT NOT NULL CONSTRAINT DF_Users_FailedLogins DEFAULT 0";
    const string AlterLockout = "IF COL_LENGTH('Users','LockoutUntil') IS NULL ALTER TABLE Users ADD LockoutUntil DATETIME NULL";
    const string AlterCover = "IF COL_LENGTH('Courses','CoverUrl') IS NULL ALTER TABLE Courses ADD CoverUrl NVARCHAR(300) NULL";

    const string SchemaLearning = @"
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
 Body NVARCHAR(500) NOT NULL, PostedAt DATETIME NOT NULL DEFAULT GETDATE());";

    const string SchemaIndexes = @"
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lessons_Course') CREATE INDEX IX_Lessons_Course ON Lessons(CourseId, SortOrder);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Questions_Course') CREATE INDEX IX_Questions_Course ON Questions(CourseId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Attempts_UserCourse') CREATE INDEX IX_Attempts_UserCourse ON QuizAttempts(UserId, CourseId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Comments_Course') CREATE INDEX IX_Comments_Course ON CourseComments(CourseId, PostedAt);";

    public static void Init()
    {
        var b = new SqlConnectionStringBuilder(Cs);
        var name = b.InitialCatalog;
        b.InitialCatalog = "master";
        using (var c = new SqlConnection(b.ConnectionString))
        using (var m = new SqlCommand("IF DB_ID('" + name + "') IS NULL CREATE DATABASE [" + name + "]", c))
        {
            c.Open();
            m.ExecuteNonQuery();
        }
        Exec(SchemaCore);
        Exec(AlterFailed);
        Exec(AlterLockout);
        Exec(AlterCover);
        Exec(SchemaLearning);
        Exec(SchemaIndexes);

        if (Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Users")) == 0)
            Exec("INSERT INTO Users(FullName,Email,PasswordHash,Role) VALUES('Administrator','admin@onyx.local',@h,'Admin')", P("@h", Hash("Admin@123")));
        Exec("IF NOT EXISTS(SELECT 1 FROM Users WHERE Email='member@onyx.local') INSERT INTO Users(FullName,Email,PasswordHash,Role) VALUES('Demo Learner','member@onyx.local',@h,'Member')", P("@h", Hash("Member@123")));
        if (Convert.ToInt32(Scalar("SELECT COUNT(*) FROM Courses")) == 0)
            Exec(@"INSERT INTO Courses(Title,Category,Level,Description) VALUES
('Web Development Fundamentals','Programming','Beginner','HTML5, CSS3 and JavaScript from first principles with hands-on exercises.'),
('Database Design with SQL','Data','Intermediate','Entity relationship modelling, normalisation and practical SQL querying.'),
('ASP.NET Web Forms in Practice','Programming','Intermediate','Server controls, validation, state management and secure authentication.'),
('Interface Design Principles','Design','Beginner','Layout, typography, contrast and navigation patterns for usable interfaces.'),
('Cybersecurity Essentials','Security','Beginner','Threat models, secure coding habits and protecting user credentials.');");

        Seed.Run();
    }
}
