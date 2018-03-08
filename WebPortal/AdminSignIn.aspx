<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AdminSignIn.aspx.cs" Inherits="AdminSignIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>
            <asp:Label ID="Label1" runat="server" Text="Sign in as Adminisrator"></asp:Label>
            </h1>
            <br />
            <asp:Label ID="Label2" runat="server" Text="UserName:" style="font-size: large"></asp:Label>
            &nbsp;&nbsp;
            <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*Please enter UserName!" ControlToValidate="UserName" ForeColor="Red"></asp:RequiredFieldValidator>
            <br />
            <br />
            <asp:Label ID="Label3" runat="server" Text="Password:" style="font-size: large"></asp:Label>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <asp:TextBox ID="Password" runat="server"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*Please enter Password!" ForeColor="Red" ControlToValidate="Password"></asp:RequiredFieldValidator>
            <br />
            <br />
            <br />
            <asp:Button ID="Button1" runat="server" Text="Log in" OnClick="Button1_Click" />
            <br />
            <br />
            <asp:Label ID="Lbl_incorrect" runat="server" Text="*Username or password is incorrect!" ForeColor="Red" Visible="False"></asp:Label>
            <br />
            <br />
        </div>
    </form>
</body>
</html>
