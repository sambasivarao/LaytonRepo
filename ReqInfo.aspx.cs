using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.UI.HtmlControls;
using System.Xml;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Infragistics.WebUI;
using Infragistics.Web.UI;
using Infragistics.Web.UI.ListControls;
using Infragistics.WebUI.WebDataInput;
using Infragistics.WebUI.WebSchedule;
using Infragistics.Web.UI.GridControls;
using Infragistics.Web.UI.EditorControls;
using Infragistics.Web.UI.LayoutControls;
using Infragistics.Web.UI.DataSourceControls;

using Telerik.Web.UI;

/// <summary>
/// Analyst request detail page
/// </summary>

public partial class Analyst_ReqInfo : System.Web.UI.Page
{
    public XmlDocument xmlForm;

    public DataSet dsOldValue;
    public DataSet dsTemplateValue;

    public Hashtable htCols;
    public Hashtable htColSize;

    public Hashtable htStatus;
    private Hashtable htOpenStatus;
    private Hashtable htMailVal;

    public Hashtable htChildName;

    public string strBackImg = "";

    public string strNewPK = "";

    public bool bAllowChange = true;
    public string strReqClass = "(Defalut)";
    public string strReqLinkType = "";
    public DataSet dsUser;

    private List<SusPeriod> listSus = null;
    private XmlDocument xmlHours = null;
    private Hashtable htOpenHr = null;
    private Hashtable htCloseHr = null;

    public string strClientIDs;

    public int nReqTypeCtrlCnt = 0;
    public Hashtable htCtrl = null;
    public Hashtable htCtrlNode = null;

    public StringBuilder sbTxt = null;
    public int nLen = 0;
    public string[] strReqTypeArray = null;

    public XmlElement nodeComment = null;

    public bool bShowTab = false;
    public Hashtable htTabNode = null;
    public Hashtable htTabOrders = null;
    public Hashtable htTabNodeWithInvIndex = null;

    public Hashtable htTabs = null;
    public Hashtable htTabText = null;
    public Hashtable htTabName = null;

    public Hashtable htBRVal = null;
    public bool bRule = false;

    public Label lbCost;

    public bool bDelComment = false;
    public bool bClosed = true;

    public string strTabHeight = "200";
    public string strTabWidth = "500";
    public RadEditor edtProblem = null;
    public RadEditor edtSolution = null;
    public static int m_iIPhone = 0;

    public string strCommentHtml = "true";

    HtmlTableCell hdgCommCell = null;

    private bool bReqNew;

    Control ctrlFocus = null;
    #region Iphone Request 
    /// <summary>
    /// Iphone Request 
    /// </summary>
    /// <returns></returns>
    private bool IsiPhone()
    {
        //string userAgent = Request.Headers["User-Agent"];
        if (Request.Browser.IsMobileDevice)
        {
            return true;
        }
        else
        {
            return false;
        }

        /*if (!string.IsNullOrEmpty(userAgent))
        {
            return true;//userAgent.ToLowerInvariant().Contains("iphone");
        }
        else
        {
            return true;
        }*/

    }
    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        if (!IsPostBack)
        {
            SaveQueryStr();
            Session["ButtonBack"] = "ReqInfo.aspx?back=true&sys_request_id=" + ViewState["sys_request_id"];
            lbTTLTemplate.Text = Application["apprequestterm"] + " Template";
            lbTTLEsc.Text = Application["apprequestterm"] + " Escalations";
            dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogEsc.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogAttachInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogComComfirm.WindowState = DialogWindowState.Hidden;
            dialogPriorityChgExplanation.WindowState = DialogWindowState.Hidden;

            dgTask.Columns[3].Header.Text = Application["appuserterm"].ToString();
            dgCallback.Columns[2].Header.Text = Application["appuserterm"].ToString();

            LayUtil.SetFont(this.dialogMsg, Application);
            LayUtil.SetFont(this.dialogTemplate, Application);
            LayUtil.SetFont(this.dialogTaskTemplate, Application);
            LayUtil.SetFont(this.dialogEsc, Application);
            LayUtil.SetFont(this.dialogDelConfirm, Application);
            LayUtil.SetFont(this.dialogAttachInfo, Application);
            LayUtil.SetFont(this.dialogComComfirm, Application);
            LayUtil.SetFont(this.dialogPriorityChgExplanation, Application);

        }
        else
        {
            if (ViewState["bRule"] != null && ViewState["bRule"].ToString() == "true")
            {
                bRule = true;
            }
            if (ViewState["htBRVal"] != null)
            {
                htBRVal = (Hashtable)ViewState["htBRVal"];
            }

        }

        //bool bValue = IsiPhone();


