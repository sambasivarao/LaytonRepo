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

public partial class SrchProblem : System.Web.UI.Page
{
    public XmlDocument xmlForm;
    public XmlDocument xmlResSetForm;

    public DataSet ds;

    public Hashtable htCols;
    public Hashtable htColSize;

    public string strBackImg = "";

    public string strNewPK = "";

    public DataSet dsUser;

    public int nReqTypeCtrlCnt = 0;
    public Hashtable htCtrl = null;

    public string[] strReqTypeArray = null;

    public string strReqClass = "";
    public string strReqLinkType = "";

    void Page_Init()
    {
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        if (!IsPostBack)
        {
            if (SysDA.GetSettingValue("apprpclevel", Application) != "true")
            {
                Response.End();
                return;
            }

            dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

            ViewState["searchsql"] = LayUtil.GetQueryString(Request, "searchsql");
            ViewState["suppliedeuser"] = LayUtil.GetQueryString(Request, "suppliedeuser");
            ViewState["suppliedeuserblockauto"] = LayUtil.GetQueryString(Request, "suppliedeuserblockauto");
            ViewState["targetframe"] = LayUtil.GetQueryString(Request, "targetframe");
            ViewState["source"] = LayUtil.GetQueryString(Request, "source");
            ViewState["reqclass"] = LayUtil.GetQueryString(Request, "reqclass");
            ViewState["linktype"] = LayUtil.GetQueryString(Request, "linktype");
            ViewState["element"] = LayUtil.GetQueryString(Request, "element");

            LayUtil.SetFont(this.dialogMsg, Application);
        }
        MyInit();
    }

    /// <summary>
    /// Initialize controls and load values
    /// </summary>
    private void MyInit()
    {
        string strFormXML = FormDesignerDA.LoadSrchForm("problem");
        xmlForm = new XmlDocument();
        if (strFormXML != "")
        {
            xmlForm.LoadXml(strFormXML);
        }

        //Get Current logon user info
        dsUser = UserDA.GetUserInfo(Session["User"].ToString());

        LoadCtrlInfo();

        LoadCtrl();

    }
    private void LoadCtrlInfo()
    {
        nReqTypeCtrlCnt = 0;
        htCtrl = new Hashtable();
        foreach (XmlElement node in xmlForm.DocumentElement.ChildNodes)
        {
            if (node.InnerText == "sys_requesttype_id")
            {
                nReqTypeCtrlCnt++;
            }

            if (htCtrl[node.InnerText] == null)
            {
                htCtrl[node.InnerText] = "true";
            }

        }
    }

