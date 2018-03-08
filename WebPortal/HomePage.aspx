<%@ Page Language="C#" AutoEventWireup="true" CodeFile="HomePage.aspx.cs" Inherits="HomePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>
                <asp:Label ID="Label1" runat="server" Text="Main menu, please choose one option:"></asp:Label>
            </h1>
            <br />
            <asp:Button ID="Btn_newUser" runat="server" Text="New user registration" OnClick="Btn_newUser_Click" />
            <br />
            <br />
            <br />
            <asp:Button ID="Btn_Admin" runat="server" Text="Administrator" OnClick="Btn_Admin_Click" />
            <br />
            <br />
            <br />
            <br />
        </div>
    </form>
</body>
</html>
