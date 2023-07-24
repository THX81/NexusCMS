using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Nexus.Reflection;

namespace Nexus.Web.UI.HtmlControls
{
    /// <summary>
    /// Method of how the Form submits input data: Post and Get.
    /// </summary>
    public enum MethodEnum { get, post }

    /// <summary>
    /// The Nexus.Web.UI.Form is an advanced Form Server Control.
    /// It replaces or supplements the standard built-in HtmlForm.
    /// It allows multiple Forms to be used on the same ASPX Page.
    /// Its Action property finally allows Posting to other Pages.
    /// 
    /// Pages that use it must descend from the Nexus.Web.UI.Page. 
    /// It currently does not support the use of Validator Controls.
    /// Otherwise it tries to fully support the ASP.NET Framework.
    /// This includes ViewState, PostBack Events, and Web-Controls.
    /// </summary>
    [ToolboxData("<{0}:Form runat=server></{0}:Form>")//,
        //ToolboxItem(typeof(WebControlToolboxItem)),
        // Designer(typeof(ReadWriteControlDesigner))
    ]
    // Base Control should be Same as for Real HtmlForm
    public class Form : HtmlContainerControl
    {
        private const string _nexusFormID = "nexusForm";
        private string _defaultButton;
        private string _defaultFocus;
        private bool _submitDisabledControls;

        private const string category = "<Nexus.Web.UI.Form>";
        private MethodEnum method = MethodEnum.post;
        private string action = "";
        private string enctype = "";
        private string target = "";
        private bool _hasValidators = false;
        private bool _hasClientScriptEnabledValidators = false;
        private bool _isClientScriptBlockRegistered = false;
        internal bool _fPostBackScriptRendered;
        private bool _fRequirePostBackScript;
        private bool _fRequireWebFormsScript;
        private bool _fWebFormsScriptRendered;
        private bool _requireFocusScript;
        private bool _requireScrollScript;


        /// <summary>
        /// Indicates where the Form its submits input data, i.e. URL.
        /// Setting a value for Action eliminates any PostBack Events.
        /// </summary>
        [DefaultValue(""), Category(Form.category),
            Description("Indicates where the Form its submits input data, i.e. URL.  Setting a value for Action eliminates any PostBack Events.")]
        public string Action
        {
            get { return action; }
            set { action = value; }
        }

        internal string ClientOnSubmitEvent
        {
            get
            {
                if (!this.Page.ClientScript.HasSubmitStatements && ((!this.SubmitDisabledControls) || (this.Page.EnabledControls.Count <= 0)))
                {
                    return string.Empty;
                }
                return "javascript:return " + this.ClientID + "_OnSubmit();";
            }
        }

        /// <summary>Gets or sets the child control of the <see cref="T:System.Web.UI.HtmlControls.HtmlForm"></see> control that causes postback when the ENTER key is pressed.</summary>
        /// <returns>The <see cref="P:System.Web.UI.Control.ID"></see> of the button control to display as the default button when the <see cref="T:System.Web.UI.HtmlControls.HtmlForm"></see> is loaded. The default value is an empty string ("").</returns>
        /// <exception cref="T:System.InvalidOperationException">The control referenced as the default button is not of the type <see cref="T:System.Web.UI.WebControls.IButtonControl"></see>.</exception>
        [WebCategory("Behavior"), DefaultValue("")]
        public string DefaultButton
        {
            get
            {
                if (this._defaultButton == null)
                {
                    return string.Empty;
                }
                return this._defaultButton;
            }
            set
            {
                this._defaultButton = value;
            }
        }

