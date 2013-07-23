using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections;

using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;

using Infragistics.Web.UI;
using Infragistics.Web.UI.ListControls;
using Infragistics.Web.UI.GridControls;
using Infragistics.Web.UI.NavigationControls;

public partial class Admin_UGEUser : System.Web.UI.Page
{
    public DataSet ds;
    Hashtable htUserDisable;
    Hashtable htUserClass;

    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginAdmin(Session, Response);
        //UpdateList();
        if (!IsPostBack)
        {
            InitMyCtrl();
        }

        UpdateList(false);
        if (dialogSelReqClass.WindowState != Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden)
        {
            InitReqClassList(textNameH.Text);
        }

    }

    private void SetButton()
    {
        if (LicenseDA.AddNewEUser(Application))
        {
            hlAdd.ImageUrl = "Application_Images/24x24/add_icon_24px.png";
            hlAdd.NavigateUrl = "EUserInfoUser.aspx";
            hlAdd.ToolTip = "Add";

            lbLicMsg.Text = "";
        }
        else
        {
            hlAdd.ImageUrl = "Application_Images/Custom/Warning.gif";
            hlAdd.NavigateUrl = "";
            hlAdd.ToolTip = "";

            lbLicMsg.Text = "The " + Application["appenduserterm"] + " license limit has been reached";
        }
    }

    private int GetPageSize(CustomPager pger, out int maxRowNumber)
    {
        int minRowNumber = (pger.CurrentIndex - 1) * pger.PageSize;
        minRowNumber = minRowNumber > 0 ? minRowNumber : 0;
        pger.CurrentIndex = pger.CurrentIndex > 0 ? pger.CurrentIndex : 1;
        maxRowNumber = (pger.CurrentIndex * pger.PageSize) + 1;
        return minRowNumber;
    }

    public void OnPageChange(object sender, CommandEventArgs e)
    {
        CustomPager pgr = sender as CustomPager;
        if (pgr != null)
        {
            pger.CurrentIndex = Convert.ToInt32(e.CommandArgument);
            Session["EUserListPage"] = pger.CurrentIndex - 1;
            //Session["ProblemListPage"] = Convert.ToInt32(e.CommandArgument);
            UpdateList(false);
        }
    }

    public void OnPageSizeChage(int pageSize, string controlAttribute)
    {

        pger.PageSize = pageSize;
        //pgr.CurrentIndex = Convert.ToInt32(e.CommandArgument);
        //Session["ProblemListPage"] = Convert.ToInt32(e.CommandArgument);
        UpdateList(false);

    }

    private void InitMyCtrl()
    {
        dialogSetPass.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogSelReqClass.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogSelReqClass.Header.CaptionText = "Select " + Application["apprequestterm"] + " Class";

        lbTTL.Text = "Manage " + Application["appenduserterm"];
        lbDept.Text = Application["appeclientterm"].ToString();

        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogHidden.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

        //dg.Columns[2].Header.Text = Application["apprequestterm"].ToString() + " Class";
        //dg.Columns[4].Header.Text = Application["appsiteterm"].ToString();

        if (SysDA.GetSettingValue("appcompanylevel", Application) == "true")
        {
            lbCompany.Text = "Select " + Application["appcompanyterm"];
            DataSet dsCom = UserDA.GetCompanies();
            DropDownItem item;
            item = new DropDownItem("All", "0");
            ddCom.Items.Add(item);

            ///Company List
            foreach (DataRow dr in dsCom.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_company_id"].ToString(), dr["sys_company_id"].ToString());
                ddCom.Items.Add(item);
            }
            string strCom = LayUtil.GetQueryString(Request, "com");
            string strDept = LayUtil.GetQueryString(Request, "dept");
            if (strCom != "")
            {
                ddCom.SelectedValue = strCom;
            }
            else if (Session["EUserSelCom"] != null)
            {
                ddCom.SelectedValue = Session["EUserSelCom"].ToString();
            }
            else
            {
                ddCom.SelectedValue = "0";
            }
            UpdDeptDD(ddCom.SelectedValue);
            if (strDept != "")
            {
                ddDept.SelectedValue = strDept;
            }
            else if (Session["EUserSelDept"] != null)
            {
                ddDept.SelectedValue = Session["EUserSelDept"].ToString();
            }
        }
        else
        {
            lbCompany.Visible = false;
            ddCom.Visible = false;

            DropDownItem item;
            item = new DropDownItem("All", "0");
            ddDept.Items.Add(item);
            DataSet dsDept = UserDA.GetComDepts("");
            ///Company List
            foreach (DataRow dr in dsDept.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_eclient_id"].ToString(), dr["sys_eclient_id"].ToString());
                ddDept.Items.Add(item);
            }

            string strDept = LayUtil.GetQueryString(Request, "dept");
            if (strDept != "")
            {
                ddDept.SelectedValue = strDept;
            }
            else
            {
                ddDept.SelectedValue = "0";
            }
        }
        //UpdateList();
        Session["EUserInfoBack"] = "UGEUser.aspx?com=0&dept=0";
        ViewState["srch"] = LayUtil.GetQueryString(Request, "srch");
        SetButton();

        LayUtil.SetFont(this.form1, Application);
    }

    private string GetSelCom()
    {
        string strCom = "";
        if (Session["EUserSelCom"] != null)
        {
            strCom = Session["EUserSelCom"].ToString();
        }
        return strCom;
    }
    private string GetSelDept()
    {
        string strDept = "";
        if (Session["EUserSelDept"] != null)
        {
            strDept = Session["EUserSelDept"].ToString();
        }
        return strDept;
    }
    private string GetSortField()
    {
        string strField = "";
        if (Session["EUserSortField"] != null)
        {
            strField = Session["EUserSortField"].ToString();
        }
        return strField;
    }
    private Infragistics.Web.UI.SortDirection GetSortAsc()
    {
        Infragistics.Web.UI.SortDirection asc = Infragistics.Web.UI.SortDirection.Ascending;
        if (Session["EUserSortAsc"] != null)
        {
            asc = (Infragistics.Web.UI.SortDirection)Session["EUserSortAsc"];
        }
        return asc;
    }
    private int GetPageIndex()
    {
        int nPageIndex = 0;
        if (Session["EUserListPage"] != null)
        {
            string strIndex = Session["EUserListPage"].ToString();
            if (LayUtil.IsNumeric(strIndex))
            {
                nPageIndex = int.Parse(strIndex);
            }
        }

        return nPageIndex;
    }
    private int GetPageSize()
    {
        int nPageSize = 17;
        if (Session["EUserListPageSize"] != null)
        {
            string strPageSize = Session["EUserListPageSize"].ToString();
            if (LayUtil.IsNumeric(strPageSize))
            {
                nPageSize = int.Parse(strPageSize);
            }
        }
        return nPageSize;
    }

    protected void dgResult_ColSorted(object sender, Infragistics.Web.UI.GridControls.SortingEventArgs e)
    {
        Session["EUserSortField"] = e.SortedColumns[0].Column.Key;

        Session["EUserSortAsc"] = e.SortedColumns[0].SortDirection;
    }
    protected void dgResult_PageIndexChanged(object sender)
    {
        WebDataGrid dg = (WebDataGrid)sender;
        Session["EUserListPage"] = dg.Behaviors.Paging.PageIndex;
    }

    public void dgResult_PageSizeChanged(object sender)
    {
        WebDataGrid dg = (WebDataGrid)sender;
        Session["EUserListPageSize"] = dg.Behaviors.Paging.PageSize;
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        dg.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgReqClass.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
        LayUtil.SetFont(this.form1, Application);

    }

    private void UpdateList(bool bCom)
    {
        dg.ClearDataSource();
        int maxRowNumber_All;
        int minRowNumber_All = GetPageSize(pger, out maxRowNumber_All);

        //written by Sparsh ID 132
        int currentIndex = pger.CurrentIndex;
        if (pger.CurrentIndex > pger.NumbersofPages)
            pger.CurrentIndex = pger.NumbersofPages > 0 ? pger.NumbersofPages : currentIndex; //changed by sparsh ID 132

        int count = 0;
        if (ViewState["srch"].ToString() == "true")
        {
            ds = (DataSet)Session["SrchEUser"];
        }
        else if (SysDA.GetSettingValue("appcompanylevel", Application) == "true")
        {
            if (bCom)
            {
                ds = UserDA.GetEndUsersList(out count, ddCom.SelectedValue, "0", minRowNumber_All, maxRowNumber_All);
                pger.ItemCount = count;
            }
            else
            {
                if (ddCom.SelectedValue == "0")
                {
                    ds = UserDA.GetEndUsersList(out count, "0", "0", minRowNumber_All, maxRowNumber_All);
                    pger.ItemCount = count;
                }
                else
                {
                    ds = UserDA.GetEndUsersList(out count, ddCom.SelectedValue, ddDept.SelectedValue, minRowNumber_All, maxRowNumber_All);
                    pger.ItemCount = count;
                }

            }
        }
        else
        {
            ds = UserDA.GetEndUsersList(out count, ddDept.SelectedValue, minRowNumber_All, maxRowNumber_All);
            pger.ItemCount = count;
        }

        pger.MinRowNumber = minRowNumber_All + 1;
        pger.MaxRowNumber = maxRowNumber_All - 1 < count ? maxRowNumber_All - 1 : count;

        if (ds == null || ds.Tables.Count <= 0)
            return;

        htUserDisable = new Hashtable();
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            htUserDisable.Add(dr["sys_eusername"].ToString(), dr["sys_disabled"].ToString());
        }
        TemplateDataField templateColumn1 = (TemplateDataField)this.dg.Columns["Disable"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "Disable";
            field1.Header.Text = "Disable";
            this.dg.Columns.Add(field1);
        }


        templateColumn1 = (TemplateDataField)this.dg.Columns["Disable"];
        templateColumn1.ItemTemplate = new CheckEUserEnableTemplate(htUserDisable, this);
        //templateColumn1.Width = new Unit("10%");


        dg.DataSource = ds;
        dg.Behaviors.Paging.PageSize = GetPageSize();
        int nPage = GetPageIndex();
        if (dg.Behaviors.Paging.PageCount > nPage)
        {
            dg.Behaviors.Paging.PageIndex = nPage;
        }
        else
        {
            dg.Behaviors.Paging.PageIndex = dg.Behaviors.Paging.PageCount - 1;
        }
        dg.Behaviors.Paging.PagerTemplate = new MyPagerTemplateEUser(dg, Page, dg.Behaviors.Paging.PageIndex, dg.Behaviors.Paging.PageSize);
        dg.DataBind();

        //Panel pnlGrid = new Panel();
        //pnlGrid.Controls.Add(dg);

        //Panel pnlPager = new Panel();
        //pnlPager.Controls.Add(pger);

        //Panel pnlOuter = new Panel();

        //pnlOuter.Controls.Add(pnlGrid);
        //pnlOuter.Controls.Add(pnlPager);





        //LayUtil.SetFont(this.form1, Application);

    }



    private void UpdatePage()
    {
        dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        UpdateList(false);
    }
    protected void ddCom_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        Session["EUserSelCom"] = ddCom.SelectedValue;
        Session["EUserSelDept"] = ddDept.SelectedValue;
        Session["EUserInfoBack"] = "UGEUser.aspx?com=" + ddCom.SelectedValue + "&dept=0";

        UpdDeptDD(ddCom.SelectedValue);
        //UpdateList(true);
    }
    protected void ddDept_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        ViewState["Dept"] = ddDept.SelectedValue;
        Session["EUserSelDept"] = ddDept.SelectedValue;
        Session["EUserInfoBack"] = "UGEUser.aspx?com=" + ddCom.SelectedValue + "&dept=" + ddDept.SelectedValue;
        //UpdateList(false);
    }

    private void UpdDeptDD(string strCompany)
    {
        ddDept.Items.Clear();
        DropDownItem item;
        item = new DropDownItem("All", "0");
        ddDept.Items.Add(item);
        if (strCompany != "0")
        {
            DataSet dsDept = UserDA.GetComDepts(strCompany);
            ///Company List
            foreach (DataRow dr in dsDept.Tables[0].Rows)
            {
                item = new DropDownItem(UserDA.GetDeptNameFrmDeptId(dr["sys_eclient_id"].ToString()), dr["sys_eclient_id"].ToString());
                ddDept.Items.Add(item);
            }
        }

        ddDept.SelectedValue = "0";
    }
    protected void btnDel_Click(object sender, ImageClickEventArgs e)
    {
        string strName = textNameH.Text;

        btnDel.Style["visibility"] = "hidden";
        int nDependant = UserDA.GetEndUserDepReq(strName);
        if (nDependant > 0)
        {
            lbDelMSG.Text = "The " + Application["appenduserterm"] + " is against " + nDependant.ToString() + " " + Application["apprequestterm"];
            if (nDependant > 1)
            {
                lbDelMSG.Text += "s";
            }
            return;
        }
        nDependant = UserDA.GetEUserDepComm(strName);
        if (nDependant > 0)
        {
            lbDelMSG.Text = "The " + Application["appenduserterm"] + " is against " + nDependant.ToString() + " " + Application["appcommentterm"];
            if (nDependant > 1)
            {
                lbDelMSG.Text += "s";
            }
            return;
        }


        UserDA.DelEndUserReqClass(strName);
        UserDA.DelEndUser(strName);
        SetButton();
        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        btnDel.Style["visibility"] = "visible";
        UpdatePage();
    }

    protected void btnEditPassPre_Clicked(object sender, ImageClickEventArgs e)
    {
        System.Web.UI.WebControls.ImageButton btnEdit = (System.Web.UI.WebControls.ImageButton)sender;
        string strName = btnEdit.CommandArgument;

    }
    protected void btnEditReqClassPre_Clicked(object sender, ImageClickEventArgs e)
    {
        System.Web.UI.WebControls.ImageButton btnEdit = (System.Web.UI.WebControls.ImageButton)sender;
        string strName = btnEdit.CommandArgument;
        textNameH.Text = strName;

        InitReqClassList(strName);

        dialogSelReqClass.Header.CaptionText = "Select " + Application["apprequestterm"] + " Class for " + strName;
        dialogSelReqClass.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
    }
    protected void btnSavePass_Click(object sender, ImageClickEventArgs e)
    {
        lbPassMsg.Text = "";
        if (textPass.Text != textRePass.Text)
        {
            lbPassMsg.Text = "Passwords don't match.";
            return;
        }
        if (LayUtil.IsNumeric(Application["appminuserpwdlength"].ToString()))
        {
            int nLen = int.Parse(Application["appminuserpwdlength"].ToString());
            if (textPass.Text.Length < nLen)
            {
                lbPassMsg.Text = "You must specify a new password with at least " + Application["appminuserpwdlength"] + " characters";
                return;
            }
        }

        UserDA.UpdEndUserPass(textNameH.Text, textPass.Text);

        dialogSetPass.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
    }

    private void InitReqClassList(string strName)
    {
        htUserClass = new Hashtable();
        DataSet dsUserClass = UserDA.GetEndUserReqClass(strName);
        if (dsUserClass != null && dsUserClass.Tables.Count > 0)
        {
            foreach (DataRow dr in dsUserClass.Tables[0].Rows)
            {
                htUserClass.Add(dr["sys_requestclass_id"].ToString(), "true");
            }
        }

        //DataSet dsSite = UserDA.GetComSites(ddCompany.SelectedValue);
        DataSet dsClass = LibReqClassBR.GetReqClassList();

        CreateClassColumns(strName);

        dgReqClass.DataSource = dsClass;

        dgReqClass.DataBind();

    }

    private class CheckReqClassTemplate : ITemplate
    {
        #region ITemplate Members

        private Admin_UGEUser ParentPage;
        private string strUser;
        public CheckReqClassTemplate(Admin_UGEUser pPage, string strName)
        {
            ParentPage = pPage;
            strUser = strName;
        }
        public void InstantiateIn(Control container)
        {
            string strReqClass = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestclass_id"].ToString();
            if (strReqClass != "")
            {
                CheckBox b1 = new CheckBox();
                b1.Checked = IsEnable(strReqClass);
                b1.ID = strUser + strReqClass;

                b1.AutoPostBack = true;
                b1.Attributes.Add("ReqClass", strReqClass);
                b1.CheckedChanged += new EventHandler(CheckBtn_CheckedChanged);
                container.Controls.Add(b1);
            }
        }

        #endregion

        private bool IsEnable(string strReqClass)
        {
            if (ParentPage.htUserClass[strReqClass] != null && ParentPage.htUserClass[strReqClass].ToString() == "true")
                return true;
            return false;
        }
        protected void CheckBtn_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkEnable = (CheckBox)sender;
            string strReqClass = chkEnable.Attributes["ReqClass"].ToString();
            //LibEmailBodyDA.SetEmailSetting(strSetting, chkEnable.Checked);
            if (chkEnable.Checked)
            {
                UserDA.AddReqClass2EndUser(strUser, strReqClass);
            }
            else
            {
                UserDA.RmvReqClassFrmEndUser(strUser, strReqClass);
            }

        }

    }

    private void CreateClassColumns(string strName)
    {
        /*
        dgReqClass.Columns.Clear();
        BoundDataField boundColumn1 = (BoundDataField)this.dgReqClass.Columns["ReqClass"];
        if (boundColumn1 == null)
        {
            BoundDataField field1 = new BoundDataField();
            field1.Key = "ReqClass";
            field1.DataFieldName = "sys_requestclass_id";
            //field1.DataSourceField = new Infragistics.Web.UI.Framework.Data.DataField(
            field1.Header.Text = Application["apprequestterm"].ToString() + " Class";
            this.dgReqClass.Columns.Add(field1);
        }
        */

        TemplateDataField templateColumn1 = (TemplateDataField)this.dgReqClass.Columns["Members"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "Members";
            field1.Header.Text = "";
            this.dgReqClass.Columns.Add(field1);
        }


        templateColumn1 = (TemplateDataField)this.dgReqClass.Columns["Members"];
        templateColumn1.ItemTemplate = new CheckReqClassTemplate(this, strName);
        templateColumn1.Width = new Unit("40%");
    }

    private class CheckEUserEnableTemplate : ITemplate
    {
        #region ITemplate Members

        private Hashtable htUserDisable;
        private Admin_UGEUser ParentPage;
        public CheckEUserEnableTemplate(Hashtable htData, Admin_UGEUser page)
        {
            htUserDisable = htData;
            ParentPage = page;
        }

        public void InstantiateIn(Control container)
        {
            string strEUser = ((DataRowView)((TemplateContainer)container).DataItem)["sys_eusername"].ToString();
            if (strEUser != "")
            {
                CheckBox b1 = new CheckBox();
                b1.Checked = IsDisable(strEUser);
                b1.ID = strEUser;
                b1.AutoPostBack = true;
                b1.Attributes.Add("EUser", strEUser);
                b1.CheckedChanged += new EventHandler(CheckBtn_CheckedChanged);
                container.Controls.Add(b1);
            }
        }

        #endregion

        private bool IsDisable(string strName)
        {
            if (htUserDisable[strName] != null && htUserDisable[strName].ToString() == "1")
                return true;
            return false;
        }
        protected void CheckBtn_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkEnable = (CheckBox)sender;
            string strEUser = chkEnable.Attributes["EUser"].ToString();
            if (!chkEnable.Checked)
            {
                if (!LicenseDA.AddNewEUser(ParentPage.Application))
                {
                    ParentPage.lbLicMsg.Text = "Cannot Enable this " + ParentPage.Application["appenduserterm"] + " because the " + ParentPage.Application["appenduserterm"] + " license limit has been reached.";
                    chkEnable.Checked = true;
                    return;
                }
            }
            UserDA.DisableEUser(strEUser, chkEnable.Checked);
            ParentPage.SetButton();
            //ParentPage.UpdateList(false);

        }

    }
    public string GetEUserUrl(object obj)
    {
        return "EUserInfoUser.aspx?sys_eusername=" + Server.UrlEncode(obj.ToString());
    }
    public string GetEUserSettingUrl(object obj)
    {
        return "EUserSetting.aspx?sys_eusername=" + obj.ToString();
    }
    private class MyPagerTemplateEUser : ITemplate
    {
        WebDataGrid dg;
        Control rootCtrl;
        int nPageIndex;
        int nCurPageIndex;
        int nPageSize;
        public MyPagerTemplateEUser(WebDataGrid dgGrid, Page page, int pageindex, int pagesize)
        {
            dg = dgGrid;
            nPageIndex = pageindex;
            nPageSize = pagesize;
            //((Analyst_UserRequest)page).ViewState["PagerPageIndex"] = pageindex;
            //((Analyst_UserRequest)page).ViewState["PagerPageSize"] = pagesize;
        }

        public void InstantiateIn(Control container)
        {
            rootCtrl = container;
            LoadCtrl();
        }

        void ddlPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            string strPage = ddl.SelectedValue;
            if (LayUtil.IsNumeric(strPage))
            {
                nPageIndex = int.Parse(strPage) - 1;
                LoadCtrl();
                ((Admin_UGEUser)dg.Page).dgResult_PageIndexChanged(dg);
            }
        }

        void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            string strPageSize = ddl.SelectedValue;
            if (LayUtil.IsNumeric(strPageSize))
            {
                //((Analyst_UserRequest)dg.Page).ViewState["PagerPageSize"] = int.Parse(strPageSize);
                nPageSize = int.Parse(strPageSize);
                LoadCtrl();
                ((Admin_UGEUser)dg.Page).dgResult_PageSizeChanged(dg);
            }
        }

        public void LoadCtrl()
        {
            rootCtrl.Controls.Clear();
            //int nPageIndex = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageIndex"].ToString());
            //int nSize = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageSize"].ToString());
            int nSize = nPageSize;
            Admin_UGEUser page = ((Admin_UGEUser)dg.Page);

            dg.Behaviors.Paging.PageSize = nSize;
            if (dg.Behaviors.Paging.PageCount > nPageIndex)
            {
                dg.Behaviors.Paging.PageIndex = nPageIndex;
            }
            else
            {
                dg.Behaviors.Paging.PageIndex = dg.Behaviors.Paging.PageCount - 1;
            }
            nPageIndex = dg.Behaviors.Paging.PageIndex;
            int nPage = nPageIndex;
            //int nSize = dg.Behaviors.Paging.PageSize;

            int nStart = nSize * nPage + 1;
            int nEnd = nSize * (nPage + 1);

            int nTotal = ((DataSet)dg.DataSource).Tables[0].Rows.Count;
            if (nTotal == 0)
            {
                nStart = 0;
            }

            if (nEnd > nTotal)
            {
                nEnd = nTotal;
            }

            int nPageCnt = (int)Math.Ceiling((Double)nTotal / (Double)nSize);
            if (nPageCnt == 0)
            {
                return;
            }
            nCurPageIndex = nPageIndex;

            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Style["width"] = "100%";
            div.Style["text-align"] = "center";

            if (nPageIndex != 0)
            {
                LinkButton lbtnFirst = new LinkButton();
                lbtnFirst.Text = page.Server.HtmlEncode("<<");
                lbtnFirst.Click += new EventHandler(lbtnFirst_Click);
                lbtnFirst.ToolTip = "First Page";
                div.Controls.Add(lbtnFirst);

                Literal lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                LinkButton lbtnPrev = new LinkButton();
                lbtnPrev.Text = page.Server.HtmlEncode("<");
                lbtnPrev.Click += new EventHandler(lbtnPrev_Click);
                lbtnPrev.ToolTip = "Previous Page";
                div.Controls.Add(lbtnPrev);

                lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);
            }
            else
            {
                Label lbtnFirst = new Label();
                lbtnFirst.Text = page.Server.HtmlEncode("<<");
                div.Controls.Add(lbtnFirst);

                Literal lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                Label lbtnPrev = new Label();
                lbtnPrev.Text = page.Server.HtmlEncode("<");
                div.Controls.Add(lbtnPrev);

                lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);
            }
            Label lbText1 = new Label();
            lbText1.Text = "Page ";
            div.Controls.Add(lbText1);

            DropDownList ddlPage = new DropDownList();
            for (int i = 1; i <= nPageCnt; i++)
            {
                ddlPage.Items.Add(new System.Web.UI.WebControls.ListItem(i.ToString(), i.ToString()));
            }
            ddlPage.SelectedIndex = nPageIndex;
            ddlPage.AutoPostBack = true;
            ddlPage.ID = "idPageIndex";
            ddlPage.SelectedIndexChanged += new EventHandler(ddlPage_SelectedIndexChanged);
            div.Controls.Add(ddlPage);



            Label lbText2 = new Label();
            lbText2.Text = " of " + nPageCnt + " (";
            div.Controls.Add(lbText2);

            Label lb = new Label();
            lb.Text = nStart.ToString() + " to " + nEnd + " of " + nTotal + " " + page.Application["appenduserterm"] + "s)";

            lb.ID = "lbPager";
            div.Controls.Add(lb);

            if (nPageIndex != nPageCnt - 1)
            {
                Literal lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                LinkButton lbtnNext = new LinkButton();
                lbtnNext.Text = page.Server.HtmlEncode(">");
                lbtnNext.Click += new EventHandler(lbtnNext_Click);
                lbtnNext.ToolTip = "Next Page";
                div.Controls.Add(lbtnNext);

                lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                LinkButton lbtnLast = new LinkButton();
                lbtnLast.Text = page.Server.HtmlEncode(">>");
                lbtnLast.Click += new EventHandler(lbtnLast_Click);
                lbtnLast.ToolTip = "Last Page";
                div.Controls.Add(lbtnLast);

            }
            else
            {
                Literal lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                Label lbtnNext = new Label();
                lbtnNext.Text = page.Server.HtmlEncode(">");
                div.Controls.Add(lbtnNext);

                lbSpace = new Literal();
                lbSpace.Text = "&nbsp;";
                div.Controls.Add(lbSpace);

                Label lbtnLast = new Label();
                lbtnLast.Text = page.Server.HtmlEncode(">>");
                div.Controls.Add(lbtnLast);

            }

            Literal lbSizeSpace = new Literal();
            lbSizeSpace.Text = "&nbsp;&nbsp;&nbsp;";
            div.Controls.Add(lbSizeSpace);

            Label lbPageSize = new Label();
            lbPageSize.Text = "Show ";
            div.Controls.Add(lbPageSize);

            DropDownList ddlPageSize = new DropDownList();
            int nDefSize = 17;
            if (nDefSize % LayUtil.nPageSizeStep != 0 || nDefSize > LayUtil.nPageMaxSize)
            {
                ddlPageSize.Items.Add(new System.Web.UI.WebControls.ListItem(nDefSize.ToString(), nDefSize.ToString()));
            }
            for (int i = LayUtil.nPageSizeStep; i <= LayUtil.nPageMaxSize; )
            {
                ddlPageSize.Items.Add(new System.Web.UI.WebControls.ListItem(i.ToString(), i.ToString()));
                i += LayUtil.nPageSizeStep;
            }
            ddlPageSize.SelectedValue = nSize.ToString();
            ddlPageSize.AutoPostBack = true;
            ddlPageSize.ID = "idPageSize";
            ddlPageSize.SelectedIndexChanged += new EventHandler(ddlPageSize_SelectedIndexChanged);
            div.Controls.Add(ddlPageSize);

            lbPageSize = new Label();
            lbPageSize.Text = " Rows Per Page";
            div.Controls.Add(lbPageSize);

            rootCtrl.Controls.Add(div);
        }

        void lbtnFirst_Click(object sender, EventArgs e)
        {
            nPageIndex = 0;
            LoadCtrl();
            ((Admin_UGEUser)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnPrev_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex - 1;
            LoadCtrl();
            ((Admin_UGEUser)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnLast_Click(object sender, EventArgs e)
        {
            nPageIndex = dg.Behaviors.Paging.PageCount - 1;
            LoadCtrl();
            ((Admin_UGEUser)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnNext_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex + 1;
            LoadCtrl();
            ((Admin_UGEUser)dg.Page).dgResult_PageIndexChanged(dg);
        }

    }
}