    /// <summary>
    /// Load controls according to form xml
    /// </summary>
    private void LoadCtrl()
    {
        Control ctrlRoot = phCtrls;

        ///To save the clientID for each control
        System.Text.StringBuilder sbTxt = new System.Text.StringBuilder();
        sbTxt.Append("var arrCtrlID = new Array();");
        sbTxt.Append("var arrClientID = new Array();");
        sbTxt.Append("var arrCtrlType = new Array();");

        int nLen = 0;

        System.Text.StringBuilder sbDDCss = new System.Text.StringBuilder("<style type=\"text/css\">");
        
        int nReqTypeIndex = 1;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            string strVal = GetValue(child.InnerText);
            if (!IsPostBack)
            {
                string strField = child.InnerText;
                if (strField == "sys_loggedby")
                {
                    sys_loggedbyH.Text = strVal;
                }
                else if (strField == "sys_assignedto")
                {
                    sys_assignedtoH.Text = strVal;
                }
                else if (strField == "sys_assignedtoanalgroup")
                {
                    sys_assignedtoanalgroupH.Text = strVal;
                }
                else if (strField == "sys_requesttype_id")
                {
                    sys_requesttype_idH.Text = strVal;
                }
                else if (strField == "sys_siteid")
                {
                    sys_siteidH.Text = strVal;
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

                if (child.InnerText == "sys_problem_status")
                {
                    /*
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
                     */
                }
                else if (child.InnerText == "sys_problem_24mins" || child.InnerText == "sys_problem_hdmins")
                {
                    /*
                    string strDispFormat = SysDA.GetSettingValue("statsdurationdisplay", Application);
                    ctrl.Text = RequestDA.Mins2Time(strVal, strDispFormat);
                     */
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
                    string strFieldType = DataDesignDA.GetFieldType("problem", child.InnerText);
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
                        if (child.GetAttribute("range") == "1")
                        {
                            Label ctrl = new Label();
                            ctrl.Text = "---";
                            ctrl.ID = "lb" + child.Name;
                            ctrl.Width = new Unit("15px");

                            ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                            ctrl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + 15) + "px";
                            ctrl.Style["Position"] = "absolute";
                            ctrl.Font.Name = child.GetAttribute("font-family");
                            ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            WebDatePicker datePickerTo = new WebDatePicker();

                            datePickerTo.Width = new Unit(strWidth + "px");
                            datePickerTo.ID = "datepicker" + child.Name + "to";
                            datePickerTo.Nullable = true;
                            datePickerTo.DisplayModeFormat = "g";
                            datePickerTo.EditModeFormat = "g";
                            datePickerTo.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

                            datePickerTo.Font.Name = child.GetAttribute("font-family");
                            datePickerTo.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            datePickerTo.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            datePickerTo.Style["Top"] = child.GetAttribute("top") + "px";
                            datePickerTo.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + 45) + "px";
                            datePickerTo.Style["Position"] = "absolute";


                            datePickerTo.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;

                            ctrlRoot.Controls.Add(datePickerTo);
                            ctrlRoot.Controls.Add(ctrl);

                        }
                        else if (child.GetAttribute("range") == "2")
                        {
                            WebDatePicker datePickerTo = new WebDatePicker();

                            datePickerTo.Width = new Unit(strWidth + "px");
                            datePickerTo.ID = "datepicker" + child.Name + "to";
                            datePickerTo.Nullable = true;
                            datePickerTo.DisplayModeFormat = "g";
                            datePickerTo.EditModeFormat = "g";
                            datePickerTo.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

                            datePickerTo.Font.Name = child.GetAttribute("font-family");
                            datePickerTo.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            datePickerTo.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            datePickerTo.Style["Top"] = (int.Parse(child.GetAttribute("top")) + 25) + "px";
                            datePickerTo.Style["Left"] = child.GetAttribute("left") + "px";
                            datePickerTo.Style["Position"] = "absolute";

                            datePickerTo.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;
                        

                            ctrlRoot.Controls.Add(datePickerTo);


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

                //Search Button
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
            }
            else
            {
                if (child.Name.Length >= 4 && child.Name.Substring(0, 4).ToLower() == "sys_")
                {
                    if (child.InnerText == "sys_problemdate" || child.InnerText == "sys_problem_closedate")
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
                        
                        ctrlRoot.Controls.Add(datePicker);

                        if (LayUtil.IsNumeric(child.GetAttribute("tabindex")))
                        {
                            datePicker.TabIndex = short.Parse(child.GetAttribute("tabindex"));
                        }

                        if (child.GetAttribute("tabindex") == "1")
                        {
                            datePicker.Focus();
                        }

                        if (child.GetAttribute("range") == "1")
                        {
                            Label ctrl = new Label();
                            ctrl.Text = "---";
                            ctrl.ID = "lb" + child.Name;
                            ctrl.Width = new Unit("15px");

                            ctrl.Style["Top"] = child.GetAttribute("top") + "px";
                            ctrl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + 15) + "px";
                            ctrl.Style["Position"] = "absolute";
                            ctrl.Font.Name = child.GetAttribute("font-family");
                            ctrl.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            ctrl.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            WebDatePicker datePickerTo = new WebDatePicker();

                            datePickerTo.Width = new Unit(strWidth + "px");
                            datePickerTo.ID = "datepicker" + child.Name + "to";
                            datePickerTo.Nullable = true;
                            datePickerTo.DisplayModeFormat = "g";
                            datePickerTo.EditModeFormat = "g";
                            datePickerTo.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

                            datePickerTo.Font.Name = child.GetAttribute("font-family");
                            datePickerTo.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            datePickerTo.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            datePickerTo.Style["Top"] = child.GetAttribute("top") + "px";
                            datePickerTo.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth) + 45) + "px";
                            datePickerTo.Style["Position"] = "absolute";
                            
                            datePickerTo.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;

                            ctrlRoot.Controls.Add(datePickerTo);
                            ctrlRoot.Controls.Add(ctrl);

                        }
                        else if (child.GetAttribute("range") == "2")
                        {
                            WebDatePicker datePickerTo = new WebDatePicker();

                            datePickerTo.Width = new Unit(strWidth + "px");
                            datePickerTo.ID = "datepicker" + child.Name + "to";
                            datePickerTo.Nullable = true;
                            datePickerTo.DisplayModeFormat = "g";
                            datePickerTo.EditModeFormat = "g";
                            datePickerTo.Buttons.SpinButtonsDisplay = Infragistics.Web.UI.EditorControls.ButtonDisplay.OnRight;

                            datePickerTo.Font.Name = child.GetAttribute("font-family");
                            datePickerTo.Font.Size = new FontUnit(child.GetAttribute("font-size") + "px");
                            datePickerTo.ForeColor = LayUtil.GetColorFrmStr(child.GetAttribute("color"));

                            datePickerTo.Style["Top"] = (int.Parse(child.GetAttribute("top")) + 25) + "px";
                            datePickerTo.Style["Left"] = child.GetAttribute("left") + "px";
                            datePickerTo.Style["Position"] = "absolute";

                            datePickerTo.CalendarAnimation.SlideOpenDirection = SlideDirection.Horizontal;
                        

                            ctrlRoot.Controls.Add(datePickerTo);


                        }

                    }
                    else if (child.InnerText == "sys_requesttype_id")
                    {
                        if (nReqTypeCtrlCnt == 1)
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

                            ctrl.Attributes["Index"] = nReqTypeIndex.ToString();

                            //Set list items
                            SetDDReqType(ctrl, strVal, nReqTypeIndex);
                            nReqTypeIndex++;

                            ctrl.ValueChanged += new DropDownValueChangedEventHandler(ReqTypeSel_ValueChanged);
                            ctrl.AutoPostBack = true;
                            //ctrl.AutoPostBackFlags += new DropDownAutoPostBackFlags(
                            /*
                            if (strVal != "")
                            {
                                ctrl.SelectedValue = strVal;
                            }
                             */
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
                    }
                    else if (child.InnerText == "sys_siteid")
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
                            hl.Style["Left"] = (int.Parse(child.GetAttribute("left")) + int.Parse(strWidth)+ LayUtil.PixelSpace + LayUtil.SmallIconSize + LayUtil.PixelSpace).ToString() + "px";
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
                    else if (child.InnerText == "sys_loggedby")
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

