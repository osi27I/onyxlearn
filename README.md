# OnyxLearn — Web-based Learning System

An e-learning website built for the CT050-3-2 Web Applications module at Asia Pacific University (APU).

**Stack:** ASP.NET Web Forms 4.8 · C# · SQL Server LocalDB · HTML/CSS/JS

## Features
- **Guests:** browse, search and filter courses, view course details and discussions
- **Members:** register/sign in, enrol in courses, text/image/audio/video lessons, mark lessons complete, quizzes with score history, progress dashboard, certificate of completion
- **Admins:** manage courses, lessons (with media upload), quiz questions and users; stats page with charts; CSV exports
- Account lockout after 5 failed logins, hashed passwords, WCAG 2.1 AA colour contrast and keyboard focus

## How to run
1. Visual Studio → File → Open → Web Site → select this folder → `Ctrl+F5`
2. The `OnyxLearn` database, tables and sample content are created automatically on first run (needs SQL Server LocalDB, which comes with Visual Studio). A manual script is in `Database/OnyxLearn.sql`.

**Demo logins**
| Role | Email | Password |
|---|---|---|
| Admin | admin@onyx.local | Admin@123 |
| Member | member@onyx.local | Member@123 |

## Project structure
```
Admin/      admin pages (courses, lessons, questions, users, stats, export)
Member/     member pages (dashboard, lesson, quiz, certificate)
App_Code/   Db.cs (data access, schema, hashing), Media.cs, Csv.cs, Seed.cs
Media/      course covers, lesson media, uploads
Database/   OnyxLearn.sql
```
