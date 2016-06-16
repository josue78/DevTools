using System;

namespace DevTools.Attributes
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Property)]
    public class FileUploadAttributte:Attribute
    {
        public string Label { get; set; }
    }
}
