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

/// <summary>
/// Change request status
/// </summary>

public partial class EReqChgStatus : System.Web.UI.Page
{
    public XmlDocument xmlForm;

    public DataSet dsReqValue;

    public string strBackImg = "";

    public string strNewPK = "";

    public DataSet dsEUser;

    private List<SusPeriod> listSus = null;
    private XmlDocument xmlHours = null;
    private Hashtable htOpenHr = null;
    private Hashtable htCloseHr = null;

    public Hashtable htOpenStatus;
    public Hashtable htStatus;

    public string strClientIDs;

    void Page_Init()
    {
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginEUser(Session, Response);
        if (!IsPostBack)
        {
            ViewState["sys_request_id"] = LayUtil.GetQueryString(Request, "sys_request_id");

            dialogMsg.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogReqStatus.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogReqSuspension.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
            dialogSurvey.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

            lbTTL.Text = "Change " + Application["apprequestterm"] + " Status";
            lbSurveyMsg.Text = "Do you wish to send a  " + Application["appsurveyterm"] + " to the " + Application["appenduserterm"] + " for the " + Application["apprequestterm"] + " " + ViewState["sys_request_id"];

            //Set Status DropDown
            string strStatus = GetReqValue("sys_requeststatus");

            DropDownItem item;
            DataSet dsStatus;
            if (SysDA.GetSettingValue("appclosebyowner", Application) == "false" && SysDA.GetSettingValue("appallowenduserclose", Application) == "true")
            {
                dsStatus = LibReqStatusDA.GetEUserStatus();
            }
            else
            {
                dsStatus = LibReqStatusDA.GetEUserOpenStatus();
            }

            if (dsStatus != null && dsStatus.Tables.Count > 0)
            {
                foreach (DataRow dr in dsStatus.Tables[0].Rows)
                {
                    if (dr["sys_status_ID"].ToString() != strStatus)
                    {
                        item = new DropDownItem(dr["sys_status"].ToString(), dr["sys_status_ID"].ToString());
                        ddStatus.Items.Add(item);

                    }
                }

                ddStatus.SelectedItemIndex = 0;
            }

            /*
            if (SysDA.GetSettingValue("appclosebyowner", Application) == "false" && SysDA.GetSettingValue("appallowenduserclose", Application) == "true")
            {
                if (strStatus != "0")
                {
                    item = new DropDownItem("Close", "0");
                    ddStatus.Items.Add(item);
                    ddStatus.SelectedValue = "0";
                }
            }
            */
            LayUtil.SetFont(this.form1, Application);
        }

        MyInit();
    }

