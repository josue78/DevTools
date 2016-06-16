using System;

namespace DevTools.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputTextAttribute : Attribute
    {
        public InputTextAttribute(string label, string property)
        {
            Label = label;
            Property = property;
        }
        public string Label { get; set; }
        public string Property { get; set; }
        public string Placeholder { get; set; }
    }
}