        /// <summary>Gets or sets the control on the form to display as the control with input focus when the <see cref="T:System.Web.UI.HtmlControls.HtmlForm"></see> control is loaded.</summary>
        /// <returns>The <see cref="P:System.Web.UI.Control.ID"></see> of the control on the form to display as the control with input focus when the <see cref="T:System.Web.UI.HtmlControls.HtmlForm"></see> is loaded. The default value is an empty string ("").</returns>
        [DefaultValue(""), WebCategory("Behavior")]
        public string DefaultFocus
        {
            get
            {
                if (this._defaultFocus == null)
                {
                    return string.Empty;
                }
                return this._defaultFocus;
            }
            set
            {
                this._defaultFocus = value;
            }
        }

        [Browsable(false)]
        public new bool Disabled
        {
            get { return false; }
        }

        [Browsable(false)]
        public override bool EnableViewState
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates what type of input data the Form is submitting.
        /// Use 'multipart/form-data' to upload Files from the Client.
        /// </summary>
        [DefaultValue(""), Category(Form.category),
            Description("Indicates what type of input data the Form is submitting.  Use 'multipart/form-data' to upload Files from the Client.")]
        public string EncType
        {
            get { return enctype; }
            set { enctype = value; }
        }

        public bool HasValidators
        {
            get { return _hasValidators; }
            set { _hasValidators = value; }
        }

        public bool HasClientScriptEnabledValidators
        {
            get { return _hasClientScriptEnabledValidators; }
            set { _hasClientScriptEnabledValidators = value; }
        }

        /// <summary>
        /// Identifier of the Form, also used for the Name Property.
        /// </summary>
        [Category(Form.category),
            Description("Identifier of the Form, also used for the Name Property.")]
        public override string ID
        {
            get { return base.ID; }
            set { base.ID = value; }
        }

        public bool IsClientScriptBlockRegistered
        {
            get { return _isClientScriptBlockRegistered; }
            set { _isClientScriptBlockRegistered = value; }
        }

        /// <summary>
        /// Indicates if this Form is the source of a Page PostBack.
        /// </summary>
        [Category(Form.category),
            Description("Indicates if this Form is the source of a Page PostBack.")]
        public bool IsPostBack
        {
            get
            {
                if (this.Context == null)
                {
                    return false;
                }
                else
                {
                    return (this.Context.Request.Form[Form._nexusFormID] == this.ClientID);
                }
            }
        }

        /// <summary>
        /// Indicates how the Form submits input data to the Server.
        /// Allowable values are limited to: Post (Default) and Get.
        /// </summary>
        [DefaultValue(MethodEnum.post), Category(Form.category),
            Description("Indicates how the Form submits input data to the Server.  Allowable values are limited to: Post (Default) and Get.")]
        public MethodEnum Method
        {
            get { return method; }
            set { method = value; }
        }

        [Browsable(false)]
        public new Nexus.Web.UI.Page Page
        {
            get { return (Nexus.Web.UI.Page)base.Page; }
        }

        [WebCategory("Behavior"), DefaultValue(false)]
        public virtual bool SubmitDisabledControls
        {
            get
            {
                return this._submitDisabledControls;
            }
            set
            {
                this._submitDisabledControls = value;
            }
        }

        [Browsable(false)]
        public override string TagName
        {
            get { return "form"; }
        }

