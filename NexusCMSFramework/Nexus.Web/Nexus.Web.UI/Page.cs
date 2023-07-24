namespace Nexus.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Web.Configuration;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using Nexus.Reflection;

    /// <summary>
    /// SessionOnly: ViewState is saved on Server using Session.
    /// EachInstance: ViewState is a repeated hidden Form field.
    /// UpLevelSingle: ViewState is a single hidden Form field.
    /// Note: UpLevelSingle is currently same as EachInstance.
    /// </summary>
    public enum ViewStateEnum { SessionOnly, EachInstance, UpLevelSingle }

    /// <summary>
    /// The Nexus.Web.UI.Page inherits from System.Web.UI.Page.
    /// It is required for Pages that use the Nexus.Web.UI.Form.
    /// </summary>
    public class Page : System.Web.UI.Page
    {
        private System.Collections.Stack _partialCachingControlStack;
        private Nexus.Web.UI.ClientScriptManager _clientScriptManager;
        private const string category = "<Nexus.Web.UI.Page>";
        private bool _clientSupportsJavaScript;
        private bool _clientSupportsJavaScriptChecked;
        internal int _scrollPositionX;
        internal readonly string _scrollPositionXID = "__scrollpositionx";
        internal int _scrollPositionY;
        internal readonly string _scrollPositionYID = "__scrollpositiony";
        internal bool hasNexusForms = false;
        internal bool hasHtmlForm = false;
        internal bool inServerForm = false;
        private static readonly Version JavascriptMinimumVersion = new Version("1.0");
        internal string postBackForm = "";
        internal string viewState = "";
        private ViewStateEnum viewStateType;

        public Page()
        {
            string key = "Nexus.Web.UI.ViewState";
            string config = ConfigurationManager.AppSettings[key];
            if (config == null)
            {
                this.viewStateType = ViewStateEnum.SessionOnly;
            }
            else if (config.ToUpper() == "EACHINSTANCE")
            {
                this.viewStateType = ViewStateEnum.EachInstance;
            }
            else if (config.ToUpper() == "UPLEVELSINGLE")
            {
                this.viewStateType = ViewStateEnum.UpLevelSingle;
            }
            else
            {
                this.viewStateType = ViewStateEnum.SessionOnly;
            }
        }




        /// <summary>
        /// Indicates the Name of the Form that caused the PostBack.
        /// The value of PostBackForm will be Empty if the HtmlForm.
        /// </summary>
        [Category(Nexus.Web.UI.Page.category),
            Description("Indicates the Name of the Form that caused the PostBack.  The value of PostBackForm will be Empty if the HtmlForm.")]
        public string PostBackForm
        {
            get { return postBackForm; }
        }

        /// <summary>
        /// Indicates how the ViewState of the Page will be Tracked.
        /// Allowable values of this ViewStateType are limited to:
        /// SessionOnly (Default), EachInstance, and UpLevelSingle.
        /// Setup default in Web.config: 'Nexus.Web.UI.ViewState'.
        /// </summary>
        [DefaultValue(ViewStateEnum.SessionOnly), Category(Nexus.Web.UI.Page.category),
            Description("Indicates how the ViewState of the Page will be Tracked.  SessionOnly (Default), EachInstance, and UpLevelSingle.")]
        public ViewStateEnum ViewStateType
        {
            get { return viewStateType; }
            set { viewStateType = value; }
        }

        internal string ClientOnSubmitEvent
        {
            get
            {
                return PropertyInvoker<string>.GetValue(this, "ClientOnSubmitEvent");
                //if (!this.ClientScript.HasSubmitStatements && (((this.Form == null) || !this.Form.SubmitDisabledControls) || (this.EnabledControls.Count <= 0)))
                //{
                //    return string.Empty;
                //}
                //return "javascript:return WebForm_OnSubmit();";
            }
        }

        public Nexus.Web.UI.ClientScriptManager ClientScript
        {
            get
            {
                if (this._clientScriptManager == null)
                {
                    this._clientScriptManager = new Nexus.Web.UI.ClientScriptManager(this);
                }
                return this._clientScriptManager;
            }
        }

        internal bool ClientSupportsFocus
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "ClientSupportsFocus");
                //if (this._request == null)
                //{
                //    return false;
                //}
                //if (this._request.Browser.EcmaScriptVersion < FocusMinimumEcmaVersion)
                //{
                //    return (this._request.Browser.JScriptVersion >= FocusMinimumJScriptVersion);
                //}
                //return true;
            }
        }

        internal bool ClientSupportsJavaScript
        {
            get
            {
                //return PropertyInvoker<bool>.GetValue(this, "ClientSupportsJavaScript");
                if (!this._clientSupportsJavaScriptChecked)
                {
                    this._clientSupportsJavaScript = (this.Request != null) && (this.Request.Browser.EcmaScriptVersion >= JavascriptMinimumVersion);
                    this._clientSupportsJavaScriptChecked = true;
                }
                return this._clientSupportsJavaScript;
            }
        }

        internal bool ContainsCrossPagePost
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "ContainsCrossPagePost");
                //return this._containsCrossPagePost;
            }
            set
            {
                PropertyInvoker<bool>.SetValue(this, "ContainsCrossPagePost", value);
                //this._containsCrossPagePost = value;
            }
        }

        internal bool DesignMode
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "DesignMode");
                //return (base.Site != null) ? base.Site.DesignMode : false;
            }
        }

        internal ArrayList EnabledControls
        {
            get
            {
                return PropertyInvoker<ArrayList>.GetValue(this, "EnabledControls");
                //if (this._enabledControls == null)
                //{
                //    this._enabledControls = new ArrayList();
                //}
                //return this._enabledControls;
            }
        }

        public virtual bool EnableEventValidation
        {
            get
            {
                return base.EnableEventValidation;
            }
            set
            {
                base.EnableEventValidation = value;
            }
        }

        /// <summary>
        /// Mod zobrazeni
        /// </summary>
        internal bool EnableLegacyRendering
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "EnableLegacyRendering");
                //Page page = this.Page;
                //if (page != null)
                //{
                //    return (page.XhtmlConformanceMode == XhtmlConformanceMode.Legacy);
                //}
                //if (!this.DesignMode && (this.Adapter == null))
                //{
                //    return (this.GetXhtmlConformanceSection().Mode == XhtmlConformanceMode.Legacy);
                //}
                //return false;
            }
        }

        internal bool IsInOnFormRender
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "IsInOnFormRender");
                //return this._inOnFormRender;
            }
        }
 
        internal System.Collections.Stack PartialCachingControlStack
        {
            get
            {
                return PropertyInvoker<System.Collections.Stack>.GetValue(this, "PartialCachingControlStack");
                //return this._partialCachingControlStack;
            }
        }

        internal bool RenderFocusScript
        {
            get
            {
                return PropertyInvoker<bool>.GetValue(this, "RenderFocusScript");
                //return this._requireFocusScript;
            }
        }

        internal NameValueCollection RequestValueCollection
        {
            get
            {
                return PropertyInvoker<NameValueCollection>.GetValue(this, "RequestValueCollection");
                //return this._requestValueCollection;
            }
        }

        internal System.Web.HttpRequest RequestInternal
        {
            get
            {
                return PropertyInvoker<System.Web.HttpRequest>.GetValue(this, "RequestInternal");
                //return this._request;
            }
        }

        internal string RequestViewStateString
        {
            get
            {
                return PropertyInvoker<string>.GetValue(this, "RequestViewStateString");
                //if (!this._cachedRequestViewState)
                //{
                //    StringBuilder builder = new StringBuilder();
                //    try
                //    {
                //        if (this.RequestValueCollection != null)
                //        {
                //            string text = this.RequestValueCollection["__VIEWSTATEFIELDCOUNT"];
                //            if ((this.MaxPageStateFieldLength == -1) || (text == null))
                //            {
                //                this._cachedRequestViewState = true;
                //                this._requestViewState = this.RequestValueCollection["__VIEWSTATE"];
                //                return this._requestViewState;
                //            }
                //            int num = Convert.ToInt32(text, CultureInfo.InvariantCulture);
                //            if (num < 0)
                //            {
                //                throw new HttpException(SR.GetString("ViewState_InvalidViewState"));
                //            }
                //            for (int i = 0; i < num; i++)
                //            {
                //                string text2 = "__VIEWSTATE";
                //                if (i > 0)
                //                {
                //                    text2 = text2 + i.ToString(CultureInfo.InvariantCulture);
                //                }
                //                string text3 = this.RequestValueCollection[text2];
                //                if (text3 == null)
                //                {
                //                    throw new HttpException(SR.GetString("ViewState_MissingViewStateField", new object[] { text2 }));
                //                }
                //                builder.Append(text3);
                //            }
                //        }
                //        this._cachedRequestViewState = true;
                //        this._requestViewState = builder.ToString();
                //    }
                //    catch (Exception exception)
                //    {
                //        ViewStateException.ThrowViewStateError(exception, builder.ToString());
                //    }
                //}
                //return this._requestViewState;
            }
        }






        /// <summary>
        /// Zjisteni zda stranka obsahuje asp.net HtmlForm
        /// </summary>
        /// <param name="obj"></param>
        protected override void AddParsedSubObject(object obj)
        {
            if (obj is HtmlForm)
            {
                this.hasHtmlForm = true;
            }
            base.AddParsedSubObject(obj);
        }

        internal IStateFormatter CreateStateFormatter()
        {
            return MethodInvokerWithReturn<IStateFormatter>.InvokeAndReturn(this, "CreateStateFormatter", null, BindingFlags.NonPublic | BindingFlags.Instance);
            //return new ObjectStateFormatter(this, true);
        }

        protected override object LoadPageStateFromPersistenceMedium()
        {
            // Make sure ViewState is Available for All Forms
            if (this.ViewStateType == ViewStateEnum.SessionOnly)
            {
                return Session["ViewState"];
            }
            else if (this.hasNexusForms)
            {
                if (this.PostBackForm == "")
                {
                    return base.LoadPageStateFromPersistenceMedium();
                }
                else
                {
                    LosFormatter format = new LosFormatter();
                    return format.Deserialize(this.Request["__viewstate"]);
                }
            }
            else
            {
                return base.LoadPageStateFromPersistenceMedium();
            }
        }

        internal void LoadScrollPosition()
        {
            if (this.Request != null)
            {
                string s = this.Request[this._scrollPositionXID];
                if ((s != null) && !int.TryParse(s, out this._scrollPositionX))
                {
                    this._scrollPositionX = 0;
                }
                string text2 = this.Request[this._scrollPositionYID];
                if ((text2 != null) && !int.TryParse(text2, out this._scrollPositionY))
                {
                    this._scrollPositionY = 0;
                }
            }
        }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            // Make sure ViewState is Processed at All Times
            if (this.hasNexusForms && !this.hasHtmlForm)
            {
                this.RegisterViewStateHandler();
            }
        }

        internal void RegisterFocusScript()
        {
            MethodInvoker.Invoke(this, "RegisterFocusScript", null, BindingFlags.NonPublic | BindingFlags.Instance);
            //if (this.ClientSupportsFocus && !this._requireFocusScript)
            //{
            //    this.ClientScript.RegisterHiddenField("__LASTFOCUS", string.Empty);
            //    this._requireFocusScript = true;
            //    if (this._partialCachingControlStack != null)
            //    {
            //        foreach (BasePartialCachingControl control in this._partialCachingControlStack)
            //        {
            //            control.RegisterFocusScript();
            //        }
            //    }
            //}
        }

        internal void RegisterPostBackScript()
        {
            MethodInvoker.Invoke(this, "RegisterPostBackScript", null, BindingFlags.NonPublic | BindingFlags.Instance);
            //if (this.ClientSupportsJavaScript && !this._fPostBackScriptRendered)
            //{
            //    if (!this._fRequirePostBackScript)
            //    {
            //        this.ClientScript.RegisterHiddenField("__EVENTTARGET", string.Empty);
            //        this.ClientScript.RegisterHiddenField("__EVENTARGUMENT", string.Empty);
            //        this._fRequirePostBackScript = true;
            //    }
            //    if (this._partialCachingControlStack != null)
            //    {
            //        foreach (BasePartialCachingControl control in this._partialCachingControlStack)
            //        {
            //            control.RegisterPostBackScript();
            //        }
            //    }
            //}
        }

        internal void RegisterWebFormsScript()
        {
            MethodInvoker.Invoke(this, "RegisterWebFormsScript", null, BindingFlags.NonPublic | BindingFlags.Instance);
            //if (this.ClientSupportsJavaScript && !this._fWebFormsScriptRendered)
            //{
            //    this.RegisterPostBackScript();
            //    this._fRequireWebFormsScript = true;
            //    if (this._partialCachingControlStack != null)
            //    {
            //        foreach (BasePartialCachingControl control in this._partialCachingControlStack)
            //        {
            //            control.RegisterWebFormsScript();
            //        }
            //    }
            //}
        }

        protected override void SavePageStateToPersistenceMedium(object viewState)
        {
            // Make sure ViewState is Available for All Forms
            if (this.ViewStateType == ViewStateEnum.SessionOnly)
            {
                Session["ViewState"] = viewState;
                // Bug requires Hidden Form Field __ViewState
                if (this.hasHtmlForm)
                {
                    //this.ClientScript.RegisterHiddenField("__viewstate", "");
                }
            }
            else if (this.hasNexusForms)
            {
                LosFormatter format = new LosFormatter();
                StringWriter writer = new StringWriter();
                format.Serialize(writer, viewState);
                this.viewState = writer.ToString();
                base.SavePageStateToPersistenceMedium(viewState);
            }
            else
            {
                base.SavePageStateToPersistenceMedium(viewState);
            }
        }

        public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
        {
            // Allows WebControls to be Used in Nexus.Form
            if (!this.inServerForm)
            {
                base.VerifyRenderingInServerForm(control);
            }
        }
    }
}
