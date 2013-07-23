using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using Infragistics.Web.UI.NavigationControls;
using Infragistics.WebUI.UltraWebToolbar;

public partial class EUserMenu : System.Web.UI.Page
{
    DataRow drEUser = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginEUser(Session, Response);
        DataSet dsEUser = UserDA.GetEUserInfo(Session["User"].ToString());
        if (dsEUser == null || dsEUser.Tables.Count <= 0 || dsEUser.Tables[0].Rows.Count <= 0)
        {
            Response.End();
            return;
        }
        drEUser = dsEUser.Tables[0].Rows[0];

        if (!IsPostBack)
        {
            int nWidth2Rmv = 0;

            UltraWebToolbar1.Items.FromKeyButtonGroup("RootMenu").Buttons[1].Text = "Search " + Application["apprequestterm"];

            WebDataMenu1.Items[0].ImageUrl = "~/Application_Images/Taskbar/icons_Help_24px_small.png";
            if (SysDA.GetSettingValue("securityhidelogoff", Application) == "false")
            {
                WebDataMenu1.Items[1].ImageUrl = "~/Application_Images/Taskbar/Icon_exit_24px_small.png";
                WebDataMenu1.Items[1].NavigateUrl = "Login.aspx?brand=" + LayUtil.GetQueryString(Request, "brand");
                WebDataMenu1.Items[1].Target = "_parent";
            }
            else
            {
                WebDataMenu1.Items.RemoveAt(1);
                nWidth2Rmv += 24;
            }


            if (SysDA.GetSettingValue("appenduserallowhelp", Application) == "false")
            {
                WebDataMenu1.Items.RemoveAt(0);
                nWidth2Rmv += 24;
            }

            WebDataMenu1.Width = new Unit((53 - nWidth2Rmv).ToString() + "px");

            TBButtonGroup bgrp = (TBButtonGroup)UltraWebToolbar1.Items[0];

            bgrp.Buttons[2].Text = Application["appselfserviceterm"].ToString();

            if (SysDA.GetSettingValue("appenduserallowmypwd", Application) != "true" && SysDA.GetSettingValue("appallowenduserselfmaint", Application) != "true")
            {
                bgrp.Buttons.RemoveAt(4);
            }
            if (SysDA.GetSettingValue("appallowenduserselfservice", Application) == "false")
            {
                bgrp.Buttons.RemoveAt(2);
                //bgrp.Buttons[2].Visible = false;
            }
            if (SysDA.GetSettingValue("appenduserallowsearchreq", Application) == "false")
            {
                bgrp.Buttons.RemoveAt(1);
            }
            // written by Sparsh ID 172
            if (SysDA.GetSettingValue("appenduserallowsurvey", Application) == "false")
            {
                bgrp.Buttons.RemoveAt(3);
            }
            // End by Sparsh ID 172

        }
    }
    protected void UltraWebToolbar1_GroupButtonClicked(object sender, Infragistics.WebUI.UltraWebToolbar.GroupButtonEvent e)
    {
        Infragistics.Web.UI.NavigationControls.DataMenuItem item;
        Infragistics.Web.UI.NavigationControls.DataMenuItem childItem;
        Infragistics.Web.UI.NavigationControls.DataMenuItem newItem;

        DataSet dsReqClass;
        if (SysDA.GetSettingValue("enduserrestrictrequestclass",Application) == "true")
        {
            dsReqClass = UserDA.GetEUserReqClassInfo(Session["User"].ToString());
        }
        else
        {
            dsReqClass = LibReqClassBR.GetReqClassListOrderByDescription();
        }

        switch (e.Group.SelectedButton.Key)
        {
            case "Home":
                this.WebDataMenu2.Visible = true;
                this.WebDataMenu2.Items.Clear();

                bool bItem = false;

                if (SysDA.GetSettingValue("appenduserallowlognew", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("Log New " + Application["apprequestterm"]);
                    newItem.ImageUrl = "~/Application_Images/16x16/add_icon_16px.png";

                    if (dsReqClass != null && dsReqClass.Tables.Count > 0 && dsReqClass.Tables[0].Rows.Count > 0)
                    {
                        if (dsReqClass.Tables[0].Rows.Count == 1)
                        {
                            newItem.NavigateUrl = "EReqInfo.aspx?reqclass=" + dsReqClass.Tables[0].Rows[0]["sys_requestclass_id"].ToString();
                            newItem.Target = "inner";
                        }
                        else
                        {
                            foreach (DataRow dr in dsReqClass.Tables[0].Rows)
                            {
                                childItem = newItem.Items.Add(dr["sys_requestclass_desc"].ToString());
                                childItem.NavigateUrl = "EReqInfo.aspx?reqclass=" + dr["sys_requestclass_id"].ToString();
                                childItem.ImageUrl = "~/Application_Images/16x16/request_16x6px.png";
                                childItem.Target = "inner";
                            }
                        }
                    }
                    bItem = true;
                }

                if (SysDA.GetSettingValue("appenduserallowmyopenreq", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("My Open " + Application["apprequestterm"] + "s");
                    newItem.NavigateUrl = "EUserRequest.aspx?status=open";
                    newItem.ImageUrl = "~/Application_Images/16x16/Request_Open.png";
                    newItem.Target = "inner";

                    bItem = true;
                }

                if (SysDA.GetSettingValue("appenduserallowmyclosedreq", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("My Closed " + Application["apprequestterm"] + "s");
                    newItem.ImageUrl = "~/Application_Images/16x16/Request_Close_16px.png";
                    
                    newItem.NavigateUrl = "EUserRequest.aspx?status=closed";
                    newItem.Target = "inner";

                    bItem = true;
                }

                if (SysDA.GetSettingValue("appenduserallowallmyreq", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("All My " + Application["apprequestterm"] + "s");
                    newItem.ImageUrl = "~/Application_Images/16x16/request_16x6px.png";
                    newItem.NavigateUrl = "EUserRequest.aspx?status=all";
                    newItem.Target = "inner";

                    bItem = true;
                }

                if (SysDA.GetSettingValue("appenduserallowviewchange", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("View Changes");
                    newItem.ImageUrl = "~/Application_Images/16x16/change16x6px.png";
                    newItem.NavigateUrl = "EUserChange.aspx";
                    newItem.Target = "inner";

                    bItem = true;
                }

                if (bItem)
                {
                    newItem = new DataMenuItem();
                    newItem.IsSeparator = true;
                    WebDataMenu2.Items.Add(newItem);
                }
                break;
            case "Detail":
                this.WebDataMenu2.Visible = true;
                this.WebDataMenu2.Items.Clear();

                bItem = false;

                if (SysDA.GetSettingValue("appenduserallowmypwd", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("My Password");
                    newItem.NavigateUrl = "EUserPass.aspx";
                    newItem.Target = "inner";

                    bItem = true;
                }
                if (SysDA.GetSettingValue("appallowenduserselfmaint", Application) == "true")
                {
                    newItem = WebDataMenu2.Items.Add("My Details");
                    newItem.NavigateUrl = "EUserInfo.aspx";
                    newItem.Target = "inner";

                    bItem = true;
                }
                
                if (bItem)
                {
                    newItem = new DataMenuItem();
                    newItem.IsSeparator = true;
                    WebDataMenu2.Items.Add(newItem);
                }

                break;
            default:
                this.WebDataMenu2.Items.Clear();
                this.WebDataMenu2.Visible = false;
                break;
        }
    }
}