        /// <summary>
        /// Indicates the frame or window the Form submits its data.
        /// Common values: _self (Default), _blank, _parent, and _top.
        /// </summary>
        [DefaultValue(""), Category(Form.category),
            Description("Indicates the frame or window the Form submits its data.  Common values: _self (Default), _blank, _parent, and _top.")]
        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// Indicates if this Form is actually Rendered to the Page.
        /// </summary>
        [DefaultValue(true), Category(Form.category),
            Description("Indicates if this Form is actually Rendered to the Page.")]
        public override bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; }
        }









        protected override void OnInit(System.EventArgs e)
        {
            // Force use of Nexus.Page and Track Nexus.Forms
            if (base.Page is Nexus.Web.UI.Page)
            {
                this.Page.hasNexusForms = true;
                if (this.IsPostBack)
                {
                    this.Page.postBackForm = this.ClientID;
                }
            }
            else
            {
                throw new Exception("Nexus.Web.UI.Form must be used in Nexus.Web.UI.Page");
            }

            // Perform ValidationGroup property for inner validable controls
            foreach (Control child in this.Controls)
            {
                if (child.GetType().GetProperty("ValidationGroup") != null)
                {
                    if (child.GetType().GetProperty("ValidationGroup").GetValue(child, null) == null
                        || String.IsNullOrEmpty(child.GetType().GetProperty("ValidationGroup").GetValue(child, null).ToString()))
                        child.GetType().GetProperty("ValidationGroup").SetValue(child, this.ClientID, null);
                }
            }

            base.OnInit(e);
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            this.CheckValidators(this);
            if (this.Page.MaintainScrollPositionOnPostBack)
            {
                this.Page.LoadScrollPosition();
            }

        }

        /// <summary>
        /// Kontrola validaoru a zda maji aktivni klientsky skript
        /// </summary>
        /// <param name="control"></param>
        private void CheckValidators(Control control)
        {
            foreach (Control child in control.Controls)
            {
                BaseValidator validator = (child as BaseValidator);
                if (validator != null)
                {
                    this.HasValidators = true;
                    if (validator.EnableClientScript)
                    {
                        this.HasClientScriptEnabledValidators = true;
                        if (!this.Page.hasHtmlForm)
                        {
                            this.Page.ClientScript.RegisterArrayDeclaration("Page_Validators", "document.getElementById(\"" + validator.ClientID + "\")");
                            this.Page.ClientScript.RegisterValidator(validator);
                        }
                    }
                }
                this.CheckValidators(child);
            }
            if (this.HasClientScriptEnabledValidators)
                RegisterValidatorCommonScript();
        }

        /// <summary>Registers code on the page for client-side validation.</summary>
        protected void RegisterValidatorCommonScript()
        {
            //if (((this.Page.ClientScript != null) && this.Page.ClientScript.SupportsPartialRendering) || !this.Page.ClientScript.IsClientScriptBlockRegistered(typeof(BaseValidator), "ValidatorIncludeScript"))
            if ((this.Page.ClientScript != null) || !this.Page.ClientScript.IsClientScriptBlockRegistered(typeof(BaseValidator), "ValidatorIncludeScript"))
            {
                if (!this.Page.hasHtmlForm)
                {
                    this.Page.ClientScript.RegisterClientScriptResource(this.GetType(), "Nexus.Web.WebForms.js");
                    this.Page.ClientScript.RegisterClientScriptResource(this.GetType(), "Nexus.Web.WebUIValidation.js");
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ValidatorIncludeScript", "\r\n    var Page_ValidationActive = false;\r\n    if (typeof(ValidatorOnLoad) == \"function\") {\r\n        ValidatorOnLoad();\r\n    }\r\n\r\n    function ValidatorOnSubmit() {\r\n" + (HttpContext.Current.IsDebuggingEnabled ? "        console.trace();\r\n" : String.Empty) + "        if (Page_ValidationActive) {\r\n            return ValidatorCommonOnSubmit();\r\n        }\r\n        else {\r\n            return true;\r\n        }\r\n    }\r\n        ", false);
                }
                this.Page.ClientScript.RegisterOnSubmitStatement(this.GetType(), "ValidatorOnSubmit", "    if (typeof(ValidatorOnSubmit) == \"function\" && ValidatorOnSubmit() == false) return false;");
            }
        }

        internal void RegisterFocusScript()
        {
            if (this.Page.ClientSupportsFocus && !this._requireFocusScript)
            {
                this.Page.ClientScript.RegisterHiddenField("__lastfocus", string.Empty);
                FieldInvoker<bool>.SetValue(this.Page, "_requireFocusScript", true);
                if (this.Page.PartialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this.Page.PartialCachingControlStack)
                    {
                        MethodInvoker.Invoke(control, "RegisterFocusScript"
                            , null, BindingFlags.Instance | BindingFlags.NonPublic);
                        //control.RegisterFocusScript();
                    }
                }
            }
        }

        internal void RegisterPostBackScript()
        {
            if (this.Page.ClientSupportsJavaScript && !this._fPostBackScriptRendered)
            {
                if (!this._fRequirePostBackScript)
                {
                    this.Page.ClientScript.RegisterHiddenField("__eventtarget", string.Empty);
                    this.Page.ClientScript.RegisterHiddenField("__eventargument", string.Empty);
                    this._fRequirePostBackScript = true;
                }
                if (this.Page.PartialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this.Page.PartialCachingControlStack)
                    {
                        MethodInvoker.Invoke(control, "RegisterPostBackScript"
                            , null, BindingFlags.Instance | BindingFlags.NonPublic);
                        //control.RegisterPostBackScript();
                    }
                }
            }
        }

        internal void RegisterWebFormsScript()
        {
            if (this.Page.ClientSupportsJavaScript && !this._fWebFormsScriptRendered)
            {
                this.RegisterPostBackScript();
                this._fRequireWebFormsScript = true;
                if (this.Page.PartialCachingControlStack != null)
                {
                    foreach (BasePartialCachingControl control in this.Page.PartialCachingControlStack)
                    {
                        MethodInvoker.Invoke(control, "RegisterWebFormsScript"
                            , null, BindingFlags.Instance | BindingFlags.NonPublic);
                        //control.RegisterWebFormsScript();
                    }
                }
            }
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            if (this.ID == null)
            {
                this.ID = this.ClientID;
            }
            //base.RenderAttributes(writer);

            if (this.Page.EnableLegacyRendering)
                writer.WriteAttribute("name", this.ClientID);

            writer.WriteAttribute("method", this.Method.ToString());

            if (this.Action == "")
                writer.WriteAttribute("action", this.Context.Request.RawUrl, true);
            else
                writer.WriteAttribute("action", this.ResolveUrl(this.Action), true);

            if (this.EncType != "")
                writer.WriteAttribute("enctype", this.EncType);

            if (this.Page.EnableLegacyRendering && this.Target != "")
                writer.WriteAttribute("target", this.Target);

            if (this.HasClientScriptEnabledValidators)
                writer.WriteAttribute("onsubmit", this.ClientOnSubmitEvent);

            base.RenderAttributes(writer);
        }

        protected override void RenderChildren(HtmlTextWriter writer)
        {
            StringBuilder html = new StringBuilder();
            StringWriter stringWriter = new StringWriter(html);
            HtmlTextWriter tempWriter = new HtmlTextWriter(stringWriter);
            
            // oblbneme dcerine controly ze se renderuji v serverovem asp.net HtmlForm
            this.Page.inServerForm = true;
            base.RenderChildren(tempWriter);
            // stahneme oblbnuti zpet
            this.Page.inServerForm = false;

            if (html.ToString().Contains("__doPostBack"))
                this.RegisterPostBackScript();
            html.Replace("__doPostBack", this.Page.ClientScript._postBackFunctionPrefix + this.UniqueID);
            


            this.BeginFormRender(writer, this.UniqueID);
            writer.Write(html.ToString());
            this.EndFormRender(writer, this.UniqueID);

            if (stringWriter != null)
                stringWriter.Dispose();
            if (tempWriter != null)
                tempWriter.Dispose();

            ////////////#region hidden fields registration
            ////////////writer.WriteLine("<div>");
            ////////////writer.WriteLine("<input type='hidden' name='" + Form.formName + "'  value='" + this.ClientID + "' />");
            ////////////if (this.Page.ViewStateType == Nexus.Web.UI.ViewStateEnum.SessionOnly)
            ////////////{
            ////////////    //writer.WriteLine("<input type='hidden' name='__viewstate'  value='' />");
            ////////////}
            ////////////else if (this.Page.hasHtmlForm || doPostBack)
            ////////////{
            ////////////    writer.WriteLine("<input type='hidden' name='__viewstate'  value='" + this.Page.viewState + "' />");
            ////////////}
            ////////////if (doPostBack)
            ////////////{
            ////////////    writer.WriteLine("<input type='hidden' name='__eventtarget'  value='' />");
            ////////////    writer.WriteLine("<input type='hidden' name='__eventargument'  value='' />");
            ////////////    writer.WriteLine("</div>");
            ////////////}
            ////////////else
            ////////////    writer.WriteLine("</div>");
            ////////////#endregion


        }

        internal void BeginFormRender(HtmlTextWriter writer, string formUniqueID)
        {
            bool flag = !this.Page.EnableLegacyRendering;
            if (flag)
            {
                writer.WriteLine();
                writer.Write("<div>");
            }
            this.Page.ClientScript.RenderHiddenFields(writer);
            //this.Page.RenderViewStateFields(writer);
            writer.WriteLine();
            if (flag)
            {
                writer.WriteLine("</div>");
            }
            if (this.Page.ClientSupportsJavaScript)
            {
                this.Page.ClientScript.RenderClientScriptStart(writer);
                if (this.Page.MaintainScrollPositionOnPostBack && !FieldInvoker<bool>.GetValue(this.Page, "_requireScrollScript"))
                {
                    this.Page.ClientScript.RegisterHiddenField(this.Page._scrollPositionXID, this.Page._scrollPositionX.ToString(CultureInfo.InvariantCulture));
                    this.Page.ClientScript.RegisterHiddenField(this.Page._scrollPositionYID, this.Page._scrollPositionY.ToString(CultureInfo.InvariantCulture));
                    this.Page.ClientScript.RegisterStartupScript(typeof(Nexus.Web.UI.Page), "PageScrollPositionScript", "\r\n    theForm.oldSubmit = theForm.submit;\r\n    theForm.submit = WebForm_SaveScrollPositionSubmit;\r\n\r\n    theForm.oldOnSubmit = theForm.onsubmit;\r\n    theForm.onsubmit = WebForm_SaveScrollPositionOnSubmit;\r\n" + (this.Page.IsPostBack ? "\r\n    theForm.oldOnLoad = window.onload;\r\n    window.onload = WebForm_RestoreScrollPosition;\r\n" : string.Empty), false);
                    //this.RegisterWebFormsScript();
                    FieldInvoker<bool>.SetValue(this.Page, "_requireScrollScript", true);
                }
                if (this.Page.ClientSupportsFocus && ((this.Page.RenderFocusScript || (this.DefaultFocus.Length > 0)) || (this.DefaultButton.Length > 0)))
                {
                    int num = 0;
                    string s = string.Empty;
                    //if (this.FocusedControlID.Length > 0)
                    //{
                    //    s = this.FocusedControlID;
                    //}
                    //else if (this.FocusedControl != null)
                    //{
                    //    if (this.FocusedControl.Visible)
                    //    {
                    //        s = this.FocusedControl.ClientID;
                    //    }
                    //}
                    //else if (this.ValidatorInvalidControl.Length > 0)
                    //{
                    //    s = this.ValidatorInvalidControl;
                    //}
                    //else if (this.LastFocusedControl.Length > 0)
                    //{
                    //    s = this.LastFocusedControl;
                    //}
                    //else if (this.Form.DefaultFocus.Length > 0)
                    //{
                    //    s = this.Form.DefaultFocus;
                    //}
                    //else if (this.Form.DefaultButton.Length > 0)
                    //{
                    //    s = this.Form.DefaultButton;
                    //}
                    Type crossSiteScriptingValidation = ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.CrossSiteScriptingValidation");
                    if (((s.Length > 0) && !MethodInvokerWithReturn<bool>.InvokeAndReturn(crossSiteScriptingValidation, "IsDangerousString", new Type[] { typeof(String), typeof(Int32) }, new object[] { s, num }, BindingFlags.NonPublic | BindingFlags.Static)) && MethodInvokerWithReturn<bool>.InvokeAndReturn(crossSiteScriptingValidation, "IsValidJavascriptId", new Type[] { typeof(String) }, new object[] { s }, BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        this.Page.ClientScript.RegisterClientScriptResource(this.GetType(), "Focus.js");
                        if (!this.Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), "Focus"))
                        {
                            this.RegisterWebFormsScript();
                            Type util = ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "Focus", "WebForm_AutoFocus('" + MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString", new Type[] { typeof(String) }, new object[] { s }, BindingFlags.NonPublic | BindingFlags.Static) + "');", true);
                        }
                        //IScriptManager scriptManager = this.ScriptManager;
                        //if (scriptManager != null)
                        //{
                        //    scriptManager.SetFocusInternal(s);
                        //}
                    }
                }
                if ((this.SubmitDisabledControls && (this.Page.EnabledControls.Count > 0)) && (this.Page.Request.Browser.W3CDomVersion.Major > 0))
                {
                    foreach (Control control in this.Page.EnabledControls)
                    {
                        this.Page.ClientScript.RegisterArrayDeclaration("__enabledControlArray", "'" + control.ClientID + "'");
                    }
                    this.Page.ClientScript.RegisterOnSubmitStatement(typeof(Page), "PageReEnableControlsScript", "WebForm_ReEnableControls();");
                    this.RegisterWebFormsScript();
                }
                if (this._fRequirePostBackScript)
                {
                    this.Page.ClientScript.RenderPostBackScript(writer, this, false);
                }
                if (this._fRequireWebFormsScript)
                {
                    this.Page.ClientScript.RenderWebFormsScript(writer);
                }
            }
            this.Page.ClientScript.RenderClientScriptEnd(writer);
            this.Page.ClientScript.RenderClientScriptBlocks(writer, this, true);
        }

        internal void EndFormRender(HtmlTextWriter writer, string formUniqueID)
        {
            if (this.Page.ClientScript.HasRegisteredHiddenFields)
            {
                bool flag = !this.Page.EnableLegacyRendering;
                if (flag)
                {
                    writer.WriteLine();
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                }
                this.Page.ClientScript.RenderHiddenFields(writer);
                if (flag)
                {
                    writer.RenderEndTag();
                }
            }
            this.Page.ClientScript.RenderClientScriptStart(writer);
            if (this.Page.ClientSupportsJavaScript)
            {
                this.Page.ClientScript.RenderArrayDeclares(writer, false);
                this.Page.ClientScript.RenderExpandoAttribute(writer, false);
            }
            //if (this.RequiresViewStateEncryptionInternal)
            //{
            //    this.ClientScript.RegisterHiddenField("__VIEWSTATEENCRYPTED", string.Empty);
            //}
            //if (this._containsCrossPagePost)
            //{
            //    string hiddenFieldInitialValue = EncryptString(this.Request.CurrentExecutionFilePath);
            //    this.ClientScript.RegisterHiddenField("__PREVIOUSPAGE", hiddenFieldInitialValue);
            //}
            //if (this.EnableEventValidation)
            //{
            //    this.ClientScript.SaveEventValidationField();
            //}
            if (this.Page.ClientSupportsJavaScript)
            {
                if (this._fRequirePostBackScript && !this._fPostBackScriptRendered)
                {
                    this.Page.ClientScript.RenderPostBackScript(writer, this, false);
                }
                if (this._fRequireWebFormsScript && !this._fWebFormsScriptRendered)
                {
                    this.Page.ClientScript.RenderWebFormsScript(writer);
                }
            }
            this.Page.ClientScript.RenderClientStartupScripts(writer, false);
            this.Page.ClientScript.RenderClientScriptEnd(writer);
        }



 



    }
}
