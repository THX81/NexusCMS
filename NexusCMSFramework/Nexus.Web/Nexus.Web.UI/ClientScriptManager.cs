namespace Nexus.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Configuration;
    using System.Reflection;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.Util;
    using Nexus.Web.UI;
    using Nexus.Reflection;

    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class ClientScriptManager
    {

        private const string _callbackFunctionName = "WebForm_DoCallback";
        private HybridDictionary _clientPostBackValidatedEventTable;
        private ArrayList _clientScriptBlocks;
        private bool _clientScriptBlocksInScriptTag;
        private bool _clientStartupScriptInScriptTag;
        private ArrayList _clientStartupScripts;
        private bool _eventValidationFieldLoaded;
        private Nexus.Web.UI.Page _owner;
        internal readonly string _postBackFunctionPrefix = "doPostBack_";
        private const string _postbackOptionsFunctionName = "WebForm_DoPostBackWithOptions";
        private IDictionary _registeredArrayDeclares;
        private ListDictionary _registeredClientScriptBlocks;
        private ListDictionary _registeredClientStartupScripts;
        private ListDictionary _registeredControlsWithExpandoAttributes;
        private IDictionary _registeredHiddenFields;
        private ListDictionary _registeredOnSubmitStatements;
        private ArrayList _validEventReferences;
        internal const string ClientScriptStartXhtml = "\r\n<script type=\"text/javascript\">\r\n/* <![CDATA[ */\r\n";
        internal const string ClientScriptEndXhtml = "/* ]]> */\r\n</script>\r\n";
        internal const string ClientScriptStart = "\r\n<script type=\"text/javascript\">\r\n<!--\r\n";
        internal const string ClientScriptEnd = "// -->\r\n</script>\r\n";
        internal const string IncludeScriptBegin = "\r\n<script src=\"";
        internal const string IncludeScriptEnd = "\" type=\"text/javascript\"></script>";
        internal const string JscriptPrefix = "javascript:";
        private const string PageCallbackScriptKey = "PageCallbackScript";



        internal ClientScriptManager(Nexus.Web.UI.Page owner)
        {
            this._owner = owner;
        }




        internal void ClearHiddenFields()
        {
            this._registeredHiddenFields = null;
        }

        private static int ComputeHashKey(string uniqueId, string argument)
        {
            Type stringUtil = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.Util.StringUtil");
            if (string.IsNullOrEmpty(argument))
            {
                return MethodInvokerWithReturn<int>.InvokeAndReturn(stringUtil, "GetStringHashCode"
                    , new object[] { uniqueId }, BindingFlags.Static | BindingFlags.NonPublic);
                //return StringUtil.GetStringHashCode(uniqueId);
            }
            int var1 = MethodInvokerWithReturn<int>.InvokeAndReturn(stringUtil, "GetStringHashCode"
                , new object[] { uniqueId }, BindingFlags.Static | BindingFlags.NonPublic);
            int var2 = MethodInvokerWithReturn<int>.InvokeAndReturn(stringUtil, "GetStringHashCode"
                , new object[] { argument }, BindingFlags.Static | BindingFlags.NonPublic);
            return (var1 ^ var2);
        }

        internal static ScriptKey CreateScriptIncludeKey(Type type, string key)
        {
            return new ScriptKey(type, key, true);
        }

        internal static ScriptKey CreateScriptKey(Type type, string key)
        {
            return new ScriptKey(type, key);
        }

        private void EnsureEventValidationFieldLoaded()
        {
            if (!this._eventValidationFieldLoaded)
            {
                this._eventValidationFieldLoaded = true;
                string text = null;
                if (this._owner.RequestValueCollection != null)
                {
                    text = this._owner.RequestValueCollection["__eventvalidation"];
                }
                if (!string.IsNullOrEmpty(text))
                {
                    System.Web.UI.IStateFormatter formatter = this._owner.CreateStateFormatter();
                    ArrayList list = null;
                    try
                    {
                        list = formatter.Deserialize(text) as ArrayList;
                    }
                    catch (Exception exception)
                    {
                        MethodInvoker.Invoke(typeof(System.Web.UI.ViewStateException), "ThrowViewStateError", new object[] { exception, text }, BindingFlags.NonPublic | BindingFlags.Static);
                        //System.Web.UI.ViewStateException.ThrowViewStateError(exception, text);
                    }
                    if ((list != null) && (list.Count >= 1))
                    {
                        int num = (int)list[0];
                        string requestViewStateString = this._owner.RequestViewStateString;
                        
                        Type stringUtil = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.Util.StringUtil");
                        int n = MethodInvokerWithReturn<int>.InvokeAndReturn(stringUtil, "GetStringHashCode"
                            , new object[] { requestViewStateString }, BindingFlags.Static | BindingFlags.NonPublic);

                        if (num != n)
                        {
                            MethodInvoker.Invoke(typeof(System.Web.UI.ViewStateException), "ThrowViewStateError", new object[] { null, text }, BindingFlags.NonPublic | BindingFlags.Static);
                            //System.Web.UI.ViewStateException.ThrowViewStateError(null, text);
                        }
                        this._clientPostBackValidatedEventTable = new HybridDictionary(list.Count - 1, true);
                        for (int i = 1; i < list.Count; i++)
                        {
                            int num3 = (int)list[i];
                            this._clientPostBackValidatedEventTable[num3] = null;
                        }
                        if (this._owner.IsCallback)
                        {
                            this._validEventReferences = list;
                        }
                    }
                }
            }
        }

        public string GetCallbackEventReference(System.Web.UI.Control control, string argument, string clientCallback, string context)
        {
            return this.GetCallbackEventReference(control, argument, clientCallback, context, false);
        }

        public string GetCallbackEventReference(System.Web.UI.Control control, string argument, string clientCallback, string context, bool useAsync)
        {
            return this.GetCallbackEventReference(control, argument, clientCallback, context, null, useAsync);
        }

        public string GetCallbackEventReference(string target, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
        {
            this._owner.RegisterWebFormsScript();
            if ((this._owner.ClientSupportsJavaScript && (this._owner.RequestInternal != null)) && this._owner.RequestInternal.Browser.SupportsCallback)
            {
                Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                String var = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                    , new object[] { this.GetWebResourceUrl(typeof(Page), "SmartNav.htm"), false }, BindingFlags.Static | BindingFlags.NonPublic);
                this.RegisterStartupScript(typeof(Page), "PageCallbackScript", ((this._owner.RequestInternal != null) && string.Equals(this._owner.RequestInternal.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase)) ? ("\r\nvar callBackFrameUrl='" + var + "';\r\nWebForm_InitCallback();") : "\r\nWebForm_InitCallback();", true);
            }
            if (argument == null)
            {
                argument = "null";
            }
            else if (argument.Length == 0)
            {
                argument = "\"\"";
            }
            if (context == null)
            {
                context = "null";
            }
            else if (context.Length == 0)
            {
                context = "\"\"";
            }
            return ("WebForm_DoCallback(" + target + "," + argument + "," + clientCallback + "," + context + "," + ((clientErrorCallback == null) ? "null" : clientErrorCallback) + "," + (useAsync ? "true" : "false") + ")");
        }

        public string GetCallbackEventReference(System.Web.UI.Control control, string argument, string clientCallback, string context, string clientErrorCallback, bool useAsync)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (!(control is System.Web.UI.ICallbackEventHandler))
            {
                Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                    , new object[] { "Page_CallBackTargetInvalid", new object[] { control.UniqueID } }, BindingFlags.Static | BindingFlags.Public);
                throw new InvalidOperationException(m);
                //throw new InvalidOperationException(System.Web.SR.GetString("Page_CallBackTargetInvalid", new object[] { control.UniqueID }));
            }
            return this.GetCallbackEventReference("'" + control.UniqueID + "'", argument, clientCallback, context, clientErrorCallback, useAsync);
        }

        internal string GetEventValidationFieldValue()
        {
            if ((this._validEventReferences != null) && (this._validEventReferences.Count != 0))
            {
                return this._owner.CreateStateFormatter().Serialize(this._validEventReferences);
            }
            return string.Empty;
        }

        public string GetPostBackClientHyperlink(System.Web.UI.Control control, string argument)
        {
            return this.GetPostBackClientHyperlink(control, argument, true, false);
        }

        public string GetPostBackClientHyperlink(System.Web.UI.Control control, string argument, bool registerForEventValidation)
        {
            return this.GetPostBackClientHyperlink(control, argument, true, registerForEventValidation);
        }

        internal string GetPostBackClientHyperlink(System.Web.UI.Control control, string argument, bool escapePercent, bool registerForEventValidation)
        {
            return (JscriptPrefix + this.GetPostBackEventReference(control, argument, escapePercent, registerForEventValidation));
        }

        public string GetPostBackEventReference(System.Web.UI.PostBackOptions options)
        {
            return this.GetPostBackEventReference(options, false);
        }

        public string GetPostBackEventReference(System.Web.UI.Control control, string argument)
        {
            return this.GetPostBackEventReference(control, argument, false, false);
        }

        public string GetPostBackEventReference(System.Web.UI.PostBackOptions options, bool registerForEventValidation)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (registerForEventValidation)
            {
                this.RegisterForEventValidation(options);
            }
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (options.RequiresJavaScriptProtocol)
            {
                builder.Append(JscriptPrefix);
            }
            if (options.AutoPostBack)
            {
                builder.Append("setTimeout('");
            }
            if ((!options.PerformValidation && !options.TrackFocus) && (options.ClientSubmit && string.IsNullOrEmpty(options.ActionUrl)))
            {
                string postBackEventReference = this.GetPostBackEventReference(options.TargetControl, options.Argument);
                if (options.AutoPostBack)
                {
                    Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                    String var = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                        , new object[] { postBackEventReference }, BindingFlags.Static | BindingFlags.NonPublic);
                    builder.Append(var);
                    builder.Append("', 0)");
                }
                else
                {
                    builder.Append(postBackEventReference);
                }
                return builder.ToString();
            }
            builder.Append("WebForm_DoPostBackWithOptions");
            builder.Append("(new WebForm_PostBackOptions(\"");
            builder.Append(options.TargetControl.UniqueID);
            builder.Append("\", ");
            if (string.IsNullOrEmpty(options.Argument))
            {
                builder.Append("\"\", ");
            }
            else
            {
                builder.Append("\"");
                Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                String var = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                    , new object[] { options.Argument }, BindingFlags.Static | BindingFlags.NonPublic);
                builder.Append(var);
                builder.Append("\", ");
            }
            if (options.PerformValidation)
            {
                flag = true;
                builder.Append("true, ");
            }
            else
            {
                builder.Append("false, ");
            }
            if ((options.ValidationGroup != null) && (options.ValidationGroup.Length > 0))
            {
                flag = true;
                builder.Append("\"");
                builder.Append(options.ValidationGroup);
                builder.Append("\", ");
            }
            else
            {
                builder.Append("\"\", ");
            }
            if ((options.ActionUrl != null) && (options.ActionUrl.Length > 0))
            {
                flag = true;
                this._owner.ContainsCrossPagePost = true;
                builder.Append("\"");
                Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                String var = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                    , new object[] { options.ActionUrl }, BindingFlags.Static | BindingFlags.NonPublic);
                builder.Append(var);
                builder.Append("\", ");
            }
            else
            {
                builder.Append("\"\", ");
            }
            if (options.TrackFocus)
            {
                this._owner.RegisterFocusScript();
                flag = true;
                builder.Append("true, ");
            }
            else
            {
                builder.Append("false, ");
            }
            if (options.ClientSubmit)
            {
                flag = true;
                this._owner.RegisterPostBackScript();
                builder.Append("true))");
            }
            else
            {
                builder.Append("false))");
            }
            if (options.AutoPostBack)
            {
                builder.Append("', 0)");
            }
            string text2 = null;
            if (flag)
            {
                text2 = builder.ToString();
                this._owner.RegisterWebFormsScript();
            }
            return text2;
        }

        public string GetPostBackEventReference(System.Web.UI.Control control, string argument, bool registerForEventValidation)
        {
            return this.GetPostBackEventReference(control, argument, false, registerForEventValidation);
        }

        private string GetPostBackEventReference(System.Web.UI.Control control, string argument, bool forUrl, bool registerForEventValidation)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            this._owner.RegisterPostBackScript();
            string uniqueID = control.UniqueID;
            if (registerForEventValidation)
            {
                this.RegisterForEventValidation(uniqueID, argument);
            }
            bool enableLegacyRendering = PropertyInvoker<bool>.GetValue(control, "EnableLegacyRendering");
            if ((enableLegacyRendering && this._owner.IsInOnFormRender) && ((uniqueID != null) && (uniqueID.IndexOf(':') >= 0)))
            {
                uniqueID = uniqueID.Replace(':', '$');
            }

            Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
            String var = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                , new object[] { argument, forUrl }, BindingFlags.Static | BindingFlags.NonPublic);
            return (("__doPostBack('" + uniqueID + "','") + var + "')");
        }

        public string GetWebResourceUrl(Type type, string resourceName)
        {
            return GetWebResourceUrl(this._owner, type, resourceName, false);
        }

        internal static string GetWebResourceUrl(Page owner, Type type, string resourceName, bool htmlEncoded)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException("resourceName");
            }
            if ((owner == null) || !owner.DesignMode)
            {
                return MethodInvokerWithReturn<String>.InvokeAndReturn(typeof(AssemblyResourceLoader)
                                                                                                                                                , "GetWebResourceUrl"
                                                                                                                                                , new object[] { type, resourceName, htmlEncoded }
                                                                                                                                                , BindingFlags.NonPublic | BindingFlags.Static);
            }
            ISite site = owner.Site;
            if (site != null)
            {
                System.Web.UI.IResourceUrlGenerator service = site.GetService(typeof(System.Web.UI.IResourceUrlGenerator)) as System.Web.UI.IResourceUrlGenerator;
                if (service != null)
                {
                    return service.GetResourceUrl(type, resourceName);
                }
            }
            return resourceName;
        }

        internal bool HasRegisteredHiddenFields
        {
            get
            {
                if (this._registeredHiddenFields != null)
                {
                    return (this._registeredHiddenFields.Count > 0);
                }
                return false;
            }
        }

        internal bool HasSubmitStatements
        {
            get
            {
                if (this._registeredOnSubmitStatements != null)
                {
                    return (this._registeredOnSubmitStatements.Count > 0);
                }
                return false;
            }
        }

        public bool IsClientScriptBlockRegistered(string key)
        {
            return this.IsClientScriptBlockRegistered(typeof(Page), key);
        }

        public bool IsClientScriptBlockRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            ScriptKey key2 = CreateScriptKey(type, key);
            if (this._registeredClientScriptBlocks != null)
            {
                return (this._registeredClientScriptBlocks[key2] != null);
            }
            return false;
        }

        public bool IsClientScriptIncludeRegistered(string key)
        {
            return this.IsClientScriptIncludeRegistered(typeof(Nexus.Web.UI.Page), key);
        }

        public bool IsClientScriptIncludeRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this._registeredClientScriptBlocks != null)
            {
                return (this._registeredClientScriptBlocks[CreateScriptIncludeKey(type, key)] != null);
            }
            return false;
        }

        public bool IsOnSubmitStatementRegistered(string key)
        {
            return this.IsOnSubmitStatementRegistered(typeof(Page), key);
        }

        public bool IsOnSubmitStatementRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this._registeredOnSubmitStatements != null)
            {
                return (this._registeredOnSubmitStatements[CreateScriptKey(type, key)] != null);
            }
            return false;
        }

        public bool IsStartupScriptRegistered(string key)
        {
            return this.IsStartupScriptRegistered(typeof(Page), key);
        }

        public bool IsStartupScriptRegistered(Type type, string key)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this._registeredClientStartupScripts != null)
            {
                return (this._registeredClientStartupScripts[CreateScriptKey(type, key)] != null);
            }
            return false;
        }

        public void RegisterArrayDeclaration(string arrayName, string arrayValue)
        {
            if (arrayName == null)
            {
                throw new ArgumentNullException("arrayName");
            }
            if (this._registeredArrayDeclares == null)
            {
                this._registeredArrayDeclares = new ListDictionary();
            }
            if (!this._registeredArrayDeclares.Contains(arrayName))
            {
                this._registeredArrayDeclares[arrayName] = new ArrayList();
            }
            ((ArrayList)this._registeredArrayDeclares[arrayName]).Add(arrayValue);
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (System.Web.UI.BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    MethodInvoker.Invoke(control, "RegisterArrayDeclaration"
                        , new Type[] { typeof(string), typeof(string) }, new object[] { arrayName, arrayValue }, BindingFlags.Instance | BindingFlags.NonPublic);
                    //control.RegisterArrayDeclaration(arrayName, arrayValue);
                }
            }
        }

        public void RegisterClientScriptBlock(Type type, string key, string script)
        {
            this.RegisterClientScriptBlock(type, key, script, false);
        }

        public void RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (addScriptTags)
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientScriptBlocksWithoutTags);
            }
            else
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientScriptBlocks);
            }
        }

        public void RegisterClientScriptInclude(string key, string url)
        {
            this.RegisterClientScriptInclude(typeof(Page), key, url);
        }

        public void RegisterClientScriptResource(Type type, string resourceName)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.RegisterClientScriptInclude(type, resourceName, this.GetWebResourceUrl(type, resourceName));
        }

        public void RegisterClientScriptInclude(Type type, string key, string url)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(url))
            {
                Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                    , new object[] { "Parameter_NullOrEmpty", new object[] { "url" } }, BindingFlags.Static | BindingFlags.Public);
                throw new ArgumentException(m);
            }
            string script = IncludeScriptBegin + HttpUtility.HtmlAttributeEncode(url) + IncludeScriptEnd;
            this.RegisterScriptBlock(CreateScriptIncludeKey(type, key), script, ClientAPIRegisterType.ClientScriptBlocks);
        }

        internal void RegisterDefaultButtonScript(System.Web.UI.Control button, System.Web.UI.HtmlTextWriter writer, bool useAddAttribute)
        {
            //////////this._owner.RegisterWebFormsScript();
            //////////if (this._owner.EnableLegacyRendering)
            //////////{
            //////////    if (useAddAttribute)
            //////////    {
            //////////        writer.AddAttribute("language", "javascript", false);
            //////////    }
            //////////    else
            //////////    {
            //////////        writer.WriteAttribute("language", "javascript", false);
            //////////    }
            //////////}
            //////////string text = "javascript:return WebForm_FireDefaultButton(event, '" + button.ClientID + "')";
            //////////if (useAddAttribute)
            //////////{
            //////////    writer.AddAttribute("onkeypress", text);
            //////////}
            //////////else
            //////////{
            //////////    writer.WriteAttribute("onkeypress", text);
            //////////}
        }

        public void RegisterExpandoAttribute(string controlId, string attributeName, string attributeValue)
        {
            this.RegisterExpandoAttribute(controlId, attributeName, attributeValue, true);
        }

        public void RegisterExpandoAttribute(string controlId, string attributeName, string attributeValue, bool encode)
        {
            Type stringUtil = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.Util.StringUtil");

            MethodInvoker.Invoke(stringUtil, "CheckAndTrimString"
                , new Type[] { typeof(string), typeof(string) }, new object[] { controlId, "controlId" }, BindingFlags.Static | BindingFlags.NonPublic);
            MethodInvoker.Invoke(stringUtil, "CheckAndTrimString"
                , new Type[] { typeof(string), typeof(string) }, new object[] { attributeName, "attributeName" }, BindingFlags.Static | BindingFlags.NonPublic);

            ListDictionary dictionary = null;
            if (this._registeredControlsWithExpandoAttributes == null)
            {
                this._registeredControlsWithExpandoAttributes = new ListDictionary(StringComparer.Ordinal);
            }
            else
            {
                dictionary = (ListDictionary)this._registeredControlsWithExpandoAttributes[controlId];
            }
            if (dictionary == null)
            {
                dictionary = new ListDictionary(StringComparer.Ordinal);
                this._registeredControlsWithExpandoAttributes.Add(controlId, dictionary);
            }
            if (encode)
            {
                Type util = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.Util");
                attributeValue = MethodInvokerWithReturn<String>.InvokeAndReturn(util, "QuoteJScriptString"
                    , new Type[] { typeof(string) }, new object[] { attributeValue }, BindingFlags.Static | BindingFlags.NonPublic);
                //attributeValue = Util.QuoteJScriptString(attributeValue);
            }
            dictionary.Add(attributeName, attributeValue);
            if (this._owner.PartialCachingControlStack != null)
            {
                //Type basePartialCachingControl = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.UI.BasePartialCachingControl");
                foreach (System.Web.UI.BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    MethodInvoker.Invoke(control, "RegisterExpandoAttribute"
                        , new Type[] { typeof(string), typeof(string), typeof(string) }, new object[] { controlId, attributeName, attributeValue }, BindingFlags.Instance | BindingFlags.NonPublic);
                    //control.RegisterExpandoAttribute(controlId, attributeName, attributeValue);
                }
            }
        }

        public void RegisterValidator(System.Web.UI.WebControls.BaseValidator validator)
        {
            if (validator.ControlToValidate.Length > 0)
            {
                String var = MethodInvokerWithReturn<String>.InvokeAndReturn(validator, "GetControlRenderID"
                    , new Type[] { typeof(String) }, new object[] { validator.ControlToValidate }, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                RegisterExpandoAttribute(validator.ClientID, "controltovalidate", var, true);
            }
            if (validator.SetFocusOnError)
                RegisterExpandoAttribute(validator.ClientID, "focusOnError", "t", false);

            if (validator.ErrorMessage.Length > 0)
                RegisterExpandoAttribute(validator.ClientID, "errormessage", validator.ErrorMessage, true);

            System.Web.UI.WebControls.ValidatorDisplay enumValue = validator.Display;
            if (enumValue != System.Web.UI.WebControls.ValidatorDisplay.Static)
                RegisterExpandoAttribute(validator.ClientID, "display", System.Web.UI.PropertyConverter.EnumToString(typeof(System.Web.UI.WebControls.ValidatorDisplay), enumValue), false);

            if (!validator.IsValid)
                RegisterExpandoAttribute(validator.ClientID, "isvalid", "False", false);

            if (!validator.Enabled)
                RegisterExpandoAttribute(validator.ClientID, "enabled", "False", false);

            if (validator.ValidationGroup.Length > 0)
                RegisterExpandoAttribute(validator.ClientID, "validationGroup", validator.ValidationGroup, true);

            #region RequiredFieldValidator
            if (validator.GetType() == typeof(System.Web.UI.WebControls.RequiredFieldValidator))
            {
                System.Web.UI.WebControls.RequiredFieldValidator requiredFieldValidator = (validator as System.Web.UI.WebControls.RequiredFieldValidator);
                RegisterExpandoAttribute(requiredFieldValidator.ClientID, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
                RegisterExpandoAttribute(requiredFieldValidator.ClientID, "initialvalue", requiredFieldValidator.InitialValue);
            }
            #endregion
            #region RegularExpressionValidator
            if (validator.GetType() == typeof(System.Web.UI.WebControls.RegularExpressionValidator))
            {
                System.Web.UI.WebControls.RegularExpressionValidator regularExpressionValidator = (validator as System.Web.UI.WebControls.RegularExpressionValidator);
                RegisterExpandoAttribute(regularExpressionValidator.ClientID, "evaluationfunction", "RegularExpressionValidatorEvaluateIsValid", false);
                if (regularExpressionValidator.ValidationExpression.Length > 0)
                    RegisterExpandoAttribute(regularExpressionValidator.ClientID, "validationexpression", regularExpressionValidator.ValidationExpression);
            }
            #endregion
            #region RangeValidator
            if (validator.GetType() == typeof(System.Web.UI.WebControls.RangeValidator))
            {
                System.Web.UI.WebControls.RangeValidator rangeValidator = (validator as System.Web.UI.WebControls.RangeValidator);
                string maximumValue = rangeValidator.MaximumValue;
                string minimumValue = rangeValidator.MinimumValue;
                if (rangeValidator.CultureInvariantValues)
                {
                    maximumValue = MethodInvokerWithReturn<String>.InvokeAndReturn(rangeValidator, "ConvertCultureInvariantToCurrentCultureFormat"
                        , new Type[] { typeof(String), rangeValidator.Type.GetType() }, new object[] { maximumValue, rangeValidator.Type }
                        , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    minimumValue = MethodInvokerWithReturn<String>.InvokeAndReturn(rangeValidator, "ConvertCultureInvariantToCurrentCultureFormat"
                        , new Type[] { typeof(String), rangeValidator.Type.GetType() }, new object[] { minimumValue, rangeValidator.Type }
                        , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                RegisterExpandoAttribute(rangeValidator.ClientID, "evaluationfunction", "RangeValidatorEvaluateIsValid", false);
                RegisterExpandoAttribute(rangeValidator.ClientID, "maximumvalue", maximumValue);
                RegisterExpandoAttribute(rangeValidator.ClientID, "minimumvalue", minimumValue);
            }
            #endregion
            #region CustomValidator
            if (validator.GetType() == typeof(System.Web.UI.WebControls.CustomValidator))
            {
                System.Web.UI.WebControls.CustomValidator customValidator = (validator as System.Web.UI.WebControls.CustomValidator);
                RegisterExpandoAttribute(customValidator.ClientID, "evaluationfunction", "CustomValidatorEvaluateIsValid", false);
                if (customValidator.ClientValidationFunction.Length > 0)
                {
                    RegisterExpandoAttribute(customValidator.ClientID, "clientvalidationfunction", customValidator.ClientValidationFunction);
                    if (customValidator.ValidateEmptyText)
                    {
                        RegisterExpandoAttribute(customValidator.ClientID, "validateemptytext", "true", false);
                    }
                }
            }
            #endregion
            #region CompareValidator
            if (validator.GetType() == typeof(System.Web.UI.WebControls.CompareValidator))
            {
                System.Web.UI.WebControls.CompareValidator compareValidator = (validator as System.Web.UI.WebControls.CompareValidator);
                RegisterExpandoAttribute(compareValidator.ClientID, "evaluationfunction", "CompareValidatorEvaluateIsValid", false);
                if (compareValidator.ControlToCompare.Length > 0)
                {
                    string controlRenderID = MethodInvokerWithReturn<String>.InvokeAndReturn(compareValidator, "GetControlRenderID"
                        , new Type[] { typeof(String) }, new object[] { compareValidator.ControlToCompare }
                        , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    RegisterExpandoAttribute(compareValidator.ClientID, "controltocompare", controlRenderID);
                    RegisterExpandoAttribute(compareValidator.ClientID, "controlhookup", controlRenderID);
                }
                if (compareValidator.ValueToCompare.Length > 0)
                {
                    string valueToCompare = compareValidator.ValueToCompare;
                    if (compareValidator.CultureInvariantValues)
                    {
                        valueToCompare = MethodInvokerWithReturn<String>.InvokeAndReturn(compareValidator, "ConvertCultureInvariantToCurrentCultureFormat"
                            , new Type[] { typeof(String), compareValidator.Type.GetType() }, new object[] { valueToCompare, compareValidator.Type }
                        , System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    }
                    RegisterExpandoAttribute(compareValidator.ClientID, "valuetocompare", valueToCompare);
                }
                if (compareValidator.Operator != System.Web.UI.WebControls.ValidationCompareOperator.Equal)
                {
                    RegisterExpandoAttribute(compareValidator.ClientID, "operator", System.Web.UI.PropertyConverter.EnumToString(typeof(System.Web.UI.WebControls.ValidationCompareOperator), compareValidator.Operator), false);
                }
            }
            #endregion
        }

        public void RegisterForEventValidation(string uniqueId)
        {
            this.RegisterForEventValidation(uniqueId, string.Empty);
        }

        public void RegisterForEventValidation(System.Web.UI.PostBackOptions options)
        {
            this.RegisterForEventValidation(options.TargetControl.UniqueID, options.Argument);
        }

        public void RegisterForEventValidation(string uniqueId, string argument)
        {
            //////////////////if ((this._owner.EnableEventValidation && !this._owner.DesignMode) && !string.IsNullOrEmpty(uniqueId))
            //////////////////{
            //////////////////    if ((this._owner.ControlState < ControlState.PreRendered) && !this._owner.IsCallback)
            //////////////////    {
            //////////////////        throw new InvalidOperationException(System.Web.SR.GetString("ClientScriptManager_RegisterForEventValidation_Too_Early"));
            //////////////////    }
            //////////////////    int num = ComputeHashKey(uniqueId, argument);
            //////////////////    string clientState = this._owner.ClientState;
            //////////////////    if (clientState == null)
            //////////////////    {
            //////////////////        clientState = string.Empty;
            //////////////////    }
            //////////////////    if (this._validEventReferences == null)
            //////////////////    {
            //////////////////        if (this._owner.IsCallback)
            //////////////////        {
            //////////////////            this.EnsureEventValidationFieldLoaded();
            //////////////////        }
            //////////////////        else
            //////////////////        {
            //////////////////            this._validEventReferences = new ArrayList();
            //////////////////            this._validEventReferences.Add(StringUtil.GetStringHashCode(clientState));
            //////////////////        }
            //////////////////    }
            //////////////////    this._validEventReferences.Add(num);
            //////////////////    if (this._owner.PartialCachingControlStack != null)
            //////////////////    {
            //////////////////        foreach (BasePartialCachingControl control in this._owner.PartialCachingControlStack)
            //////////////////        {
            //////////////////            control.RegisterForEventValidation(uniqueId, argument);
            //////////////////        }
            //////////////////    }
            //////////////////}
        }

        /// <summary>Registers a hidden value with the <see cref="T:System.Web.UI.Page"></see> object.</summary>
        /// <param name="hiddenFieldInitialValue">The initial value of the field to register.</param>
        /// <param name="hiddenFieldName">The name of the hidden field to register.</param>
        public void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue)
        {
            if (hiddenFieldName == null)
            {
                throw new ArgumentNullException("hiddenFieldName");
            }
            if (this._registeredHiddenFields == null)
            {
                this._registeredHiddenFields = new ListDictionary();
            }
            if (!this._registeredHiddenFields.Contains(hiddenFieldName))
            {
                this._registeredHiddenFields.Add(hiddenFieldName, hiddenFieldInitialValue);
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (System.Web.UI.BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    MethodInvoker.Invoke(control, "RegisterHiddenField"
                        , new object[] { hiddenFieldName, hiddenFieldInitialValue }, BindingFlags.Instance | BindingFlags.NonPublic);
                    //control.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);
                }
            }
        }

        public void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.RegisterOnSubmitStatementInternal(CreateScriptKey(type, key), script);
        }

        internal void RegisterOnSubmitStatementInternal(ScriptKey key, string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                    , new object[] { "Parameter_NullOrEmpty", new object[] { "script" } }, BindingFlags.Static | BindingFlags.Public);
                throw new ArgumentException(m);
            }
            if (this._registeredOnSubmitStatements == null)
            {
                this._registeredOnSubmitStatements = new ListDictionary();
            }
            int index = script.Length - 1;
            while ((index >= 0) && char.IsWhiteSpace(script, index))
            {
                index--;
            }
            if ((index >= 0) && (script[index] != ';'))
            {
                script = script.Substring(0, index + 1) + ";" + script.Substring(index + 1);
            }
            if (this._registeredOnSubmitStatements[key] == null)
            {
                this._registeredOnSubmitStatements.Add(key, script);
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (System.Web.UI.BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    MethodInvoker.Invoke(control, "RegisterOnSubmitStatement"
                        , new object[] { key, script }, BindingFlags.Instance | BindingFlags.NonPublic);
                    //control.RegisterOnSubmitStatement(key, script);
                }
            }
        }

        internal void RegisterScriptBlock(ScriptKey key, string script, ClientAPIRegisterType type)
        {
            switch (type)
            {
                case ClientAPIRegisterType.ClientScriptBlocks:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientScriptBlocks, ref this._clientScriptBlocks, false, ref this._clientScriptBlocksInScriptTag);
                    break;

                case ClientAPIRegisterType.ClientScriptBlocksWithoutTags:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientScriptBlocks, ref this._clientScriptBlocks, true, ref this._clientScriptBlocksInScriptTag);
                    break;

                case ClientAPIRegisterType.ClientStartupScripts:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientStartupScripts, ref this._clientStartupScripts, false, ref this._clientStartupScriptInScriptTag);
                    break;

                case ClientAPIRegisterType.ClientStartupScriptsWithoutTags:
                    this.RegisterScriptBlock(key, script, ref this._registeredClientStartupScripts, ref this._clientStartupScripts, true, ref this._clientStartupScriptInScriptTag);
                    break;
            }
            if (this._owner.PartialCachingControlStack != null)
            {
                foreach (System.Web.UI.BasePartialCachingControl control in this._owner.PartialCachingControlStack)
                {
                    MethodInvoker.Invoke(control, "RegisterScriptBlock"
                        , new object[] { type, key, script }, BindingFlags.Instance | BindingFlags.NonPublic);
                    //control.RegisterScriptBlock(type, key, script);
                }
            }
        }

        private void RegisterScriptBlock(ScriptKey key, string script, ref ListDictionary scriptBlocks, ref ArrayList scriptList, bool needsScriptTags, ref bool inScriptBlock)
        {
            if (scriptBlocks == null)
            {
                scriptBlocks = new ListDictionary();
            }
            if (scriptBlocks[key] == null)
            {
                scriptBlocks.Add(key, script);
                if (scriptList == null)
                {
                    scriptList = new ArrayList();
                    if (needsScriptTags)
                    {
                        if (this._owner.EnableLegacyRendering)
                            scriptList.Add(ClientScriptStart);
                        else
                            scriptList.Add(ClientScriptStartXhtml);
                    }
                }
                else if (needsScriptTags)
                {
                    if (!inScriptBlock)
                    {
                        if (this._owner.EnableLegacyRendering)
                            scriptList.Add(ClientScriptStart);
                        else
                            scriptList.Add(ClientScriptStartXhtml);
                    }
                }
                else if (inScriptBlock)
                {
                    if (this._owner.EnableLegacyRendering)
                        scriptList.Add(ClientScriptEnd);
                    else
                        scriptList.Add(ClientScriptEndXhtml);
                }
                scriptList.Add(script);
                inScriptBlock = needsScriptTags;
            }
        }

        public void RegisterStartupScript(Type type, string key, string script)
        {
            this.RegisterStartupScript(type, key, script, false);
        }

        public void RegisterStartupScript(Type type, string key, string script, bool addScriptTags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (addScriptTags)
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientStartupScriptsWithoutTags);
            }
            else
            {
                this.RegisterScriptBlock(CreateScriptKey(type, key), script, ClientAPIRegisterType.ClientStartupScripts);
            }
        }

        internal void RenderArrayDeclares(System.Web.UI.HtmlTextWriter writer)
        {
            RenderArrayDeclares(writer, true);
        }
        internal void RenderArrayDeclares(System.Web.UI.HtmlTextWriter writer, bool withScriptTags)
        {
            if ((this._registeredArrayDeclares != null) && (this._registeredArrayDeclares.Count != 0))
            {
                if (withScriptTags)
                    RenderClientScriptStart(writer);
                IDictionaryEnumerator enumerator = this._registeredArrayDeclares.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    writer.Write("    var " + enumerator.Key + " =  new Array(");
                    IEnumerator enumerator2 = ((ArrayList)enumerator.Value).GetEnumerator();
                    bool flag = true;
                    while (enumerator2.MoveNext())
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            writer.Write(", ");
                        }
                        writer.Write(enumerator2.Current);
                    }
                    writer.WriteLine(");");
                }
                if (withScriptTags)
                    RenderClientScriptEnd(writer);
            }
        }

        internal void RenderClientScriptBlocks(System.Web.UI.HtmlTextWriter writer, Nexus.Web.UI.HtmlControls.Form form, bool withScriptTags)
        {
            if (this._clientScriptBlocks != null)
            {
                writer.WriteLine();
                foreach (string text in this._clientScriptBlocks)
                {
                    writer.Write(text);
                }
            }
            if (!string.IsNullOrEmpty(form.ClientOnSubmitEvent) && this._owner.ClientSupportsJavaScript)
            {
                if (!this._clientScriptBlocksInScriptTag)
                    writer.Write((this._owner.EnableLegacyRendering ? ClientScriptStart : ClientScriptStartXhtml));

                writer.Write("    function " + form.UniqueID + "_OnSubmit() {\r\n");
                if (HttpContext.Current.IsDebuggingEnabled)
                    writer.Write("        console.trace();\r\n");
                if (this._registeredOnSubmitStatements != null)
                {
                    foreach (string text2 in this._registeredOnSubmitStatements.Values)
                        writer.Write("        " + text2);

                }
                writer.WriteLine("\r\n        return true;\r\n}");
                writer.Write((this._owner.EnableLegacyRendering ? ClientScriptEnd : ClientScriptEndXhtml));
            }
            else if (this._clientScriptBlocksInScriptTag)
                writer.Write((this._owner.EnableLegacyRendering ? ClientScriptEnd : ClientScriptEndXhtml));

        }

        internal void RenderClientScriptResources(System.Web.UI.HtmlTextWriter writer)
        {
            if (this._clientScriptBlocks != null)
            {
                writer.WriteLine();
                foreach (string text in this._clientScriptBlocks)
                {
                    writer.Write(text);
                }
            }
        }

        internal void RenderClientScriptStart(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write((this._owner.EnableLegacyRendering ? ClientScriptStart : ClientScriptStartXhtml));
        }
        internal void RenderClientScriptEnd(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write((this._owner.EnableLegacyRendering ? ClientScriptEnd : ClientScriptEndXhtml));
        }

        internal void RenderOnSubmitStatementsFunction(System.Web.UI.HtmlTextWriter writer, Nexus.Web.UI.HtmlControls.Form form, bool withScriptTags)
        {
            if (!string.IsNullOrEmpty(form.ClientOnSubmitEvent) && this._owner.ClientSupportsJavaScript)
            {
                if (withScriptTags)
                    RenderClientScriptStart(writer);

                writer.Write("    function " + (!String.IsNullOrEmpty(form.ClientID) ? form.ClientID : "WebForm") + "_OnSubmit() {\r\n");
                if (HttpContext.Current.IsDebuggingEnabled)
                    writer.Write("        console.trace();\r\n");
                if (this._registeredOnSubmitStatements != null)
                {
                    foreach (string text2 in this._registeredOnSubmitStatements.Values)
                        writer.Write("    " + text2);

                }
                writer.WriteLine("\r\n        return true;\r\n    }");
                if (withScriptTags)
                    RenderClientScriptEnd(writer);
            }
        }

        internal void RenderClientStartupScripts(System.Web.UI.HtmlTextWriter writer, bool withScriptTags)
        {
            if (this._clientStartupScripts != null)
            {
                writer.WriteLine();
                if (withScriptTags)
                    RenderClientScriptStart(writer);
                
                foreach (string text in this._clientStartupScripts)
                    writer.Write(text);

                if (withScriptTags)
                    RenderClientScriptEnd(writer);
            }
        }

        internal void RenderExpandoAttribute(System.Web.UI.HtmlTextWriter writer)
        {
            RenderExpandoAttribute(writer, true);
        }
        internal void RenderExpandoAttribute(System.Web.UI.HtmlTextWriter writer, bool withScriptTags)
        {
            if ((this._registeredControlsWithExpandoAttributes != null) && (this._registeredControlsWithExpandoAttributes.Count != 0))
            {
                if (withScriptTags)
                    RenderClientScriptStart(writer);
                foreach (DictionaryEntry entry in this._registeredControlsWithExpandoAttributes)
                {
                    string key = (string)entry.Key;
                    writer.Write("    var " + key + " = document.all ? document.all[\"" + key + "\"] : document.getElementById(\"" + key);
                    writer.WriteLine("\");");
                    ListDictionary dictionary = (ListDictionary)entry.Value;
                    foreach (DictionaryEntry entry2 in dictionary)
                    {
                        writer.Write("    " + key + "." + entry2.Key);
                        if (entry2.Value == null)
                        {
                            writer.WriteLine(" = null;");
                            continue;
                        }
                        writer.Write(" = \"" + entry2.Value);
                        writer.WriteLine("\";");
                    }
                }
                if (withScriptTags)
                    RenderClientScriptEnd(writer);
            }
        }

        internal void RenderHiddenFields(System.Web.UI.HtmlTextWriter writer)
        {
            if ((this._registeredHiddenFields != null) && (this._registeredHiddenFields.Count != 0))
            {
                foreach (DictionaryEntry entry in this._registeredHiddenFields)
                {
                    string key = (string)entry.Key;
                    if (key == null)
                    {
                        key = string.Empty;
                    }
                    writer.WriteLine();
                    writer.Write("<input type=\"hidden\" name=\"");
                    writer.Write(key);
                    writer.Write("\" id=\"");
                    writer.Write(key);
                    writer.Write("\" value=\"");
                    HttpUtility.HtmlEncode((string)entry.Value, writer);
                    writer.Write("\" />");
                }
                this.ClearHiddenFields();
            }
        }

        internal void RenderPostBackScript(System.Web.UI.HtmlTextWriter writer, Nexus.Web.UI.HtmlControls.Form form, bool withScriptTags)
        {
            if (withScriptTags)
                RenderClientScriptStart(writer);
            
            //if (this._owner.PageAdapter != null)
            //    writer.Write("\r\n    var theForm = " + this._owner.PageAdapter.GetPostBackFormReference(formUniqueID) + ";");
            //else
            writer.Write("\r\n    var theForm = document.forms['" + form.UniqueID + "'];\r\n    if (!theForm) {\r\n        theForm = document." + form.UniqueID + ";\r\n    }");
            writer.WriteLine("\r\n    function " + _postBackFunctionPrefix + form.UniqueID + "(eventTarget, eventArgument) {\r\n" + (HttpContext.Current.IsDebuggingEnabled ? "        console.trace();\r\n" : String.Empty) + "        if (!theForm.onsubmit || (theForm.onsubmit() != false)) {\r\n            theForm.__eventtarget.value = eventTarget;\r\n            theForm.__eventargument.value = eventArgument;\r\n            theForm.submit();\r\n        }\r\n    }");
            form._fPostBackScriptRendered = true;

            if (withScriptTags)
                RenderClientScriptEnd(writer);
        }

        internal void RenderWebFormsScript(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(IncludeScriptBegin);
            writer.Write(GetWebResourceUrl(this._owner, typeof(Nexus.Web.UI.Page), "Nexus.Web.WebForms.js", true));
            writer.WriteLine(IncludeScriptEnd);
        }

        internal void RenderWebUIValidationScript(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(IncludeScriptBegin);
            writer.Write(GetWebResourceUrl(this._owner, typeof(Nexus.Web.UI.Page), "Nexus.Web.WebUIValidation.js", true));
            writer.WriteLine(IncludeScriptEnd);
        }

        internal void SaveEventValidationField()
        {
            string eventValidationFieldValue = this.GetEventValidationFieldValue();
            if (!string.IsNullOrEmpty(eventValidationFieldValue))
            {
                this.RegisterHiddenField("__eventvalidation", eventValidationFieldValue);
            }
        }

        public void ValidateEvent(string uniqueId)
        {
            this.ValidateEvent(uniqueId, string.Empty);
        }

        public void ValidateEvent(string uniqueId, string argument)
        {
            if (this._owner.EnableEventValidation)
            {
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                    String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                        , new object[] { "Parameter_NullOrEmpty", new object[] { "uniqueId" } }, BindingFlags.Static | BindingFlags.Public);
                    throw new ArgumentException(m, "uniqueId");
                    //throw new ArgumentException(System.Web.SR.GetString("Parameter_NullOrEmpty", new object[] { "uniqueId" }), "uniqueId");
                }
                this.EnsureEventValidationFieldLoaded();
                if (this._clientPostBackValidatedEventTable == null)
                {
                    Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                    String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                        , new object[] { "ClientScriptManager_InvalidPostBackArgument" }, BindingFlags.Static | BindingFlags.Public);
                    throw new ArgumentException(m);
                    //throw new ArgumentException(System.Web.SR.GetString("ClientScriptManager_InvalidPostBackArgument"));
                }
                int key = ComputeHashKey(uniqueId, argument);
                if (!this._clientPostBackValidatedEventTable.Contains(key))
                {
                    Type t = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                    String m = MethodInvokerWithReturn<String>.InvokeAndReturn(t, "GetString"
                        , new object[] { "ClientScriptManager_InvalidPostBackArgument" }, BindingFlags.Static | BindingFlags.Public);
                    throw new ArgumentException(m);
                    //throw new ArgumentException(System.Web.SR.GetString("ClientScriptManager_InvalidPostBackArgument"));
                }
            }
        }
    }
}
