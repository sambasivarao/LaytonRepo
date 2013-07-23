using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;
using System.Text;
using System.Web.UI.HtmlControls;

using Infragistics.WebUI;
using Infragistics.Web.UI.LayoutControls;
using Infragistics.Web.UI;
using Infragistics.Web.UI.GridControls;
using Infragistics.Web.UI.ListControls;
using Infragistics.WebUI.WebDataInput;
using Infragistics.WebUI.WebSchedule;
using Infragistics.WebUI.UltraWebTab;

using System.Collections;
using System.Data;
using System.Data.SqlClient;

using Telerik.Web.UI;

using Infragistics.Web.UI.EditorControls;

public partial class UserChange : System.Web.UI.Page
{
    public XmlDocument xmlForm;
    public XmlNode dgNode = null;

    //WebDataGrid dgResult;

    private Hashtable htOpenStatus = null;
    private Hashtable htChgColor = null;

    private Hashtable htPriorityColor = null;
    private Hashtable htStatusColor = null;

    private Hashtable htImpact = null;
    private Hashtable htUrgency = null;
    private Hashtable htChgType = null;

    //private WebDropDown ddFilter = null;
    private string strFilter = "";
    private int nPageSize = LayUtil.ResSetDefPageSize;

    public string AutoRefresh = "";
    public string strAutoUrl = "";

    private string strDataCmdText = "";
    private SqlCommand cmdData = null;

    private void Page_Init()
    {
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
        {
            Response.End();
            return;
        }

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
            ViewState["approval"] = LayUtil.GetQueryString(Request, "approval");

            SetPageBack();

            WebDropDown ctrl = ddFilter;
            DropDownItem item;
            item = new DropDownItem("", "");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Assigned " + "Change" + "s(Open)", "myassignedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + "Change" + "s", "unassigned");
            ctrl.Items.Add(item);
            item = new DropDownItem("My Assigned " + "Change" + "s(All)", "myassignedall");
            ctrl.Items.Add(item);

            item = new DropDownItem("Assigned " + "Change" + "s In My Group(Open)", "mygroupassginedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + "Change" + "s In My Group(Open)", "mygroupunassginedopen");
            ctrl.Items.Add(item);
            item = new DropDownItem("Assigned " + "Change" + "s In My Group(All)", "mygroupassginedall");
            ctrl.Items.Add(item);
            item = new DropDownItem("Unassigned " + "Change" + "s In My Group(All)", "mygroupunassginedall");
            ctrl.Items.Add(item);
            /*
            item = new DropDownItem("Problems Oustside SLA", "outsla");
            ctrl.Items.Add(item);
            item = new DropDownItem("Problems at Level 1", "escl1");
            ctrl.Items.Add(item);
            item = new DropDownItem("Problems at Level 2", "escl2");
            ctrl.Items.Add(item);
            item = new DropDownItem("Problems at Level 3", "escl3");
            ctrl.Items.Add(item);
            */
            item = new DropDownItem("All Open Changes", "allopen");
            ctrl.Items.Add(item);
             
            item = new DropDownItem("All Changes", "all");
            ctrl.Items.Add(item);

            ctrl.DisplayMode = DropDownDisplayMode.DropDownList;
        }

