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

using Infragistics.Web.UI;
using Infragistics.Web.UI.ListControls;
using Infragistics.WebUI.WebDataInput;
using Infragistics.WebUI.WebSchedule;
using Infragistics.Web.UI.GridControls;

using Infragistics.Web.UI.EditorControls;

using Telerik.Web.UI;

/// <summary>
/// Tasks detail page
/// </summary>

public partial class Analyst_TaskInfo : System.Web.UI.Page
{
    public XmlDocument xmlForm;

    public DataSet dsOldValue;
    public DataSet dsReqValue;
    public DataSet dsProblemValue;

    public Hashtable htCols;
    public Hashtable htColSize;

    public Hashtable htStatus;

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

    public System.Text.StringBuilder sbTxt = null;
    public int nLen = 0;

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
        LayUtil.CheckLoginUser(Session, Response);
        if (!IsPostBack)
        {
            SaveQueryStr();
            dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogCompleteConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogCloseReq.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            

            LayUtil.SetFont(this.dialogMsg, Application);
            LayUtil.SetFont(this.dialogCompleteConfirm, Application);
            //LayUtil.SetFont(this.dialogCloseReq, Application);
        }

        MyInit();

        //if (dialogTemplate.WindowState != Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden)
        //{
        //    UpdateTemplateList();
        //}
        //LayUtil.SetFont(this.form1, Application);

    }

    private void SaveQueryStr()
    {
        ViewState["sys_request_id"] = LayUtil.GetQueryString(Request, "sys_request_id");
        ViewState["sys_problem_id"] = LayUtil.GetQueryString(Request, "sys_problem_id");
        ViewState["sys_change_id"] = LayUtil.GetQueryString(Request, "sys_change_id");
        ViewState["sys_action_id"] = LayUtil.GetQueryString(Request, "sys_action_id");
        ViewState["from"] = LayUtil.GetQueryString(Request, "from");
        ViewState["field"] = LayUtil.GetQueryString(Request, "field");
    }
    /// <summary>
    /// Initialize controls and load values
    /// </summary>
    private void MyInit()
    {

        //Load Form
        string strReqId = ViewState["sys_request_id"].ToString();
        string strProblemId = ViewState["sys_problem_id"].ToString();
        string strChangeId = ViewState["sys_change_id"].ToString();
        string strActionId = ViewState["sys_action_id"].ToString();


        string strXml = "";
        DataSet dsForm = FormDesignerDA.LoadFormXML("action");

        if (dsForm != null && dsForm.Tables.Count > 0 && dsForm.Tables[0].Rows.Count > 0)
        {
            strXml = dsForm.Tables[0].Rows[0]["sys_formprofile_xml"].ToString();

            xmlForm = new XmlDocument();

            if (strXml != "")
            {
                xmlForm.LoadXml(strXml);
            }
        }
        else
        {
            LayUtil.LogInfo(LayUtil.LogType.Error, "Couldn't load action form", "DataSet doesn't contain data", DateTime.Now);
            Response.End();
        }

        /*
        //Set Status Hashtable
        DataSet dsStatus = LibReqStatusDA.GetReqStatusList(-1);
        htStatus = new Hashtable();
        if (dsStatus != null && dsStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsStatus.Tables[0].Rows)
            {
                htStatus.Add(dr["sys_status_ID"].ToString(), dr["sys_status"].ToString());
            }
        }
        */

        //Get Current logon user info
        dsUser = UserDA.GetUserInfo(Session["User"].ToString());

        dsReqValue = RequestDA.GetReqFullInfoById(strReqId);
        dsProblemValue = ProblemDA.GetProblemFullInfoById(ViewState["sys_problem_id"].ToString());

        ///Load request and related value
        if (ViewState["sys_action_id"].ToString() != "")
        {
            LoadVal();


            string strDate = GetValue("sys_actiondate");
            if (strDate == "")
            {
                strDate = DateTime.Now.ToString();
            }

            if(strReqId != "")
            {
                LoadSus(strReqId,"", strDate);
            }
            else if (strProblemId != "")
            {
                LoadSus("",strProblemId, strDate);
            }
            else if (strChangeId != "")
            {
                LoadSus("", "", strDate);
            }
            else
            {
                LoadSus("", "", strDate);
            }
        }

        string strTbl = "action";

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

        LoadCtrl();

        //ds = LayUtilDA.GetTableValue("priority", "sys_priority_id", LayUtil.GetQueryString(Request, "sys_priority_id"));

        LayUtil.SetFont(this.dialogMsg, Application);
    }

    /// <summary>
    /// Load Value From Database
    /// </summary>
    private void LoadVal()
    {
        if (xmlForm.DocumentElement == null)
            return;

        string strTbl = "action";
        string strSql = "SELECT [" + strTbl + "].* ";

        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            string strDispTbl = node.GetAttribute("displaytable");
            string strDispLink = node.GetAttribute("displaytablelink");
            string strLink = node.GetAttribute("mastertablelink");

            if (strDispTbl != "" && strDispTbl != strTbl)
            {
                strSql += ", (SELECT " + node.InnerText + " FROM [" + strDispTbl + "] WHERE [" + strDispTbl + "]." + strDispLink + "=[" + strTbl + "]." + strLink + ") AS " + node.InnerText;
            }
        }

        strSql += " FROM [" + strTbl + "]";

        if (ViewState["sys_action_id"] == null || ViewState["sys_action_id"].ToString() == "")
        {
            strSql += " WHERE sys_action_id = Null";
        }
        else
        {
            strSql += " WHERE sys_action_id='" + ViewState["sys_action_id"].ToString().Replace("'", "''") + "'";
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

        htChildName = new Hashtable();

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
            if ((child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field") || (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_"))
            {
                if (htChildName[child.InnerText] == null)
                {
                    htChildName.Add(child.InnerText, child.Name);
                }
            } 
            
            string strVal = GetValue(child.InnerText);

            if (!IsPostBack)
            {
                if (child.InnerText == "sys_request_id")
                {
                    sys_request_idH.Text = strVal;
                }
                else if (child.InnerText == "sys_problem_id")
                {
                    sys_problem_idH.Text = strVal;
                }
                else if (child.InnerText == "sys_change_id")
                {
                    sys_change_idH.Text = strVal;
                }
                else if (child.InnerText == "sys_actiontype_id")
                {
                    sys_actiontype_idH.Text = strVal;
                }
                else if (child.InnerText == "sys_username")
                {
                    sys_usernameH.Text = strVal;
                }
            }
            if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "label")
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

                if (child.InnerText == "sys_actionstatus")
                {
                    if (strVal == "1")
                    {
                        ctrl.Text = "Complete";
                    }
                    else
                    {
                        ctrl.Text = "Scheduled";
                    }
                }
                else if (child.InnerText == "sys_action_24mins" || child.InnerText == "sys_action_hdmins")
                {
                    string strDispFormat = SysDA.GetSettingValue("statsdurationdisplay", Application);
                    ctrl.Text = RequestDA.Mins2Time(strVal, strDispFormat);
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

                    if (child.GetAttribute("tabindex") == "1")
                    {
                        ctrl.Focus();
                    }
                }
                else
                {
                    string strFieldType = DataDesignDA.GetFieldType("action", child.InnerText);
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

                //Save Button
                if (child.Name == "sys_button1")
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                    ctrl.ImageUrl = child.GetAttribute("image");
                    ctrl.Click += new ImageClickEventHandler(sys_button1_Click);

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
                else if(child.Name == "sys_button2")
                {
                    System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                    ctrl.ImageUrl = child.GetAttribute("image");
                    ctrl.Click += new ImageClickEventHandler(sys_button2_Click);

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
                    if (ViewState["sys_action_id"].ToString() != "")
                    {
                        System.Web.UI.WebControls.ImageButton ctrl = new System.Web.UI.WebControls.ImageButton();

                        ctrl.ImageUrl = child.GetAttribute("image");
                        ctrl.OnClientClick = "DelConfirm(this);return false;";

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
            }
            else
            {
                if (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_")
                {
                    if (child.InnerText == "sys_action_id")
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

                        ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                        ctrl.Style["Left"] = child.GetAttribute("left") + "px";
                        ctrl.Style["Position"] = "absolute";

                        ctrl.ReadOnly = true;

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
                    else if (child.InnerText == "sys_actiondate")
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

                    }
                    else if (child.InnerText == "sys_actionpriority")
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
                        SetDDPriority(ctrl, strVal);

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
                    else if (child.InnerText == "sys_username")
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

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectuser('" + ctrl.ClientID + "');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select " + Application["appuserterm"];

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_actiontype_id")
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

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectactiontype('" + ctrl.ClientID + "');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select " + Application["appactiontypeterm"];

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_request_id")
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

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectrequest('" + ctrl.ClientID + "');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select " + Application["apprequestterm"];

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_problem_id")
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

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectproblem('" + ctrl.ClientID + "');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select Problem";

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_change_id")
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

                        //Create Select image
                        HyperLink hl = new HyperLink();
                        hl.NavigateUrl = "javascript:selectchange('" + ctrl.ClientID + "');";

                        hl.ImageUrl = "Application_Images/16x16/select_16px.png";
                        hl.ToolTip = "Select Change";

                        hl.ID = "hl" + child.Name;

                        hl.Style["Top"] = child.GetAttribute("top") + "px";
                        hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + LayUtil.PixelSpace).ToString() + "px";
                        hl.Style["Position"] = "absolute";

                        ctrlRoot.Controls.Add(hl);

                    }
                    else if (child.InnerText == "sys_actionstatus")
                    {
                        WebDropDown ctrl = new WebDropDown();
                        ctrl.ID = "dd" + child.Name;

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            ctrl.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                         //written by Sparsh ID 77
                        if (ctrl.ID == "ddsys_field6")
                        {
                            ctrl.Button.Visible = false;
                            ctrl.Enabled = true;
                        }
                        // End by sparsh ID 77

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
                        DropDownItem item = new DropDownItem("Scheduled", "0");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("Complete", "1");
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
                    else if (child.InnerText == "sys_actionscheduleddate")
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


                    }
                    else if (child.InnerText == "sys_actioncompleteddate")
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

                    }
                    else if (child.InnerText == "sys_action_notifybeforedue")
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

                    }
                    else if (child.InnerText == "sys_EnableReminder")
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
                        DropDownItem item = new DropDownItem("Enabled", "true");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("Disabled", "false");
                        ctrl.Items.Add(item);

                        if (strVal == "")
                        {
                            ctrl.SelectedItemIndex = 0;
                        }
                        else
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

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            ctrl.Focus();
                        }
                    }
                    else if (child.InnerText == "sys_ReminderInterval")
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
                        DropDownItem item = new DropDownItem("0 minutes", "0");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("5 minutes", "300");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("10 minutes", "600");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("15 minutes", "900");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("30 minutes", "1800");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("1 hour", "3600");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("2 hours", "7200");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("4 hours", "14400");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("8 hours", "28800");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("0.5 days", "43200");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("1 day", "86400");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("2 days", "172800");
                        ctrl.Items.Add(item);
                        ctrl.SelectedItemIndex = 0;

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
                    if (LayUtil.GetAttribute(child,"user").ToLower() == Session["User"].ToString().ToLower())
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
        string strVal = "";
        Control ctrlRoot = phCtrls;

        if (strField == "sys_request_id")
        {
            return sys_request_idH.Text;
        }
        else if (strField == "sys_problem_id")
        {
            return sys_problem_idH.Text;
        }
        else if (strField == "sys_change_id")
        {
            return sys_change_idH.Text;
        }
        else if (strField == "sys_actiontype_id")
        {
            return sys_actiontype_idH.Text;
        }
        else if (strField == "sys_username")
        {
            return sys_usernameH.Text;
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
        string strVal = "";
        Control ctrlRoot = phCtrls;

        if (htChildName[strField] != null)
        {
            WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + htChildName[strField].ToString());
            if (ctrl != null)
            {
                strVal = ctrl.SelectedValue;
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
        if (child.InnerText == "sys_request_id")
        {
            return sys_request_idH.Text;
        }
        else if (child.InnerText == "sys_problem_id")
        {
            return sys_problem_idH.Text;
        }
        else if (child.InnerText == "sys_change_id")
        {
            return sys_change_idH.Text;
        }
        else if (child.InnerText == "sys_actiontype_id")
        {
            return sys_actiontype_idH.Text;
        }
        else if (child.InnerText == "sys_username")
        {
            return sys_usernameH.Text;
        }

        string strVal = "";
        Control ctrlRoot = phCtrls;
        if (child.Name.Length >= 5 && child.Name.Substring(0, 5).ToLower() == "field")
        {

            if (child.Name.Length >= 6 && child.Name.Substring(0, 6).ToLower() == "fieldc")
            {
                WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                if (ctrl != null)
                {
                    strVal = ctrl.SelectedValue;
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
                else if (DataDesignDA.GetFieldType("action", child.InnerText) == "DateTime")
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
                if (child.InnerText == "sys_actionpriority" || child.InnerText == "sys_actionstatus" || child.InnerText == "sys_ReminderInterval")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null)
                    {
                        strVal = ctrl.SelectedValue;
                    }
                }
                else if (child.InnerText == "sys_EnableReminder")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null)
                    {
                        strVal = ctrl.SelectedValue;
                        if (strVal == "false")
                            return "0";
                        else
                            return "1";
                    }
                }
                else if (child.InnerText == "sys_actiondate" || child.InnerText == "sys_actionscheduleddate" || child.InnerText == "sys_actioncompleteddate" || child.InnerText == "sys_action_notifybeforedue")
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
            if (child.InnerText == "sys_action_id")
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
                if (child.InnerText == "sys_action_timespent")
                {
                    if (strVal != "" && !CheckDuration(strVal))
                    {
                        ShowMsg("You must enter a valid duration (format specified in settings) in the " + child.GetAttribute("caption") + " box.");
                        return false;
                    }
                }
                else if (strVal != "" && child.InnerText != "sys_action_id" && child.InnerText != "sys_EnableReminder")
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
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        string strSql;

        //Insert
        bool bFirst = true;

        strSql = "INSERT INTO [action]( ";
        string strSqlVal = " VALUES( ";
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_action_id")
                continue;

            string strVal = GetUserInput(child);
            if (child.InnerText == "sys_action_timespent" && strVal != "")
            {
                strVal = RequestDA.Time2Mins(strVal);
            }
            else if (child.InnerText == "sys_action_notifybeforedue")
            {
                if (strVal == "")
                {
                    if (LayUtil.IsNumeric(SysDA.GetSettingValue("useractionremindhours", Application)))
                    {
                        string str = GetUserInputDate("sys_actionscheduleddate");
                        if (str != "")
                        {
                            int nRmdHours = int.Parse(SysDA.GetSettingValue("useractionremindhours", Application));
                            TimeSpan ts = new TimeSpan(nRmdHours, 0, 0);

                            DateTime dtRmdTime = DateTime.Parse(str).Subtract(ts);
                            strVal = dtRmdTime.ToString();

                            if (DateTime.Compare(dtRmdTime, DateTime.Now) < 0)
                            {
                                strVal = "";
                            }

                        }
                    }

                }
            }

            if ((child.Name.Length >= 9 && child.Name.Substring(0, 9) == "sys_field") || (child.Name.Length >= 5 && child.Name.Substring(0, 5) == "field"))
            {
                if (!bFirst)
                {
                    strSql += ",";
                    strSqlVal += ",";

                }
                bFirst = false;

                strSql += "["+child.InnerText+"]";

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
                    if (child.InnerText == "sys_actiondate" || child.InnerText == "sys_actionscheduleddate" || child.InnerText == "sys_actioncompleteddate" || child.InnerText == "sys_action_notifybeforedue" || DataDesignDA.GetFieldType("action", child.InnerText) == "DateTime")
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


        string strPriority = GetUserInputDD("sys_actionpriority");

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

        string strBegin = GetUserInputDate("sys_actiondate");
        //string strSiteId = GetReqValue("sys_siteid");

        if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0 ) && strBegin != "")
        {
            string strOrgBegin = GetValue("sys_actiondate");
            DateTime dtBegin = DateTime.Parse(strBegin);
            DateTime dtOrgBegin = DateTime.Parse(strOrgBegin);

            if (DateTime.Compare(dtBegin, dtOrgBegin) != 0)
            {
                LoadSus(GetUserInputText("sys_request_id"),GetUserInputText("sys_problem_id"), strBegin);
            }


            if (dReqReslvHr != 0)
            {
                TimeSpan ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                string strTime = RequestDA.GetDurationTime(strBegin, ts, listSus);

                if (strTime != "")
                {
                    strSql += ", [sys_resolve]";
                    strSqlVal += ",@sys_resolve";

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
                    strSql += ", [sys_escalate1]";
                    strSqlVal += ", @sys_escalate1";

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
                    strSql += ", [sys_escalate2]";
                    strSqlVal += ", @sys_escalate2";

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
                    strSql += ", [sys_escalate3]";
                    strSqlVal += ", @sys_escalate3";

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@sys_escalate3";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = DateTime.Parse(strTime);
                    cmd.Parameters.Add(parameter);
                }
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
        strSql = "UPDATE [action] SET ";
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (child.InnerText == "sys_action_id")
                continue;


            string strVal = GetUserInput(child);

            if (child.InnerText == "sys_action_timespent" && strVal != "")
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
                    strSql += "["+child.InnerText + "]=NULL";
                }
                else
                {
                    strSql += "["+child.InnerText + "]=@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (child.InnerText == "sys_actiondate" || child.InnerText == "sys_actionscheduleddate" || child.InnerText == "sys_actioncompleteddate" || child.InnerText == "sys_action_notifybeforedue" || DataDesignDA.GetFieldType("action", child.InnerText) == "DateTime")
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


        ///Set Escalation setting
        string strPriority = GetUserInputDD("sys_actionpriority");

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

        string strBegin = GetUserInputDate("sys_actiondate");
        if ((dReqReslvHr != 0 || dReqEsc1Hr != 0 || dReqEsc2Hr != 0 || dReqEsc3Hr != 0 ) && strBegin != "")
        {
            string strOrgBegin = GetValue("sys_actiondate");
            DateTime dtOrgBegin = DateTime.Parse(strOrgBegin);

            DateTime dtBegin = DateTime.Parse(strBegin);

            if (DateTime.Compare(dtBegin,dtOrgBegin) != 0)
            {
                LoadSus(GetUserInputText("sys_request_id"), GetUserInputText("sys_problem_id"), strBegin);
            }


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
        parameter.Value = ViewState["sys_action_id"].ToString();
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
        if (ViewState["sys_action_id"] != null)
        {
            strOldPK = ViewState["sys_action_id"].ToString();
        }

        if (strOldPK == "")
        {
            cmd = GetInsCmd();
            return LayUtilDA.RunSqlCmd(cmd); ;
        }
        else
        {
            cmd = GetUpdCmd();
            return LayUtilDA.RunSqlCmd(cmd); ;
        }

    }

    /// <summary>
    /// Process the request after save data
    /// </summary>
    private void ProcessUpdAction()
    {
        //Indicate if its new request
        string strActionId = "";
        if (ViewState["sys_action_id"] != null)
        {
            strActionId = ViewState["sys_action_id"].ToString();
        }

        //Apply suspension for newly created request if need be
        if (strActionId == "")
        {
            strActionId = RequestDA.GetMaxActionId();
        }

        ///user changed
        string strOrgUser = GetValue("sys_username");
        string strNewUser = GetUserInputText("sys_username");

        if (strNewUser != "" && strOrgUser != strNewUser && strNewUser != Session["User"].ToString())
        {
            DataSet dsNewUser = UserDA.GetUserInfo(strNewUser);
            if (dsNewUser != null && dsNewUser.Tables.Count > 0 && dsNewUser.Tables[0].Rows.Count > 0)
            {
                string strEmail = dsNewUser.Tables[0].Rows[0]["sys_email"].ToString();
                if (strEmail != "")
                {
                    DataSet dsAction = RequestDA.GetReqActionInfo(strActionId);
                    if (dsAction != null && dsAction.Tables.Count > 0 && dsAction.Tables[0].Rows.Count > 0)
                    {
                        if (LayUtil.IsNumeric(sys_request_idH.Text) && Application["appemailuserassignaction"].ToString() == "true")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemailuserassignactionsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }

                            LayUtil.SendOutEmail(Application, "action_assign_notify", strEmail, strNewUser, "", "request", dsAction.Tables[0].Rows[0]["sys_request_id"].ToString(), dsAction.Tables[0].Rows[0]["sys_requestclass_id"].ToString(), RequestDA.GetReqActionMailCmd(strActionId), strSignatureUser);
                        }
                        else if (LayUtil.IsNumeric(sys_problem_idH.Text) && Application["appemailproblemuserassignaction"].ToString() == "true")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemailproblemuserassignactionsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }
                            LayUtil.SendOutEmail(Application, "actionproblem_assign_notify", strEmail, strNewUser, "", "problem", dsAction.Tables[0].Rows[0]["sys_problem_id"].ToString(), "", ProblemDA.GetProblemActionMailCmd(strActionId));
                        }
                        else if (LayUtil.IsNumeric(sys_change_idH.Text) && Application["appemailchangeuserassignaction"].ToString() == "true")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemailchangeuserassignactionsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }
                            LayUtil.SendOutEmail(Application, "actionchange_assign_notify", strEmail, strNewUser, "", "change", dsAction.Tables[0].Rows[0]["sys_change_id"].ToString(), "", ChangeDA.GetChgActionMailCmd(strActionId));
                        }
                        else if(Application["appemailuserassignaction"].ToString() == "true")
                        {
                            string strSignatureUser = "";
                            if (SysDA.GetSettingValue("appemailuserassignactionsignature", Application) == "true")
                            {
                                strSignatureUser = Session["User"].ToString();
                            }

                            LayUtil.SendOutEmail(Application, "action_assign_notify", strEmail, strNewUser, "", "request", "", "", RequestDA.GetReqActionMailCmd(strActionId), strSignatureUser);
                        }
                    }
                }
            }
        }

    }

    /// <summary>
    /// Save request data to db
    /// </summary>
    /// <returns></returns>
    private bool SaveActionData()
    {
        if (!CheckInput())
            return false;

        if (SaveData())
        {
            ViewState["sys_request_id"] = GetUserInputText("sys_request_id");
            ViewState["sys_problem_id"] = GetUserInputText("sys_problem_id");
            ViewState["sys_change_id"] = GetUserInputText("sys_change_id");

            ProcessUpdAction();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button1_Click(object sender, ImageClickEventArgs e)
    {
        if (!SaveActionData())
        {
            return;
        }

        NavBack();
    }

    /// <summary>
    /// Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button2_Click(object sender, ImageClickEventArgs e)
    {
        NavBack();
    }

    private void NavBack()
    {
        string strURL;
        if (ViewState["from"].ToString() == "req")
        {
            strURL = "ReqInfo.aspx?back=true&field=" + ViewState["field"] + "&sys_request_id=" + ViewState["sys_request_id"];
        }
        else if (ViewState["from"].ToString() == "problem")
        {
            strURL = "ProblemInfo.aspx?back=true&field=" + ViewState["field"] + "&sys_problem_id=" + ViewState["sys_problem_id"];
        }
        else if (ViewState["from"].ToString() == "change")
        {
            strURL = "ChangeInfo.aspx?back=true&field=" + ViewState["field"] + "&sys_change_id=" + ViewState["sys_change_id"];
        }
        else if (!LayUtil.IsNumeric(ViewState["sys_action_id"].ToString()))
        {
            strURL = "UserTask.aspx?user=" + Session["User"].ToString();
        }
        else
        {
            if (Session["TaskList"] != null)
            {
                strURL = Session["TaskList"].ToString();
                if (strURL.IndexOf("back=true") == -1)
                {
                    if (strURL.IndexOf(".aspx?") == -1)
                    {
                        strURL += "?";
                    }
                    else
                    {
                        strURL += "&";
                    }

                    strURL += "back=true";

                }
            }
            else
            {
                strURL = "";
            }
        }
        Response.Redirect(strURL);
    }

    /// <summary>
    /// Button3 to complete task
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void imgComplete_Click(object sender, ImageClickEventArgs e)
    {
        if (!SaveActionData())
        {
            return;
        }

        string strActionId = ViewState["sys_action_id"].ToString();
        if (strActionId == "")
        {
            return;
        }

        //check required field
        DataTable dtTaskInfo = TaskDA.GetTaskInfoById(strActionId);
        if (dtTaskInfo == null || dtTaskInfo.Rows.Count <= 0)
        {
            return;
        }

        DataRow drTaskInfo = dtTaskInfo.Rows[0];        
        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            if (node.GetAttribute("closereq") == "true" && drTaskInfo[node.InnerText].ToString() == "")
            {
                dialogCompleteConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
                ShowMsg("You must enter a value in the " + node.GetAttribute("caption") + " box before you close this " + Application["appactionterm"] + ".");
                return;
            }
        }
        //

        string strActionDate = GetUserInputDate("sys_actiondate");

        if (strActionDate != "")
        {
            string strDurHD = ((int)(RequestDA.GetReqDuration(strActionDate, DateTime.Now, listSus))).ToString();
            string strDur24 = ((int)(RequestDA.GetReqDuration24Min(strActionDate, DateTime.Now))).ToString();

            RequestDA.UpdActionComplete(strActionId, DateTime.Now, strDurHD, strDur24);
        }
        else
        {
            RequestDA.UpdActionComplete(strActionId, DateTime.Now, "0", "0");
        }

        string strReqId;
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
                        strReqId = drDepnd["sys_request_id"].ToString();
                        string strProblemId = drDepnd["sys_problem_id"].ToString();
                        string strChangeId = drDepnd["sys_change_id"].ToString();

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
                    parameter.Value = ViewState["sys_action_id"].ToString();
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
                                else if (Application["appemailuserassignaction"].ToString() == "true")
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
        strReqId = GetUserInputText("sys_request_id");
        if (LayUtil.IsNumeric(strReqId))
        {
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

                    /*
                    //Check For Incomplete Tasks Against Requests
                    if(SysDA.GetSettingValue("apprequestcloseactionoption",Application) != "nowarn")
                    {
                        DataSet dsReqOpenAction = RequestDA.GetReqOpenActions(strReqId);
                        if (dsReqOpenAction != null && dsReqOpenAction.Tables.Count > 0 && dsReqOpenAction.Tables[0].Rows.Count > 0)
                        {
                            if (SysDA.GetSettingValue("apprequestcloseactionoption", Application) == "warn")
                            {
                                strMsg = "This " + Application["apprequestterm"] + " contains incomplete tasks. Do you wish to close anyway?";
                                lbCloseReqMsg.Text = strMsg;
                                dialogCloseReq.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                                return;
                            }
                            else if (SysDA.GetSettingValue("apprequestcloseactionoption", Application) == "prevent")
                            {
                                strMsg = "You cannot close this " + Application["apprequestterm"] + " as it contains incomplete "+Application["appactionterm"]+".";
                                ShowMsg(strMsg);
                                return;
                            }
                        }

                    }
                    */

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

        NavBack();

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

                string strOldStatusName = GetReqValue("sys_requeststatus");
                if (GetReqValue("StatusName") != "")
                {
                    strOldStatusName += " - " + GetReqValue("StatusName");
                }

                RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Status Changed", "sys_requeststatus", strOldStatusName, strNewStatusName);
            }
        }

        Response.Redirect("UserTask.aspx?sys_request_id=" + ViewState["sys_request_id"]);
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

                if (ViewState["sys_action_id"].ToString() == "")
                {
                    if (dr["isdefault"].ToString() != "")
                    {
                        dd.SelectedValue = dr["listitem"].ToString();
                    }
                }
            }

            if (ViewState["sys_action_id"].ToString() != "")
            {
                dd.SelectedValue = strList;
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

        bool bMatch = false;
        if (strVal == "")
            bMatch = true;


        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_priority_id"].ToString(), dr["sys_priority_id"].ToString());
                dd.Items.Add(item);

                if (dr["sys_priority_id"].ToString() == strVal)
                    bMatch = true;
            }
            dd.SelectedValue = "";
        }

        if (!bMatch && strVal != "")
        {
            item = new DropDownItem(strVal, strVal);
            dd.Items.Add(item);
        }

        dd.SelectedValue = strVal;
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
    /// Get values for the form field variable
    /// </summary>
    /// <param name="strField">field</param>
    /// <returns>Value</returns>
    public string GetReqValue(string strField)
    {
        //Default Value
        if (dsReqValue == null || dsReqValue.Tables.Count <= 0 || dsReqValue.Tables[0].Rows.Count <= 0)
        {
            return "";
        }



        //Get value from DataSet
        try
        {
            DataRow dr = dsReqValue.Tables[0].Rows[0];
            string strVal = dr[strField].ToString();

            return strVal;
        }
        catch
        {
            return "";
        }

    }

    /// <summary>
    /// Get values for the form field variable
    /// </summary>
    /// <param name="strField">field</param>
    /// <returns>Value</returns>
    public string GetProblemValue(string strField)
    {
        //Default Value
        if (dsProblemValue == null || dsProblemValue.Tables.Count <= 0 || dsProblemValue.Tables[0].Rows.Count <= 0)
        {
            return "";
        }



        //Get value from DataSet
        try
        {
            DataRow dr = dsProblemValue.Tables[0].Rows[0];
            string strVal = dr[strField].ToString();

            return strVal;
        }
        catch
        {
            return "";
        }

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
            switch (strField)
            {
                case "sys_action_id":
                    strVal = "(New " + Application["appactionterm"] + ")";
                    break;
                case "sys_username":
                    strVal = Session["User"].ToString();
                    break;
                case "sys_actiondate":
                    strVal = DateTime.Now.ToString();
                    break;
                case "sys_request_id":
                    strVal = ViewState["sys_request_id"].ToString();
                    break;
                case "sys_problem_id":
                    strVal = ViewState["sys_problem_id"].ToString();
                    break;
                case "sys_change_id":
                    strVal = ViewState["sys_change_id"].ToString();
                    break;
                case "sys_actionstatus":
                    strVal = "0";
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

            if (strField == "sys_action_hdmins")
            {
                if (strVal == "")
                {
                    strVal = RequestDA.GetReqDuration(dr["sys_actiondate"].ToString(), DateTime.Now, listSus).ToString();
                }
            }
            else if (strField == "sys_action_24mins")
            {
                if (strVal == "")
                {
                    strVal = RequestDA.GetReqDuration24Min(dr["sys_actiondate"].ToString(), DateTime.Now).ToString();
                }
            }
            else if (strField == "sys_action_timespent")
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
    private void LoadSus(string strReqId,string strProblemId, string strBegin)
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
            dsProblemValue = ProblemDA.GetProblemFullInfoById(strProblemId);
            strSiteId = GetProblemValue("sys_siteid");
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
        DateTime dtEnd = DateTime.Now.AddMonths(int.Parse(SysDA.GetSettingValue("appescalationlimit",Application)));

        //if same day
        if (DateTime.Compare(dtBegin.Date, dtEnd.Date) == 0)
        {
            if(!IsWorking(dtBegin))
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
        return DateTime.Compare(spA.dtStart,spB.dtStart);
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


    protected void imgCloseReq_Click(object sender, ImageClickEventArgs e)
    {
        CloseRequest(ViewState["sys_request_id"].ToString());
    }
}
