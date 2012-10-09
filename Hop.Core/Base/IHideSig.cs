using System;
using System.ComponentModel;

namespace Hop.Core.Base
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableNameAttribute : Attribute
    {
        private readonly string _name;
        public string Name { get { return _name; } }

        public TableNameAttribute(string name)
        {
            _name = name;
        }
    }    
    
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IdAttribute : Attribute
    {
        public IdAttribute()
        {
        }
    }
    
    public interface IHideSig
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();
    }
}