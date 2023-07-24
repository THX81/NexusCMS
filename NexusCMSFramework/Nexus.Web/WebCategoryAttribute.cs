namespace Nexus.Web
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class WebCategoryAttribute : CategoryAttribute
    {
        internal WebCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            string localizedString = base.GetLocalizedString(value);
            if (localizedString == null)
            {
                Type sr = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.SR");
                localizedString = Nexus.Reflection.MethodInvokerWithReturn<String>.InvokeAndReturn(sr, "GetString"
                    , new object[] { "Category_" + value }, BindingFlags.Instance | BindingFlags.NonPublic);
                //localizedString = System.Web.SR.GetString("Category_" + value);
            }
            return localizedString;
        }

        public override object TypeId
        {
            get
            {
                return typeof(CategoryAttribute);
            }
        }
    }
}

