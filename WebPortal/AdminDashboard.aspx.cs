using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;

public partial class AdminDashboard : System.Web.UI.Page
{
    private static OperationsDB DB;

    protected void Page_Load(object sender, EventArgs e)
    {
        DB = new OperationsDB();
        UpdateUsersCount();
        UpdateFilesCount();

        GridView1.DataBound += GridView1_DataBound;

        FileName.Attributes.Add("onkeypress", "foo");

        //this.FileName.Attributes.Add(
        //"onkeypress", "button_click(this,'" + this.Button1.ClientID + "')");
    }



    private void GridView1_DataBound(object sender, EventArgs e)
    {
        Lbl_emptyFields.Visible = false;
        Lbl_userNameExist.Visible = false;

        UpdateUsersCount();
        UpdateFilesCount();
    }

    private void GridView1_DataBinding(object sender, EventArgs e)
    {
        Lbl_emptyFields.Visible = false;
        Lbl_userNameExist.Visible = false;

        UpdateUsersCount();
        UpdateFilesCount();
    }

    private void UpdateUsersCount()
    {
        int[] userCount = DB.GetUsersCount();
        // userCount[0] = all users.
        // userCount[1] = active users.

        Lbl_totalUsers.Text = userCount[0].ToString();
        Lbl_activeUsers.Text = userCount[1].ToString();
    }

    private void UpdateFilesCount()
    {
        Lbl_totalFiles.Text = DB.GetFilesCount().ToString();
    }

    protected void Btn_addNewUser_Click(object sender, EventArgs e)
    {
        if (AnyFieldsIsEmpty())
        {
            Lbl_userNameExist.Visible = false;
            Lbl_emptyFields.Visible = true;
        }
        else if (DB.UserAlreadyExist(UserName.Text.Trim().ToString()))
        {
            Lbl_userNameExist.Visible = true;
            Lbl_emptyFields.Visible = false;
        }
        else
        {
            DB.AddNewUser(UserName.Text.Trim().ToString(), Password.Text.Trim().ToString());
            GridView1.DataBind();
            Lbl_userNameExist.Visible = false;
            Lbl_emptyFields.Visible = false;
            UserName.Text = "";
            Password.Text = "";

        }
    }

    private bool AnyFieldsIsEmpty()
    {
        return (String.IsNullOrEmpty(UserName.Text.Trim().ToString()) || String.IsNullOrEmpty(Password.Text.Trim().ToString()));
    }

    protected void Btn_search_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(FileName.Text.Trim().ToString()))
        {
            Lbl_emptyFileName.Visible = true;
        }
        else
        {
            Lbl_emptyFileName.Visible = false;
        }
    }
}