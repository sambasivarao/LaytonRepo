<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeFile="SysWhiteBoard.aspx.cs" Inherits="Admin_SysWhiteBoard"  %>

<%@ Register Assembly="Infragistics35.Web.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.Web.UI.EditorControls" TagPrefix="ig" %>

<%@ Register Assembly="Infragistics35.WebUI.WebDateChooser.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.WebUI.WebSchedule" TagPrefix="igsch" %>

<%@ Register Assembly="Infragistics35.Web.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.Web.UI.ListControls" TagPrefix="ig" %>

<%@ Register Assembly="Infragistics35.Web.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb"
    Namespace="Infragistics.Web.UI.LayoutControls" TagPrefix="ig" %>

<%@ Register assembly="Infragistics35.Web.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb" namespace="Infragistics.Web.UI.GridControls" tagprefix="ig" %>

<%@ Register assembly="Infragistics35.WebUI.WebDataInput.v11.1, Version=11.1.20111.2238, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb" namespace="Infragistics.WebUI.WebDataInput" tagprefix="igtxt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script type="text/javascript" language="javascript">

        function initDropDown(sender, args) {
            sender.behavior.set_zIndex(200000);
        }

        function dg_RowSelectionChanged(sender, args) {
            
        }

        function btnAddPre_Clicked(sender) {
            //document.getElementById("OpType").value = "Add";
            document.getElementById('<%=textOP.ClientID %>').value = "Add";

            document.getElementById('<%=textMessage.ClientID %>').value = '';
            document.getElementById('<%=textAnalGrp.ClientID %>').value = '';
            document.getElementById('<%=textSite.ClientID %>').value = '';
            document.getElementById('<%=textDept.ClientID %>').value = '';
            document.getElementById('<%=textAuthUser.ClientID %>').value = '<%=Session["User"].ToString() %>';
            

            var msg = document.getElementById('<%=lbMSG.ClientID %>');
            msg.innerHTML ="";

            var dialog = $find('<%=dialogInfo.ClientID %>');
            dialog.show();

            var header = dialog.get_header();
            header.setCaptionText("Add Message");
        }
        
        function btnDelPre_Clicked(sender) {
            //// written by sparsh ID 178

            document.getElementById('<%=textNameH.ClientID %>').value = sender.alt;

            var msg = document.getElementById('<%=lbDelMSG.ClientID %>');
            msg.innerHTML = "Do you wish to delete white board message" + " ?";

            document.getElementById('<%=btnDel.ClientID %>').style.visibility = "visible";

            var dialog = $find("<%=dialogDelConfirm.ClientID%>");
            if (dialog != null) {
                dialog.Top = '100px';
                dialog.Left = '100px';
                dialog.show();
            }

            //// End by Sparsh ID 178
        }
        function selectsite(element) {
            wguide = window.open("SelSite.aspx?element=" + element, "wguide", "width=500,height=500,resizable=yes,scrollbars=yes");
        }
        function effect_site(value, element) {
            document.getElementById(element).value = value;
            document.getElementById('<%=sys_siteidH.ClientID %>').value = value;
        }

        function selectdept(element) {
            wguide = window.open("SelDept.aspx?element=" + element, "wguide", "width=500,height=500,resizable=yes,scrollbars=yes");
        }
        function effect_dept(value, element) {
            document.getElementById(element).value = value;
            document.getElementById('<%=sys_eclient_idH.ClientID %>').value = value;
        }
        function selectanalgroup(element)
        {
            wguide = window.open("SelAnalGrp.aspx?element=" + element +"&suggestgrp=no&requesttype=&assignedto=", "wguide", "width=330,height=450,resizable=yes,scrollbars=yes");
        }

        function effect_analgroup(value, element,assignedtovalue)
        {
            document.getElementById(element).value = value;
            document.getElementById('<%=sys_assignedtoanalgroupH.ClientID %>').value = value;
        }
        

    </script>
    <style type="text/css">
        .style1
        {
            height: 32px;
        }
        .style2
        {
            width: 126px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
      <table>
        <tr>
            <td>
                <asp:Label ID="lbTTL" runat="server" Text="Manage WhiteBoard"></asp:Label>
            </td>
        </tr>
        <tr>
            <td><br /></td>
        </tr>
        <tr>
            <td align="right">
                <asp:ImageButton ID="btnAddPre" runat="server" ImageUrl="Application_Images/24x24/add_icon_24px.png" OnClientClick="btnAddPre_Clicked(this);return false;" AlternateText="Add" />
            </td>
        </tr>
        <tr>
            <td>
              <ig:WebDataGrid ID="dg" runat="server" Width="850px" AutoGenerateColumns="False">
                    <Behaviors>
                    </Behaviors>
                    <EmptyRowsTemplate>
                        <asp:Label ID="Label23" runat="server" Text="No Message"></asp:Label>
                    </EmptyRowsTemplate>
                     <Columns>
                        <ig:TemplateDataField Key="sys_whiteboarddate" Width="20%">
                            <ItemTemplate>
                                <asp:LinkButton ID="sys_whiteboarddate" runat="server" onclick="btnEditPre_Clicked" CommandArgument='<%# Eval("sys_whiteboard_id") %>'><%# Eval("sys_whiteboarddate")%></asp:LinkButton>
                            </ItemTemplate>
                            <Header Text="Start Date" />
                        </ig:TemplateDataField>
                        <ig:TemplateDataField Key="sys_whiteboarddateend" Width="20%">
                            <ItemTemplate>
                                <asp:LinkButton ID="sys_whiteboarddateend" runat="server" onclick="btnEditPre_Clicked" CommandArgument='<%# Eval("sys_whiteboard_id") %>'><%# Eval("sys_whiteboarddateend")%></asp:LinkButton>
                            </ItemTemplate>
                            <Header Text="End Date" />
                        </ig:TemplateDataField>
                        <ig:TemplateDataField Key="sys_whiteboardsubject" Width="40%" CssClass="Col_Left">
                            <ItemTemplate>
                                <asp:LinkButton ID="sys_whiteboardsubject" runat="server" onclick="btnEditPre_Clicked" CommandArgument='<%# Eval("sys_whiteboard_id") %>'><%# Eval("sys_whiteboardsubject")%></asp:LinkButton>
                            </ItemTemplate>
                            <Header Text="Message" />
                        </ig:TemplateDataField>
                        <ig:TemplateDataField Key="PubPriType" Width="10%">
                            <ItemTemplate>
                                <asp:LinkButton ID="PubPriType" runat="server" onclick="btnEditPre_Clicked" CommandArgument='<%# Eval("sys_whiteboard_id") %>'><%# GetPriPub(DataBinder.Eval(Container, "DataItem.sys_visible_euser")) %></asp:LinkButton>
                            </ItemTemplate>
                            <Header Text="Type" />
                        </ig:TemplateDataField>
                        <ig:TemplateDataField Key="Delete" Width="10%">
                            <ItemTemplate>
                            <%--Add OnClientClick event by Sparsh ID 178 --%>
                                <asp:ImageButton ID="btnDelPre" runat="server" ImageUrl="Application_Images/16x16/delete_icon_16px.png" 
                                OnClientClick="btnDelPre_Clicked(this);return false;" 
                                CommandArgument='<%# Eval("sys_whiteboard_id") %>' AlternateText='<%# Eval("sys_whiteboard_id") %>' />
                            </ItemTemplate>
                            <Header Text="" />
                        </ig:TemplateDataField>
                    </Columns>
               </ig:WebDataGrid>
            </td>
        </tr>
      </table>
      
      <ig:WebDialogWindow ID="dialogInfo" runat="server" Height="420px" Width="542px" Left="150px" 
                                Top="150px" Modal="True">
        <ContentPane>
            <Template>
                <table>
                    <tr>
                        <td>
                            <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td height="5" colspan="2"></td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label24" runat="server" Text="Start"></asp:Label>
                        </td>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <ig:WebDatePicker ID="dateStart" runat="server" DisplayModeFormat="g" 
                                            EditModeFormat="g">
                                            <Buttons SpinButtonsDisplay="OnRight">
                                            </Buttons>
                                        </ig:WebDatePicker>
                                    </td>
                                    <td>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label25" runat="server" Text="End"></asp:Label>
                        </td>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <ig:WebDatePicker ID="dateEnd" runat="server" DisplayModeFormat="g" 
                                            EditModeFormat="g">
                                            <Buttons SpinButtonsDisplay="OnRight">
                                            </Buttons>
                                        </ig:WebDatePicker>
                                    </td>
                                    <td>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lb" runat="server" Text="Message"></asp:Label>
                        </td>
                        <td class="style2" colspan="2">
                            <asp:TextBox ID="textMessage" runat="server" Width="400px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lbAuthUser" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="textAuthUser" runat="server" Enabled="false"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="Label2" runat="server" Text="Display To"></asp:Label>
                        </td>
                        <td>
                            <ig:WebDropDown ID="ddDispTo" runat="server" Width="200px" EnableDropDownAsChild="False" DisplayMode="DropDownList" DropDownContainerHeight="0px">
                                <ClientEvents Initialize="initDropDown" />
                            </ig:WebDropDown>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lbUserGrp" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="textAnalGrp" runat="server" ReadOnly="True"></asp:TextBox>
                            <img title="Select <%=Application["appuserterm"].ToString() %> Group" src="Application_Images/16x16/select_16px.png" onclick="selectanalgroup('<%=textAnalGrp.ClientID%>'); return false" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lbSite" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="textSite" runat="server" ReadOnly="true"></asp:TextBox>
                            <img title="Select <%=Application["appsiteterm"].ToString() %>" src="Application_Images/16x16/select_16px.png" onclick="selectsite('<%=textSite.ClientID%>'); return false" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lbDept" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="textDept" runat="server" ReadOnly="true"></asp:TextBox>
                            <img title="Select <%=Application["appeclientterm"].ToString() %>" src="Application_Images/16x16/select_16px.png" onclick="selectdept('<%=textDept.ClientID%>'); return false" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Label ID="Label3" runat="server" Text="If you want to change the color of the White Board message, please go to Administration->Form Design->Banners and set the color in page properties."></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="center">
                            <asp:Label ID="lbMSG" runat="server" ForeColor="Red"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="center">
                            <asp:Label ID="Label4" runat="server" Text="Select Save and Press F5 for changes to take effect."></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td align="left">
                            <img title="Close" onclick="var dialog = $find('<%=dialogInfo.ClientID%>');dialog.hide();" src="Application_Images/24x24/cancel_icon_24px.png" value="Close" /></td>
                        <td align="right">
                            <asp:ImageButton ID="btnUpdate" runat="server" AlternateText="Save" ImageUrl="Application_Images/24x24/Save_icon_24px.png" onclick="btnUpdate_Click" />
                        </td>
                    </tr>
                 </table>
            </Template>
        </ContentPane>
    </ig:WebDialogWindow>
      <ig:WebDialogWindow ID="dialogDelConfirm" runat="server" Height="118px" Width="361px" 
        Modal="True" Left="150px" Top="150px">
          <Header CaptionText="Please Confirm">
          </Header>
        <ContentPane>
            <Template>
                <table>
                    <tr>
                        <td colspan="2">
                            <asp:Label ID="lbDelMSG" runat="server"  Text="Do you wish to delete?"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td align="left">
                            <img title="Close" onclick="var dialog = $find('<%=dialogDelConfirm.ClientID%>');dialog.hide();" src="Application_Images/24x24/cancel_icon_24px.png" value="Close" />
                        </td>
                        <td align="right">
                            <asp:ImageButton ID="btnDel" runat="server" ImageUrl="Application_Images/24x24/ok_icon_24px.png" onclick="btnDel_Click" AlternateText="Delete" />
                        </td>
                    </tr>
                 </table>
            </Template>
        </ContentPane>
    </ig:WebDialogWindow>
      <ig:WebDialogWindow ID="dialogHidden" runat="server" Height="255px" Width="361px" Modal="True" Left="150px" Top="150px" WindowState="Hidden">
        <ContentPane>
            <Template>
                <asp:TextBox ID="textOP" runat="server" Text="Add"></asp:TextBox>
                <asp:TextBox ID="textNameH" runat="server" Text="0"></asp:TextBox>
                <asp:TextBox ID="sys_siteidH" runat="server"></asp:TextBox>
                <asp:TextBox ID="sys_eclient_idH" runat="server"></asp:TextBox>
                <asp:TextBox ID="sys_assignedtoanalgroupH" runat="server"></asp:TextBox>
            </Template>
        </ContentPane>
      </ig:WebDialogWindow>
        </ContentTemplate>
    </asp:UpdatePanel>
    <input type="hidden" name="OpType" id="OpType" value="0" />    
 </form>
</asp:Content>




