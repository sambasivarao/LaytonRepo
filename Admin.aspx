<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Admin.aspx.cs" Inherits="Admin_Admin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
 <%--Add by Sprash for Browser compatability end--%>
    <meta content="IE=9" http-equiv="X-UA-Compatible"/>
     <%--Add by Sprash for Browser compatability end--%>
    <title><%=Application["apptitle"] %></title>
</head>
<frameset rows="74,*" frameborder="0" framespacing="0">
  <frame name="banner" noResize scrolling="no" src="Banner.aspx">
  <frame name="menu" scrolling="no" src="AdminMenu.aspx">
</frameset>
</html>
