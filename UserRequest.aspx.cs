using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.Web.UI.HtmlControls;
using ASPnetControls;
using Infragistics.WebUI;
using Infragistics.Web.UI;
using Infragistics.Web.UI.GridControls;
using Infragistics.Web.UI.ListControls;
using Infragistics.WebUI.WebDataInput;
using Infragistics.WebUI.WebSchedule;
using Infragistics.Web.UI.LayoutControls;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Infragistics.Web.UI.EditorControls;
using Telerik.Web.UI;
using System.Web.Services;

public partial class Analyst_UserRequest : System.Web.UI.Page
{
    int i = 0;
    public XmlDocument xmlForm;
    public XmlNode dgNode = null;

    //WebDataGrid dgResult;

    private Hashtable htReqClass = null;
    private Hashtable htReqType = null;


    private bool bSYS_REQUEST_IDinColumn = false;


    private Hashtable htOpenStatus = null;
    private Hashtable htReqColor = null;
    private Hashtable htPriorityColor = null;
    private Hashtable htStatusColor = null;

    //private WebDropDown ddFilter = null;
    private string strFilter = "";
    private int nPageSize = LayUtil.ResSetDefPageSize;

    public string AutoRefresh = "";
    public string strAutoUrl = "";

    public Hashtable htDG = new Hashtable();
    public Hashtable htReqDataCmd = new Hashtable();
    public Hashtable htReqDataCmdText = new Hashtable();
    
    public Hashtable htLinks = new Hashtable();
    public Hashtable htJoinedTbl = new Hashtable();

    private void Page_Init()
    {
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        if (!IsPostBack)
        {
            ViewState["back"] = LayUtil.GetQueryString(Request, "back");

            ViewState["user"] = LayUtil.GetQueryString(Request, "user");
            ViewState["searchsql"] = LayUtil.GetQueryString(Request, "searchsql");
            ViewState["analgroup"] = LayUtil.GetQueryString(Request, "analgroup");
            ViewState["dashborad"] = LayUtil.GetQueryString(Request, "dashborad");
            ViewState["owned"] = LayUtil.GetQueryString(Request, "owned");
            ViewState["status"] = LayUtil.GetQueryString(Request, "status");
            ViewState["action"] = LayUtil.GetQueryString(Request, "action");
            ViewState["today"] = LayUtil.GetQueryString(Request, "today");
            ViewState["escl"] = LayUtil.GetQueryString(Request, "escl");
            ViewState["srchcmd"] = LayUtil.GetQueryString(Request, "srchcmd");
            ViewState["filter"] = LayUtil.GetQueryString(Request, "filter");
            ViewState["groupque"] = LayUtil.GetQueryString(Request, "groupque");
            ViewState["unassgn"] = LayUtil.GetQueryString(Request, "unassgn");

            //statistics
            ViewState["stat"] = LayUtil.GetQueryString(Request, "stat");
            ViewState["statby"] = LayUtil.GetQueryString(Request, "statby");
            ViewState["col"] = LayUtil.GetQueryString(Request, "col");
            ViewState["row"] = LayUtil.GetQueryString(Request, "row");

            SetPageBack();


            WebDropDown ctrl = ddFilter;
            DropDownItem item;
            item = new DropDownItem("", "");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Assigned " + Application["apprequestterm"] + "s(Open)", "myassignedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Owned " + Application["apprequestterm"] + "s(Open)", "myownedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + Application["apprequestterm"] + "s", "unassigned");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Assigned " + Application["apprequestterm"] + "s(All)", "myassignedall");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Owned " + Application["apprequestterm"] + "s(All)", "myownedall");
            ctrl.Items.Add(item);

