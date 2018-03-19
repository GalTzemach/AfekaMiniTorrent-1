<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CreateNewUser.aspx.cs" Inherits="CreateNewUser" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="text-align: center">
    <form id="form1" runat="server">
        <div style="direction: ltr">
            <h1>
                <asp:Label ID="Label1" runat="server" Text="Create a new user"></asp:Label>
            </h1>
            <asp:Table ID="Table1" runat="server" HorizontalAlign="Center" Height="166px" Width="682px">
                <asp:TableRow runat="server">
                    <asp:TableCell runat="server" HorizontalAlign="Right">
                        <asp:Label ID="Label2" runat="server" Text="UserName:" Style="font-size: large"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell runat="server">
                        <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                    </asp:TableCell>
                    <asp:TableCell runat="server" HorizontalAlign="Left">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*Please enter UserName!" ControlToValidate="UserName" ForeColor="Red"></asp:RequiredFieldValidator>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow runat="server">
                    <asp:TableCell runat="server" HorizontalAlign="Right">
                        <asp:Label ID="Label3" runat="server" Text="Password:" Style="font-size: large"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell runat="server">
                        <asp:TextBox ID="Password" runat="server"></asp:TextBox>
                    </asp:TableCell>
                    <asp:TableCell runat="server" HorizontalAlign="Left">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*Please enter Password!" ControlToValidate="Password" ForeColor="Red"></asp:RequiredFieldValidator>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow runat="server">
                    <asp:TableCell runat="server" HorizontalAlign="Right">
                        <asp:Label ID="Label4" runat="server" Text="Confirm password:" Style="font-size: large"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell runat="server">
                        <asp:TextBox ID="ConfirmPassword" runat="server"></asp:TextBox>
                    </asp:TableCell>
                    <asp:TableCell runat="server" HorizontalAlign="Left">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*Please enter Confirm Password!" ControlToValidate="ConfirmPassword" ForeColor="Red"></asp:RequiredFieldValidator>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>

            <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="*Password mismatch" ControlToCompare="Password" ForeColor="Red" ControlToValidate="ConfirmPassword" ValidateRequestMode="Enabled"></asp:CompareValidator>
            <br />
            <asp:Button ID="Btn_Create" runat="server" Text="Create" OnClick="Btn_Create_Click" />
            <br />
            <asp:Label ID="Lbl_UserNameAlreadyExist" runat="server" Text="*UserName already exist!" ForeColor="Red" Visible="False"></asp:Label>
            <br />
            <br />
        </div>
    </form>
</body>
</html>
