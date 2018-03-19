using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;

public partial class CreateNewUser : System.Web.UI.Page
{
    private static OperationsDB DB;

    protected void Page_Load(object sender, EventArgs e)
    {
        DB = new OperationsDB();
    }

    protected void Btn_Create_Click(object sender, EventArgs e)
    {
        if (DB.UserAlreadyExist(UserName.Text.Trim().ToString()))
        {
            Lbl_UserNameAlreadyExist.Visible = true;
        }
        else
        {
            DB.AddUser(UserName.Text.Trim().ToString(), Password.Text.Trim().ToString());
            Response.Redirect("UserCreated.aspx");
        }
    }

}