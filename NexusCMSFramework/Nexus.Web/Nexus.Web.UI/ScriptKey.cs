namespace Nexus.Web.UI
{
    using System;
    using System.Reflection;
    using System.Web.Util;

    internal class ScriptKey
    {
        private bool _isInclude;
        private string _key;
        private Type _type;

        internal ScriptKey(Type type, string key) : this(type, key, false)
        {
        }

        internal ScriptKey(Type type, string key, bool isInclude)
        {
            this._type = type;
            if (string.IsNullOrEmpty(key))
            {
                key = null;
            }
            this._key = key;
            this._isInclude = isInclude;
        }

        public override bool Equals(object o)
        {
            ScriptKey key = (ScriptKey) o;
            if ((key._type == this._type) && (key._key == this._key))
            {
                return (key._isInclude == this._isInclude);
            }
            return false;
        }

        public override int GetHashCode()
        {
            Type hashCodeCombiner = Nexus.Reflection.ReflectionHelper.GetFrameworkTypeAssembly("System.Web", "System.Web.Util.HashCodeCombiner");
            return Nexus.Reflection.MethodInvokerWithReturn<int>.InvokeAndReturn(hashCodeCombiner, "CombineHashCodes"
                , new object[] { this._type.GetHashCode(), this._key.GetHashCode(), this._isInclude.GetHashCode() }, BindingFlags.Static | BindingFlags.NonPublic);

            //return HashCodeCombiner.CombineHashCodes(this._type.GetHashCode(), this._key.GetHashCode(), this._isInclude.GetHashCode());
        }
    }
}

