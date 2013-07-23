using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;

using Infragistics.Web.UI.ListControls;
using Infragistics.Web.UI.GridControls;

public partial class Admin_SysWhiteBoard : System.Web.UI.Page
{
    public DataSet ds;
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        UpdateList();
        if (!IsPostBack)
        {
            InitMyCtrl();
        }
    }

    private void InitMyCtrl()
    {
        lbAuthUser.Text = Application["appuserterm"] + " Author";

        DropDownItem item;
        item = new DropDownItem(Application["appuserterm"] + "s", "0");
        ddDispTo.Items.Add(item);
        item = new DropDownItem(Application["appenduserterm"] + "s and " + Application["appuserterm"]+"s", "1");
        ddDispTo.Items.Add(item);
        ddDispTo.SelectedValue = "0";

        lbUserGrp.Text = Application["appuserterm"] + " Group";
        lbSite.Text = Application["appsiteterm"].ToString();
        lbDept.Text = Application["appeclientterm"].ToString();

        dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogInfo.Header.CaptionText = "Edit Impact";
        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogHidden.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;

        LayUtil.SetFont(this.form1, Application);

    }

    private void UpdateList()
    {
        dg.ClearDataSource();

        if (Session["Role"].ToString() != "administrator")
        {
            //Written by Sparhs ID 178
            ds = SysDA.GetWhiteBoard(Session["User"].ToString());
            //End by Sparsh ID 178
        }
        else
        {
            //Written by Sparsh ID 178
            ds = SysDA.GetWhiteBoard("");
            //End by Sparsh ID 178
        }

        if (ds == null || ds.Tables.Count <= 0)
            return;

        dg.DataSource = ds;

        dg.DataBind();
        LayUtil.SetFont(this.form1, Application);
    }

    protected void btnSite_Clicked(object sender, ImageClickEventArgs e)
    {
    }


    protected void btnEditPre_Clicked(object sender, EventArgs e)
    {
        LinkButton btnEdit = (LinkButton)sender;
        string strId = btnEdit.CommandArgument;
        textNameH.Text = strId;
        
        lbMSG.Text = "";

        DataSet dsItem = SysDA.GetWBInfo(strId);

        if (dsItem == null || dsItem.Tables.Count == 0 || dsItem.Tables[0].Rows.Count == 0)
            return;

        DataRow dr = dsItem.Tables[0].Rows[0];

        string strStart = dr["sys_whiteboarddate"].ToString();
        if (strStart != "")
        {
            dateStart.Value = DateTime.Parse(strStart);
        }
        else
        {
            dateStart.Text = "";
        }
        string strEnd = dr["sys_whiteboarddateend"].ToString();
        if (strEnd != "")
        {
            dateEnd.Value = DateTime.Parse(strEnd);
        }
        else
        {
            dateEnd.Text = "";
        }

        textMessage.Text = dr["sys_whiteboardsubject"].ToString();
        textAuthUser.Text = dr["sys_whiteboarduser"].ToString();
        textAnalGrp.Text = dr["sys_analgroup"].ToString();
        sys_assignedtoanalgroupH.Text = textAnalGrp.Text;
        textSite.Text = dr["sys_siteid"].ToString();
        sys_siteidH.Text = textSite.Text;
        textDept.Text = dr["sys_eclient_id"].ToString();
        sys_eclient_idH.Text = textDept.Text;
        
        ddDispTo.SelectedValue = dr["sys_visible_euser"].ToString();
        if (ddDispTo.SelectedValue == "")
            ddDispTo.SelectedValue = "0";


        
        textOP.Text = "Update";

        dialogInfo.Header.CaptionText = "Edit Message";
        dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
    }

    public string GetPriPub(object objValue)
    {
        string strValue = objValue.ToString();

        if(strValue == "1")
            return "Public";

        return "Private";
    }
    protected void btnUpdate_Click(object sender, ImageClickEventArgs e)
    {
        string strStart = dateStart.Text;
        string strEnd = dateEnd.Text ;
        if (strStart != "" && strEnd != "")
        {
            DateTime start = DateTime.Parse(strStart);
            DateTime end = DateTime.Parse(strEnd);
            if (DateTime.Compare(start, end) > 0)
            {
                lbMSG.Text = "You must sepcify a begin point before the end point.";
                return;
            }
        }
        
        string strOp = textOP.Text;

        if (strOp == "Add")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "INSERT INTO [whiteboard](sys_whiteboarddate,sys_whiteboarddateend,sys_whiteboardsubject,sys_whiteboarduser,sys_visible_euser,sys_siteid,sys_eclient_id,sys_analgroup) VALUES(@dateStart,@dateEnd,@strMSG,@strUser,@strDispTo,@strSite,@strDept,@strAnalGrp)";
            cmd.CommandType = CommandType.Text;

            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = "@dateStart";
            parameter.Direction = ParameterDirection.Input;
            if (strStart == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = DateTime.Parse(strStart);
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@dateEnd";
            parameter.Direction = ParameterDirection.Input;
            if (strEnd == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = DateTime.Parse(strEnd);
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strMSG";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = textMessage.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strUser";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = textAuthUser.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strDispTo";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ddDispTo.SelectedValue;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strSite";
            parameter.Direction = ParameterDirection.Input;
            if (sys_siteidH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_siteidH.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strDept";
            parameter.Direction = ParameterDirection.Input;
            if (sys_eclient_idH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_eclient_idH.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strAnalGrp";
            parameter.Direction = ParameterDirection.Input;
            if (sys_assignedtoanalgroupH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_assignedtoanalgroupH.Text;
            cmd.Parameters.Add(parameter);

            LayUtilDA.RunSqlCmd(cmd);
        }
        else if (strOp == "Update")
        {
            string strId = textNameH.Text;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "UPDATE [whiteboard] SET sys_whiteboarddate=@dateStart,sys_whiteboarddateend=@dateEnd,sys_whiteboardsubject=@strMSG,sys_whiteboarduser=@strUser,sys_visible_euser=@strDispTo,sys_siteid=@strSite,sys_eclient_id=@strDept,sys_analgroup=@strAnalGrp WHERE sys_whiteboard_id=@strId";
            cmd.CommandType = CommandType.Text;

            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = "@strId";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = strId;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@dateStart";
            parameter.Direction = ParameterDirection.Input;
            if (strStart == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = DateTime.Parse(strStart);
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@dateEnd";
            parameter.Direction = ParameterDirection.Input;
            if (strEnd == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = DateTime.Parse(strEnd);
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strMSG";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = textMessage.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strUser";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = textAuthUser.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strDispTo";
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ddDispTo.SelectedValue;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strSite";
            parameter.Direction = ParameterDirection.Input;
            if (sys_siteidH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_siteidH.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strDept";
            parameter.Direction = ParameterDirection.Input;
            if (sys_eclient_idH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_eclient_idH.Text;
            cmd.Parameters.Add(parameter);

            parameter = new SqlParameter();
            parameter.ParameterName = "@strAnalGrp";
            parameter.Direction = ParameterDirection.Input;
            if (sys_assignedtoanalgroupH.Text == "")
                parameter.Value = DBNull.Value;
            else
                parameter.Value = sys_assignedtoanalgroupH.Text;
            cmd.Parameters.Add(parameter);

            LayUtilDA.RunSqlCmd(cmd);
        }
        
        UpdatePage();
    }
    private void UpdatePage()
    {
        dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        UpdateList();
    }
    protected void btnDel_Click(object sender, ImageClickEventArgs e)
    {
        System.Web.UI.WebControls.ImageButton btnEdit = (System.Web.UI.WebControls.ImageButton)sender;
        //string strId = btnEdit.CommandArgument;   //// Commented by SParsh ID 178
        string strId = textNameH.Text;  ///// Written by Sparsh ID 178

        SysDA.DelWB(strId);

        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        UpdatePage();
        
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

}