            item = new DropDownItem("Assigned " + Application["apprequestterm"] + "s In My Group(Open)", "mygroupassginedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + Application["apprequestterm"] + "s In My Group(Open)", "mygroupunassginedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Assigned " + Application["apprequestterm"] + "s In My Group(All)", "mygroupassginedall");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + Application["apprequestterm"] + "s In My Group(All)", "mygroupunassginedall");
            ctrl.Items.Add(item);

            item = new DropDownItem("Open " + Application["apprequestterm"] + "s at My " + Application["appsiteterm"] + "s", "mysiteopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("All " + Application["apprequestterm"] + "s at My " + Application["appsiteterm"] + "s", "mysiteall");
            ctrl.Items.Add(item);

            item = new DropDownItem(Application["apprequestterm"] + "s Oustside SLA", "outsla");
            ctrl.Items.Add(item);
            item = new DropDownItem(Application["apprequestterm"] + "s at Level 1", "escl1");
            ctrl.Items.Add(item);
            item = new DropDownItem(Application["apprequestterm"] + "s at Level 2", "escl2");
            ctrl.Items.Add(item);
            item = new DropDownItem(Application["apprequestterm"] + "s at Level 3", "escl3");
            ctrl.Items.Add(item);

            item = new DropDownItem("All Open " + Application["apprequestterm"] + "s", "allopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("All " + Application["apprequestterm"] + "s", "all");
            ctrl.Items.Add(item);

            ctrl.DisplayMode = DropDownDisplayMode.DropDownList;
            //ctrl.AutoPostBackFlags.ValueChanged = AutoPostBackFlag.On;
            //ctrl.ValueChanged += new DropDownValueChangedEventHandler(Filter_ValueChanged);

        }

        MyInit();
        
        //SetAutoUrl();
    }

    private void MyInit()
    {

        xmlForm = FormXMLUtil.LoadResultSetXML("sys_request_resultset_xml", Session["User"].ToString());

        //Get Grid Page Size
        string strPageSize = LayUtil.GetAttribute(xmlForm.DocumentElement.GetElementsByTagName("resultset")[0], "maxrows");
        if (LayUtil.IsNumeric(strPageSize))
        {
            nPageSize = int.Parse(strPageSize);
        }

        //if user change column width or order on this page, just save to xml
        CheckColsChange();

        if (htOpenStatus == null)
        {
            htOpenStatus = new Hashtable();
            DataSet dsOpenStatus = RequestDA.GetOpenStatus();
            if (dsOpenStatus != null && dsOpenStatus.Tables.Count > 0)
            {
                foreach (DataRow dr in dsOpenStatus.Tables[0].Rows)
                {
                    htOpenStatus.Add(dr["sys_status_ID"].ToString(), "true");
                }
            }

        }

        LoadCtrl(false);
    }

    #region set/get settings needed when navigate back list
    private void SetPageBack()
    {
        string strURL = Request.QueryString.ToString();
        if (strURL.IndexOf("back=true") == -1)
        {
            if (strURL != "")
            {
                strURL += "&";
            }

            strURL += "back=true";
        }
        Session["ReqList"] = "UserRequest.aspx?" + strURL;
        Session["ButtonBack"] = Session["ReqList"];
    }
    private int GetCurReqClassIndex()
    {
        int nIndex = 0;
        HttpCookie cookie = Request.Cookies["ReqTabIndex"];
        if (cookie != null)
        {
            string strIndex = cookie.Value.ToString();
            if (LayUtil.IsNumeric(strIndex))
            {
                nIndex = int.Parse(strIndex);
            }
        }
        return nIndex;
    }
    private string GetSortField(string strReqClass)
    {
        string strField = "";
        if (bSYS_REQUEST_IDinColumn)
        {
            strField = "sys_request_id";
        }
        if (Session["ReqSortField"] != null)
        {
            Hashtable ht = (Hashtable)Session["ReqSortField"];
            if (ht[strReqClass] != null)
            {
                strField = ht[strReqClass].ToString();
            }
        }
        return strField;
    }
    private Infragistics.Web.UI.SortDirection GetSortAsc(string strReqClass)
    {
        Infragistics.Web.UI.SortDirection asc = Infragistics.Web.UI.SortDirection.Descending;
        if (Session["ReqSortAsc"] != null)
        {
            Hashtable ht = (Hashtable)Session["ReqSortAsc"];
            if (ht[strReqClass] != null)
            {
                asc = (Infragistics.Web.UI.SortDirection)ht[strReqClass];
            }
        }

        return asc;
    }
    private int GetCurReqClassPageIndex(string strReqClass)
    {
        int nPageIndex = 0;
        if (Session["ReqListPage"] != null)
        {
            Hashtable ht = (Hashtable)Session["ReqListPage"];
            if (ht[strReqClass] != null)
            {
                string strIndex = ht[strReqClass].ToString();
                if (LayUtil.IsNumeric(strIndex))
                {
                    nPageIndex = int.Parse(strIndex);
                }
            }
        }

        return nPageIndex;
    }
    private int GetCurReqClassPageSize(string strReqClass)
    {
        int nReqClassPageSize = 0;
        if (Session["ReqListPageSize"] != null)
        {
            Hashtable ht = (Hashtable)Session["ReqListPageSize"];
            if (ht[strReqClass] != null)
            {
                string strSize = ht[strReqClass].ToString();
                if (LayUtil.IsNumeric(strSize))
                {
                    nReqClassPageSize = int.Parse(strSize);
                }
            }
            else
            {
                nReqClassPageSize = nPageSize;
            }
        }
        else
        {
            nReqClassPageSize = nPageSize;
        }
        return nReqClassPageSize;
    }

    private void ClearBackSession()
    {
        Session["ReqSortField"] = null;
        Session["ReqListPage"] = null;
    }
    #endregion

    /// <summary>
    /// Check if user resizes or drops columns
    /// </summary>
    private void CheckColsChange()
    {
        string strWidth = textWidthH.Text;
        string strHeight = textHeightH.Text;
        string strColName = textColNameH.Text;
        string strColWidth = textColWidthH.Text;
        string strMoveCol = textMoveColH.Text;
        string strIndex = textTargetIndexH.Text;

        bool bDirty = false;
        if (strWidth != "" || strHeight != "" || strColName != "" || strMoveCol != "")
        {
            bDirty = true;
        }
        if (strWidth != "")
        {
            LayUtil.SetAttribute((XmlElement)xmlForm.DocumentElement.GetElementsByTagName("resultset")[0], "width", strWidth);
            textWidthH.Text = "";
        }
        if (strHeight != "")
        {
            //LayUtil.SetAttribute((XmlElement)xmlForm.DocumentElement.GetElementsByTagName("resultset")[0], "height", strHeight);
            textHeightH.Text = "";
        }

        if (strColName != "" || strMoveCol != "")
        {
            foreach (XmlNode node in xmlForm.DocumentElement.ChildNodes)
            {
                if (node.Name.Substring(0, 6).ToLower() == "result")
                {
                    //Column Resize
                    if (strColName != "")
                    {
                        foreach (XmlElement child in node.ChildNodes)
                        {
                            if (child.InnerText == strColName)
                            {
                                child.SetAttribute("width", strColWidth);
                            }
                        }
                        textColNameH.Text = "";
                    }

                    //Column Move
                    if (strMoveCol != "")
                    {
                        if (strIndex == "0")
                        {
                            foreach (XmlElement child in node.ChildNodes)
                            {
                                if (child.InnerText == strMoveCol)
                                {
                                    node.RemoveChild(child);
                                    node.InsertBefore(child, node.ChildNodes[0]);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            XmlNode nodeMoved = node.ChildNodes[int.Parse(strIndex) - 1];
                            foreach (XmlElement child in node.ChildNodes)
                            {
                                if (child.InnerText == strMoveCol)
                                {
                                    if (nodeMoved != child)
                                    {
                                        node.RemoveChild(child);
                                        node.InsertAfter(child, nodeMoved);
                                    }
                                    break;
                                }
                            }
                        }
                        textMoveColH.Text = "";
                    }

                    break;
                }
            }
        }

        if (bDirty)
        {
            FormDesignerDA.SaveUserRessetForm("request", Session["User"].ToString(), xmlForm.InnerXml);
        }
    }

    protected void dgResult_ColumnResized(object sender, ColumnResizingEventArgs e)
    {
        ;
    }

    protected void dgResult_ColSorted(object sender, Infragistics.Web.UI.GridControls.SortingEventArgs e)
    {
        WebDataGrid dg = (WebDataGrid)sender;
        string strReqClass = dg.Attributes["ReqClass"].ToString();

        //Sort columns
        Hashtable ht;
        if (Session["ReqSortField"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqSortField"];
        }

        if (ht[strReqClass] != null)
        {
            ht.Remove(strReqClass);
        }

        ht.Add(strReqClass, e.SortedColumns[0].Column.Key);
        Session["ReqSortField"] = ht;

        if (Session["ReqSortAsc"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqSortAsc"];
        }

        if (ht[strReqClass] != null)
        {
            ht.Remove(strReqClass);
        }

        ht.Add(strReqClass, e.SortedColumns[0].SortDirection);
        Session["ReqSortAsc"] = ht;

        UpdateDGData(strReqClass,dg);
    }
    private void UpdateDGData(string strReqClass, WebDataGrid dg)
    {
        SqlCommand cmd = (SqlCommand)htReqDataCmd[strReqClass];
        string strCmdText = htReqDataCmdText[strReqClass].ToString();
        string strField = GetSortField(strReqClass);

        Infragistics.Web.UI.SortDirection dir = GetSortAsc(strReqClass);
        if (strField != "")
        {
            string strOrderBy = "";
            if (htLinks[strField] != null)
            {
                strOrderBy = "[request].";
            }

            if (dir == Infragistics.Web.UI.SortDirection.Ascending)
            {
                strOrderBy = strOrderBy + strField + " asc";
            }
            else
            {
                strOrderBy = strOrderBy + strField + " desc";
            }
            //cmd.Parameters["@strReqClass"].Value = strReqClass;
            cmd.CommandText = strCmdText.Replace("(ORDER BY sys_request_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
        }
        DataSet dsReq = LayUtilDA.GetDSCMD(cmd);

        SetColor(dsReq);

        if (strField != "")
        {
            if (dg.Columns.FromKey(strField) != null)
            {
                dg.Behaviors.Sorting.SortedColumns.Add(strField, dir);
                string strOrder = dir.ToString();
                string strSort = "";
                if (strOrder == "Ascending")
                {
                    strSort = "asc";
                }
                else if (strOrder == "Descending")
                {
                    strSort = "desc";
                }
                // Changed by Sparsh ID 194
                if((dsReq.Tables.Count > 0)&&(dsReq.Tables[0].Rows.Count > 0))
                {
                    dsReq.Tables[0].DefaultView.Sort = strField + " " + strSort;

                    dg.DataSource = dsReq.Tables[0].DefaultView;
                    // Sparsh ID 194
                    dg.Attributes["ReqClass"] = strReqClass;
                    dg.DataBind();
                }
            }
        }
        if (dsReq.Tables.Count > 0)
        {
            dg.DataSource = dsReq.Tables[0].DefaultView;
            dg.Attributes["ReqClass"] = strReqClass;
            dg.DataBind();
            // for bug  986
            dg.RequestFullAsyncRender();
        }
        //}

    }
    public void dgResult_PageIndexChanged(object sender)
    {
        //dg.Behaviors.Paging.PageIndex = 6;
        WebDataGrid dg = (WebDataGrid)sender;
        string strReqClass = dg.Attributes["ReqClass"].ToString();

        //Sort columns
        Hashtable ht;
        if (Session["ReqListPage"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqListPage"];
        }

        ht[strReqClass] = dg.Behaviors.Paging.PageIndex;
        Session["ReqListPage"] = ht;

        //dg.Behaviors.Paging.PagerTemplate = new MyPagerTemplate(dg);
    }
    public void dgResult_PageSizeChanged(object sender)
    {
        //dg.Behaviors.Paging.PageIndex = 6;
        WebDataGrid dg = (WebDataGrid)sender;
        string strReqClass = dg.Attributes["ReqClass"].ToString();

        //Sort columns
        Hashtable ht;
        if (Session["ReqListPageSize"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqListPageSize"];
        }

        ht[strReqClass] = dg.Behaviors.Paging.PageSize;
        Session["ReqListPageSize"] = ht;

        //dg.Behaviors.Paging.PagerTemplate = new MyPagerTemplate(dg);
    }

    //Done By Shailesh but use same logic as previous For Custome Paging
    public void dgResult_PageSizeChanged(int pageSize, string reqClass)
    {
        //Sort columns
        Hashtable ht;
        if (Session["ReqListPageSize"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqListPageSize"];
        }
        ht[reqClass] = pageSize;
        Session["ReqListPageSize"] = ht;

    }
    //Done By Shailesh For Custome Paging
    public void dgResult_PageIndexChanged(string controlAttribute, int currentPageIndex)
    {
        if (string.IsNullOrEmpty(controlAttribute)) return;
        Hashtable ht;
        if (Session["ReqListPage"] == null)
        {
            ht = new Hashtable();
        }
        else
        {
            ht = (Hashtable)Session["ReqListPage"];
        }

        ht[controlAttribute] = currentPageIndex;
        Session["ReqListPage"] = ht;


    }

    private class MyPagerTemplate : ITemplate
    {
        WebDataGrid dg;
        Control rootCtrl;
        int nPageIndex;
        int nCurPageIndex;
        int nPageSize;
        public MyPagerTemplate(WebDataGrid dgGrid, Page page, int pageindex, int pagesize)
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
                ((Analyst_UserRequest)dg.Page).dgResult_PageIndexChanged(dg);
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
                ((Analyst_UserRequest)dg.Page).dgResult_PageSizeChanged(dg);
            }
        }

        public void LoadCtrl()
        {
            rootCtrl.Controls.Clear();
            //int nPageIndex = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageIndex"].ToString());
            //int nSize = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageSize"].ToString());
            int nSize = nPageSize;
            Analyst_UserRequest page = ((Analyst_UserRequest)dg.Page);

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
            lb.Text = nStart.ToString() + " to " + nEnd + " of " + nTotal + " " + page.Application["apprequestterm"] + "s)";

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
            int nDefSize = page.nPageSize;
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
            ((Analyst_UserRequest)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnPrev_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex - 1;
            LoadCtrl();
            ((Analyst_UserRequest)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnLast_Click(object sender, EventArgs e)
        {
            nPageIndex = dg.Behaviors.Paging.PageCount - 1;
            LoadCtrl();
            ((Analyst_UserRequest)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnNext_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex + 1;
            LoadCtrl();
            ((Analyst_UserRequest)dg.Page).dgResult_PageIndexChanged(dg);
        }

    }

    /// <summary>
    /// Generate WebDataGrid control
    /// </summary>
    /// <param name="child"></param>
    /// <param name="bInTab"></param>
    /// <returns></returns>
    private WebDataGrid GenDG(XmlElement child, bool bInTab)
    {
        WebDataGrid dgResult = new WebDataGrid();
        dgResult.EnableViewState = false;
        dgResult.EnableDataViewState = false;
        dgResult.AutoGenerateColumns = false;
        dgResult.Behaviors.CreateBehavior<Paging>();
        dgResult.Behaviors.CreateBehavior<ColumnResizing>();
        dgResult.Behaviors.CreateBehavior<ColumnMoving>();
        dgResult.Behaviors.CreateBehavior<Sorting>();

        // for bug  986
        dgResult.Behaviors.CreateBehavior<EditingCore>();
        dgResult.Behaviors.EditingCore.AutoCRUD = false;

        //dgResult.Behaviors.CreateBehavior<Filtering>();
        //dgResult.AjaxIndicator.


        dgResult.Behaviors.ColumnMoving.ColumnMovingClientEvents.HeaderDropped = "dgResult_ColDropped";
        dgResult.Behaviors.ColumnResizing.ColumnResizingClientEvents.ColumnResized = "dgResult_ColResized";
        dgResult.Behaviors.Sorting.ColumnSorted += new ColumnSortedHandler(dgResult_ColSorted);
        //dgResult.Behaviors.Paging.PageIndexChanged += new PageIndexChangedHandler(dgResult_PageIndexChanged);
        dgResult.Behaviors.ColumnResizing.AutoPostBackFlags.ColumnResized = true;

        //dgResult.Behaviors.Paging.PageSize = GetCurReqClassPageSize;
        dgResult.EmptyRowsTemplate = new MyEmptyRowsTemplate("No " + Application["apprequestterm"]);

        dgResult.Font.Name = Application["appfont"].ToString();
        dgResult.Font.Size = new FontUnit(Application["appdeffontpx"].ToString() + "px");
        dgResult.ForeColor = LayUtil.GetColorFrmStr(Application["appdeffontcolor"].ToString());

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /*
        BoundDataField templateColumn1 = (BoundDataField)dgResult.Columns["test"];
        if (templateColumn1 == null)
        {
            BoundDataField field1 = new BoundDataField();
            field1.Key = "test";
            field1.DataFieldName = "sys_asset_id";
            field1.Header.Text = "test Column";
            string strAlign = "center";
            string strHeaderAlign = "center";
            string strAlignCss = "Col_Center";
            if (strAlign == "left")
            {
                strAlignCss = "Col_Left";
            }
            else if (strAlign == "right")
            {
                strAlignCss = "Col_Right";
            }
            field1.CssClass = strAlignCss;

            string strHeaderAlignCss = "Col_Center";
            if (strHeaderAlign == "left")
            {
                strHeaderAlignCss = "Col_Left";
            }
            else if (strHeaderAlign == "right")
            {
                strHeaderAlignCss = "Col_Right";
            }
            field1.Header.CssClass = strHeaderAlignCss;

            field1.Width = new Unit("50px");
            dgResult.Columns.Add(field1);
        }
        */
        ///////////////////////////////////////////////////////////////////////////////////////////////

        string strWidth = child.GetAttribute("width");
        if (LayUtil.IsNumeric(strWidth))
        {
            dgResult.Width = new Unit(strWidth + "px");
        }
        string strHeight = child.GetAttribute("height");
        if (LayUtil.IsNumeric(strHeight))
        {
            dgResult.Height = new Unit(strHeight + "px");
        }

        //Setting Columns
        foreach (XmlElement node in child.ChildNodes)
        {
            if (node.InnerText == "sys_request_id")
            {
                bSYS_REQUEST_IDinColumn = true;
            }

            AddColumn(node, dgResult);
        }

        //if (!bInTab)
        //{
        //    dgResult.Style["Top"] = child.GetAttribute("top") + "px";
        //    dgResult.Style["Left"] = child.GetAttribute("left") + "px";
        //    dgResult.Style["Position"] = "absolute";
        //}

        return dgResult;
    }


    private void ClearVwState()
    {
        ViewState["back"] = "";

        ViewState["user"] = "";
        ViewState["searchsql"] = "";
        ViewState["analgroup"] = "";
        ViewState["dashborad"] = "";
        ViewState["owned"] = "";
        ViewState["status"] = "";
        ViewState["action"] = "";
        ViewState["today"] = "";
        ViewState["escl"] = "";
        ViewState["srchcmd"] = "";
    }

    private void SetColor(DataSet ds)
    {
        if (SysDA.GetSettingValue("appcolorrow", Application) == "true")
        {
            if (htReqColor == null)
            {
                htReqColor = new Hashtable();
            }

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string strReqId = dr["sys_request_id"].ToString();
                    if (htReqColor[strReqId] == null)
                    {
                        //System.Drawing.Color colorReq = System.Drawing.Color.White;
                        string strColor = "";
                        if (dr["statusforcecolor"].ToString() == "1")
                        {
                            strColor = dr["statuscolor"].ToString();
                        }
                        else
                        {
                            if (htOpenStatus[dr["sys_requeststatus"].ToString()] != null)
                            {
                                if (dr["sys_resolve"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_resolve"].ToString())) > 0)
                                {
                                    strColor = SysDA.GetSettingValue("appexpiredcolor", Application);
                                }
                                else if (dr["sys_escalate3"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate3"].ToString())) > 0)
                                {
                                    strColor = SysDA.GetSettingValue("appescalate3color", Application);
                                }
                                else if (dr["sys_escalate2"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate2"].ToString())) > 0)
                                {
                                    strColor = SysDA.GetSettingValue("appescalate2color", Application);
                                }
                                else if (dr["sys_escalate1"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate1"].ToString())) > 0)
                                {
                                    strColor = SysDA.GetSettingValue("appescalate1color", Application);
                                }
                                else if (dr["sys_responded"].ToString() == "" && dr["sys_respond"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_respond"].ToString())) > 0)
                                {
                                    strColor = SysDA.GetSettingValue("appescalate0color", Application);
                                }

                            }
                        }

                        htReqColor[strReqId] = strColor;
                    }
                }
            }
        }
        else
        {
            if (htPriorityColor == null)
            {
                htPriorityColor = new Hashtable();
            }
            if (htStatusColor == null)
            {
                htStatusColor = new Hashtable();
            }
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string strReqId = dr["sys_request_id"].ToString();
                    if (htStatusColor[strReqId] == null)
                    {
                        htStatusColor[strReqId] = dr["statuscolor"].ToString();
                    }

                    if (htPriorityColor[strReqId] == null && htOpenStatus[dr["sys_requeststatus"].ToString()] != null)
                    {
                        string strColor = "";
                        if (dr["sys_resolve"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_resolve"].ToString())) > 0)
                        {
                            strColor = SysDA.GetSettingValue("appexpiredcolor", Application);
                        }
                        else if (dr["sys_escalate3"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate3"].ToString())) > 0)
                        {
                            strColor = SysDA.GetSettingValue("appescalate3color", Application);
                        }
                        else if (dr["sys_escalate2"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate2"].ToString())) > 0)
                        {
                            strColor = SysDA.GetSettingValue("appescalate2color", Application);
                        }
                        else if (dr["sys_escalate1"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_escalate1"].ToString())) > 0)
                        {
                            strColor = SysDA.GetSettingValue("appescalate1color", Application);
                        }
                        else if (dr["sys_responded"].ToString() == "" && dr["sys_respond"].ToString() != "" && DateTime.Compare(DateTime.Now, DateTime.Parse(dr["sys_respond"].ToString())) > 0)
                        {
                            strColor = SysDA.GetSettingValue("appescalate0color", Application);
                        }

                        htPriorityColor[strReqId] = strColor;
                    }

                }
            }
        }
    }

    private void GenDGControl(XmlElement child, Control ctrlRoot)
    {
        //GenDG(child, true);

        DataSet dsUser = UserDA.GetUserInfo(Session["User"].ToString());
        if (dsUser == null || dsUser.Tables.Count <= 0 || dsUser.Tables[0].Rows.Count <= 0)
            return;

        AutoRefresh = dsUser.Tables[0].Rows[0]["sys_autorefresh"].ToString();
        if (AutoRefresh == "0")
        {
            AutoRefresh = "";
        }

        string strSqlPre = "";

        SqlCommand cmd = null;

        SqlParameter parameter;
        string strRecordCount = " Select COUNT(*) From ";
        string strSelectedRecord = " Select * From ";

        //string strSqlField = "(SELECT Top 100 percent [request].*, ROW_NUMBER() OVER (ORDER BY sys_request_id desc) as RowIndex, (SELECT Sys_Status FROM [status] WHERE sys_status_id = sys_requeststatus) As Status, (SELECT Sys_StatusForceColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusForceColor, (SELECT Sys_StatusColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusColor, (SELECT urgency.sys_urgency FROM urgency WHERE request.sys_urgency=urgency.sys_urgency_id)AS UrgencyName, (SELECT impact.sys_impact FROM impact WHERE request.sys_impact=impact.sys_impact_id)AS ImpactName, (SELECT itservice.sys_itservice FROM itservice WHERE request.sys_itservice=itservice.sys_itservice_id)AS ITServiceName,(SELECT Count(sys_action_id) FROM [action] WHERE action.sys_request_id = request.sys_request_id) As [ActionCount], (SELECT Count(sys_comment_id) FROM [comments] WHERE comments.sys_request_id = request.sys_request_id) As [CommentCount], (SELECT count(comments.sys_comment_id) FROM comments INNER JOIN commentsviewed ON comments.sys_comment_id = commentsviewed.sys_comment_id WHERE commentsviewed.sys_username = @strUser AND comments.sys_request_id = request.sys_request_id) As [ReadCommentCount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype = 1) AS [spawncount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype is null) AS [linkcount] FROM request ";
        string strSqlField = "(SELECT Top 100 percent [request].*, ROW_NUMBER() OVER (ORDER BY sys_request_id desc) as RowIndex, (SELECT Sys_Status FROM [status] WHERE sys_status_id = sys_requeststatus) As Status, (SELECT Sys_StatusForceColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusForceColor, (SELECT Sys_StatusColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusColor, (SELECT urgency.sys_urgency FROM urgency WHERE request.sys_urgency=urgency.sys_urgency_id)AS UrgencyName, (SELECT impact.sys_impact FROM impact WHERE request.sys_impact=impact.sys_impact_id)AS ImpactName, (SELECT itservice.sys_itservice FROM itservice WHERE request.sys_itservice=itservice.sys_itservice_id)AS ITServiceName,(SELECT Count(sys_action_id) FROM [action] WHERE action.sys_request_id = request.sys_request_id) As [ActionCount], (SELECT Count(sys_comment_id) FROM [comments] WHERE comments.sys_request_id = request.sys_request_id) As [CommentCount], (SELECT count(comments.sys_comment_id) FROM comments INNER JOIN commentsviewed ON comments.sys_comment_id = commentsviewed.sys_comment_id WHERE commentsviewed.sys_username = @strUser AND comments.sys_request_id = request.sys_request_id) As [ReadCommentCount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype = 1) AS [spawncount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype is null) AS [linkcount] ";

        string strJoinTbl = "";
        foreach (XmlElement node in child.ChildNodes)
        {
            string strDispTbl = node.GetAttribute("displaytable");
            string strDispLink = node.GetAttribute("displaytablelink");
            string strLink = node.GetAttribute("mastertablelink");

            htLinks[strLink] = strLink;

            if (strDispTbl != "" && strDispTbl != "request")
            {
                string strAsField = node.InnerText;
                if (node.InnerText == "sys_forename")
                {
                    if (strDispTbl == "euser")
                    {
                        strAsField = "euser_forename";
                    }
                    else if (strDispTbl == "user")
                    {
                        if (strLink == "sys_assignedto")
                        {
                            strAsField = "assignedto_forename";
                        }
                        else if (strLink == "sys_ownedby")
                        {
                            strAsField = "ownedby_forename";
                        }
                        else if (strLink == "sys_respondedby")
                        {
                            strAsField = "respondedby_forename";
                        }
                    }
                }
                if (node.InnerText == "sys_surname")
                {
                    if (strDispTbl == "euser")
                    {
                        strAsField = "euser_surname";
                    }
                    else if (strDispTbl == "user")
                    {
                        if (strLink == "sys_assignedto")
                        {
                            strAsField = "assignedto_surname";
                        }
                        else if (strLink == "sys_ownedby")
                        {
                            strAsField = "ownedby_surname";
                        }
                        else if (strLink == "sys_respondedby")
                        {
                            strAsField = "respondedby_surname";
                        }
                    }
                }

                //strSqlField += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[request]." + strLink + ") AS " + strAsField;

                // Written by Sparsh ID 188
                if (strAsField != "")
                {
                    string dmpstrAsField = "";
                    string dmpDbField = "AS " + strAsField;
                    if (strSqlField.Contains(dmpDbField))
                    {
                        dmpstrAsField = strAsField + Convert.ToString(i);
                        strSqlField += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[request]." + strLink + ") AS " + dmpstrAsField;
                        i++;
                    }
                    else
                strSqlField += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[request]." + strLink + ") AS " + strAsField;
                }
                // End by Sparsh ID 188

                if (htJoinedTbl[strDispTbl] == null)
                {
                    strJoinTbl += " LEFT JOIN [" + strDispTbl + "] ON [" + strDispTbl + "]." + strDispLink + "=[request]." + strLink + " ";
                    htJoinedTbl[strDispTbl] = "true";
                }
            }
        }

        strSqlField += " FROM [request] " + strJoinTbl;

        string strOpenStatus = " (sys_requeststatus IN (SELECT sys_status_ID FROM status WHERE sys_suspend<>2))";

        bool bWhere = true;

        bool bTalAllSel = false;

        if (ViewState["searchsql"] != null && ViewState["searchsql"].ToString() != "")
        {
            if (Session["ReqSrchCmd"] != null)
            {
                SqlCommand cmdSrch = (SqlCommand)Session["ReqSrchCmd"];

                cmd = cmdSrch.Clone();

                strSqlPre = cmd.CommandText;
                if (strSqlPre == "")
                {
                    bWhere = false;
                }
                else
                {
                    bWhere = true;
                }

                strSqlPre = strSqlField + strSqlPre;
            }

            if (ViewState["back"].ToString() == "")
            {
                bTalAllSel = true;
            }
        }
        else if (ViewState["dashborad"] != null && ViewState["dashborad"].ToString() != "")
        {
            if (ViewState["srchcmd"].ToString() == "true" && Session["ReqDashboardCmd"] != null)
            {
                SqlCommand cmdDsh = (SqlCommand)Session["ReqDashboardCmd"];

                cmd = cmdDsh.Clone();

                strSqlPre = cmd.CommandText;
                if (strSqlPre == "")
                {
                    bWhere = false;
                }
                else
                {
                    bWhere = true;
                }

                strSqlPre = strSqlField + strSqlPre;
            }
            else
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;

                if (ViewState["user"] != null && ViewState["user"].ToString() != "")
                {
                    string strQUser = ViewState["user"].ToString();
                    if (strQUser == "!")
                    {
                        string strGrp = ViewState["analgroup"].ToString();
                        if (strGrp == "")
                        {
                            strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null AND " + strOpenStatus;

                            bWhere = true;
                        }
                        else if (strGrp == "!")
                        {
                            string strAssigned = ViewState["assigned"].ToString();
                            if (strAssigned == "false")
                            {
                                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null AND " + strOpenStatus;

                                bWhere = true;
                            }
                            else
                            {
                                strSqlPre = strSqlField + "WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup Is Null AND " + strOpenStatus;

                                bWhere = true;
                            }
                        }
                        else
                        {
                            string strAssigned = ViewState["assigned"].ToString();
                            if (strAssigned == "false")
                            {
                                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup =@strGrp AND " + strOpenStatus;

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@strGrp";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = strGrp;
                                cmd.Parameters.Add(parameter);

                                bWhere = true;
                            }
                            else
                            {
                                strSqlPre = strSqlField + "WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup =@strGrp AND " + strOpenStatus;

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@strGrp";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = strGrp;
                                cmd.Parameters.Add(parameter);

                                bWhere = true;
                            }
                        }
                    }
                    else
                    {
                        if (Session["User"].ToString() != strQUser)
                            return;

                        strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser AND " + strOpenStatus;

                        bWhere = true;
                    }
                }
                else if (ViewState["status"] != null && ViewState["status"].ToString() != "")
                {
                    if (ViewState["status"].ToString() == "hold")
                    {
                        strSqlPre = strSqlField + "WHERE sys_requeststatus in (SELECT sys_status_ID FROM [status] WHERE [sys_suspend]=1)";
                    }
                    else if (ViewState["status"].ToString() == "unassigned")
                    {
                        strSqlPre = strSqlField + "WHERE (sys_assignedto IS NULL OR sys_assignedto='') AND " + strOpenStatus;
                    }
                    else if (ViewState["status"].ToString() == "missresponse")
                    {
                        strSqlPre = strSqlField + "WHERE sys_responded IS NULL AND sys_respond IS NOT NULL AND GetDate() > sys_respond AND " + strOpenStatus;
                    }
                    else
                    {
                        strSqlPre = strSqlField + "WHERE " + strOpenStatus;
                    }

                    bWhere = true;
                }
                else if (ViewState["action"] != null && ViewState["action"].ToString() != "")
                {
                    if (ViewState["action"].ToString() == "close" && ViewState["today"].ToString() != "")
                    {
                        strSqlPre = strSqlField + "WHERE DATEDIFF(D,sys_requestclosedate, GETDATE())=0 AND (sys_requeststatus=0 OR (sys_requeststatus IN (SELECT sys_status_ID FROM status WHERE sys_suspend=2)))";
                    }
                    else if (ViewState["action"].ToString() == "log" && ViewState["today"].ToString() != "")
                    {
                        strSqlPre = strSqlField + "WHERE DATEDIFF(D,sys_requestdate, GETDATE())=0";
                    }

                    bWhere = true;
                }
                else if (ViewState["escl"] != null && ViewState["escl"].ToString() != "")
                {
                    string strLevel = ViewState["escl"].ToString();

                    if (strLevel == "4")
                    {
                        strSqlPre = strSqlField + "WHERE sys_resolve<getdate() AND " + strOpenStatus;
                    }
                    else if (strLevel == "3")
                    {
                        strSqlPre = strSqlField + "WHERE (sys_escalate3<getdate() AND ((sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_resolve is NULL) )) AND " + strOpenStatus;
                    }
                    else if (strLevel == "2")
                    {
                        strSqlPre = strSqlField + "WHERE (sys_escalate2<getdate() AND ((sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NULL) )) AND " + strOpenStatus;
                    }
                    else if (strLevel == "1")
                    {
                        strSqlPre = strSqlField + "WHERE ( sys_escalate1<getdate() AND ((sys_escalate2 is NOT NULL AND sys_escalate2>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NULL) )) AND " + strOpenStatus;
                    }

                    bWhere = true;
                }
                else if (ViewState["owned"] != null && ViewState["owned"].ToString() != "")
                {
                    strSqlPre = strSqlField + "WHERE sys_ownedby =@strOwned AND " + strOpenStatus;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@strOwned";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = ViewState["owned"].ToString();
                    cmd.Parameters.Add(parameter);

                    bWhere = true;
                }
                else if (ViewState["homesite"] != null && ViewState["homesite"].ToString() != "")
                {
                    DataSet dsSite = UserDA.GetUserSites(Session["User"].ToString());
                    string strSiteSql = "";
                    if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
                    {
                        strSiteSql = " AND (sys_siteid=''";
                        foreach (DataRow dr in dsSite.Tables[0].Rows)
                        {
                            strSiteSql += " OR sys_siteid='" + dr["sys_siteid"] + "'";
                        }

                        strSiteSql += ") ";
                    }

                    string strGrpSql = "";
                    DataSet dsGrp = UserDA.GetUserGrps(Session["User"].ToString());
                    if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                    {
                        strGrpSql = " AND (sys_assignedtoanalgroup=''";
                        foreach (DataRow dr in dsGrp.Tables[0].Rows)
                        {
                            strGrpSql += " OR sys_assignedtoanalgroup='" + dr["sys_analgroup"] + "'";
                        }
                        strGrpSql += ") ";
                    }


                    string strHomeSite = ViewState["homesite"].ToString();
                    if (strHomeSite == "openmy")
                    {
                        strSqlPre = strSqlField + "WHERE " + strOpenStatus + strSiteSql;

                        bWhere = true;
                    }
                    else if (strHomeSite == "allmy")
                    {
                        strSqlPre = strSqlField + "WHERE " + strSiteSql;

                        bWhere = true;
                    }
                    else if (strHomeSite == "assignedmygroup")
                    {
                        strSqlPre = strSqlField + "WHERE sys_assignedto IS NOT NULL " + strGrpSql;

                        bWhere = true;
                    }
                    else if (strHomeSite == "unassignedmygroup")
                    {
                        strSqlPre = strSqlField + "WHERE sys_assignedto IS NULL " + strGrpSql;

                        bWhere = true;
                    }
                    else if (strHomeSite == "assignedmygroupopen")
                    {
                        strSqlPre = strSqlField + "WHERE sys_assignedto IS NOT NULL AND " + strOpenStatus + strGrpSql;

                        bWhere = true;
                    }
                    else if (strHomeSite == "unassignedmygroupopen")
                    {
                        strSqlPre = strSqlField + "WHERE sys_assignedto IS NULL AND " + strOpenStatus + strGrpSql;

                        bWhere = true;
                    }
                }
                else
                {
                    strSqlPre = strSqlField + "WHERE " + strOpenStatus;

                    bWhere = true;
                }
            }

            if (ViewState["back"].ToString() == "")
            {
                bTalAllSel = true;
            }
        }
        else if (ViewState["groupque"].ToString() == "true")
        {
            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            if (ViewState["unassgn"].ToString() == "true")
            {
                string strGrp = ViewState["analgroup"].ToString();
                if (strGrp != "")
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NULL OR sys_assignedto='') AND (sys_assignedtoanalgroup=@strGrp) AND " + strOpenStatus;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@strGrp";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strGrp;
                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NULL OR sys_assignedto='') AND (sys_assignedtoanalgroup IS NULL OR sys_assignedtoanalgroup='') AND " + strOpenStatus;
                }
            }
            else
            {
                string strGrp = ViewState["analgroup"].ToString();
                if (strGrp != "")
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NOT NULL AND sys_assignedto<>'') AND (sys_assignedtoanalgroup=@strGrp) AND " + strOpenStatus;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@strGrp";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strGrp;
                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NOT NULL AND sys_assignedto<>'') AND (sys_assignedtoanalgroup IS NULL OR sys_assignedtoanalgroup='') AND " + strOpenStatus;
                }
            }

            bWhere = true;
            bTalAllSel = true;
        }
        /*
        else if (ViewState["stat"].ToString() == "true")
        {
            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            
            if (ViewState["statby"].ToString() == "user")
            {
                string strCol = ViewState["col"].ToString();
                string strUser = ViewState["row"].ToString();
                if(strCol == "CountOfOpen")
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NULL OR sys_assignedto='') AND (sys_assignedtoanalgroup=@strGrp) AND " + strOpenStatus;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@strGrp";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strGrp;
                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    strSqlPre = strSqlField + " WHERE (sys_assignedto IS NULL OR sys_assignedto='') AND (sys_assignedtoanalgroup IS NULL OR sys_assignedtoanalgroup='') AND " + strOpenStatus;
                }
            bWhere = true;
            bTalAllSel = true;
        }
         */
        else
        {
            strFilter = ddFilter.SelectedValue;
            if (strFilter == "")
            {
                string strHome = dsUser.Tables[0].Rows[0]["sys_home"].ToString();

                switch (strHome)
                {
                    case "1":
                        strFilter = "myassignedopen";
                        break;
                    case "2":
                        strFilter = "myownedopen";
                        break;
                    case "3":
                        strFilter = "unassigned";
                        break;
                    case "4":
                        strFilter = "all";
                        break;
                    case "5":
                        strFilter = "allopen";
                        break;
                    case "8":
                        strFilter = "mysiteopen";
                        break;
                    case "9":
                        strFilter = "mysiteall";
                        break;
                    case "10":
                        strFilter = "mygroupassginedall";
                        break;
                    case "11":
                        strFilter = "mygroupunassginedall";
                        break;
                    case "12":
                        strFilter = "mygroupassginedopen";
                        break;
                    case "13":
                        strFilter = "mygroupunassginedopen";
                        break;
                    default:
                        strFilter = "myassignedopen";
                        break;
                }

                ddFilter.SelectedValue = strFilter;
            }

            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            if (strFilter == "myassignedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "myownedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_ownedby =@strOwned AND " + strOpenStatus;

                parameter = new SqlParameter();
                parameter.ParameterName = "@strOwned";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = Session["User"].ToString();
                cmd.Parameters.Add(parameter);

                bWhere = true;
            }
            else if (strFilter == "unassigned")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "myassignedall")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser";
                bWhere = true;
            }
            else if (strFilter == "myownedall")
            {
                strSqlPre = strSqlField + "WHERE sys_ownedby =@strOwned";

                parameter = new SqlParameter();
                parameter.ParameterName = "@strOwned";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = Session["User"].ToString();
                cmd.Parameters.Add(parameter);

                bWhere = true;
            }
            else if (strFilter == "mygroupassginedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "mygroupunassginedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "mygroupassginedall")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser)";
                bWhere = true;
            }
            else if (strFilter == "mygroupunassginedall")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser)";
                bWhere = true;
            }
            else if (strFilter == "mysiteopen")
            {
                strSqlPre = strSqlField + "WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username=@strUser) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "mysiteall")
            {
                strSqlPre = strSqlField + "WHERE sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username=@strUser)";
                bWhere = true;
            }
            else if (strFilter == "outsla")
            {
                strSqlPre = strSqlField + "WHERE sys_resolve<getdate() AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "escl3")
            {
                strSqlPre = strSqlField + "WHERE (sys_escalate3<getdate() AND ((sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_resolve is NULL) )) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "escl2")
            {
                strSqlPre = strSqlField + "WHERE (sys_escalate2<getdate() AND ((sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NULL) )) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "escl1")
            {
                strSqlPre = strSqlField + "WHERE ( sys_escalate1<getdate() AND ((sys_escalate2 is NOT NULL AND sys_escalate2>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NULL) )) AND " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "allopen")
            {
                strSqlPre = strSqlField + "WHERE " + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "all")
            {
                strSqlPre = strSqlField;
                bWhere = false;
            }
            ///for empty test
            //strSqlPre = "SELECT *, (SELECT Sys_Status FROM status WHERE sys_status_id = sys_requeststatus) As Status, (SELECT Sys_StatusForceColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusForceColor, (SELECT Sys_StatusColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusColor, (SELECT Count(sys_comment_id) FROM [comments] WHERE comments.sys_request_id = request.sys_request_id) As [CommentCount], (SELECT count(comments.sys_comment_id) FROM comments INNER JOIN commentsviewed ON comments.sys_comment_id = commentsviewed.sys_comment_id WHERE commentsviewed.sys_username = @strUser AND comments.sys_request_id = request.sys_request_id) As [readCommentCount], (SELECT Count(sys_action_id) FROM action WHERE action.sys_request_id = request.sys_request_id)As [ActionCount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype = 1) AS [spawncount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype is null) AS [linkcount] FROM request WHERE sys_requeststatus = -1";

            //bWhere = true;
        }

        ///Add Group Restriction Clauses
        ///
        string strAccess = dsUser.Tables[0].Rows[0]["sys_reqaccess"].ToString();
        string strAccessSite = dsUser.Tables[0].Rows[0]["sys_reqaccesssite"].ToString();

        if (strAccess == "1")
        {
            if (bWhere)
            {
                strSqlPre += " AND ";
            }
            else
            {
                strSqlPre += " WHERE ";
            }

            strSqlPre += "(sys_ownedby=@strUser OR sys_assignedto=@strUser) ";

            bWhere = true;
        }
        else if (strAccess == "3")
        {
            if (bWhere)
            {
                strSqlPre += " AND ";
            }
            else
            {
                strSqlPre += " WHERE ";
            }

            strSqlPre += "(sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser)) ";

            bWhere = true;
        }
        else if (strAccess == "2")
        {
            if (bWhere)
            {
                strSqlPre += " AND ";
            }
            else
            {
                strSqlPre += " WHERE ";
            }

            strSqlPre += "(sys_ownedby = @strUser OR sys_assignedto=@strUser OR sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser)) ";

            bWhere = true;
        }


        if (strAccessSite == "1")
        {
            if (bWhere)
            {
                strSqlPre += " AND ";
            }
            else
            {
                strSqlPre += " WHERE ";
            }

            strSqlPre += "(sys_siteid IN (SELECT sys_siteid FROM analsite WHERE sys_username = @strUser)) ";

            bWhere = true;
        }
        /*
        parameter = new SqlParameter();
        parameter.ParameterName = "@strUser";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = Session["User"].ToString();
        cmd.Parameters.Add(parameter);
         */


        parameter = new SqlParameter();
        parameter.ParameterName = "@strUser";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = Session["User"].ToString();
        cmd.Parameters.Add(parameter);

        //////////////////////////////////////////////////////////////////////////////



        string strAllowTab = SysDA.GetSettingValue("requestclassusetabs", Application);

        //if only one request class allowed, do not use tab
        DataSet dsReqClass;
        if (dsUser.Tables[0].Rows[0]["sys_requestclassrestrict"].ToString() == "1")
        {
            dsReqClass = UserDA.GetUserReqClassInfoOrderByDescription(Session["User"].ToString());
        }
        else
        {
            dsReqClass = LibReqClassBR.GetReqClassListOrderByDescription();
        }

        if (dsReqClass == null || dsReqClass.Tables.Count <= 0 || dsReqClass.Tables[0].Rows.Count <= 0)
            return;

        if (dsReqClass.Tables[0].Rows.Count == 1)
        {
            strAllowTab = "false";
        }

        if (strAllowTab == "true")
        {

            // if only 1 Request class, still use tab?
            /*
            if (dsReqClass.Tables[0].Rows.Count == 1)
            {
            }
            */

            WebTab ReqClassTab = new WebTab();
            ReqClassTab.Tabs.Clear();
            ReqClassTab.EnableOverlappingTabs = true;

            ReqClassTab.ID = "ReqClassTabs";

            ReqClassTab.BorderWidth = new Unit("0px");
            ReqClassTab.BorderColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));
            //ReqClassTab.Tabs.Clear();

            //ctrlRoot.Controls.Add(ReqClassTab);
            //ReqClassTab.TabClick += new TabClickEventHandler(ReqClassTab_TabClick);
            //ReqClassTab.ClientSideEvents.AfterSelectedTabChange = "";
            //ReqClassTab.AutoPostBack = true;

            ReqClassTab.Style["Top"] = child.GetAttribute("top") + "px";
            ReqClassTab.Style["Left"] = child.GetAttribute("left") + "px";
            ReqClassTab.Style["Position"] = "absolute";

            ContentTabItem tab;

            WebDataGrid dg;

            /*
            string strOrder = "sys_request_id desc";
            if (Session["sort_anal_request_listview"] != null)
            {
                strOrder = Session["sort_anal_request_listview"].ToString();
            }
             */

            string strSql = strSqlPre;
            if (bWhere)
            {
                strSql += " AND ";
            }
            else
            {
                strSql += " WHERE ";
            }

            strSql = strSql + " [request].sys_requestclass_id=@strReqClass";

            foreach (DataRow dr in dsReqClass.Tables[0].Rows)
            {
                //SqlCommand cmdReq = new SqlCommand(

                tab = new ContentTabItem();
                tab.Text = dr["sys_requestclass_desc"].ToString().Replace("&", "and");

                string strReqClass = dr["sys_requestclass_id"].ToString();
                dg = GenDG(child, true);

                //cmd.Parameters.RemoveAt
                dg.ID = "dg" + dr["sys_requestclass_desc"].ToString().Replace("&", "and"); ;
                htDG[dg.ID] = dg;
                tab.AutoSize = DefaultableBoolean.True;
                /*
                ReqClassTab.Width = new Unit((dg.Width.Value + 5).ToString() + "px") ;
                if (dg.Height.Value > 0)
                {
                    ReqClassTab.Height = new Unit((dg.Height.Value + 32).ToString() + "px");
                }
                else
                {
                    ReqClassTab.Height = dg.Height;
                }
                */
                //strSql += " sys_requestclass_id=@strReqClass";
                parameter = new SqlParameter();
                parameter.ParameterName = "@strReqClass";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strReqClass;
                if (cmd.Parameters.Contains("@strReqClass"))
                {
                    cmd.Parameters.RemoveAt("@strReqClass");
                }
                cmd.Parameters.Add(parameter);



                CustomPager pger = LoadControl("~/CustomPager.ascx") as CustomPager;
                pger.OnPageChange += new CustomPager.PageChange(pger_Command_strReqClass);
                pger.OnPageSizeChange += new CustomPager.PageSizeChange(pgerr_Tab);

                pger.CurrentIndex = GetCurReqClassPageIndex(strReqClass);
                pger.PageSize = GetCurReqClassPageSize(strReqClass);
                pger.GenerateFirstLastSection = true;
                pger.GeneratePagerInfoSection = true;
                pger.GenerateGoToSection = true;

                SqlCommand cmdCount = cmd;
                cmdCount.CommandText = strRecordCount + " " + strSql + ") as InnerTable";

                int totalRecord = Convert.ToInt32(LayUtilDA.ExecuteCommandString_ExecuteScalar(cmdCount));
                pger.ItemCount = totalRecord;

                if (pger.CurrentIndex > pger.NumbersofPages)
                    pger.CurrentIndex = pger.NumbersofPages;

                int maxRowNumber;
                int minRowNumber = GetPageSize(pger, out maxRowNumber);

                pger.MinRowNumber = minRowNumber + 1;
                pger.MaxRowNumber = maxRowNumber - 1 < totalRecord ? maxRowNumber - 1 : totalRecord;

                cmd.CommandText = strSelectedRecord + strSql + ") as InnerTable Where RowIndex > " +
                              minRowNumber +
                              " and RowIndex < " + maxRowNumber;




                string strField = GetSortField(strReqClass);

                htReqDataCmdText[strReqClass] = cmd.CommandText;
                htReqDataCmd[strReqClass] = cmd.Clone();
                Infragistics.Web.UI.SortDirection dir = GetSortAsc(strReqClass);
                if (strField != "")
                {
                    string strOrderBy = "";
                    if (htLinks[strField] != null)
                    {
                        strOrderBy = "[request].";
                    }

                    if (dir == Infragistics.Web.UI.SortDirection.Ascending)
                    {
                        strOrderBy = strOrderBy + strField + " asc";
                    }
                    else
                    {
                        strOrderBy = strOrderBy + strField + " desc";
                    }

                    cmd.CommandText = cmd.CommandText.Replace("(ORDER BY sys_request_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
                }
                DataSet dsReq = LayUtilDA.GetDSCMD(cmd);

                SetColor(dsReq); 
                
                if (strField != "")
                {
                    if (dg.Columns.FromKey(strField) != null)
                    {
                        dg.Behaviors.Sorting.SortedColumns.Add(strField, dir);
                        string strOrder = dir.ToString();
                        string strSort = "";
                        if (strOrder == "Ascending")
                        {
                            strSort = "asc";
                        }
                        else if (strOrder == "Descending")
                        {
                            strSort = "desc";
                        }
                        if (dsReq.Tables[0].Rows.Count > 0)
                        {
                            dsReq.Tables[0].DefaultView.Sort = strField + " " + strSort;

                        }
                    }
                }
                dg.DataSource = dsReq.Tables[0].DefaultView;
                dg.Attributes["ReqClass"] = strReqClass;
                pger.ControlAttribute = strReqClass;
                dg.DataBind();
                // for bug  986
                dg.RequestFullAsyncRender();
                //}

                Panel pnGrid = new Panel();
                pnGrid.Controls.Add(dg);

                Panel pnPager = new Panel();
                pnPager.Controls.Add(pger);

                Panel pnOuter = new Panel();
                pnOuter.Controls.Add(pnGrid);
                pnOuter.Controls.Add(pnPager);
                dg.Behaviors.Paging.Enabled = false;

                tab.Controls.Add(pnOuter);

                tab.ImageAltText = dr["sys_requestclass_id"].ToString();

                ReqClassTab.Tabs.Add(tab);
                tab.BackColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));
                tab.BorderColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));
                tab.ScrollBars = ContentOverflow.Hidden;

                WebResizingExtender ext = new WebResizingExtender();
                ext.ID = "ext" + dr["sys_requestclass_desc"].ToString().Replace("&", "and");
                ext.TargetControlID = dg.ID;
                ext.OnClientResize = "myResizeFnc";
                ext.OnClientResizing = "myResizingFnc";
                tab.Controls.Add(ext);

            }

            tab = new ContentTabItem();
            tab.Text = "All";

            tab.AutoSize = DefaultableBoolean.True;
            dg = GenDG(child, true);
            dg.ID = "dgAll";
            htDG[dg.ID] = dg;
            dg.Attributes["ReqClass"] = "All";

            if (dsUser.Tables[0].Rows[0]["sys_requestclassrestrict"].ToString() == "1")
            {
                if (bWhere)
                {
                    strSqlPre += " AND ";
                }
                else
                {
                    strSqlPre += " WHERE ";
                }

                strSqlPre += "sys_requestclass_id IN (SELECT sys_requestclass_id FROM userrequestclass WHERE sys_username=@strUser)";
            }


            CustomPager pgerr = LoadControl("~/CustomPager.ascx") as CustomPager;
            pgerr.OnPageChange += new CustomPager.PageChange(pger_Command_All);
            pgerr.OnPageSizeChange += new CustomPager.PageSizeChange(pgerr_All);
            pgerr.ControlAttribute = "All";
            pgerr.CurrentIndex = GetCurReqClassPageIndex("All");
            pgerr.PageSize = GetCurReqClassPageSize("All");
            pgerr.GenerateFirstLastSection = true;
            pgerr.GeneratePagerInfoSection = true;
            pgerr.GenerateGoToSection = true;




            SqlCommand cmdCountRecord = cmd;
            cmdCountRecord.CommandText = strRecordCount + " " + strSqlPre + ") as InnerTable";

            int totalRecords = Convert.ToInt32(LayUtilDA.ExecuteCommandString_ExecuteScalar(cmdCountRecord));
            pgerr.ItemCount = totalRecords;

            if (pgerr.CurrentIndex > pgerr.NumbersofPages)
                pgerr.CurrentIndex = pgerr.NumbersofPages;

            int maxRowNumber_All;
            int minRowNumber_All = GetPageSize(pgerr, out maxRowNumber_All);


            pgerr.MinRowNumber = minRowNumber_All + 1;
            pgerr.MaxRowNumber = maxRowNumber_All - 1 < totalRecords ? maxRowNumber_All - 1 : totalRecords;

            cmd.CommandText = strSelectedRecord + strSqlPre + ") as InnerTable Where RowIndex > " +
                              minRowNumber_All +
                              " and RowIndex < " + maxRowNumber_All;


            string strAllField = GetSortField("All");

            Infragistics.Web.UI.SortDirection dirAll = GetSortAsc("All");
            htReqDataCmdText["All"] = cmd.CommandText;
            htReqDataCmd["All"] = cmd.Clone();
            if (strAllField != "")
            {
                string strOrderBy = "";
                if (htLinks[strAllField] != null)
                {
                    strOrderBy = "[request].";
                }

                if (dirAll == Infragistics.Web.UI.SortDirection.Ascending)
                {
                    strOrderBy = strOrderBy + strAllField + " asc";
                }
                else
                {
                    strOrderBy = strOrderBy + strAllField + " desc";
                }

                cmd.CommandText = cmd.CommandText.Replace("(ORDER BY sys_request_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
            }
            DataSet dsAll = LayUtilDA.GetDSCMD(cmd);
            SetColor(dsAll); 
            
            if (strAllField != "")
            {
                if (dg.Columns.FromKey(strAllField) != null)
                {
                    dg.Behaviors.Sorting.SortedColumns.Add(strAllField, dirAll);
                    string strOrder = dirAll.ToString();
                    string strSort = "";
                    if (strOrder == "Ascending")
                    {
                        strSort = "asc";
                    }
                    else if (strOrder == "Descending")
                    {
                        strSort = "desc";
                    }
                    if (dsAll.Tables[0].Rows.Count > 0)
                    {
                        dsAll.Tables[0].DefaultView.Sort = strAllField + " " + strSort;

                    }
                }
            }
            dg.DataSource = dsAll.Tables[0].DefaultView;
            dg.DataBind();
            // for bug  986
            dg.RequestFullAsyncRender();

            //}
            dg.Behaviors.Paging.Enabled = false;

            Panel pnlGrid = new Panel();
            pnlGrid.Controls.Add(dg);

            Panel pnlPager = new Panel();
            pnlPager.Controls.Add(pgerr);

            Panel pnlOuter = new Panel();

            pnlOuter.Controls.Add(pnlGrid);
            pnlOuter.Controls.Add(pnlPager);

            tab.Controls.Add(pnlOuter);

            ReqClassTab.Tabs.Add(tab);
            tab.BackColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));
            tab.ScrollBars = ContentOverflow.Hidden;


            ReqClassTab.ClientEvents.SelectedIndexChanged = "ReqClassTab_AfterSelectedTabChange";



            WebTab ultTab = (WebTab)ctrlRoot.FindControl("ReqClassTabs");
            if (ultTab != null)
            {
                ctrlRoot.Controls.Remove(ultTab);
            }
            ctrlRoot.Controls.Add(ReqClassTab);

            int nReqTabIndex = GetCurReqClassIndex();
            if (nReqTabIndex < ReqClassTab.Tabs.Count)
            {
                ReqClassTab.SelectedIndex = nReqTabIndex;
            }
            if (bTalAllSel)
            {
                ReqClassTab.SelectedIndex = ReqClassTab.Tabs.Count - 1;
            }

            WebResizingExtender extAll = new WebResizingExtender();
            extAll.ID = "extAll";
            extAll.TargetControlID = dg.ID;
            extAll.OnClientResize = "myResizeFnc";
            extAll.OnClientResizing = "myResizingFnc";
            tab.Controls.Add(extAll);

            textTabClientIDH.Text = ReqClassTab.ClientID;

        }
        else
        {
            WebTab ReqClassTab = new WebTab();
            ReqClassTab.Tabs.Clear();
            ReqClassTab.EnableOverlappingTabs = true;
            ReqClassTab.BorderStyle = BorderStyle.None;
            ReqClassTab.VisibleHeader = false;

            ReqClassTab.ID = "SingleTabs";

            ReqClassTab.BorderWidth = new Unit("0px");
            ReqClassTab.BorderColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));

            ReqClassTab.Style["Top"] = child.GetAttribute("top") + "px";
            ReqClassTab.Style["Left"] = child.GetAttribute("left") + "px";
            ReqClassTab.Style["Position"] = "absolute";

            ContentTabItem tab;

            WebDataGrid dg;
            tab = new ContentTabItem();
            tab.BorderStyle = BorderStyle.None;
            //tab.Text = "All";

            tab.AutoSize = DefaultableBoolean.True;
            dg = GenDG(child, true);

            dg.Attributes["ReqClass"] = "Single";
            ReqClassTab.Width = dg.Width;



            //Panel pnlOuterSingle = ctrlRoot.FindControl("pnlOuterSingle") as Panel;
            //if (pnlOuterSingle == null)
            //{
            //    pnlOuterSingle = new Panel();
            //    pnlOuterSingle.ID = "pnlOuterSingle";
            //}

            //Panel pnGridSingle = pnlOuterSingle.FindControl("pnGridSingle") as Panel;
            //if (pnGridSingle == null)
            //{
            //    pnGridSingle = new Panel();
            //    pnGridSingle.ID = "pnGridSingle";
            //}
            //// pnGridSingle.Controls.Clear();

            //Panel pnlPagerSingle = pnlOuterSingle.FindControl("pnlPagerSingle") as Panel;
            //if (pnlPagerSingle == null)
            //{
            //    pnlPagerSingle = new Panel();
            //    pnlPagerSingle.ID = "pnlPagerSingle";
            //}

            //WebDataGrid dg = pnGridSingle.FindControl("dgSingle") as WebDataGrid;
            //if (dg == null)
            //{
            //    dg=GenDG(child, false);
            //    dg.ID = "dgSingle";// +Guid.NewGuid();
            //    dg.Attributes["ReqClass"] = "Single";
            //    htDG[dg.ID] = dg;
            //    dg.Behaviors.Paging.Enabled = false;

            //}


            if (dsUser.Tables[0].Rows[0]["sys_requestclassrestrict"].ToString() == "1")
            {
                if (bWhere)
                {
                    strSqlPre += " AND ";
                }
                else
                {
                    strSqlPre += " WHERE ";
                }

                strSqlPre +=
                    "sys_requestclass_id IN (SELECT sys_requestclass_id FROM userrequestclass WHERE sys_username=@strUser)";
            }

            //CustomPager pger = pnlPagerSingle.FindControl("pgerSingle") as CustomPager;
            ////PagerV2_8 pger;
            //if (pger==null)
            //{
            //    pger = LoadControl("~/CustomPager.ascx") as CustomPager;
            //    pger.ID = "pgerSingle";
            //    pger.OnPageChange += new CustomPager.PageChange(pger_Command_Single);
            //    pger.OnPageSizeChange += new CustomPager.PageSizeChange(pger_Single);
            //    pger.GenerateFirstLastSection = true;
            //    pger.GeneratePagerInfoSection = true;
            //    pger.GenerateGoToSection = true;
            //    pger.ControlAttribute = "Single";
            //}
            //pger.CurrentIndex = GetCurReqClassPageIndex("Single");
            //pger.PageSize = GetCurReqClassPageSize("Single");

            CustomPager pger = LoadControl("~/CustomPager.ascx") as CustomPager;
            pger.OnPageChange += new CustomPager.PageChange(pger_Command_All);
            pger.OnPageSizeChange += new CustomPager.PageSizeChange(pgerr_All);
            pger.ControlAttribute = "Single";
            pger.CurrentIndex = GetCurReqClassPageIndex("Single"); ;
            pger.PageSize = GetCurReqClassPageSize("Single");
            pger.GenerateFirstLastSection = true;
            pger.GeneratePagerInfoSection = true;
            pger.GenerateGoToSection = true;




            SqlCommand cmdCountRecords = cmd;
            cmdCountRecords.CommandText = strRecordCount + " " + strSqlPre + ") as InnerTable";

            int totalRecords = Convert.ToInt32(LayUtilDA.ExecuteCommandString_ExecuteScalar(cmdCountRecords));
            pger.ItemCount = totalRecords;

            if (pger.CurrentIndex > pger.NumbersofPages)
                pger.CurrentIndex = pger.NumbersofPages;

            int maxRowNumber_All;
            int minRowNumber_All = GetPageSize(pger, out maxRowNumber_All);

            //pger.RequestClass = "Requests";
            pger.MinRowNumber = minRowNumber_All + 1;
            pger.MaxRowNumber = maxRowNumber_All - 1 < totalRecords ? maxRowNumber_All - 1 : totalRecords;

            cmd.CommandText = strSelectedRecord + strSqlPre + ") as InnerTable Where RowIndex > " +
                              minRowNumber_All +
                              " and RowIndex < " + maxRowNumber_All;



            //cmd.CommandText = strSqlPre + ") as InnerTable Where RowIndex Between 0 and 10"; ;

            string strSingleField = GetSortField("Single");

            Infragistics.Web.UI.SortDirection dirSingle = GetSortAsc("Single");
            htReqDataCmdText["Single"] = cmd.CommandText;
            htReqDataCmd["Single"] = cmd.Clone();
            if (strSingleField != "")
            {
                string strOrderBy = "";
                if (htLinks[strSingleField] != null)
                {
                    strOrderBy = "[request].";
                }

                if (dirSingle == Infragistics.Web.UI.SortDirection.Ascending)
                {
                    strOrderBy = strOrderBy + strSingleField + " asc";
                }
                else
                {
                    strOrderBy = strOrderBy + strSingleField + " desc";
                }

                cmd.CommandText = cmd.CommandText.Replace("(ORDER BY sys_request_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
            }
           
            DataSet dsReq = LayUtilDA.GetDSCMD(cmd);
            SetColor(dsReq);

            if (strSingleField != "")
            {
                if (dg.Columns.FromKey(strSingleField) != null)
                {
                    dg.Behaviors.Sorting.SortedColumns.Add(strSingleField, dirSingle);
                    string strOrder = dirSingle.ToString();
                    string strSort = "";
                    if (strOrder == "Ascending")
                    {
                        strSort = "asc";
                    }
                    else if (strOrder == "Descending")
                    {
                        strSort = "desc";
                    }
                    // Changed by Sparsh ID 194
                    if ((dsReq.Tables.Count > 0) && (dsReq.Tables[0].Rows.Count > 0))
                    {
                        dsReq.Tables[0].DefaultView.Sort = strSingleField + " " + strSort;
                        dg.DataSource = dsReq.Tables[0];
                        Session["dstbl"] = dsReq.Tables[0];
                        dg.DataBind();
                    }
                    else
                    {
                        dg.DataSource =(DataTable)Session["dstbl"];
                        dg.DataBind();
                    }
                }
            }
            // Changed by Sparsh ID194

            ////if (dsReq.Tables.Count > 0) // Sparsh ID 194
            ////{ // Sparsh ID 194
            ////    dg.DataSource = dsReq.Tables[0].DefaultView;
            
            ////dg.DataBind();
            ////// for bug  986
            ////dg.RequestFullAsyncRender();
            ////} // Sparsh ID 194

            //}

            //dg.Behaviors.Paging.PageSize = GetCurReqClassPageSize("Single");
            //int nAllPage = GetCurReqClassPageIndex("Single");
            //if (dg.Behaviors.Paging.PageCount > nAllPage)
            //{
            //    dg.Behaviors.Paging.PageIndex = nAllPage;
            //}
            //else
            //{
            //    dg.Behaviors.Paging.PageIndex = dg.Behaviors.Paging.PageCount - 1;
            //}



            //dg.Behaviors.Paging.PagerTemplate = new MyPagerTemplate(dg, Page, dg.Behaviors.Paging.PageIndex, dg.Behaviors.Paging.PageSize);
            //dg.EnableAjax = false;


            // pnlPagerSingle.Controls.Clear();


            // pnlOuterSingle.Controls.Clear();
            //pnGridSingle.Controls.Add(dg);
            //pnlPagerSingle.Controls.Add(pger);
            //pnlOuterSingle.Controls.Add(pnGridSingle);
            //pnlOuterSingle.Controls.Add(pnlPagerSingle);

            //pnlOuterSingle.Style["Top"] = child.GetAttribute("top") + "px";
            //pnlOuterSingle.Style["Left"] = child.GetAttribute("left") + "px";
            //pnlOuterSingle.Style["Position"] = "absolute";

            //if (ctrlRoot.FindControl(pnlOuterSingle.ID) != null)
            //{
            //    ctrlRoot.Controls.Remove(pnlOuterSingle);
            //}

            dg.Behaviors.Paging.Enabled = false;
            Panel pnlGrid = new Panel();
            pnlGrid.Controls.Add(dg);

            Panel pnlPager = new Panel();
            pnlPager.Controls.Add(pger);

            Panel pnlOuter = new Panel();

            pnlOuter.Controls.Add(pnlGrid);
            pnlOuter.Controls.Add(pnlPager);

            tab.Controls.Add(pnlOuter);

            ReqClassTab.Tabs.Add(tab);
            tab.BackColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));
            tab.ScrollBars = ContentOverflow.Hidden;

            ///ctrlRoot.Controls.Add(pnlOuterSingle);
            ///
            WebTab ultTab = (WebTab)ctrlRoot.FindControl("SingleTabs");
            if (ultTab != null)
            {
                ctrlRoot.Controls.Remove(ultTab);
            }
            ctrlRoot.Controls.Add(ReqClassTab);


            //WebResizingExtender extAll = ctrlRoot.FindControl("extAll") as WebResizingExtender;
            //if (extAll == null)
            //{
            //    extAll = new WebResizingExtender();
            //    extAll.ID = "extAll";
            //    extAll.TargetControlID = dg.ID;
            //    extAll.OnClientResize = "myResizeFnc";
            //}
            //ctrlRoot.Controls.Add(extAll);

            WebResizingExtender extAll = new WebResizingExtender();
            extAll.ID = "extAll";
            extAll.TargetControlID = dg.ID;
            extAll.OnClientResize = "myResizeFnc";
            extAll.OnClientResizing = "myResizingFnc";
            tab.Controls.Add(extAll);
        }


    }

    void pgerr_Tab(int pageSize, string controlAttribute)
    {
        dgResult_PageSizeChanged(pageSize, controlAttribute);
        ReBind();
    }

    void pgerr_All(int pageSize, string controlAttribute)
    {
        dgResult_PageSizeChanged(pageSize, controlAttribute);
        ReBind();
    }

    void pger_Single(int pageSize, string controlAttribute)
    {
        dgResult_PageSizeChanged(pageSize, controlAttribute);
        ReBind();

    }

    private int GetPageSize(CustomPager pger, out int maxRowNumber)
    {
        int minRowNumber = (pger.CurrentIndex - 1) * pger.PageSize;
        minRowNumber = minRowNumber > 0 ? minRowNumber : 0;
        pger.CurrentIndex = pger.CurrentIndex > 0 ? pger.CurrentIndex : 1;
        maxRowNumber = (pger.CurrentIndex * pger.PageSize) + 1;
        return minRowNumber;
    }

    void pger_Command_strReqClass(object sender, CommandEventArgs e)
    {
        CustomPager pgr = sender as CustomPager;
        if (pgr != null)
        {
            dgResult_PageIndexChanged(pgr.ControlAttribute, Convert.ToInt32(e.CommandArgument));
            ReBind();
        }
    }

    void pger_Command_All(object sender, CommandEventArgs e)
    {
        CustomPager pgr = sender as CustomPager;
        if (pgr != null)
        {

            dgResult_PageIndexChanged(pgr.ControlAttribute, Convert.ToInt32(e.CommandArgument));
            ReBind();
        }
    }

    void pger_Command_Single(object sender, CommandEventArgs e)
    {
        CustomPager pgr = sender as CustomPager;
        if (pgr != null)
        {
            dgResult_PageIndexChanged(pgr.ControlAttribute, Convert.ToInt32(e.CommandArgument));
            ReBind();
        }
    }

    public void SetAutoUrl()
    {
        /*
        string strNewUrl = Request.RawUrl;
        int nIndex = strNewUrl.IndexOf("filter=");
        if (nIndex != -1)
        {
            string strPre = strNewUrl.Substring(0, nIndex);
            string strPos = strNewUrl.Substring(nIndex);
            strAutoUrl = strPre + "filter=" + strFilter;

            nIndex = strPos.IndexOf("&");
            if (nIndex != -1)
            {
                strAutoUrl += strPos.Substring(nIndex);
            }
        }
        else
        {
            if (strNewUrl.IndexOf("?") == -1)
            {
                strAutoUrl = strNewUrl + "?filter=" + strFilter;
            }
            else
            {
                strAutoUrl = strNewUrl + "&filter=" + strFilter;
            }
        }

        Response.Redirect(strAutoUrl);
         */
        Response.Redirect("UserRequest.aspx?filter=" + strFilter);
    }

    protected void Filter_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        WebDropDown dd = (WebDropDown)sender;
        strFilter = dd.SelectedValue;

        SetAutoUrl();

        ClearVwState();

        Session["ReqList"] = "UserRequest.aspx?back=true&filter=" + strFilter;
        Session["ButtonBack"] = Session["ReqList"];

        phCtrls.Controls.Clear();
        ClearBackSession();

        LoadCtrl(true);

    }

    private void LoadCtrl(bool bRes)
    {
        Control ctrlRoot = phCtrls;
        phCtrls.Controls.Clear();

        StringBuilder sbDDCss = new StringBuilder("<style type=\"text/css\">");

        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = "";
            if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "result")
            {

                GenDGControl(child, ctrlRoot);

            }
            else if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "label")
            {
                Label ctrl = new Label();
                ctrl.Text = LayUtil.RplTm(Application, child.InnerText);
                ctrl.ID = "lb" + child.Name;
                ctrl.EnableViewState = false;

                ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                ctrl.Style["Position"] = "absolute";
                ctrl.Font.Name = child.GetAttribute("font-family");
                ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                if (child.GetAttribute("font-bold") == "true")
                {
                    ctrl.Font.Bold = true;
                }

                ctrlRoot.Controls.Add(ctrl);

                if (child.GetAttribute("tabindex") == "1")
                {
                    ctrl.Focus();
                }
            }
            else if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "filter")
            {
                WebDropDown ctrl = ddFilter;
                if (!IsPostBack)
                {
                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    string strWidth = child.GetAttribute("width");
                    if (!LayUtil.IsNumeric(strWidth))
                    {
                        strWidth = LayUtil.FilterDefWidth;
                    }
                    ctrl.Width = new Unit(strWidth + "px");

                    ctrl.DropDownContainerHeight = new Unit("0px");
                    ctrl.DropDownContainerMaxHeight = new Unit("500px");

                    ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                    ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                    ctrl.Style["Position"] = "absolute";
                    string strZIndex = "20000";
                    string strTop = child.GetAttribute("top");
                    int nTop = 0;
                    if (int.TryParse(strTop, out nTop))
                    {
                        strZIndex = (20000 - nTop).ToString();
                    }
                    ctrl.Style["Z-Index"] = strZIndex;

                    ctrl.DropDownContainerMaxHeight = new Unit(LayUtil.DropDownMaxHeight);
                    ctrl.DropDownContainerHeight = new Unit("0px");

                    //ctrl.AutoPostBack = true;

                    ctrl.Font.Name = child.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));


                    sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));

