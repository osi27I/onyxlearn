using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class QuizPage : System.Web.UI.Page
{
    int uid, courseId;
    protected int courseIdValue { get { return courseId; } }

    protected void Page_Load(object sender, EventArgs e)
    {
        uid = Db.UserId(User.Identity.Name);
        courseId = Db.ToInt(Request.QueryString["course"], -1);
        var c = Db.Query("SELECT Title,IsPublished FROM Courses WHERE CourseId=@c", Db.P("@c", courseId));
        if (c.Rows.Count == 0) throw new HttpException(404, "Course not found");
        if (!User.IsInRole("Admin") && (!(bool)c.Rows[0]["IsPublished"] || !Db.IsEnrolled(uid, courseId)))
        {
            Response.Redirect("~/Course.aspx?id=" + courseId + "&msg=enrol");
            return;
        }
        if (IsPostBack) return;

        lblTitle.Text = Server.HtmlEncode((string)c.Rows[0]["Title"]) + " quiz";
        lnkCourse.Text = "&larr; Back to " + Server.HtmlEncode((string)c.Rows[0]["Title"]);
        lnkCourse.NavigateUrl = "~/Course.aspx?id=" + courseId;

        var q = Db.Query("SELECT QuestionId,QuestionText,OptionA,OptionB,OptionC,OptionD FROM Questions WHERE CourseId=@c ORDER BY QuestionId", Db.P("@c", courseId));
        pnlQuiz.Visible = q.Rows.Count > 0;
        pnlNone.Visible = q.Rows.Count == 0;
        rpQ.DataSource = q;
        rpQ.DataBind();
        BindHistory();
    }

    protected void rpQ_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
        var row = (DataRowView)e.Item.DataItem;
        var rbl = (RadioButtonList)e.Item.FindControl("rbl");
        rbl.Items.Add(new ListItem(Server.HtmlEncode(row["OptionA"].ToString()), "A"));
        rbl.Items.Add(new ListItem(Server.HtmlEncode(row["OptionB"].ToString()), "B"));
        rbl.Items.Add(new ListItem(Server.HtmlEncode(row["OptionC"].ToString()), "C"));
        rbl.Items.Add(new ListItem(Server.HtmlEncode(row["OptionD"].ToString()), "D"));
    }

    void BindHistory()
    {
        var h = Db.Query("SELECT TOP 10 Score,Total,TakenAt,CASE WHEN Total=0 THEN 0 ELSE 100*Score/Total END AS Pct FROM QuizAttempts WHERE UserId=@u AND CourseId=@c ORDER BY TakenAt DESC", Db.P("@u", uid), Db.P("@c", courseId));
        rpH.DataSource = h;
        rpH.DataBind();
        lblNoH.Visible = h.Rows.Count == 0;
        object best = Db.Scalar("SELECT MAX(CASE WHEN Total=0 THEN 0 ELSE 100*Score/Total END) FROM QuizAttempts WHERE UserId=@u AND CourseId=@c", Db.P("@u", uid), Db.P("@c", courseId));
        bool has = best != null && best != DBNull.Value;
        lblBest.Visible = has;
        lblBest.Text = has ? "Best score so far: " + best + "%" : "";
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        // The correct answers are read again from the database; nothing is trusted from the browser.
        var key = Db.Query("SELECT QuestionId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption FROM Questions WHERE CourseId=@c", Db.P("@c", courseId));
        var answers = new System.Collections.Generic.Dictionary<int, string>();
        foreach (RepeaterItem it in rpQ.Items)
        {
            var rbl = (RadioButtonList)it.FindControl("rbl");
            var hf = (HiddenField)it.FindControl("hfId");
            if (rbl.SelectedIndex < 0)
            {
                lblWarn.Text = "Please answer every question before submitting.";
                return;
            }
            answers[Db.ToInt(hf.Value, -1)] = rbl.SelectedValue;
        }

        var res = new DataTable();
        res.Columns.Add("QuestionText", typeof(string));
        res.Columns.Add("Chosen", typeof(string));
        res.Columns.Add("Correct", typeof(string));
        res.Columns.Add("Right", typeof(bool));
        int score = 0, total = 0;
        foreach (DataRow k in key.Rows)
        {
            int id = (int)k["QuestionId"];
            string chosen;
            if (!answers.TryGetValue(id, out chosen)) continue;
            string correct = ((string)k["CorrectOption"]).Trim();
            bool right = chosen == correct;
            total++;
            if (right) score++;
            res.Rows.Add(k["QuestionText"], k["Option" + chosen], k["Option" + correct], right);
        }
        if (total == 0) return;

        Db.Exec("INSERT INTO QuizAttempts(UserId,CourseId,Score,Total) VALUES(@u,@c,@s,@t)",
            Db.P("@u", uid), Db.P("@c", courseId), Db.P("@s", score), Db.P("@t", total));

        int pct = 100 * score / total;
        bool pass = pct >= Db.PassMark;
        lblScore.Text = score + " out of " + total + " correct";
        lblVerdict.Text = pass ? "Well done, you passed! The pass mark is " + Db.PassMark + "%." : "Not there yet. The pass mark is " + Db.PassMark + "%. Review the lessons and try again.";
        litRing.Text = Media.Ring(pct, pass ? "pass" : "fail", "Score " + pct + " percent");
        if (Db.CourseCompleted(uid, courseId))
        {
            lnkCert.Visible = true;
            lnkCert.NavigateUrl = "~/Member/Certificate.aspx?course=" + courseId;
        }
        else if (pass)
        {
            lblCertHint.Text = "Finish every lesson in the course to unlock your certificate.";
        }
        rpR.DataSource = res;
        rpR.DataBind();
        pnlQuiz.Visible = false;
        pnlResult.Visible = true;
        BindHistory();
    }
}
