using System;

namespace DevTools.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependencyOfAttribute:Attribute
    {
        public Type Type { get; set; }
    }
}
