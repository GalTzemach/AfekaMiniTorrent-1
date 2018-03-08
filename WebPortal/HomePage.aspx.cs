using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class HomePage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Btn_newUser_Click(object sender, EventArgs e)
    {
        Response.Redirect("CreateNewUser.aspx");
    }

    protected void Btn_Admin_Click(object sender, EventArgs e)
    {
        Response.Redirect("AdminSignIn.aspx");

    }
}