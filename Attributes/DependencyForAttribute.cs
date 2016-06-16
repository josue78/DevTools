using System;

namespace DevTools.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DependencyForAttribute : Attribute
    {
        public Type Type { get; set; }
    }
}
