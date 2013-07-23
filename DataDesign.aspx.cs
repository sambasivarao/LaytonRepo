using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;
using System.Data;
using System.Data.SqlClient;
using Infragistics.Web.UI;
using Infragistics.Web.UI.GridControls;
using Infragistics.Web.UI.ListControls;

public partial class Admin_DataDesign : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        LayUtil.CheckLoginAdmin(Session, Response);
        if (!IsPostBack)
        {

            textCurTableH.Text = LayUtil.GetQueryString(Request, "table");

            if (textCurTableH.Text == "")
                textCurTableH.Text = "request";

            if (textCurTableH.Text == "euser")
            {
                textCurTableH.ToolTip = Application["appenduserterm"].ToString();
            }
            else if (textCurTableH.Text == "problem" || textCurTableH.Text == "change")
            {
                if (textCurTableH.Text == "problem")
                {
                    textCurTableH.ToolTip = "Problem";
                }
                else
                {
                    textCurTableH.ToolTip = "Change";
                }
            }
            else
            {
                textCurTableH.ToolTip = Application["app" + textCurTableH.Text + "term"].ToString();
            }

            //textCurTableH.ToolTip = ;
            InitGrid();


            ddType.Items.Clear();
            DropDownItem item;
            item = new DropDownItem("TEXT", "nvarchar");
            ddType.Items.Add(item);
            item = new DropDownItem("INTEGER", "integer");
            ddType.Items.Add(item);
            item = new DropDownItem("DECIMAL", "numeric(18,4)");
            ddType.Items.Add(item);
            item = new DropDownItem("DATE", "smalldatetime");
            ddType.Items.Add(item);
            item = new DropDownItem("MEMO(Large Text)", "ntext");
            ddType.Items.Add(item);

            this.dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        }

        lbMSG.Text = string.Empty;
        UpdateList();
    }

    protected override void OnPreRender(EventArgs e)
    {
        //this.dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        base.OnPreRender(e);
    }



    public void InitGrid()
    {
        dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogInfo.Header.CaptionText = "Add Field";
        this.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        dialogHidden.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        lbTTL.Text = textCurTableH.ToolTip + " - Data Structure";

        LayUtil.SetFont(this.form1, Application);

    }
    private void UpdateList()
    {
        dg.ClearDataSource();

        DataSet ds = DataDesignDA.GetTblCol(textCurTableH.Text);

        CreateColumns();

        dg.DataSource = ds;

        dg.DataBind();
    }
    private void CreateColumns()
    {
        TemplateDataField templateColumn1 = (TemplateDataField)this.dg.Columns["ColName"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "ColName";
            field1.Header.Text = "Fields Name";
            this.dg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)this.dg.Columns["ColName"];
        templateColumn1.ItemTemplate = new NameTemplate();
        templateColumn1.Width = new Unit("60%");
        templateColumn1.CssClass = "Col_Left";



        templateColumn1 = (TemplateDataField)this.dg.Columns["ColType"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "ColType";
            field1.Header.Text = "Type";
            this.dg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)this.dg.Columns["ColType"];
        templateColumn1.ItemTemplate = new TypeTemplate();
        templateColumn1.Width = new Unit("15%");


        templateColumn1 = (TemplateDataField)this.dg.Columns["ColSize"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "ColSize";
            field1.Header.Text = "Size";
            this.dg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)this.dg.Columns["ColSize"];
        templateColumn1.ItemTemplate = new SizeTemplate();
        templateColumn1.Width = new Unit("15%");

        templateColumn1 = (TemplateDataField)this.dg.Columns["DelField"];
        if (templateColumn1 == null)
        {
            TemplateDataField field1 = new TemplateDataField();
            field1.Key = "DelField";
            field1.Header.Text = "";
            this.dg.Columns.Add(field1);
        }
        templateColumn1 = (TemplateDataField)this.dg.Columns["DelField"];
        templateColumn1.ItemTemplate = new DelTemplate(textCurTableH.ToolTip, this);
        templateColumn1.Width = new Unit("10%");


    }


    private class NameTemplate : ITemplate
    {
        #region ITemplate Members

        public NameTemplate()
        {
        }
        public void InstantiateIn(Control container)
        {
            Label lb1 = new Label();
            lb1.Text = ((DataRowView)((TemplateContainer)container).DataItem)["ColName"].ToString();
            container.Controls.Add(lb1);
        }

        #endregion
    }
    private class TypeTemplate : ITemplate
    {
        #region ITemplate Members

        public TypeTemplate()
        {
        }
        public void InstantiateIn(Control container)
        {
            Label lb1 = new Label();
            lb1.Text = ((DataRowView)((TemplateContainer)container).DataItem)["ColType"].ToString();
            container.Controls.Add(lb1);
        }

        #endregion
    }
    private class SizeTemplate : ITemplate
    {
        #region ITemplate Members

        public SizeTemplate()
        {
        }
        public void InstantiateIn(Control container)
        {
            if (((DataRowView)((TemplateContainer)container).DataItem)["ColSize"].ToString() != "")
            {
                Label lb1 = new Label();
                lb1.Text = ((DataRowView)((TemplateContainer)container).DataItem)["ColSize"].ToString();
                container.Controls.Add(lb1);
            }
        }

        #endregion
    }
    private class DelTemplate : ITemplate
    {
        #region ITemplate Members

        public string strTable;
        private Admin_DataDesign ParentPage;
        public DelTemplate(string table, Admin_DataDesign pPage)
        {
            strTable = table;
            ParentPage = pPage;
        }
        public void InstantiateIn(Control container)
        {
            string strName = ((DataRowView)((TemplateContainer)container).DataItem)["ColName"].ToString();
            if (strName.Length > 4 && strName.Substring(0, 4) == "usr_")
            {
                System.Web.UI.WebControls.ImageButton b1 = new System.Web.UI.WebControls.ImageButton();
                b1.ImageUrl = "Application_Images/16x16/delete_icon_16px.png";
                b1.CommandArgument = ((DataRowView)((TemplateContainer)container).DataItem)["ColName"].ToString();
                b1.Attributes.Add("table", strTable);
                //b1.Click += new ImageClickEventHandler(b1_Click);
                b1.OnClientClick = "btnDelPre_Clicked('" + strTable + "','" + b1.CommandArgument + "');return false;";
                container.Controls.Add(b1);
            }
        }

        protected void b1_Click(object sender, ImageClickEventArgs e)
        {
            System.Web.UI.WebControls.ImageButton b1 = (System.Web.UI.WebControls.ImageButton)sender;
            ParentPage.textColNameH.Text = b1.CommandArgument;
            if (ParentPage.dialogDelConfirm.WindowState == Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden)
            {
                //ParentPage.lbDelMSG.Text = "Do you wish to remove the field '" + b1.CommandArgument + "' from your '" + b1.Attributes["table"].ToString() + "' form data? This will also remove the field from all forms where it is currently in use.";
                ParentPage.lbDelMSG.Text = "This will remove the user-defined field '" + b1.CommandArgument + "' from the entire application.";
                ParentPage.dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Normal;
            }
            else
            {
                ;
            }
        }

        #endregion
    }
    private bool VerifyFldName(string strFldName)
    {
        if (strFldName == "")
        {
            lbMSG.Text = "You can't specify a blank fieldname.";
            return false;
        }
        else if (strFldName.Substring(0, 4) != "usr_")
        {
            lbMSG.Text = "User Defined Fields must begin with 'usr_'.";
            return false;
        }
        else if (strFldName == "usr_")
        {
            lbMSG.Text = "User Defined Fields must contain characters after the 'usr_' prefix.";
            return false;
        }

        //char ch[] = strFldName.ToCharArray();
        foreach (char ch in strFldName.ToCharArray())
        {
            if (ch < 65 || ch > 90)
            {
                if (ch < 97 || ch > 122)
                {
                    if (ch < 48 || ch > 57)
                    {
                        if (ch != 95)
                        {
                            if (ch != 45)
                            {
                                lbMSG.Text = "You have invalid characters in your fieldname. You can only specify alpha-numeric characters and underscores & hyphens.";
                                return false;
                            }
                        }
                    }


                }
            }
        }

        return true;
    }
    private int VerifySize(string strSize)
    {
        int nSize = 50;
        try
        {
            nSize = int.Parse(strSize);

            if (nSize >= 256)
                nSize = 50;
        }
        catch
        {
        }

        return nSize;
    }
    protected void btnUpdate_Click(object sender, ImageClickEventArgs e)
    {
        string strType = ddType.SelectedValue;
        if (strType == "")
            strType = "nvarchar";

        if (strType == "nvarchar")
        {
            textSize.Style["visibility"] = "visible";
            lbSize.Style["visibility"] = "visible";
        }
        else
        {
            textSize.Style["visibility"] = "hidden";
            lbSize.Style["visibility"] = "hidden";
        }
        //Changed by Sparsh ID 188
        if ((DataDesignDA.CheckExist(textCurTableH.Text, textName.Text)) || (DataDesignDA.CheckExist("action", textName.Text)) || (DataDesignDA.CheckExist("request", textName.Text)) || (DataDesignDA.CheckExist("problem", textName.Text)) || (DataDesignDA.CheckExist("change", textName.Text)) || (DataDesignDA.CheckExist("user", textName.Text)) || (DataDesignDA.CheckExist("euser", textName.Text)) || (DataDesignDA.CheckExist("company", textName.Text)) || (DataDesignDA.CheckExist("company", textName.Text)) || (DataDesignDA.CheckExist("eclient", textName.Text)) || (DataDesignDA.CheckExist("site", textName.Text)) || (DataDesignDA.CheckExist("solution", textName.Text)) || (DataDesignDA.CheckExist("priority", textName.Text)) || (DataDesignDA.CheckExist("survey", textName.Text)))
        {
            lbMSG.Text = "'" + textName.Text + "' already exist";
            return;
        }
        //Changed by Sparsh ID 188

        if (!VerifyFldName(textName.Text))
        {
            //lbMSG.Text = "You have invalid characters in your fieldname. You can only specify alpha-numeric characters and underscores hyphens.";
            return;
        }

        // Add Main Table
        AddField(textCurTableH.Text, strType);

        //Add Related
        if (textCurTableH.Text.ToLower() == "request")
        {
            AddField("requesttemplate", strType);
        }
        else if (textCurTableH.Text.ToLower() == "action")
        {
            AddField("requesttypeactions", strType);
        }

        dialogInfo.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
        UpdateList();
    }

    private void AddField(string strTable, string strType)
    {
        SqlCommand cmd;
        if (strType == "nvarchar")
        {
            int nSize = VerifySize(textSize.Text);

            cmd = new SqlCommand();

            cmd.CommandText = "ALTER TABLE [" + strTable + "] ADD " + textName.Text + " nvarchar(" + nSize.ToString() + ")";

            LayUtilDA.RunSqlCmd(cmd);
        }
        else if ((strType == "smalldatetime") && strTable == "euser")
        {
            cmd = new SqlCommand();
            cmd.CommandText = "ALTER TABLE [" + strTable + "] ADD " + textName.Text + " smalldatetime";
            LayUtilDA.RunSqlCmd(cmd);
            /*
            if (chkForceCurDate.Checked)
            {
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [" + strTable + "] SET " + textName.Text + "=@now";
                SqlParameter parameter = new SqlParameter();
                parameter.ParameterName = "@now";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = DateTime.Now;
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }
             */ 
        }
        else
        {
            cmd = new SqlCommand();
            cmd.CommandText = "ALTER TABLE [" + strTable + "] ADD " + textName.Text + " " + strType;
            LayUtilDA.RunSqlCmd(cmd);
        }
    }
    protected void btnDel_Click(object sender, ImageClickEventArgs e)
    {
        DropField(textCurTableH.Text, textColNameH.Text);

        if (textCurTableH.Text == "request")
        {
            DropField("requesttemplate", textColNameH.Text);
        }
        else if (textCurTableH.Text == "action")
        {
            DropField("requesttypeactions", textColNameH.Text);
        }

        UpdateRelated(textCurTableH.Text, textColNameH.Text);

        UpdateList();
        dialogDelConfirm.WindowState = Infragistics.Web.UI.LayoutControls.DialogWindowState.Hidden;
    }

    private void UpdateRelated(string strTable, string strCol)
    {
        XmlDocument xml = new XmlDocument();
        DataSet ds;
        SqlCommand cmd;
        SqlParameter parameter;
        if (strTable == "request")
        {
            ds = LibReqClassDA.GetReqClassAllInfo("");

            //--------------------------------------------------
            // Loop Request Classes and Remove deleted field
            //--------------------------------------------------
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_requestclass_xmluserform"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE requestclass SET sys_requestclass_xmluserform=@sys_requestclass_xmluserform WHERE sys_requestclass_id=@sys_requestclass_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_xmluserform";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_requestclass_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);



                xml.LoadXml(dr["sys_requestclass_xmluserformspawn"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE requestclass SET sys_requestclass_xmluserformspawn=@sys_requestclass_xmluserformspawn WHERE sys_requestclass_id=@sys_requestclass_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_xmluserformspawn";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_requestclass_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);



                xml.LoadXml(dr["sys_requestclass_xmlenduserform"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE requestclass SET sys_requestclass_xmlenduserform=@sys_requestclass_xmlenduserform WHERE sys_requestclass_id=@sys_requestclass_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_xmlenduserform";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_requestclass_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_requestclass_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);


            }

            //--------------------------------------------------
            // Loop Users
            //--------------------------------------------------
            ds = UserDA.GetUsers();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_request_resultset_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE user SET sys_request_resultset_xml=@sys_request_resultset_xml WHERE sys_username=@sys_username";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_request_resultset_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_username";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_username"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

            ds = FormDesignerDA.GetResultSetXML("enduserrequest", "defaultrequest");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_resultsetprofile_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [resultsetprofile] SET sys_resultsetprofile_xml=@sys_resultsetprofile_xml WHERE sys_resultsetprofile_id=@sys_resultsetprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_resultsetprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

            ds = FormDesignerDA.GetSrchFormXML();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_searchformprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [searchformprofile] SET sys_searchformprofile_xml=@sys_searchformprofile_xml WHERE sys_searchformprofile_id=@sys_searchformprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_searchformprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

        }
        else if (strTable == "action")
        {
            ds = FormDesignerDA.LoadFormXML("action");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }


            ds = UserDA.GetUsers();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_action_resultset_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE user SET sys_action_resultset_xml=@sys_action_resultset_xml WHERE sys_username=@sys_username";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_action_resultset_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_username";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_username"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }

            ds = FormDesignerDA.GetResultSetXML("enduseraction", "defaultaction");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_resultsetprofile_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [resultsetprofile] SET sys_resultsetprofile_xml=@sys_resultsetprofile_xml WHERE sys_resultsetprofile_id=@sys_resultsetprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_resultsetprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

            ds = LayUtilDA.GetDSSQL("SELECT * FROM searchformprofile WHERE sys_searchformprofile_id='action'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_searchformprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [searchformprofile] SET sys_searchformprofile_xml=@sys_searchformprofile_xml WHERE sys_searchformprofile_id=@sys_searchformprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_searchformprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }


        }
        else if (strTable == "eclient")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='client'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }
        }
        else if (strTable == "solution")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='solution'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }
        }
        else if (strTable == "user")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='user'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }
        }
        else if (strTable == "priority")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='priority'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }
        }
        else if (strTable == "survey")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='survey'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }


            ds = LayUtilDA.GetDSSQL("SELECT * FROM searchformprofile WHERE sys_searchformprofile_id='survey'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_searchformprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [searchformprofile] SET sys_searchformprofile_xml=@sys_searchformprofile_xml WHERE sys_searchformprofile_id=@sys_searchformprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_searchformprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }

            ds = LayUtilDA.GetDSSQL("SELECT * FROM resultsetprofile WHERE sys_resultsetprofile_id='defaultsurvey'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_resultsetprofile_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [resultsetprofile] SET sys_resultsetprofile_xml=@sys_resultsetprofile_xml WHERE sys_resultsetprofile_id=@sys_resultsetprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_resultsetprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }

            ds = LayUtilDA.GetDSSQL("SELECT sys_username, sys_survey_resultset_xml FROM [USER]");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_survey_resultset_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [user] SET sys_survey_resultset_xml=@sys_survey_resultset_xml WHERE sys_username=@sys_username";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_survey_resultset_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_username";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_username"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }


        }
        else if (strTable == "site")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='client'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }


            ds = LayUtilDA.GetDSSQL("SELECT * FROM quickinfoprofile WHERE sys_quickinfoprofile_id='site'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_quickinfoprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [quickinfoprofile] SET sys_quickinfoprofile_xml=@sys_quickinfoprofile_xml WHERE sys_quickinfoprofile_id=@sys_quickinfoprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_quickinfoprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_quickinfoprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_quickinfoprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

        }
        else if (strTable == "euser")
        {
            ds = LayUtilDA.GetDSSQL("SELECT * FROM quickinfoprofile WHERE sys_quickinfoprofile_id='enduser'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_quickinfoprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [quickinfoprofile] SET sys_quickinfoprofile_xml=@sys_quickinfoprofile_xml WHERE sys_quickinfoprofile_id=@sys_quickinfoprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_quickinfoprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_quickinfoprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_quickinfoprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='enduser'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);

            }

            ds = LayUtilDA.GetDSSQL("SELECT * FROM formprofile WHERE sys_formprofile_id='registerenduser'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_formprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [formprofile] SET sys_formprofile_xml=@sys_formprofile_xml WHERE sys_formprofile_id=@sys_formprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_formprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_formprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }


            ds = LayUtilDA.GetDSSQL("SELECT * FROM searchformprofile WHERE sys_searchformprofile_id='searchselectenduser'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_searchformprofile_xml"].ToString());
                xml = RemoveNodeXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [searchformprofile] SET sys_searchformprofile_xml=@sys_searchformprofile_xml WHERE sys_searchformprofile_id=@sys_searchformprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_searchformprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_searchformprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }


            ds = LayUtilDA.GetDSSQL("SELECT * FROM resultsetprofile WHERE sys_resultsetprofile_id='defaultselectenduser'");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_resultsetprofile_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [resultsetprofile] SET sys_resultsetprofile_xml=@sys_resultsetprofile_xml WHERE sys_resultsetprofile_id=@sys_resultsetprofile_id";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_resultsetprofile_id";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_resultsetprofile_id"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }

            ds = LayUtilDA.GetDSSQL("SELECT sys_username, sys_selectenduser_resultset_xml FROM [USER]");
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                xml.LoadXml(dr["sys_selectenduser_resultset_xml"].ToString());
                xml = RemoveNodeRSXml(xml, strCol);
                cmd = new SqlCommand();
                cmd.CommandText = "UPDATE [user] SET sys_selectenduser_resultset_xml=@sys_selectenduser_resultset_xml WHERE sys_username=@sys_username";
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_selectenduser_resultset_xml";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = xml.InnerXml;
                cmd.Parameters.Add(parameter);
                parameter = new SqlParameter();
                parameter.ParameterName = "@sys_username";
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = dr["sys_username"];
                cmd.Parameters.Add(parameter);
                LayUtilDA.RunSqlCmd(cmd);
            }

        }
    }

    private XmlDocument RemoveNodeXml(XmlDocument xml, string strCol)
    {
        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
        {
            if (node.Name == strCol)
            {
                xml.DocumentElement.RemoveChild(node);
            }
        }
        return xml;
    }
    private XmlDocument RemoveNodeRSXml(XmlDocument xml, string strCol)
    {
        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
        {
            if (node.Name == "resultset")
            {
                foreach (XmlNode gnode in node.ChildNodes)
                {
                    if (gnode.Name == strCol)
                    {
                        node.RemoveChild(gnode);
                    }
                }

            }
        }

        return xml;
    }
    private void DropField(string strTable, string strCol)
    {
        SqlCommand cmd;
        cmd = new SqlCommand();

        cmd.CommandText = "ALTER TABLE [" + strTable + "] DROP COLUMN " + strCol;

        LayUtilDA.RunSqlCmd(cmd);

    }


    protected void ddType_ValueChanged(object sender, DropDownValueChangedEventArgs e)
    {
        chkForceCurDate.Visible = false;
        if (textCurTableH.Text == "euser")
        {
            //if (ddType.SelectedValue == "smalldatetime")
                //chkForceCurDate.Visible = true;
        }

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
