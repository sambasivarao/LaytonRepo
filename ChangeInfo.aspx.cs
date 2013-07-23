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
/// Change detail page
/// </summary>

public partial class ChangeInfo : System.Web.UI.Page
{
    public XmlDocument xmlForm;

    public DataSet dsOldValue;
    public DataSet dsTemplateValue;

    public Hashtable htCols;
    public Hashtable htColSize;

    public Hashtable htCtrlNode = null;

    public Hashtable htStatus;

    //private Hashtable htOpenStatus;

    private Hashtable htMailVal;
    private Hashtable htReqVal;
    private Hashtable htProblemVal;

    public Hashtable htChildName;

    public string strBackImg = "";

    public string strNewPK = "";

    public bool bAllowChange = true;
    public DataSet dsUser;

    private List<SusPeriod> listSus = null;
    private XmlDocument xmlHours = null;
    private Hashtable htOpenHr = null;
    private Hashtable htCloseHr = null;

    public string strClientIDs;

    public int nReqTypeCtrlCnt = 0;
    public Hashtable htCtrl = null;

    public string[] strReqTypeArray = null;

    public XmlElement nodeComment = null;

    public bool bShowTab = false;
    public Hashtable htTabNode = null;
    public Hashtable htTabOrders = null;
    public Hashtable htTabNodeWithInvIndex = null;

    public Hashtable htTabs = null;
    public Hashtable htTabText = null;
    public Hashtable htTabName = null;
    //public string strTabSize = "";

    public Hashtable htBRVal = null;
    public bool bRule = false;

    public Label lbCost;

    public bool bDelComment = false;
    public bool bClosed = true;

    public string strTabHeight = "200";
    public RadEditor edtChange = null;
    public RadEditor edtImpact = null;
    public RadEditor edtImplementation = null;
    public RadEditor edtRollback = null;

    HtmlTableCell hdgCommCell = null;

    public System.Text.StringBuilder sbTxt = null;
    public int nLen = 0;

    Hashtable htChgType = null;