                        if (!UserReadOnly(child))
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
                    else if (child.InnerText == "sys_priority")
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
                        SetDDImpact(ctrl, strVal);

                        ctrl.SelectedItemIndex = 0;

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
                        SetDDUrgency(ctrl, strVal);

                        ctrl.SelectedItemIndex = 0;

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
                        SetDDITService(ctrl, strVal);

                        ctrl.SelectedItemIndex = 0;

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
                    else if (child.InnerText == "sys_problem_status")
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
                        item = new DropDownItem("", "");
                        ctrl.Items.Add(item);
                        item = new DropDownItem("(All Open statuses)", "-100");
                        ctrl.Items.Add(item);
                        //item = new DropDownItem("Closed", "0");
                        //ctrl.Items.Add(item);
                        DataSet dsStatus = LibProblemStatusDA.GetStatusList("");
                        if (dsStatus != null && dsStatus.Tables.Count > 0)
                        {
                            foreach (DataRow dr in dsStatus.Tables[0].Rows)
                            {
                                item = new DropDownItem(dr["sys_problem_status"].ToString(), dr["sys_problem_status_id"].ToString());
                                ctrl.Items.Add(item);
                            }
                        }
                        ctrl.SelectedItemIndex = 0;


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
                            ctrl.Content = strVal;

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