        MyInit();
        ApplyMultiReqType();
    }

    private void ApplyMultiReqType()
    {
        if (LayUtil.GetQueryString(Request, "__EVENTTARGET") != "Javascript")
            return;

        if (tbApplyMulitipleRequestTypeH.Text != "1")
        {
            return;
        }
        tbApplyMulitipleRequestTypeH.Text = "";

        SetReqTypeValue(GetUserInputText("sys_requesttype_id"));

    }

    private void SaveQueryStr()
    {
        ViewState["sys_request_id"] = LayUtil.GetQueryString(Request, "sys_request_id");
        ViewState["reqclass"] = LayUtil.GetQueryString(Request, "reqclass");
        ViewState["sys_requestparent_id"] = LayUtil.GetQueryString(Request, "sys_requestparent_id");
        ViewState["templateid"] = LayUtil.GetQueryString(Request, "templateid");
        ViewState["field"] = LayUtil.GetQueryString(Request, "field");

        ViewState["sys_mail_id"] = LayUtil.GetQueryString(Request, "sys_mail_id");
        ViewState["from"] = LayUtil.GetQueryString(Request, "from");
        ViewState["sys_problem_id"] = LayUtil.GetQueryString(Request, "sys_problem_id");
        ViewState["sys_change_id"] = LayUtil.GetQueryString(Request, "sys_change_id");

        ViewState["sys_requesttype_id"] = "";

        //for request xml export
        Session["requestexportid"] = ViewState["sys_request_id"].ToString();
    }
    /// <summary>
    /// Initialize controls and load values
    /// </summary>
    private void MyInit()
    {

        //Load Form
        string strReqId = "";
        strReqClass = "(Defalut)";
        strReqLinkType = "";

        if (ViewState["sys_request_id"] != null)
        {
            strReqId = ViewState["sys_request_id"].ToString();
        }

        if (ViewState["reqclass"] != null)
        {
            strReqClass = ViewState["reqclass"].ToString();
        }

        string strTemplateId = ViewState["templateid"].ToString();
        if (LayUtil.IsNumeric(strTemplateId))
        {
            dsTemplateValue = LibReqTemplateDA.GetTemplateInfoById(strTemplateId);
            if (dsTemplateValue != null && dsTemplateValue.Tables.Count > 0 && dsTemplateValue.Tables[0].Rows.Count > 0)
            {
                strReqClass = dsTemplateValue.Tables[0].Rows[0]["sys_requestclass_id"].ToString();
            }
        }

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

        if (strReqId != "")
        {
            DataSet dsReq = RequestDA.GetReqById(strReqId);

            if (dsReq == null || dsReq.Tables.Count <= 0 || dsReq.Tables[0].Rows.Count <= 0)
            {
                ShowMsg("Couldn't find this " + Application["apprequestterm"]);
                return;
            }

            strReqClass = dsReq.Tables[0].Rows[0]["sys_requestclass_id"].ToString();

            if (htOpenStatus[dsReq.Tables[0].Rows[0]["sys_requeststatus"].ToString()] == null && Application["applockuserclose"].ToString() == "true")
            {
                bAllowChange = false;
                textChgAllowed.Text = "false";
            }
            else
            {
                strReqLinkType = dsReq.Tables[0].Rows[0]["sys_requestlinktype"].ToString();
            }
        }

        if (ViewState["sys_requestparent_id"] != null && ViewState["sys_requestparent_id"].ToString() != "")
        {
            strReqLinkType = "1";
        }

        if (strReqId != "" && strReqClass == "")
        {
            ShowMsg("Couldn't find " + Application["apprequestterm"] + " Class");
            return;
        }

        string strXml = "";
        if (strReqLinkType == "1")
        {
            if (FormDesignerDA.ExistUserSpawnReqForm(Session["User"].ToString(), strReqClass))
            {
                DataSet dsFormXml = FormDesignerDA.GetUserSpawnReqForm(Session["User"].ToString(), strReqClass);
                strXml = dsFormXml.Tables[0].Rows[0]["formxml"].ToString();
            }
            else
            {
                DataSet dsReqClass = LibReqClassDA.GetReqClassAllInfo(strReqClass);

                if (dsReqClass == null || dsReqClass.Tables.Count <= 0 || dsReqClass.Tables[0].Rows.Count <= 0)
                {
                    ShowMsg("Couldn't find " + Application["apprequestterm"] + " Class");
                    return;
                }

                strXml = dsReqClass.Tables[0].Rows[0]["sys_requestclass_xmluserformspawn"].ToString();
            }
        }
        else
        {
            if (FormDesignerDA.ExistUserReqForm(Session["User"].ToString(), strReqClass))
            {
                DataSet dsFormXml = FormDesignerDA.GetUserReqForm(Session["User"].ToString(), strReqClass);

                strXml = dsFormXml.Tables[0].Rows[0]["formxml"].ToString();

            }
            else
            {
                DataSet dsReqClass = LibReqClassDA.GetReqClassAllInfo(strReqClass);

                if (dsReqClass == null || dsReqClass.Tables.Count <= 0 || dsReqClass.Tables[0].Rows.Count <= 0)
                {
                    ShowMsg("Couldn't find " + Application["apprequestterm"] + " Class");
                    return;
                }

                strXml = dsReqClass.Tables[0].Rows[0]["sys_requestclass_xmluserform"].ToString();
            }
        }

        xmlForm = new XmlDocument();
        if (strXml != "")
        {
            xmlForm.LoadXml(strXml);
        }


        //Set Status Hashtable
        DataSet dsStatus = LibReqStatusDA.GetReqStatusList("");
        htStatus = new Hashtable();
        if (dsStatus != null && dsStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsStatus.Tables[0].Rows)
            {
                htStatus.Add(dr["sys_status_ID"].ToString(), dr["sys_status"].ToString());
            }
        }

        //Get Current logon user info
        dsUser = UserDA.GetUserInfo(Session["User"].ToString());
        if (!IsPostBack)
        {
            if (dsUser.Tables[0].Rows[0]["sys_requestclassrestrict"].ToString() == "1")
            {
                if (!UserDA.UserAccessReqClass(Session["User"].ToString(), strReqClass))
                {
                    Response.End();
                    return;
                }
            }
        }

        CheckTabSize();

        ///Load request and related value
        if (ViewState["sys_request_id"] != null && ViewState["sys_request_id"].ToString() != "")
        {
            LoadVal();

            LoadSus(GetValue("sys_request_id"), "", GetValue("sys_requestdate"));
        }

        LoadCtrlInfo();

        LoadCtrl();

        LayUtil.SetFont(this.dialogMsg, Application);
        LayUtil.SetFont(this.dialogTemplate, Application);
        LayUtil.SetFont(this.dialogTaskTemplate, Application);
        LayUtil.SetFont(this.dialogEsc, Application);
        LayUtil.SetFont(this.dialogDelConfirm, Application);
        LayUtil.SetFont(this.dialogAttachInfo, Application);
        LayUtil.SetFont(this.dialogComComfirm, Application);
    }

    private void LoadCtrlInfo()
    {
        nReqTypeCtrlCnt = 0;
        htCtrl = new Hashtable();
        htCtrlNode = new Hashtable();
        htChildName = new Hashtable();
        htTabNode = new Hashtable();
        htTabNodeWithInvIndex = new Hashtable();

        int nInvalidNodeIndex = 0;
        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            if (node.InnerText == "sys_requesttype_id")
            {
                nReqTypeCtrlCnt++;
            }

            if ((node.Name.Length >= 5 && node.Name.Substring(0, 5).ToLower() == "field") || (node.Name.Length >= 4 && node.Name.Substring(0, 4).ToLower() == "sys_"))
            {
                htChildName[node.InnerText] = node.Name;
                htCtrl[node.InnerText] = "true";
                htCtrlNode[node.InnerText] = node;
            }

            if (LayUtil.GetAttribute(node, "ontab") == "true")
            {
                bShowTab = true;

                string strTabOrder = LayUtil.GetAttribute(node, "ontaborder");
                if (strTabOrder != "")
                {
                    if (htTabNode[strTabOrder] == null)
                    {
                        //Checking the request is coming from the which device.
                        //if (IsiPhone())
                        //{
                        //    if (node.InnerText == "sys_problemdesc" || node.InnerText == "sys_solutiondesc" || node.Name == "sys_button7")
                        //    {
                        //        continue;
                        //    }
                        //}
                        htTabNode[strTabOrder] = node;
                    }
                    else
                    {
                        nInvalidNodeIndex++;
                        htTabNodeWithInvIndex[nInvalidNodeIndex.ToString()] = node;
                    }
                }
            }
        }

        if (nInvalidNodeIndex > 0)
        {
            foreach (string key in htTabNodeWithInvIndex.Keys)
            {
                for (int nInx = 0; nInx < 100; nInx++)
                {
                    if (htTabNode[nInx] == null)
                    {
                        htTabNode[nInx] = htTabNodeWithInvIndex[key];
                        break;
                    }
                }
            }

        }

        //Initialize columns hashtable
        string strTbl = "request";
        htCols = new Hashtable();
        htColSize = new Hashtable();

        DataSet dsCols = DataDesignDA.GetTblCol(strTbl);

        if (dsCols != null && dsCols.Tables.Count > 0)
        {
            foreach (DataRow dr in dsCols.Tables[0].Rows)
            {
                htCols[dr["ColName"].ToString()] = dr["ColType"].ToString();
                htColSize[dr["ColName"].ToString()] = dr["ColSize"].ToString();
            }
        }

    }
    /// <summary>
    /// Load Value From Database
    /// </summary>
    private void LoadVal()
    {
        if (xmlForm.DocumentElement == null)
            return;

        string strTbl = "request";
        string strSql = "SELECT [" + strTbl + "].* ";

        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            string strDispTbl = node.GetAttribute("displaytable");
            string strDispLink = node.GetAttribute("displaytablelink");
            string strLink = node.GetAttribute("mastertablelink");

            if (strDispTbl != "" && strDispTbl != strTbl)
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

                strSql += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[" + strTbl + "]." + strLink + ") AS " + strAsField;
            }
        }

        strSql += " FROM [" + strTbl + "]";

        if (ViewState["sys_request_id"] == null || ViewState["sys_request_id"].ToString() == "")
        {
            strSql += " WHERE sys_request_id = Null";
        }
        else
        {
            strSql += " WHERE sys_request_id='" + ViewState["sys_request_id"].ToString().Replace("'", "''") + "'";
        }

        dsOldValue = LayUtilDA.GetDSSQL(strSql);

    }

    private void SetCtrlAtt(Control ctrl)
    {
    }

    /// <summary>
    /// Load controls according to form xml
    /// </summary>
    private void LoadCtrl()
    {
        Control ctrlRoot = phCtrls;

        ///To save the clientID for each control
        sbTxt = new StringBuilder();
        sbTxt.Append("var arrCtrlID = new Array();");
        sbTxt.Append("var arrClientID = new Array();");
        sbTxt.Append("var arrCtrlType = new Array();");
        sbTxt.Append("var arrCtrlSize = new Array();");

        nLen = 0;

      StringBuilder sbDDCss = new StringBuilder("<style type=\"text/css\">");

        int nReqTypeIndex = 1;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = GetValue(child.InnerText);
            if (!IsPostBack)
            {
                string strField = child.InnerText;
                if (strField == "sys_siteid")
                {
                    if (SysDA.GetSettingValue("appcompanylevel", Application) == "true" && SysDA.GetSettingValue("appdispcompanyseperately", Application) == "true")
                    {
                        sys_siteidH.Text = UserDA.GetSiteNameFrmSiteId(strVal);
                        sys_company_idH.Text = UserDA.GetComNameFrmSiteId(strVal);
                    }
                    else
                    {
                        sys_siteidH.Text = strVal;
                    }
                }
                else if (strField == "sys_company_id")
                {
                    sys_company_idH.Text = strVal;
                }
                else if (strField == "sys_solution_id")
                {
                    sys_solution_idH.Text = strVal;
                }
                else if (strField == "sys_requesttype_id")
                {
                    sys_requesttype_idH.Text = strVal;
                }
                else if (strField == "sys_assignedto")
                {
                    sys_assignedtoH.Text = strVal;

                    // written by Sparsh ID 169

                    if (dsUser.Tables[0].Rows[0]["sys_requestclassrestrict"].ToString() == "1" && dsUser.Tables[0].Rows[0]["sys_username"].ToString() == Session["User"].ToString())
                        {
                           // ShowMsg("Request class restricted " + child.GetAttribute("caption") + " box.");
                            strVal = "";
                            sys_assignedtoH.Text = strVal;
                        }
                        else
                            sys_assignedtoH.Text = strVal;
                    
                    //End by Sparsh ID 169
                }
                else if (strField == "sys_ownedby")
                {
                    sys_ownedbyH.Text = strVal;
                }
                else if (strField == "sys_assignedtoanalgroup")
                {
                    sys_assignedtoanalgroupH.Text = strVal;
                }
                else if (strField == "sys_requestparent_id")
                {
                    sys_requestparent_idH.Text = strVal;
                }
            }
            if (child.Name == "tab")
            {
                if (!bShowTab)
                {
                    continue;
                }

                WebTab tab = new WebTab();
                tab.Tabs.Clear();
                tab.EnableViewState = false;
                string strWidth = child.GetAttribute("width");
                string strHeight = child.GetAttribute("height");

                if (LayUtil.IsNumeric(strWidth))
                {
                    tab.Width = new Unit(strWidth + "px");
                    strTabWidth = strWidth;
                }
                else
                {
                    tab.Width = new Unit("200px");
                    strTabWidth = "200";
                }

                if (LayUtil.IsNumeric(strHeight))
                {
                    tab.Height = new Unit(strHeight + "px");
                }
                else
                {
                    tab.Height = new Unit("100px");

                }

                strTabHeight = tab.Height.Value.ToString();

                tab.ToolTip = child.GetAttribute("caption");
                tab.Font.Name = child.GetAttribute("font-family");
                tab.Font.Size = new FontUnit(child.GetAttribute("font-size" + "px"));

                tab.Style["Top"] = child.GetAttribute("top") + "px";
                tab.Style["Left"] = child.GetAttribute("left") + "px";
                tab.Style["Position"] = "absolute";

                tab.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                {
                    tab.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                }

                if (child.GetAttribute("tabindex") == "1")
                {
                    //tab.Focus();                   
                }

                tab.ID = "tabMain";


                tab.EnableOverlappingTabs = true;
                tab.TabMoving.Enabled = false;

                tab.AutoPostBackFlags.TabMoved = Infragistics.Web.UI.AutoPostBackFlag.On;
                tab.TabItemsMoved += new TabItemsMovedEventHandler(tab_TabItemsMoved);

                WebResizingExtender extTab = new WebResizingExtender();
                extTab.TargetControlID = "tabMain";
                extTab.OnClientResize = "myResizeFnc";
                extTab.OnClientResizing = "myResizingFnc";
                extTab.HandleClass = "hand0";
                
                ctrlRoot.Controls.Add(tab);
                ctrlRoot.Controls.Add(extTab);

                textTabCIdH.Text = tab.ClientID;
                AddTabs(tab);
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
            else if (child.Name.Length >= 13 && child.Name.Substring(0, 13).ToLower() == "readonlyfield")
            {
                Label ctrl = new Label();

                if (child.InnerText == "sys_requeststatus")
                {
                    if (ViewState["sys_request_id"] != null && ViewState["sys_request_id"].ToString() != "")
                    {
                        if (htStatus[strVal] != null)
                        {
                            ctrl.Text = htStatus[strVal].ToString();
                        }
                        else
                        {
                            ctrl.Text = "Closed";
                        }
                    }
                    else
                    {
                        ctrl.Text = "";
                    }
                }
                else if (child.InnerText == "sys_request_24mins" || child.InnerText == "sys_request_hdmins")
                {
                    string strDispFormat = SysDA.GetSettingValue("statsdurationdisplay", Application);

                    ctrl.Text = RequestDA.Mins2Time(strVal, strDispFormat);
                }
                else if (child.InnerText == "sys_forename")
                {
                    string strDispTbl = child.GetAttribute("displaytable");
                    string strDispLink = child.GetAttribute("displaytablelink");
                    string strLink = child.GetAttribute("mastertablelink");

                    string strAsField = child.InnerText;
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
                    ctrl.Text = GetValue(strAsField);
                }
                else if (child.InnerText == "sys_surname")
                {
                    string strDispTbl = child.GetAttribute("displaytable");
                    string strDispLink = child.GetAttribute("displaytablelink");
                    string strLink = child.GetAttribute("mastertablelink");

                    string strAsField = child.InnerText;
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
                    ctrl.Text = GetValue(strAsField);
                }
                else
                {
                    ctrl.Text = LayUtil.RplTm(Application, strVal);
                }

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
                ctrl.NavigateUrl = LayUtil.RplDataVal(child.GetAttribute("url"), dsOldValue); ;

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

                if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                {
                    ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                }

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
                    ctrl.DisplayMode = DropDownDisplayMode.DropDownList;
                    ctrl.DropDownContainerMaxHeight = new Unit(LayUtil.DropDownMaxHeight);
                    ctrl.DropDownContainerHeight = new Unit("0px");


                    sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));


                    //Set list items
                    bool bAllowEmpty = false;
                    if (child.GetAttribute("userreq") != "true")
                    {
                        bAllowEmpty = true;
                    }

                    SetDropDown(ctrl, child.GetAttribute("values"), bAllowEmpty);

                    if (strVal != "")
                    {
                        ctrl.SelectedValue = strVal;
                    }
                    ctrlRoot.Controls.Add(ctrl);


                    sbTxt.Append("arrCtrlID[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append(child.InnerText);
                    sbTxt.Append("';");
                    sbTxt.Append("arrClientID[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append(ctrl.ClientID);
                    sbTxt.Append("';");
                    sbTxt.Append("arrCtrlType[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append("DD");
                    sbTxt.Append("';");
                    nLen++;

                    if (UserReadOnly(child))
                    {
                        ctrl.Enabled = false;
                    }

                    if (child.GetAttribute("tabindex") == "1")
                    {
                        ctrl.Focus();
                    }
                }
                else
                {
                    /*
                    string strFieldType = DataDesignDA.GetFieldType("request", child.InnerText);
                    if (strFieldType == "")
                        continue;
                    */

                    if (htCols[child.InnerText] == null)
                        continue;
                    string strFieldType = htCols[child.InnerText].ToString();

                    if (child.GetAttribute("htmleditor") == "true")
                    {
                        #region RadEditor Control
                        RadEditor ctrl = LayUtil.CreateHTMLEditor(Page, child);
                        
                        if (IsiPhone())
                        {
                            ctrl.EditModes = EditModes.Html;
                        }
                        ctrl.Content = strVal;
                        ctrl.OnClientCommandExecuting = "OnClientCommandExecuting";
                        //ctrl.ContentAreaMode = EditorContentAreaMode.Div;
                        ctrl.OnClientLoad = "OnClientLoad";
                        ctrl.OnClientModeChange = "OnClientModeChange";

                        HtmlGenericControl div = new HtmlGenericControl("div");
                        div.Style["Top"] = child.GetAttribute("top") + "px";
                        div.Style["Left"] = child.GetAttribute("left") + "px";
                        div.Style["Position"] = "absolute";

                        div.Controls.Add(ctrl);
                        ctrlRoot.Controls.Add(div);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("HTML");
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlSize[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(htColSize[child.InnerText].ToString());
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        #endregion
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
                        datePicker.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

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

                        if (UserReadOnly(child))
                        {
                            datePicker.ReadOnly = true;
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
                            ctrl.Height = new Unit(strHeight + "px");
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

                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.ReadOnly = true;
                        }
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

                string strShowOnTab = LayUtil.GetAttribute(child, "ontab");
                if (strShowOnTab == "true")
                {
                    continue;
                }

                //Save Button
                if (child.Name == "sys_button1")
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;
                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button1_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);return false;";

                    }

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button2")
                {
                    //change status
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button2_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqChgStatus.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button3")
                {
                    //Tasks
                    string strImg = child.GetAttribute("image");
                    int nCnt = 0;
                    if (ViewState["sys_request_id"].ToString() != "")
                    {
                        nCnt = RequestDA.GetReqTaskCnt(ViewState["sys_request_id"].ToString());
                        if (nCnt > 0)
                        {
                            if (child.GetAttribute("hasitemsimage") != "")
                            {
                                strImg = child.GetAttribute("hasitemsimage");
                            }

                        }
                    }
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = strImg;
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button3_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "UserTask.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button4")
                {
                    HyperLink ctrl = new HyperLink();
                    ctrl.NavigateUrl = GetListURL();
                    ctrl.ImageUrl = child.GetAttribute("image");

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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

                }
                else if (child.Name == "sys_button5")
                {
                    //Request History
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ImageUrl = child.GetAttribute("image");
                    ctrl.ID = "imgbtn" + child.Name;

                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button5_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqHistory.aspx?sys_request_id=" + ViewState["sys_request_id"] + "';return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button6")
                {
                    //Attachment
                    string strImg = child.GetAttribute("image");
                    int nCnt = RequestDA.GetReqAttchCnt(ViewState["sys_request_id"].ToString());

                    if (nCnt > 0)
                    {
                        if (child.GetAttribute("hasitemsimage") != "")
                        {
                            strImg = child.GetAttribute("hasitemsimage");
                        }

                    }

                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = strImg;
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button6_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqAttach.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button7")
                {
                    strCommentHtml = child.GetAttribute("commenthtmleditor");
                    //Comments
                    nodeComment = child;
                    if (xmlForm.DocumentElement.GetAttribute("commentsinline") == "true")
                    {
                    }
                    else
                    {
                        //not in line
                        string strImg = child.GetAttribute("image");
                        int nCnt = 0;
                        int nReadCnt = 0;

                        if (ViewState["sys_request_id"].ToString() != "")
                        {
                            nCnt = RequestDA.GetCommentCntByReqId(ViewState["sys_request_id"].ToString());
                            nReadCnt = RequestDA.GetCommentReadCntByUserReqId(ViewState["sys_request_id"].ToString(), Session["User"].ToString());

                            if (nCnt > 0)
                            {
                                if (nCnt > nReadCnt)
                                {
                                    if (child.GetAttribute("buttonhasunreaditemsimg") != "")
                                    {
                                        strImg = child.GetAttribute("buttonhasunreaditemsimg");
                                    }
                                }
                                else
                                {
                                    if (child.GetAttribute("hasitemsimage") != "")
                                    {
                                        strImg = child.GetAttribute("hasitemsimage");
                                    }

                                }
                            }
                        }

                        System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                        ctrl.ID = "imgbtn" + child.Name;

                        ctrl.ImageUrl = strImg;
                        if (bAllowChange)
                        {
                            ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                            ctrl.Click += new ImageClickEventHandler(sys_button7_Click);
                        }
                        else
                        {
                            ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqComment.aspx?CommentHTML=" + strCommentHtml + "&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                        }

                        ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                        ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

                        ctrl.Style["cursor"] = "hand";
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
                    }
                }
                else if (child.Name == "sys_button8")
                {
                    if (SysDA.GetSettingValue("appreqallowspawn", Application) != "true")
                    {
                        continue;
                    }

                    //Spawn request
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");

                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button8_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button9")
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "selecttemplate();return false;";
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button10")
                {
                    if (SysDA.GetSettingValue("showcopyrequest", Application) != "true")
                        continue;


                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button10_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "document.location='" + "ReqCopy.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button11")
                {
                    //change request class
                    string strReqClassRestrct = GetUserVal("sys_requestclassrestrict");

                    DataSet dsReqClass = null;
                    if (strReqClassRestrct == "0")
                    {
                        dsReqClass = LibReqClassBR.GetReqClassList();
                    }
                    else
                    {
                        dsReqClass = LibReqClassDA.GetUserReqClass(Session["User"].ToString());
                    }

                    if (dsReqClass == null || dsReqClass.Tables.Count <= 0 || dsReqClass.Tables[0].Rows.Count <= 1)
                    {
                        continue;
                    }
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button11_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button12")
                {
                    //Call back
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button12_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqCallback.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button13")
                {
                    //Cost
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button13_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqCost.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button14")
                {
                    if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
                    {
                        continue;
                    }

                    //Call back
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button14_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ReqLink.aspx?sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button15")
                {
                    if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
                    {
                        continue;
                    }

                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button15_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ProblemInfo.aspx?newfromreq=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else if (child.Name == "sys_button16")
                {
                    if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
                    {
                        continue;
                    }

                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button16_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ChangeInfo.aspx?newfromreq=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
                else
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button1_Click);
                    }
                    else
                    {
                        ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

                    ctrl.Style["cursor"] = "hand";
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
                }
            }
            else
            {
                if (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_")
                {
                    if (child.InnerText == "sys_problemdesc" || child.InnerText == "sys_solutiondesc")
                    {
                        string strShowOnTab = LayUtil.GetAttribute(child, "ontab");
                        if (strShowOnTab == "true")
                        {
                            continue;
                        }
                    }

                    if (child.InnerText == "sys_request_id")
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

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_requestdate")
                    {
                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DatePickerDefWidth;
                        }

                        WebDatePicker datePicker = new WebDatePicker();

                        datePicker.Width = new Unit(strWidth + "px");

                        datePicker.ID = "datepicker" + child.Name;
                        datePicker.Nullable = false;
                        datePicker.DisplayModeFormat = "g";
                        datePicker.EditModeFormat = "g";
                        datePicker.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

                        datePicker.Font.Name = child.GetAttribute("font-family");
                        datePicker.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        datePicker.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        datePicker.Style["Top"] = child.GetAttribute("top") + "px";
                        datePicker.Style["Left"] = child.GetAttribute("left") + "px";
                        datePicker.Style["Position"] = "absolute";

                        datePicker.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;
                        //datePicker.
                        
                        

                        //Get Value
                        DateTime dt;
                        if (strVal != "")
                        {
                            dt = DateTime.Parse(strVal);
                        }
                        else
                        {
                            dt = DateTime.Now;
                        }
                        datePicker.Value = dt;

                        ctrlRoot.Controls.Add(datePicker);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            datePicker.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            datePicker.Focus();
                        }

                        if (Session["Role"].ToString() == "administrator")
                        {
                            if (SysDA.GetSettingValue("appallowreqdatechanges", Application) == "3")
                            {
                                datePicker.ReadOnly = true;
                            }
                            else
                            {
                                if (UserReadOnly(child))
                                {
                                    datePicker.ReadOnly = true;
                                }
                            }
                        }
                        else if (Session["Role"].ToString() == "standard")
                        {
                            if (SysDA.GetSettingValue("appallowreqdatechanges", Application) == "" || SysDA.GetSettingValue("appallowreqdatechanges", Application) == "1")
                            {
                                if (UserReadOnly(child))
                                {
                                    datePicker.ReadOnly = true;
                                }
                            }
                            else
                            {
                                datePicker.ReadOnly = true;
                            }
                        }

                    }
                    else if (child.InnerText == "sys_requestclosedate")
                    {
                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DatePickerDefWidth;
                        }

                        WebDatePicker datePicker = new WebDatePicker();

                        datePicker.Width = new Unit(strWidth + "px");
                        datePicker.ID = "datepicker" + child.Name;
                        datePicker.Nullable = false;

                        datePicker.DisplayModeFormat = "g";
                        datePicker.EditModeFormat = "g";
                        datePicker.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

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
                        }
                        else
                        {
                            dt = DateTime.Now;
                        }
                        datePicker.Value = dt;

                        ctrlRoot.Controls.Add(datePicker);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            datePicker.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            datePicker.Focus();
                        }

                        if (UserReadOnly(child))
                        {
                            datePicker.ReadOnly = true;
                        }

                    }
                    else if (child.InnerText == "sys_eusername")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        ctrl.Text = strVal;
                        sys_eusernameH.Text = strVal;
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

                        //ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        ctrlRoot.Controls.Add(ctrl);

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                            ctrlFocus = ctrl;
                        }

                        ctrl.Attributes["onblur"] = "javascript:verify_enduser('" + ctrl.ClientID + "','" + strReqClass + "','" + strReqLinkType + "');return false;";

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.ReadOnly = true;
                            //Create End User Quick Info image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfoenduser('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appenduserterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                        else
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectenduser('" + ctrl.ClientID + "','" + strReqClass + "','" + strReqLinkType + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appenduserterm"];

                            hl.ID = "hl" + child.Name + "seleuser";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                            //Create End User Quick Info image
                            hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfoenduser('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appenduserterm"];

                            hl.ID = "hl" + child.Name + "qukeuser";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_asset_id")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
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


                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);
                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }
                        else
                        {
                            if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                            {
                                ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                            }
                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                        }

                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (SysDA.GetSettingValue("appawenabled", Application) == "true")
                        {
                            if (UserReadOnly(child))
                            {
                                //Create End User Quick Info image
                                HyperLink hl = new HyperLink();
                                hl.NavigateUrl = "javascript:quickinfoasset('" + ctrl.ClientID + "')";

                                hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                                hl.ToolTip = "Quick Info on Asset";

                                hl.ID = "hl" + child.Name;

                                hl.Style["Top"] = child.GetAttribute("top") + "px";
                                hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                                hl.Style["Position"] = "absolute";

                                ctrlRoot.Controls.Add(hl);


                            }
                            else
                            {
                                //Create Select image
                                HyperLink hl = new HyperLink();
                                hl.NavigateUrl = "javascript:selectawasset('" + ctrl.ClientID + "')";

                                hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                                hl.ToolTip = "Select Asset";

                                hl.ID = "hl" + child.Name + "selasset";

                                hl.Style["Top"] = child.GetAttribute("top") + "px";
                                hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                                hl.Style["Position"] = "absolute";

                                ctrlRoot.Controls.Add(hl);

                                //Create End User Quick Info image
                                hl = new HyperLink();
                                hl.NavigateUrl = "javascript:quickinfoasset('" + ctrl.ClientID + "')";

                                hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                                hl.ToolTip = "Quick Info on Asset";

                                hl.ID = "hl" + child.Name + "qukasset";

                                hl.Style["Top"] = child.GetAttribute("top") + "px";
                                hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                                hl.Style["Position"] = "absolute";

                                ctrlRoot.Controls.Add(hl);

                            }
                        }
                    }
                    else if (child.InnerText == "sys_asset_location")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
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


                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }

                        ctrlRoot.Controls.Add(ctrl);
                        //sys_asset_locationH.Text = ctrl.ClientID;
                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }
                        else
                        {
                            if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                            {
                                ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                            }
                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }


                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (SysDA.GetSettingValue("appawenabled", Application) == "true" && !UserReadOnly(child))
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectawassetlocation('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select Asset";

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_requesttype_id")
                    {
                        if (nReqTypeCtrlCnt == 1)
                        {
                            //Create Textbox
                            TextBox ctrl = new TextBox();
                            if (!IsPostBack)
                            {
                                ctrl.Text = strVal;
                                ViewState["sys_requesttype_id"] = strVal;
                            }
                            else
                            {
                                ctrl.Text = sys_requesttype_idH.Text;
                            }
                            ctrl.ID = "dd" + child.Name;

                            string strWidth = child.GetAttribute("width");
                            if (!LayUtil.IsNumeric(strWidth))
                            {
                                strWidth = LayUtil.TextDefWidth;
                            }

                            ctrl.Width = new Unit(strWidth + "px");

                            ctrl.Font.Name = child.GetAttribute("font-family");
                            ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            ctrl.ReadOnly = true;

                            ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                            ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                            ctrl.Style["Position"] = "absolute";

                            string strHeight = child.GetAttribute("height");
                            if (LayUtil.IsNumeric(strHeight))
                            {
                                ctrl.TextMode = TextBoxMode.MultiLine;
                                ctrl.Height = new Unit(strHeight + "px");
                            }
                            if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                            {
                                ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                            }


                            ctrlRoot.Controls.Add(ctrl);

                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                            string strMaxSize = htColSize[child.InnerText].ToString();
                            if (LayUtil.IsNumeric(strMaxSize))
                            {
                                try
                                {
                                    ctrl.MaxLength = int.Parse(strMaxSize);
                                }
                                catch
                                {
                                }
                            }

                            sbTxt.Append("arrCtrlID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(child.InnerText);
                            sbTxt.Append("';");
                            sbTxt.Append("arrClientID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(ctrl.ClientID);
                            sbTxt.Append("';");
                            sbTxt.Append("arrCtrlType[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append("TEXTBOX");
                            sbTxt.Append("';");
                            nLen++;

                            if (!UserReadOnly(child))
                            {
                                //Create Select image
                                HyperLink hl = new HyperLink();
                                hl.NavigateUrl = "javascript:selectrequesttype('" + ctrl.ClientID + "');";

                                hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                                hl.ToolTip = "Select " + Application["apprequesttypeterm"];

                                hl.ID = "hl" + child.Name;

                                hl.Style["Top"] = child.GetAttribute("top") + "px";
                                hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                                hl.Style["Position"] = "absolute";

                                ctrlRoot.Controls.Add(hl);

                            }
                        }
                        else
                        {
                            WebDropDown ctrl = new WebDropDown();
                            ctrl.ID = "dd" + child.InnerText + nReqTypeIndex;

                            if (!IsPostBack)
                            {
                                ViewState["sys_requesttype_id"] = strVal;
                            }
                            else
                            {
                                strVal = sys_requesttype_idH.Text;
                            }

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

                            ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                            ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                            ctrl.Style["Position"] = "absolute";
                            //ctrl.Style["Z-Index"] = (20010 - nReqTypeIndex).ToString();
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

                            sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));

                            //ctrl.Font.Name = child.GetAttribute("font-family");
                            //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                            ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                            ctrl.Attributes["Index"] = nReqTypeIndex.ToString();

                            //Set list items
                            SetDDReqType(ctrl, strVal, nReqTypeIndex);
                            nReqTypeIndex++;

                            ctrl.ValueChanged += new DropDownValueChangedEventHandler(ReqTypeSel_ValueChanged);
                            ctrl.AutoPostBack = true;
                            ctrl.AutoPostBackFlags.ValueChanged = AutoPostBackFlag.On;

                            int iCount1 = 0;
                            iCount1 = ctrl.Items.Count;

                            ctrlRoot.Controls.Add(ctrl);

                            //////////////////////////////////////////////
                            int iCount2 = ctrl.Items.Count;
                            int iRemoveIndex = iCount1;
                            if ((iCount2 > iCount1) && (iCount1 != 0))
                            {
                                int iCurrentCount = iCount2;
                                while (iRemoveIndex < iCurrentCount)
                                {
                                    ctrl.Items.RemoveAt(iRemoveIndex);
                                    iCurrentCount = ctrl.Items.Count;
                                }
                            }
                            ///////////////////////////////////////////////////////
                            sbTxt.Append("arrCtrlID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(child.InnerText);
                            sbTxt.Append("';");
                            sbTxt.Append("arrClientID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(ctrl.ClientID);
                            sbTxt.Append("';");
                            sbTxt.Append("arrCtrlType[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append("DD");
                            sbTxt.Append("';");
                            nLen++;

                            if (UserReadOnly(child))
                            {
                                ctrl.Enabled = false;
                            }

                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                        }
                    }
                    else if (child.InnerText == "sys_siteid")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        ctrl.Text = sys_siteidH.Text;
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }

                        ctrlRoot.Controls.Add(ctrl);
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }


                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            //Create End User Quick Info image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfosite('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                        else
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectsite('" + ctrl.ClientID + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name + "selsite";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                            //Create End User Quick Info image
                            hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfosite('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name + "quksite";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_company_id")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_company_idH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }

                        ctrlRoot.Controls.Add(ctrl);


                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        /*
                        if (UserReadOnly(child))
                        {
                            //Create End User Quick Info image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfocompany('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                        else
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectsite('" + ctrl.ClientID + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name + "selsite";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                            //Create End User Quick Info image
                            hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfosite('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Quick Info on " + Application["appsiteterm"];

                            hl.ID = "hl" + child.Name + "quksite";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                         */ 
                    }
                    else if (child.InnerText == "sys_solution_id")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_solution_idH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        if (!UserReadOnly(child))
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            if (ViewState["sys_request_id"] != null)
                            {
                                hl.NavigateUrl = "javascript:selectsolution('" + ctrl.ClientID + "','sys_solutiondesc','" + ViewState["sys_request_id"].ToString() + "')";
                            }
                            else
                            {
                                hl.NavigateUrl = "javascript:selectsolution('" + ctrl.ClientID + "','sys_solutiondesc','')";
                            }

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appsolutionterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);


                        }
                    }
                    else if (child.InnerText == "sys_assignedto")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_assignedtoH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);

                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        string strReqAssgn = GetUserVal("sys_requestassign");
                        if (!UserReadOnly(child) && strReqAssgn == "0")
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectuser('" + ctrl.ClientID + "','yes');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appuserterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);


                        }
                    }
                    else if (child.InnerText == "sys_ownedby")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_ownedbyH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;

                        string strOwnReq = GetUserVal("sys_requestowner");
                        if (!UserReadOnly(child) && strOwnReq == "0")
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectuser('" + ctrl.ClientID + "','no');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appuserterm"];

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_assignedtoanalgroup")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_assignedtoanalgroupH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);
                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;


                        if (!UserReadOnly(child))
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selectanalgroup('" + ctrl.ClientID + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appuserterm"] + " Group";

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_requestpriority")
                    {
                        WebDropDown ctrl = new WebDropDown();
                        ctrl.ID = "dd" + child.Name;

                        ctrl.ClientEvents.ValueChanged = "priorityChange";

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

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDPriority(ctrl, strVal);



                        if (strVal != "")
                        {
                            ctrl.SelectedValue = strVal;
                        }

                        ctrlRoot.Controls.Add(ctrl);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("DD");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_requestparent_id")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_requestparent_idH.Text;
                        }
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

                        ctrl.ReadOnly = true;

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.TextMode = TextBoxMode.MultiLine;
                            ctrl.Height = new Unit(strHeight + "px");
                        }
                        ctrlRoot.Controls.Add(ctrl);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                        string strMaxSize = htColSize[child.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;


                        if (UserReadOnly(child))
                        {
                            //Create End User Quick Info image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ctrl.ClientID + "')";

                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Link Information";

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                        else
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            if (ViewState["sys_request_id"] != null)
                            {
                                hl.NavigateUrl = "javascript:selectlinkrequest('" + ctrl.ClientID + "','" + ViewState["sys_request_id"].ToString() + "')";
                            }
                            else
                            {
                                hl.NavigateUrl = "javascript:selectlinkrequest('" + ctrl.ClientID + "','')";
                            }

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["apprequestterm"];

                            hl.ID = "hl" + child.Name + "sellkreq";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                            //Create End User Quick Info image
                            hl = new HyperLink();
                            if (ViewState["sys_request_id"] != null)
                            {
                                hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ctrl.ClientID + "','" + ViewState["sys_request_id"].ToString() + "')";
                            }
                            else
                            {
                                hl.NavigateUrl = "javascript:quickinfolinkspawn('" + ctrl.ClientID + "')";
                            }
                            hl.ImageUrl = "Application_Images/16x16/Info_16px.png";
                            hl.ToolTip = "Link Information";

                            hl.ID = "hl" + child.Name + "quklinreq";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_blocksurvey")
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


                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;


                        //Set list items
                        DropDownItem item = new DropDownItem("Off", "0");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("On", "1");
                        ctrl.Items.Add(item);

                        if (strVal == "1")
                        {
                            ctrl.SelectedValue = "1";
                        }
                        else
                        {
                            ctrl.SelectedValue = "0";
                        }
                        ctrlRoot.Controls.Add(ctrl);


                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("DD");
                        sbTxt.Append("';");
                        nLen++;

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_impact")
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


                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDImpact(ctrl, strVal);

                        ctrlRoot.Controls.Add(ctrl);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("DD");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_urgency")
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


                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDUrgency(ctrl, strVal);

                        ctrlRoot.Controls.Add(ctrl);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("DD");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_itservice")
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


                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDITService(ctrl, strVal);

                        ctrlRoot.Controls.Add(ctrl);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(child.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("DD");
                        sbTxt.Append("';");
                        nLen++;

                        if (UserReadOnly(child))
                        {
                            ctrl.Enabled = false;
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else
                    {
                        if (child.GetAttribute("htmleditor") == "true")
                        {
                            RadEditor ctrl = LayUtil.CreateHTMLEditor(Page, child);
                            if (IsiPhone())
                            {
                                ctrl.EditModes = EditModes.Html;
                            }
                            
                            ctrl.Content = strVal;
                            ctrl.OnClientCommandExecuting = "OnClientCommandExecuting";
                            //ctrl.ContentAreaMode = EditorContentAreaMode.Div;
                            ctrl.OnClientLoad = "OnClientLoad";
                            ctrl.OnClientModeChange = "OnClientModeChange";

                            HtmlGenericControl div = new HtmlGenericControl("div");
                            div.Style["Top"] = child.GetAttribute("top") + "px";
                            div.Style["Left"] = child.GetAttribute("left") + "px";
                            div.Style["Position"] = "absolute";

                            div.Controls.Add(ctrl);
                            ctrlRoot.Controls.Add(div);

                            sbTxt.Append("arrCtrlID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(child.InnerText);
                            sbTxt.Append("';");
                            sbTxt.Append("arrClientID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(ctrl.ClientID);
                            sbTxt.Append("';");
                            sbTxt.Append("arrCtrlType[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append("HTML");
                            sbTxt.Append("';");
                            sbTxt.Append("arrCtrlSize[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(htColSize[child.InnerText].ToString());
                            sbTxt.Append("';");
                            nLen++;

                            if (UserReadOnly(child))
                            {
                                ctrl.Enabled = false;
                            }

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

                            string strHeight = child.GetAttribute("height");
                            if (LayUtil.IsNumeric(strHeight))
                            {
                                ctrl.TextMode = TextBoxMode.MultiLine;
                                ctrl.Height = new Unit(strHeight + "px");
                            }
                            string strMaxSize = htColSize[child.InnerText].ToString();
                            if (LayUtil.IsNumeric(strMaxSize))
                            {
                                try
                                {
                                    ctrl.MaxLength = int.Parse(strMaxSize);
                                }
                                catch
                                {
                                }
                            }

                            ctrlRoot.Controls.Add(ctrl);


                            sbTxt.Append("arrCtrlID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(child.InnerText);
                            sbTxt.Append("';");
                            sbTxt.Append("arrClientID[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append(ctrl.ClientID);
                            sbTxt.Append("';");
                            sbTxt.Append("arrCtrlType[");
                            sbTxt.Append(nLen.ToString());
                            sbTxt.Append("]='");
                            sbTxt.Append("TEXTBOX");
                            sbTxt.Append("';");
                            nLen++;
                            if (UserReadOnly(child))
                            {
                                ctrl.Enabled = false;
                            }

                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                            /*
                            if (child.InnerText == "sys_problemdesc")
                            {
                                
                                RadEditor edtHelp = new RadEditor();
                                edtHelp.Content = strVal;
                                ctrl.Text = edtHelp.Text;
                                
                                ctrl.Text = strVal;
                            }
                            */
                        }
                    }
                }
            }

        }

        sbDDCss.AppendLine("</style>");

        StyleArea.InnerHtml = sbDDCss.ToString();

        sbTxt.Append("var ctrlno=");
        sbTxt.Append(nLen.ToString());
        sbTxt.Append(";");
        txtArray.Text = sbTxt.ToString();

        if (ctrlFocus != null)
        {
            ctrlFocus.Focus();
        }
    }

    /// <summary>
    /// Check whether it's readonly for current user
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool UserReadOnly(XmlElement node)
    {
        bool bRes = false;

        if (node.GetAttribute("fieldreadonly") == "true")
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Length >= 12 && child.Name.Substring(0, 12).ToLower() == "readonlyuser")
                {
                    if (LayUtil.GetAttribute(child, "user").ToLower() == Session["User"].ToString().ToLower())
                    {
                        bRes = true;
                        break;
                    }
                }
            }
        }

        return bRes;
    }

    /// <summary>
    /// Show message in popup windows
    /// </summary>
    /// <param name="strMsg"></param>
    private void ShowMsg(string strMsg)
    {
        lbMsg.Text = strMsg;
        dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
        dialogMsg.MaintainLocationOnScroll = false;
        dialogMsg.InitialLocation = Infragistics.Web.UI.LayoutControls.DialogWindowPosition.Centered;
    }

    private string GetUserInputText(string strField)
    {
        if (bRule)
        {
            if (htBRVal != null && htBRVal[strField] != null)
            {
                if (htCtrlNode[strField] != null)
                {
                    if (!IgnoreBRVal((XmlElement)htCtrlNode[strField]))
                    {
                        return htBRVal[strField].ToString();
                    }
                }
            }
        }


        string strVal = "";
        Control ctrlRoot = phCtrls;

        if (strField == "sys_siteid")
        {
            return sys_siteidH.Text;
        }
        else if (strField == "sys_solution_id")
        {
            return sys_solution_idH.Text;
        }
        else if (strField == "sys_requesttype_id")
        {
            return sys_requesttype_idH.Text;
        }
        else if (strField == "sys_assignedto")
        {
            return sys_assignedtoH.Text;
        }
        else if (strField == "sys_ownedby")
        {
            return sys_ownedbyH.Text;
        }
        else if (strField == "sys_assignedtoanalgroup")
        {
            return sys_assignedtoanalgroupH.Text;
        }
        else if (strField == "sys_requestparent_id")
        {
            return sys_requestparent_idH.Text;
        }

        if (htChildName[strField] != null)
        {
            TextBox ctrl = (TextBox)ctrlRoot.FindControl("text" + htChildName[strField].ToString());
            if (ctrl != null)
            {
                strVal = ctrl.Text;
            }
            else
            {
                //Get Original Value
                strVal = GetValue(strField);
            }
        }
        return strVal;
    }
    private string GetUserInputDD(string strField)
    {
        if (bRule)
        {
            //if Priority and Priority to override BR
            if (strField == "sys_requestpriority" && htCtrl["sys_requestpriority"] != null && ViewState["sys_request_id"].ToString() != "" && SysDA.GetSettingValue("apppopupwhenprioritychange", Application) == "true")
            {
                //not apply BR
            }
            else
            {
                if (htBRVal != null && htBRVal[strField] != null)
                {
                    if (htCtrlNode[strField] != null)
                    {
                        if (!IgnoreBRVal((XmlElement)htCtrlNode[strField]))
                        {
                            return htBRVal[strField].ToString();
                        }
                    }
                }
            }
        }
        string strVal = "";
        Control ctrlRoot = phCtrls;

        if (htChildName[strField] != null)
        {
            WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + htChildName[strField].ToString());
            if (ctrl != null)
            {

                //bug 1032
                //if (ctrl.SelectedItem.Value == "" && hfpriority.Value != "")
                //    strVal = hfpriority.Value;
                //else
                    strVal = ctrl.SelectedItem.Value;
            }
            else
            {
                //Get Original Value
                strVal = GetValue(strField);
            }
        }
        return strVal;
    }
    private string GetUserInputDate(string strField)
    {
        if (bRule)
        {
            if (htBRVal != null && htBRVal[strField] != null)
            {
                if (htCtrlNode[strField] != null)
                {
                    if (!IgnoreBRVal((XmlElement)htCtrlNode[strField]))
                    {
                        return htBRVal[strField].ToString();
                    }
                }
            }
        }
        string strVal = "";
        Control ctrlRoot = phCtrls;

        if (htChildName[strField] != null)
        {
            WebDatePicker datePicker = (WebDatePicker)ctrlRoot.FindControl("datepicker" + htChildName[strField].ToString());

            if (datePicker != null)
            {
                strVal = datePicker.Text;
            }
            else
            {
                //Get Original Value
                strVal = GetValue(strField);
            }

        }
        return strVal;
    }
    private string GetUserInput(XmlElement child)
    {
        string strField = child.InnerText;

        if (bRule)
        {
            //if Priority and Priority to override BR
            if (strField == "sys_requestpriority" && htCtrl["sys_requestpriority"] != null && ViewState["sys_request_id"].ToString() != "" && SysDA.GetSettingValue("apppopupwhenprioritychange", Application) == "true")
            {
                //not apply BR
            }
            else
            {
                if (htBRVal != null && htBRVal[strField] != null)
                {
                    if(!IgnoreBRVal(child))
                    {
                        return htBRVal[strField].ToString();
                    }
                }
            }
        }

        if (strField == "sys_siteid")
        {
            return sys_siteidH.Text;
        }
        else if (strField == "sys_solution_id")
        {
            return sys_solution_idH.Text;
        }
        else if (strField == "sys_requesttype_id")
        {
            return sys_requesttype_idH.Text;
        }
        else if (strField == "sys_assignedto")
        {
            return sys_assignedtoH.Text;
        }
        else if (strField == "sys_ownedby")
        {
            return sys_ownedbyH.Text;
        }
        else if (strField == "sys_assignedtoanalgroup")
        {
            return sys_assignedtoanalgroupH.Text;
        }
        else if (strField == "sys_requestparent_id")
        {
            return sys_requestparent_idH.Text;
        }

        if (htCols[child.InnerText] == null)
            return "";

        string strVal = "";
        Control ctrlRoot = phCtrls;
        if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field")
        {
            if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "fieldc")
            {
                WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                if (ctrl != null)
                {
                    strVal = ctrl.SelectedItem.Value;
                }
            }
            else
            {
                if (child.GetAttribute("htmleditor") == "true")
                {
                    RadEditor ctrl = (RadEditor)ctrlRoot.FindControl("html" + child.Name);

                    if (ctrl != null)
                    {
                        strVal = ctrl.Content;
                    }

                }
                //else if (DataDesignDA.GetFieldType("request", child.InnerText) == "DateTime")
                else if (htCols[child.InnerText].ToString() == "DateTime")
                {
                    WebDatePicker datePicker = (WebDatePicker)ctrlRoot.FindControl("datepicker" + child.Name);

                    if (datePicker != null)
                    {
                        strVal = datePicker.Text;
                    }
                    else
                    {
                        //Get Original Value
                        strVal = GetValue(strField);
                    }

                }
                else
                {
                    TextBox ctrl = (TextBox)ctrlRoot.FindControl("text" + child.Name);
                    if (ctrl != null)
                    {
                        strVal = ctrl.Text;
                    }

                }
            }
        }
        else
        {
            if (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_")
            {
                if (child.InnerText == "sys_problemdesc" || child.InnerText == "sys_solutiondesc")
                {
                    if (child.GetAttribute("ontab") == "true")
                    {

                        if (bShowTab)
                        {
                            WebTab tabMain = (WebTab)ctrlRoot.FindControl("tabMain");
                            if (tabMain != null)
                            {
                                foreach (ContentTabItem tab in tabMain.Tabs)
                                {
                                    if (tab.ImageAltText == child.Name)
                                    {
                                        if (child.GetAttribute("htmleditor") == "true")
                                        {
                                            RadEditor ctrl = (RadEditor)tab.FindControl("html" + child.Name);

                                            if (ctrl != null)
                                            {
                                                strVal = ctrl.Content; ;
                                            }

                                        }
                                        else
                                        {
                                            TextBox ctrl = (TextBox)tab.FindControl("text" + child.Name);
                                            if (ctrl != null)
                                            {
                                                strVal = ctrl.Text;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (child.GetAttribute("htmleditor") == "true")
                        {
                            RadEditor ctrl = (RadEditor)ctrlRoot.FindControl("html" + child.Name);

                            if (ctrl != null)
                            {
                                strVal = ctrl.Content;
                            }

                        }
                        else
                        {
                            TextBox ctrl = (TextBox)ctrlRoot.FindControl("text" + child.Name);
                            if (ctrl != null)
                            {
                                strVal = ctrl.Text;
                            }
                        }
                    }

                }
                else if (child.InnerText == "sys_requestpriority")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null && ctrl.SelectedItem != null)
                    {
                        strVal = ctrl.SelectedItem.Value;
                        if (strVal == "(None)")
                        {
                            strVal = "";
                        }
                    }
                }
                else if (child.InnerText == "sys_impact" || child.InnerText == "sys_urgency" || child.InnerText == "sys_itservice" || child.InnerText == "sys_blocksurvey")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null)
                    {
                        strVal = ctrl.SelectedItem.Value;
                    }
                }
                else if (child.InnerText == "sys_requestdate" || child.InnerText == "sys_requestclosedate")
                {
                    WebDatePicker datePicker = (WebDatePicker)ctrlRoot.FindControl("datepicker" + child.Name);

                    if (datePicker != null)
                    {
                        strVal = datePicker.Text;
                    }
                }
                else
                {
                    if (child.GetAttribute("htmleditor") == "true")
                    {
                        RadEditor ctrl = (RadEditor)ctrlRoot.FindControl("html" + child.Name);

                        if (ctrl != null)
                        {
                            strVal = ctrl.Content;
                        }

                    }
                    else
                    {
                        TextBox ctrl = (TextBox)ctrlRoot.FindControl("text" + child.Name);
                        if (ctrl != null)
                        {
                            strVal = ctrl.Text;
                        }
                    }
                }
            }

        }


        return strVal;
    }

    /// <summary>
    /// Check user inputs against db schema
    /// </summary>
    /// <returns></returns>
    private bool CheckInput()
    {
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = GetUserInput(child);
            //Check PK
            if (child.InnerText == "sys_request_id")
            {
                strNewPK = strVal;

            }

            if (child.InnerText == "sys_eusername")
            {
                //Check required field
                if (child.GetAttribute("userreq") == "true" && strVal == "")
                {
                    ShowMsg("You must enter a value in the " + child.GetAttribute("caption") + " box.");
                    return false;
                }

                if (strVal != "")
                {
                    DataSet dsEUser = UserDA.GetEUserInfo(strVal);
                    if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                    {
                    }
                    else
                    {
                        ShowMsg("You must enter a valid value in " + Application["appenduserterm"] + " box.");
                        return false;
                    }
                }
            }
            //only check sys_field and field
            if (!(child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") && !(child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                continue;
            }

            //Check all other fields type
            if (htCols[child.InnerText] == null)
            {
                continue;
            }

            string strType = htCols[child.InnerText].ToString();
            if (strType == "Integer")
            {
                if (child.InnerText == "sys_request_timespent")
                {
                    if (strVal != "" && !CheckDuration(strVal))
                    {
                        ShowMsg("You must enter a valid duration (format specified in settings) in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }
                }
                else if (strVal != "" && child.InnerText != "sys_request_id")
                {
                    if (!LayUtil.IsNumeric(strVal))
                    {
                        ShowMsg("You must enter a numeric value in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }
                    else
                    {
                        int nVal = int.Parse(strVal);
                        if (nVal > 10000000 || nVal < -10000000)
                        {
                            ShowMsg("You must enter a numeric value between -10,000,000 and 10,000,000 in the " + child.GetAttribute("caption") + " box.");
                            return false;
                        }
                    }
                }
            }
            else if (strType == "Decimal")
            {
                if (strVal != "" && !LayUtil.IsNumeric(strVal))
                {
                    ShowMsg("You must enter a numeric value in the " + child.GetAttribute("caption") + " box.");
                    return false;
                }
            }
            else if (strType == "DateTime")
            {
                //The control itself can guarantee correct format
                ;
            }
            else if (strType == "Text")
            {
                string strSize = "";
                if (htColSize[child.InnerText] != null)
                {
                    strSize = htColSize[child.InnerText].ToString();
                }
                if (LayUtil.IsNumeric(strSize))
                {
                    if (strVal.Length > int.Parse(strSize))
                    {
                        ShowMsg("You can only enter a maximum of " + strSize + " characters in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }
                }
            }

            //Check required field

            //// written & Changed by Sparsh ID 175        
                if (child.GetAttribute("userreq") == "true")
                 {
                     if (strVal == "" && child.InnerText == "sys_requesttype_id")
                    {
                        ShowMsg("You must enter a value in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }

                     else if ((strVal.Contains("/") == false) && child.InnerText == "sys_requesttype_id")
            {

                ShowMsg("You must enter a value in the " + child.GetAttribute("caption") + " box.");
                return false;

            }

        }
            // End by Sparsh ID 175
        }
        return true;
    }

    /// <summary>
    /// Generate Insert SqlCommand
    /// </summary>
    /// <returns>SqlCommand generated</returns>
    private SqlCommand GetInsCmd()
    {
        if ((htCtrl["sys_assignedto"] == null && htCtrl["sys_assignedtoanalgroup"] == null) || (GetUserInputText("sys_assignedto") == "" && GetUserInputText("sys_assignedtoanalgroup") == ""))
        //if(GetUserInputText("sys_assignedto") == "" && GetUserInputText("sys_assignedtoanalgroup") == "")
        {
            GetSuggestedUser();
        }

        //Check if Assignedto belongs to Assignedtogroup; if not, clear one based on Business rule
        VerifyAssignedTo();

        string strLT = "";
        if (strReqLinkType != "")
        {
            strLT = strReqLinkType;
        }

        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        StringBuilder sbSql = new StringBuilder();
        StringBuilder sbSqlVal = new StringBuilder();
        //string strSql;

        //Insert
        bool bReqTypeFirst = true;

        //Some fields in htBR may not show on form, so need to set it seperately
        Hashtable htFields = new Hashtable();

        sbSql.Append("INSERT INTO request (sys_requestclass_id, sys_source, sys_requestlinktype, sys_requeststatus, sys_createdby, sys_request_lastupdate ");
        sbSqlVal.Append(" VALUES(@strReqClass,'Web {{user}}',@strLT,@strReqStatus, @sys_createdby, @sys_requst_lastupdate");

        htFields["sys_requestclass_id"] = "true";
        htFields["sys_source"] = "true";
        htFields["sys_requestlinktype"] = "true";
        htFields["sys_requeststatus"] = "true";
        htFields["sys_createdby"] = "true";
        htFields["sys_request_lastupdate"] = "true";

        //Request Class
        string strEffectReqClass = strReqClass;
        if (htBRVal["sys_requestclass_id"] != null)
        {
            string strBRReqClassId = htBRVal["sys_requestclass_id"].ToString();
            if (LibReqClassBR.ReqClassExist(strBRReqClassId))
            {
                strEffectReqClass = strBRReqClassId;
            }

        }


        parameter = new SqlParameter();
        parameter.ParameterName = "@strReqClass";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = strEffectReqClass;
        cmd.Parameters.Add(parameter);

        parameter = new SqlParameter();
        parameter.ParameterName = "@strLT";
        parameter.Direction = ParameterDirection.Input;
        if (strLT == "")
        {
            parameter.Value = DBNull.Value;
        }
        else
        {
            parameter.Value = strLT;
        }
        cmd.Parameters.Add(parameter);

        string strReqStatus = Application["appdefaultstatus"].ToString();
        if (htBRVal["sys_requeststatus"] != null)
        {
            strReqStatus = htBRVal["sys_requeststatus"].ToString();
        }

        parameter = new SqlParameter();
        parameter.ParameterName = "@strReqStatus";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = strReqStatus;
        cmd.Parameters.Add(parameter);

        cmd.Parameters.AddWithValue("@sys_createdby", "{{user}}");
        cmd.Parameters.AddWithValue("@sys_requst_lastupdate", DateTime.Now);

        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_request_id")
                continue;

            if (child.InnerText == "sys_requesttype_id")
            {
                if (!bReqTypeFirst)
                {
                    continue;
                }
                bReqTypeFirst = false;
            }

            if (child.InnerText == "sys_requestpriority")
            {
                continue;
            }
            
            string strVal = GetUserInput(child);
            if (child.InnerText == "sys_request_timespent" && strVal != "")
            {
                strVal = RequestDA.Time2Mins(strVal);
            }

            if (child.InnerText == "sys_eusername")
            {
                DataSet dsEUser = UserDA.GetEUserInfo(strVal);

                if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                {
                    strVal = dsEUser.Tables[0].Rows[0]["sys_eusername"].ToString();
                }
            }

            if (SysDA.GetSettingValue("appcompanylevel", Application) == "true" && SysDA.GetSettingValue("appdispcompanyseperately", Application) == "true")
            {
                if (child.InnerText == "sys_siteid")
                {
                    if (sys_company_idH.Text != "")
                    {
                        strVal = sys_company_idH.Text + "/" + strVal;
                    }
                }
            }
            if ((child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") || (child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                htFields[child.InnerText] = "true";
                sbSql.Append(",[" + child.InnerText + "]");

                if (strVal == "")
                {
                    sbSqlVal.Append(",NULL");
                }
                else
                {
                    sbSqlVal.Append(",@" + child.Name);

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (child.InnerText == "sys_requestdate" || child.InnerText == "sys_requestclosedate" || htCols[child.InnerText].ToString() == "DateTime")
                    {
                        parameter.Value = DateTime.Parse(strVal);
                    }
                    else
                    {
                        parameter.Value = LayUtil.RplTm(Application, strVal);
                    }
                    cmd.Parameters.Add(parameter);
                }

            }
        }

        if (htCtrl["sys_assignedto"] == null)
        {
            if (htCtrl["sys_requesttype_id"] != null)
            {
                string strReqType = GetUserInput((XmlElement)htCtrlNode["sys_requesttype_id"]);
                if (strReqType != "")
                {
                    string strEUser = GetUserInputText("sys_eusername");
                    DataSet dsEUser = null;
                    if (strEUser != "")
                    {
                        dsEUser = UserDA.GetEUserInfo(strEUser);
                    }

                    DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(strReqType);
                    if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsReqType.Tables[0].Rows[0];

                        string strAssign = "";
                        string strAssignType = dr["sys_requesttype_assigntype"].ToString();
                        if (strAssignType == "1")
                        {
                            DataSet dsSite = null;
                            string strSiteId = "";
                            if (htCtrl["sys_siteid"] == null)
                            {
                                if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                                {
                                    strSiteId = dsEUser.Tables[0].Rows[0]["sys_siteid"].ToString();
                                }
                            }
                            else
                            {
                                strSiteId = GetUserInputText("sys_siteid");
                            }


                            if (strSiteId != "")
                            {
                                dsSite = UserDA.GetSitesById(strSiteId);
                                if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
                                {
                                    strAssign = dsSite.Tables[0].Rows[0]["sys_site_username"].ToString();
                                }
                            }
                        }
                        else if (strAssignType == "2")
                        {
                            if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                            {
                                strAssign = dsEUser.Tables[0].Rows[0]["sys_eclient_username"].ToString();
                            }
                        }
                        else if (strAssignType == "3")
                        {
                            strAssign = dr["sys_requesttype_assignto"].ToString();
                        }

                        if (strAssign != "")
                        {
                            htFields["sys_assignedto"] = "true";

                            sbSql.Append(", [sys_assignedto]");
                            sbSqlVal.Append(",@sys_assignedto");

                            parameter = new SqlParameter();
                            parameter.ParameterName = "@sys_assignedto";
                            parameter.Direction = ParameterDirection.Input;
                            parameter.Value = strAssign;
                            cmd.Parameters.Add(parameter);
                        }

                    }

                }
            }
        }
        if (htCtrl["sys_urgency"] == null)
        {
            string strVal = SysDA.GetSettingValue("appdefaulturgency", Application);

            htFields["sys_urgency"] = "true";
            sbSql.Append(",[sys_urgency]");

            if (strVal == "")
            {
                sbSqlVal.Append(",NULL");
            }
            else
            {
                sbSqlVal.Append(",@sys_urgency");

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_urgency";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);
            }
        }
        if (htCtrl["sys_itservice"] == null)
        {
            string strVal = SysDA.GetSettingValue("appdefaultitservice", Application);

            htFields["sys_itservice"] = "true";
            sbSql.Append(",[sys_itservice]");

            if (strVal == "")
            {
                sbSqlVal.Append(",NULL");
            }
            else
            {
                sbSqlVal.Append(",@sys_itservice");

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_itservice";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);
            }
        }
        if (htCtrl["sys_impact"] == null)
        {
            string strVal = SysDA.GetSettingValue("appdefaultimpact", Application);

            htFields["sys_impact"] = "true";
            sbSql.Append(",[sys_impact]");

            if (strVal == "")
            {
                sbSqlVal.Append(",NULL");
            }
            else
            {
                sbSqlVal.Append(",@sys_impact");

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_impact";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);
            }
        }

        string strPriority = "";
        if (htBRVal["sys_requestpriority"] == null)
        {
            if (htCtrl["sys_requestpriority"] == null)
            {
                string strTempReqType = GetUserInputText("sys_requesttype_id");
                if (htCtrl["sys_requesttype_id"] == null || strTempReqType == "")
                {
                    strPriority = Application["appdefaultpriority"].ToString();
                }
                else
                {
                    DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(strTempReqType);
                    if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                    {
                        strPriority = dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString();
                    }
                }

            }
            else
            {
                strPriority = GetUserInputDD("sys_requestpriority");
            }
        }
        else
        {
            strPriority = htBRVal["sys_requestpriority"].ToString();
        }

        if (strPriority != "")
        {
            htFields["sys_requestpriority"] = "true";
            sbSql.Append(", [sys_requestpriority]");
            sbSqlVal.Append(",@sys_requestpriority");

            parameter = new SqlParameter();
            parameter.ParameterName = "@sys_requestpriority";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = strPriority;
            cmd.Parameters.Add(parameter);
        }

        bool bPriorityEdit = false;
        DataSet dsPri = null;
        if (strPriority != "")
        {
            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (dr["sys_allowedit"].ToString() == "1")
                {
                    bPriorityEdit = true;
                }

            }
        }

        if (!bPriorityEdit)
        {
            double dReqReslvHr = 0;
            double dReqEsc1Hr = 0;
            double dReqEsc2Hr = 0;
            double dReqEsc3Hr = 0;
            double dReqResHr = 0;


            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (LayUtil.IsNumeric(dr["sys_resolvehours"].ToString()))
                {
                    dReqReslvHr = double.Parse(dr["sys_resolvehours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate1hours"].ToString()))
                {
                    dReqEsc1Hr = double.Parse(dr["sys_escalate1hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate2hours"].ToString()))
                {
                    dReqEsc2Hr = double.Parse(dr["sys_escalate2hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate3hours"].ToString()))
                {
                    dReqEsc3Hr = double.Parse(dr["sys_escalate3hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_respondhours"].ToString()))
                {
                    dReqResHr = double.Parse(dr["sys_respondhours"].ToString());
                }
            }

            string strBegin = GetUserInputDate("sys_requestdate");
            string strSiteId = GetUserInputText("sys_siteid");

            if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0 || dReqResHr != 0) && strBegin != "")
            {
                string strOrgBegin = GetValue("sys_requestdate");
                DateTime dtBegin = DateTime.Parse(strBegin);
                DateTime dtOrgBegin = DateTime.Parse(strOrgBegin);

                //if (strSiteId != GetValue("sys_siteid") || DateTime.Compare(dtBegin, dtOrgBegin) != 0)
                {
                    LoadSus("", "", strSiteId, strBegin);
                }


                if (dReqReslvHr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        htFields["sys_resolve"] = "true";
                        sbSql.Append(", [sys_resolve]");
                        sbSqlVal.Append(",@sys_resolve");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_resolve";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                }

                if (dReqEsc1Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        htFields["sys_escalate1"] = "true";
                        sbSql.Append(", [sys_escalate1]");
                        sbSqlVal.Append(", @sys_escalate1");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate1";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                }

                if (dReqEsc2Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc2Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        htFields["sys_escalate2"] = "true";
                        sbSql.Append(", [sys_escalate2]");
                        sbSqlVal.Append(", @sys_escalate2");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate2";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                }

                if (dReqEsc3Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc3Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        htFields["sys_escalate3"] = "true";
                        sbSql.Append(", [sys_escalate3]");
                        sbSqlVal.Append(", @sys_escalate3");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate3";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                }

                if (dReqResHr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqResHr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        htFields["sys_respond"] = "true";
                
                        sbSql.Append(", [sys_respond]");
                        sbSqlVal.Append(", @sys_respond");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_respond";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                }
            }
        }

        foreach (string strField in htBRVal.Keys)
        {
            if (htFields[strField] == null)
            {
                sbSql.Append(", " + strField);
                sbSqlVal.Append(", @" + strField);

                string strVal = htBRVal[strField].ToString();

                if (strVal == "")
                {
                    cmd.Parameters.AddWithValue("@" + strField, DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@" + strField, strVal);
                }
            }
        }

        sbSql.Append(")" + sbSqlVal.ToString() + ")");

        cmd.CommandText = sbSql.ToString();

        return cmd;
    }

    /// <summary>
    /// Generate Update SqlCommand
    /// </summary>
    /// <returns>SqlCommand generated</returns>
    private SqlCommand GetUpdCmd()
    {
        //Check if Assignedto belongs to Assignedtogroup; if not, clear one based on Business rule
        VerifyAssignedTo();

        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        StringBuilder sbSql = new StringBuilder();

        Hashtable htFields = new Hashtable();

        //Update
        bool bReqTypeFirst = true;
        sbSql.Append("UPDATE [request] SET sys_request_lastupdate=@sys_requst_lastupdate ");
        cmd.Parameters.AddWithValue("@sys_requst_lastupdate", DateTime.Now);

        htFields["sys_request_lastupdate"] = "true";

        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_request_id")
                continue;

            if (child.InnerText == "sys_requesttype_id")
            {
                if (!bReqTypeFirst)
                {
                    continue;
                }
                bReqTypeFirst = false;
            }


            string strVal = GetUserInput(child);

            if (child.InnerText == "sys_request_timespent" && strVal != "")
            {
                strVal = RequestDA.Time2Mins(strVal);
            }

            if (child.InnerText == "sys_eusername")
            {
                DataSet dsEUser = UserDA.GetEUserInfo(strVal);

                if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                {
                    strVal = dsEUser.Tables[0].Rows[0]["sys_eusername"].ToString();
                }
            }

            if (SysDA.GetSettingValue("appcompanylevel", Application) == "true" && SysDA.GetSettingValue("appdispcompanyseperately", Application) == "true")
            {
                if (child.InnerText == "sys_siteid")
                {
                    if (sys_company_idH.Text != "")
                    {
                        strVal = sys_company_idH.Text + "/" + strVal;
                    }
                }
            }
            
            if ((child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") || (child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                htFields[child.InnerText] = "true";

                if (strVal == "")
                {
                    sbSql.Append(",[");
                    sbSql.Append(child.InnerText);
                    sbSql.Append("]=NULL");
                }
                else
                {
                    sbSql.Append(",[");
                    sbSql.Append(child.InnerText);
                    sbSql.Append("]=@");
                    sbSql.Append(child.Name);

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (child.InnerText == "sys_requestdate" || child.InnerText == "sys_requestclosedate" || htCols[child.InnerText].ToString() == "DateTime")
                    {
                        parameter.Value = DateTime.Parse(strVal);
                    }
                    else
                    {
                        parameter.Value = LayUtil.RplTm(Application, strVal);
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        bool bPriorityEdit = false;
        string strPriority = "";
        if (htCtrl["sys_requestpriority"] != null)
        {
            strPriority = GetUserInputDD("sys_requestpriority");
        }
        else
        {
            if (htBRVal["sys_requesttype_id"] != null)
            {
                strPriority = htBRVal["sys_requesttype_id"].ToString();
            }
            else if (htCtrl["sys_requesttype_id"] == null || GetUserInputText("sys_requesttype_id") == "")
            {
                strPriority = Application["appdefaultpriority"].ToString();
            }
            else
            {
                DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(GetUserInputText("sys_requesttype_id"));
                if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                {
                    strPriority = dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString();
                }
            }

            htFields["sys_requestpriority"] = "true";
            if (strPriority == "")
            {
                sbSql.Append(", [sys_requestpriority]=NULL");
            }
            else
            {
                sbSql.Append(", [sys_requestpriority]=@sys_requestpriority");

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestpriority";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strPriority;
                cmd.Parameters.Add(parameter);
            }
        }

        DataSet dsPri = null;
        if (strPriority != "")
        {
            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (dr["sys_allowedit"].ToString() == "1")
                {
                    bPriorityEdit = true;
                }

            }
        }

        string strOldPriority = GetValue("sys_requestpriority");
        if (!bPriorityEdit && strOldPriority != strPriority)
        {
            double dReqReslvHr = 0;
            double dReqEsc1Hr = 0;
            double dReqEsc2Hr = 0;
            double dReqEsc3Hr = 0;
            double dReqResHr = 0;


            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (LayUtil.IsNumeric(dr["sys_resolvehours"].ToString()))
                {
                    dReqReslvHr = double.Parse(dr["sys_resolvehours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate1hours"].ToString()))
                {
                    dReqEsc1Hr = double.Parse(dr["sys_escalate1hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate2hours"].ToString()))
                {
                    dReqEsc2Hr = double.Parse(dr["sys_escalate2hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate3hours"].ToString()))
                {
                    dReqEsc3Hr = double.Parse(dr["sys_escalate3hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_respondhours"].ToString()))
                {
                    dReqResHr = double.Parse(dr["sys_respondhours"].ToString());
                }
            }

            htFields["sys_resolve"] = "true";
            htFields["sys_escalate1"] = "true";
            htFields["sys_escalate2"] = "true";
            htFields["sys_escalate3"] = "true";
            htFields["sys_respond"] = "true";
            
            string strBegin = GetUserInputDate("sys_requestdate");
            string strSiteId = GetUserInputText("sys_siteid");

            if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0 || dReqResHr != 0) && strBegin != "")
            {
                string strOrgBegin = GetValue("sys_requestdate");
                DateTime dtOrgBegin = DateTime.Parse(strOrgBegin);

                DateTime dtBegin = DateTime.Parse(strBegin);

                if (strSiteId != GetValue("sys_siteid") || DateTime.Compare(dtBegin, dtOrgBegin) != 0)
                {
                    LoadSus(GetValue("sys_request_id"), "", strSiteId, strBegin);
                }


                if (dReqReslvHr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        sbSql.Append(", [sys_resolve]=@sys_resolve");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_resolve";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                    else
                    {
                        sbSql.Append(", [sys_resolve]=NULL");
                    }
                }
                else
                {
                    sbSql.Append(", [sys_resolve]=NULL");
                }

                if (dReqEsc1Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        sbSql.Append(", [sys_escalate1]=@sys_escalate1");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate1";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                    else
                    {
                        sbSql.Append(", [sys_escalate1]=NULL");
                    }
                }
                else
                {
                    sbSql.Append(", [sys_escalate1]=NULL");
                }

                if (dReqEsc2Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc2Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        sbSql.Append(", [sys_escalate2]=@sys_escalate2");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate2";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                    else
                    {
                        sbSql.Append(", [sys_escalate2]=NULL");
                    }
                }
                else
                {
                    sbSql.Append(", [sys_escalate2]=NULL");
                }


                if (dReqEsc3Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc3Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        sbSql.Append(", [sys_escalate3]=@sys_escalate3");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_escalate3";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                    else
                    {
                        sbSql.Append(", [sys_escalate3]=NULL");
                    }
                }
                else
                {
                    sbSql.Append(", [sys_escalate3]=NULL");
                }

                if (dReqResHr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqResHr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        sbSql.Append(", [sys_respond]=@sys_respond");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_respond";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strTime);
                        cmd.Parameters.Add(parameter);
                    }
                    else
                    {
                        sbSql.Append(", [sys_respond]=NULL");
                    }
                }
                else
                {
                    sbSql.Append(", [sys_respond]=NULL");
                }
            }
            else
            {
                sbSql.Append(", [sys_resolve]=NULL");
                sbSql.Append(", [sys_escalate1]=NULL");
                sbSql.Append(", [sys_escalate2]=NULL");
                sbSql.Append(", [sys_escalate3]=NULL");
                sbSql.Append(", [sys_respond]=NULL");
            }


        }

        //Request Class
        if (htBRVal["sys_requestclass_id"] != null)
        {
            string strBRReqClassId = htBRVal["sys_requestclass_id"].ToString();
            if (LibReqClassBR.ReqClassExist(strBRReqClassId))
            {
                htFields["sys_requestclass_id"] = "true";
                sbSql.Append(", sys_requestclass_id=@sys_requestclass_id");
                cmd.Parameters.AddWithValue("@sys_requestclass_id", strBRReqClassId);
            }

        }

        foreach (string strField in htBRVal.Keys)
        {
            if (htFields[strField] == null)
            {
                sbSql.Append(", " + strField + "=@" + strField);

                string strVal = htBRVal[strField].ToString();

                if (strVal == "")
                {
                    cmd.Parameters.AddWithValue("@" + strField, DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@" + strField, strVal);
                }
            }
        }


        sbSql.Append(" WHERE [sys_request_id]=@sys_request_id");
        parameter = new SqlParameter();
        parameter.ParameterName = "@sys_request_id";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = ViewState["sys_request_id"].ToString();
        cmd.Parameters.Add(parameter);

        cmd.CommandText = sbSql.ToString();

        return cmd;
    }

    private bool MatchSel(DataRow dr)
    {
        string strField = dr["sys_brsel_field"].ToString();
        string strOp = dr["sys_brsel_op"].ToString();
        string strValue = dr["sys_brsel_value"].ToString().ToLower();

        string strVal = "";
        if (strField == "sys_company_id" || strField == "sys_eclient_id")  //removed || strField == "sys_requestpriority"
        {
            string strEUser = GetUserInputText("sys_eusername");
            DataSet dsEUser = EUserDA.GetEUserInfo(strEUser);
            string strEclientId = "";
            if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
            {
                strEclientId = dsEUser.Tables[0].Rows[0]["sys_eclient_id"].ToString();
            }

            if (strField == "sys_eclient_id")
            {
                strVal = strEclientId;
            }
            else if (strField == "sys_company_id")
            {
                strVal = UserDA.GetComNameFrmDeptId(strEclientId);
            }
            //else if (strField == "sys_requestpriority")
            //{
            //    strVal = Application["appdefaultpriority"].ToString();
            //}
        }
        else if (strField == "sys_requestclass_id")
        {
            strVal = strReqClass;
        }
        else if (strField == "sys_createdby")
        {
            strVal = GetValue(strField);
        }
        else
        {
            if (htCtrlNode[strField] != null)
            {
                strVal = GetUserInput((XmlElement)htCtrlNode[strField]);
                if (strField == "sys_urgency")
                {
                    DataSet objDataSet = LibUrgencyDA.GetItemInfoById(strVal);
                    if (objDataSet != null && objDataSet.Tables.Count > 0 && objDataSet.Tables[0].Rows.Count > 0)
                    {
                        strVal = objDataSet.Tables[0].Rows[0]["sys_urgency"].ToString();
                    }
                }
                else if (strField == "sys_impact")
                {
                    DataSet objDataSet = LibImpactDA.GetItemInfoById(strVal);
                    if (objDataSet != null && objDataSet.Tables.Count > 0 && objDataSet.Tables[0].Rows.Count > 0)
                    {
                        strVal = objDataSet.Tables[0].Rows[0]["sys_impact"].ToString();
                    }
                }
            }
        }

        strVal = strVal.ToLower();

        bool bRes = false;
        switch (strOp)
        {
            case "is":
                if (strVal == strValue)
                {
                    bRes = true;
                }
                break;
            case "is not":
                if (strVal != strValue)
                {
                    bRes = true;
                }
                break;
            case "contains":
                if (strVal.IndexOf(strValue) != -1)
                {
                    bRes = true;
                }
                break;
            case "does not contain":
                if (strVal.IndexOf(strValue) == -1)
                {
                    bRes = true;
                }
                break;
            case "begins with":
                bRes = strVal.StartsWith(strValue);
                break;
            case "ends with":
                bRes = strVal.EndsWith(strValue);
                break;
            default:
                break;
        }

        return bRes;
    }

    /// <summary>
    /// Go through the Business Rules and set proper value for 
    /// </summary>
    public void CheckBusinessRule()
    {
        htBRVal = new Hashtable();
        DataSet dsBR = BusinessRuleDA.GetEnableReqBRList();
        if (dsBR != null && dsBR.Tables.Count > 0 && dsBR.Tables[0].Rows.Count > 0)
        {
            foreach (DataRow drBR in dsBR.Tables[0].Rows)
            {
                string strBRId = drBR["sys_businessrule_id"].ToString();
                string strLogic = drBR["sys_businessrule_logic"].ToString();
                bool bAnd = false;
                if (strLogic == "AND")
                {
                    bAnd = true;
                }

                DataSet dsBRSel = BusinessRuleDA.GetBRSelList(strBRId);
                if (dsBRSel != null && dsBRSel.Tables.Count > 0 && dsBRSel.Tables[0].Rows.Count > 0)
                {
                    bool bMatch = false;
                    if (bAnd)
                    {
                        bMatch = true;
                    }

                    foreach (DataRow drBRSel in dsBRSel.Tables[0].Rows)
                    {
                        if (bAnd)
                        {
                            bMatch = bMatch && MatchSel(drBRSel);
                            if (!bMatch)
                            {
                                break;
                            }
                        }
                        else
                        {
                            bMatch = bMatch || MatchSel(drBRSel);
                            if (bMatch)
                            {
                                break;
                            }
                        }
                    }

                    if (bMatch)
                    {
                        DataSet dsBRAction = BusinessRuleDA.GetBRActionList(strBRId);
                        if (dsBRAction != null && dsBRAction.Tables.Count > 0 && dsBRAction.Tables[0].Rows.Count > 0)
                        {
                            bRule = true;
                            ViewState["bRule"] = "true";
                            foreach (DataRow drBRAction in dsBRAction.Tables[0].Rows)
                            {
                                string strField = drBRAction["sys_braction_field"].ToString();
                                string strVal = drBRAction["sys_braction_value"].ToString();
                                if (htBRVal[strField] == null)
                                {
                                    //Check Value first
                                    if (strField == "sys_assignedto")
                                    {
                                        if (!UserDA.CheckExistUser(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_assignedtoanalgroup")
                                    {
                                        if (!UserDA.CheckExistAnaGrp(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_requestclass_id")
                                    {
                                        if (!LibReqClassBR.ReqClassExist(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_requesttype_id")
                                    {
                                        if (!LibReqTypeDA.CheckExist(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_requeststatus")
                                    {
                                        if (!LibReqStatusDA.CheckExistId(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_requestpriority")
                                    {
                                        if (!LibPriorityDA.CheckExistPri(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_urgency")
                                    {
                                        if (!LibUrgencyDA.CheckExistId(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_itservice")
                                    {
                                        if (!LibITServiceDA.CheckExistId(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_impact")
                                    {
                                        if (!LibImpactDA.CheckExistId(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField.Length >= 4 && strField.Substring(0, 4) != "sys_")
                                    {
                                        if (htCols[strField] == null)
                                            continue;

                                        string strType = htCols[strField].ToString();
                                        if (strType == "Integer" || strType == "Decimal")
                                        {
                                            if (!LayUtil.IsNumeric(strVal))
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    htBRVal[strField] = strVal;
                                }
                            }

                            ViewState["htBRVal"] = htBRVal;
                        }
                    }
                }
            }
        }

    }

    public bool IgnoreBRVal(XmlElement node)
    {
        string strOldPK = "";
        if (ViewState["sys_request_id"] != null)
        {
            strOldPK = ViewState["sys_request_id"].ToString();
        }

        string strIgnore = "false";
        if (strOldPK == "")
        {
            strIgnore = node.GetAttribute("IgnoreBRNew");
        }
        else
        {
            strIgnore = node.GetAttribute("IgnoreBRUpd");
        }

        if (strIgnore == "true")
            return true;

        return false;
    }


    private void VerifyAssignedTo()
    {
        string strProposedAssignedTo = GetUserInputText("sys_assignedto");
        string strProposedAssignedToGroup = GetUserInputText("sys_assignedtoanalgroup");

        if (strProposedAssignedTo != "" && strProposedAssignedToGroup != "")
        {
            if (!UserDA.IfUserInGrp(strProposedAssignedTo, strProposedAssignedToGroup))
            {
                if (htBRVal["sys_assignedtoanalgroup"] == null)
                {
                    sys_assignedtoanalgroupH.Text = "";
                }
                else
                {
                    sys_assignedtoH.Text = "";
                }
            }
        }
    }

    /// <summary>
    /// Save data to database
    /// </summary>
    /// <returns></returns>
    private bool SaveData()
    {
        SqlCommand cmd;

        string strOldPK = "";
        if (ViewState["sys_request_id"] != null)
        {
            strOldPK = ViewState["sys_request_id"].ToString();
        }

        if (strOldPK == "")
        {
            cmd = GetInsCmd();
        }
        else
        {
            cmd = GetUpdCmd();
        }

        bool bRes = LayUtilDA.RunSqlCmd(cmd);
        if (!bRes)
        {
            ShowMsg("Failed to Save data.");
        }
        return bRes;

    }

    /// <summary>
    /// Process the request after save data
    /// </summary>
    private bool ProcessUpdReq()
    {
        //Indicate if its new request
        bReqNew = false;
        string strReqId = "";
        if (ViewState["sys_request_id"] != null)
        {
            strReqId = ViewState["sys_request_id"].ToString();
        }

        //Apply suspension for newly created request if need be
        if (strReqId == "")
        {
            bReqNew = true;
            strReqId = RequestDA.GetMaxReqId();
            Session["requestexportid"] = strReqId;

            ViewState["sys_request_id"] = strReqId;
            Session["ButtonBack"] = "ReqInfo.aspx?back=true&sys_request_id=" + strReqId;
            if (RequestDA.ReqInSus(strReqId))
            {
                RequestDA.ReqInsertSus(strReqId, GetUserInputDate("sys_requestdate"), Session["User"].ToString());

                RequestDA.UpdReqEsc(strReqId);
            }
        }

        ///Create History/Audit
        string strAudit = SysDA.GetSettingValue("securityauditproblemdescfull", Application).ToLower();

        //Edit Request
        if (!bReqNew)
        {
            foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
            {
                if (!(child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field") && !(child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_"))
                {
                    continue;
                }

                //if (child.InnerText == "sys_request_hdmins" || child.InnerText == "sys_request_24mins" || child.InnerText == "sys_request_timespent")
                if (child.InnerText == "sys_request_hdmins" || child.InnerText == "sys_request_24mins")
                {
                    continue;
                }

                string strOrgVal = GetValue(child.InnerText);
                string strNewVal = GetUserInput(child);


                if (strOrgVal != strNewVal)
                {
                    if (htCols[child.InnerText] != null)
                    {
                        if (htCols[child.InnerText].ToString() == "Text")
                        {
                            if (strNewVal.Length > int.Parse(htColSize[child.InnerText].ToString()) && strAudit != "true")
                            {
                                strNewVal = "(Large Text Field Edit)";
                            }
                        }
                        else if (htCols[child.InnerText].ToString() == "DateTime")
                        {
                            if (strOrgVal != "" && strNewVal != "")
                            {
                                if (DateTime.Compare(DateTime.Parse(strOrgVal), DateTime.Parse(strNewVal)) == 0)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "'Edited'", child.InnerText, strOrgVal, strNewVal);
                }
            }
        }
        else
        {
            foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
            {
                if (!(child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field") && !(child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_"))
                {
                    continue;
                }

                //if (child.InnerText == "sys_request_hdmins" || child.InnerText == "sys_request_24mins" || child.InnerText == "sys_request_timespent")
                if (child.InnerText == "sys_request_hdmins" || child.InnerText == "sys_request_24mins")
                {
                    continue;
                }

                string strOrgVal = "<NULL>";
                string strNewVal = GetUserInput(child);

                if (strNewVal == "")
                    continue;

                if (htCols[child.InnerText] != null)
                {
                    if (htCols[child.InnerText].ToString() == "Text")
                    {
                        if (strNewVal.Length > int.Parse(htColSize[child.InnerText].ToString()) && strAudit != "true")
                        {
                            strNewVal = "(Large Text Field Edit)";
                        }
                    }
                }

                RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "'Created'", child.InnerText, strOrgVal, strNewVal);
            }
        }


        string strEUser = GetUserInputText("sys_eusername");

        string strEUserMail = "";
        if (strEUser != "")
        {
            DataSet dsEUser = UserDA.GetEUserInfo(strEUser);
            if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
            {
                strEUserMail = dsEUser.Tables[0].Rows[0]["sys_email"].ToString();
            }
        }

        ///Send mail for new request
        if (bReqNew)
        {
            if (SysDA.GetSettingValue("appemailenduserrequestcreated", Application) == "true")
            {
                if (strEUser != "" && strEUserMail != "")
                {
                    string strSignatureUser = "";
                    if (SysDA.GetSettingValue("appemailenduserrequestcreatedsignature", Application) == "true")
                    {
                        strSignatureUser = Session["User"].ToString();
                    }

                    LayUtil.SendOutEmail(Application, "req_created_euser", strEUserMail, "", strEUser, "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId), strSignatureUser);
                }
            }
        }

        ///Send mail if Priority changes
        if (!bReqNew)
        {
            if (htCtrl["sys_requestpriority"] != null)
            {
                string strOrgVal = GetValue("sys_requestpriority");
                string strNewVal = GetUserInputDD("sys_requestpriority");

                if (strOrgVal != strNewVal)
                {
                    if (SysDA.GetSettingValue("appemaileuserchangestatus", Application) == "true")
                    {
                        if (strEUser != "" && strEUserMail != "")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemaileuserchangestatussignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }

                            LayUtil.SendOutEmail(Application, "req_changepriority_enduser", strEUserMail, "", strEUser, "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId), strSignatureUser);
                        }
                    }

                    RequestDA.UpdReqEmail(strReqId);
                }
            }
        }


        ///assignedto or assignedtoanalgroup changed
        string strOrgAssignedTo = GetValue("sys_assignedto");
        string strNewAssignedTo = GetUserInputText("sys_assignedto");

        string strOrgAssignedToGrp = GetValue("sys_assignedtoanalgroup");
        string strNewAssignedToGrp = GetUserInputText("sys_assignedtoanalgroup");

        if (bReqNew)
        {
            strOrgAssignedTo = "";
            strOrgAssignedToGrp = "";

            string strUnassgnGrp = SysDA.GetSettingValue("appemailnewrequsergroup", Application);
            if (strUnassgnGrp != "" && strNewAssignedTo == "" && strNewAssignedToGrp == "")
            {
                string strGrpEmail = "";
                DataSet dsGrp;
                dsGrp = RequestDA.GetReqGrpEMail(strUnassgnGrp);

                if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drGrp in dsGrp.Tables[0].Rows)
                    {
                        strGrpEmail = drGrp["sys_email"].ToString();
                        if (strGrpEmail != "" && drGrp["Absence"].ToString() == "0")
                        {
                            LayUtil.SendOutEmail(Application, "req_newunassigned_usergroup", strGrpEmail, drGrp["sys_username"].ToString(), "", "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId));
                        }
                    }
                }
            }
        }

        if (strOrgAssignedTo != strNewAssignedTo || strOrgAssignedToGrp != strNewAssignedToGrp)
        {
            if (SysDA.GetSettingValue("responsemarkwhen", Application) == "0")
            {
                if (GetValue("sys_responded") == "")
                {
                    if (strReqId != "" && strNewAssignedTo != "")
                    {
                        RequestDA.UpdReqRespond(strReqId, DateTime.Now, strNewAssignedTo);
                    }
                }
            }


            //Put Email In Queue To Advise Analyst or Group of Analysts or both
            if (SysDA.GetSettingValue("appemailassignuserflag", Application) == "true")
            {
                if (strOrgAssignedTo != strNewAssignedTo)
                {
                    DataSet dsNewUser = UserDA.GetUserInfo(strNewAssignedTo);
                    if (dsNewUser != null && dsNewUser.Tables.Count > 0 && dsNewUser.Tables[0].Rows.Count > 0)
                    {
                        string strUserEmail = dsNewUser.Tables[0].Rows[0]["sys_email"].ToString();
                        if (strUserEmail != "")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemailassignuserflagsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }

                            LayUtil.SendOutEmail(Application, "req_assign_user", strUserEmail, strNewAssignedTo, "", "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId), strSignatureUser);
                        }
                    }
                }
            }

            if (SysDA.GetSettingValue("appemailassignusergroupflag", Application) == "true")
            {
                if (strOrgAssignedToGrp != strNewAssignedToGrp)
                {
                    string strGrpEmail = "";
                    DataSet dsGrp;
                    if (SysDA.GetSettingValue("appemailassignusergroupflag", Application) == "true" && strNewAssignedTo == "")
                    {
                        dsGrp = RequestDA.GetReqGrpEMail(strNewAssignedToGrp);
                    }
                    else
                    {
                        dsGrp = RequestDA.GetReqGrpEMail("");
                    }

                    if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow drGrp in dsGrp.Tables[0].Rows)
                        {
                            strGrpEmail = drGrp["sys_email"].ToString();
                            if (strGrpEmail != "" && drGrp["Absence"].ToString() == "0")
                            {
                                LayUtil.SendOutEmail(Application, "req_assign_usergroup", strGrpEmail, drGrp["sys_username"].ToString(), "", "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId));
                            }
                        }
                    }

                }
            }


            ///Put Email In Queue To Advise End User
            ///
            if (SysDA.GetSettingValue("appemailassignenduserflag", Application) == "true")
            {
                if (strOrgAssignedTo != strNewAssignedTo || strOrgAssignedToGrp != strNewAssignedToGrp)
                {
                    if (strEUser != "" && strEUserMail != "")
                    {
                        string strSignatureUser = "";
                        if (SysDA.GetSettingValue("appemailassignenduserflagsignature", Application) == "true")
                        {
                            strSignatureUser = Session["User"].ToString();
                        }

                        LayUtil.SendOutEmail(Application, "req_assign_enduser", strEUserMail, "", strEUser, "request", strReqId, strReqClass, RequestDA.GetReqMailCmd(strReqId), strSignatureUser);
                    }
                }
            }
        }


        //if created from incoming email, check and copy attachment
        string strMailId = ViewState["sys_mail_id"].ToString();
        if (LayUtil.IsNumeric(strMailId))
        {
            if (RequestDA.CopyAttachFrmMail(strMailId, strReqId))
            {
                MailDA.DelMailIn(strMailId);
            }
            else
            {
                ShowMsg("Failed to copy all attachements!");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Save request data to db
    /// </summary>
    /// <returns></returns>
    private bool SaveReqData()
    {
        CheckBusinessRule();

        if (!CheckInput())
            return false;

        if (SaveData())
        {
            return ProcessUpdReq();
        }

        //LoadVal();

        return false;
    }

    private string GetListURL()
    {
        string strURL = "";
        if (ViewState["sys_mail_id"].ToString() != "")
        {
            strURL = "UserEmailPending.aspx";

        }
        else
        {
            if (ViewState["from"].ToString() != "")
            {
                if (ViewState["from"].ToString() == "problem")
                {
                    strURL = "ProblemInfo.aspx?field=" + ViewState["field"] + "&sys_problem_id=" + ViewState["sys_problem_id"];
                }
                else if (ViewState["from"].ToString() == "change")
                {
                    strURL = "ChangeInfo.aspx?field=" + ViewState["field"] + "&sys_change_id=" + ViewState["sys_change_id"];
                }
                else if (ViewState["from"].ToString() == "asset")
                {
                    strURL = "AWAssetInfoRPC.aspx?AssetID=" + LayUtil.GetQueryString(Request, "AssetID") + "&sys_asset_id=" + LayUtil.GetQueryString(Request, "sys_asset_id");
                }
                else
                {
                    strURL = Session["ReqList"].ToString();
                }
            }
            else if (Session["ReqList"] != null)
            {
                strURL = Session["ReqList"].ToString();
            }
            else
            {
                strURL = "UserRequest.aspx?user=" + Session["User"].ToString();
            }
        }
        return strURL;
    }

    private void LoadDefEscTime()
    {
        bool bPriorityEdit = false;
        string strPriority = "";
        if (htCtrl["sys_requestpriority"] == null)
        {
            strPriority = Application["appdefaultpriority"].ToString();
        }
        else
        {
            strPriority = GetUserInputDD("sys_requestpriority");
        }

        DataSet dsPri = null;
        if (strPriority != "")
        {
            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (dr["sys_allowedit"].ToString() == "1")
                {
                    bPriorityEdit = true;
                }

            }
        }

        if (bPriorityEdit)
        {
            double dReqReslvHr = 0;
            double dReqEsc1Hr = 0;
            double dReqEsc2Hr = 0;
            double dReqEsc3Hr = 0;


            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (LayUtil.IsNumeric(dr["sys_resolvehours"].ToString()))
                {
                    dReqReslvHr = double.Parse(dr["sys_resolvehours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate1hours"].ToString()))
                {
                    dReqEsc1Hr = double.Parse(dr["sys_escalate1hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate2hours"].ToString()))
                {
                    dReqEsc2Hr = double.Parse(dr["sys_escalate2hours"].ToString());
                }
                if (LayUtil.IsNumeric(dr["sys_escalate3hours"].ToString()))
                {
                    dReqEsc3Hr = double.Parse(dr["sys_escalate3hours"].ToString());
                }
            }

            string strBegin = GetUserInputDate("sys_requestdate");
            string strSiteId = GetUserInputText("sys_siteid");

            if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0) && strBegin != "")
            {
                string strOrgBegin = GetValue("sys_requestdate");
                DateTime dtBegin = DateTime.Parse(strBegin);
                DateTime dtOrgBegin = DateTime.Parse(strOrgBegin);

                if (dReqReslvHr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        dpExpire.Value = DateTime.Parse(strTime);
                    }
                }

                if (dReqEsc1Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        dpEsc1.Value = DateTime.Parse(strTime);
                    }
                }

                if (dReqEsc2Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc2Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        dpEsc2.Value = DateTime.Parse(strTime);
                    }
                }

                if (dReqEsc3Hr != 0)
                {
                    TimeSpan ts = new TimeSpan(0, (int)(dReqEsc3Hr * 60), 0);
                    string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                    if (strTime != "")
                    {
                        dpEsc3.Value = DateTime.Parse(strTime);
                    }
                }

            }
        }
    }
    private bool PromptPopup()
    {
        //check if need to ask for explanation for priority change
        if (htCtrl["sys_requestpriority"] != null && SysDA.GetSettingValue("apppopupwhenprioritychange", Application) == "true")
        {
            string strOldPriority = GetValue("sys_requestpriority");
            string strNewPriority = GetUserInputDD("sys_requestpriority");
            if (strOldPriority != strNewPriority)
            {
                lbPriorityExplanation.Text = "Please explain why you change the " + Application["apppriorityterm"] + " from '" + strOldPriority + "' to '" + strNewPriority + "'";
                lbExplanationMsg.Text = "";
                dialogPriorityChgExplanation.WindowState = DialogWindowState.Normal;
                return true;
            }
        }

        //Priority Edit
        bool bPriorityEdit = false;

        string strPriority = "";
        if (htCtrl["sys_requestpriority"] == null)
        {
            if (htCtrl["sys_requesttype_id"] == null || GetUserInputText("sys_requesttype_id") == "")
            {
                strPriority = Application["appdefaultpriority"].ToString();
            }
            else
            {
                DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(GetUserInputText("sys_requesttype_id"));
                if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                {
                    strPriority = dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString();
                }
            }
        }
        else
        {
            strPriority = GetUserInputDD("sys_requestpriority");
        }

        DataSet dsPri = null;
        if (strPriority != "")
        {
            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (dr["sys_allowedit"].ToString() == "1")
                {
                    bPriorityEdit = true;
                }

            }
        }

        if (bPriorityEdit)
        {
            string strReslv = GetValue("sys_resolve");
            if (strReslv != "")
            {
                dpExpire.Value = DateTime.Parse(strReslv);
            }
            else
            {
                dpExpire.Value = null;
            }

            string strEsc1 = GetValue("sys_escalate1");
            if (strEsc1 != "")
            {
                dpEsc1.Value = DateTime.Parse(strEsc1);
            }
            else
            {
                dpEsc1.Value = null;
            }

            string strEsc2 = GetValue("sys_escalate1");
            if (strEsc2 != "")
            {
                dpEsc2.Value = DateTime.Parse(strEsc2);
            }
            else
            {
                dpEsc2.Value = null;
            }

            string strEsc3 = GetValue("sys_escalate3");
            if (strEsc3 != "")
            {
                dpEsc3.Value = DateTime.Parse(strEsc3);
            }
            else
            {
                dpEsc1.Value = null;
            }

            dialogEsc.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
            return true;
        }


        //Task Template
        string strTemplateId = "";
        if(ViewState["templateid"]!=null)
        {
            strTemplateId=ViewState["templateid"].ToString();
        }

        //if (GetValue("sys_requesttype_id") != GetUserInputText("sys_requesttype_id"))
        if ((ViewState["sys_requesttype_id"].ToString() != GetUserInputText("sys_requesttype_id")) || (!string.IsNullOrEmpty(strTemplateId)))
        {
            DataSet dsTask = TaskDA.GetReqTypeTasks(GetUserInputText("sys_requesttype_id"));
            if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
            {
                hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                //lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " contains predefined " + Application["appactionterm"] + "s which can be added to the " + Application["apprequestterm"] + ". These will replace any existings. Do you want to do this?";
                lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                return true;
            }
        }

        return false;
    }

    protected void imgPriorityExplanation_Click(object sender, ImageClickEventArgs e)
    {
        //Save Explanation into DB
        string strExplanation = tbPriorityExplanation.Text.Trim();
        if (strExplanation == "")
        {
            lbExplanationMsg.Text = "Cannot be empty.";
            return;
        }

        string strReqId = ViewState["sys_request_id"].ToString();
        DataTable dtPriorityAudit = RequestDA.GetReqLastPriorityChgAudit(strReqId, "sys_requestpriority");
        if (dtPriorityAudit != null && dtPriorityAudit.Rows.Count > 0)
        {
            if (!RequestDA.UpdAuditNotes(dtPriorityAudit.Rows[0]["sys_auditrequest_id"].ToString(), strExplanation))
            {
                lbExplanationMsg.Text = "Database error.";
                return;
            }
        }

        dialogPriorityChgExplanation.WindowState = DialogWindowState.Hidden;

        //Priority Edit
        bool bPriorityEdit = false;

        string strPriority = "";
        if (htCtrl["sys_requestpriority"] == null)
        {
            if (htCtrl["sys_requesttype_id"] == null || GetUserInputText("sys_requesttype_id") == "")
            {
                strPriority = Application["appdefaultpriority"].ToString();
            }
            else
            {
                DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(GetUserInputText("sys_requesttype_id"));
                if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                {
                    strPriority = dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString();
                }
            }
        }
        else
        {
            strPriority = GetUserInputDD("sys_requestpriority");
        }

        DataSet dsPri = null;
        if (strPriority != "")
        {
            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
            if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
            {
                DataRow dr = dsPri.Tables[0].Rows[0];
                if (dr["sys_allowedit"].ToString() == "1")
                {
                    bPriorityEdit = true;
                }

            }
        }

        if (bPriorityEdit)
        {
            string strReslv = GetValue("sys_resolve");
            if (strReslv != "")
            {
                dpExpire.Value = DateTime.Parse(strReslv);
            }
            else
            {
                dpExpire.Value = null;
            }

            string strEsc1 = GetValue("sys_escalate1");
            if (strEsc1 != "")
            {
                dpEsc1.Value = DateTime.Parse(strEsc1);
            }
            else
            {
                dpEsc1.Value = null;
            }

            string strEsc2 = GetValue("sys_escalate1");
            if (strEsc2 != "")
            {
                dpEsc2.Value = DateTime.Parse(strEsc2);
            }
            else
            {
                dpEsc2.Value = null;
            }

            string strEsc3 = GetValue("sys_escalate3");
            if (strEsc3 != "")
            {
                dpEsc3.Value = DateTime.Parse(strEsc3);
            }
            else
            {
                dpEsc1.Value = null;
            }

            dialogEsc.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
            return;
        }


        //Task Template

        string strTemplateId = "";
        if (ViewState["templateid"] != null)
        {
            strTemplateId = ViewState["templateid"].ToString();
        }

        //if (GetValue("sys_requesttype_id") != GetUserInputText("sys_requesttype_id"))
        if ((ViewState["sys_requesttype_id"].ToString() != GetUserInputText("sys_requesttype_id")) || (!string.IsNullOrEmpty(strTemplateId)))
        {
            DataSet dsTask = TaskDA.GetReqTypeTasks(GetUserInputText("sys_requesttype_id"));
            if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
            {
                hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                //lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " contains predefined " + Application["appactionterm"] + "s which can be added to the " + Application["apprequestterm"] + ". These will replace any existings. Do you want to do this?";
                lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                return;
            }
        }

        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }

    /// <summary>
    /// Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button1_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {

            ViewState["PostConfirmURL"] = GetListURL();
            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Change Status
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button2_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqChgStatus.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Task button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button3_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "UserTask.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Cancel button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button4_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect(GetListURL());
    }

    /// <summary>
    /// Request History
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button5_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqHistory.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Attachment button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button6_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqAttach.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }



    /// <summary>
    /// Comments button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button7_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqComment.aspx?CommentHTML=" + strCommentHtml + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Spawn button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button8_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqInfo.aspx?sys_requestparent_id=" + ViewState["sys_request_id"] + "&reqclass=" + strReqClass;

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Copy Request
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button10_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqCopy.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Change Template
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button11_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqChgClass.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Call back
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button12_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqCallback.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Cost
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button13_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqCost.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Link to Problem and Change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button14_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqLink.aspx?sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Create Problem
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button15_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ProblemInfo.aspx?newfromreq=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Create Change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button16_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ChangeInfo.aspx?newfromreq=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Set WebDropdown list items
    /// </summary>
    /// <param name="dd">WebDropdown</param>
    /// <param name="strList">List Parent Name</param>
    private void SetDropDown(WebDropDown dd, string strList, bool bEmpty)
    {
        DataSet dsItem = LibComboDA.GetSubItemList(strList);

        DropDownItem item;

        item = new DropDownItem("", "");
        dd.Items.Add(item);

        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["listitem"].ToString(), dr["listitem"].ToString());
                dd.Items.Add(item);

                if (ViewState["sys_request_id"].ToString() == "")
                {
                    if (dr["isdefault"].ToString() != "")
                    {
                        dd.SelectedValue = dr["listitem"].ToString();
                    }
                }
            }

            if (ViewState["sys_request_id"].ToString() != "")
            {
                dd.SelectedValue = strList;
            }

        }
    }

    /// <summary>
    /// Get ReqType
    /// </summary>
    /// <param name="nDepth"></param>
    /// <returns></returns>
    private string GetReqTypeDD(int nDepth)
    {
        string strVal = "";
        Control ctrlRoot = phCtrls;

        WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_requesttype_id" + nDepth.ToString());
        if (ctrl != null && ctrl.SelectedItem != null)
        {
            strVal = ctrl.SelectedItem.Value;
        }
        else
        {
            //Get Original Value
            strVal = "";
        }

        return strVal;
    }

    private string GetParentReqType(int nDepth)
    {
        string strParentReqType = "";

        for (int i = 1; i < nDepth; i++)
        {
            if (i != 1)
            {
                strParentReqType += "/";
            }
            strParentReqType += GetReqTypeDD(i);
        }

        return strParentReqType;
    }

    private void ReqTypeChgRelated(string strReqTypeId)
    {
        Control ctrlRoot = phCtrls;

        if (Session["Role"].ToString() != "enduser" && SysDA.GetSettingValue("appautoassignrequest", Application).Length >= 2 && SysDA.GetSettingValue("appautoassignrequest", Application).Substring(0, 2).ToLower() == "ap")
        {
            string strSiteManager = "";
            string strDeptManager = "";
            string strSiteId = sys_siteidH.Text;
            string strEUser = GetUserInputText("sys_eusername");

            if (strSiteId != "")
            {
                DataSet dsSite = UserDA.GetSitesById(strSiteId);
                if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
                {
                    strSiteManager = dsSite.Tables[0].Rows[0]["sys_site_username"].ToString();
                }
            }
            if (strEUser != "")
            {
                DataSet dsDept = EUserDA.GetDeptByEUser(strEUser);
                if (dsDept != null && dsDept.Tables.Count > 0 && dsDept.Tables[0].Rows.Count > 0)
                {
                    strDeptManager = dsDept.Tables[0].Rows[0]["sys_eclient_username"].ToString();
                }
            }


            string strSuggestedUser = "";
            string strSuggestedGrp = "";
            string strAssignType = "";
            DataSet dsReqType = RequestDA.GetReqTypeById(strReqTypeId);
            if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
            {

                DataRow dr = dsReqType.Tables[0].Rows[0];

                strAssignType = dr["sys_requesttype_assigntype"].ToString();
                if (strAssignType == "1")
                {
                    strSuggestedUser = strSiteManager;
                }
                else if (strAssignType == "2")
                {
                    strSuggestedUser = strDeptManager;
                }
                else if (strAssignType == "3")
                {
                    strSuggestedUser = dr["sys_requesttype_assignto"].ToString();
                }

                /////
                string strPriority = dr["sys_priority_id"].ToString();
                if (strPriority != "")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_priority_id");
                    if (ctrl != null)
                    {
                        ctrl.SelectedValue = strPriority;
                    }
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
                    foreach (DataRow drr in dsReqTypeAll.Tables[0].Rows)
                    {
                        htReqType.Add(drr["sys_requesttype_id"].ToString(), drr["sys_requesttypeparent_id"].ToString());
                    }

                }

                if (strAutoAssgn == "ap preference load balancing")
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
                }

                if (strSuggestedUser == "" && (SysDA.GetSettingValue("appautoassigncontrainskills", Application) == "false" || strAutoAssgn == "ap load balancing"))
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



                if (strAutoAssgn == "ap preference load balancing group")
                {
                    string strType = strReqTypeId;
                    while (strType != "" && strSuggestedGrp == "")
                    {
                        if (strSiteId != "" && strConstrnSite == "true")
                        {
                            strSuggestedGrp = UserDA.GetReqTypeSuggestedGrpSiteRes(strType, strSiteId);
                        }
                        else
                        {
                            strSuggestedGrp = UserDA.GetReqTypeSuggestedGrp(strType);
                        }

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
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrpSiteRes(strSiteId);
                    }
                    else
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
                    }
                }

            }


            TextBox ctrluser = (TextBox)ctrlRoot.FindControl("text" + htChildName["sys_assignedto"]);
            TextBox ctrlgrp = (TextBox)ctrlRoot.FindControl("text" + htChildName["sys_assignedtoanalgroup"]);
            if (strSuggestedUser != "")
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

                if (ctrluser != null)
                {
                    ctrluser.Text = strSuggestedUser;
                }
                sys_assignedtoH.Text = strSuggestedUser;

                if (ctrlgrp != null)
                {
                    ctrlgrp.Text = strSuggestedGrp;
                }
                sys_assignedtoanalgroupH.Text = strSuggestedGrp;

            }
            else
            {
                if (strSuggestedGrp != "")
                {
                    if (ctrlgrp != null)
                    {
                        ctrlgrp.Text = strSuggestedGrp;
                    }
                    sys_assignedtoanalgroupH.Text = strSuggestedGrp;
                }
            }
        }

    }
    private void SetReqTypeValue(string strReqTypeApply)
    {
        Control ctrlRoot = phCtrls;
        WebDropDown ctrl;
        string[] strReqTypeApplyArray = strReqTypeApply.Split(new Char[] { '/' });
        string strCurReqType = strReqTypeApplyArray[0];
        ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_requesttype_id1");
        if (ctrl != null)
        {
            SetDDReqType(ctrl, strCurReqType, 1);
        }
        for (int i = 1; i < strReqTypeApplyArray.Length;)
        {
            strCurReqType += "/" + strReqTypeApplyArray[i];
            i++;
            ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_requesttype_id"+i.ToString());
            if (ctrl != null)
            {
                SetDDReqType(ctrl, strCurReqType, i);
            }
        }
    }
    protected void ReqTypeSel_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        Control ctrlRoot = phCtrls;
        WebDropDown ctrl;

        WebDropDown dd = (WebDropDown)sender;

        ///////////////////////////////////////////////////
        int iRemoveIndex = 1;
        while (iRemoveIndex < dd.Items.Count)        
        {
            if ((dd.Items[iRemoveIndex].Text.ToString() == "")) //|| (dd.Items[iRemoveIndex].Value.ToString() == "")
            {
                dd.Items.RemoveAt(iRemoveIndex);
                iRemoveIndex = 1;
            }
            else
            {
                iRemoveIndex = iRemoveIndex + 1;
            }

        }  
        ////////////////////////////////////////////////////////////////

        string strIndex = dd.Attributes["Index"].ToString();
        if (!LayUtil.IsNumeric(strIndex))
            return;

        int nIndex = int.Parse(strIndex);

        for (int i = nIndex + 1; i <= nReqTypeCtrlCnt; i++)
        {
            ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_requesttype_id" + i.ToString());
            if (ctrl != null)
            {
                ctrl.Items.Clear();
                ctrl.SelectedValue = "";
                ctrl.CurrentValue = "";
                //ctrl.se
            }
        }
        string strCurType = GetParentReqType(nIndex);
        if (dd.SelectedValue != "")
        {
            if (strCurType != "")
            {
                strCurType += "/";
            }
            strCurType += dd.SelectedValue;
        }

        sys_requesttype_idH.Text = strCurType;

        if (strCurType == "")
        {
            return;
        }

        ReqTypeChgRelated(strCurType);

        ctrl = (WebDropDown)ctrlRoot.FindControl("ddsys_requesttype_id" + (nIndex + 1).ToString());

        if (ctrl != null)
        {
            ctrl.Items.Clear();
            DataSet dsReq = RequestDA.GetReqTypeByClassParent(strReqClass, strCurType);

            if (dsReq != null && dsReq.Tables.Count > 0)
            {
                DropDownItem item;
                if (dsReq.Tables[0].Rows.Count > 0)
                {
                    if (dsReq.Tables[0].Rows[0]["denycount"].ToString() == "0")
                    {
                        //item = new DropDownItem("", dsReq.Tables[0].Rows[0]["sys_requesttypeparent_id"].ToString());
                        item = new DropDownItem("", "");
                        ctrl.Items.Add(item);
                    }
                }

                foreach (DataRow dr in dsReq.Tables[0].Rows)
                {
                    if (dr["denycount"].ToString() == "0")
                    {
                        string strReqType = dr["sys_requesttype_id"].ToString();
                        string strParent = dr["sys_requesttypeparent_id"].ToString();

                        string strReqTypeShort = strReqType;
                        if (strParent != "")
                        {
                            strReqTypeShort = strReqType.Substring(strParent.Length + 1);
                        }
                        item = new DropDownItem(strReqTypeShort, strReqTypeShort);
                        ctrl.Items.Add(item);
                    }
                }

            }
        }

    }
    /// <summary>
    /// Set request type dropdown
    /// </summary>
    /// <param name="dd"></param>
    /// <param name="strVal"></param>
    /// <param name="nDepth"></param>
    private void SetDDReqType(WebDropDown dd, string strVal, int nDepth)
    {
        DataSet dsReq = null;
        if (nDepth == 1)
        {
            dsReq = RequestDA.GetReqTypeByClassParent(strReqClass, "");
        }
        else
        {
            string strParent = GetParentReqType(nDepth);
            if (strParent != "")
            {
                dsReq = RequestDA.GetReqTypeByClassParent(strReqClass, strParent);
            }
        }

        if (dsReq != null && dsReq.Tables.Count > 0)
        {
            dd.Items.Clear();
            DropDownItem item;
            //if (nDepth != 1)
            {
                if (dsReq.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drReq in dsReq.Tables[0].Rows)
                    {
                        if (drReq["denycount"].ToString() == "0")
                        {
                            item = new DropDownItem("", "");
                            dd.Items.Add(item);
                            break;
                        }
                    }
                }
            }
            foreach (DataRow dr in dsReq.Tables[0].Rows)
            {
                if (dr["denycount"].ToString() == "0")
                {
                    string strReqType = dr["sys_requesttype_id"].ToString();
                    string strParent = dr["sys_requesttypeparent_id"].ToString();

                    string strReqTypeShort = strReqType;
                    if (strParent != "")
                    {
                        strReqTypeShort = strReqType.Substring(strParent.Length + 1);
                    }
                    item = new DropDownItem(strReqTypeShort, strReqTypeShort);
                    dd.Items.Add(item);
                }
            }

        }

        if (strReqTypeArray == null)
        {
            strReqTypeArray = strVal.Split(new Char[] { '/' });
        }


        //if (!IsPostBack)
        {
            //dd.SelectedValue = strSelVal;// strReqTypeArray[nDepth - 1];
            if (strReqTypeArray.Length > nDepth - 1)
            {
                dd.SelectedValue = strReqTypeArray[nDepth - 1];
            }
        }
    }

    /// <summary>
    /// Set Priority Dropdown
    /// </summary>
    /// <param name="dd"></param>
    /// <param name="strVal"></param>
    private void SetDDPriority(WebDropDown dd, string strVal)
    {

        string strPriSite = GetUserVal("sys_priorityaccesssite");
        dd.Items.Clear();
        DataSet dsItem;
        if (strPriSite == "0")
        {
            dsItem = LibPriorityDA.GetPriorityInfo("");
        }
        else
        {
            dsItem = LibPriorityDA.GetUserSitePri(Session["User"].ToString());
        }

        DropDownItem item;

        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_priority_id"].ToString(), dr["sys_priority_id"].ToString());
                dd.Items.Add(item);
            }

        }

    }


    private void SetDDImpact(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibImpactDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_impact"].ToString(), dr["sys_impact_id"].ToString());
                dd.Items.Add(item);
            }
        }
        if (strVal == "")
        {
            dd.SelectedItemIndex = 0;
        }
        else
        {
            dd.SelectedValue = strVal;
        }
    }

    private void SetDDUrgency(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibUrgencyDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_urgency"].ToString(), dr["sys_urgency_id"].ToString());
                dd.Items.Add(item);
            }
        }
        if (strVal == "")
        {
            dd.SelectedItemIndex = 0;
        }
        else
        {
            dd.SelectedValue = strVal;
        }
    }
    private void SetDDITService(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibITServiceDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_itservice"].ToString(), dr["sys_itservice_id"].ToString());
                dd.Items.Add(item);
            }
        }
        if (strVal == "")
        {
            dd.SelectedItemIndex = 0;
        }
        else
        {
            dd.SelectedValue = strVal;
        }
    }
    public string GetUserVal(string strField)
    {
        if (dsUser == null || dsUser.Tables.Count <= 0 || dsUser.Tables[0].Rows.Count <= 0)
        {
            return "";
        }

        //Get value from DataSet
        try
        {
            return dsUser.Tables[0].Rows[0][strField].ToString();
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Load Mail Value
    /// </summary>
    /// <returns></returns>
    private bool LoadMailVal()
    {
        //if (strField == "sys_problemsummary" || strField == "sys_problemdesc" || strField == "sys_eusername" || strField == "sys_siteid" || strField == "sys_asset_id" || strField == "sys_asset_location" || strField == "sys_requesttype_id" || strField == "sys_assignedto" || strField == "sys_assignedtoanalgroup")
        //{
        htMailVal = new Hashtable();

        string strMailId = ViewState["sys_mail_id"].ToString();
        if (!LayUtil.IsNumeric(strMailId))
        {
            return false;
        }
        DataSet dsMailIn = MailDA.LoadMailInById(strMailId);
        if (dsMailIn != null && dsMailIn.Tables.Count > 0 && dsMailIn.Tables[0].Rows.Count > 0)
        {
            DataRow drMailIn = dsMailIn.Tables[0].Rows[0];
            string strSub = LayUtil.RmvSubjectPrefix(drMailIn["sys_mail_subject"].ToString().Trim());
            string strProblemDesc = drMailIn["sys_mail_body"].ToString();
            htMailVal.Add("sys_mail_html", drMailIn["sys_mail_html"].ToString());

            if (strSub.Length > 255)
            {
                strSub = strSub.Substring(0, 255);
            }

            string strSubL = strSub.ToLower();

            htMailVal.Add("sys_problemsummary", strSub);
            htMailVal.Add("sys_problemdesc", strProblemDesc);

            string strSiteId = drMailIn["sys_siteid"].ToString();

            string strEUser = "";
            DataSet dsEUser = UserDA.GetEUserInfoByEmail(drMailIn["sys_mail_fromemail"].ToString());
            if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
            {
                strEUser = dsEUser.Tables[0].Rows[0]["sys_eusername"].ToString();
                htMailVal.Add("sys_eusername", strEUser);

                if (strSiteId == "")
                {
                    strSiteId = dsEUser.Tables[0].Rows[0]["sys_siteid"].ToString();
                }
            }

            htMailVal.Add("sys_siteid", strSiteId);

            string strDate = drMailIn["sys_mail_date"].ToString();
            if (strDate != "")
            {
                htMailVal.Add("sys_requestdate", strDate);
            }
            else
            {
                htMailVal.Add("sys_requestdate", DateTime.Now.ToString());
            }

            htMailVal.Add("sys_requestclass_id", ViewState["reqclass"].ToString());
            if (strEUser != "")
            {
                DataSet dsAsset = AssetDA.GetEUserReqLastAssetDS(strEUser);
                if (dsAsset != null && dsAsset.Tables.Count > 0 && dsAsset.Tables[0].Rows.Count > 0)
                {
                    htMailVal.Add("sys_asset_id", dsAsset.Tables[0].Rows[0]["sys_asset_id"].ToString());
                    htMailVal.Add("sys_asset_location", dsAsset.Tables[0].Rows[0]["sys_asset_location"].ToString());
                }
            }

            string strReqType = "";
            if (strSub != "")
            {
                DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(strSub);
                if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                {
                    strReqType = dsReqType.Tables[0].Rows[0]["sys_requesttype_id"].ToString();
                }
            }

            if (strReqType == "")
            {
                if (strSub != "")
                {
                    DataSet dsKeyword = LibEmailKeyWordsDA.GetItemInfo("");
                    if (dsKeyword != null && dsKeyword.Tables.Count > 0)
                    {
                        foreach (DataRow drKeyword in dsKeyword.Tables[0].Rows)
                        {
                            if (strSubL.Contains(drKeyword["sys_keyword"].ToString().ToLower()))
                            {
                                strReqType = drKeyword["sys_requesttype_id"].ToString();
                                break;
                            }
                        }

                        if (strReqType == "")
                        {
                            foreach (DataRow drKeyword in dsKeyword.Tables[0].Rows)
                            {
                                if (strProblemDesc.Contains(drKeyword["sys_keyword"].ToString()))
                                {
                                    strReqType = drKeyword["sys_requesttype_id"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            htMailVal.Add("sys_requesttype_id", strReqType);

            string strPriority = Application["appdefaultpriority"].ToString();
            if (strReqType != "")
            {
                DataSet dsReqType = LibReqTypeDA.GetReqTypeInfo(strReqType);
                if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
                {
                    if (dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString() != "")
                    {
                        strPriority = dsReqType.Tables[0].Rows[0]["sys_priority_id"].ToString();
                    }
                }
            }

            htMailVal.Add("sys_requestpriority", strPriority);

            string strAutoAssign = SysDA.GetSettingValue("appautopopulateassignedto", Application);
            if (strAutoAssign == "true")
            {
                string strAssignedTo = Session["User"].ToString();
                string strAssignedToGrp = UserDA.GetUserDefGrp(strAssignedTo);

                htMailVal["sys_assignedto"] = strAssignedTo;
                htMailVal["sys_assignedtoanalgroup"] = strAssignedToGrp;
            }
            else if (strAutoAssign == "spec")
            {
                if (SysDA.GetSettingValue("appautopopulateassignedtospec", Application) != "")
                {
                    if (UserDA.CheckExistUser(SysDA.GetSettingValue("appautopopulateassignedtospec", Application)))
                    {
                        string strAssignedTo = SysDA.GetSettingValue("appautopopulateassignedtospec", Application);

                        string strAssignedToGrp = UserDA.GetUserDefGrp(strAssignedTo);

                        htMailVal["sys_assignedto"] = strAssignedTo;
                        htMailVal["sys_assignedtoanalgroup"] = strAssignedToGrp;
                    }

                }
            }
            else
            {
                #region Autoassign User and Group

                string strSuggestedUser = "";
                string strSuggestedGrp = "";


                string strAutoAssgn = SysDA.GetSettingValue("appautoassignenduserrequest", Application).ToLower();
                string strConstrnSite = SysDA.GetSettingValue("appautoassigncontraintosite", Application);

                Hashtable htReqType = new Hashtable();
                DataSet dsReqTypeAll = RequestDA.GetReqTypeList();
                if (dsReqTypeAll != null && dsReqTypeAll.Tables.Count > 0)
                {
                    foreach (DataRow drReqTypeAll in dsReqTypeAll.Tables[0].Rows)
                    {
                        htReqType.Add(drReqTypeAll["sys_requesttype_id"].ToString(), drReqTypeAll["sys_requesttypeparent_id"].ToString());
                    }

                }

                if (strAutoAssgn == "preference load balancing")
                {
                    string strType = strReqType;
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
                else if (strAutoAssgn == "load balancing")
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

                if (strAutoAssgn == "preference load balancing group")
                {
                    string strType = strReqType;
                    while (strType != "" && strSuggestedGrp == "")
                    {
                        if (strSiteId != "" && strConstrnSite == "true")
                        {
                            strSuggestedGrp = UserDA.GetReqTypeSuggestedGrpSiteRes(strType, strSiteId);
                        }
                        else
                        {
                            strSuggestedGrp = UserDA.GetReqTypeSuggestedGrp(strType);
                        }

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
                    if (strSuggestedGrp == "" && SysDA.GetSettingValue("appautoassigncontrainskills", Application) == "false")
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
                    }

                }
                if (strAutoAssgn == "load balancing group")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrpSiteRes(strSiteId);
                    }
                    else
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
                    }
                }

                #endregion

                htMailVal["sys_assignedto"] = strSuggestedUser;
                htMailVal["sys_assignedtoanalgroup"] = strSuggestedGrp;
            }

        }
        return true;
    }

    /// <summary>
    /// Get values for the form field variable
    /// </summary>
    /// <param name="strField">field</param>
    /// <returns>Value</returns>
    public string GetValue(string strField)
    {
        //Default Value
        if (dsOldValue == null || dsOldValue.Tables.Count <= 0 || dsOldValue.Tables[0].Rows.Count <= 0)
        {
            string strVal = "";

            //if converting from mail
            if (ViewState["sys_mail_id"].ToString() != "")
            {
                if (htMailVal == null)
                {
                    LoadMailVal();
                }

                if (strField == "sys_problemdesc")
                {
                    if (htCtrlNode["sys_problemdesc"] != null)
                    {
                        XmlElement node = (XmlElement)htCtrlNode["sys_problemdesc"];
                        if (node.GetAttribute("htmleditor") == "true" && htMailVal["sys_mail_html"].ToString() != "true")
                        {
                            return htMailVal[strField].ToString().Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
                        }
                        if (node.GetAttribute("htmleditor") != "true" && htMailVal["sys_mail_html"].ToString() == "true")
                        {
                            RadEditor edt = new RadEditor();
                            edt.Content = htMailVal[strField].ToString();
                            return edt.Text;
                        }
                    }
                }

                if (htMailVal[strField] != null)
                {
                    return htMailVal[strField].ToString();
                }
            }

            if (LayUtil.IsNumeric(ViewState["templateid"].ToString()))
            {
                if (strField != "sys_request_id" && strField != "sys_requestdate")
                {
                    try
                    {
                        return dsTemplateValue.Tables[0].Rows[0][strField].ToString();
                    }
                    catch
                    {
                        return "";
                    }
                }
            }

            string str;
            switch (strField)
            {
                case "sys_request_id":
                    strVal = "(New " + Application["apprequestterm"] + ")";
                    break;
                case "sys_ownedby":
                    str = SysDA.GetSettingValue("appautopopulateownedby", Application);
                    if (str == "true")
                    {
                        strVal = Session["User"].ToString();
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulateownedbyspec", Application) != "")
                        {
                            if (UserDA.CheckExistUser(SysDA.GetSettingValue("appautopopulateownedbyspec", Application)))
                            {
                                strVal = SysDA.GetSettingValue("appautopopulateownedbyspec", Application);
                            }

                        }
                    }
                    break;
                case "sys_assignedto":
                    str = SysDA.GetSettingValue("appautopopulateassignedto", Application);
                    if (str == "true")
                    {
                        strVal = Session["User"].ToString();
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulateassignedtospec", Application) != "")
                        {
                            if (UserDA.CheckExistUser(SysDA.GetSettingValue("appautopopulateassignedtospec", Application)))
                            {
                                strVal = SysDA.GetSettingValue("appautopopulateassignedtospec", Application);
                            }

                        }
                    }
                    break;
                case "sys_assignedtoanalgroup":
                    str = SysDA.GetSettingValue("appautopopulateassignedto", Application);
                    if (str == "true")
                    {
                        strVal = UserDA.GetUserDefGrp(Session["User"].ToString());
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulateassignedtospec", Application) != "")
                        {
                            strVal = UserDA.GetUserDefGrp(SysDA.GetSettingValue("appautopopulateassignedtospec", Application));
                        }
                    }
                    break;
                case "sys_requestdate":
                    strVal = DateTime.Now.ToString();
                    break;
                case "sys_requestpriority":
                    strVal = SysDA.GetSettingValue("appdefaultpriority", Application);
                    break;
                case "sys_requestparent_id":
                    if (ViewState["sys_requestparent_id"] != null)
                    {
                        strVal = ViewState["sys_requestparent_id"].ToString();
                    }
                    break;
                case "sys_impact":
                    strVal = SysDA.GetSettingValue("appdefaultimpact", Application);
                    break;
                case "sys_urgency":
                    strVal = SysDA.GetSettingValue("appdefaulturgency", Application);
                    break;
                case "sys_itservice":
                    strVal = SysDA.GetSettingValue("appdefaultitservice", Application);
                    break;
                case "sys_createdby":
                    strVal = "{{user}}";
                    break;
                case "sys_requestclass_id":
                    strVal = strReqClass;
                    break;
                default:
                    strVal = "";
                    break;

            }

            return strVal;
        }



        //Get value from DataSet
        try
        {
            DataRow dr = dsOldValue.Tables[0].Rows[0];
            string strVal = dr[strField].ToString();

            if (strField == "sys_request_hdmins")
            {
                if (strVal == "")
                {
                    strVal = RequestDA.GetReqDuration(dr["sys_requestdate"].ToString(), DateTime.Now, listSus).ToString();
                }
            }
            else if (strField == "sys_request_24mins")
            {
                if (strVal == "")
                {
                    strVal = RequestDA.GetReqDuration24Min(dr["sys_requestdate"].ToString(), DateTime.Now).ToString();
                }
            }
            else if (strField == "sys_request_timespent")
            {
                string strDispFormat = SysDA.GetSettingValue("statsdurationdisplay", Application);
                strVal = RequestDA.Mins2Time(strVal, strDispFormat);
            }

            return strVal;
        }
        catch
        {
            return "";
        }

    }

    /// <summary>
    /// Check the format of input value, should be xxxhr xxm
    /// </summary>
    /// <param name="strVal"></param>
    /// <returns></returns>
    private bool CheckDuration(string strVal)
    {
        bool bRes = true;
        string strDispFormat = SysDA.GetSettingValue("statsdurationdisplay", Application);

        if (strDispFormat == "decimal")
        {
            if (strVal != "")
            {
                if (!LayUtil.IsNumeric(strVal))
                {
                    bRes = false;
                }
            }
        }
        else
        {
            if (strVal != "")
            {
                for (int i = 0; i < strVal.Length; i++)
                {
                    string[] strValArray = strVal.Split(new Char[] { ' ' });
                    if (strValArray.Length == 2)
                    {
                        string strHr = strValArray.ElementAt<string>(0);

                        if (strHr.Substring(strHr.Length - 2).ToLower() == "hr")
                        {
                            if (LayUtil.IsNumeric(strHr.Substring(0, strHr.Length - 2)))
                            {

                            }
                            else
                            {
                                bRes = false;
                            }
                        }
                        else
                        {
                            bRes = false;
                        }

                        string strM = strValArray.ElementAt<string>(1);
                        if (strM.Substring(strM.Length - 1).ToLower() == "m")
                        {
                            string strMVal = strM.Substring(0, strM.Length - 1);
                            if (LayUtil.IsNumeric(strMVal))
                            {
                                int nMVal = int.Parse(strMVal);
                                if (nMVal >= 60)
                                {
                                    bRes = false;
                                }
                            }
                            else
                            {
                                bRes = false;
                            }
                        }
                        else
                        {
                            bRes = false;
                        }
                    }
                    else
                    {
                        bRes = false;
                    }
                }
            }
            else
            {
                bRes = false;
            }
        }

        return bRes;
    }

    /// <summary>
    /// Get weekday in the form of 1,2,3,4,5,6,7
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private string GetWeekDayNum(DateTime dt)
    {
        string strVal;
        switch (dt.DayOfWeek.ToString())
        {
            case "Monday":
                strVal = "1";
                break;
            case "Tuesday":
                strVal = "2";
                break;
            case "Wednesday":
                strVal = "3";
                break;
            case "Thursday":
                strVal = "4";
                break;
            case "Friday":
                strVal = "5";
                break;
            case "Saturday":
                strVal = "6";
                break;
            case "Sunday":
                strVal = "7";
                break;
            default:
                strVal = "7";
                break;
        }

        return strVal;
    }

    private void PrePareHrHash()
    {
        htOpenHr = new Hashtable();
        htCloseHr = new Hashtable();

        for (int i = 1; i < 8; i++)
        {
            string strOpen = "open" + i.ToString();
            string strClose = "close" + i.ToString();

            htOpenHr.Add(strOpen, xmlHours.DocumentElement.GetElementsByTagName(strOpen).Item(0).InnerText);

            htCloseHr.Add(strClose, xmlHours.DocumentElement.GetElementsByTagName(strClose).Item(0).InnerText);
        }
    }

    private string GetOpenTime(DateTime dt)
    {
        if (htOpenHr == null)
        {
            PrePareHrHash();
        }

        string strOpen = "open" + GetWeekDayNum(dt);
        if (htOpenHr[strOpen] != null)
        {
            return htOpenHr[strOpen].ToString();
        }
        else
        {
            return "";
        }
    }

    private string GetCloseTime(DateTime dt)
    {
        if (htCloseHr == null)
        {
            PrePareHrHash();
        }

        string strClose = "close" + GetWeekDayNum(dt);
        if (htCloseHr[strClose] != null)
        {
            return htCloseHr[strClose].ToString();
        }
        else
        {
            return "";
        }
    }

    private bool IsWorking(DateTime dt)
    {
        string strOpen = GetOpenTime(dt);
        string strClose = GetCloseTime(dt);

        if (strOpen == "" || strClose == "")
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Load Suspension period
    /// Suspension has 3 sources: 1. Working Hours; 2. Closed Periods; 3. Request on hold;
    /// 2 and 3 are from suspention table
    /// </summary>
    /// <param name="strSiteId"></param>
    private void LoadSus(string strReqId, string strProblemId, string strBegin)
    {
        if (strBegin == "")
            return;

        string strSiteId = "";
        if (LayUtil.IsNumeric(strReqId))
        {
            DataSet dsReqValue = RequestDA.GetReqFullInfoById(strReqId);
            strSiteId = LayUtil.GetValue(dsReqValue, "sys_siteid");
        }
        else if (LayUtil.IsNumeric(strProblemId))
        {
            DataSet dsProblemValue = ProblemDA.GetProblemFullInfoById(strProblemId);
            strSiteId = LayUtil.GetValue(dsProblemValue, "sys_siteid");
        }

        LoadSus(strReqId, strProblemId, strSiteId, strBegin);

    }
    private void LoadSus(string strReqId, string strProblemId, string strSiteId, string strBegin)
    {
        //Load Hours first
        string strCompanyId = UserDA.GetComNameFrmSiteId(strSiteId);

        string strHoursXml = "";

        DataSet dsSite = UserDA.GetSitesById(strSiteId);
        if (dsSite == null || dsSite.Tables.Count <= 0 || dsSite.Tables[0].Rows.Count <= 0 || dsSite.Tables[0].Rows[0]["sys_workinghours_xml"].ToString() == "")
        {
            DataSet dsCom = UserDA.GetCompanyInfo(strCompanyId);
            if (dsCom == null || dsCom.Tables.Count <= 0 || dsCom.Tables[0].Rows.Count <= 0 || dsCom.Tables[0].Rows[0]["sys_workinghours_xml"].ToString() == "")
            {
                xmlHours = UserDA.LoadGlobalHoursXML();
            }
            else
            {
                strHoursXml = dsCom.Tables[0].Rows[0]["sys_workinghours_xml"].ToString();
                xmlHours = new XmlDocument();
                xmlHours.LoadXml(strHoursXml);
            }

        }
        else
        {
            strHoursXml = dsSite.Tables[0].Rows[0]["sys_workinghours_xml"].ToString();
            xmlHours = new XmlDocument();
            xmlHours.LoadXml(strHoursXml);
        }


        listSus = new List<SusPeriod>();
        SusPeriod sp;

        ///Get suspension from WH
        DateTime dtBegin = DateTime.Parse(strBegin);
        DateTime dtEnd = DateTime.Now.AddMonths(int.Parse(SysDA.GetSettingValue("appescalationlimit", Application)));

        //if same day
        if (DateTime.Compare(dtBegin.Date, dtEnd.Date) == 0)
        {
            if (!IsWorking(dtBegin))
            {
                sp = new SusPeriod(dtBegin, dtEnd);
                listSus.Add(sp);
            }
            else
            {
                string strOpen = GetOpenTime(dtBegin);
                string strClose = GetCloseTime(dtBegin);

                DateTime dtOpen = DateTime.Parse(dtBegin.ToShortDateString() + " " + strOpen);
                DateTime dtClose = DateTime.Parse(dtBegin.ToShortDateString() + " " + strClose);

                if (DateTime.Compare(dtBegin, dtOpen) < 0)
                {
                    if (DateTime.Compare(dtEnd, dtOpen) < 0)
                    {
                        sp = new SusPeriod(dtBegin, dtEnd);
                        listSus.Add(sp);
                    }
                    else if (DateTime.Compare(dtEnd, dtClose) > 0)
                    {
                        sp = new SusPeriod(dtBegin, dtOpen);
                        listSus.Add(sp);

                        sp = new SusPeriod(dtClose, dtEnd);
                        listSus.Add(sp);

                    }
                    else
                    {
                        sp = new SusPeriod(dtBegin, dtOpen);
                        listSus.Add(sp);
                    }
                }
                else if (DateTime.Compare(dtBegin, dtClose) < 0)
                {
                    if (DateTime.Compare(dtEnd, dtClose) > 0)
                    {
                        sp = new SusPeriod(dtClose, dtEnd);
                        listSus.Add(sp);
                    }
                }
                else
                {
                    sp = new SusPeriod(dtBegin, dtEnd);
                    listSus.Add(sp);
                }
            }
        }
        else
        {
            TimeSpan tsDays = dtEnd.Date.Subtract(dtBegin.Date);
            int nMidDays = tsDays.Days;


            DateTime dtSusST;

            //Process First Day
            if (!IsWorking(dtBegin))
            {
                dtSusST = dtBegin;
            }
            else
            {
                string strOpen = GetOpenTime(dtBegin);
                string strClose = GetCloseTime(dtBegin);



                DateTime dtOpen = DateTime.Parse(dtBegin.ToShortDateString() + " " + strOpen);
                DateTime dtClose = DateTime.Parse(dtBegin.ToShortDateString() + " " + strClose);

                if (DateTime.Compare(dtBegin, dtOpen) < 0)
                {
                    sp = new SusPeriod(dtBegin, dtOpen);
                    listSus.Add(sp);

                    dtSusST = dtClose;
                }
                else if (DateTime.Compare(dtBegin, dtClose) < 0)
                {
                    dtSusST = dtClose;
                }
                else
                {
                    dtSusST = dtBegin;
                }

            }

            //Middle days
            for (int i = 1; i < nMidDays; i++)
            {
                DateTime dtDay = dtBegin.AddDays(i);
                if (!IsWorking(dtDay))
                {
                    continue;
                }

                string strOpen = GetOpenTime(dtDay);
                string strClose = GetCloseTime(dtDay);

                DateTime dtOpen = DateTime.Parse(dtDay.ToShortDateString() + " " + strOpen);
                DateTime dtClose = DateTime.Parse(dtDay.ToShortDateString() + " " + strClose);

                sp = new SusPeriod(dtSusST, dtOpen);
                listSus.Add(sp);

                dtSusST = dtClose;
            }

            //last day
            if (!IsWorking(dtEnd))
            {
                sp = new SusPeriod(dtSusST, dtEnd);
                listSus.Add(sp);
            }
            else
            {
                string strOpen = GetOpenTime(dtEnd);
                string strClose = GetCloseTime(dtEnd);

                DateTime dtOpen = DateTime.Parse(dtEnd.ToShortDateString() + " " + strOpen);
                DateTime dtClose = DateTime.Parse(dtEnd.ToShortDateString() + " " + strClose);

                if (DateTime.Compare(dtEnd, dtOpen) < 0)
                {
                    sp = new SusPeriod(dtSusST, dtEnd);
                    listSus.Add(sp);
                }
                else if (DateTime.Compare(dtEnd, dtClose) < 0)
                {
                    sp = new SusPeriod(dtSusST, dtOpen);
                    listSus.Add(sp);
                }
                else
                {
                    sp = new SusPeriod(dtSusST, dtOpen);
                    listSus.Add(sp);

                    sp = new SusPeriod(dtClose, dtEnd);
                    listSus.Add(sp);
                }
            }

        }

        /// load from suspention table
        DataSet dsSus = null;
        if (strReqId != "")
        {
            dsSus = UserDA.GetSuspension(strReqId, strSiteId, strCompanyId, dtBegin, DateTime.Now);
        }
        else if (strProblemId != "")
        {
            dsSus = UserDA.GetProblemSuspension(strProblemId, strSiteId, strCompanyId, dtBegin, DateTime.Now);
        }
        /////////////////////////////////////////////////////////////////////////////////////////
        else
        {
            dsSus = UserDA.GetGlobalSuspensionReq(dtBegin);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        if (dsSus != null && dsSus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsSus.Tables[0].Rows)
            {
                string strEnd = dr["sys_suspend_end"].ToString();
                if (strEnd == "")
                {
                    sp = new SusPeriod(DateTime.Parse(dr["sys_suspend_begin"].ToString()), dtEnd.AddYears(1000), true);
                }
                else
                {
                    sp = new SusPeriod(DateTime.Parse(dr["sys_suspend_begin"].ToString()), DateTime.Parse(dr["sys_suspend_end"].ToString()));
                }

                listSus.Add(sp);
            }
        }


        ///Combine/Merge suspension period
        MergeSusPrdList();


    }

    /// <summary>
    /// Check if two suspension period overlap
    /// </summary>
    /// <param name="spA"></param>
    /// <param name="spB"></param>
    /// <returns></returns>
    private bool SPOverlapped(SusPeriod spA, SusPeriod spB)
    {
        if (DateTime.Compare(spA.dtStart, spB.dtEnd) > 0 || DateTime.Compare(spA.dtEnd, spB.dtStart) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// Merge two suspension period if overlap
    /// </summary>
    /// <param name="spA"></param>
    /// <param name="spB"></param>
    /// <returns></returns>
    private SusPeriod MergeSP(SusPeriod spA, SusPeriod spB)
    {
        SusPeriod sp = new SusPeriod();

        if (spA.bEndNull || spB.bEndNull)
        {
            sp.bEndNull = true;

            if (spA.bEndNull)
            {
                sp.dtEnd = spA.dtEnd;
            }
            else
            {
                sp.dtEnd = spB.dtEnd;
            }

        }
        else
        {
            if (DateTime.Compare(spA.dtEnd, spB.dtEnd) < 0)
            {
                sp.dtEnd = spB.dtEnd;
            }
            else
            {
                sp.dtEnd = spA.dtEnd;
            }

            sp.bEndNull = false;
        }

        if (DateTime.Compare(spA.dtStart, spB.dtStart) < 0)
        {
            sp.dtStart = spA.dtStart;
        }
        else
        {
            sp.dtStart = spB.dtStart;
        }


        return sp;
    }

    /// <summary>
    /// Compare the two suspension period to sort it
    /// </summary>
    /// <param name="spA"></param>
    /// <param name="spB"></param>
    /// <returns></returns>
    private static int CompareSP(SusPeriod spA, SusPeriod spB)
    {
        return DateTime.Compare(spA.dtStart, spB.dtStart);
    }

    /// <summary>
    /// Merge and sort Suspension period in the list
    /// </summary>
    /// <returns></returns>
    //private bool MergeSusPrdList()
    private void MergeSusPrdList()
    {
        int nPos = 0;

        while (nPos < listSus.Count)
        {
            SusPeriod spMain = listSus.ElementAt<SusPeriod>(nPos);

            int nCompared = nPos + 1;
            bool bMerged = false;

            while (nCompared < listSus.Count)
            {
                SusPeriod spCom = listSus.ElementAt<SusPeriod>(nCompared);

                if (SPOverlapped(spMain, spCom))
                {
                    SusPeriod sp = MergeSP(spMain, spCom);

                    listSus.Remove(spMain);
                    listSus.Remove(spCom);

                    listSus.Add(sp);

                    bMerged = true;

                    break;
                }
                else
                {
                    nCompared++;
                }

            }

            if (!bMerged)
            {
                nPos++;
            }

        }


        //Sort the list
        listSus.Sort(CompareSP);
    }


    protected void btnApplyTaskTemplate_Click(object sender, ImageClickEventArgs e)
    {
        string strReqId = ViewState["sys_request_id"].ToString();
        if (!RequestDA.ClearReqTasks(strReqId))
        {
            ShowMsg("Failed to delete existing " + Application["appactionterm"] + "s.");
        }

        if (!CreateTasksFromTemplate(GetUserInputText("sys_requesttype_id")))
        {
            ShowMsg("Failed to create " + Application["appactionterm"] + "s from template.");
            return;
        }

        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }
    protected void imgSaveEsc_Click(object sender, ImageClickEventArgs e)
    {
        string strReqId = ViewState["sys_request_id"].ToString();
        string strReslv = GetValue("sys_resolve");
        if (!LayUtil.EqualTime(strReslv, dpExpire.Text))
        {
            RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Edited", "sys_resolve", strReslv, dpExpire.Text);
        }
        string strEsc1 = GetValue("sys_escalate1");
        if (!LayUtil.EqualTime(strEsc1, dpEsc1.Text))
        {
            RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Edited", "sys_escalate1", strEsc1, dpEsc1.Text);
        }
        string strEsc2 = GetValue("sys_escalate2");
        if (!LayUtil.EqualTime(strEsc2, dpEsc2.Text))
        {
            RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Edited", "sys_escalate2", strEsc2, dpEsc2.Text);
        }
        string strEsc3 = GetValue("sys_escalate3");
        if (!LayUtil.EqualTime(strEsc3, dpEsc3.Text))
        {
            RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Edited", "sys_escalate3", strEsc3, dpEsc3.Text);
        }

        RequestDA.UpdEscalation(ViewState["sys_request_id"].ToString(), dpEsc1.Text, dpEsc2.Text, dpEsc3.Text, dpExpire.Text);
        dialogEsc.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;


        //Task Template
        if (htCtrl["sys_requesttype_id"] != null)
        {
            if (ViewState["sys_requesttype_id"].ToString() != GetUserInputText("sys_requesttype_id"))
            {
                DataSet dsTask = TaskDA.GetReqTypeTasks(GetUserInputText("sys_requesttype_id"));
                if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
                {
                    hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                    //lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " contains predefined " + Application["appactionterm"] + "s which can be added to the " + Application["apprequestterm"] + ". These will replace any existing " + Application["appactionterm"] + "s. Do you want to do this?";
                    lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                    dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                    return;
                }
            }
        }
        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }


    private bool CreateTasksFromTemplate(string strReqTypeId)
    {
        string strReqId = ViewState["sys_request_id"].ToString();

        if (!LayUtil.IsNumeric(strReqId))
        {
            return false;
        }

        Hashtable htTasks = new Hashtable();

        DataSet dsTasks = TaskDA.GetReqTypeTasks(strReqTypeId);
        if (dsTasks != null && dsTasks.Tables.Count > 0 && dsTasks.Tables[0].Rows.Count > 0)
        {
            DataSet dsField = DataDesignDA.GetTblCol("action");
            if (dsField == null || dsField.Tables.Count <= 0 || dsField.Tables[0].Rows.Count <= 0)
            {
                LayUtil.LogInfo(LayUtil.LogType.Error, "Create Tasks From Template", "Failed to get action table columns type", DateTime.Now);
                return false;
            }


            foreach (DataRow drTasks in dsTasks.Tables[0].Rows)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "";
                cmd.CommandType = CommandType.Text;
                SqlParameter parameter;

                StringBuilder sbSql = new StringBuilder();
                StringBuilder sbSqlVal = new StringBuilder();
                sbSql.Append("INSERT INTO [action](sys_action_createdfrom, sys_request_id ");
                sbSqlVal.Append(" VALUES(NULL, " + strReqId);

                foreach (DataRow drField in dsField.Tables[0].Rows)
                {
                    string strCol = drField["ColName"].ToString();
                    string strType = drField["ColType"].ToString();
                    if (strCol != "sys_action_id" && strCol != "sys_actiondepends_id" && strCol != "sys_requesttype_id" && strCol != "sys_schedule_id")
                    {
                        string strVal = "";
                        try
                        {
                            strVal = drTasks[strCol].ToString();
                        }
                        catch (Exception e)
                        {
                            //LayUtil.LogInfo(LayUtil.LogType.Error, "Create Tasks From Template", "Failed to get action fields value", DateTime.Now);
                        }

                        if (strVal != "")
                        {
                            sbSql.Append(",");
                            sbSql.Append(strCol);

                            sbSqlVal.Append(",");
                            sbSqlVal.Append("@" + strCol);

                            parameter = new SqlParameter();
                            parameter.ParameterName = "@" + strCol;
                            parameter.Direction = ParameterDirection.Input;

                            if (strType == "DateTime")
                            {
                                parameter.Value = DateTime.Parse(strVal);
                            }
                            else
                            {
                                parameter.Value = strVal;
                            }
                            cmd.Parameters.Add(parameter);
                        }
                    }
                }

                sbSql.Append(")");
                sbSqlVal.Append(")");

                cmd.CommandText = sbSql.Append(sbSqlVal.ToString()).ToString();

                if (LayUtilDA.RunSqlCmd(cmd))
                {
                    string strActionId = RequestDA.GetMaxActionId();
                    htTasks.Add(drTasks["sys_action_id"].ToString(), strActionId);
                }
            }


            //Setup Dependency
            DateTime dtPlaced = DateTime.Now;
            foreach (DataRow drTasks in dsTasks.Tables[0].Rows)
            {
                string strId = drTasks["sys_action_id"].ToString();
                string strTaskId = htTasks[strId].ToString();
                DataSet dsDpd = TaskDA.GetTemplateDependedAction(strId);
                if (dsDpd != null && dsDpd.Tables.Count > 0 && dsDpd.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drDpd in dsDpd.Tables[0].Rows)
                    {
                        TaskDA.InsRealDpd(strTaskId, htTasks[drDpd["sys_actiondependson_id"].ToString()].ToString());
                    }
                }
                else
                {
                    DataSet dsActTask = RequestDA.GetActionById(strTaskId);
                    if (dsActTask != null && dsActTask.Tables.Count > 0 && dsActTask.Tables[0].Rows.Count > 0)
                    {
                        DataRow drActTask = dsActTask.Tables[0].Rows[0];

                        DateTime dtActionDate = DateTime.Now;

                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "";
                        cmd.CommandType = CommandType.Text;
                        SqlParameter parameter;

                        StringBuilder sbSql = new StringBuilder();
                        StringBuilder sbSqlVal = new StringBuilder();
                        sbSql.Append("UPDATE [action] SET sys_actiondate=@sys_actiondate ");

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_actiondate";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = dtActionDate;
                        cmd.Parameters.Add(parameter);

                        string strPriority = drActTask["sys_actionpriority"].ToString();

                        DataSet dsPri = null;
                        if (strPriority != "")
                        {
                            dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
                        }



                        double dReqReslvHr = 0;
                        double dReqEsc1Hr = 0;
                        double dReqEsc2Hr = 0;
                        double dReqEsc3Hr = 0;


                        if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
                        {
                            DataRow drPriority = dsPri.Tables[0].Rows[0];
                            if (LayUtil.IsNumeric(drPriority["sys_resolvehours"].ToString()))
                            {
                                dReqReslvHr = double.Parse(drPriority["sys_resolvehours"].ToString());
                            }
                            if (LayUtil.IsNumeric(drPriority["sys_escalate1hours"].ToString()))
                            {
                                dReqEsc1Hr = double.Parse(drPriority["sys_escalate1hours"].ToString());
                            }
                            if (LayUtil.IsNumeric(drPriority["sys_escalate2hours"].ToString()))
                            {
                                dReqEsc2Hr = double.Parse(drPriority["sys_escalate2hours"].ToString());
                            }
                            if (LayUtil.IsNumeric(drPriority["sys_escalate3hours"].ToString()))
                            {
                                dReqEsc3Hr = double.Parse(drPriority["sys_escalate3hours"].ToString());
                            }
                        }

                        string strBegin = dtActionDate.ToString();
                        if (dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0)
                        {
                            LoadSus(drActTask["sys_request_id"].ToString(), "", strBegin);

                            if (dReqReslvHr != 0)
                            {
                                TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                                string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                                if (strTime != "")
                                {
                                    sbSql.Append(", [sys_resolve]=@sys_resolve");

                                    parameter = new SqlParameter();
                                    parameter.ParameterName = "@sys_resolve";
                                    parameter.Direction = ParameterDirection.Input;
                                    parameter.Value = DateTime.Parse(strTime);
                                    cmd.Parameters.Add(parameter);
                                }
                                else
                                {
                                    sbSql.Append(", [sys_resolve]=NULL");
                                }
                            }
                            else
                            {
                                sbSql.Append(", [sys_resolve]=NULL");
                            }

                            if (dReqEsc1Hr != 0)
                            {
                                TimeSpan ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                                string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                                if (strTime != "")
                                {
                                    sbSql.Append(", [sys_escalate1]=@sys_escalate1");

                                    parameter = new SqlParameter();
                                    parameter.ParameterName = "@sys_escalate1";
                                    parameter.Direction = ParameterDirection.Input;
                                    parameter.Value = DateTime.Parse(strTime);
                                    cmd.Parameters.Add(parameter);
                                }
                                else
                                {
                                    sbSql.Append(", [sys_escalate1]=NULL");
                                }
                            }
                            else
                            {
                                sbSql.Append(", [sys_escalate1]=NULL");
                            }

                            if (dReqEsc2Hr != 0)
                            {
                                TimeSpan ts = new TimeSpan(0, (int)(dReqEsc2Hr * 60), 0);
                                string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                                if (strTime != "")
                                {
                                    sbSql.Append(", [sys_escalate2]=@sys_escalate2");

                                    parameter = new SqlParameter();
                                    parameter.ParameterName = "@sys_escalate2";
                                    parameter.Direction = ParameterDirection.Input;
                                    parameter.Value = DateTime.Parse(strTime);
                                    cmd.Parameters.Add(parameter);
                                }
                                else
                                {
                                    sbSql.Append(", [sys_escalate2]=NULL");
                                }
                            }
                            else
                            {
                                sbSql.Append(", [sys_escalate2]=NULL");
                            }


                            if (dReqEsc3Hr != 0)
                            {
                                TimeSpan ts = new TimeSpan(0, (int)(dReqEsc3Hr * 60), 0);
                                string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                                if (strTime != "")
                                {
                                    sbSql.Append(", [sys_escalate3]=@sys_escalate3");

                                    parameter = new SqlParameter();
                                    parameter.ParameterName = "@sys_escalate3";
                                    parameter.Direction = ParameterDirection.Input;
                                    parameter.Value = DateTime.Parse(strTime);
                                    cmd.Parameters.Add(parameter);
                                }
                                else
                                {
                                    sbSql.Append(", [sys_escalate3]=NULL");
                                }
                            }
                            else
                            {
                                sbSql.Append(", [sys_escalate3]=NULL");
                            }

                        }
                        else
                        {
                            sbSql.Append(", [sys_resolve]=NULL");
                            sbSql.Append(", [sys_escalate1]=NULL");
                            sbSql.Append(", [sys_escalate2]=NULL");
                            sbSql.Append(", [sys_escalate3]=NULL");
                        }


                        sbSql.Append(" WHERE [sys_action_id]=@sys_action_id");
                        parameter = new SqlParameter();
                        parameter.ParameterName = "@sys_action_id";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = drActTask["sys_action_id"].ToString();
                        cmd.Parameters.Add(parameter);

                        cmd.CommandText = sbSql.ToString();

                        LayUtilDA.RunSqlCmd(cmd);


                        //
                        string strAssignedTo = drActTask["sys_username"].ToString();
                        if (strAssignedTo != "")
                        {
                            DataSet dsNewUser = UserDA.GetUserInfo(strAssignedTo);
                            if (dsNewUser != null && dsNewUser.Tables.Count > 0 && dsNewUser.Tables[0].Rows.Count > 0)
                            {
                                string strEmail = dsNewUser.Tables[0].Rows[0]["sys_email"].ToString();
                                if (strEmail != "")
                                {
                                    if (LayUtil.IsNumeric(drActTask["sys_request_id"].ToString()))
                                    {
                                        LayUtil.SendOutEmail(Application, "action_assign_notify", strEmail, strAssignedTo, "", "request", drActTask["sys_request_id"].ToString(), "", RequestDA.GetReqActionMailCmd(strTaskId));
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        return true;
    }
    protected void imgLdDefEsc_Click(object sender, ImageClickEventArgs e)
    {
        LoadDefEscTime();

        //Task Template
        if (htCtrl["sys_requesttype_id"] != null)
        {
            if (ViewState["sys_requesttype_id"].ToString() != GetUserInputText("sys_requesttype_id"))
            {
                DataSet dsTask = TaskDA.GetReqTypeTasks(GetUserInputText("sys_requesttype_id"));
                if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
                {
                    hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                    //lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " contains predefined " + Application["appactionterm"] + "s which can be added to the " + Application["apprequestterm"] + ". These will replace any existing " + Application["appactionterm"] + "s. Do you want to do this?";
                    lbTaskTemplateMsg.Text = "The selected " + Application["apprequesttypeterm"] + " will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                    dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                    return;
                }
            }
        }
        //Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }

    private void GetSuggestedUser()
    {
        Control ctrlRoot = phCtrls;

        string strSiteManager = "";
        string strDeptManager = "";
        string strSiteId = sys_siteidH.Text;
        string strEUser = sys_eusernameH.Text;

        if (strSiteId != "")
        {
            DataSet dsSite = UserDA.GetSitesById(strSiteId);
            if (dsSite != null && dsSite.Tables.Count > 0 && dsSite.Tables[0].Rows.Count > 0)
            {
                strSiteManager = dsSite.Tables[0].Rows[0]["sys_site_username"].ToString();
            }
        }
        if (strEUser != "")
        {
            DataSet dsDept = EUserDA.GetDeptByEUser(strEUser);
            if (dsDept != null && dsDept.Tables.Count > 0 && dsDept.Tables[0].Rows.Count > 0)
            {
                strDeptManager = dsDept.Tables[0].Rows[0]["sys_eclient_username"].ToString();
            }
        }

        if (SysDA.GetSettingValue("appautopopulateassignedto", Application) == "spec")
        {
            sys_assignedtoH.Text = SysDA.GetSettingValue("appautopopulateassignedtospec", Application);
        }

        string strSuggestedUser = sys_assignedtoH.Text;
        string strSuggestedGrp = sys_assignedtoanalgroupH.Text;

        string strReqTypeId = GetUserInputText("sys_requesttype_id");
        if (strSuggestedUser == "")
        {
            DataSet dsReqType = RequestDA.GetReqTypeById(strReqTypeId);
            if (dsReqType != null && dsReqType.Tables.Count > 0 && dsReqType.Tables[0].Rows.Count > 0)
            {

                DataRow dr = dsReqType.Tables[0].Rows[0];
                string strAssignType = dr["sys_requesttype_assigntype"].ToString();
                if (strAssignType == "1")
                {
                    strSuggestedUser = strSiteManager;
                }
                else if (strAssignType == "2")
                {
                    strSuggestedUser = strDeptManager;
                }
                else if (strAssignType == "3")
                {
                    strSuggestedUser = dr["sys_requesttype_assignto"].ToString();
                }
            }
        }


        if (strSuggestedUser == "")
        {
            string strAutoAssgn = SysDA.GetSettingValue("appautoassignenduserrequest", Application).ToLower();
            string strConstrnSite = SysDA.GetSettingValue("appautoassigncontraintosite", Application);

            Hashtable htReqType = new Hashtable();
            DataSet dsReqTypeAll = RequestDA.GetReqTypeList();
            if (dsReqTypeAll != null && dsReqTypeAll.Tables.Count > 0)
            {
                foreach (DataRow drr in dsReqTypeAll.Tables[0].Rows)
                {
                    htReqType.Add(drr["sys_requesttype_id"].ToString(), drr["sys_requesttypeparent_id"].ToString());
                }

            }

            if (strAutoAssgn == "preference load balancing")
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
            else if (strAutoAssgn == "load balancing")
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

            if (strAutoAssgn == "preference load balancing group")
            {
                string strType = strReqTypeId;
                while (strType != "" && strSuggestedGrp == "")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedGrp = UserDA.GetReqTypeSuggestedGrpSiteRes(strType, strSiteId);
                    }
                    else
                    {
                        strSuggestedGrp = UserDA.GetReqTypeSuggestedGrp(strType);
                    }

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
                if (strSuggestedGrp == "" && SysDA.GetSettingValue("appautoassigncontrainskills", Application) == "false")
                {
                    if (strSiteId != "" && strConstrnSite == "true")
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrpSiteRes(strSiteId);
                    }
                    else
                    {
                        strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
                    }
                }

            }
            if (strAutoAssgn == "load balancing group")
            {
                if (strSiteId != "" && strConstrnSite == "true")
                {
                    strSuggestedGrp = UserDA.GetLoadSuggestedGrpSiteRes(strSiteId);
                }
                else
                {
                    strSuggestedGrp = UserDA.GetLoadSuggestedGrp();
                }
            }
        }

        TextBox ctrluser = (TextBox)ctrlRoot.FindControl("text" + htChildName["sys_assignedto"]);
        TextBox ctrlgrp = (TextBox)ctrlRoot.FindControl("text" + htChildName["sys_assignedtoanalgroup"]);

        if (strSuggestedUser != "")
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

            if (ctrluser != null)
            {
                ctrluser.Text = strSuggestedUser;
            }

            sys_assignedtoH.Text = strSuggestedUser;

            if (ctrlgrp != null)
            {
                ctrlgrp.Text = strSuggestedGrp;
            }

            sys_assignedtoanalgroupH.Text = strSuggestedGrp;

        }
        else
        {
            if (strSuggestedGrp != "")
            {
                if (ctrlgrp != null)
                {
                    ctrlgrp.Text = strSuggestedGrp;
                }
                sys_assignedtoanalgroupH.Text = strSuggestedGrp;
            }
        }

    }

    void tab_TabItemsMoved(object sender, EventArgs e)
    {
        WebTab tab = (WebTab)sender;
        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            if (LayUtil.GetAttribute(node, "ontab") == "true")
            {
                foreach (ContentTabItem item in tab.Tabs)
                {
                    //if (item.Attributes["Item"] == node.Name)
                    if (item.ImageAltText == node.Name)
                    {
                        if (item.VisibleIndex >= 0 && item.VisibleIndex < htTabOrders.Count)
                        {
                            LayUtil.SetAttribute(node, "ontaborder", htTabOrders[item.VisibleIndex.ToString()].ToString());
                        }
                        else
                        {
                            LayUtil.SetAttribute(node, "ontaborder", htTabOrders[item.Index.ToString()].ToString());
                        }
                    }
                }
            }
        }

        SaveXML();
    }

    private bool SaveXML()
    {
        if (strReqLinkType == "1")
        {
            return FormDesignerBR.SaveUserSpawnReqForm(Session["User"].ToString(), strReqClass, xmlForm.InnerXml);
        }
        else
        {
            return FormDesignerBR.SaveUserReqForm(Session["User"].ToString(), strReqClass, xmlForm.InnerXml);
        }
    }
    private void AddTabs(WebTab tab)
    {
        int nAdded = 0;
        byte i = 0;
        htTabOrders = new Hashtable();
        htTabs = new Hashtable();
        htTabText = new Hashtable();
        htTabName = new Hashtable();
       
        while (nAdded < htTabNode.Count && i < byte.MaxValue)
        {
            if (htTabNode[i.ToString()] == null)
            {
                i++;
                continue;
            }
            XmlNode node = (XmlNode)htTabNode[i.ToString()];
            if (SysDA.GetSettingValue("apprpclevel", Application) != "true" && node.Name == "sys_button14")
            {
                i++;
                nAdded++;
                continue;
            }

            htTabOrders[tab.Tabs.Count.ToString()] = i.ToString();
            ContentTabItem tabItem = new ContentTabItem();
            tab.Tabs.Add(tabItem);
            tabItem.ScrollBars = ContentOverflow.Auto;

            if (node.InnerText == "sys_problemdesc" || node.InnerText == "sys_solutiondesc")
            {
                tabItem.ScrollBars = ContentOverflow.Hidden;
                tabItem.Text = LayUtil.GetAttribute(node, "caption");
                string strVal = GetValue(node.InnerText);

                if (!IsiPhone())
                {
                    if (LayUtil.GetAttribute(node, "htmleditor") == "true")
                    {
                        HtmlTable table = new HtmlTable();
                        HtmlTableRow row = new HtmlTableRow();
                        HtmlTableCell cell = new HtmlTableCell();
                        table.Width = "100%";


                        row.Cells.Add(cell);
                        HyperLink hl = new HyperLink();

                        hl.ImageUrl = "~/Application_Images/16x16/icons_edit_16px.png";
                        hl.ToolTip = "Show/Hide Tools";

                        hl.ID = "hl" + node.Name;

                        cell.Align = "right";
                        cell.Height = "16px";
                        cell.Controls.Add(hl);

                        table.Controls.Add(row);


                        row = new HtmlTableRow();
                        cell = new HtmlTableCell();

                        cell.Width = "100%";


                        row.Cells.Add(cell);

                        RadEditor edt = LayUtil.CreateHTMLEditorInTab(Page, node);
                        //edt.MaxHtmlLength = 5;
                        edt.OnClientCommandExecuting="OnClientCommandExecuting";
                        //edt.ContentAreaMode = EditorContentAreaMode.Div;

                        if (IsiPhone())
                        {
                            edt.EditModes = EditModes.Html;
                        }
                        edt.Content = strVal;
                        edt.EnableResize = false;


                        cell.VAlign = "top";
                        cell.Controls.Add(edt);
                        table.Controls.Add(row);

                        //tabItem.cli
                        tabItem.Controls.Add(table);

                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(node.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(edt.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("HTML");
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlSize[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(htColSize[node.InnerText].ToString());
                        sbTxt.Append("';");
                        
                        nLen++;

                        if (node.InnerText == "sys_problemdesc")
                        {
                            edtProblem = edt as RadEditor;
                        }
                        else if (node.InnerText == "sys_solutiondesc")
                        {
                            edtSolution = edt as RadEditor;
                        }
                        hl.NavigateUrl = "javascript:Show_HTMLTools(this, '" + edt.ClientID + "')";
                    }

                    else
                    {
                        TextBox ctrl = new TextBox();
                        ctrl.ID = "text" + node.Name;

                        ctrl.Font.Name = LayUtil.GetAttribute(node, "font-family");
                        ctrl.Font.Size = new FontUnit(LayUtil.GetAttribute(node, "font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(node, "color"));

                        ctrl.Width = new Unit("99.5%");

                        ctrl.TextMode = TextBoxMode.MultiLine;
                        ctrl.Height = new Unit((tab.Height.Value - 32).ToString() + "px");

                        ctrl.Style["overflow"] = "auto";
                        ctrl.Style["position"] = "relative";

                        /*
                        if (node.InnerText == "sys_problemdesc")
                        {
                            RadEditor edtHelp = new RadEditor();
                            edtHelp.Content = strVal;
                            ctrl.Text = edtHelp.Text;
                        }
                        else
                        {
                            ctrl.Text = strVal;
                        }
                        */
                        ctrl.Text = strVal;
                        string strMaxSize = htColSize[node.InnerText].ToString();
                        if (LayUtil.IsNumeric(strMaxSize))
                        {
                            try
                            {
                                ctrl.MaxLength = int.Parse(strMaxSize);
                            }
                            catch
                            {
                            }
                        }

                        tabItem.Controls.Add(ctrl);
                        sbTxt.Append("arrCtrlID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(node.InnerText);
                        sbTxt.Append("';");
                        sbTxt.Append("arrClientID[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append(ctrl.ClientID);
                        sbTxt.Append("';");
                        sbTxt.Append("arrCtrlType[");
                        sbTxt.Append(nLen.ToString());
                        sbTxt.Append("]='");
                        sbTxt.Append("TEXTBOX");
                        sbTxt.Append("';");
                        nLen++;
                    }
                }
                else
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();
                    table.Width = "100%";


                    row.Cells.Add(cell);

                   /* HyperLink hl = new HyperLink();

                    hl.ImageUrl = "~/Application_Images/16x16/icons_edit_16px.png";
                    hl.ToolTip = "Show/Hide Tools";

                    hl.ID = "hl" + node.Name;

                    cell.Align = "right";
                    cell.Height = "16px";
                    cell.Controls.Add(hl);*/

                    table.Controls.Add(row);


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    cell.Width = "100%";
                    row.Cells.Add(cell);

                    table.Controls.Add(row);
                    tabItem.Controls.Add(table);

                    TextBox ctrl = new TextBox();
                    ctrl.ID = "text" + node.Name;

                    ctrl.Font.Name = LayUtil.GetAttribute(node, "font-family");
                    ctrl.Font.Size = new FontUnit(LayUtil.GetAttribute(node, "font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(LayUtil.GetAttribute(node, "color"));

                    ctrl.Width = new Unit("99.5%");

                    ctrl.TextMode = TextBoxMode.MultiLine;
                    ctrl.Height = new Unit((tab.Height.Value - 32).ToString() + "px");

                    ctrl.Style["overflow"] = "auto";
                    ctrl.Style["position"] = "relative";

                    /*
                    if (node.InnerText == "sys_problemdesc")
                    {
                        RadEditor edtHelp = new RadEditor();
                        edtHelp.Content = strVal;
                        ctrl.Text = edtHelp.Text;
                    }
                    else
                    {
                        ctrl.Text = strVal;
                    }
                    */
                    ctrl.Text = strVal;

                    string strMaxSize = htColSize[node.InnerText].ToString();
                    if (LayUtil.IsNumeric(strMaxSize))
                    {
                        try
                        {
                            ctrl.MaxLength = int.Parse(strMaxSize);
                        }
                        catch
                        {
                        }
                    }


                    tabItem.Controls.Add(ctrl);
                    sbTxt.Append("arrCtrlID[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append(node.InnerText);
                    sbTxt.Append("';");
                    sbTxt.Append("arrClientID[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append(ctrl.ClientID);
                    sbTxt.Append("';");
                    sbTxt.Append("arrCtrlType[");
                    sbTxt.Append(nLen.ToString());
                    sbTxt.Append("]='");
                    sbTxt.Append("TEXTBOX");
                    sbTxt.Append("';");
                    nLen++;
                }
            }
            else
            {
                string strTabImg = LayUtil.GetAttribute(node, "tabimage");
                if (strTabImg != "")
                {
                    tabItem.ImageUrl = strTabImg;
                    tabItem.ToolTip = LayUtil.RplTm(Application, LayUtil.GetAttribute(node, "tabimagetitle"));
                }

                if (node.Name == "sys_button3")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);

                    System.Web.UI.WebControls.ImageButton btnAddTask = new System.Web.UI.WebControls.ImageButton();
                    btnAddTask.Style["Cursor"] = "pointer !important";

                    btnAddTask.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    if (bAllowChange)
                    {
                        btnAddTask.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAddTask.Click += new ImageClickEventHandler(btnAddTask_Click);
                    }
                    else
                    {
                        btnAddTask.OnClientClick = "CheckAllowed(this);document.location='" + "TaskInfo.aspx?from=req&field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    btnAddTask.ToolTip = "Add " + Application["appactionterm"];
                    btnAddTask.ID = "btnAddTask";

                    cell.Align = "right";
                    cell.Controls.Add(btnAddTask);
                    table.Controls.Add(row);


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgTask.Visible = true;

                    phDG.Controls.Remove(dgTask);

                    cell.Controls.Add(dgTask);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    htTabs["task"] = tabItem;
                    htTabText["task"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["task"] = node.Name;

                    UpdateTaskList();


                }
                else if (node.Name == "sys_button5")
                {

                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);


                    WebDropDown ddField = new WebDropDown();

                    DropDownItem item;
                    item = new DropDownItem("(None)", "");
                    ddField.Items.Add(item);

                    DataSet dsField = DataDesignDA.GetTblCol("request");
                    if (dsField != null && dsField.Tables.Count > 0)
                    {
                        foreach (DataRow dr in dsField.Tables[0].Rows)
                        {
                            item = new DropDownItem(dr["ColName"].ToString(), dr["ColName"].ToString());
                            ddField.Items.Add(item);
                        }
                    }

                    ddField.EnableDropDownAsChild = false;
                    ddField.Style["z-index"] = "20000";
                    ddField.Width = new Unit("240px");

                    ddField.AutoPostBackFlags.ValueChanged = Infragistics.Web.UI.AutoPostBackFlag.On;
                    ddField.ValueChanged += new DropDownValueChangedEventHandler(ddField_ValueChanged);
                    ddField.DisplayMode = DropDownDisplayMode.DropDownList;

                    cell.Align = "right";
                    cell.Controls.Add(ddField);
                    table.Controls.Add(row);


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgHistory.Visible = true;


                    phDG.Controls.Remove(dgHistory);

                    cell.Controls.Add(dgHistory);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    htTabs["history"] = tabItem;
                    htTabText["history"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["history"] = node.Name;

                    UpdateHistoryList(ddField.SelectedValue);
                    tabItem.Text = LayUtil.RplTm(Application, node.InnerText);

                }
                else if (node.Name == "sys_button6")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();

                    row.Cells.Add(cell);

                    System.Web.UI.WebControls.ImageButton btnAttach = new System.Web.UI.WebControls.ImageButton();
                    btnAttach.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    btnAttach.Style["Cursor"] = "pointer !important";
                    if (bAllowChange)
                    {
                        btnAttach.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAttach.Click += new ImageClickEventHandler(btnAttach_Click);
                    }
                    else
                    {
                        btnAttach.OnClientClick = "CheckAllowed(this);document.location='" + "ReqAttach.aspx?field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    btnAttach.ID = "btnAttach";
                    btnAttach.ToolTip = "Add Attachment";

                    cell.Controls.Add(btnAttach);

                    cell.Align = "right";
                    table.Controls.Add(row);


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgAttach.Visible = true;


                    if (Session["Role"].ToString() == "enduser" && SysDA.GetSettingValue("appenduserattachdelete", Application) == "false")
                    {
                        TemplateDataField templateColumn1 = (TemplateDataField)dgAttach.Columns["Delete"];
                        if (templateColumn1 != null)
                        {
                            dgAttach.Columns.Remove(templateColumn1);
                        }
                    }

                    phDG.Controls.Remove(dgAttach);

                    cell.Controls.Add(dgAttach);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    htTabs["attach"] = tabItem;
                    htTabText["attach"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["attach"] = node.Name;

                    UpdateAttachList();
                    Session["ButtonBack"] = "ReqInfo.aspx?field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"];
                }
                else if (node.Name == "sys_button7")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();
                    
                    strCommentHtml = LayUtil.GetAttribute(node, "commenthtmleditor");
                    row.Cells.Add(cell);


                    System.Web.UI.WebControls.ImageButton btnAddCommnt = new System.Web.UI.WebControls.ImageButton();

                    btnAddCommnt.Style["Cursor"] = "pointer !important";
                    btnAddCommnt.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    if (bAllowChange)
                    {
                        btnAddCommnt.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAddCommnt.Click += new ImageClickEventHandler(btnAddCommnt_Click);
                    }
                    else
                    {
                        btnAddCommnt.OnClientClick = "CheckAllowed(this);document.location='" + "ReqCommtInfo.aspx?from=req&field=" + node.Name + "&CommentHTML=" + strCommentHtml + "&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    btnAddCommnt.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    btnAddCommnt.ToolTip = "Add " + Application["appcommentterm"];

                    btnAddCommnt.ID = "btnAddCommnt";


                    cell.Align = "right";
                    cell.Controls.Add(btnAddCommnt);
                    table.Controls.Add(row);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();



                    hdgCommCell = cell;


                    row.Cells.Add(cell);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    htTabs["comment"] = tabItem;
                    htTabText["comment"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["comment"] = node.Name;


                    UpdateCommntList();
                }
                else if (node.Name == "sys_button12")
                {

                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);



                    System.Web.UI.WebControls.ImageButton btnAddCallback = new System.Web.UI.WebControls.ImageButton();
                    btnAddCallback.Style["Cursor"] = "pointer !important";

                    btnAddCallback.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    if (bAllowChange)
                    {
                        btnAddCallback.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAddCallback.Click += new ImageClickEventHandler(btnAddCallback_Click);
                    }
                    else
                    {
                        btnAddCallback.OnClientClick = "CheckAllowed(this);document.location='" + "ReqCallbackInfo.aspx?from=req&field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    btnAddCallback.ToolTip = "Add Callback";

                    btnAddCallback.ID = "btnAddCallback";

                    cell.Align = "right";
                    cell.Controls.Add(btnAddCallback);
                    table.Controls.Add(row);


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgCallback.Visible = true;


                    phDG.Controls.Remove(dgCallback);

                    cell.Controls.Add(dgCallback);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    htTabs["callback"] = tabItem;
                    htTabText["callback"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["callback"] = node.Name;


                    UpdateCallbackList();

                    Session["CallbackBack"] = "ReqInfo.aspx?back=true&field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"];

                }
                else if (node.Name == "sys_button13")
                {

                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);



                    System.Web.UI.WebControls.ImageButton btnAddCost = new System.Web.UI.WebControls.ImageButton();
                    btnAddCost.Style["Cursor"] = "pointer !important";

                    btnAddCost.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    if (bAllowChange)
                    {
                        btnAddCost.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAddCost.Click += new ImageClickEventHandler(btnAddCost_Click);
                    }
                    else
                    {
                        btnAddCost.OnClientClick = "CheckAllowed(this);document.location='" + "CostTransn.aspx?back=Req&field=" + node.Name + "&from=req&sys_request_id=" + ViewState["sys_request_id"] + "'; return false;";
                    }

                    btnAddCost.ToolTip = "Add Cost";

                    btnAddCost.ID = "btnAddCost";

                    cell.Align = "right";
                    cell.Controls.Add(btnAddCost);
                    table.Controls.Add(row);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgCost.Visible = true;


                    phDG.Controls.Remove(dgCost);

                    cell.Controls.Add(dgCost);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    tabItem.Controls.Add(table);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);

                    lbCost = new Label();

                    lbCost.Font.Bold = true;

                    cell.Align = "right";
                    cell.Controls.Add(lbCost);
                    table.Controls.Add(row);

                    htTabs["cost"] = tabItem;
                    htTabText["cost"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["cost"] = node.Name;


                    UpdateCostList();

                    Session["CostBack"] = "ReqInfo.aspx?back=true&field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"];
                }
                else if (node.Name == "sys_button14")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();
                    table.Width = "100%";

                    row.Cells.Add(cell);


                    System.Web.UI.WebControls.ImageButton btn = new System.Web.UI.WebControls.ImageButton();

                    btn.Style["Cursor"] = "pointer !important";
                    btn.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    btn.ToolTip = "Edit Links";

                    btn.ID = "btnEditLink";
                    btn.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                    btn.Click += new ImageClickEventHandler(btnLink_Click);

                    cell.Align = "right";
                    cell.Width = "100%";
                    cell.Controls.Add(btn);
                    table.Controls.Add(row);

                    tabItem.Controls.Add(table);
                    tabLink.Visible = true;
                    phDG.Controls.Remove(tabLink);
                    tabItem.Controls.Add(tabLink);

                    tabLink.Tabs.Clear();
                    ContentTabItem subItem = new ContentTabItem();
                    subItem.Text = "Problems";
                    tabLink.Tabs.Add(subItem);
                    subItem.Controls.Add(dgProblem);
                    dgProblem.Visible = true;

                    subItem = new ContentTabItem();
                    subItem.Text = "Changes";
                    tabLink.Tabs.Add(subItem);
                    subItem.Controls.Add(dgChange);
                    dgChange.Visible = true;


                    tabLink.Tabs[0].BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());
                    tabLink.Tabs[1].BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());

                    htTabs["link"] = tabItem;
                    htTabText["link"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["link"] = node.Name;

                    UpdateLinkList();
                    tabItem.Text = LayUtil.RplTm(Application, node.InnerText);
                    Session["ButtonBack"] = "ReqInfo.aspx?field=" + node.Name + "&sys_request_id=" + ViewState["sys_request_id"];
                }

            }

            tabItem.BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());

            tabItem.ImageAltText = node.Name;

            if (ViewState["field"].ToString() == node.Name)
            {
                tab.SelectedIndex = tabItem.Index;
            }

            tab.ClientEvents.SelectedIndexChanged = "";
            i++;
            nAdded++;
        }
    }

    public WebHierarchicalDataGrid CreateWHDG()
    {
        WebHierarchicalDataGrid hdg = new WebHierarchicalDataGrid();
        hdg.AutoGenerateBands = false;
        hdg.AutoGenerateColumns = false;
        hdg.DataKeyFields = "sys_comment_id";
        hdg.InitialDataBindDepth = -1;
        hdg.RowIslandsPopulated += new ContainerRowEventHandler(hdgData_RowIslandsPopulated);
        hdg.Width = new Unit("100%");
        hdg.EnableViewState = false;

        AddColumn(hdg, "sys_commentdate", "Date", "20%", this);
        AddColumn(hdg, "sys_comment_user", "Author", "15%", this);
        AddColumn(hdg, "sys_commentsubject", "Subject", "40%", this);
        AddColumn(hdg, "sys_visible_euser", "", "5%", this);
        if (GetCommntDelete())
        {
            AddColumn(hdg, "delete", "Date", "5%", this);
        }

        Band band = new Band();
        band.DataMember = "dvDetail";
        band.Key = "DetailInfo";
        band.DataKeyFields = "sys_comment_id";
        band.AutoGenerateColumns = false;
        band.ShowHeader = false;
        band.ShowFooter = false;
        band.IsSelfReference = false;

        AddColumn(band, "sys_comment", "", "100%", this);

        return hdg;
    }

    private void AddColumn(Band hdg, string strField, string strHeader, string strWidth, Analyst_ReqInfo page)
    {
        TemplateDataField templateColumn1 = (TemplateDataField)hdg.Columns[strField];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = strField;
            field1.Header.Text = strHeader;

            hdg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)hdg.Columns[strField];

        templateColumn1.Width = new Unit(strWidth);

        templateColumn1.ItemTemplate = new CommentCell(strField, page);
    }
    private void AddColumn(WebHierarchicalDataGrid hdg, string strField, string strHeader, string strWidth, Analyst_ReqInfo page)
    {
        TemplateDataField templateColumn1 = (TemplateDataField)hdg.Columns[strField];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = strField;
            field1.Header.Text = strHeader;

            hdg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)hdg.Columns[strField];

        templateColumn1.Width = new Unit(strWidth);

        templateColumn1.ItemTemplate = new CommentCell(strField, page);
    }

    private class CommentCell : ITemplate
    {
        #region ITemplate Members

        public string strField;
        public Analyst_ReqInfo ParentPage;
        public CommentCell(string field, Analyst_ReqInfo page)
        {
            strField = field;
            ParentPage = page;
        }
        public void InstantiateIn(Control container)
        {
            Infragistics.Web.UI.DataSourceControls.WebHierarchyData data = (Infragistics.Web.UI.DataSourceControls.WebHierarchyData)((TemplateContainer)container).DataItem;
            if (strField != "delete")
            {
                HyperLink ctrl = new HyperLink();
                ctrl.NavigateUrl = "ReqCommtInfo.aspx?from=req&field=" + ParentPage.htTabName["comment"].ToString() + "&sys_comment_id=" + ((DataRowView)data.Item)["sys_comment_id"].ToString();

                if (strField == "sys_comment_user")
                {
                    ctrl.Text = ParentPage.GetCommntAuthor(((DataRowView)data.Item)["sys_comment_user"].ToString(), ((DataRowView)data.Item)["sys_comment_euser"].ToString());
                }
                else if (strField == "sys_visible_euser")
                {
                    ctrl.Text = ParentPage.GetCommntVisibleText(((DataRowView)data.Item)["sys_comment_euser"].ToString());
                }
                else
                {
                    ctrl.Text = ((DataRowView)data.Item)[strField].ToString();
                }


                ctrl.ID = "hl" + strField;
                container.Controls.Add(ctrl);
            }
            else
            {
                System.Web.UI.WebControls.ImageButton img = new System.Web.UI.WebControls.ImageButton();
                img.ImageUrl = "Application_Images/16x16/delete_icon_16px.png";
                img.OnClientClick = "btnDelPre_Clicked(this,'comment');return false;";
                img.ToolTip = "Delete";
                img.AlternateText = ((DataRowView)data.Item)["sys_comment_id"].ToString();
            }
        }


        #endregion
    }

    void btnAddTask_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "TaskInfo.aspx?from=req&field=" + htTabName["task"] + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    void btnAttach_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            Session["ButtonBack"] = "ReqInfo.aspx?from=req&field=" + htTabName["attach"] + "&sys_request_id=" + ViewState["sys_request_id"];
            ViewState["PostConfirmURL"] = "ReqAttach.aspx?from=req&field=" + htTabName["attach"] + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    void btnAddCallback_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            if (Session["CallbackBack"] != null && ViewState["sys_request_id"] != null && !Session["CallbackBack"].ToString().Contains(ViewState["sys_request_id"].ToString()))
                Session["CallbackBack"] = Session["CallbackBack"].ToString() + ViewState["sys_request_id"].ToString();
            ViewState["PostConfirmURL"] = "ReqCallbackInfo.aspx?from=req&field=" + htTabName["callback"] + "&sys_request_id=" + ViewState["sys_request_id"];
            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }
    void btnAddCommnt_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            ViewState["PostConfirmURL"] = "ReqCommtInfo.aspx?from=req&field=" + htTabName["comment"] + "&CommentHTML=" + strCommentHtml + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    void btnAddCost_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            if (Session["CostBack"] != null && ViewState["sys_request_id"] != null && !Session["CostBack"].ToString().Contains(ViewState["sys_request_id"].ToString()))
                Session["CostBack"] = Session["CostBack"].ToString() + ViewState["sys_request_id"].ToString();
            ViewState["PostConfirmURL"] = "CostTransn.aspx?back=Req&from=req&field=" + htTabName["cost"] + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }
    void btnLink_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveReqData())
        {
            Session["ButtonBack"] = "ReqInfo.aspx?field=" + htTabName["link"] + "&sys_request_id=" + ViewState["sys_request_id"];
            ViewState["PostConfirmURL"] = "ReqLink.aspx?back=Req&from=req&field=" + htTabName["link"] + "&sys_request_id=" + ViewState["sys_request_id"];

            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    public string GetAttachUrl(object obj)
    {
        return "~/GetAttachment.aspx?fn=" + Server.UrlEncode(obj.ToString()) + "&sys_request_id=" + ViewState["sys_request_id"].ToString();
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        dgTask.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgAttach.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgCost.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgProblem.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgChange.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgHistory.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgCallback.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
        LayUtil.SetFont(this.dgTask, Application);
        LayUtil.SetFont(this.dgAttach, Application);
        LayUtil.SetFont(this.dgCost, Application);
        LayUtil.SetFont(this.dgProblem, Application);
        LayUtil.SetFont(this.dgChange, Application);
        LayUtil.SetFont(this.dgHistory, Application);
        LayUtil.SetFont(this.dgCallback, Application);
    }

    private void CheckTabSize()
    {
        if (ele_tabsize.Text != "")
        {
            string strTabSize = ele_tabsize.Text;

            foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
            {
                if (child.Name == "tab" && strTabSize != "")
                {
                    string strWidth = strTabSize.Substring(0, strTabSize.IndexOf(","));
                    string strHeight = strTabSize.Substring(strTabSize.IndexOf(",") + 1);

                    child.SetAttribute("width", strWidth);
                    child.SetAttribute("height", strHeight);
                }
            }

            ele_tabsize.Text = "";
            SaveXML();
        }
    }
    private void UpdateTaskList()
    {
        dgTask.ClearDataSource();
        dgTask.DataSource = RequestDA.GetReqTasks(ViewState["sys_request_id"].ToString());
        dgTask.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["task"];

        tabItem.Text = htTabText["task"] + " (" + ((DataSet)dgTask.DataSource).Tables[0].Rows.Count + ")";
    }
    private void UpdateAttachList()
    {
        dgAttach.ClearDataSource();

        string strReqId = ViewState["sys_request_id"].ToString();
        dgAttach.DataSource = RequestDA.GetReqAttachment(strReqId);
        dgAttach.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["attach"];

        tabItem.Text = htTabText["attach"] + " (" + ((DataSet)dgAttach.DataSource).Tables[0].Rows.Count + ")";
    }
    private void UpdateCallbackList()
    {
        dgCallback.ClearDataSource();
        string strReqId = ViewState["sys_request_id"].ToString();
        dgCallback.DataSource = RequestDA.GetReqCallback(strReqId);
        dgCallback.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["callback"];

        tabItem.Text = htTabText["callback"] + " (" + ((DataSet)dgCallback.DataSource).Tables[0].Rows.Count + ")";
    }
    #region SetHistory Column Tooltip
    private void SetColTooltip(WebDataGrid dgHis)
    {
        AddColumn("sys_changedfrom", dgHis);
        AddColumn("sys_changedto", dgHis);
    }
    private void AddColumn(string strField, WebDataGrid dg)
    {
        TemplateDataField templateColumn1 = (TemplateDataField)dg.Columns[strField];
        templateColumn1.ItemTemplate = new GridCell(strField, this);
    }
    private class GridCell : ITemplate
    {
        #region ITemplate Members

        private string strField = "";
        Analyst_ReqInfo page;
        public GridCell(string Field, Analyst_ReqInfo pPage)
        {
            strField = Field;
            page = pPage;
        }
        public void InstantiateIn(Control container)
        {
            string strVal = ((DataRowView)((TemplateContainer)container).DataItem)[strField].ToString();
            if (strVal == "")
                return;
            strVal = LayUtil.RemoveTroublesomeCharacters(strVal);

            RadEditor rad = new RadEditor();
            rad.Content = strVal;
            string strText = rad.Text;
            

            Label lb = new Label();
            lb.Text = strField;
            lb.ID = "lb" + strField;
            if (strText.Length < 15)
            {
                lb.Text = strText;
                container.Controls.Add(lb);
            }
            else
            {
                lb.Text = strText.Substring(0, 12) + "...";

                RadToolTip tooltip = new RadToolTip();
                tooltip.ID = "tt" + strField;


                tooltip.TargetControlID = lb.ID;
                tooltip.Position = ToolTipPosition.BottomCenter;

                tooltip.Text = strVal;
                tooltip.Style["Z-Index"] = "20001";

                tooltip.Width = new Unit("600px");

                container.Controls.Add(lb);
                container.Controls.Add(tooltip);
            }


        }
        #endregion
    }

    #endregion
    private void UpdateHistoryList(string strField)
    {
        SetColTooltip(dgHistory);
        dgHistory.ClearDataSource();
        dgHistory.DataSource = RequestDA.GetReqHistory(ViewState["sys_request_id"].ToString(), strField);
        dgHistory.DataBind();
    }

    #region Link
    private void UpdateLinkList()
    {
        UpdateLinkProblemList();
        UpdateLinkChgList();
    }
    private void UpdateLinkProblemList()
    {
        dgProblem.ClearDataSource();
        string strReqId = ViewState["sys_request_id"].ToString();

        dgProblem.DataSource = RequestDA.GetLinkedProblem(strReqId);
        dgProblem.DataBind();

    }
    private void UpdateLinkChgList()
    {
        dgChange.ClearDataSource();
        string strReqId = ViewState["sys_request_id"].ToString();

        dgChange.DataSource = RequestDA.GetLinkedChange(strReqId);
        dgChange.DataBind();
    }

    private void UpdateCommntList()
    {
        Hashtable htOpenStatus = new Hashtable();
        DataSet dsOpenStatus = RequestDA.GetOpenStatus();
        if (dsOpenStatus != null && dsOpenStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsOpenStatus.Tables[0].Rows)
            {
                htOpenStatus.Add(dr["sys_status_ID"].ToString(), "true");
            }
        }
        DataSet dsReq = RequestDA.GetReqById(ViewState["sys_request_id"].ToString());
        if (dsReq != null && dsReq.Tables.Count > 0 && dsReq.Tables[0].Rows.Count > 0)
        {
            if ((htOpenStatus[dsReq.Tables[0].Rows[0]["sys_requeststatus"].ToString()] != null))
            {
                bClosed = false;
            }
        }


        string strGrpPriv = "1";
        DataSet dsUser = UserDA.GetUserInfo(Session["User"].ToString());

        if (dsUser != null || dsUser.Tables.Count > 0 && dsUser.Tables[0].Rows.Count > 0)
        {
            strGrpPriv = dsUser.Tables[0].Rows[0]["sys_comment_grouppriv"].ToString();
            if (dsUser.Tables[0].Rows[0]["sys_comment_delete"].ToString() == "1")
            {
                bDelComment = true;
            }
        }

        DataSet ds = null;
        if (strGrpPriv == "1")
        {
            ds = RequestDA.GetCommentByReqIdWithGrpRes(ViewState["sys_request_id"].ToString(), Session["User"].ToString());
        }
        else
        {
            ds = RequestDA.GetCommentByReqIdUser(ViewState["sys_request_id"].ToString(), Session["User"].ToString());
        }

        hdgCommCell.Controls.Clear();
        Control c = LayUtil.LoadControl(Page, "ReqHdgCommt.ascx", ViewState["sys_request_id"].ToString(), htTabName, ds, bClosed, bDelComment,strCommentHtml);

        hdgCommCell.Controls.Add(c);

        ContentTabItem tabItem = (ContentTabItem)htTabs["comment"];

        tabItem.Text = htTabText["comment"] + " (" + ds.Tables[0].Rows.Count + ")";
    }
    public string GetProblemUrl(object obj)
    {
        return "ProblemInfo.aspx?field=" + htTabName["link"] + "&from=req&sys_request_id=" + ViewState["sys_request_id"].ToString() + "&sys_problem_id=" + obj.ToString();
    }
    public string GetChangeUrl(object obj)
    {
        return "ChangeInfo.aspx?field=" + htTabName["link"] + "&from=req&sys_request_id=" + ViewState["sys_request_id"].ToString() + "&sys_change_id=" + obj.ToString();
    }

    #endregion
    private void UpdateCostList()
    {
        dgCost.ClearDataSource();

        DataSet dsTotal = CostDA.GetReqTotalCost(ViewState["sys_request_id"].ToString());
        if (dsTotal != null && dsTotal.Tables.Count > 0 && dsTotal.Tables[0].Rows.Count > 0)
        {
            lbCost.Text = "Total: " + CostDA.FormatCost(dsTotal.Tables[0].Rows[0]["CostTotal"], SysDA.GetSettingValue("appcostcurrency", Application));
        }
        else
        {
            lbCost.Text = "Total: " + SysDA.GetSettingValue("appcostcurrency", Application) + "0.00";
        }

        dgCost.DataSource = CostDA.GetReqCost(ViewState["sys_request_id"].ToString()); ;

        dgCost.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["cost"];

        tabItem.Text = htTabText["cost"] + " (" + ((DataSet)dgCost.DataSource).Tables[0].Rows.Count + ")";
    }


    protected void btnDel_Click(object sender, ImageClickEventArgs e)
    {
        if (textItemH.Text == "attach")
        {
            string strFile = textIdH.Text;
            RequestDA.DelReqFile(ViewState["sys_request_id"].ToString(), strFile);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateAttachList();
        }
        else if (textItemH.Text == "callback")
        {
            string strId = textIdH.Text;
            RequestDA.DeleteReqCallback(strId);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateCallbackList();
        }
        else if (textItemH.Text == "cost")
        {
            string strId = textIdH.Text;
            CostDA.DelCostTransactions(strId);
            CostDA.DelCost(strId);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateCostList();
        }
        else if (textItemH.Text == "comment")
        {
            string strId = textIdH.Text;
            RequestDA.DelComment(strId);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateCommntList();
        }
        else if (textItemH.Text == "task")
        {
            string strId = textIdH.Text;
            RequestDA.DelTask(strId);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateTaskList();
        }
    }
    protected void btnUpload_Click(object sender, ImageClickEventArgs e)
    {
        string strFile = fuMain.FileName;
        string strExt = LayUtil.GetFileExt(strFile);
        if (strExt == "dll" || strExt == "hta" || strExt == "exe" || strExt == "vbs" || strExt == "asp" || strExt == "aspx" || strExt == "js")
        {
            lbAttachMsg.Text = "You are not permitted to upload illegal file types.";
            return;
        }

        if (RequestDA.ReqFileExist(ViewState["sys_request_id"].ToString(), strFile))
        {
            if (Session["Role"].ToString() == "enduser" && Application["appenduserattachoverwrite"].ToString() == "false")
            {
                lbAttachMsg.Text = "You are not permitted to overwrite attachments.";
                return;
            }
        }

        try
        {
            fuMain.SaveAs(RequestDA.GetReqFileFullPath(ViewState["sys_request_id"].ToString(), strFile));
            dialogAttachInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateAttachList();
        }
        catch
        {
            lbAttachMsg.Text = "Failed to upload attachment.";
            return;
        }
    }

    protected void ddField_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        WebDropDown ddField = (WebDropDown)sender;
        UpdateHistoryList(ddField.SelectedValue);

        WebTab tabMain = (WebTab)phCtrls.FindControl("tabMain");
        if (tabMain != null)
        {
            tabMain.SelectedIndex = ((ContentTabItem)htTabs["history"]).Index;
        }
    }

    public string GetCallbackUrl(object objVal)
    {
        string strCallId = objVal.ToString();
        return "ReqCallbackInfo.aspx?field=" + htTabName["callback"] + "&sys_callback_id=" + strCallId + "&sys_request_id=" + ViewState["sys_request_id"].ToString();
    }
    public string GetCallbackStatus(object objVal)
    {
        string strVal = objVal.ToString();
        if (strVal.ToLower() == "true")
            return "Enabled";
        else
            return "Disabled";
    }

    public string GetTaskStatus(object objVal)
    {
        string strVal = objVal.ToString();
        if (strVal == "0")
            return "Scheduled";
        else
            return "Complete";
    }

    public string GetTaskUrl(object obj)
    {
        return "TaskInfo.aspx?from=req&field=" + htTabName["task"] + "&sys_action_id=" + obj.ToString() + "&sys_request_id=" + ViewState["sys_request_id"];
    }

    public string GetCostTransactionUrl(object objVal)
    {
        string strCostId = objVal.ToString();
        return "CostTransn.aspx?back=Req&field=" + htTabName["cost"] + "&sys_cost_id=" + strCostId + "&sys_request_id=" + ViewState["sys_request_id"];
    }
    public string GetCommntUrl(object obj)
    {
        return "ReqCommtInfo.aspx?from=req&field=" + htTabName["comment"] + "&CommentHTML=" + strCommentHtml + "&inline=" + ViewState["inline"] + "&sys_comment_id=" + obj.ToString() + "&sys_request_id=" + ViewState["sys_request_id"];
    }
    public string GetCommntAuthor(object objUser, object objEUser)
    {
        string strUser = objUser.ToString();
        if (strUser != "")
            return strUser;

        return objEUser.ToString();
    }

    public string GetCommntVisibleText(object obj)
    {
        if (obj.ToString() == "1")
        {
            return "Public";
        }
        return "Private";
    }

    public bool GetCommntDelete()
    {
        if (bDelComment && !bClosed)
        {
            return true;
        }
        else
            return false;
    }
    protected void hdgData_RowIslandsPopulated(object sender, ContainerRowEventArgs e)
    {
        DataRowView dr = (DataRowView)(((WebHierarchyData)e.Row.DataItem).Item);
        string strId = dr["sys_comment_id"].ToString();
        RequestDA.SetCommentRead(strId, Session["User"].ToString());
    }
    public string GetNoTaskText()
    {
        return "No " + Application["appactionterm"] + "s";
    }
    public string GetNoCommntText()
    {
        return "No " + Application["appcommentterm"] + "s";
    }

    protected void btnCompltTask_Click(object sender, ImageClickEventArgs e)
    {
        string strActionId = textIdH.Text;

        if (LayUtil.IsNumeric(strActionId))
        {
            Complete_Task(strActionId);
            dialogComComfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateTaskList();
        }
    }

    protected void Complete_Task(string strActionId)
    {
        DataSet dsMainTask = RequestDA.GetActionById(strActionId);
        if (dsMainTask == null || dsMainTask.Tables.Count <= 0 || dsMainTask.Tables[0].Rows.Count <= 0)
            return;

        DataRow drMainTask = dsMainTask.Tables[0].Rows[0];

        string strReqId = drMainTask["sys_request_id"].ToString();
        string strProblemId = drMainTask["sys_problem_id"].ToString();
        string strChangeId = drMainTask["sys_change_id"].ToString();

        string strActionDate = drMainTask["sys_actiondate"].ToString();

        if (strActionDate != "")
        {
            if (strReqId != "")
            {
                LoadSus(strReqId, "", strActionDate);
            }
            else if (strProblemId != "")
            {
                LoadSus("", strProblemId, strActionDate);
            }
            else if (strChangeId != "")
            {
                LoadSus("", "", strActionDate);
            }
            else
            {
                LoadSus("", "", strActionDate);
            }

            string strDurHD = ((int)(RequestDA.GetReqDuration(strActionDate, DateTime.Now, listSus))).ToString();
            string strDur24 = ((int)(RequestDA.GetReqDuration24Min(strActionDate, DateTime.Now))).ToString();

            RequestDA.UpdActionComplete(strActionId, DateTime.Now, strDurHD, strDur24);
        }
        else
        {
            RequestDA.UpdActionComplete(strActionId, DateTime.Now, "0", "0");
        }

        ///Update dependant actions
        DataSet dsDepnd = RequestDA.GetActionDepnd(strActionId);
        if (dsDepnd != null && dsDepnd.Tables.Count > 0)
        {
            DateTime dtNow = DateTime.Now;
            string strBegin = dtNow.ToString();
            foreach (DataRow drDepnd in dsDepnd.Tables[0].Rows)
            {
                string strDepndActionId = drDepnd["sys_action_id"].ToString();
                if (RequestDA.ActionPreFined(strDepndActionId))
                {
                    string strPriority = "";
                    DataSet dsPriority = RequestDA.GetActionById(strDepndActionId);
                    if (dsPriority != null && dsPriority.Tables.Count > 0 && dsPriority.Tables[0].Rows.Count > 0)
                    {
                        strPriority = dsPriority.Tables[0].Rows[0]["sys_actionpriority"].ToString();
                    }


                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "";
                    cmd.CommandType = CommandType.Text;

                    SqlParameter parameter;

                    string strSql = "UPDATE [action] SET sys_actiondate=@dtNow";
                    parameter = new SqlParameter();
                    parameter.ParameterName = "@dtNow";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = dtNow;
                    cmd.Parameters.Add(parameter);

                    DataSet dsPri = null;
                    if (strPriority != "")
                    {
                        dsPri = LibPriorityDA.GetPriorityInfo(strPriority);
                    }


                    double dReqReslvHr = 0;
                    double dReqEsc1Hr = 0;
                    double dReqEsc2Hr = 0;
                    double dReqEsc3Hr = 0;


                    if (dsPri != null && dsPri.Tables.Count > 0 && dsPri.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsPri.Tables[0].Rows[0];
                        if (LayUtil.IsNumeric(dr["sys_resolvehours"].ToString()))
                        {
                            dReqReslvHr = double.Parse(dr["sys_resolvehours"].ToString());
                        }
                        if (LayUtil.IsNumeric(dr["sys_escalate1hours"].ToString()))
                        {
                            dReqEsc1Hr = double.Parse(dr["sys_escalate1hours"].ToString());
                        }
                        if (LayUtil.IsNumeric(dr["sys_escalate2hours"].ToString()))
                        {
                            dReqEsc2Hr = double.Parse(dr["sys_escalate2hours"].ToString());
                        }
                        if (LayUtil.IsNumeric(dr["sys_escalate3hours"].ToString()))
                        {
                            dReqEsc3Hr = double.Parse(dr["sys_escalate3hours"].ToString());
                        }
                    }



                    if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0) && strBegin != "")
                    {
                        strReqId = drMainTask["sys_request_id"].ToString();
                        strProblemId = drMainTask["sys_problem_id"].ToString();
                        strChangeId = drMainTask["sys_change_id"].ToString();

                        if (strReqId != "")
                        {
                            LoadSus(strReqId, "", strBegin);
                        }
                        else if (strProblemId != "")
                        {
                            LoadSus("", strProblemId, strBegin);
                        }
                        else if (strChangeId != "")
                        {
                            LoadSus("", "", strBegin);
                        }
                        else
                        {
                            LoadSus("", "", strBegin);
                        }

                        DateTime dtBegin = DateTime.Parse(strBegin);

                        if (dReqReslvHr != 0)
                        {
                            TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                            string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                            if (strTime != "")
                            {
                                strSql += ", [sys_resolve]=@sys_resolve";

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@sys_resolve";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = DateTime.Parse(strTime);
                                cmd.Parameters.Add(parameter);
                            }
                            else
                            {
                                strSql += ", [sys_resolve]=NULL";
                            }
                        }
                        else
                        {
                            strSql += ", [sys_resolve]=NULL";
                        }

                        if (dReqEsc1Hr != 0)
                        {
                            TimeSpan ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                            string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                            if (strTime != "")
                            {
                                strSql += ", [sys_escalate1]=@sys_escalate1";

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@sys_escalate1";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = DateTime.Parse(strTime);
                                cmd.Parameters.Add(parameter);
                            }
                            else
                            {
                                strSql += ", [sys_escalate1]=NULL";
                            }
                        }
                        else
                        {
                            strSql += ", [sys_escalate1]=NULL";
                        }

                        if (dReqEsc2Hr != 0)
                        {
                            TimeSpan ts = new TimeSpan(0, (int)(dReqEsc2Hr * 60), 0);
                            string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                            if (strTime != "")
                            {
                                strSql += ", [sys_escalate2]=@sys_escalate2";

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@sys_escalate2";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = DateTime.Parse(strTime);
                                cmd.Parameters.Add(parameter);
                            }
                            else
                            {
                                strSql += ", [sys_escalate2]=NULL";
                            }
                        }
                        else
                        {
                            strSql += ", [sys_escalate2]=NULL";
                        }


                        if (dReqEsc3Hr != 0)
                        {
                            TimeSpan ts = new TimeSpan(0, (int)(dReqEsc3Hr * 60), 0);
                            string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                            if (strTime != "")
                            {
                                strSql += ", [sys_escalate3]=@sys_escalate3";

                                parameter = new SqlParameter();
                                parameter.ParameterName = "@sys_escalate3";
                                parameter.Direction = ParameterDirection.Input;
                                parameter.Value = DateTime.Parse(strTime);
                                cmd.Parameters.Add(parameter);
                            }
                            else
                            {
                                strSql += ", [sys_escalate3]=NULL";
                            }
                        }
                        else
                        {
                            strSql += ", [sys_escalate3]=NULL";
                        }

                    }
                    else
                    {
                        strSql += ", [sys_resolve]=NULL";
                        strSql += ", [sys_escalate1]=NULL";
                        strSql += ", [sys_escalate2]=NULL";
                        strSql += ", [sys_escalate3]=NULL";
                    }


                    strSql += " WHERE [sys_action_id]=@sys_action_id";
                    parameter = new SqlParameter();
                    parameter.ParameterName = "@sys_action_id";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strDepndActionId;
                    cmd.Parameters.Add(parameter);

                    cmd.CommandText = strSql;

                    LayUtilDA.RunSqlCmd(cmd);


                    //Send out email
                    DataSet dsAction = RequestDA.GetReqActionInfo(strDepndActionId);
                    if (dsAction != null && dsAction.Tables.Count > 0 && dsAction.Tables[0].Rows.Count > 0)
                    {
                        DataRow drAction = dsAction.Tables[0].Rows[0];
                        string strUser = drAction["sys_username"].ToString();
                        DataSet dsUser = UserDA.GetUserInfo(strUser);
                        if (dsUser != null && dsUser.Tables.Count > 0 && dsUser.Tables[0].Rows.Count > 0)
                        {
                            string strEmail = dsUser.Tables[0].Rows[0]["sys_email"].ToString();
                            if (strEmail != "")
                            {
                                if (LayUtil.IsNumeric(drAction["sys_request_id"].ToString()) && Application["appemailuserassignaction"].ToString() == "true")
                                {
                                    LayUtil.SendOutEmail(Application, "action_assign_notify", strEmail, strUser, "", "request", drAction["sys_request_id"].ToString(), drAction["sys_requestclass_id"].ToString(), RequestDA.GetReqActionMailCmd(strDepndActionId));
                                }
                                else if (LayUtil.IsNumeric(drAction["sys_problem_id"].ToString()) && Application["appemailproblemuserassignaction"].ToString() == "true")
                                {
                                    LayUtil.SendOutEmail(Application, "actionproblem_assign_notify", strEmail, strUser, "", "problem", drAction["sys_problem_id"].ToString(), "", ProblemDA.GetProblemActionMailCmd(strDepndActionId));
                                }
                                else if (LayUtil.IsNumeric(drAction["sys_change_id"].ToString()) && Application["appemailchangeuserassignaction"].ToString() == "true")
                                {
                                    LayUtil.SendOutEmail(Application, "actionchange_assign_notify", strEmail, strUser, "", "change", drAction["sys_change_id"].ToString(), "", ChangeDA.GetChgActionMailCmd(strDepndActionId));
                                }
                                else if(Application["appemailuserassignaction"].ToString() == "true")
                                {
                                    LayUtil.SendOutEmail(Application, "action_assign_notify", strEmail, strUser, "", "request", "", "", RequestDA.GetReqActionMailCmd(strDepndActionId));
                                }

                            }
                        }
                    }

                }
            }
        }


        //
        string strMsg = "";
        strReqId = drMainTask["sys_request_id"].ToString();
        if (LayUtil.IsNumeric(strReqId))
        {
            ViewState["sys_request_id"] = strReqId;

            DataSet dsReqClass = RequestDA.GetReqClassByReqId(strReqId);

            if (dsReqClass != null && dsReqClass.Tables.Count > 0 && dsReqClass.Tables[0].Rows.Count > 0)
            {
                DataRow drReqClass = dsReqClass.Tables[0].Rows[0];
                if (drReqClass["sys_requestclass_closeontaskscomplete"].ToString() == "1" && drReqClass["IncompleteCount"].ToString() == "0")
                {
                    string strReqSpwnClose = SysDA.GetSettingValue("requestspawnclose", Application);

                    DataSet dsActionReq = RequestDA.GetReqFullInfoById(strReqId);
                    string strReqClass = "";
                    string strReqDate = "";
                    string strReqPriority = "";
                    string strReqSpwnCnt = "";
                    string strReqLnkCnt = "";
                    if (dsActionReq != null && dsActionReq.Tables.Count > 0 && dsActionReq.Tables[0].Rows.Count > 0)
                    {
                        strReqClass = dsActionReq.Tables[0].Rows[0]["sys_requestclass_id"].ToString();
                        strReqDate = dsActionReq.Tables[0].Rows[0]["sys_requestdate"].ToString();
                        strReqPriority = dsActionReq.Tables[0].Rows[0]["sys_requestpriority"].ToString();
                        strReqSpwnCnt = dsActionReq.Tables[0].Rows[0]["spawncount"].ToString();
                        strReqLnkCnt = dsActionReq.Tables[0].Rows[0]["linkcount"].ToString();


                        //Validate Fields That Should Be Specified for closed request are closed
                        if (strReqClass != "")
                        {
                            string strReqClassXml = FormDesignerDA.LoadClassFormXML("sys_requestclass_xmluserform", strReqClass);

                            XmlDocument xmlReqClass = new XmlDocument();
                            xmlReqClass.LoadXml(strReqClass);

                            foreach (XmlElement child in xmlReqClass.DocumentElement.ChildNodes)
                            {
                                if (!(child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") && !(child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
                                {
                                    if (child.GetAttribute("closereq") == "true")
                                    {
                                        if (dsActionReq.Tables[0].Rows[0][child.InnerText].ToString() == "")
                                        {
                                            strMsg = "You must specify the '" + child.GetAttribute("caption") + "' field before closing the related " + Application["apprequestterm"] + ".";
                                            ShowMsg(strMsg);
                                            return;
                                        }
                                    }
                                }
                            }

                        }


                        if (SysDA.GetSettingValue("appclosebyowner", Application) == "true")
                        {
                            if (dsActionReq.Tables[0].Rows[0]["sys_ownedby"].ToString() != Session["User"].ToString())
                            {
                                strMsg = "Sorry, the related " + Application["apprequestterm"] + " can only be closed by the owner.";
                                ShowMsg(strMsg);
                                return;
                            }
                        }
                    }


                    if (strReqSpwnClose == "")
                    {
                        if (strReqSpwnCnt != "0")
                        {
                            strMsg = "You cannot close the related " + Application["apprequestterm"] + " as it contains spawned " + Application["apprequestterm"] + "s that are not yet closed.";
                            ShowMsg(strMsg);
                            return;
                        }
                    }

                    if (strMsg == "")
                    {
                        CloseRequest(strReqId);
                        return;
                    }
                }

            }
        }

    }
    /// <summary>
    /// close request when all tasks finish
    /// </summary>
    /// <param name="strReqId"></param>
    private void CloseRequest(string strReqId)
    {
        DataSet dsReqEUser = RequestDA.GetReqWithEUser(strReqId);
        if (dsReqEUser != null && dsReqEUser.Tables.Count > 0 && dsReqEUser.Tables[0].Rows.Count > 0)
        {
            DataRow dr = dsReqEUser.Tables[0].Rows[0];
            if (dr["sys_requeststatus"].ToString() != "0")
            {
                string strCloseOnSchedule = "0";
                if (dr["sys_resolve"].ToString() != "")
                {
                    DateTime dtRes = DateTime.Parse(dr["sys_resolve"].ToString());
                    if (DateTime.Compare(dtRes, DateTime.Now) >= 0)
                    {
                        strCloseOnSchedule = "1";
                    }
                    else
                    {
                        strCloseOnSchedule = "-1";
                    }
                }

                LoadSus(strReqId, "", dr["sys_requestdate"].ToString());

                string strDurHD = ((int)(RequestDA.GetReqDuration(dr["sys_requestdate"].ToString(), DateTime.Now, listSus))).ToString();
                string strDur24 = ((int)(RequestDA.GetReqDuration24Min(dr["sys_requestdate"].ToString(), DateTime.Now))).ToString();

                RequestDA.UpdReqComplete(strReqId, "0", DateTime.Now, strDurHD, strDur24, strCloseOnSchedule);


                ///Update Audit
                //Get Closed status name
                DataSet dsStatus = LibReqStatusDA.GetReqStatusList("Closed");
                string strNewStatusName = "";
                if (dsStatus != null && dsStatus.Tables.Count > 0 && dsStatus.Tables[0].Rows.Count > 0)
                {
                    strNewStatusName = dsStatus.Tables[0].Rows[0]["sys_status"].ToString();
                    if (strNewStatusName == "")
                    {
                        strNewStatusName = "Closed";
                    }

                    strNewStatusName = "0 - " + strNewStatusName;
                }

                string strOldStatusName = GetValue("sys_requeststatus");
                if (GetValue("StatusName") != "")
                {
                    strOldStatusName += " - " + GetValue("StatusName");
                }

                RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Status Changed", "sys_requeststatus", strOldStatusName, strNewStatusName);
            }
        }

    }
}