    void Page_Init()
    {
        //        if (!IsPostBack)
        //        {
        //            SaveQueryStr();
        //        }
        //        MyInit();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;

        LayUtil.CheckLoginUser(Session, Response);
        if (!IsPostBack)
        {
            if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
            {
                Response.End();
                return;
            }

            SaveQueryStr();
            Session["ButtonBack"] = "ChangeInfo.aspx?sys_change_id=" + ViewState["sys_change_id"];

            dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogComComfirm.WindowState = DialogWindowState.Hidden;
            dialogDelConfirm.WindowState = DialogWindowState.Hidden;
            dialogApplyApproval.WindowState = DialogWindowState.Hidden;
            dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

            LayUtil.SetFont(this.dialogMsg, Application);
            LayUtil.SetFont(this.dialogComComfirm, Application);
            LayUtil.SetFont(this.dialogDelConfirm, Application);
            LayUtil.SetFont(this.dialogApplyApproval, Application);
            LayUtil.SetFont(this.dialogTaskTemplate, Application);
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

        MyInit();
    }

    private void SaveQueryStr()
    {
        ViewState["sys_change_id"] = LayUtil.GetQueryString(Request, "sys_change_id");
        ViewState["sys_mail_id"] = LayUtil.GetQueryString(Request, "sys_mail_id");

        ViewState["field"] = LayUtil.GetQueryString(Request, "field");
        ViewState["from"] = LayUtil.GetQueryString(Request, "from");
        ViewState["sys_problem_id"] = LayUtil.GetQueryString(Request, "sys_problem_id");
        ViewState["sys_request_id"] = LayUtil.GetQueryString(Request, "sys_request_id");

        ViewState["newfromreq"] = LayUtil.GetQueryString(Request, "newfromreq");
        ViewState["newfromproblem"] = LayUtil.GetQueryString(Request, "newfromproblem");

        ViewState["sys_change_requesttype"] = "";
    }
    /// <summary>
    /// Initialize controls and load values
    /// </summary>
    private void MyInit()
    {

        //Load Form
        string strChangeId = "";

        if (ViewState["sys_change_id"] != null)
        {
            strChangeId = ViewState["sys_change_id"].ToString();
        }
        
        /*
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
        */

        if (strChangeId != "")
        {
            DataSet dsChange = ChangeDA.GetChangeFullInfoById(strChangeId);

            if (dsChange == null || dsChange.Tables.Count <= 0 || dsChange.Tables[0].Rows.Count <= 0)
            {
                ShowMsg("Couldn't find this Change");
                return;
            }

            string strStatus = dsChange.Tables[0].Rows[0]["sys_change_status"].ToString();

            /*
            if (htOpenStatus[strStatus] == null && Application["applockuserclosechange"].ToString() == "true")
            {
                bAllowChange = false;
                textChgAllowed.Text = "false";
            }
             */ 
        }



        string strXml = "";
        if (FormDesignerDA.ExistUserChangeForm(Session["User"].ToString()))
        {
            DataSet dsFormXml = FormDesignerDA.GetUserChangeForm(Session["User"].ToString());
            strXml = dsFormXml.Tables[0].Rows[0]["formxml"].ToString();
        }
        else
        {
            strXml = FormDesignerDA.LoadFormXMLFromDB("change");
        }
        xmlForm = new XmlDocument();

        if (strXml != "")
        {
            xmlForm.LoadXml(strXml);
        }

        //Set Status Hashtable
        DataSet dsStatus = LibChgStatusDA.GetChgStatusList("");
        htStatus = new Hashtable();
        if (dsStatus != null && dsStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsStatus.Tables[0].Rows)
            {
                htStatus.Add(dr["sys_change_status_id"].ToString(), dr["sys_change_status"].ToString());
            }
        }

        //Get Current logon user info
        dsUser = UserDA.GetUserInfo(Session["User"].ToString());

        ///Load request and related value
        if (ViewState["sys_change_id"] != null && ViewState["sys_change_id"].ToString() != "")
        {
            LoadVal();

            //LoadSus(GetValue("sys_change_id"), GetValue("sys_siteid"), GetValue("sys_changedate"));
        }

        DataSet dsItem = LibChgTypeDA.GetList("");

        htChgType = new Hashtable();
        htChgType[""] = "(None)";

        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                htChgType[dr["sys_changetype_id"].ToString()] = dr["sys_changetype"].ToString();
            }
        }

        CheckTabSize();

        LoadCtrlInfo();

        LoadCtrl();

        //ds = LayUtilDA.GetTableValue("priority", "sys_priority_id", LayUtil.GetQueryString(Request, "sys_priority_id"));

        LayUtil.SetFont(this.dialogMsg, Application);
        LayUtil.SetFont(this.dialogComComfirm, Application);
        LayUtil.SetFont(this.dialogDelConfirm, Application);
        LayUtil.SetFont(this.dialogApplyApproval, Application);
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
            /*
            if (node.InnerText == "sys_requesttype_id")
            {
                nReqTypeCtrlCnt++;
            }
            */

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
        string strTbl = "change";

        //Initialize columns hashtable
        htCols = new Hashtable();
        htColSize = new Hashtable();

        DataSet dsCols = DataDesignDA.GetTblCol(strTbl);

        if (dsCols != null && dsCols.Tables.Count > 0)
        {
            foreach (DataRow dr in dsCols.Tables[0].Rows)
            {
                htCols.Add(dr["ColName"].ToString(), dr["ColType"].ToString());
                htColSize.Add(dr["ColName"].ToString(), dr["ColSize"].ToString());
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

        string strTbl = "change";
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
                        else if (strLink == "sys_requestedby")
                        {
                            strAsField = "requestedby_forename";
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
                        else if (strLink == "sys_requestedby")
                        {
                            strAsField = "requestedby_surname";
                        }
                    }
                }

                strSql += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[" + strTbl + "]." + strLink + ") AS " + strAsField;
            }
        }

        strSql += " FROM [" + strTbl + "]";

        if (ViewState["sys_change_id"] == null || ViewState["sys_change_id"].ToString() == "")
        {
            strSql += " WHERE sys_change_id = Null";
        }
        else
        {
            strSql += " WHERE sys_change_id='" + ViewState["sys_change_id"].ToString().Replace("'", "''") + "'";
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
        sbTxt = new System.Text.StringBuilder();
        sbTxt.Append("var arrCtrlID = new Array();");
        sbTxt.Append("var arrClientID = new Array();");
        sbTxt.Append("var arrCtrlType = new Array();");
        sbTxt.Append("var arrCtrlSize = new Array();");

        nLen = 0;

        System.Text.StringBuilder sbDDCss = new System.Text.StringBuilder("<style type=\"text/css\">");

        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = GetValue(child.InnerText);
            if (!IsPostBack)
            {
                string strField = child.InnerText;
                if (strField == "sys_siteid")
                {
                    sys_siteidH.Text = strVal;
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
                else if (strField == "sys_requestedby")
                {
                    sys_requestedbyH.Text = strVal;
                }
                else if (strField == "sys_change_requesttype")
                {
                    sys_change_requesttypeH.Text = strVal;
                }
                else if (strField == "sys_assignedtoanalgroup")
                {
                    sys_assignedtoanalgroupH.Text = strVal;
                }
                else if (strField == "sys_assignedtocabgroup")
                {
                    sys_assignedtocabgroupH.Text = strVal;
                }
            }
            if (child.Name == "tab")
            {
                /*
                if (LayUtil.GetAttribute(xmlForm.DocumentElement, "tab") != "true")
                {
                    continue;
                }
                */

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
                }
                else
                {
                    tab.Width = new Unit("200px");
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
                if (child.GetAttribute("tabindex") == "1")
                {
                    tab.Focus();
                }

                tab.ID = "tabMain";

                //tab.BackColor = System.Drawing.Color.Gray;
                tab.EnableOverlappingTabs = true;
                tab.TabMoving.Enabled = false;
                //tab.DisplayMode = TabDisplayMode.MultiRow;

                //tab.ClientEvents.TabMovedEnd = "TabMovedEnd";
                tab.AutoPostBackFlags.TabMoved = Infragistics.Web.UI.AutoPostBackFlag.On;
                tab.TabItemsMoved += new TabItemsMovedEventHandler(tab_TabItemsMoved);
                //tab.PostBackOptions.EnableLoadOnDemand = true;

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

                if (child.InnerText == "sys_change_status")
                {
                    if (ViewState["sys_change_id"] != null && ViewState["sys_change_id"].ToString() != "")
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
                        else if (strLink == "sys_requestedby")
                        {
                            strAsField = "requestedby_forename";
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
                        else if (strLink == "sys_requestedby")
                        {
                            strAsField = "requestedby_surname";
                        }
                    }
                    ctrl.Text = GetValue(strAsField);
                }
                else
                {
                    ctrl.Text = strVal;
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

                    ctrl.DropDownContainerMaxHeight = new Unit(LayUtil.DropDownMaxHeight);
                    ctrl.DropDownContainerHeight = new Unit("0px");

                    //ctrl.Font.Name = child.GetAttribute("font-family");
                    //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                    ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                    sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                    ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

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
                    string strFieldType = DataDesignDA.GetFieldType("change", child.InnerText);
                    if (strFieldType == "")
                        continue;
                    */

                    if (htCols[child.InnerText] == null)
                        continue;

                    string strFieldType = htCols[child.InnerText].ToString();

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

                        ctrl.OnClientCommandExecuting = "OnClientCommandExecuting";

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
                    //if (bAllowChange)
                    {
                        ctrl.Click += new ImageClickEventHandler(sys_button1_Click);
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                    }
                    //else
                    //{
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    // }

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button2_Click);
                    }
                    //else
                    //{
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    //}

                    if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                    {
                        ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    if (ViewState["sys_change_id"].ToString() != "")
                    {
                        nCnt = ChangeDA.GetChangeTaskCnt(ViewState["sys_change_id"].ToString());
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
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button3_Click);
                    }
                    //else
                    {
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

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
                    //ctrl.Target = child.GetAttribute("target");
                    ctrl.NavigateUrl = GetListURL(true);
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
                    //
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button5_Click);
                    }
                    //else
                    {
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText= LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    int nCnt = ChangeDA.GetChangeAttchCnt(ViewState["sys_change_id"].ToString());

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
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button6_Click);
                    }
                    //else
                    {
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

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
                    nodeComment = child;

                    //Comments
                    if (xmlForm.DocumentElement.GetAttribute("commentsinline") == "true")
                    {
                        if (ViewState["sys_change_id"].ToString() != "")
                        {
                            //list comments
                        }
                    }
                    else
                    {
                        //not in line
                        string strImg = child.GetAttribute("image");
                        int nCnt = 0;
                        int nReadCnt = 0;

                        if (ViewState["sys_change_id"].ToString() != "")
                        {
                            string strAlt;

                            nCnt = ChangeDA.GetCommentCntByChangeId(ViewState["sys_change_id"].ToString());
                            nReadCnt = ChangeDA.GetCommentReadCntByUserChangeId(ViewState["sys_change_id"].ToString(), Session["User"].ToString());

                            if (nCnt > 0)
                            {
                                if (nCnt > nReadCnt)
                                {
                                    if (child.GetAttribute("buttonhasunreaditemsimg") != "")
                                    {
                                        strImg = child.GetAttribute("buttonhasunreaditemsimg");
                                    }
                                    //linkImg = LayUtil.GetAttribute(node, "linkhasunreaditemsimg");

                                    //strAlt = LayUtil.RplTm(ParentPage.Application, node.GetAttribute("linkimgalt")) + " (" + strCnt + " Items, " + (nCnt - nReadCnt).ToString() + " Un-Read)";
                                }
                                else
                                {
                                    if (child.GetAttribute("hasitemsimage") != "")
                                    {
                                        strImg = child.GetAttribute("hasitemsimage");
                                    }

                                    //linkImg = LayUtil.GetAttribute(node, "linkhasitemsimg");

                                }
                            }
                        }

                        System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                        ctrl.ID = "imgbtn" + child.Name;

                        ctrl.ImageUrl = strImg;
                        //if (bAllowChange)
                        {
                            ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                            ctrl.Click += new ImageClickEventHandler(sys_button7_Click);
                        }
                        //else
                        {
                        //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                        }

                        ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";
                        ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt")) + " (" + nCnt.ToString() + " items)";

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

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button12_Click);
                    }
                    //else
                    {
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button13_Click);
                    }
                    //else
                    //{
                    //    ctrl.OnClientClick = "CheckAllowed(this);document.location='" + "ChangeCost.aspx?sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
                    //}

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    //Link
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button14_Click);
                    }
                    //else
                    {
                    //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    //Cab Group
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();
                    ctrl.ID = "imgbtn" + child.Name;

                    ctrl.ImageUrl = child.GetAttribute("image");
                    //if (bAllowChange)
                    {
                        ctrl.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        ctrl.Click += new ImageClickEventHandler(sys_button15_Click);
                    }
                    //else
                    {
                        //    ctrl.OnClientClick = "CheckAllowed(this);return false;";
                    }

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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

                    ctrl.ToolTip = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));
                    ctrl.AlternateText = LayUtil.RplTm(Application, child.GetAttribute("imagealt"));

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
                    if (child.InnerText == "sys_change_description" || child.InnerText == "sys_change_impact" || child.InnerText == "sys_change_implementation" || child.InnerText == "sys_change_rollback")
                    {
                        string strShowOnTab = LayUtil.GetAttribute(child, "ontab");
                        if (strShowOnTab == "true")
                        {
                            continue;
                        }
                    }

                    if (child.InnerText == "sys_change_id")
                    {
                        //Create Textbox
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
                    }
                    else if (child.InnerText == "sys_completedate")
                    {
                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DatePickerDefWidth;
                        }

                        WebDatePicker datePicker = new WebDatePicker();

                        datePicker.Width = new Unit(strWidth + "px");

                        datePicker.ID = "datepicker" + child.Name;
                        datePicker.Nullable = true;

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
                            datePicker.Value = DateTime.Parse(strVal);
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
                    else if (child.InnerText == "sys_changedate")
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
                    else if (child.InnerText == "sys_startdate" || child.InnerText == "sys_finishdate")
                    {
                        string strWidth = child.GetAttribute("width");
                        if (!LayUtil.IsNumeric(strWidth))
                        {
                            strWidth = LayUtil.DatePickerDefWidth;
                        }

                        WebDatePicker datePicker = new WebDatePicker();

                        datePicker.Width = new Unit(strWidth + "px");

                        datePicker.ID = "datepicker" + child.Name;
                        datePicker.Nullable = true;

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
                        if (strVal != "")
                        {
                            datePicker.Value = DateTime.Parse(strVal);
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
                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }

                        ctrl.Attributes["onblur"] = "javascript:verify_enduser('" + ctrl.ClientID + "','','');return false;";

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
                            hl.NavigateUrl = "javascript:selectenduser('" + ctrl.ClientID + "','','');";

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
                    else if (child.InnerText == "sys_assignedto")
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
                    else if (child.InnerText == "sys_requestedby")
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

                        //string strOwnReq = GetUserVal("sys_requestowner");
                        //if (!UserReadOnly(child) && strOwnReq == "0")
                        //{
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


                        //}
                    }
                    else if (child.InnerText == "sys_assignedtoanalgroup")
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
                    else if (child.InnerText == "sys_siteid")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_siteidH.Text;
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
                    else if (child.InnerText == "sys_assignedtocabgroup")
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
                            hl.NavigateUrl = "javascript:selectcabgroup('" + ctrl.ClientID + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select " + Application["appuserterm"] + " Group";

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

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

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
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
                    else if (child.InnerText == "sys_change_requesttype")
                    {
                        //Create Textbox
                        TextBox ctrl = new TextBox();
                        if (!IsPostBack)
                        {
                            ctrl.Text = strVal;
                            ViewState["sys_change_requesttype"] = strVal;
                        }
                        else
                        {
                            ctrl.Text = sys_change_requesttypeH.Text;
                        }

                        //ctrl.Text = strVal;
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
                            hl.NavigateUrl = "javascript:selectchgreqtype('" + ctrl.ClientID + "','" + strVal + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select Change Request Type";

                            hl.ID = "hl" + child.Name;

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);
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

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
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
                    else if (child.InnerText == "sys_assetsaffected")
                    {
                        RadListBox ctrl = new RadListBox();
                        ctrl.ID = "lbsys_assetsaffected";

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

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.Height = new Unit(strHeight + "px");
                        }

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                        ctrl.EnableViewState = true;

                        //GetListItem
                        //if (!IsPostBack)
                        {
                            if (LayUtil.IsNumeric(ViewState["sys_change_id"].ToString()))
                            {
                                DataSet dsItem = ChangeDA.GetAssets(ViewState["sys_change_id"].ToString());
                                if (dsItem != null && dsItem.Tables.Count > 0 && dsItem.Tables[0].Rows.Count > 0)
                                {
                                    RadListBoxItem item;
                                    foreach (DataRow dr in dsItem.Tables[0].Rows)
                                    {
                                        item = new RadListBoxItem(dr["sys_asset_id"].ToString(), dr["sys_assettype"].ToString());
                                        ctrl.Items.Add(item);
                                    }
                                }
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
                        if (!UserReadOnly(child))
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selassetaffected('" + ctrl.ClientID + "','problem" + ViewState["sys_problem_id"] + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select Assets affected";

                            hl.ID = "hl" + child.Name + "selassets";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);
                        }
                        //Create End User Quick Info image
                        HyperLink hlq = new HyperLink();
                        hlq.NavigateUrl = "javascript:quickinfoasset('" + ctrl.ClientID + "')";

                        hlq.ImageUrl = "Application_Images/16x16/Info_16px.png";
                        hlq.ToolTip = "Quick Info on Asset";

                        hlq.ID = "hlq" + child.Name;

                        hlq.Style["Top"] = child.GetAttribute("top") + "px";
                        hlq.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
                        hlq.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hlq);

                    }
                    else if (child.InnerText == "sys_itservice")
                    {
                        RadListBox ctrl = new RadListBox();
                        ctrl.ID = "lbsys_itservice";

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

                        string strHeight = child.GetAttribute("height");
                        if (LayUtil.IsNumeric(strHeight))
                        {
                            ctrl.Height = new Unit(strHeight + "px");
                        }

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));
                        ctrl.EnableViewState = true;

                        //GetListItem
                        //if (!IsPostBack)
                        {
                            if (LayUtil.IsNumeric(ViewState["sys_change_id"].ToString()))
                            {
                                DataSet dsSvsItem = ChangeDA.GetItServices(ViewState["sys_change_id"].ToString());
                                if (dsSvsItem != null && dsSvsItem.Tables.Count > 0 && dsSvsItem.Tables[0].Rows.Count > 0)
                                {
                                    RadListBoxItem item;
                                    foreach (DataRow dr in dsSvsItem.Tables[0].Rows)
                                    {
                                        item = new RadListBoxItem(dr["sys_itservice"].ToString(), dr["sys_itservice_id"].ToString());
                                        ctrl.Items.Add(item);
                                    }
                                }
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
                        if (!UserReadOnly(child))
                        {
                            //Create Select image
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = "javascript:selitservice('" + ctrl.ClientID + "','change" + ViewState["sys_change_id"] + "');";

                            hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                            hl.ToolTip = "Select Services affected";

                            hl.ID = "hl" + child.Name + "selitservice";

                            hl.Style["Top"] = child.GetAttribute("top") + "px";
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                            hl.Style["Position"] = "absolute";

                            ctrlRoot.Controls.Add(hl);

                        }
                    }
                    else if (child.InnerText == "sys_change_type")
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
                        ctrl.Style["Z-Index"] = "20000";

                        ctrl.DropDownContainerHeight = new Unit(0);

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDChangeType(ctrl, strVal);

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
                    else if (child.InnerText == "sys_approval_status")
                    {
                        WebDropDown ctrl = new WebDropDown();
                        ctrl.ID = "dd" + child.Name;

                        if (GetUserVal("sys_setchgapprovalstatus") != "1")
                        {
                            ctrl.Enabled = false;
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

                        //ctrl.Font.Name = child.GetAttribute("font-family");
                        //ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                        ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                        sbDDCss.Append(LayUtil.GetDropDownFontCss(ctrl, child.GetAttribute("font-family"), child.GetAttribute("font-size") + "px"));
                        ctrl.DisplayMode = DropDownDisplayMode.DropDownList;

                        //Set list items
                        SetDDApprovalStatus(ctrl, strVal);

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
                    }
                    else
                    {
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
                            ctrl.OnClientCommandExecuting = "OnClientCommandExecuting";


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
                                ctrl.Enabled = false;
                            }

                            if (child.GetAttribute("tabindex") == "1")
                            {
                                ctrl.Focus();
                            }
                            /*
                            if (child.InnerText == "sys_change_description")
                            {
                                RadEditor edtHelp = new RadEditor();
                                edtHelp.Content = strVal;
                                ctrl.Text = edtHelp.Text;
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

        if (strField == "sys_assignedto")
        {
            return sys_assignedtoH.Text;
        }
        else if (strField == "sys_requestedby")
        {
            return sys_requestedbyH.Text;
        }
        else if (strField == "sys_change_requesttype")
        {
            return sys_change_requesttypeH.Text;
        }
        else if (strField == "sys_assignedtoanalgroup")
        {
            return sys_assignedtoanalgroupH.Text;
        }
        else if (strField == "sys_siteid")
        {
            return sys_siteidH.Text;
        }
        else if (strField == "sys_assignedtocabgroup")
        {
            return sys_assignedtocabgroupH.Text;
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
            WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + htChildName[strField].ToString());
            if (ctrl != null && ctrl.SelectedItem != null)
            {
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
            if (htBRVal != null && htBRVal[strField] != null)
            {
                if (!IgnoreBRVal(child))
                {
                    return htBRVal[strField].ToString();
                }
            }
        }

        if (strField == "sys_assignedto")
        {
            return sys_assignedtoH.Text;
        }
        else if (strField == "sys_requestedby")
        {
            return sys_requestedbyH.Text;
        }
        else if (strField == "sys_assignedtoanalgroup")
        {
            return sys_assignedtoanalgroupH.Text;
        }
        else if (strField == "sys_assignedtocabgroup")
        {
            return sys_assignedtocabgroupH.Text;
        }
        else if (strField == "sys_change_requesttype")
        {
            return sys_change_requesttypeH.Text;
        }
        else if (strField == "sys_siteid")
        {
            return sys_siteidH.Text;
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
                if (ctrl != null && ctrl.SelectedItem != null)
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
                //else if (DataDesignDA.GetFieldType("change", child.InnerText) == "DateTime")
                else if (htCols[child.InnerText].ToString() == "DateTime")
                {
                    WebDatePicker datePicker = (WebDatePicker)ctrlRoot.FindControl("datepicker" + child.Name);

                    if (datePicker != null)
                    {
                        strVal = datePicker.Text;
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
                if (child.InnerText == "sys_change_description" || child.InnerText == "sys_change_impact" || child.InnerText == "sys_change_implementation" || child.InnerText == "sys_change_rollback")
                {
                    if (child.GetAttribute("ontab") == "true")
                    {
                        //if (LayUtil.GetAttribute(xmlForm.DocumentElement, "tab") == "true")
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
                else if (child.InnerText == "sys_approval_status")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null && ctrl.SelectedItem != null)
                    {
                        strVal = ctrl.SelectedItem.Value;
                    }
                }
                else if (child.InnerText == "sys_change_type" || child.InnerText == "sys_impact" || child.InnerText == "sys_urgency")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null && ctrl.SelectedItem != null)
                    {
                        strVal = ctrl.SelectedItem.Value;
                    }
                }
                else if (child.InnerText == "sys_changedate" || child.InnerText == "sys_startdate" || child.InnerText == "sys_finishdate" || child.InnerText == "sys_completedate")
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
        //bool bError = false;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = GetUserInput(child);
            //Check PK
            if (child.InnerText == "sys_change_id")
            {
                strNewPK = strVal;

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
                if (child.InnerText == "sys_change_timespent")
                {
                    if (strVal != "" && !CheckDuration(strVal))
                    {
                        ShowMsg("You must enter a valid duration (format specified in settings) in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }
                }
                else if (strVal != "" && child.InnerText != "sys_change_id")
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
            if (child.GetAttribute("userreq") == "true" && strVal == "")
            {
                ShowMsg("You must enter a value in the " + child.GetAttribute("caption") + " box.");
                return false;
            }

        }
        return true;
    }

    /// <summary>
    /// Generate Insert SqlCommand
    /// </summary>
    /// <returns>SqlCommand generated</returns>
    private SqlCommand GetInsCmd()
    {

        /*
        string strLT = "NULL";
        if (strReqLinkType != "")
        {
            strLT = strReqLinkType;
        }
        */
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        string strSql;

        //Insert
        bool bFirst = true;

        strSql = "INSERT INTO change(sys_change_status,";
        string strSqlVal = " VALUES(" + Application["appdefaultstatuschg"].ToString() + ","; ;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_change_id" || child.InnerText == "sys_itservice" || child.InnerText == "sys_assetsaffected")
                continue;

            if (htCols[child.InnerText] == null)
                continue;
            
            string strVal = GetUserInput(child);
            if (child.InnerText == "sys_change_timespent" && strVal != "")
            {
                strVal = RequestDA.Time2Mins(strVal);
            }

            if ((child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") || (child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                if (!bFirst)
                {
                    strSql += ",";
                    strSqlVal += ",";

                }
                bFirst = false;

                strSql += "[" + child.InnerText + "]";

                if (strVal == "")
                {
                    strSqlVal += "NULL";
                }
                else
                {
                    strSqlVal += "@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (child.InnerText == "sys_changedate" || child.InnerText == "sys_startdate" || child.InnerText == "sys_finishdate" || child.InnerText == "sys_completedate" || htCols[child.InnerText].ToString() == "DateTime")
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
        if (htCtrl["sys_urgency"] == null)
        {
            string strVal = SysDA.GetSettingValue("appdefaulturgencychg", Application);

            strSql += ",[sys_urgency]";

            if (strVal == "")
            {
                strSqlVal += ",NULL";
            }
            else
            {
                strSqlVal += ",@sys_urgency";

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_urgency";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);
            }
        }
        if (htCtrl["sys_impact"] == null)
        {
            string strVal = SysDA.GetSettingValue("appdefaultimpactchg", Application);

            strSql += ",[sys_impact]";

            if (strVal == "")
            {
                strSqlVal += ",NULL";
            }
            else
            {
                strSqlVal += ",@sys_impact";

                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_impact";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);
            }
        }

        strSql += ")" + strSqlVal + ")";

        cmd.CommandText = strSql;

        return cmd;
    }

    /// <summary>
    /// Generate Update SqlCommand
    /// </summary>
    /// <returns>SqlCommand generated</returns>
    private SqlCommand GetUpdCmd()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        string strSql;

        //Update
        bool bFirst = true;
        strSql = "UPDATE [change] SET ";
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_change_id" || child.InnerText == "sys_itservice" || child.InnerText == "sys_assetsaffected")
                continue;

            if (htCols[child.InnerText] == null)
                continue;

            string strVal = GetUserInput(child);
            if (child.InnerText == "sys_change_timespent" && strVal != "")
            {
                strVal = RequestDA.Time2Mins(strVal);
            }

            if ((child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") || (child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                if (!bFirst)
                {
                    strSql += ",";
                }
                bFirst = false;

                if (strVal == "")
                {
                    strSql += "[" + child.InnerText + "]=NULL";
                }
                else
                {
                    strSql += "[" + child.InnerText + "]=@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (child.InnerText == "sys_changedate" || child.InnerText == "sys_startdate" || child.InnerText == "sys_finishdate" || child.InnerText == "sys_completedate" || htCols[child.InnerText].ToString() == "DateTime")
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

        strSql += " WHERE [sys_change_id]=@sys_change_id";
        parameter = new SqlParameter();
        parameter.ParameterName = "@sys_change_id";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = ViewState["sys_change_id"].ToString();
        cmd.Parameters.Add(parameter);

        cmd.CommandText = strSql;

        return cmd;
    }
    /// <summary>
    /// Save data to database
    /// </summary>
    /// <returns></returns>
    private bool SaveData()
    {
        SqlCommand cmd;

        string strOldPK = "";
        if (ViewState["sys_change_id"] != null)
        {
            strOldPK = ViewState["sys_change_id"].ToString();
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

        SaveItService();

        SaveAssets();

        if (LayUtil.IsNumeric(ViewState["newfromreq"].ToString()))
        {
            string strChangeId = ChangeDA.GetMaxChangeId();
            string strReqId = ViewState["newfromreq"].ToString();
            RequestDA.DeLinkReq2Change(strReqId, strChangeId);
            RequestDA.LinkReq2Change(strReqId, strChangeId);
        }
        else if (LayUtil.IsNumeric(ViewState["newfromproblem"].ToString()))
        {
            string strChangeId = ChangeDA.GetMaxChangeId();
            string strProblemId = ViewState["newfromproblem"].ToString();
            ProblemDA.DeLinkProblem2Change(strProblemId, strChangeId);
            ProblemDA.LinkProblem2Change(strProblemId, strChangeId);
        }

        return bRes;
    }

    private bool SaveAssets()
    {

        if (htCtrl["sys_assetsaffected"] == null)
        {
            return true;
        }

        string strChangeId = ViewState["sys_change_id"].ToString();

        if (strChangeId == "")
        {
            strChangeId = ChangeDA.GetMaxChangeId();
        }
        else
        {
            ChangeDA.ClearAssets(strChangeId);
        }

        RadListBox lb = (RadListBox)phCtrls.FindControl("lbsys_assetsaffected");
        if (lb != null)
        {
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ChangeDA.InsAssets(strChangeId, lb.Items[i].Text, lb.Items[i].Value);
            }
        }

        return true;
    }
    private bool SaveItService()
    {

        if (htCtrl["sys_itservice"] == null)
        {
            return true;
        }

        string strChangeId = ViewState["sys_change_id"].ToString();

        if (strChangeId == "")
        {
            strChangeId = ChangeDA.GetMaxChangeId();
        }
        else
        {
            ChangeDA.ClearItServices(strChangeId);
        }

        RadListBox lb = (RadListBox)phCtrls.FindControl("lbsys_itservice");
        if (lb != null)
        {
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ChangeDA.InsItServices(strChangeId, lb.Items[i].Value);
            }
        }

        return true;
    }
    /// <summary>
    /// Process the request after save data
    /// </summary>
    private bool ProcessUpdChange()
    {
        //Indicate if its new request
        bool bChangeNew = false;
        string strChangeId = "";
        if (ViewState["sys_change_id"] != null)
        {
            strChangeId = ViewState["sys_change_id"].ToString();
        }

        if (strChangeId == "")
        {
            bChangeNew = true;
            
            strChangeId = ChangeDA.GetMaxChangeId();

            ViewState["sys_change_id"] = strChangeId;
        }
        /*
        //Apply suspension for newly created request if need be
        if (strChangeId == "")
        {
            bchangeNew = true;

            strChangeId = ChangeDA.GetMaxChangeId();

            if (ChangeDA.ChangeInSus(strChangeId))
            {
                ChangeDA.ChangeInsertSus(strChangeId, GetUserInputDate("sys_changedate"), Session["User"].ToString());

                ChangeDA.UpdChangeEsc(strChangeId);
            }
        }
        */

        ///Create History/Audit
        string strAudit = SysDA.GetSettingValue("securityauditproblemdescfull", Application).ToLower();

        //Edit Request
        if (!bChangeNew)
        {
            foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
            {
                if (!(child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field") && !(child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_"))
                {
                    continue;
                }

                /*
                if (child.InnerText == "sys_change_hdmins" || child.InnerText == "sys_change_24mins" || child.InnerText == "sys_change_timespent")
                {
                    continue;
                }
                */

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

                    ChangeDA.InsChangeAudit(strChangeId, DateTime.Now, Session["User"].ToString(), "'Edited'", child.InnerText, strOrgVal, strNewVal);
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
                
                /*
                if (child.InnerText == "sys_change_hdmins" || child.InnerText == "sys_change_24mins" || child.InnerText == "sys_change_timespent")
                {
                    continue;
                }
                */

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

                ChangeDA.InsChangeAudit(strChangeId, DateTime.Now, Session["User"].ToString(), "'Created'", child.InnerText, strOrgVal, strNewVal);
            }
        }


        /*
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
        if (bchangeNew)
        {
            if (SysDA.GetSettingValue("appemailenduserrequestcreated", Application) == "true")
            {
                if (strEUser != "" && strEUserMail != "")
                {
                    LayUtil.SendReqEmail("req_created_euser", strEUserMail, "", "", strReqId, strReqClass, strEUser);
                }
            }
        }
        

        ///Send mail if Priority changes
        if (!bchangeNew)
        {
            string strOrgVal = GetValue("sys_requestpriority");
            string strNewVal = GetUserInputDD("sys_requestpriority");

            if (strOrgVal != strNewVal)
            {
                if (SysDA.GetSettingValue("appemaileuserchangestatus", Application) == "true")
                {
                    LayUtil.SendReqEmail("req_changepriority_enduser", strEUserMail, "", "", strReqId, strReqClass, strEUser);
                }

                RequestDA.UpdReqEmail(strReqId);
            }
        }

        */

        ///assignedto or assignedtoanalgroup changed
        string strOrgAssignedTo = GetValue("sys_assignedto");
        string strNewAssignedTo = GetUserInputText("sys_assignedto");

        string strOrgAssignedToGrp = GetValue("sys_assignedtoanalgroup");
        string strNewAssignedToGrp = GetUserInputText("sys_assignedtoanalgroup");

        if (bChangeNew)
        {
            strOrgAssignedTo = "";
            strOrgAssignedToGrp = "";
        }

        if (strOrgAssignedTo != strNewAssignedTo || strOrgAssignedToGrp != strNewAssignedToGrp)
        {
            /*
            bool strRespMarked = false;
            if (SysDA.GetSettingValue("responsemarkwhen", Application) == "0")
            {
                strRespMarked = true;
            }

            if (strRespMarked)
            {
                bool bResponded = false;
                if (GetValue("sys_responded") != "")
                {
                    bResponded = true;
                }

                if (!bResponded)
                {
                    if (strReqId != "" && strNewAssignedTo != "")
                    {
                        RequestDA.UpdReqRespond(strReqId, DateTime.Now, strNewAssignedTo);
                    }
                }
            }

             */ 

            //Put Email In Queue To Advise Analyst or Group of Analysts or both
            if (SysDA.GetSettingValue("appemailchangeassignuserflag", Application) == "true")
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
                            if (SysDA.GetSettingValue("appemailchangeassignuserflagsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }

                            LayUtil.SendOutEmail(Application, "change_assign_user", strUserEmail, strNewAssignedTo, "", "change", strChangeId, "", ChangeDA.GetChgMailCmd(strChangeId), strSignatureUser);
                        }
                    }
                }
            }

            if (SysDA.GetSettingValue("appemailchangeassignusergroupflag", Application) == "true")
            {
                if (strOrgAssignedToGrp != strNewAssignedToGrp)
                {
                    string strGrpEmail = "";
                    DataSet dsGrp;
                    if (SysDA.GetSettingValue("appemailchangeassignusergroupflag", Application) == "true" && strNewAssignedTo == "")
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
                            if (strGrpEmail != "")
                            {
                                LayUtil.SendOutEmail(Application,"change_assign_usergroup", strGrpEmail, drGrp["sys_username"].ToString(), "", "change", strChangeId, "", ChangeDA.GetChgMailCmd(strChangeId));
                            }
                        }
                    }

                }
            }

            /*
            ///Put Email In Queue To Advise End User
            ///
            if (SysDA.GetSettingValue("appemailassignenduserflag", Application) == "true")
            {
                if (strEUser != "" && strEUserMail != "")
                {
                    LayUtil.SendReqEmail("req_assign_enduser", strEUserMail, "", "", strReqId, strReqClass, strEUser);
                }
            }
             */ 
        }


        //Send out Email
        if (SysDA.GetSettingValue("appemailchangeapprovalstatusflag", Application) == "true")
        {
            string strOrgAppvStatus = GetValue("sys_approval_status");
            string strNewAppvStatus = GetUserInputDD("sys_approval_status");

            if (strOrgAppvStatus != strNewAppvStatus)
            {
                string strRequestedBy = GetUserInputText("sys_requestedby");

                if (strRequestedBy != "")
                {
                    DataSet dsTUser = UserDA.GetUserInfo(strRequestedBy);
                    if (dsTUser != null && dsTUser.Tables.Count > 0 && dsTUser.Tables[0].Rows.Count > 0)
                    {
                        string strEmail = dsTUser.Tables[0].Rows[0]["sys_email"].ToString();
                        if (strEmail != "")
                        {
                            LayUtil.SendOutEmail(Application, "change_approvalstatus_requestedby", strEmail, strRequestedBy, "", "change", strChangeId, "", ChangeDA.GetChgMailCmd(strChangeId));
                        }
                    }
                }
            }
        }

        if (SysDA.GetSettingValue("appemailchangeassigncabgroupflag", Application) == "true")
        {
            string strOrgCabGrp = GetValue("sys_assignedtocabgroup");
            string strNewCabGrp = GetUserInputText("sys_assignedtocabgroup");

            if (strOrgCabGrp != strNewCabGrp)
            {
                DataSet dsGrp = ChangeDA.GetCabGrpEmail(strNewCabGrp);
                if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drGrp in dsGrp.Tables[0].Rows)
                    {
                        string strGrpEmail = drGrp["sys_email"].ToString();
                        if (strGrpEmail != "")
                        {
                            LayUtil.SendOutEmail(Application, "change_assign_cabgroup", strGrpEmail, drGrp["sys_username"].ToString(), "", "change", strChangeId, "", ChangeDA.GetChgMailCmd(strChangeId));
                        }
                    }
                }
            }

        }
        //if created from incoming email, check and copy attachment
        string strMailId = ViewState["sys_mail_id"].ToString();
        if (LayUtil.IsNumeric(strMailId))
        {
            if (ChangeDA.CopyAttachFrmMail(strMailId, strChangeId))
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
    private bool SaveChangeData()
    {
        CheckBusinessRule();

        if (!CheckInput())
            return false;

        if (SaveData())
        {
            return ProcessUpdChange();
        }

        //LoadVal();

        return false;
    }


    private bool PromptPopup()
    {
        string strOldChgReqType = GetValue("sys_change_requesttype");
        string strNewChgReqType = GetUserInputText("sys_change_requesttype");

        string strOldChgType = htChgType[GetValue("sys_change_type")].ToString();
        string strNewChgType = htChgType[GetUserInputDD("sys_change_type")].ToString();

        bool bChgReqTypeChanged = false;
        if (strOldChgReqType != strNewChgReqType)
        {
            bChgReqTypeChanged = true;
        }
        bool bChgTypeChanged = false;
        if (strOldChgType != strNewChgType)
        {
            bChgTypeChanged = true;
        }

        if (bChgReqTypeChanged || bChgTypeChanged)
        {

            bool bChgReqTypeTemplate = true;
            DataSet dsChgReqTypeApprovl = LibChgReqTypeDA.GetChgReqTypeApprovalTemplate(strNewChgReqType);
            if (dsChgReqTypeApprovl == null || dsChgReqTypeApprovl.Tables.Count <= 0 || dsChgReqTypeApprovl.Tables[0].Rows.Count <= 0)
                bChgReqTypeTemplate = false;

            bool bChgTypeTemplate = true;
            DataSet dsChgTypeApprovl = LibChgTypeDA.GetChgTypeApprovalTemplate(strNewChgType);
            if (dsChgTypeApprovl == null || dsChgTypeApprovl.Tables.Count <= 0 || dsChgTypeApprovl.Tables[0].Rows.Count <= 0)
                bChgTypeTemplate = false;

            bool bApplyCab = true;
            if (bChgReqTypeTemplate || bChgTypeTemplate)
            {
                bool bExistApproval = false;
                DataSet dsChgApprovl = ChangeDA.GetChangeApproval(ViewState["sys_change_id"].ToString());
                if (dsChgApprovl == null || dsChgApprovl.Tables.Count <= 0 || dsChgApprovl.Tables[0].Rows.Count <= 0)
                {
                    bExistApproval = false;
                }
                else
                {
                    bExistApproval = true;
                }

                string strTemplateType = "";
                string strCabTemplatePriority = SysDA.GetSettingValue("appcabtemplatepriority", Application);
                if (strCabTemplatePriority == "" || strCabTemplatePriority == "changetype")
                {
                    if (bChgTypeTemplate)
                    {
                        if (bChgTypeChanged)
                        {
                            if (bExistApproval)
                            {
                                lbApprvMsg.Text = "The Change Type '" + strNewChgType + "' has a pre-defined approval template. Do you wish to replace the existing Change approval(s) with the pre-defined approval template for this Change Request?";
                            }
                            else
                            {
                                lbApprvMsg.Text = "The Change Type '" + strNewChgType + "' has a pre-defined approval template. Do you wish to apply the approval template to this Change Request?";
                            }
                            strTemplateType = "changetype";
                        }
                        else
                        {
                            bApplyCab = false;
                        }
                    }
                    else if (bChgReqTypeTemplate)
                    {
                        if (bChgReqTypeChanged)
                        {
                            if (bExistApproval)
                            {
                                lbApprvMsg.Text = "The Change Request Type '" + strNewChgReqType + "' has a pre-defined approval template. Do you wish to replace the existing Change approval(s) with the pre-defined approval template for this Change Request?";
                            }
                            else
                            {
                                lbApprvMsg.Text = "The Change Request Type '" + strNewChgReqType + "' has a pre-defined approval template. Do you wish to apply the approval template to this Change Request?";
                            }
                            strTemplateType = "changereqtype";
                        }
                        else
                        {
                            bApplyCab = false;
                        }
                    }
                    else
                    {
                        bApplyCab = false;
                    }
                }
                else
                {
                    if (bChgReqTypeTemplate)
                    {
                        if (bChgReqTypeChanged)
                        {
                            if (bExistApproval)
                            {
                                lbApprvMsg.Text = "The Change Request Type '" + strNewChgReqType + "' has a pre-defined approval template. Do you wish to replace the existing Change approval(s) with the pre-defined approval template for this Change Request?";
                            }
                            else
                            {
                                lbApprvMsg.Text = "The Change Request Type '" + strNewChgReqType + "' has a pre-defined approval template. Do you wish to apply the approval template to this Change Request?";
                            }
                            strTemplateType = "changereqtype";
                        }
                        else
                        {
                            bApplyCab = false;
                        }
                    }
                    else if (bChgTypeTemplate)
                    {
                        if (bChgTypeChanged)
                        {
                            if (bExistApproval)
                            {
                                lbApprvMsg.Text = "The Change Type '" + strNewChgType + "' has a pre-defined approval template. Do you wish to replace the existing Change approval(s) with the pre-defined approval template for this Change Request?";
                            }
                            else
                            {
                                lbApprvMsg.Text = "The Change Type '" + strNewChgType + "' has a pre-defined approval template. Do you wish to apply the approval template to this Change Request?";
                            }
                            strTemplateType = "changetype";
                        }
                        else
                        {
                            bApplyCab = false;
                        }
                    }
                    else
                    {
                        bApplyCab = false;
                    }
                }

                if (bApplyCab)
                {
                    if (SysDA.GetSettingValue("appneedconfirmationforapprovaltemplate", Application) == "false")
                    {
                        ApplyApprovalTemplate(strTemplateType);
                        //return false;
                    }
                    else
                    {
                        ViewState["TemplateType"] = strTemplateType;
                        dialogApplyApproval.WindowState = DialogWindowState.Normal;
                        return true;
                    }
                }
            }
        }

        if (ViewState["sys_change_requesttype"].ToString() != GetUserInputText("sys_change_requesttype"))
        {
            DataSet dsTask = TaskDA.GetChgReqTypeTasks(GetUserInputText("sys_change_requesttype"));
            if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
            {
                hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                //lbTaskTemplateMsg.Text = "The selected Change Request Type contains predefined " + Application["appactionterm"] + "s which can be added to the Change. These will replace any existings. Do you want to do this?";
                lbTaskTemplateMsg.Text = "The selected Request Type will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                return true;
            }
        }

        return false;

    }

    protected void btnApplyTaskTemplate_Click(object sender, ImageClickEventArgs e)
    {
        string strChgId = ViewState["sys_change_id"].ToString();
        if (!ChangeDA.ClearChgTasks(strChgId))
        {
            ShowMsg("Failed to delete existing " + Application["appactionterm"] + "s.");
        }

        if (!CreateTasksFromTemplate(GetUserInputText("sys_change_requesttype")))
        {
            ShowMsg("Failed to create " + Application["appactionterm"] + "s from template.");
            return;
        }

        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }

    private bool CreateTasksFromTemplate(string strChgReqTypeId)
    {
        string strChgId = ViewState["sys_change_id"].ToString();

        if (!LayUtil.IsNumeric(strChgId))
        {
            return false;
        }

        Hashtable htTasks = new Hashtable();

        DataSet dsTasks = TaskDA.GetChgReqTypeTasks(strChgReqTypeId);
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

                System.Text.StringBuilder sbSql = new System.Text.StringBuilder();
                System.Text.StringBuilder sbSqlVal = new System.Text.StringBuilder();
                sbSql.Append("INSERT INTO [action](sys_action_createdfrom, sys_change_id ");
                sbSqlVal.Append(" VALUES(NULL, " + strChgId);

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

                        System.Text.StringBuilder sbSql = new System.Text.StringBuilder();
                        System.Text.StringBuilder sbSqlVal = new System.Text.StringBuilder();
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
                                    if (LayUtil.IsNumeric(drActTask["sys_change_id"].ToString()))
                                    {
                                        LayUtil.SendOutEmail(Application, "actionchange_assign_notify", strEmail, strAssignedTo, "", "change", drActTask["sys_change_id"].ToString(), "", ChangeDA.GetChgActionMailCmd(strTaskId));
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

    /// <summary>
    /// Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button1_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = GetListURL(false);
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeChgStatus.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "UserTask.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        Response.Redirect(GetListURL(true));
    }

    /// <summary>
    /// Request History
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button5_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeHistory.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeAttach.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeComment.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeCallback.aspx?sys_change_id=" + ViewState["sys_change_id"];
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
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeCost.aspx?sys_change_id=" + ViewState["sys_change_id"];
            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Link
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button14_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeLink.aspx?sys_change_id=" + ViewState["sys_change_id"];
            if (!PromptPopup())
            {
                Response.Redirect(ViewState["PostConfirmURL"].ToString());
            }
        }
    }

    /// <summary>
    /// Cab Group
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button15_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeApproval.aspx?sys_change_id=" + ViewState["sys_change_id"];
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

                if (ViewState["sys_change_id"].ToString() == "")
                {
                    if (dr["isdefault"].ToString() != "")
                    {
                        dd.SelectedValue = dr["listitem"].ToString();
                    }
                }
            }

            if (ViewState["sys_change_id"].ToString() != "")
            {
                dd.SelectedValue = strList;
            }

        }
    }
    
    /*
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
            if (ctrl != null)
            {
                strVal = ctrl.SelectedValue;
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
            DataSet dsReqType = RequestDA.GetReqTypeById(strReqTypeId);
            if (dsReqType == null || dsReqType.Tables.Count <= 0 || dsReqType.Tables[0].Rows.Count <= 0)
            {
                return;
            }

            DataRow dr = dsReqType.Tables[0].Rows[0];
            Control ctrlRoot = phCtrls;

            if (Session["Role"].ToString() != "enduser" && SysDA.GetSettingValue("appautoassignrequest", Application).Length >= 2 && SysDA.GetSettingValue("appautoassignrequest", Application).Substring(0, 2).ToLower() == "ap")
            {
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


                string strSuggestedUser = "";
                string strSuggestedGrp = "";
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

                }


                TextBox ctrluser = (TextBox)ctrlRoot.FindControl("textsys_assignedto");
                TextBox ctrlgrp = (TextBox)ctrlRoot.FindControl("textsys_assignedtoanalgroup");
                if (strSuggestedUser != "")
                {
                    DataSet dsGrp = UserDA.GetUserGrpsDef(Session["User"].ToString());
                    if (dsGrp != null && dsGrp.Tables.Count > 0 && dsGrp.Tables[0].Rows.Count > 0)
                    {
                        strSuggestedGrp = dsGrp.Tables[0].Rows[0]["sys_analgroup"].ToString();
                    }
                    else
                    {
                        strSuggestedGrp = "";
                    }

                    ctrluser.Text = strSuggestedUser;
                    sys_assignedtoH.Text = strSuggestedUser;

                    ctrlgrp.Text = strSuggestedGrp;
                    sys_assignedtoanalgroupH.Text = strSuggestedGrp;

                }
                else
                {
                    if (strSuggestedGrp != "")
                    {
                        ctrlgrp.Text = strSuggestedGrp;
                        sys_assignedtoanalgroupH.Text = strSuggestedGrp;
                    }
                }
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

        protected void ReqTypeSel_ValueChanged(object sender, DropDownValueChangedEventArgs e)
        {
            Control ctrlRoot = phCtrls;
            WebDropDown ctrl;

            WebDropDown dd = (WebDropDown)sender;

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
                    //ctrl.se
                }
            }
            string strCurType = GetParentReqType(nIndex);
            if (e.NewValue.ToString() != "")
            {
                if (strCurType != "")
                {
                    strCurType += "/";
                }
                strCurType += e.NewValue.ToString();
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
                DataSet dsReq = RequestDA.GetReqTypeByClassParent("", strCurType);

                if (dsReq != null && dsReq.Tables.Count > 0)
                {
                    DropDownItem item;
                    if (dsReq.Tables[0].Rows.Count > 0)
                    {
                        //if (dsReq.Tables[0].Rows[0]["denycount"].ToString() == "0")
                        {
                            item = new DropDownItem("", dsReq.Tables[0].Rows[0]["sys_requesttypeparent_id"].ToString());
                            ctrl.Items.Add(item);
                        }
                    }

                    foreach (DataRow dr in dsReq.Tables[0].Rows)
                    {
                        //if (dr["denycount"].ToString() == "0")
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
                dsReq = RequestDA.GetReqTypeByClassParent("", "");
            }
            else
            {
                string strParent = GetParentReqType(nDepth);
                if (strParent != "")
                {
                    dsReq = RequestDA.GetReqTypeByClassParent("", strParent);
                }
            }

            if (dsReq != null && dsReq.Tables.Count > 0)
            {
                DropDownItem item;
                //if (nDepth != 1)
                {
                    if (dsReq.Tables[0].Rows.Count > 0)
                    {
                        //if (dsReq.Tables[0].Rows[0]["denycount"].ToString() == "0")
                        {
                            item = new DropDownItem("", "");
                            dd.Items.Add(item);
                        }
                    }
                }
                foreach (DataRow dr in dsReq.Tables[0].Rows)
                {
                    //if (dr["denycount"].ToString() == "0")
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
    */
    /// <summary>
    /// Set Priority Dropdown
    /// </summary>
    /// <param name="dd"></param>
    /// <param name="strVal"></param>
    private void SetDDPriority(WebDropDown dd, string strVal)
    {
        string strPriSite = GetUserVal("sys_priorityaccesssite");

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

        item = new DropDownItem("", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_priority_id"].ToString(), dr["sys_priority_id"].ToString());
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
    private void SetDDChangeType(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibChgTypeDA.GetList("");

        DropDownItem item;
        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_changetype"].ToString(), dr["sys_changetype_id"].ToString());
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
    private void SetDDApprovalStatus(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibChgTypeDA.GetList("");

        DropDownItem item;
        item = new DropDownItem("Pending", "Pending");
        dd.Items.Add(item);
        item = new DropDownItem("Rejected", "Rejected");
        dd.Items.Add(item);
        item = new DropDownItem("Approved", "Approved");
        dd.Items.Add(item);

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


    private bool LoadMailVal()
    {
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
            string strSub = LayUtil.RmvSubjectPrefix(drMailIn["sys_mail_subject"].ToString());
            string strProblemDesc = drMailIn["sys_mail_body"].ToString();
            htMailVal.Add("sys_mail_html", drMailIn["sys_mail_html"].ToString());

            /*
            if (drMailIn["sys_mail_html"].ToString() != "true")
            {
                strProblemDesc = LayUtil.Plain2HTMLBR(drMailIn["sys_mail_body"].ToString());
            }
            */
            if (strSub.Length > 255)
            {
                strSub = strSub.Substring(0, 255);
            }
            
            htMailVal.Add("sys_change_summary", strSub);
            htMailVal.Add("sys_change_description", strProblemDesc);

            string strSiteId = drMailIn["sys_siteid"].ToString();

            
            DataSet dsMUser = UserDA.GetUserInfoByEmail(drMailIn["sys_mail_fromemail"].ToString());
            if (dsMUser != null && dsMUser.Tables.Count > 0 && dsMUser.Tables[0].Rows.Count > 0)
            {
                string strUser = dsMUser.Tables[0].Rows[0]["sys_username"].ToString();
                htMailVal.Add("sys_requestedby", strUser);

            }
            DataSet dsEUser = UserDA.GetEUserInfoByEmail(drMailIn["sys_mail_fromemail"].ToString());
            if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
            {
                string strEUser = dsEUser.Tables[0].Rows[0]["sys_eusername"].ToString();
                htMailVal.Add("sys_eusername", strEUser);
            }

            htMailVal.Add("sys_siteid", strSiteId);
            
            string strDate = drMailIn["sys_mail_date"].ToString();
            if (strDate != "")
            {
                htMailVal.Add("sys_changedate", strDate);
            }
            else
            {
                htMailVal.Add("sys_changedate", DateTime.Now.ToString());
            }

            /*
            htMailVal.Add("sys_requestclass_id", ViewState["reqclass"].ToString());
            if (htMailVal["sys_eusername"] != null)
            {
                DataSet dsAsset = AssetDA.GetEUserReqLastAssetDS(htMailVal["sys_eusername"].ToString());
                if (dsAsset != null && dsAsset.Tables.Count > 0 && dsAsset.Tables[0].Rows.Count > 0)
                {
                    htMailVal.Add("sys_asset_id", dsAsset.Tables[0].Rows[0]["sys_asset_id"].ToString());
                    htMailVal.Add("sys_asset_location", dsAsset.Tables[0].Rows[0]["sys_asset_location"].ToString());
                }
            }
            */
            /*
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
                            if (strSub.Contains(drKeyword["sys_keyword"].ToString()))
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

            htMailVal.Add("sys_priority", strPriority);
             */
        }
        return true;
    }

    private bool LoadReqVal()
    {
        htReqVal = new Hashtable();
        string strReqId = ViewState["newfromreq"].ToString();
        if (!LayUtil.IsNumeric(strReqId))
        {
            return false;
        }

        DataSet dsReq = RequestDA.GetReqFullInfoById(strReqId);
        if (dsReq != null && dsReq.Tables.Count > 0 && dsReq.Tables[0].Rows.Count > 0)
        {
            DataRow dr = dsReq.Tables[0].Rows[0];
            htReqVal["sys_change_summary"] = dr["sys_problemsummary"].ToString();
            htReqVal["sys_change_description"] = dr["sys_problemdesc"].ToString();
        }

        return true;
    }

    private bool LoadProblemVal()
    {
        htProblemVal = new Hashtable();
        string strProblemId = ViewState["newfromproblem"].ToString();
        if (!LayUtil.IsNumeric(strProblemId))
        {
            return false;
        }

        DataSet dsProblem = ProblemDA.GetProblemById(strProblemId);
        if (dsProblem != null && dsProblem.Tables.Count > 0 && dsProblem.Tables[0].Rows.Count > 0)
        {
            DataRow dr = dsProblem.Tables[0].Rows[0];
            htProblemVal["sys_change_summary"] = dr["sys_problem_summary"].ToString();
            htProblemVal["sys_change_description"] = dr["sys_problem_description"].ToString();
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

                if (strField == "sys_change_description")
                {
                    if (htCtrlNode["sys_change_description"] != null)
                    {
                        XmlElement node = (XmlElement)htCtrlNode["sys_change_description"];
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
            else if (ViewState["newfromreq"].ToString() != "")
            {
                if (htReqVal == null)
                {
                    LoadReqVal();
                }

                if (htReqVal[strField] != null)
                {
                    return htReqVal[strField].ToString();
                }
            }
            else if (ViewState["newfromproblem"].ToString() != "")
            {
                if (htProblemVal == null)
                {
                    LoadProblemVal();
                }

                if (htProblemVal[strField] != null)
                {
                    return htProblemVal[strField].ToString();
                }
            }

            string str;
            switch (strField)
            {
                case "sys_change_id":
                    strVal = "(New Change)";
                    break;
                case "sys_requestedby":
                    str = SysDA.GetSettingValue("appautopopulaterequestedbychg", Application);
                    if (str == "true")
                    {
                        strVal = Session["User"].ToString();
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulaterequestedbyspecchg", Application) != "")
                        {
                            if (UserDA.CheckExistUser(SysDA.GetSettingValue("appautopopulaterequestedbyspecchg", Application)))
                            {
                                strVal = SysDA.GetSettingValue("appautopopulaterequestedbyspecchg", Application);
                            }
                        }
                    }
                    break;
                case "sys_assignedto":
                    str = SysDA.GetSettingValue("appautopopulateassignedtochg", Application);
                    if (str == "true")
                    {
                        strVal = Session["User"].ToString();
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulateassignedtospecchg", Application) != "")
                        {
                            if (UserDA.CheckExistUser(SysDA.GetSettingValue("appautopopulateassignedtospecchg", Application)))
                            {
                                strVal = SysDA.GetSettingValue("appautopopulateassignedtospecchg", Application);
                            }

                        }
                    }
                    break;
                case "sys_assignedtoanalgroup":
                    str = SysDA.GetSettingValue("appautopopulateassignedtochg", Application);
                    if (str == "true")
                    {
                        strVal = UserDA.GetUserDefGrp(Session["User"].ToString());
                    }
                    else if (str == "spec")
                    {
                        if (SysDA.GetSettingValue("appautopopulateassignedtospecchg", Application) != "")
                        {
                            strVal = UserDA.GetUserDefGrp(SysDA.GetSettingValue("appautopopulateassignedtospecchg", Application));
                        }
                    }
                    break;
                case "sys_assignedtocabgroup":
                    strVal = "";
                    break;
                case "sys_changedate":
                    strVal = DateTime.Now.ToString();
                    break;
                case "sys_impact":
                    strVal = SysDA.GetSettingValue("appdefaultimpactchg", Application);
                    break;
                case "sys_urgency":
                    strVal = SysDA.GetSettingValue("appdefaulturgencychg", Application);
                    break;
                case "sys_itservice":
                    strVal = SysDA.GetSettingValue("appdefaultitservicechg", Application);
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

            /*
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
            */
            if (strField == "sys_change_timespent")
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

    private string GetListURL(bool bCancel)
    {
        string strURL = "";
        if (ViewState["sys_mail_id"].ToString() != "")
        {
            if (bCancel)
            {
                strURL = "InMailInfo.aspx?sys_mail_id=" + ViewState["sys_mail_id"];
            }
            else
            {
                if (Session["MailList"] != null)
                {
                    strURL = Session["MailList"].ToString();
                }
                else
                {
                    strURL = "UserEMailPending.aspx?back=true";
                }
            } 
        }
        else if (LayUtil.IsNumeric(ViewState["newfromreq"].ToString()))
        {
            strURL = "ReqInfo.aspx?sys_request_id=" + ViewState["newfromreq"];
        }
        else if (LayUtil.IsNumeric(ViewState["newfromproblem"].ToString()))
        {
            strURL = "ProblemInfo.aspx?sys_problem_id=" + ViewState["newfromproblem"];
        }
        else
        {
            if (ViewState["from"].ToString() != "")
            {
                if (ViewState["from"].ToString() == "problem")
                {
                    strURL = "ProblemInfo.aspx?field=" + ViewState["field"] + "&sys_problem_id=" + ViewState["sys_problem_id"];
                }
                else if (ViewState["from"].ToString() == "req")
                {
                    strURL = "ReqInfo.aspx?field=" + ViewState["field"] + "&sys_request_id=" + ViewState["sys_request_id"];
                }
                else if (ViewState["from"].ToString() == "asset")
                {
                    strURL = "AWAssetInfoRPC.aspx?AssetID=" + LayUtil.GetQueryString(Request, "AssetID") + "&sys_asset_id=" + LayUtil.GetQueryString(Request, "sys_asset_id");
                }
                else
                {
                    strURL = Session["ChangeList"].ToString();
                }
            }
            else if (Session["ChangeList"] != null)
            {
                strURL = Session["ChangeList"].ToString();
            }
            else
            {
                strURL = "UserChange.aspx?user=" + Session["User"].ToString();
            }
        }
        return strURL;
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
        return FormDesignerBR.SaveUserChangeForm(Session["User"].ToString(), xmlForm.InnerXml);
        //return FormDesignerBR.SaveFormXML("change", xmlForm.InnerXml);
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
            if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
            {
                i++;
                nAdded++;
                continue;
            }

            htTabOrders[tab.Tabs.Count.ToString()] = i.ToString();

            ContentTabItem tabItem = new ContentTabItem();
            tab.Tabs.Add(tabItem);
            tabItem.ScrollBars = ContentOverflow.Auto;

            if (node.InnerText == "sys_change_description" || node.InnerText == "sys_change_impact" || node.InnerText == "sys_change_implementation" || node.InnerText == "sys_change_rollback")
            {
                tabItem.ScrollBars = ContentOverflow.Hidden;

                tabItem.Text = LayUtil.GetAttribute(node, "caption");

                string strVal = GetValue(node.InnerText);

                if (LayUtil.GetAttribute(node, "htmleditor") == "true")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();
                    table.Width = "100%";
                    //table.Height = "100%";

                    row.Cells.Add(cell);
                    //row.Height = "100%";

                    //WebDataGrid dgAttach = GenDG();
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
                    //row.Height = "100%";
                    cell.Width = "100%";
                    //cell.Height = "100%";

                    row.Cells.Add(cell);
                    RadEditor edt = LayUtil.CreateHTMLEditorInTab(Page, node);
                    edt.Content = strVal;


                    //edt.ToolsFile = "ToolsFile.xml";

                    //ctrl.TextMode = TextBoxMode.MultiLine;
                    //edt.Height = new Unit("100%");
                    edt.EnableResize = false;
                    cell.VAlign = "top";
                    cell.Controls.Add(edt);
                    table.Controls.Add(row);

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

                    if (node.InnerText == "sys_change_description")
                    {
                        edtChange = edt;
                    }
                    else if (node.InnerText == "sys_change_impact")
                    {
                        edtImpact = edt;
                    }
                    else if (node.InnerText == "sys_change_implementation")
                    {
                        edtImplementation = edt;
                    }
                    else if (node.InnerText == "sys_change_rollback")
                    {
                        edtRollback = edt;
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
                    //ctrl.Height = tab.Height;
                    ctrl.Style["overflow"] = "auto";
                    ctrl.Style["position"] = "relative";

                    /*
                    if (node.InnerText == "sys_change_description")
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
                    //tabItem.ID = node.Name;
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
                        btnAddTask.OnClientClick = "CheckAllowed(this);document.location='" + "TaskInfo.aspx?from=change&field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
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

                    //Session["TaskBack"] = "ReqInfo.aspx?sys_request_id=" + ViewState["sys_request_id"];
                }
                else if (node.Name == "sys_button5")
                {

                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);

                    //WebDataGrid dgAttach = GenDG();
                    //phHisFilter.Visible = true;
                    WebDropDown ddField = new WebDropDown();

                    DropDownItem item;
                    item = new DropDownItem("(None)", "");
                    ddField.Items.Add(item);

                    DataSet dsField = DataDesignDA.GetTblCol("change");
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
                    table.Width = "100%";
                    cell.Width = "100%";


                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);
                    dgHistory.Visible = true;


                    phDG.Controls.Remove(dgHistory);

                    cell.Controls.Add(dgHistory);
                    table.Controls.Add(row);

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

                    //WebDataGrid dgAttach = GenDG();

                    System.Web.UI.WebControls.ImageButton btnAttach = new System.Web.UI.WebControls.ImageButton();
                    btnAttach.Style["Cursor"] = "pointer !important";

                    btnAttach.ImageUrl = "Application_Images/16x16/add_icon_16px.png";

                    if (bAllowChange)
                    {
                        btnAttach.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";
                        btnAttach.Click += new ImageClickEventHandler(btnAttach_Click);
                    }
                    else
                    {
                        btnAttach.OnClientClick = "CheckAllowed(this);document.location='" + "ChangeAttach.aspx?field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
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
                    Session["ButtonBack"] = "ChangeInfo.aspx?field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"];
                }
                else if (node.Name == "sys_button7")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


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
                        btnAddCommnt.OnClientClick = "CheckAllowed(this);document.location='" + "ChangeCommtInfo.aspx?from=change&field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
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

                    //hdgCommntData.Visible = true;
                    //phDG.Controls.Remove(hdgCommntData);
                    //cell.Controls.Add(hdgCommntData);

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

                    //WebDataGrid dgAttach = GenDG();

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
                        btnAddCallback.OnClientClick = "CheckAllowed(this);document.location='" + "ChangeCallbackInfo.aspx?from=change&field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
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

                    Session["CallbackBack"] = "ChangeInfo.aspx?back=true&field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"];

                }
                else if (node.Name == "sys_button13")
                {

                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();


                    row.Cells.Add(cell);

                    //WebDataGrid dgAttach = GenDG();

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
                        btnAddCost.OnClientClick = "CheckAllowed(this);document.location='" + "CostTransn.aspx?back=Change&field=" + node.Name + "&from=change&sys_change_id=" + ViewState["sys_change_id"] + "'; return false;";
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

                    tabItem.Controls.Add(table);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();

                    row.Cells.Add(cell);

                    lbCost = new Label();
                    //lbCost.ID = "lbCost";
                    lbCost.Font.Bold = true;

                    cell.Align = "right";
                    cell.Controls.Add(lbCost);
                    table.Controls.Add(row);
                    table.Width = "100%";
                    cell.Width = "100%";

                    htTabs["cost"] = tabItem;
                    htTabText["cost"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["cost"] = node.Name;


                    UpdateCostList();

                    Session["CostBack"] = "ChangeInfo.aspx?back=true&field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"];
                }
                else if (node.Name == "sys_button14")
                {
                    HtmlTable table = new HtmlTable();
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell = new HtmlTableCell();
                    table.Width = "100%";
                    //table.Height = "100%";

                    row.Cells.Add(cell);
                    //row.Style["width"] = "100%";
                    //WebDataGrid dgAttach = GenDG();

                    System.Web.UI.WebControls.ImageButton btn = new System.Web.UI.WebControls.ImageButton();

                    btn.Style["Cursor"] = "pointer !important";

                    btn.ImageUrl = "Application_Images/16x16/add_icon_16px.png";
                    btn.ToolTip = "Edit Links";

                    btn.ID = "btnEditLink";
                    btn.Click += new ImageClickEventHandler(btnLink_Click);
                    btn.OnClientClick = "javascript:__doPostBack(this.name,'');return false;";

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
                    subItem.Text = Application["apprequestterm"] + "s";
                    tabLink.Tabs.Add(subItem);
                    subItem.Controls.Add(dgReq);
                    dgReq.Visible = true;

                    subItem = new ContentTabItem();
                    subItem.Text = "Problems";
                    tabLink.Tabs.Add(subItem);
                    subItem.Controls.Add(dgProblem);
                    dgProblem.Visible = true;


                    tabLink.Tabs[0].BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());
                    tabLink.Tabs[1].BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());

                    htTabs["link"] = tabItem;
                    htTabText["link"] = LayUtil.RplTm(Application, node.InnerText);
                    htTabName["link"] = node.Name;

                    UpdateLinkList();
                    tabItem.Text = LayUtil.RplTm(Application, node.InnerText);
                    Session["ButtonBack"] = "ChangeInfo.aspx?field=" + node.Name + "&sys_change_id=" + ViewState["sys_change_id"];
                }

            }

            tabItem.BackColor = LayUtil.GetColorFrmStr(Application["appbackcolor"].ToString());
            //tabItem.Attributes["Item"] = node.Name;
            tabItem.ImageAltText = node.Name;
            //tabItem.Attributes["TabIndex"] = i.ToString();

            if (ViewState["field"].ToString() == node.Name)
            {
                tab.SelectedIndex = tabItem.Index;
            }

            tab.ClientEvents.SelectedIndexChanged = "";
            i++;
            nAdded++;
        }
    }

    void btnAddTask_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "TaskInfo.aspx?from=change&field=" + htTabName["task"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }

    void btnAttach_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeAttach.aspx?from=change&field=" + htTabName["attach"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Session["ButtonBack"] = "ChangeInfo.aspx?field=" + htTabName["attach"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }

    void btnAddCallback_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeCallbackInfo.aspx?from=change&field" + htTabName["callback"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }
    void btnAddCommnt_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeCommtInfo.aspx?from=change&field=" + htTabName["comment"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }

    void btnAddCost_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "CostTransn.aspx?back=Change&from=change&field=" + htTabName["cost"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }
    void btnLink_Click(object sender, ImageClickEventArgs e)
    {
        if (SaveChangeData())
        {
            ViewState["PostConfirmURL"] = "ChangeLink.aspx?back=change&from=change&field=" + htTabName["link"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Session["ButtonBack"] = "ChangeInfo.aspx?back=change&field=" + htTabName["link"] + "&sys_change_id=" + ViewState["sys_change_id"];
            Response.Redirect(ViewState["PostConfirmURL"].ToString());
        }
    }

    public string GetAttachUrl(object obj)
    {
        return "~/GetAttachment.aspx?fn=" + Server.UrlEncode(obj.ToString()) + "&sys_change_id=" + ViewState["sys_change_id"].ToString();
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        dgTask.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgAttach.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgCost.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgReq.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgProblem.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgHistory.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
        dgCallback.InitializeRow += new InitializeRowEventHandler(dg_InitializeRow);
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
        LayUtil.SetFont(this.dgTask, Application);
        LayUtil.SetFont(this.dgAttach, Application);
        LayUtil.SetFont(this.dgCost, Application);
        LayUtil.SetFont(this.dgReq, Application);
        LayUtil.SetFont(this.dgProblem, Application);
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
        dgTask.DataSource = ChangeDA.GetChangeTask(ViewState["sys_change_id"].ToString());
        dgTask.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["task"];

        tabItem.Text = htTabText["task"] + " (" + ((DataSet)dgTask.DataSource).Tables[0].Rows.Count + ")";
    }
    private void UpdateAttachList()
    {
        dgAttach.ClearDataSource();

        string strId = ViewState["sys_change_id"].ToString();
        dgAttach.DataSource = ChangeDA.GetChangeAttachment(strId);
        dgAttach.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["attach"];

        tabItem.Text = htTabText["attach"] + " (" + ((DataSet)dgAttach.DataSource).Tables[0].Rows.Count + ")";
    }
    private void UpdateCallbackList()
    {
        dgCallback.ClearDataSource();
        string strId = ViewState["sys_change_id"].ToString();
        dgCallback.DataSource = ChangeDA.GetChangeCallback(strId);
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
        ChangeInfo page;
        public GridCell(string Field, ChangeInfo pPage)
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
                /*
                //tooltip.Text = strVal;
                int nStartIndex = strVal.IndexOf("<body");
                int nEndIndex = strVal.IndexOf("</body>");
                if (nStartIndex != -1 && nEndIndex != -1)
                {
                    strVal = strVal.Substring(nStartIndex , nEndIndex - nStartIndex);
                    strVal = strVal.Substring(strVal.IndexOf('\r') + 4);
                }
                */
                tooltip.Text = strVal;
                tooltip.Style["Z-Index"] = "20001";
                //tooltip.HideDelay = 60000;
                //tooltip.Width = new Unit(page.strTabWidth + "px");
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
        dgHistory.DataSource = ChangeDA.GetChangeHistory(ViewState["sys_change_id"].ToString(), strField);
        dgHistory.DataBind();

    }

    #region Link
    private void UpdateLinkList()
    {
        UpdateLinkReqList();
        UpdateLinkProblemList();
    }
    private void UpdateLinkReqList()
    {
        dgReq.ClearDataSource();
        string strId = ViewState["sys_change_id"].ToString();

        dgReq.DataSource = ChangeDA.GetLinkedRequest(strId);
        dgReq.DataBind();

    }
    private void UpdateLinkProblemList()
    {
        dgProblem.ClearDataSource();
        string strId = ViewState["sys_change_id"].ToString();

        dgProblem.DataSource = ChangeDA.GetLinkedProblem(strId);
        dgProblem.DataBind();
    }

    private void UpdateCommntList()
    {
        Hashtable htOpenStatus = new Hashtable();
        DataSet dsOpenStatus = ChangeDA.GetOpenStatus();
        if (dsOpenStatus != null && dsOpenStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsOpenStatus.Tables[0].Rows)
            {
                htOpenStatus.Add(dr["sys_change_status_id"].ToString(), "true");
            }
        }
        DataSet dsChange = ChangeDA.GetChangeById(ViewState["sys_change_id"].ToString());
        if (dsChange != null && dsChange.Tables.Count > 0 && dsChange.Tables[0].Rows.Count > 0)
        {
            if ((htOpenStatus[dsChange.Tables[0].Rows[0]["sys_change_status"].ToString()] != null))
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
            ds = ChangeDA.GetCommentByChangeIdWithGrpRes(ViewState["sys_change_id"].ToString(), Session["User"].ToString());
        }
        else
        {
            ds = ChangeDA.GetCommentByChangeIdUser(ViewState["sys_change_id"].ToString(), Session["User"].ToString());
        }

        hdgCommCell.Controls.Clear();
        Control c = LayUtil.LoadControl(Page, "ChangeHdgCommt.ascx", ViewState["sys_change_id"].ToString(), htTabName, ds, bClosed, bDelComment);
        //Control c = this.LoadControl(Object.GetType("ucGrid"),null);
        hdgCommCell.Controls.Add(c);

        ContentTabItem tabItem = (ContentTabItem)htTabs["comment"];

        tabItem.Text = htTabText["comment"] + " (" + ds.Tables[0].Rows.Count + ")";
    }
    public string GetReqUrl(object obj)
    {
        return "ReqInfo.aspx?field=" + htTabName["link"] + "&from=change&sys_problem_id=" + ViewState["sys_problem_id"].ToString() + "&sys_request_id=" + obj.ToString();
    }
    public string GetProblemUrl(object obj)
    {
        return "ProblemInfo.aspx?field=" + htTabName["link"] + "&from=change&sys_request_id=" + ViewState["sys_request_id"].ToString() + "&sys_problem_id=" + obj.ToString();
    }
    public string GetChangeUrl(object obj)
    {
        return "ChangeInfo.aspx?field=" + htTabName["link"] + "&from=change&sys_problem_id=" + ViewState["sys_problem_id"].ToString() + "&sys_change_id=" + obj.ToString();
    }

    #endregion
    private void UpdateCostList()
    {
        dgCost.ClearDataSource();

        DataSet dsTotal = CostDA.GetChangeTotalCost(ViewState["sys_change_id"].ToString());
        if (dsTotal != null && dsTotal.Tables.Count > 0 && dsTotal.Tables[0].Rows.Count > 0)
        {
            lbCost.Text = "Total: " + CostDA.FormatCost(dsTotal.Tables[0].Rows[0]["CostTotal"], SysDA.GetSettingValue("appcostcurrency", Application));
        }
        else
        {
            lbCost.Text = "Total: " + SysDA.GetSettingValue("appcostcurrency", Application) + "0.00";
        }

        dgCost.DataSource = CostDA.GetChangeCost(ViewState["sys_change_id"].ToString()); ;

        dgCost.DataBind();

        ContentTabItem tabItem = (ContentTabItem)htTabs["cost"];

        tabItem.Text = htTabText["cost"] + " (" + ((DataSet)dgCost.DataSource).Tables[0].Rows.Count + ")";
    }


    protected void btnDel_Click(object sender, ImageClickEventArgs e)
    {
        if (textItemH.Text == "attach")
        {
            string strFile = textIdH.Text;
            ChangeDA.DelChangeFile(ViewState["sys_change_id"].ToString(), strFile);
            dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            UpdateAttachList();
        }
        else if (textItemH.Text == "callback")
        {
            string strId = textIdH.Text;
            ChangeDA.DeleteCallback(strId);
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
        return "ChangeCallbackInfo.aspx?field=" + htTabName["callback"] + "&sys_callback_id=" + strCallId + "&sys_change_id=" + ViewState["sys_change_id"].ToString();
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
        return "TaskInfo.aspx?from=change&field=" + htTabName["task"] + "&sys_action_id=" + obj.ToString() + "&sys_change_id=" + ViewState["sys_change_id"];
    }

    public string GetCostTransactionUrl(object objVal)
    {
        string strCostId = objVal.ToString();
        return "CostTransn.aspx?back=Change&field=" + htTabName["cost"] + "&sys_cost_id=" + strCostId + "&sys_change_id=" + ViewState["sys_change_id"];
    }
    public string GetCommntUrl(object obj)
    {
        return "ChangeCommtInfo.aspx?from=change&field=" + htTabName["comment"] + "&inline=" + ViewState["inline"] + "&sys_comment_id=" + obj.ToString() + "&sys_change_id=" + ViewState["sys_change_id"];
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
    public string GetNoReqText()
    {
        return "No " + Application["apprequestterm"] + "s";
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

                    //string strBegin = GetUserInputDate("sys_actiondate");

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

    }

    protected void btnCloseApplyApprvl_Click(object sender, ImageClickEventArgs e)
    {
        dialogApplyApproval.WindowState = DialogWindowState.Hidden;

        if (ViewState["sys_change_requesttype"].ToString() != GetUserInputText("sys_change_requesttype"))
        {
            DataSet dsTask = TaskDA.GetChgReqTypeTasks(GetUserInputText("sys_change_requesttype"));
            if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
            {
                hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                //lbTaskTemplateMsg.Text = "The selected Change Request Type contains predefined " + Application["appactionterm"] + "s which can be added to the Change. These will replace any existings. Do you want to do this?";
                lbTaskTemplateMsg.Text = "The selected Request Type will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                return;
            }
        }

        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }
    protected void btnApplyApprvl_Click(object sender, ImageClickEventArgs e)
    {
        ApplyApprovalTemplate(ViewState["TemplateType"].ToString());
        dialogApplyApproval.WindowState = DialogWindowState.Hidden;

        if (ViewState["sys_change_requesttype"].ToString() != GetUserInputText("sys_change_requesttype"))
        {
            DataSet dsTask = TaskDA.GetChgReqTypeTasks(GetUserInputText("sys_change_requesttype"));
            if (dsTask != null && dsTask.Tables.Count > 0 && dsTask.Tables[0].Rows.Count > 0)
            {
                hlNoTaskTemplate.NavigateUrl = ViewState["PostConfirmURL"].ToString();
                //lbTaskTemplateMsg.Text = "The selected Change Request Type contains predefined " + Application["appactionterm"] + "s which can be added to the Change. These will replace any existings. Do you want to do this?";
                lbTaskTemplateMsg.Text = "The selected Request Type will replace all current " + Application["appactionterm"] + "s with the predefined " + Application["appactionterm"] + " Template. Please confirm.";
                dialogTaskTemplate.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                return;
            }
        }

        Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }
    private void ApplyApprovalTemplate(string strTemplateType)
    {
        string strChgId = ViewState["sys_change_id"].ToString();
        if (!ChangeDA.ClearChgApproval(strChgId))
        {
            ShowMsg("Failed to delete existing Approvals.");
        }

        string strNewChgReqType = GetUserInputText("sys_change_requesttype");

        string strNewChgType = htChgType[GetUserInputDD("sys_change_type")].ToString();


        string strChgType = "";
        string strChgReqType = "";

        if (strTemplateType == "changetype")
        {
            strChgType = strNewChgType;
        }
        else
        {
            strChgReqType = strNewChgReqType;
        }

        if (!CreateApprovalFromTemplate(strChgType, strChgReqType, strChgId))
        {
            ShowMsg("Failed to create " + Application["appactionterm"] + "s from template.");
            return;
        }

        //Response.Redirect(ViewState["PostConfirmURL"].ToString());
    }

    private bool CreateApprovalFromTemplate(string strChgType, string strChgReqType, string strChgId)
    {
        Hashtable htApproval = new Hashtable();
        DataSet dsApprovals = null;

        if (strChgReqType != "")
        {
            dsApprovals = LibChgReqTypeDA.GetChgReqTypeApprovalTemplate(strChgReqType);
        }
        else if (strChgType != "")
        {
            dsApprovals = LibChgTypeDA.GetChgTypeApprovalTemplate(strChgType);
        }

        if (dsApprovals != null && dsApprovals.Tables.Count > 0 && dsApprovals.Tables[0].Rows.Count > 0)
        {
            foreach (DataRow drApprovals in dsApprovals.Tables[0].Rows)
            {
                if (ChangeDA.InsChgApprover(strChgId, drApprovals["sys_username"].ToString(), drApprovals["sys_cab"].ToString(), drApprovals["sys_approval_state"].ToString()))
                {
                    string strApprovalId = ChangeDA.GetChgApprovalMaxId();
                    htApproval[drApprovals["sys_changeapproval_id"].ToString()] = strApprovalId;

                    if (drApprovals["sys_approval_state"].ToString() == LayUtil.idChgApprvState_Requested)
                    {
                        string strApprover = drApprovals["sys_username"].ToString();
                        if (strApprover != "")
                        {
                            DataSet dsUser = UserDA.GetUserInfo(strApprover);
                            if (dsUser != null && dsUser.Tables.Count > 0 && dsUser.Tables[0].Rows.Count > 0)
                            {
                                string strEmail = dsUser.Tables[0].Rows[0]["sys_email"].ToString();
                                if (strEmail != "")
                                {
                                    LayUtil.SendOutEmail(Application, "change_requested_cabmember", strEmail, strApprover, "", "change", strChgId, "", ChangeDA.GetChgMailCmd(strChgId, strApprover));
                                }
                            }
                        }
                    }
                }
                else
                {
                    LayUtil.LogInfo(LayUtil.LogType.Error, "Create Change Approvals From Template", "Failed to Insert Change Approval", DateTime.Now);
                }
            }

            //Setup Dependency
            foreach (DataRow drApprovals in dsApprovals.Tables[0].Rows)
            {
                string strId = drApprovals["sys_changeapproval_id"].ToString();
                string strRealId = htApproval[strId].ToString();
                DataSet dsDpd = LibChgReqTypeDA.GetDependedApprvl(strId);
                if (dsDpd != null && dsDpd.Tables.Count > 0 && dsDpd.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drDpd in dsDpd.Tables[0].Rows)
                    {
                        ChangeDA.AddDepend2Id(strRealId, htApproval[drDpd["sys_changeapprovaldependson_id"].ToString()].ToString());
                    }
                }
            }

        }

        return true;
    }

    
    #region Business Rule

    private bool MatchSel(DataRow dr)
    {
        string strField = dr["sys_brsel_field"].ToString();
        string strOp = dr["sys_brsel_op"].ToString();
        string strValue = dr["sys_brsel_value"].ToString().ToLower();

        string strVal = "";
        if (strField == "sys_company_id" || strField == "sys_eclient_id")  //removed || strField == "sys_requestpriority"
        {
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
                else if (strField == "sys_itservice")
                {
                    DataSet objDataSet = LibITServiceDA.GetItemInfoById(strVal);
                    if (objDataSet != null && objDataSet.Tables.Count > 0 && objDataSet.Tables[0].Rows.Count > 0)
                    {
                        strVal = objDataSet.Tables[0].Rows[0]["sys_itservice"].ToString();
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
        DataSet dsBR = BusinessRuleDA.GetEnableChgBRList();
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
                                    else if (strField == "sys_change_requesttype")
                                    {
                                        if (!LibChgReqTypeDA.CheckExist(strVal))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (strField == "sys_change_status")
                                    {
                                        if (!LibChgStatusDA.CheckExist(strVal))
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

    #endregion Business Rule
}