        MyInit();
        //SetAutoUrl();
    }

    private void MyInit()
    {
        xmlForm = FormXMLUtil.LoadResultSetXML("sys_change_resultset_xml", Session["User"].ToString());

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
            DataSet dsOpenStatus = ChangeDA.GetOpenStatus();
            if (dsOpenStatus != null && dsOpenStatus.Tables.Count > 0)
            {
                foreach (DataRow dr in dsOpenStatus.Tables[0].Rows)
                {
                    htOpenStatus.Add(dr["sys_change_status_id"].ToString(), "true");
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
        Session["ChangeList"] = "UserChange.aspx?" + strURL;
        Session["ButtonBack"] = Session["ChangeList"];
    }
    private string GetSortField()
    {
        string strField = "";
        if (Session["ChangeSortField"] != null)
        {
            strField = Session["ChangeSortField"].ToString();
        }
        return strField;
    }
    private Infragistics.Web.UI.SortDirection GetSortAsc()
    {
        Infragistics.Web.UI.SortDirection asc = Infragistics.Web.UI.SortDirection.Ascending;
        if (Session["ChangeSortAsc"] != null)
        {
            asc = (Infragistics.Web.UI.SortDirection)Session["ChangeSortAsc"];
        }

        return asc;
    }
    private int GetCurPageIndex()
    {
        int nPageIndex = 0;
        if (Session["ChangeListPage"] != null)
        {
            string strIndex = Session["ChangeListPage"].ToString();
            if (LayUtil.IsNumeric(strIndex))
            {
                nPageIndex = int.Parse(strIndex);
            }
        }

        return nPageIndex;
    }
    private int GetCurPageSize()
    {
        int nChgPageSize = nPageSize;
        if (Session["ChangeListPageSize"] != null)
        {
            string strPageSize = Session["ChangeListPageSize"].ToString();
            if (LayUtil.IsNumeric(strPageSize))
            {
                nChgPageSize = int.Parse(strPageSize);
            }
        }
        return nChgPageSize;
    }

    private void ClearBackSession()
    {
        Session["ChangeSortField"] = null;
        Session["ChangeListPage"] = null;
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
            FormDesignerDA.SaveUserRessetForm("change", Session["User"].ToString(), xmlForm.InnerXml);
        }
    }

    protected void dgResult_ColumnResized(object sender, ColumnResizingEventArgs e)
    {
        ;
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
    }

    protected void dgResult_ColSorted(object sender, Infragistics.Web.UI.GridControls.SortingEventArgs e)
    {
        WebDataGrid dg = (WebDataGrid)sender;

        //Sort columns
        Session["ChangeSortField"] = e.SortedColumns[0].Column.Key;
        Session["ChangeSortAsc"] = e.SortedColumns[0].SortDirection;

        UpdateDGData(dg);

    }

    private void UpdateDGData(WebDataGrid dg)
    {
        SqlCommand cmd = cmdData;
        string strCmdText = strDataCmdText;
        string strField = GetSortField();

        Infragistics.Web.UI.SortDirection dir = GetSortAsc();
        if (strField != "")
        {
            string strOrderBy = "";
            if (dir == Infragistics.Web.UI.SortDirection.Ascending)
            {
                strOrderBy = strField + " asc";
            }
            else
            {
                strOrderBy = strField + " desc";
            }
            cmd.CommandText = strCmdText.Replace("(ORDER BY sys_change_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
        }
        DataSet dsChange = LayUtilDA.GetDSCMD(cmd);

        SetColor(dsChange);

        if (strField != "")
        {
            if (dg.Columns.FromKey(strField) != null)
            {
                dg.Behaviors.Sorting.SortedColumns.Add(strField, GetSortAsc());
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
                if (dsChange.Tables[0].Rows.Count > 0)
                {
                    dsChange.Tables[0].DefaultView.Sort = strField + " " + strSort;

                }
            }
        }
        dg.DataSource = dsChange.Tables[0].DefaultView;
        dg.DataBind();
        // for bug  986
        dg.RequestFullAsyncRender();
        //}

    }
    protected void dgResult_PageIndexChanged(object sender)
    {
        WebDataGrid dg = (WebDataGrid)sender;

        Session["ChangeListPage"] = dg.Behaviors.Paging.PageIndex;
    }

    public void dgResult_PageSizeChanged(object sender)
    {
        WebDataGrid dg = (WebDataGrid)sender;
        Session["ChangeListPageSize"] = dg.Behaviors.Paging.PageSize;
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
        //dgResult.Behaviors.CreateBehavior<Filtering>();

        // for bug  986
        dgResult.Behaviors.CreateBehavior<EditingCore>();
        dgResult.Behaviors.EditingCore.AutoCRUD = false;
        
        dgResult.Behaviors.ColumnMoving.ColumnMovingClientEvents.HeaderDropped = "dgResult_ColDropped";
        dgResult.Behaviors.ColumnResizing.ColumnResizingClientEvents.ColumnResized = "dgResult_ColResized";
        dgResult.Behaviors.Sorting.ColumnSorted += new ColumnSortedHandler(dgResult_ColSorted);
        //dgResult.Behaviors.Paging.PageIndexChanged += new PageIndexChangedHandler(dgResult_PageIndexChanged);
        dgResult.Behaviors.ColumnResizing.AutoPostBackFlags.ColumnResized = true;

        dgResult.Behaviors.Paging.PageSize = nPageSize;
        dgResult.EmptyRowsTemplate = new MyEmptyRowsTemplate("No Changes");


        dgResult.Font.Name = Application["appfont"].ToString();
        dgResult.Font.Size = new FontUnit(Application["appdeffontpx"].ToString() + "px");
        dgResult.ForeColor = LayUtil.GetColorFrmStr(Application["appdeffontcolor"].ToString());

        string strWidth = child.GetAttribute("width");
        if (LayUtil.IsNumeric(strWidth))
        {
            dgResult.Width = new Unit(strWidth + "px");
        }
        string strHeight = child.GetAttribute("height");
        if (LayUtil.IsNumeric(strHeight))
        {
            dgResult.Height= new Unit(strHeight + "px");
        }

        //Setting Columns
        foreach (XmlElement node in child.ChildNodes)
        {
            AddColumn(node, dgResult);
        }

        //if (!bInTab)
        //{
        //    dgResult.Style["Top"] = child.GetAttribute("top") + "px";
        //    dgResult.Style["Left"] = child.GetAttribute("left") + "px";
        //    dgResult.Style["Position"] = "absolute";
        //}

        dgResult.ID = "dgResult";
       // extSize.TargetControlID = "dgResult";
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

        string strSqlField = "(SELECT Top 100 percent *, ROW_NUMBER() OVER (ORDER BY sys_change_id desc) as RowIndex, (SELECT changestatus.sys_change_status FROM [changestatus] WHERE changestatus.sys_change_status_id=change.sys_change_status) As Status, (SELECT sys_change_statusforcecolor FROM changestatus WHERE sys_change_status_id=change.sys_change_status) As StatusForceColor, (SELECT sys_change_statuscolor FROM changestatus WHERE sys_change_status_id=change.sys_change_status) As StatusColor, (SELECT Count(sys_action_id) FROM [action] WHERE action.sys_change_id = change.sys_change_id) As [ActionCount], (SELECT Count(sys_comment_id) FROM [comments] WHERE comments.sys_change_id = change.sys_change_id) As [CommentCount], (SELECT count(comments.sys_comment_id) FROM comments INNER JOIN commentsviewed ON comments.sys_comment_id = commentsviewed.sys_comment_id WHERE commentsviewed.sys_username=@strUser AND comments.sys_change_id = change.sys_change_id) As [ReadCommentCount] FROM change ";
        string strOpenStatus = " (sys_change_status IN (SELECT sys_change_status_id FROM changestatus WHERE sys_change_suspend<>2))";
        string strCloseStatus = " (sys_change_status=0 OR sys_change_status IN (SELECT sys_change_status_id FROM changestatus WHERE sys_change_suspend=2))";
        bool bWhere = true;

        if (ViewState["searchsql"] != null && ViewState["searchsql"].ToString() != "")
        {
            if (Session["ChangeSrchCmd"] != null)
            {
                SqlCommand cmdSrch = (SqlCommand)Session["ChangeSrchCmd"];

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

        }
        else if (ViewState["dashborad"] != null && ViewState["dashborad"].ToString() != "")
        {
            if (ViewState["srchcmd"].ToString() == "true" && Session["ChangeDashboardCmd"] != null)
            {
                SqlCommand cmdDsh = (SqlCommand)Session["ChangeDashboardCmd"];

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
                            strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null AND" + strOpenStatus;
                            //strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null";

                            bWhere = true;
                        }
                        else if (strGrp == "!")
                        {
                            string strAssigned = ViewState["assigned"].ToString();
                            if (strAssigned == "false")
                            {
                                strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null AND" + strOpenStatus;
                                //strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup Is Null";

                                bWhere = true;
                            }
                            else
                            {
                                strSqlPre = strSqlField + " WHERE sys_assignedto Is Not Null AND" + strOpenStatus;

                                bWhere = true;
                            }
                        }
                        else
                        {
                            string strAssigned = ViewState["assigned"].ToString();
                            if (strAssigned == "false")
                            {
                                strSqlPre = strSqlField + " WHERE sys_assignedto Is Null AND sys_assignedtoanalgroup=@strGrp AND" + strOpenStatus;

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@strGrp";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = strGrp;
                                cmd.Parameters.Add(parameter);

                                bWhere = true;
                            }
                            else
                            {
                                strSqlPre = strSqlField + " WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup=@strGrp AND" + strOpenStatus;

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

                        if (ViewState["approval"].ToString() == "true")
                        {
                            strSqlPre = strSqlField + "WHERE sys_change_id IN (SELECT sys_change_id FROM changeapproval WHERE sys_username=@strUser AND sys_approval_state=@Requested) AND " + strOpenStatus;
                            parameter = new SqlParameter();
                            parameter.ParameterName = "@Requested";
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = LayUtil.idChgApprvState_Requested;
                            cmd.Parameters.Add(parameter);
                        }
                        else
                        {
                            strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser AND " + strOpenStatus;
                        }

                        bWhere = true;
                    }
                }
                else if (ViewState["status"] != null && ViewState["status"].ToString() != "")
                {
                    /*
                    if (ViewState["status"].ToString() == "hold")
                    {
                        strSqlPre = strSqlField + " WHERE sys_problem_status in (SELECT sys_status_ID FROM [status] WHERE [sys_suspend]=1)";
                    }
                    else
                    {
                        strSqlPre = strSqlField + " WHERE " + strOpenStatus;
                    }
                    */
                    if (ViewState["status"].ToString() == "open")
                    {
                        strSqlPre = strSqlField + " WHERE " + strOpenStatus;
                    }
                    else if (ViewState["status"].ToString() == "thisweek")
                    {
                        DateTime dtStart = DateTime.Now.Date;
                        DateTime dtEnd = dtStart.AddDays(7);

                        strSqlPre = strSqlField + " WHERE (sys_startdate BETWEEN @dtStart AND @dtEnd)";
                        
                        parameter = new SqlParameter();
                        parameter.ParameterName = "@dtStart";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = dtStart;
                        cmd.Parameters.Add(parameter);

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@dtEnd";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = dtEnd;
                        cmd.Parameters.Add(parameter);
                    }
                    else if (ViewState["status"].ToString() == "overdue")
                    {
                        DateTime dtStart = DateTime.Now.Date;
                        DateTime dtEnd = dtStart.AddDays(7);

                        strSqlPre = strSqlField + " WHERE (GetDATE()>sys_finishdate) AND " + strOpenStatus;

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@dtStart";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = dtStart;
                        cmd.Parameters.Add(parameter);

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@dtEnd";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = dtEnd;
                        cmd.Parameters.Add(parameter);
                    }

                    bWhere = true;
                }
                else if (ViewState["action"] != null && ViewState["action"].ToString() != "")
                {
                    if (ViewState["action"].ToString() == "close" && ViewState["today"].ToString() != "")
                    {
                        strSqlPre = strSqlField + "WHERE DATEDIFF(D,sys_completedate, GETDATE())=0 AND " + strCloseStatus;
                    }
                    else if (ViewState["action"].ToString() == "log" && ViewState["today"].ToString() != "")
                    {
                        strSqlPre = strSqlField + "WHERE DATEDIFF(D,sys_changedate, GETDATE())=0";
                    }

                    bWhere = true;
                }
                /*
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
                 */ 
                else if (ViewState["owned"] != null && ViewState["owned"].ToString() != "")
                {
                    strSqlPre = strSqlField + "WHERE sys_requestedby=@strOwned AND" + strOpenStatus;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@strOwned";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = ViewState["owned"].ToString();
                    cmd.Parameters.Add(parameter);

                    bWhere = true;
                }
                /*
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
                        strSqlPre = strSqlField + "WHERE 1 " + strSiteSql;

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
                 */ 
                else
                {
                    strSqlPre = strSqlField;

                    bWhere = false;
                }
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
        }
        else
        {
            strFilter = ddFilter.SelectedValue;
            if (strFilter == "")
            {
                strFilter = "unassigned";
            }

            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            if (strFilter == "myassignedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser AND" + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "myownedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_requestedby=@strOwned AND" + strOpenStatus;

                parameter = new SqlParameter();
                parameter.ParameterName = "@strOwned";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = Session["User"].ToString();
                cmd.Parameters.Add(parameter);

                bWhere = true;
            }
            else if (strFilter == "unassigned")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Null AND" + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "myassignedall")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto=@strUser";
                bWhere = true;
            }
            else if (strFilter == "myownedall")
            {
                strSqlPre = strSqlField + "WHERE sys_requestedby=@strOwned";

                parameter = new SqlParameter();
                parameter.ParameterName = "@strOwned";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = Session["User"].ToString();
                cmd.Parameters.Add(parameter);

                bWhere = true;
            }
            else if (strFilter == "mygroupassginedopen")
            {
                strSqlPre = strSqlField + "WHERE sys_assignedto Is Not Null AND sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser) AND" + strOpenStatus;
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
            /*
            else if (strFilter == "mysiteopen")
            {
                strSqlPre = strSqlField + "WHERE sys_siteid=(SELECT sys_siteid FROM analsite WHERE sys_username=@strUser) AND ";
                bWhere = true;
            }
            else if (strFilter == "mysiteall")
            {
                strSqlPre = strSqlField + "WHERE sys_siteid=(SELECT sys_siteid FROM analsite WHERE sys_username=@strUser)";
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
                strSqlPre = strSqlField + "WHERE (sys_escalate2<getdate() AND ((sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate3 is NULL AND sys_resolve is NULL) )) AND" + strOpenStatus;
                bWhere = true;
            }
            else if (strFilter == "escl1")
            {
                strSqlPre = strSqlField + "WHERE ( sys_escalate1<getdate() AND ((sys_escalate2 is NOT NULL AND sys_escalate2>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NOT NULL AND sys_escalate3>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NOT NULL AND sys_resolve>getdate()) OR (sys_escalate2 is NULL AND sys_escalate3 is NULL AND sys_resolve is NULL) )) AND " + strOpenStatus;
                bWhere = true;
            }
             */ 
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
        string strAccess = dsUser.Tables[0].Rows[0]["sys_changeaccess"].ToString();

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

            strSqlPre += "(sys_requestedby= @strUser OR sys_assignedto = @strUser) ";

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

            strSqlPre += "(sys_requestedby= @strUser OR sys_assignedto=@strUser OR sys_assignedtoanalgroup IN (SELECT sys_analgroup FROM useranalgroup WHERE sys_username=@strUser)) ";

            bWhere = true;
        }


        /*
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


        WebTab ReqClassTab = new WebTab();
        ReqClassTab.Tabs.Clear();
        ReqClassTab.EnableOverlappingTabs = true;
        ReqClassTab.BorderStyle = BorderStyle.None;
        ReqClassTab.VisibleHeader = false;

        ReqClassTab.ID = "ChangeTabs";

        ReqClassTab.BorderWidth = new Unit("0px");
        ReqClassTab.BorderColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(xmlForm.DocumentElement, "pagecolor"));

        ReqClassTab.Style["Top"] = child.GetAttribute("top") + "px";
        ReqClassTab.Style["Left"] = child.GetAttribute("left") + "px";
        ReqClassTab.Style["Position"] = "absolute";

        ContentTabItem tab = new ContentTabItem();
        tab.BorderStyle = BorderStyle.None;

        tab.AutoSize = DefaultableBoolean.True;


        WebDataGrid dg = GenDG(child, false);
        ReqClassTab.Width = dg.Width;

        CustomPager pger = LoadControl("~/CustomPager.ascx") as CustomPager;
        pger.OnPageChange += new CustomPager.PageChange(pger_Command_All);
        pger.OnPageSizeChange += new CustomPager.PageSizeChange(pgerr_All);
        pger.ControlAttribute = "Single";
        pger.CurrentIndex = GetCurPageIndex();
        pger.PageSize = GetCurPageSize();
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


        string strField = GetSortField();

        Infragistics.Web.UI.SortDirection dirAll = GetSortAsc();
        strDataCmdText = cmd.CommandText;
        cmdData = cmd;
        if (strField != "")
        {
            string strOrderBy = "";
            if (dirAll == Infragistics.Web.UI.SortDirection.Ascending)
            {
                strOrderBy = strField + " asc";
            }
            else
            {
                strOrderBy = strField + " desc";
            }
            cmd.CommandText = cmd.CommandText.Replace("(ORDER BY sys_change_id desc) as RowIndex", "(ORDER BY " + strOrderBy + ") as RowIndex");
        }
        
        DataSet dsChg = LayUtilDA.GetDSCMD(cmd);        

        SetColor(dsChg);

        if (strField != "")
        {
            if (dg.Columns.FromKey(strField) != null)
            {
                dg.Behaviors.Sorting.SortedColumns.Add(strField, dirAll);
                string strOrder = dirAll.ToString();
                string strSort = "";
                if (strOrder == "Ascending")
                {
                    strSort = "ASC";
                }
                else if (strOrder == "Descending")
                {
                    strSort = "DESC";
                }
                if (dsChg.Tables[0].Rows.Count > 0)
                {
                    dsChg.Tables[0].DefaultView.Sort = strField + " " + strSort;

                }
            }
        }
        // Added by Sparhs ID 200
        if (dsChg.Tables.Count > 0)
        {
        dg.DataSource = dsChg.Tables[0].DefaultView;
        }
        // End by Sparsh ID 200

        //dg.Behaviors.Paging.PageSize = GetCurPageSize();
        //int nPage = GetCurPageIndex();
        //if (dg.Behaviors.Paging.PageCount > nPage)
        //{
        //    dg.Behaviors.Paging.PageIndex = nPage;
        //}
        //else
        //{
        //    dg.Behaviors.Paging.PageIndex = dg.Behaviors.Paging.PageCount - 1;
        //}
        dg.Behaviors.Paging.Enabled = false;
        dg.EnableAjax = false;
        dg.DataBind();

        // for bug  986
        dg.RequestFullAsyncRender();


        // dg.Behaviors.Paging.PagerTemplate = new MyPagerTemplateChange(dg, Page, dg.Behaviors.Paging.PageIndex, dg.Behaviors.Paging.PageSize);
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
        WebTab ultTab = (WebTab)ctrlRoot.FindControl("ChangeTabs");
        if (ultTab != null)
        {
            ctrlRoot.Controls.Remove(ultTab);
        }
        ctrlRoot.Controls.Add(ReqClassTab);
        // <%--  <igui:WebResizingExtender ID="extSize" runat="server" OnClientResize="myResizeFnc" />--%>

        WebResizingExtender extAll = new WebResizingExtender();
        extAll.ID = "extAll";
        extAll.TargetControlID = dg.ID;
        extAll.OnClientResize = "myResizeFnc";
        extAll.OnClientResizing = "myResizeFnc";
        tab.Controls.Add(extAll); 
        //ctrlRoot.Controls.Add(dg);


    }

    private int GetPageSize(CustomPager pger, out int maxRowNumber)
    {
        int minRowNumber = (pger.CurrentIndex - 1) * pger.PageSize;
        minRowNumber = minRowNumber > 0 ? minRowNumber : 0;
        pger.CurrentIndex = pger.CurrentIndex > 0 ? pger.CurrentIndex : 1;
        maxRowNumber = (pger.CurrentIndex * pger.PageSize) + 1;
        return minRowNumber;
    }

    void pger_Command_All(object sender, CommandEventArgs e)
    {
        CustomPager pgr = sender as CustomPager;
        if (pgr != null)
        {
            Session["ChangeListPage"] = Convert.ToInt32(e.CommandArgument);
            ReBind();
        }
    }

    void pgerr_All(int pageSize, string controlAttribute)
    {
        Session["ChangeListPageSize"] = pageSize;
        ReBind();
    }

    void ReBind()
    {
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            Control ctrlRoot = phCtrls;
            if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "result")
            {
                GenDGControl(child, ctrlRoot);

            }
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
        Response.Redirect("UserChange.aspx?filter=" + strFilter);
    }

    protected void Filter_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        WebDropDown dd = (WebDropDown)sender;
        strFilter = dd.SelectedValue;

        SetAutoUrl();

        ClearVwState();

        Session["ChangeList"] = "UserChange.aspx?back=true&filter=" + strFilter;
        Session["ButtonBack"] = Session["ChangeList"];

        /*
        if (tabReqClass != null)
        {
            phCtrls.Controls.Remove(tabReqClass);
            tabReqClass = null;
        }
        */

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

                    ctrl.Font.Name = child.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                    sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));

                    if (ViewState["searchsql"].ToString() != "" && Session["ChangeSrchCmd"] != null)
                    {
                        ctrl.SelectedItemIndex = 0;
                    }
                    else if (ViewState["dashborad"].ToString() != "")
                    {
                        ctrl.SelectedItemIndex = 0;
                    }
                    else
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
                    string strFieldType = DataDesignDA.GetFieldType("change", child.InnerText);
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
        templateColumn1.ItemTemplate = new GridCell(this, node);

        string strWidth = LayUtil.GetAttribute(node, "width");
        if (LayUtil.IsPercentage(strWidth))
        {
            templateColumn1.Width = new Unit(strWidth);
        }
        else if (LayUtil.IsNumeric(strWidth))
        {
            templateColumn1.Width = new Unit(strWidth+"px");
        }
        else
            templateColumn1.Width = new Unit("10%");

        //templateColumn1.Width = new Unit(LayUtil.GetAttribute(node,"width"));
        //templateColumn1.
    }

    private class GridCell : ITemplate
    {
        #region ITemplate Members

        private UserChange ParentPage;
        XmlElement node;
        public GridCell(UserChange pPage, XmlElement child)
        {
            ParentPage = pPage;
            node = child;
        }
        public void InstantiateIn(Control container)
        {
            /////////////////////////////////////////////////////////////////////////////
            Style style1 = new Style();
            GridRecordItem item = (GridRecordItem)(((TemplateContainer)container).Item);
            if (SysDA.GetSettingValue("appcolorrow", ParentPage.Application) == "true")
            {
                string strColor = ParentPage.GetChgColor(((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString());
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
                if (node.InnerText == "sys_change_id")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                    ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_change_status")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                    //ctrl.Text = LayUtil.RplTm(ParentPage.Application, ((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    //ctrl.BackColor = color

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);

                    string strStatus = ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_status"].ToString();
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
                        string strColor = ParentPage.GetChgColor(((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString());
                        if (strColor != "")
                        {
                            strColor = "background-color: " + strColor + " !important;";
                            item.CssClass = Infragistics.Web.UI.Framework.AppSettings.AppStylingManager.CssRegistry.Add(strColor, "tbody tr td.{0}");
                        }
                    }

                }
                else if (node.InnerText == "sys_assignedto")
                {
                    string strAssignedTo = ((DataRowView)((TemplateContainer)container).DataItem)["sys_assignedto"].ToString();

                    if (strAssignedTo == "")
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ChangeTake.aspx?option=assign&sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                        ctrl.ImageUrl = "Application_Images/16x16/take_change.png";
                        ctrl.ToolTip = "Take Assignment";

                        ctrl.ID = "hl" + node.Name;
                        ctrl.Style.Add("TEXT-DECORATION", "none");

                        container.Controls.Add(ctrl);
                    }
                    else
                    {
                        HyperLink ctrl = new HyperLink();
                        ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

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
                else if (node.InnerText == "sys_impact")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                    ctrl.Text = ParentPage.GetImpactText(((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_urgency")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                    ctrl.Text = ParentPage.GetUrgencyText(((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else if (node.InnerText == "sys_change_type")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                    ctrl.Text = ParentPage.GetChgTypeText(((DataRowView)((TemplateContainer)container).DataItem)[node.InnerText].ToString());

                    ctrl.Font.Name = node.GetAttribute("font-family");
                    ctrl.Font.Size = new FontUnit(node.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(node.GetAttribute("color"));

                    ctrl.ID = "hl" + node.Name;
                    ctrl.Style.Add("TEXT-DECORATION", "none");

                    container.Controls.Add(ctrl);
                }
                else
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = "ChangeInfo.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

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
            else if (strNodeName == "actionlink")
            {
                string linkImg;
                string strActCnt = ((DataRowView)((TemplateContainer)container).DataItem)["actioncount"].ToString();
                if (strActCnt == "" || strActCnt == "0")
                {
                    linkImg = LayUtil.GetAttribute(node, "linkimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/16x16/task_16px.png";
                    }
                }
                else
                {
                    linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                    if (linkImg == "")
                    {
                        linkImg = "Application_Images/16x16/task_hasitems.png";
                    }
                }

                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "UserTask.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

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
                        linkImg = "Application_Images/16x16/Comment_icon_16px.png";
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
                            linkImg = "Application_Images/16x16/Commentshasunreaditem16px.png";
                        }
                        strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items, " + (nCnt - nReadCnt).ToString() + " Un-Read)";
                    }
                    else
                    {
                        linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                        if (linkImg == "")
                        {
                            linkImg = "Application_Images/16x16/Commentshasreaditem16px.png";
                        }
                        strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items)";
                    }
                }


                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "ChangeComment.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                ctrl.ImageUrl = linkImg;
                ctrl.ToolTip = strAlt;

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);

            }
            else if (strNodeName == "attachlink")
            {
                string strReqId = ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();
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
                ctrl.NavigateUrl = "ChangeAttach.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString();

                ctrl.ImageUrl = linkImg;
                ctrl.ToolTip = "Attachments (" + nAttchCnt + " Items)";

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);
            }
            else if (strNodeName == "opencloselink")
            {
                string strStatus = ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_status"].ToString();

                string strImg = "";
                string strAlt = "";

                if (strStatus == "" || ParentPage.htOpenStatus[strStatus] == null)
                {
                    //strImg = "images/close.gif";
                    strImg = "Application_Images/16x16/Request_Close_16px.png";
                    strAlt = "Status: Completed - Click to change status";
                }
                else
                {
                    //strImg = "images/open.gif";
                    strImg = "Application_Images/16x16/Change_Open.png";
                    strAlt = "Status: Open - Click to change status";
                }

                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "ChangeChgStatus.aspx?sys_change_id=" + ((DataRowView)((TemplateContainer)container).DataItem)["sys_change_id"].ToString() + "&openclose=" + strStatus;

                ctrl.ImageUrl = strImg;
                ctrl.ToolTip = strAlt;

                ctrl.ID = "hl" + node.Name;
                ctrl.Style.Add("TEXT-DECORATION", "none");

                container.Controls.Add(ctrl);
            }


        }


        #endregion
    }

    private void SetColor(DataSet ds)
    {
        if (htChgColor == null)
        {
            htChgColor = new Hashtable();
        }

        if (ds != null && ds.Tables.Count > 0)
        {
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string strChgId = dr["sys_change_id"].ToString();
                if (htChgColor[strChgId] == null)
                {
                    //System.Drawing.Color colorReq = System.Drawing.Color.White;
                    string strColor = "";
                    strColor = dr["StatusColor"].ToString();

                    htChgColor[strChgId] = strColor;
                }
            }
        }
    }

    private string GetChgColor(string strChgId)
    {
        if (htChgColor[strChgId] != null)
        {
            return htChgColor[strChgId].ToString();
        }


        return "";
    }
    private string GetImpactText(string strId)
    {
        if (htImpact == null)
        {
            htImpact = new Hashtable();
            DataSet dsImpact = LibImpactDA.GetItemInfo("");
            if (dsImpact != null && dsImpact.Tables.Count > 0 && dsImpact.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsImpact.Tables[0].Rows)
                {
                    htImpact[dr["sys_impact_id"].ToString()] = dr["sys_impact"].ToString();
                }
            }
        }

        if (htImpact[strId] != null)
            return htImpact[strId].ToString();

        return "";
    }

    private string GetUrgencyText(string strId)
    {
        if (htUrgency == null)
        {
            htUrgency = new Hashtable();
            DataSet dsUrgency = LibUrgencyDA.GetItemInfo("");
            if (dsUrgency != null && dsUrgency.Tables.Count > 0 && dsUrgency.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsUrgency.Tables[0].Rows)
                {
                    htUrgency[dr["sys_urgency_id"].ToString()] = dr["sys_urgency"].ToString();
                }
            }
        }

        if (htUrgency[strId] != null)
            return htUrgency[strId].ToString();

        return "";
    }
    private string GetChgTypeText(string strId)
    {
        if (htChgType == null)
        {
            htChgType = new Hashtable();
            DataSet dsChgType = LibChgTypeDA.GetList("");
            if (dsChgType != null && dsChgType.Tables.Count > 0 && dsChgType.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsChgType.Tables[0].Rows)
                {
                    htChgType[dr["sys_changetype_id"].ToString()] = dr["sys_changetype"].ToString();
                }
            }
        }

        if (htChgType[strId] != null)
            return htChgType[strId].ToString();

        return "";
    }
    private class MyPagerTemplateChange : ITemplate
    {
        WebDataGrid dg;
        Control rootCtrl;
        int nPageIndex;
        int nCurPageIndex;
        int nPageSize;
        public MyPagerTemplateChange(WebDataGrid dgGrid, Page page, int pageindex, int pagesize)
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
                ((UserChange)dg.Page).dgResult_PageIndexChanged(dg);
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
                ((UserChange)dg.Page).dgResult_PageSizeChanged(dg);
            }
        }

        public void LoadCtrl()
        {
            rootCtrl.Controls.Clear();
            //int nPageIndex = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageIndex"].ToString());
            //int nSize = int.Parse(((Analyst_UserRequest)dg.Page).ViewState["PagerPageSize"].ToString());
            int nSize = nPageSize;
            UserChange page = ((UserChange)dg.Page);

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
            lb.Text = nStart.ToString() + " to " + nEnd + " of " + nTotal + " Changes)";

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
            ((UserChange)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnPrev_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex - 1;
            LoadCtrl();
            ((UserChange)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnLast_Click(object sender, EventArgs e)
        {
            nPageIndex = dg.Behaviors.Paging.PageCount - 1;
            LoadCtrl();
            ((UserChange)dg.Page).dgResult_PageIndexChanged(dg);
        }
        void lbtnNext_Click(object sender, EventArgs e)
        {
            nPageIndex = nPageIndex + 1;
            LoadCtrl();
            ((UserChange)dg.Page).dgResult_PageIndexChanged(dg);
        }

    }
}