        sbTxt.Append("var ctrlno=");
        sbTxt.Append(nLen.ToString());
        sbTxt.Append(";");

        txtArray.Text = sbTxt.ToString();

        sbDDCss.AppendLine("</style>");

        StyleArea.InnerHtml = sbDDCss.ToString();

    }

    private SqlCommand GetSrchCmd()
    {
        ///Build query sql first
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "";
        cmd.CommandType = CommandType.Text;

        SqlParameter parameter;
        string strSql = "";
        if (htCols == null || htColSize == null)
        {
            //Initialize columns hashtable
            DataSet dsCols = DataDesignDA.GetTblCol("problem");

            if (dsCols == null || dsCols.Tables.Count <= 0)
            {
                ShowMsg("Cannot load 'problem' table");
                return null;
            }

            htCols = new Hashtable();
            htColSize = new Hashtable();

            foreach (DataRow dr in dsCols.Tables[0].Rows)
            {
                htCols[dr["ColName"].ToString()] = dr["ColType"].ToString();
                htColSize[dr["ColName"].ToString()] = dr["ColSize"].ToString();
            }
        }

        //strSql = "SELECT *, (SELECT Sys_Status FROM [status] WHERE sys_status_id = sys_requeststatus) As Status, (SELECT Sys_StatusForceColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusForceColor, (SELECT Sys_StatusColor FROM status WHERE sys_status_id = sys_requeststatus) As StatusColor, (SELECT Count(sys_action_id) FROM [action] WHERE action.sys_request_id = request.sys_request_id) As [ActionCount], (SELECT Count(sys_comment_id) FROM [comments] WHERE comments.sys_request_id = request.sys_request_id) As [CommentCount], (SELECT count(comments.sys_comment_id) FROM comments INNER JOIN commentsviewed ON comments.sys_comment_id = commentsviewed.sys_comment_id WHERE commentsviewed.sys_username = @strUser AND comments.sys_request_id = request.sys_request_id) As [ReadCommentCount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype = 1) AS [spawncount], (SELECT count(sys_request_id) FROM request AS [request2] WHERE request2.sys_requestparent_id = request.sys_request_id AND request2.sys_requestlinktype is null) AS [linkcount] FROM request";
        strSql = "";

        /*
        parameter = new SqlParameter();
        parameter.ParameterName = "@strUser";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = Session["User"].ToString();
        cmd.Parameters.Add(parameter);
        */

        bool bReqType = false;

        bool bFirst = true;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
            if (htCols[child.InnerText] == null)
                continue;

            string strVal = GetUserInput(child);

            if (child.InnerText == "sys_requesttype_id")
            {
                if (bReqType)
                {
                    continue;
                }
                bReqType = true;
            }
            else if (child.InnerText == "sys_problem_status")
            {
                if (strVal == "")
                    continue;

                if (bFirst)
                {
                    strSql += " WHERE ";
                    bFirst = false;
                }
                else
                {
                    strSql += " AND ";
                }

                if (strVal == "-100")
                {
                    strSql += "(sys_problem_status IN (SELECT sys_problem_status_id FROM problemstatus WHERE sys_problem_suspend<>2))";
                }
                else
                {
                    strSql += " sys_problem_status=@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strVal;
                    cmd.Parameters.Add(parameter);
                }

                continue;
            }
            else if (child.InnerText == "sys_itservice")
            {
                if (strVal == "")
                {
                    continue;
                }

                if (bFirst)
                {
                    strSql += " WHERE ";
                    bFirst = false;
                }
                else
                {
                    strSql += " AND ";
                }

                strSql += " (@"+child.Name+" in (SELECT [sys_itservice_id] FROM itserviceitem WHERE itserviceitem.sys_problem_id=problem.sys_problem_id))";

                parameter = new SqlParameter();
                parameter.ParameterName = "@" + child.Name;
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);

                continue;
            }

            else if (child.InnerText == "sys_assetsaffected")
            {
                if (strVal == "")
                {
                    continue;
                }

                if (bFirst)
                {
                    strSql += " WHERE ";
                    bFirst = false;
                }
                else
                {
                    strSql += " AND ";
                }

                strSql += " (@" + child.Name + " IN (SELECT sys_asset_id FROM assetsaffected WHERE assetsaffected.sys_problem_id=problem.sys_problem_id))";

                parameter = new SqlParameter();
                parameter.ParameterName = "@" + child.Name;
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = strVal;
                cmd.Parameters.Add(parameter);

                continue;
            }

            if ((htCols[child.InnerText].ToString() == "DateTime"))
            {
                if (child.GetAttribute("range") == "1" || child.GetAttribute("range") == "2")
                {
                    strVal = GetUserInput(child);
                    string strValTo = GetUserInputRangeTo(child);
                    if (strVal == "" && strValTo == "")
                    {
                        continue;
                    }

                    if (bFirst)
                    {
                        strSql += " WHERE ";
                        bFirst = false;
                    }
                    else
                    {
                        strSql += " AND ";
                    }

                    if (strVal != "" && strValTo == "")
                    {
                        strSql += child.InnerText + ">=@" + child.Name;

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@" + child.Name;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strVal);
                        cmd.Parameters.Add(parameter);
                    }
                    else if (strVal == "" && strValTo != "")
                    {
                        strSql += child.InnerText + "<=@" + child.Name;

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@" + child.Name;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strValTo);
                        cmd.Parameters.Add(parameter);
                    }
                    else if (strVal != "" && strValTo != "")
                    {
                        strSql += child.InnerText + " BETWEEN @" + child.Name + " AND @" + child.Name + "to";

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@" + child.Name;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strVal);
                        cmd.Parameters.Add(parameter);

                        parameter = new SqlParameter();
                        parameter.ParameterName = "@" + child.Name + "to";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = DateTime.Parse(strValTo);
                        cmd.Parameters.Add(parameter);
                    }
                }
                else
                {
                    strVal = GetUserInput(child);
                    if (strVal == "")
                    {
                        continue;
                    }

                    if (bFirst)
                    {
                        strSql += " WHERE ";
                        bFirst = false;
                    }
                    else
                    {
                        strSql += " AND ";
                    }

                    DateTime dtVal = DateTime.Parse(strVal);

                    strSql += child.InnerText + " BETWEEN @" + child.Name + " AND @" + child.Name + "to";

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = DateTime.Parse(dtVal.Date.ToString());
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name + "to";
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = DateTime.Parse(dtVal.ToShortDateString() + " 23:59:59");
                    cmd.Parameters.Add(parameter);
                }
            }
            else
            {
                strVal = GetUserInput(child);
                if (strVal == "")
                {
                    continue;
                }

                if (htCols[child.InnerText] == null)
                    continue;

                if (bFirst)
                {
                    strSql += " WHERE ";
                    bFirst = false;
                }
                else
                {
                    strSql += " AND ";
                }

                if (child.InnerText == "sys_loggedby" || child.InnerText == "sys_requesttype_id"
                    || child.InnerText == "sys_assignedto" || child.InnerText == "sys_assignedtoanalgroup" || child.InnerText == "sys_priority"
                    || child.InnerText == "sys_siteid")
                {
                    strSql += child.InnerText + "=@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strVal;
                    cmd.Parameters.Add(parameter);
                }
                else if (htCols[child.InnerText].ToString() == "Text")
                {
                    strSql += child.InnerText + " like @" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    if (strVal.Length > 2)
                        parameter.Value = "%" + strVal + "%";
                    else
                        parameter.Value = strVal + "%";

                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    strSql += child.InnerText + "=@" + child.Name;

                    parameter = new SqlParameter();
                    parameter.ParameterName = "@" + child.Name;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = strVal;
                    cmd.Parameters.Add(parameter);
                }
            }


        }

        cmd.CommandText = strSql;

        return cmd;
    }

    void dg_InitializeRow(object sender, RowEventArgs e)
    {
        WebDataGrid dg = (WebDataGrid)sender;

        string strPageSize = LayUtil.GetAttribute(xmlResSetForm.DocumentElement.GetElementsByTagName("resultset")[0], "maxrows");
        if (LayUtil.IsNumeric(strPageSize))
        {
            dg.Behaviors.Paging.PageSize = int.Parse(strPageSize);
        }
        else
            dg.Behaviors.Paging.PageSize = LayUtil.ResSetDefPageSize;

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

    private void ShowMsg(string strMsg)
    {
        lbMsg.Text = strMsg;
        dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
        dialogMsg.MaintainLocationOnScroll = false;
        dialogMsg.InitialLocation = Infragistics.Web.UI.LayoutControls.DialogWindowPosition.Centered;
    }

    private string GetUserInputRangeTo(XmlElement child)
    {
        string strVal = "";
        Control ctrlRoot = phCtrls;

        WebDatePicker datePicker = (WebDatePicker)ctrlRoot.FindControl("datepicker" + child.Name + "to");

        if (datePicker != null)
        {
            strVal = datePicker.Text;
        } 
        
        return strVal;
    }

    private string GetUserInput(XmlElement child)
    {
        string strField = child.InnerText;
        if (strField == "sys_loggedby")
        {
            return sys_loggedbyH.Text;
        }
        else if (strField == "sys_siteid")
        {
            return sys_siteidH.Text;
        }
        else if (strField == "sys_requesttype_id")
        {
            return sys_requesttype_idH.Text;
        }
        else if (strField == "sys_assignedto")
        {
            return sys_assignedtoH.Text;
        }
        else if (strField == "sys_assignedtoanalgroup")
        {
            return sys_assignedtoanalgroupH.Text;
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
                else if (DataDesignDA.GetFieldType("problem", child.InnerText) == "DateTime")
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
                if (child.InnerText == "sys_priority" || child.InnerText == "sys_problem_status" || child.InnerText == "sys_urgency" || child.InnerText == "sys_impact" || child.InnerText == "sys_itservice")
                {
                    WebDropDown ctrl = (WebDropDown)ctrlRoot.FindControl("dd" + child.Name);
                    if (ctrl != null)
                    {
                        strVal = ctrl.SelectedValue;
                    }
                }
                else if (child.InnerText == "sys_problemdate" || child.InnerText == "sys_problem_closedate")
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

        /*
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
        */
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
                ctrl.CurrentValue = "";
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
            ctrl.Items.Clear();
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

        /*
        string strSelVal = "";
        for (int i = 0; i < nDepth; i++)
        {
            if (i > 0)
            {
                strSelVal += "/";
            }
            strSelVal += strReqTypeArray[i];
        }
         * */
        //if (!IsPostBack)
        {
            //dd.SelectedValue = strSelVal;// strReqTypeArray[nDepth - 1];
            //dd.SelectedValue = strReqTypeArray[nDepth - 1];
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

        item = new DropDownItem("(None)", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_priority_id"].ToString(), dr["sys_priority_id"].ToString());
                dd.Items.Add(item);
            }
            if (dd.Items.Count > 0)
            {
                dd.SelectedItemIndex = 0;
            }
        }
    }

    /// <summary>
    /// Check user inputs against db schema
    /// </summary>
    /// <returns></returns>
    private bool CheckUserInput()
    {
        string strTbl = "problem";

        if (htCols == null || htColSize == null)
        {
            //Initialize columns hashtable
            DataSet dsCols = DataDesignDA.GetTblCol(strTbl);

            if (dsCols == null || dsCols.Tables.Count <= 0)
            {
                ShowMsg("Cannot load 'problem' table");
                return false;
            }

            htCols = new Hashtable();
            htColSize = new Hashtable();

            foreach (DataRow dr in dsCols.Tables[0].Rows)
            {
                htCols.Add(dr["ColName"].ToString(), dr["ColType"].ToString());
                htColSize.Add(dr["ColName"].ToString(), dr["ColSize"].ToString());
            }
        }

        //bool bError = false;
        foreach (XmlElement child in xmlForm.DocumentElement.ChildNodes)
        {
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

            string strVal = GetUserInput(child);
            string strType = htCols[child.InnerText].ToString();
            if (strType == "Integer")
            {
                if (strVal != "")
                {
                    //if (child.Name != "sys_field1")
                    {
                        if (!LayUtil.IsNumeric(strVal))
                        {
                            ShowMsg("You must enter a numeric value in the " + child.GetAttribute("caption") + " box.");
                            return false;
                        }
                        else
                        {
                            /*
                            int nVal = int.Parse(strVal);
                            if (nVal > 32767 || nVal < -32768)
                            {
                                ShowMsg("You must enter a numeric value between -32,768 and 32,767 in the " + child.GetAttribute("caption") + " box.");
                                return false;
                            }
                             */ 
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
                if (strSize != "" && LayUtil.IsNumeric(strSize))
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
    /// Save Button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void sys_button1_Click(object sender, ImageClickEventArgs e)
    {
        if (!CheckUserInput())
            return;

        SqlCommand cmd = GetSrchCmd();
        Session["ProblemSrchCmd"] = cmd;

        //HttpCookie cookie = new HttpCookie("TabIndex", "-1");
        //cookie.Expires = DateTime.Today.AddMonths(1);
        //Response.Cookies.Add(cookie);
        //Request.Cookies["TabIndex"].Value = "-1";

        Response.Redirect("UserProblem.aspx?searchsql=true");
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

            }

            dd.SelectedValue = "";

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
    /// Get values for the form field variable
    /// </summary>
    /// <param name="strField">field</param>
    /// <returns>Value</returns>
    public string GetValue(string strField)
    {
        //Default Value
        if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
        {
            string strVal = "";

            return strVal;
        }

        //Get value from DataSet
        try
        {
            return ds.Tables[0].Rows[0][strField].ToString();
        }
        catch
        {
            return "";
        }

    }
    private void SetDDImpact(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibImpactDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_impact"].ToString(), dr["sys_impact_id"].ToString());
                dd.Items.Add(item);
            }
        }
    }

    private void SetDDUrgency(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibUrgencyDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_urgency"].ToString(), dr["sys_urgency_id"].ToString());
                dd.Items.Add(item);
            }
        }
    }
    private void SetDDITService(WebDropDown dd, string strVal)
    {
        DataSet dsItem = LibITServiceDA.GetItemInfo("");

        DropDownItem item;
        item = new DropDownItem("", "");
        dd.Items.Add(item);
        if (dsItem != null && dsItem.Tables.Count > 0)
        {
            foreach (DataRow dr in dsItem.Tables[0].Rows)
            {
                item = new DropDownItem(dr["sys_itservice"].ToString(), dr["sys_itservice_id"].ToString());
                dd.Items.Add(item);
            }
        }
    }
}
