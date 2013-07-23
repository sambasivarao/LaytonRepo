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

public partial class UserChgHome : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginUser(Session, Response);
        DataSet dsUser = UserDA.GetUserInfo(Session["User"].ToString());
        if (dsUser == null || dsUser.Tables.Count <= 0 || dsUser.Tables[0].Rows.Count <= 0)
            return;

        // Written begin by Sparsh
        string strHome = dsUser.Tables[0].Rows[0]["sys_chghome"].ToString();
        //Written end by Sparsh
        string strURL;
        switch (strHome)
        {
            case "0":
                strURL = "Blank.aspx";
                break;
            case "1":
                strURL = "UserChange.aspx?filter=myassignedopen";
                break;
            case "2":
                strURL = "UserChange.aspx?filter=myassignedall";
                break;
            case "3":
                strURL = "UserChange.aspx?filter=unassigned";
                break;
            case "4":
                strURL = "UserChange.aspx?filter=all";
                break;
            case "5":
                strURL = "UserChange.aspx?filter=allopen";
                break;
            case "10":
                strURL = "UserChange.aspx?filter=mygroupassginedall";
                break;
            case "11":
                strURL = "UserChange.aspx?filter=mygroupunassginedall";
                break;
            case "12":
                strURL = "UserChange.aspx?filter=mygroupassginedopen";
                break;
            case "13":
                strURL = "UserChange.aspx?filter=mygroupunassginedopen";
                break;
            default:
                strURL = "UserChange.aspx?filter=myassignedopen";
                break;
        }

        Server.Transfer(strURL);
    }
}