    /// <summary>
    /// Initialize controls and load values
    /// </summary>
    private void MyInit()
    {

        //Load Form
        string strReqId = "";

        if (ViewState["sys_request_id"] != null)
        {
            strReqId = ViewState["sys_request_id"].ToString();
        }

        //Get Current logon user info
        dsEUser = UserDA.GetEUserInfo(Session["User"].ToString());

        //Get Request values
        //dsReqValue = RequestDA.GetReqFullInfoById(ViewState["sys_request_id"].ToString());

        //Set Status DropDown
        string strStatus = GetReqValue("sys_requeststatus");
        htStatus = new Hashtable();

        DataSet dsStatus = LibReqStatusDA.GetReqStatusList("");
        if (dsStatus != null && dsStatus.Tables.Count > 0)
        {
            foreach (DataRow dr in dsStatus.Tables[0].Rows)
            {
                htStatus.Add(dr["sys_status_ID"].ToString(), dr["sys_status"].ToString());

            }
        }
        //htStatus.Add("0", "Closed");
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


    /// <summary>
    /// close request when all tasks finish
    /// </summary>
    /// <param name="strReqId"></param>
    private void CloseRequest(string strReqId)
    {
        string strReqSpwnClose = SysDA.GetSettingValue("requestspawnclose", Application);

        DataSet dsReq2Close = RequestDA.GetAllReq2Close(strReqId, false);

        if (dsReq2Close == null && dsReq2Close.Tables.Count <= 0)
            return;

        foreach (DataRow drReq in dsReq2Close.Tables[0].Rows)
        {
            string strCloseOnSchedule = "0";
            if (drReq["sys_resolve"].ToString() != "")
            {
                DateTime dtRes = DateTime.Parse(drReq["sys_resolve"].ToString());
                if (DateTime.Compare(dtRes, DateTime.Now) >= 0)
                {
                    strCloseOnSchedule = "1";
                }
                else
                {
                    strCloseOnSchedule = "-1";
                }
            }

            LoadSus(drReq["sys_request_id"].ToString(), drReq["sys_siteid"].ToString(), drReq["sys_requestdate"].ToString());

            string strDurHD = ((int)(RequestDA.GetReqDuration(drReq["sys_requestdate"].ToString(), DateTime.Now, listSus))).ToString();
            string strDur24 = ((int)(RequestDA.GetReqDuration24Min(drReq["sys_requestdate"].ToString(), DateTime.Now))).ToString();

            string strReqStatus = drReq["sys_requeststatus"].ToString();
            if (IsStatusOpen(strReqStatus))
            {
                RequestDA.UpdReqComplete(drReq["sys_request_id"].ToString(), ddStatus.SelectedValue, DateTime.Now, strDurHD, strDur24, strCloseOnSchedule);

                ///Update Audit
                //Get Closed status name
                string strNewStatus = ddStatus.SelectedValue;
                string strNewStatusName = strNewStatus + " - " + htStatus[strNewStatus];

                string strOldStatus = drReq["sys_requeststatus"].ToString();
                string strOldStatusName = strOldStatus + " - " + htStatus[strOldStatus];

                RequestDA.InsReqAudit(drReq["sys_request_id"].ToString(), DateTime.Now, Session["User"].ToString(), "Status Changed", "sys_requeststatus", strOldStatusName, strNewStatusName);

            }
        }
    }
    public string GetEUserVal(string strField)
    {
        if (dsEUser == null || dsEUser.Tables.Count <= 0 || dsEUser.Tables[0].Rows.Count <= 0)
        {
            return "";
        }

        //Get value from DataSet
        try
        {
            return dsEUser.Tables[0].Rows[0][strField].ToString();
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
        if (dsReqValue == null)
        {
            dsReqValue = RequestDA.GetReqFullInfoById(ViewState["sys_request_id"].ToString());
        }

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
    private void LoadSus(string strReqId, string strSiteId, string strBegin)
    {
        if (strBegin == "")
            return;

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
        DataSet dsSus = UserDA.GetSuspension(strReqId, strSiteId, strCompanyId, dtBegin, DateTime.Now);
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

    private void ShowSusFrom(bool bShow)
    {
        if (bShow)
        {
            lbSusFrom.Visible = true;
            datePicker.Visible = true;
            datePicker.Value = DateTime.Now;
        }
        else
        {
            lbSusFrom.Visible = false;
            datePicker.Visible = false;
        }
    }

    protected void imgSaveStatus_Click(object sender, ImageClickEventArgs e)
    {
        string strMsg = "";
        string strReqId = ViewState["sys_request_id"].ToString();
        string strNewStatus = ddStatus.SelectedValue;
        string strOldStatus = GetReqValue("sys_requeststatus");

        if (strNewStatus == "")
        {
            strMsg = "No status available.";
            return;
        }

        if (strNewStatus == strOldStatus)
        {
            NavigateBack();
            return;
        }

        ShowSusFrom(false);

        if (IsStatusOpen(strOldStatus) && !IsStatusOpen(strNewStatus))
        {
            //Check For Incomplete Tasks Against Requests
            if (SysDA.GetSettingValue("apprequestcloseactionoption", Application) != "nowarn")
            {
                DataSet dsReqOpenAction = RequestDA.GetReqOpenActions(strReqId);
                if (dsReqOpenAction != null && dsReqOpenAction.Tables.Count > 0 && dsReqOpenAction.Tables[0].Rows.Count > 0)
                {
                    if (SysDA.GetSettingValue("apprequestcloseactionoption", Application) == "warn")
                    {
                        strMsg = "This " + Application["apprequestterm"] + " contains incomplete tasks. Do you wish to close anyway?";
                        lbStatusReqMsg.Text = strMsg;
                        dialogReqStatus.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                        return;
                    }
                    else if (SysDA.GetSettingValue("apprequestcloseactionoption", Application) == "prevent")
                    {
                        strMsg = "You cannot close this " + Application["apprequestterm"] + " as it contains incomplete " + Application["appactionterm"] + ".";
                        ShowMsg(strMsg);
                        return;
                    }
                }

            }

            string strReqSpwnClose = SysDA.GetSettingValue("requestspawnclose", Application);
            string strReqSpwnCnt = GetReqValue("spawncount");
            //if (strReqSpwnClose == "")
            {
                if (strReqSpwnCnt != "0")
                {
                    strMsg = "You cannot close this " + Application["apprequestterm"] + " as it contains spawned " + Application["apprequestterm"] + "s that are not yet closed.";
                    ShowMsg(strMsg);
                    return;
                }
            }

        }
        else
        {
            if (LayUtil.IsNumeric(strNewStatus))
            {
                DataSet dsStatus = LibReqStatusDA.GetReqStatusById(strNewStatus);
                if (dsStatus != null && dsStatus.Tables.Count > 0 && dsStatus.Tables[0].Rows.Count > 0)
                {
                    string strSuspend = dsStatus.Tables[0].Rows[0]["sys_suspend"].ToString();
                    if (strSuspend == "1")
                    {
                        strMsg = "The status you selected will suspend the " + Application["apprequestterm"] + ". Are you sure you wish to do this?";
                        lbSuspension.Text = strMsg;
                        ShowSusFrom(false);
                        dialogReqSuspension.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
                        return;
                    }
                }

            }
        }

        if (strMsg == "")
        {
            if (IsStatusOpen(strOldStatus) && !IsStatusOpen(strNewStatus))
            {
                CloseRequest(strReqId);
            }
            else
            {
                ChangeReqStatus(DateTime.Now.ToString());
            }
        }

        //NavigateBack(); // commented by Sparsh

        // Added by Sparsh ID 184
        Response.Redirect("EUserHome.aspx");
        // End by Sparsh ID 184
    }

    private bool ChangeReqStatus(string strSusTime)
    {
        if (strSusTime == "")
            return false;

        string strReqId = ViewState["sys_request_id"].ToString();
        string strNewStatus = ddStatus.SelectedValue;
        DataSet dsStatus = LibReqStatusDA.GetReqStatusById(strNewStatus);
        if (dsStatus != null && dsStatus.Tables.Count > 0 && dsStatus.Tables[0].Rows.Count > 0)
        {
            string strReqPriority = GetReqValue("sys_requestpriority");
            string strSuspend = dsStatus.Tables[0].Rows[0]["sys_suspend"].ToString();
            if (strSuspend == "1")
            {
                DateTime dtFrom = DateTime.Parse(strSusTime);
                DateTime dtReqDate = DateTime.Parse(GetReqValue("sys_requestdate"));
                if (DateTime.Compare(dtFrom, DateTime.Now) > 0)
                {
                    dtFrom = DateTime.Now;
                }

                if (DateTime.Compare(dtFrom, dtReqDate) < 0)
                {
                    dtFrom = dtReqDate;
                }

                if (!RequestDA.CheckExistReqSus(strReqId))
                {
                    RequestDA.ReqInsertSus(strReqId, dtFrom.ToString(), "");
                }

                if (strReqPriority != "")
                {
                    //update request record to clear escalation points not yet reached
                    RequestDA.UpdReqResolve(strReqId, dtFrom);
                    RequestDA.UpdReqEsc1(strReqId, dtFrom);
                    RequestDA.UpdReqEsc2(strReqId, dtFrom);
                    RequestDA.UpdReqEsc3(strReqId, dtFrom);

                }
            }
            else if (strSuspend == "0")
            {
                if (RequestDA.CheckExistReqSus(strReqId))
                {
                    RequestDA.CompleteReqSuspention(strReqId, DateTime.Now, Session["User"].ToString());
                }
                if (strReqPriority != "")
                {
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    double dReqReslvHr = 0;
                    double dReqEsc1Hr = 0;
                    double dReqEsc2Hr = 0;
                    double dReqEsc3Hr = 0;

                    DataSet dsPri = LibPriorityDA.GetPriorityInfo(strReqPriority);

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

                    string strBegin = GetReqValue("sys_requestdate");
                    string strSiteId = GetReqValue("sys_siteid");
                    LoadSus(GetReqValue("sys_request_id"), strSiteId, strBegin);

                    TimeSpan ts;
                    string strReslvTime = "";
                    if (dReqReslvHr != 0)
                    {
                        ts = new TimeSpan(0, (int)(dReqReslvHr * 60), 0);
                        strReslvTime = RequestDA.GetDurationTime(strBegin, ts, listSus);
                    }

                    string strEsc1Time = "";
                    if (dReqEsc1Hr != 0)
                    {
                        ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                        strEsc1Time = RequestDA.GetDurationTime(strBegin, ts, listSus);
                    }

                    string strEsc2Time = "";
                    if (dReqEsc1Hr != 0)
                    {
                        ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                        strEsc2Time = RequestDA.GetDurationTime(strBegin, ts, listSus);
                    }
                    string strEsc3Time = "";
                    if (dReqEsc1Hr != 0)
                    {
                        ts = new TimeSpan(0, (int)(dReqEsc1Hr * 60), 0);
                        strEsc3Time = RequestDA.GetDurationTime(strBegin, ts, listSus);
                    }

                    RequestDA.UpdReqEscEmail(strReqId, strReslvTime, strEsc1Time, strEsc2Time, strEsc3Time);
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
            }
        }


        ////reopen
        string strOldStatus = GetReqValue("sys_requeststatus");
        if (!IsStatusOpen(strOldStatus) && IsStatusOpen(strNewStatus))
        {
            RequestDA.ReOpenReq(strReqId, strNewStatus);
        }
        else
        {
            RequestDA.UpdReqStatus(strReqId, strNewStatus);
        }


        ///Insert into audit
        string strNewStatusName = "";
        strNewStatusName = strNewStatus + " - " + htStatus[strNewStatus];

        string strOldStatusName = "";
        strOldStatusName = strOldStatus + " - " + htStatus[strOldStatus];

        RequestDA.InsReqAudit(strReqId, DateTime.Now, Session["User"].ToString(), "Status Changed", "sys_requeststatus", strOldStatusName, strNewStatusName);

        return true;
    }

    private void NavigateBack()
    {
        Response.Redirect(Session["ButtonBack"].ToString());
    }

    protected void imgCloseReq_Click(object sender, ImageClickEventArgs e)
    {
        CloseRequest(ViewState["sys_request_id"].ToString());
        NavigateBack();
    }
    protected void imgChgStatus_Click(object sender, ImageClickEventArgs e)
    {
        ChangeReqStatus(DateTime.Now.ToString());

        NavigateBack();
    }
    protected void imgSurvey_Click(object sender, ImageClickEventArgs e)
    {
        string strReqId = ViewState["sys_request_id"].ToString();
        bool bSurveyBlked = false;
        if (GetReqValue("sys_blocksurvey") == "1")
        {
            bSurveyBlked = true;
        }

        DataSet dsReqEUser = RequestDA.GetReqWithEUser(strReqId);
        if (dsReqEUser != null && dsReqEUser.Tables.Count > 0 && dsReqEUser.Tables[0].Rows.Count > 0)
        {
            if (dsReqEUser.Tables[0].Rows[0]["sys_blocksurvey"].ToString() == "1")
            {
                bSurveyBlked = true;
            }
        }

        if (!bSurveyBlked)
        {
            if (RequestDA.InsSurvey(DateTime.Now, strReqId))
            {
                if (SysDA.GetSettingValue("appemailendusersurveyflag", Application) == "true")
                {
                    string strEUser = GetReqValue("sys_eusername");
                    if (strEUser != "")
                    {
                        DataSet dsEUser = UserDA.GetEUserInfo(strEUser);
                        if (dsEUser != null && dsEUser.Tables.Count > 0 && dsEUser.Tables[0].Rows.Count > 0)
                        {
                            string strEmail = dsEUser.Tables[0].Rows[0]["sys_email"].ToString();
                            if (strEmail != "")
                            {
                                string strSurveyId = RequestDA.GetMaxSurveyId();
                                if (strSurveyId != "")
                                {
                                    LayUtil.SendOutEmail(Application, "survey", strEmail, "", strEUser, "request", strReqId, GetReqValue("sys_requestclass_id"), RequestDA.GetReqSurveyMailCmd(strSurveyId));
                                }
                            }
                        }
                    }
                }
            }
        }

        dialogSurvey.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

        NavigateBack();
    }

    private bool IsStatusOpen(string strStatus)
    {
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

        if (htOpenStatus != null && htOpenStatus[strStatus] != null)
        {
            return true;
        }
        else
            return false;
    }
}
