using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace DevTools
{
    public class MethodDescription
    {
        public string Comment { get; set; }
        public string ReturnSample { get; set; }
        public string ParamsSample { get; set; }
        public string Uri { get; set; }
        public string UnitOfWorkCall { get; set; }
        public string Method { get; set; }
        public string HttpMethod { get; set; }
    }
    public class ControllerDescription
    {
        public string ControllerName { get; set; }
        public IEnumerable<MethodDescription> Descriptions { get; set; }
        public string AngularController { get; set; }
    }
    public static class ControllerExtension
    {
        //private static string GetComment(this MethodInfo method)
        //{

        //}
        private static string GetUri(Attribute attribute, string controller)
        {
            var routeAttribute = attribute as RouteAttribute;
            return routeAttribute == null ? $"api/{controller}" : routeAttribute.Template;
        }

        public static string GetJsonSample(this Type type)
        {

            if (type == null) return "null";
            if (type == typeof(Task)) return "";
            if (type.Name.StartsWith("Task"))
                type = type.GetGenericArguments()[0];
            var isString = type == typeof(string) || type == typeof(String);
            if (isString)
                return "\"Sample string \"";
            if (type.Name.StartsWith("Nullable")) return "null";
            if (type == typeof(DateTime))
                return DateTime.Now.ToString();
            var isPrimitive = type.IsPrimitive;
            if (isPrimitive)
            {
                if (typeof(bool) == type) return "false";
                if (typeof(decimal) == type) return "0.0";
                if (typeof(float) == type) return "0.0";
                if (typeof(double) == type) return "0.0";
                if (typeof(long) == type) return "0";
                if (typeof(int) == type) return "0";
                return typeof(short) == type ? "0" : type.FullName;
            }
            var isArray = type.IsArray || type.FullName.Contains("IEnumerable") || type.FullName.Contains("IQueryable");
            if (!isArray) return "{" + string.Join(",", type.GetProperties()
                .Where(t => t.GetCustomAttribute(typeof(JsonIgnoreAttribute), true) == null)
                .Select(t => t.Name + ":" + t.PropertyType.GetJsonSample())) + "}";
            return "[" + string.Join(",", type.GetGenericArguments().Select(x => x.GetJsonSample())) + "]";
        }

        public static string GetHttpMethod(this MethodInfo methodInfo)
        {
            if (methodInfo == null) return "";
            if (methodInfo.GetCustomAttribute(typeof(HttpGetAttribute)) != null) return "Get";
            if (methodInfo.GetCustomAttribute(typeof(HttpPostAttribute)) != null) return "Post";
            if (methodInfo.GetCustomAttribute(typeof(HttpPutAttribute)) != null) return "Put";
            return methodInfo.GetCustomAttribute(typeof(HttpDeleteAttribute)) != null ? "Delete" : "";
        }

        public static string GetUnitOfWorkCall(this MethodInfo methodInfo, string url, string controllerName)
        {
            var method = methodInfo.GetHttpMethod();
            if (method == "") return "No aplica";
            //Get and Delete calls
            var uri = url.Replace("api/" + controllerName, "").Split('/').Where(t => !string.IsNullOrEmpty(t)).Select(t => t.StartsWith("{") ? t : $"\"{t}\"");
            if (new[] { "Delete", "Get" }.Contains(method))
                return $"unitOfWork.{controllerName}.complex{method}([{string.Join(",", uri)}]).success(function(data)" +
                    "{});";
            //Post and Put calls
            return $"unitOfWork.{controllerName}.complex{method}([{string.Join(",", uri)}],{methodInfo.GetParameters()[0].ParameterType.GetJsonSample()}).success(function(data)" +
                "{});";
        }

        public static string GetController(this ControllerDescription controllerDescription, string angularModule)
        {
            var result = $"var app=angular.module(\"{angularModule}\");\n";
            result += $"app.controller('{controllerDescription.ControllerName}',[\n";
            result += "'unitOfWork', function(unitOfWork){\n";
            result += "\tvar vm=this;\n";
            foreach (var methodDescription in controllerDescription.Descriptions)
            {
                if (new[] { "Post", "Put" }.Contains(methodDescription.HttpMethod))
                    result += $"\tvm.{methodDescription.Method}Model = {methodDescription.ParamsSample};\n";
                result += $"\tvm.{methodDescription.Method} = function()";
                result += "{\n";
                if (new[] { "Post", "Put" }.Contains(methodDescription.HttpMethod))
                    result += $"\t\t{methodDescription.UnitOfWorkCall.Replace(methodDescription.ParamsSample, $"vm.{methodDescription.Method}Model")}\n";
                else
                    result += $"\t\t{methodDescription.UnitOfWorkCall}\n";
                result += "\t};\n";

            }
            result += "}]);\n";

            return result;
        }
        public static ControllerDescription GetDescriptions(this ApiController controller)
        {
            var result = new ControllerDescription
            {
                Descriptions = controller
                    .GetType()
                    .GetMethods()
                    .Where(t => typeof(ApiController).GetMethods().All(x => x.Name != t.Name))
                    .Select(t => new MethodDescription
                    {
                        Method = t.Name,
                        Uri =
                            GetUri(t.GetCustomAttribute(typeof(RouteAttribute)),
                                controller.GetType().Name.Replace("Controller", "")),
                        ReturnSample = t.ReturnType.GetJsonSample(),
                        HttpMethod = t.GetHttpMethod(),
                        UnitOfWorkCall =
                            t.GetUnitOfWorkCall(
                                GetUri(t.GetCustomAttribute(typeof(RouteAttribute)),
                                    controller.GetType().Name.Replace("Controller", "")),
                                controller.GetType().Name.Replace("Controller", "")),
                        ParamsSample = t.GetParameters().Any() ? t.GetParameters().Select(x => x.ParameterType.GetJsonSample()).FirstOrDefault() : null
                    }),
                ControllerName = controller.GetType().Name.Replace("Controller", ""),
            };
            result.AngularController = result.GetController("RDash");
            return result;
        }
    }
}