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

public partial class UserProblemHome : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        DataSet dsUser = UserDA.GetUserInfo(Session["User"].ToString());
        if (dsUser == null || dsUser.Tables.Count <= 0 || dsUser.Tables[0].Rows.Count <= 0)
            return;

        // Written begin by Sparsh ID 176
        string strHome = dsUser.Tables[0].Rows[0]["sys_problemhome"].ToString();
        // Written end by Sparsh ID 176
        string strURL;
        switch (strHome)
        {
            case "0":
                strURL = "Blank.aspx";
                break;
            case "1":
                strURL = "UserProblem.aspx?filter=myassignedopen";
                break;
            case "2":
                strURL = "UserProblem.aspx?filter=myassignedall";
                break;
            case "3":
                strURL = "UserProblem.aspx?filter=unassigned";
                break;
            case "4":
                strURL = "UserProblem.aspx?filter=all";
                break;
            case "5":
                strURL = "UserProblem.aspx?filter=allopen";
                break;
            case "10":
                strURL = "UserProblem.aspx?filter=mygroupassginedall";
                break;
            case "11":
                strURL = "UserProblem.aspx?filter=mygroupunassginedall";
                break;
            case "12":
                strURL = "UserProblem.aspx?filter=mygroupassginedopen";
                break;
            case "13":
                strURL = "UserProblem.aspx?filter=mygroupunassginedopen";
                break;
            case "14":
                strURL = "UserProblem.aspx?filter=outsla";
                break;
            case "15":
                strURL = "UserProblem.aspx?filter=escl1";
                break;
            case "16":
                strURL = "UserProblem.aspx?filter=escl2";
                break;
            case "17":
                strURL = "UserProblem.aspx?filter=escl3";
                break;
             default:
                strURL = "UserProblem.aspx?filter=myassignedopen";
                break;
        }

        Server.Transfer(strURL);
    }
}
