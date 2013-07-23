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
using System.Data.SqlClient;

using Infragistics.Web.UI;
using Infragistics.Web.UI.GridControls;


public partial class SelUser : System.Web.UI.Page
{
    private DataSet dsAtSite;
    private DataSet dsAtGrp;

    public Hashtable htUserSite;
    public Hashtable htUserGrp;

    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLogin(Session, Response);
        if (!IsPostBack)
        {
            InitCtrl();
        }

        dg.Behaviors.Sorting.ColumnSorted += new ColumnSortedHandler(dgResult_ColSorted);
        UpdateList();
        if (!IsPostBack)
        {
            SetSuggestedUser();
        }
    }

    private void InitCtrl()
    {
        lbTTL.Text = "Select " + Application["appuserterm"];

        ViewState["assignedtoanalgroup"] = LayUtil.GetQueryString(Request, "assignedtoanalgroup");
        ViewState["hidedisabled"] = LayUtil.GetQueryString(Request, "hidedisabled");
        ViewState["element"] = LayUtil.GetQueryString(Request, "element");
        ViewState["siteid"] = LayUtil.GetQueryString(Request, "siteid");
        ViewState["suggestuser"] = LayUtil.GetQueryString(Request, "suggestuser");
        ViewState["sys_eusername"] = LayUtil.GetQueryString(Request, "sys_eusername");
        ViewState["requesttype"] = LayUtil.GetQueryString(Request, "requesttype");
        ViewState["selectall"] = LayUtil.GetQueryString(Request, "selectall");
        ViewState["notclickablewithoutemail"] = LayUtil.GetQueryString(Request, "ncwe");

        hlClear.NavigateUrl = "javascript:window.opener.effect_user('','','" + ViewState["element"].ToString() + "','','');self.close();";
        hlClear.ImageUrl = "Application_Images/24x24/clear_icon_24px.png";
        hlClear.Text = "Clear";


        
        lbFilterAnalGrp.Text = "";

        if (Session["Role"].ToString() != "enduser")
        {
            DataSet ds = UserDA.GetUserInfo(Session["User"].ToString());

            ViewState["sys_selectuser_filteranalgroup"] = ds.Tables[0].Rows[0]["sys_selectuser_filteranalgroup"].ToString();
            ViewState["sys_reqaccesssite"] = ds.Tables[0].Rows[0]["sys_reqaccesssite"].ToString();


            if (ViewState["sys_selectuser_filteranalgroup"].ToString() == "1" && ViewState["assignedtoanalgroup"].ToString() != "")
            {
                lbFilterAnalGrp.Text = "(Filtered On Group)";
            }
            else
            {
                lbFilterAnalGrp.Text = "";
            }
        }

    }
    protected void dgResult_ColSorted(object sender, Infragistics.Web.UI.GridControls.SortingEventArgs e)
    {
        Session["SelUserSortField"] = e.SortedColumns[0].Column.Key;
        Session["SelUserSortDir"] = e.SortedColumns[0].SortDirection;
    }

    private bool UserAtSite(string strUser, string strSite)
    {
        if (htUserSite[strUser + "/" + strSite] != null && htUserSite[strUser + "/" + strSite].ToString() == "true")
            return true;

        return false;
    }

    private bool UserInGrp(string strUser, string strGrp)
    {
        if (htUserGrp[strUser + "/" + strGrp] != null && htUserGrp[strUser + "/" + strGrp].ToString() == "true")
            return true;

        return false;
    }

    private void UpdateList()
    {
        dg.ClearDataSource();

        DataSet ds;
        //Build htUserSite table
        htUserSite = new Hashtable();
        ds = UserDA.GetUserSites();
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            htUserSite.Add(dr["sys_username"].ToString() + "/" + dr["sys_siteid"].ToString(), "true");
        }

        //Build htUserGrp table
        htUserGrp = new Hashtable();
        ds = UserDA.GetUserGrps();
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            htUserGrp.Add(dr["sys_username"].ToString() + "/" + dr["sys_analgroup"].ToString(), "true");
        }


        
        ds = GetUserList();

        CreateColumns();

        if(Session["SelUserSortField"] != null && Session["SelUserSortDir"] != null)
        {
            string strField = Session["SelUserSortField"].ToString();
            Infragistics.Web.UI.SortDirection dir = (Infragistics.Web.UI.SortDirection)Session["SelUserSortDir"];

            TemplateDataField templateColumn1 = (TemplateDataField)this.dg.Columns[strField];

            if (strField != "" && templateColumn1 != null)
            {
                dg.Behaviors.Sorting.SortedColumns.Clear();
                dg.Behaviors.Sorting.SortedColumns.Add(strField, dir);
            }
        }

        dg.DataSource = ds;

        dg.DataBind();

        LayUtil.SetFont(this.form1, Application);
    }

    private void CreateColumns()
    {
        TemplateDataField templateColumn1 = (TemplateDataField)this.dg.Columns["sys_username"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "sys_username";
            field1.Header.Text = "Username";
            this.dg.Columns.Add(field1);
        }


        templateColumn1 = (TemplateDataField)this.dg.Columns["sys_username"];
        templateColumn1.ItemTemplate = new UsernameTemplate(this);

        templateColumn1 = (TemplateDataField)this.dg.Columns["Absence"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "Absence";
            field1.Header.Text = "Available";
            this.dg.Columns.Add(field1);
        }


        templateColumn1 = (TemplateDataField)this.dg.Columns["Absence"];
        templateColumn1.ItemTemplate = new AvalTemplate();



        if (ViewState["siteid"].ToString() != "")
        {
            templateColumn1 = (TemplateDataField)this.dg.Columns["AtSiteId"];
            if (templateColumn1 == null)
            {
                TemplateDataField field1 = new TemplateDataField();
                field1.Key = "AtSiteId";
                field1.Header.Text = "At "+Application["appsiteterm"];
                this.dg.Columns.Add(field1);
            }


            templateColumn1 = (TemplateDataField)this.dg.Columns["AtSiteId"];
            templateColumn1.ItemTemplate = new AtSiteTemplate(this);
            templateColumn1.Width = new Unit("15%");
        }
    }

    private class UsernameTemplate : ITemplate
    {
        #region ITemplate Members

        SelUser pPage;
        public UsernameTemplate(SelUser page)
        {
            pPage = page;
        }
        public void InstantiateIn(Control container)
        {
            string strName = ((DataRowView)((TemplateContainer)container).DataItem)["sys_username"].ToString();
            string strGrp = ((DataRowView)((TemplateContainer)container).DataItem)["anal_group"].ToString();
            string strAssign = ((DataRowView)((TemplateContainer)container).DataItem)["sys_assign"].ToString();
            string strCrossSiteAssign = ((DataRowView)((TemplateContainer)container).DataItem)["sys_crosssiteassignto"].ToString();
            //string strAssign = ((DataRowView)((TemplateContainer)container).DataItem)["sys_assign"].ToString();
            string strEmail = ((DataRowView)((TemplateContainer)container).DataItem)["sys_email"].ToString();
            string strUserFullName = ((DataRowView)((TemplateContainer)container).DataItem)["UserFullName"].ToString();

            bool bAtSite = pPage.UserAtSite(strName,pPage.ViewState["siteid"].ToString());

            //if (strGrp != "")
            {
                if (pPage.UserInGrp(strName, pPage.ViewState["assignedtoanalgroup"].ToString()))
                {
                    strGrp = pPage.ViewState["assignedtoanalgroup"].ToString();
                }
            }

            if ((strAssign == "1" || strCrossSiteAssign == "1" || pPage.ViewState["selectall"].ToString() == "true") || (pPage.ViewState["siteid"].ToString() == "" || pPage.Application["appautoassigncontraintosite"].ToString() == "false" || bAtSite))
            {
                HyperLink h1 = new HyperLink();
                if (pPage.ViewState["notclickablewithoutemail"].ToString() != "true" || strEmail != "")
                {
                    h1.NavigateUrl = "javascript:window.opener.effect_user('" + strName + "','" + strGrp + "','" + pPage.ViewState["element"].ToString() + "','" + strEmail + "','" + strUserFullName + "');self.close();";
                }
                h1.Text = strName;
                container.Controls.Add(h1);
            }
            else
            {
                Label lb1 = new Label();
                lb1.Text = strName;
                container.Controls.Add(lb1);
            }
        }

        #endregion
    }

    private class AvalTemplate : ITemplate
    {
        #region ITemplate Members

        public AvalTemplate()
        {
        }
        public void InstantiateIn(Control container)
        {
            string str = ((DataRowView)((TemplateContainer)container).DataItem)["absence"].ToString();

            if (str == "0")
            {
                HtmlImage img = new HtmlImage();
                img.Border = 0;
                img.Src = "Application_Images/Custom/Check_LT.gif";
                img.Height = 16;
                img.Width = 16;
                container.Controls.Add(img);
            }
        }

        #endregion
    }
    private class AtSiteTemplate : ITemplate
    {
        #region ITemplate Members

        SelUser pPage;
        public AtSiteTemplate(SelUser page)
        {
            pPage = page;
        }
        public void InstantiateIn(Control container)
        {
            string strName = ((DataRowView)((TemplateContainer)container).DataItem)["sys_username"].ToString();
            string strSite = ((DataRowView)((TemplateContainer)container).DataItem)["AtSiteId"].ToString();

            bool bAtSite = pPage.UserAtSite(strName, pPage.ViewState["siteid"].ToString());

            if (bAtSite)
            {
                HtmlImage img = new HtmlImage();
                img.Border = 0;
                img.Src = "Application_Images/Custom/Check_LT.gif";
                img.Height = 16;
                img.Width = 16;
                container.Controls.Add(img);
            }
        }

        #endregion
    }

    public DataSet GetUserList()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandType = CommandType.Text;
        DataSet ds;
        if (Session["Role"].ToString() != "enduser")
        {
            if (ViewState["sys_reqaccesssite"].ToString() == "1")
            {
                if (ViewState["sys_selectuser_filteranalgroup"].ToString() == "1" && ViewState["assignedtoanalgroup"].ToString() != "")
                {
                    //cmd.CommandText = "SELECT *, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND useranalgroup.sys_userdefault=1 ORDER BY sys_userdefault DESC) AS [anal_group] FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                    //cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username ORDER BY sys_userdefault DESC) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username=[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                    cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND sys_userdefault=1) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username=[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                }
                else
                {
                    //cmd.CommandText = "SELECT *, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND sys_userdefault=1 ORDER BY useranalgroup.sys_userdefault DESC) AS [anal_group] FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                    //cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username ORDER BY useranalgroup.sys_userdefault DESC) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username =[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                    cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND sys_userdefault=1) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username =[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] WHERE [user].sys_username IN (SELECT sys_username FROM analsite WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username =@strUser)) OR sys_crosssiteassignto=1 ORDER BY [user].sys_username";
                }
            }
            else
            {
                if (ViewState["sys_selectuser_filteranalgroup"].ToString() == "1" && ViewState["assignedtoanalgroup"].ToString() != "")
                {
                    //cmd.CommandText = "SELECT *, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND useranalgroup.sys_userdefault=1 ORDER BY sys_userdefault DESC) AS [anal_group] FROM [user] ORDER BY [user].sys_username";
                    //cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username ORDER BY sys_userdefault DESC) AS [anal_group] , (SELECT sys_siteid FROM analsite WHERE sys_username =[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] ORDER BY [user].sys_username";
                    cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], (SELECT count(sys_analgroup) FROM [useranalgroup] WHERE [useranalgroup].sys_username = [user].sys_username AND [useranalgroup].sys_analgroup =@assignedtoanalgroup) AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND sys_userdefault=1) AS [anal_group] , (SELECT sys_siteid FROM analsite WHERE sys_username =[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] ORDER BY [user].sys_username";
                }
                else
                {
                    //cmd.CommandText = "SELECT *, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND useranalgroup.sys_userdefault=1 ORDER BY sys_userdefault DESC) AS [anal_group] FROM [user] ORDER BY [user].sys_username";
                    //cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username ORDER BY sys_userdefault DESC) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username=[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] ORDER BY [user].sys_username";
                    cmd.CommandText = "SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND sys_userdefault=1) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username=[user].sys_username AND sys_siteid=@sys_siteid) AS AtSiteId FROM [user] ORDER BY [user].sys_username";
                }
            }

            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = "@strUser";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = Session["User"].ToString();
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@assignedtoanalgroup";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ViewState["assignedtoanalgroup"].ToString();
            cmd.Parameters.Add(parameter);

            cmd.Parameters.AddWithValue("@sys_siteid", ViewState["siteid"].ToString());

            ds = LayUtilDA.GetDSCMD(cmd);
        }
        else
        {
            ds = LayUtilDA.GetDSSQL("SELECT *, (sys_forename+ ' ' + sys_surname) AS UserFullName, (SELECT count(sys_userunavail_id) FROM userunavail WHERE userunavail.sys_username = [user].sys_username AND (getdate() BETWEEN sys_userunavail_startdatetime AND sys_userunavail_enddatetime)) AS [Absence], 1 AS [InGroup], (SELECT top 1 sys_analgroup FROM useranalgroup WHERE sys_username = [user].sys_username AND useranalgroup.sys_userdefault=1 ORDER BY sys_userdefault DESC) AS [anal_group], (SELECT sys_siteid FROM analsite WHERE sys_username=[user].sys_username AND sys_siteid='" + ViewState["siteid"].ToString() + "') AS AtSiteId FROM [user] ORDER BY [user].sys_username");

        }
        
        string strHideDis = ViewState["hidedisabled"].ToString();
        for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
        {
            DataRow dr = ds.Tables[0].Rows[i];
            ////Add by sparsh (dr["sys_requestclassrestrict"].ToString() == "1") in if condition
            if ((strHideDis == "true" && dr["sys_disabled"].ToString() == "1") || dr["InGroup"].ToString() != "1" || dr["sys_requestclassrestrict"].ToString() == "1")
            {
                ds.Tables[0].Rows.Remove(dr);
            }
        }

        return ds;
    }
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        dg.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
        LayUtil.SetFont(this.form1, Application);

    }

    private void SetSuggestedUser()
    {

        string strSuggest = ViewState["suggestuser"].ToString();
        if (strSuggest != "yes")
        {
            lbSuggestMsg.Text = "No Suggestion";
            lbSuggestMsg.Visible = false;
            hlSuggested.Visible = false;
            return;
        }

        string strReqTypeId = ViewState["requesttype"].ToString();
        string strSiteId = ViewState["siteid"].ToString();
        string strEUser = ViewState["sys_eusername"].ToString();

        string strSuggestedUser = "";
        string strSuggestedGrp = "";

        DataSet dsReqType = RequestDA.GetReqTypeById(strReqTypeId);
        if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
        {
            string strAssgnType = dsReqType.Tables[0].Rows[0]["sys_requesttype_assigntype"].ToString();
            if (strAssgnType == "1")
            {
                //Use site person
                DataSet dsSite = UserDA.GetSitesById(strSiteId);
                if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
                {
                    strSuggestedUser = dsSite.Tables[0].Rows[0]["sys_site_username"].ToString();
                }
            }
            else if (strAssgnType == "2")
            {
                //Use dept person
                DataSet dsDept = UserDA.GetDeptByEUser(strEUser);
                if (dsDept != null && dsDept.Tables.Count > 0 && dsDept.Tables[0].Rows.Count > 0)
                {
                    strSuggestedUser = dsDept.Tables[0].Rows[0]["sys_eclient_username"].ToString();
                }
            }
            else if (strAssgnType == "3")
            {
                strSuggestedUser = dsReqType.Tables[0].Rows[0]["sys_requesttype_assignto"].ToString();
            }
        }

        if (strSuggestedUser == "")
        {
            string strAutoAssgn = SysDA.GetSettingValue("appautoassignrequest", Application).ToLower();
            string strConstrnSite = SysDA.GetSettingValue("appautoassigncontraintosite", Application);

            Hashtable htReqType = new Hashtable();
            DataSet dsReqTypeAll = RequestDA.GetReqTypeList();
            if (dsReqTypeAll != null && dsReqTypeAll.Tables.Count > 0)
            {
                foreach (DataRow dr in dsReqTypeAll.Tables[0].Rows)
                {
                    htReqType.Add(dr["sys_requesttype_id"].ToString(), dr["sys_requesttypeparent_id"].ToString());
                }

            }

            if (strAutoAssgn == "preference load balancing" || strAutoAssgn == "ap preference load balancing")
            {
                string strType = strReqTypeId;
                while (strType != "" && strSuggestedUser == "")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedUser = UserDA.GetReqTypeSuggestedUserSiteRes(strType, strSiteId);
                    }
                    else
                    {
                        strSuggestedUser = UserDA.GetReqTypeSuggestedUser(strType);
                    }

                    if (strSuggestedUser != "")
                    {
                        break;
                    }

                    if (htReqType[strType] != null)
                    {
                        strType = htReqType[strType].ToString();
                    }
                    else
                    {
                        break;
                    }
                }

                if (strSuggestedUser == "" && SysDA.GetSettingValue("appautoassigncontrainskills", Application) == "false")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedUser = UserDA.GetLoadSuggestedUserSiteRes(strSiteId);
                    }
                    else
                    {
                        strSuggestedUser = UserDA.GetLoadSuggestedUser();
                    }
                }
            }
            else if(strAutoAssgn == "load balancing" || strAutoAssgn == "ap load balancing")
            {
                if (strSuggestedUser == "")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedUser = UserDA.GetLoadSuggestedUserSiteRes(strSiteId);
                    }
                    else
                    {
                        strSuggestedUser = UserDA.GetLoadSuggestedUser();
                    }
                }
            }

            /*
            if (strAutoAssgn == "ap preference load balancing group")
            {
                string strType = strReqTypeId;
                while (strType != "" && strSuggestedGrp == "")
                {
                    strSuggestedGrp = UserDA.GetReqTypeSuggestedGrp(strType);

                    if (strSuggestedGrp != "")
                    {
                        break;
                    }

                    if (htReqType[strType] != null)
                    {
                        strType = htReqType[strType].ToString();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (strSuggestedGrp == "" && (SysDA.GetSettingValue("appautoassigncontrainskills", Application) == "false" || strAutoAssgn == "ap load balancing group"))
            {
                strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
            }
             */ 

        }


        if (strSuggestedUser != "")
        {
            string strGrp = ViewState["assignedtoanalgroup"].ToString();
            if (strGrp != "" && UserInGrp(strSuggestedUser, strGrp))
            {
                strSuggestedGrp = strGrp;
            }
            else
            {
                DataSet dsGrp = UserDA.GetUserGrpsDef(strSuggestedUser);
                if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                {
                    strSuggestedGrp = dsGrp.Tables[0].Rows[0]["sys_analgroup"].ToString();
                }
                else
                {
                    strSuggestedGrp = "";
                }
            }


            ////Set hyperlink
            lbSuggestMsg.Text = "Suggested "+Application["appuserterm"] +": ";
            hlSuggested.Visible = true;
            hlSuggested.NavigateUrl = "javascript:window.opener.effect_user('" + strSuggestedUser + "','" + strSuggestedGrp + "','" + ViewState["element"].ToString() + "','','');self.close();";
            hlSuggested.Text = strSuggestedUser;

            

        }
        else
        {
            lbSuggestMsg.Text = "No Suggestion";
            hlSuggested.Visible = false;
        }
    }
}