                    if (ViewState["searchsql"].ToString() != "" && Session["ReqSrchCmd"] != null)
                    {
                        ctrl.SelectedItemIndex = 0;
                    }
                    else if (ViewState["dashborad"].ToString() != "")
                    {
                        ctrl.SelectedItemIndex = 0;
                    }
                    else if (ViewState["filter"].ToString() != "")
                    {
                        ctrl.SelectedItemIndex = 1;
                        ctrl.SelectedValue = ViewState["filter"].ToString();
                        strFilter = ctrl.SelectedValue;
                    }

                }
                else
                {
                    if (bRes)
                    {
                        ctrl.SelectedValue = strFilter;
                    }
                }

                if (child.GetAttribute("tabindex") == "1")
                {
                    ctrl.Focus();
                }
            }
            else if (child.Name.Length >= 13 && child.Name.Substring(0, 13).ToLower() == "readonlyfield")
            {
                Label ctrl = new Label();
                ctrl.Text = strVal;
                ctrl.ID = "lb" + child.Name;

                ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                ctrl.Style["Position"] = "absolute";
                ctrl.Font.Name = child.GetAttribute("font-family");
                ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                ctrlRoot.Controls.Add(ctrl);

                if (child.GetAttribute("tabindex") == "1")
                {
                    ctrl.Focus();
                }
            }
            else if (child.Name.Length >= 9 && child.Name.Substring(0, 9).ToLower() == "hyperlink")
            {
                HyperLink ctrl = new HyperLink();
                ctrl.Target = child.GetAttribute("target");
                ctrl.NavigateUrl = child.GetAttribute("url");

                if (child.GetAttribute("image") != "")
                {
                    ctrl.ImageUrl = child.GetAttribute("image");
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                }
                else
                {
                    ctrl.Text = LayUtil.RplTm(Application, child.InnerText);

                    ctrl.Font.Name = child.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                }
                ctrl.ID = "hl" + child.Name;

                ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                ctrl.Style["Position"] = "absolute";

                ctrlRoot.Controls.Add(ctrl);

                if (child.GetAttribute("tabindex") == "1")
                {
                    ctrl.Focus();
                }
            }
            else if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field")
            {
                if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "fieldc")
                {
                    WebDropDown ctrl = new WebDropDown();
                    ctrl.ID = "dd" + child.Name;

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    string strWidth = child.GetAttribute("width");
                    if (!LayUtil.IsNumeric(strWidth))
                    {
                        strWidth = LayUtil.DDDefWidth;
                    }
                    ctrl.Width = new Unit(strWidth + "px");
                    sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));

                    ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                    ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                    ctrl.Style["Position"] = "absolute";
                    string strZIndex = "20000";
                    string strTop = child.GetAttribute("top");
                    int nTop = 0;
                    if (int.TryParse(strTop, out nTop))
                    {
                        strZIndex = (20000 - nTop).ToString();
                    }
                    ctrl.Style["Z-Index"] = strZIndex;

                    ctrl.DropDownContainerMaxHeight = new Unit(LayUtil.DropDownMaxHeight);
                    ctrl.DropDownContainerHeight = new Unit("0px");

                    ctrl.Font.Name = child.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));


                    //Set list items
                    //SetDropDown(ctrl, child.GetAttribute("values"));

                    if (strVal != "")
                    {
                        ctrl.SelectedValue = strVal;
                    }
                    ctrlRoot.Controls.Add(ctrl);

                    if (child.GetAttribute("tabindex") == "1")
                    {
                        ctrl.Focus();
                    }
                }
                else
                {
                    string strFieldType = DataDesignDA.GetFieldType("request", child.InnerText);
                    if (strFieldType == "")
                        continue;

                    if (child.GetAttribute("htmleditor") == "true")
                    {
                        RadEditor ctrl = LayUtil.CreateHTMLEditor(Page, child);
                        ctrl.Content = strVal;

                        HtmlGenericControl div = new HtmlGenericControl("div");
                        div.Style["Top"] = child.GetAttribute("top") + "px";
                        div.Style["Left"] = child.GetAttribute("left") + "px";
                        div.Style["Position"] = "absolute";

                        div.Controls.Add(ctrl);
                        ctrlRoot.Controls.Add(div);

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (strFieldType == "DateTime")
                    {
                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DatePickerDefWidth;
                        }

                        WebDatePicker datePicker = new WebDatePicker();

                        datePicker.Width = new Unit(strWidth + "px");
                        datePicker.ID = "datepicker" + child.Name;
                        
                        datePicker.DisplayModeFormat = "g";
                        datePicker.EditModeFormat = "g";

                        datePicker.Font.Name = child.GetAttribute("font-family");
                        datePicker.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        datePicker.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        datePicker.Style["Top"] = child.GetAttribute("top") + "px";
                        datePicker.Style["Left"] = child.GetAttribute("left") + "px";
                        datePicker.Style["Position"] = "absolute";

                        datePicker.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;

                        //Get Value
                        DateTime dt;
                        if (strVal != "")
                        {
                            dt = DateTime.Parse(strVal);
                            datePicker.Value = dt;
                        }
                        else
                        {
                            if (child.GetAttribute("autopopulate") == "true")
                            {
                                dt = DateTime.Now;
                                datePicker.Value = dt;
                            }
                            else
                            {
                                datePicker.Nullable = true;
                                datePicker.Value = null;
                            }
                        }

                        ctrlRoot.Controls.Add(datePicker);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            datePicker.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            datePicker.Focus();
                        }

                    }
                    else
                    {
                        TextBox ctrl = new TextBox();
                        ctrl.ID = "text" + child.Name;

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";
                        ctrl.Font.Name = child.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.TextDefWidth;
                        }
                        ctrl.Width = new Unit(strWidth + "px");

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit((int.Parse(strHeight) * 16).ToString() + "px");
                        }

                        if (strFieldType == "Decimal")
                        {
                            if (LayUtil.IsNumeric(strVal))
                            {
                                strVal = string.Format("{0:F2}", float.Parse(strVal));
                            }
                        }

                        ctrl.Text = strVal;

                        ctrlRoot.Controls.Add(ctrl);

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                }
            }
            else if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "image")
            {
                HtmlImage ctrl = new HtmlImage();
                ctrl.Src = child.InnerText;
                ctrl.Border = 0;

                ctrl.Style["Cursor"] = "auto !important";

                ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                ctrl.Style["Position"] = "absolute";
                if (LayUtil.IsNumeric(child.GetAttribute("width")))
                {
                    ctrl.Width = int.Parse(child.GetAttribute("width"));
                }
                if (LayUtil.IsNumeric(child.GetAttribute("height")))
                {
                    ctrl.Height = int.Parse(child.GetAttribute("height"));
                }
                ctrlRoot.Controls.Add(ctrl);

                if (child.GetAttribute("tabindex") == "1")
                {
                    ctrl.Focus();
                }
            }
            else if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "sys_b")
            {
                if (LayUtil.GetAttribute(child, "hide") == "true")
                    continue;

                if (child.Name == "sys_button1")
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                    ctrl.ImageUrl = child.GetAttribute("image");
                    //ctrl.Click += new ImageClickEventHandler(sys_button1_Click);
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
                    ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                    ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                    ctrl.Style["Position"] = "absolute";
                    ctrlRoot.Controls.Add(ctrl);

                    if (child.GetAttribute("tabindex") == "1")
                    {
                        ctrl.Focus();
                    }
                }
                else
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                    ctrl.ImageUrl = child.GetAttribute("image");
                    ctrl.PostBackUrl = "LibPriority.aspx";

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
                    ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                    ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                    ctrl.Style["Position"] = "absolute";
                    ctrlRoot.Controls.Add(ctrl);

                    if (child.GetAttribute("tabindex") == "1")
                    {
                        ctrl.Focus();
                    }
                }
            }
            else
            {
                if (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_")
                {
                    if (child.InnerText == "sys_autoresolve_assignto" ||
                        child.InnerText == "sys_autores_assignto" ||
                        child.InnerText == "sys_autoesc1_assignto" ||
                        child.InnerText == "sys_autoesc2_assignto" ||
                        child.InnerText == "sys_autoesc3_assignto")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        ctrl.Text = strVal;
                        ctrl.ID = "text" + child.Name;

                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.TextDefWidth;
                        }
                        ctrl.Width = new Unit(strWidth + "px");

                        ctrl.Font.Name = child.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        ctrl.Enabled = false;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectuser('" + ctrl.ClientID + "','sys_requesttype_id');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select " + Application["appuserterm"];

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        /*
                        <a href='javascript:selectuser("<%=textOwnBySpf.ClientID %>");'><img src="Application_Images/16x16/select_16px.png" border="0" title="Select"></a>                         */


                        //ctrlRoot.Controls.Add(ctrl);
                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_autoresolve_assigntogroup" ||
                        child.InnerText == "sys_autoesc1_assigntogroup" ||
                        child.InnerText == "sys_autoesc2_assigntogroup" ||
                        child.InnerText == "sys_autoesc3_assigntogroup" ||
                        child.InnerText == "sys_autores_assigntogroup")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        ctrl.Text = strVal;
                        ctrl.ID = "text" + child.Name;

                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.TextDefWidth;
                        }
                        ctrl.Width = new Unit(strWidth + "px");

                        ctrl.Font.Name = child.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        ctrl.Enabled = false;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectanalgroup('" + ctrl.ClientID + "','sys_requesttype_id');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select " + Application["appuserterm"] + " Group";

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_allowedit")
                    {
                        WebDropDown ctrl = new WebDropDown();
                        ctrl.ID = "dd" + child.Name;

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DDDefWidth;
                        }
                        ctrl.Width = new Unit(strWidth + "px");

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";
                        string strZIndex = "20000";
                        string strTop = child.GetAttribute("top");
                        int nTop = 0;
                        if (int.TryParse(strTop, out nTop))
                        {
                            strZIndex = (20000 - nTop).ToString();
                        }
                        ctrl.Style["Z-Index"] = strZIndex;

                        ctrl.DropDownContainerMaxHeight = new Unit(LayUtil.DropDownMaxHeight);
                        ctrl.DropDownContainerHeight = new Unit("0px");

                        ctrl.Font.Name = child.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        //Set list items
                        DropDownItem item;
                        item = new DropDownItem("No", "0");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("Yes", "1");
                        ctrl.Items.Add(item);
                        if (strVal == "")
                        {
                            strVal = "0";
                        }
                        ctrl.SelectedValue = strVal;

                        ctrlRoot.Controls.Add(ctrl);

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else
                    {
                        TextBox ctrl = new TextBox();
                        ctrl.Text = strVal;
                        ctrl.ID = "text" + child.Name;

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.TextDefWidth;
                        }
                        ctrl.Width = new Unit(strWidth + "px");

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";
                        ctrl.Font.Name = child.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        ctrlRoot.Controls.Add(ctrl);

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                }
            }


        }


        sbDDCss.AppendLine("</style>");

        StyleArea.InnerHtml = sbDDCss.ToString();

    }

    private void AddColumn(XmlElement node, WebDataGrid dgResult)
    {
        string strField = node.InnerText;
        string strNodeName = node.Name;
        TemplateDataField templateColumn1 = (TemplateDataField)dgResult.Columns[strField];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = strField;
            field1.Header.Text = LayUtil.RplTm(Application, LayUtil.GetAttribute(node, "heading"));
            string strAlign = LayUtil.GetAttribute(node, "align");
            string strHeaderAlign = LayUtil.GetAttribute(node, "headingalign");
            string strAlignCss = "Col_Center";
            if (strAlign == "left")
            {
                strAlignCss = "Col_Left";
            }
            else if (strAlign == "right")
            {
                strAlignCss = "Col_Right";
            }
            field1.CssClass = strAlignCss;

            string strHeaderAlignCss = "Col_Center";
            if (strHeaderAlign == "left")
            {
                strHeaderAlignCss = "Col_Left";
            }
            else if (strHeaderAlign == "right")
            {
                strHeaderAlignCss = "Col_Right";
            }
            field1.Header.CssClass = strHeaderAlignCss;

            dgResult.Columns.Add(field1);
        }
        if ((strNodeName == "commentlink") || (strNodeName == "actionlink") || (strNodeName == "attachlink") || (strNodeName == "opencloselink"))
        {
            Sorting behavior = dgResult.Behaviors.Sorting;
            SortingColumnSetting obj = new SortingColumnSetting();
            //ColumnSetting objC = new ColumnSetting(dgResult);
            obj.ColumnKey = strField;
            obj.Sortable = false;
            behavior.ColumnSettings.Add(obj);
        }
            
        templateColumn1 = (TemplateDataField)dgResult.Columns[strField];
        templateColumn1.ItemTemplate = new RequestCell(this, node);

        string strWidth = LayUtil.GetAttribute(node, "width");
        if (LayUtil.IsPercentage(strWidth))
        {
            templateColumn1.Width = new Unit(strWidth);
        }
        else if (LayUtil.IsNumeric(strWidth))
        {
            templateColumn1.Width = new Unit(strWidth + "px");
        }
        else
            templateColumn1.Width = new Unit("10%");

        //templateColumn1.Width = new Unit(LayUtil.GetAttribute(node,"width"));
        //templateColumn1.
    }

    private class RequestCell : ITemplate
    {
        #region ITemplate Members

        private Analyst_UserRequest ParentPage;
        XmlElement node;
        public RequestCell(Analyst_UserRequest pPage, XmlElement child)
        {
            ParentPage = pPage;
            node = child;

        }
        public void InstantiateIn(Control container)
        {
            /////////////////////////////////////////////////////////////////////////////
            //htReqColor
            GridRecordItem item = (GridRecordItem)(((TemplateContainer)container).Item);
            if (SysDA.GetSettingValue("appcolorrow", ParentPage.Application) == "true")
            {
                string strColor = ParentPage.GetReqColor(((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString());
                if (strColor != "")
                {
                    strColor = "background-color: " + strColor + " !important;";
                    item.CssClass = Infragistics.Web.UI.Framework.AppSettings.AppStylingManager.CssRegistry.Add(strColor, "tbody tr td.{0}");
                }
            }


            /////////////////////////////////////////////////////////////////////////////


            string strNodeName = node.Name;

            if (strNodeName.Length >= 5 && strNodeName.Substring(0, 5).ToLower() == "field")
            {
                if (node.InnerText == "sys_request_id")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.Style.Add("TEXT-DECORATION", "none");
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;

                    container.Controls.Add(ctrl);

                    string strSpawnCnt = ((DataRowView)((TemplateContainer)container).DataItem)["spawncount"].ToString();
                    string strLinkCnt = ((DataRowView)((TemplateContainer)container).DataItem)["linkcount"].ToString();
                    string strParentId = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestparent_id"].ToString();
                    if (LayUtil.IsNumeric(strSpawnCnt) && int.Parse(strSpawnCnt) > 0)
                    {
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString() + "','')";

                        hl.Text = "SP";

                        hl.Font.Name = node.GetAttribute("font-family");
                        hl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        hl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                        //ctrl.BackColor = color

                        hl.ID = "hl" + node.Name;

                        Literal lit = new Literal();
                        lit.Text = " ";
                        container.Controls.Add(lit);

                        container.Controls.Add(hl);
                    }
                    else if (LayUtil.IsNumeric(strLinkCnt) && int.Parse(strLinkCnt) > 0)
                    {
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString() + "','')";

                        hl.Text = "LNK";

                        hl.Font.Name = node.GetAttribute("font-family");
                        hl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        hl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                        //ctrl.BackColor = color

                        hl.ID = "hl" + node.Name;
                        hl.Style.Add("TEXT-DECORATION", "none");

                        Literal lit = new Literal();
                        lit.Text = " ";
                        container.Controls.Add(lit);

                        container.Controls.Add(hl);
                    }
                    else if (strParentId != "")
                    {
                        string strReqLinkType = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestlinktype"].ToString();
                        if (strReqLinkType == "1")
                        {

                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestparent_id"].ToString() + "','')";

                            hl.Text = "sp";

                            hl.Font.Name = node.GetAttribute("font-family");
                            hl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                            hl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                            //ctrl.BackColor = color

                            hl.ID = "hl" + node.Name;
                            hl.Style.Add("TEXT-DECORATION", "none");
                            Literal lit = new Literal();
                            lit.Text = " ";
                            container.Controls.Add(lit);
                            container.Controls.Add(hl);
                        }
                        else
                        {

                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestparent_id"].ToString() + "','')";

                            hl.Text = "lnk";

                            hl.Font.Name = node.GetAttribute("font-family");
                            hl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                            hl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                            //ctrl.BackColor = color

                            hl.ID = "hl" + node.Name;
                            hl.Style.Add("TEXT-DECORATION", "none");

                            Literal lit = new Literal();
                            lit.Text = " ";
                            container.Controls.Add(lit);
                            container.Controls.Add(hl);
                        }
                    }

                }
                else if (node.InnerText == "sys_requeststatus")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);

                    string strStatus = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requeststatus"].ToString();
                    if (LayUtil.IsNumeric(strStatus))
                    {
                        ctrl.Text = ((DataRowView)((TemplateContainer)container).DataItem)["status"].ToString();
                    }
                    else
                    {
                        ctrl.Text = "Closed";
                    }

                    if (SysDA.GetSettingValue("appcolorrow", ParentPage.Application) != "true")
                    {
                        string strColor = ParentPage.GetStatusColor(((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString());
                        if (strColor != "")
                        {
                            strColor = "background-color: " + strColor + " !important;";
                            item.CssClass = Infragistics.Web.UI.Framework.AppSettings.AppStylingManager.CssRegistry.Add(strColor, "tbody tr td.{0}");
                        }
                    }
                }
                else if (node.InnerText == "sys_urgency")
                {
                    string strUrgency = ((DataRowView)((TemplateContainer)container).DataItem)["UrgencyName"].ToString();
                    if (strUrgency != "")
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();
                        ctrl.Text = strUrgency;
                        ctrl.Font.Name = node.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));
                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }

                }
                else if (node.InnerText == "sys_impact")
                {
                    string strImpact = ((DataRowView)((TemplateContainer)container).DataItem)["ImpactName"].ToString();
                    if (strImpact != "")
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();
                        ctrl.Text = strImpact;
                        ctrl.Font.Name = node.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));
                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }
                }
                else if (node.InnerText == "sys_itservice")
                {
                    string strITService = ((DataRowView)((TemplateContainer)container).DataItem)["ITServiceName"].ToString();
                    if (strITService != "")
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();
                        ctrl.Text = strITService;
                        ctrl.Font.Name = node.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));
                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }
                }

                else if (node.InnerText == "sys_assignedto")
                {
                    string strAssignedTo = ((DataRowView)((TemplateContainer)container).DataItem)["sys_assignedto"].ToString();

                    if (strAssignedTo == "")
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ReqTake.aspx?option=assign&sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                        ctrl.ImageUrl = "Application_Images/16x16/Takeassign_16px.png";
                        ctrl.ToolTip = "Take Assignment";

                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }
                    else
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                        ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                        ctrl.Font.Name = node.GetAttribute("font-family");
                        ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                        //ctrl.BackColor = color

                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }
                }
                else if (node.InnerText == "sys_requestclass_id")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    ctrl.Text = ParentPage.GetReqClassDesc(((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_requesttype_id")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    if (SysDA.GetSettingValue("apptruncatereqtype", ParentPage.Application) == "true")
                    {
                        ctrl.Text = ParentPage.GetReqTypeShort(((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());
                    }
                    else
                    {
                        ctrl.Text = ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString();
                    }
                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_forename")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    string strDispTbl = node.GetAttribute("displaytable");
                    string strDispLink = node.GetAttribute("displaytablelink");
                    string strLink = node.GetAttribute("mastertablelink");

                    string strAsField = node.InnerText;
                    if (strDispTbl == "euser")
                    {
                        strAsField = "euser_forename";
                    }
                    else if (strDispTbl == "user")
                    {
                        if (strLink == "sys_assignedto")
                        {
                            strAsField = "assignedto_forename";
                        }
                        else if (strLink == "sys_ownedby")
                        {
                            strAsField = "ownedby_forename";
                        }
                        else if (strLink == "sys_respondedby")
                        {
                            strAsField = "respondedby_forename";
                        }
                    }

                    ctrl.Text = ((DataRowView)((TemplateContainer)container).DataItem)[strAsField].ToString();
                    
                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_surname")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    string strDispTbl = node.GetAttribute("displaytable");
                    string strDispLink = node.GetAttribute("displaytablelink");
                    string strLink = node.GetAttribute("mastertablelink");

                    string strAsField = node.InnerText;
                    if (strDispTbl == "euser")
                    {
                        strAsField = "euser_surname";
                    }
                    else if (strDispTbl == "user")
                    {
                        if (strLink == "sys_assignedto")
                        {
                            strAsField = "assignedto_surname";
                        }
                        else if (strLink == "sys_ownedby")
                        {
                            strAsField = "ownedby_surname";
                        }
                        else if (strLink == "sys_respondedby")
                        {
                            strAsField = "respondedby_surname";
                        }
                    }

                    ctrl.Text = ((DataRowView)((TemplateContainer)container).DataItem)[strAsField].ToString();

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ReqInfo.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                    ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    //ctrl.BackColor = color
                    if (node.InnerText == "sys_requestpriority")
                    {
                        if (SysDA.GetSettingValue("appcolorrow", ParentPage.Application) != "true")
                        {
                            string strColor = ParentPage.GetPriorityColor(((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString());
                            if (strColor != "")
                            {
                                strColor = "background-color: " + strColor + " !important;";
                                item.CssClass = Infragistics.Web.UI.Framework.AppSettings.AppStylingManager.CssRegistry.Add(strColor, "tbody tr td.{0}");
                            }
                        }
                    }
                    ctrl.ID = "hl" + node.Name;

                    container.Controls.Add(ctrl);
                }
            }
            else if (strNodeName == "actionlink")
            {
                string linkImg;
                string strActCnt = ((DataRowView)((TemplateContainer)container).DataItem)["actioncount"].ToString();
                if (strActCnt == "" || strActCnt == "0")
                {
                    linkImg = LayUtil.GetAttribute(node, "linkimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/task_16px.png";
                    }
                }
                else
                {
                    linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/task_hasitems.png";
                    }
                }

                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "UserTask.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                ctrl.ImageUrl = linkImg;
                ctrl.ToolTip = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strActCnt + " Items)";

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);

            }
            else if (strNodeName == "commentlink")
            {
                string linkImg;
                string strAlt;

                string strCnt = ((DataRowView)((TemplateContainer)container).DataItem)["commentcount"].ToString();
                string strReadCnt = ((DataRowView)((TemplateContainer)container).DataItem)["readcommentcount"].ToString();

                int nCnt = 0;
                if (LayUtil.IsNumeric(strCnt))
                {
                    nCnt = int.Parse(strCnt);
                }
                int nReadCnt = 0;
                if (LayUtil.IsNumeric(strReadCnt))
                {
                    nReadCnt = int.Parse(strReadCnt);
                }

                if (nCnt == 0)
                {
                    linkImg = LayUtil.GetAttribute(node, "linkimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/Comment_icon_16px.png";
                    }
                    strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items)";
                }
                else
                {
                    if (nCnt > 0 && nCnt > nReadCnt)
                    {
                        linkImg = LayUtil.GetAttribute(node, "linkhasunreaditemsimg");

                        if (linkImg == "")
                        {
                            linkImg = "Application_Images/Commentshasunreaditem16px.png";
                        }
                        strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items, " + (nCnt - nReadCnt).ToString() + " Un-Read)";
                    }
                    else
                    {
                        linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                        if (linkImg == "")
                        {
                            linkImg = "Application_Images/Commentshasreaditem16px.png";
                        }
                        strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items)";
                    }
                }


                HyperLink ctrl = new HyperLink();
                //ctrl.NavigateUrl = "ReqComment.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();
                string strParent = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requestparent_id"].ToString();
                ctrl.NavigateUrl = "ReqComment.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString() + "&sys_requestparent_id=" + strParent;

                ctrl.ImageUrl = linkImg;
                ctrl.ToolTip = strAlt;

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);

            }
            else if (strNodeName == "attachlink")
            {
                string strReqId = ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();
                int nAttchCnt = RequestDA.GetReqAttchCnt(strReqId);

                string linkImg = LayUtil.GetAttribute(node, "image");
                if (nAttchCnt > 0)
                {
                    linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/16x16/attachhasitems_icon_16px.png";
                    }
                }
                else
                {
                    linkImg = LayUtil.GetAttribute(node, "linkimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/16x16/attach_icon_16px.png";
                    }
                }

                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "ReqAttach.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString();

                ctrl.ImageUrl = linkImg;
                ctrl.ToolTip = "Attachments (" + nAttchCnt + " Items)";

                ctrl.ID = "hl" + node.Name;

                ctrl.Style.Add("TEXT-DECORATION", "none");
                container.Controls.Add(ctrl);
            }
            else if (strNodeName == "opencloselink")
            {
                string strStatus = ((DataRowView)((TemplateContainer)container).DataItem)["sys_requeststatus"].ToString();

                string strImg = "";
                string strAlt = "";

                if (strStatus == "" || ParentPage.htOpenStatus[strStatus] == null)
                {
                    //strImg = "images/close.gif";
                    strImg = "Application_Images/16x16/Request_Close_16px.png";
                    strAlt = "Status: Closed - Click to change status";
                }
                else
                {
                    //strImg = "images/open.gif";
                    strImg = "Application_Images/16x16/Request_Open.png";
                    strAlt = "Status: Open - Click to change status";
                }

                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "ReqChgStatus.aspx?sys_request_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_request_id"].ToString() + "&openclose=" + strStatus;

                ctrl.ImageUrl = strImg;
                ctrl.ToolTip = strAlt;

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);
            }
         


        }


        #endregion
    }

    /// <summary>
    /// Get Request class description
    /// </summary>
    /// <param name="strReqClassId"></param>
    /// <returns></returns>
    private string GetReqClassDesc(string strReqClassId)
    {
        if (htReqClass == null)
        {
            htReqClass = new Hashtable();

            DataSet dsReqClass = LibReqClassBR.GetReqClassList();
            if (dsReqClass != null && dsReqClass.Tables.Count > 0)
            {
                foreach (DataRow dr in dsReqClass.Tables[0].Rows)
                {
                    htReqClass[dr["sys_requestclass_id"].ToString()] = dr["sys_requestclass_desc"].ToString();
                }
            }
        }

        if (htReqClass[strReqClassId] != null)
        {
            return htReqClass[strReqClassId].ToString();
        }
        else
        {
            return strReqClassId;
        }
    }

    /// <summary>
    /// Get short version of Request Type
    /// </summary>
    /// <param name="strReqType"></param>
    /// <returns></returns>
    private string GetReqTypeShort(string strReqType)
    {
        if (htReqType == null)
        {
            htReqType = new Hashtable();

            DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo("");
            if (dsReqType != null && dsReqType.Tables.Count > 0)
            {
                foreach (DataRow dr in dsReqType.Tables[0].Rows)
                {
                    htReqType[dr["sys_requesttype_id"].ToString()] = dr["sys_requesttypeparent_id"].ToString();
                }
            }
        }

        if (htReqType[strReqType] != null && htReqType[strReqType].ToString() != "")
        {
            string strParent = htReqType[strReqType].ToString();

            return strReqType.Substring(strParent.Length + 1);
        }
        else
        {
            return strReqType;
        }
    }

    private DataSet GetListDataSet()
    {

        return null;
    }

    private string GetReqColor(string strReqId)
    {
        if (htReqColor[strReqId] != null)
        {
            return htReqColor[strReqId].ToString();
        }


        return "";
    }

    private string GetPriorityColor(string strReqId)
    {
        if (htPriorityColor[strReqId] != null)
        {
            return htPriorityColor[strReqId].ToString();
        }


        return "";
    }

    private string GetStatusColor(string strReqId)
    {
        if (htStatusColor[strReqId] != null)
        {
            return htStatusColor[strReqId].ToString();
        }


        return "";
    }

    protected void changePage(object sender, EventArgs e)
    {
    }

    static object customepagesize = 0;
    static string reqst = "";

    void ReBind()
    {
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = "";
            Control ctrlRoot = phCtrls;
            if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "result")
            {
                GenDGControl(child, ctrlRoot);

            }
        }

    }


}
