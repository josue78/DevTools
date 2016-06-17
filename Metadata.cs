using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevTools.Attributes;
using Newtonsoft.Json;

namespace DevTools
{
    public class Property
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public InputTextAttribute InputText { get; set; }
        public FileUploadAttributte FileUpload { get; set; }
        public DependencyOfAttribute DependencyOf { get; set; }
    }
    public class ReadResult
    {
        public string MethodName { get; set; }
        public Type ReturnType { get; set; }
        public IEnumerable<Property> Properties { get; set; }
    }
    public class CrudResult
    {
        public ReadResult ReadResult { get; set; }

    }
    public static class Metadata
    {
        public static CrudResult GetMetadata(this object item)
        {
            var crud = item.GetType().GetCustomAttribute(typeof(CrudAttribute)) as CrudAttribute;
            if (crud == null) return null;
            var result = new CrudResult
            {
                ReadResult = crud.ReadResult()
            };
            return result;
        }
        public static string GetCrudMetadata(this object item)
        {
            var crud = item.GetType().GetCustomAttribute(typeof(CrudAttribute)) as CrudAttribute;
            if (crud == null) return null;
            var result = new CrudResult
            {
                ReadResult = crud.ReadResult()
            };


            return JsonConvert.SerializeObject(result);
        }

        private static ReadResult ReadResult(this object item)
        {
            var readMethod = item.GetType()
                .GetMethods()
                .FirstOrDefault(t => t.GetCustomAttribute(typeof(ReadAttribute)) != null);
            if (readMethod == null) return null;
            return new ReadResult
            {
                MethodName = readMethod.Name,
                ReturnType = readMethod.ReturnType,
                Properties = readMethod.ReturnType.GetProperties().Select(t => new Property
                {
                    Type = t.PropertyType,
                    Name = t.Name,
                    DependencyOf = t.GetCustomAttribute(typeof(DependencyOfAttribute)) as DependencyOfAttribute,
                    FileUpload = t.GetCustomAttribute(typeof(FileUploadAttributte)) as FileUploadAttributte,
                    InputText = t.GetCustomAttribute(typeof(InputTextAttribute)) as InputTextAttribute
                })
            };
        }
    }
}
