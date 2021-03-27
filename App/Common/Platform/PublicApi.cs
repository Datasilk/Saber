using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saber.Common.Platform
{
    public static class PublicApi
    {
        private static List<Models.PublicApiInfo> list { get; set; } = new List<Models.PublicApiInfo>();
        
        public static List<Models.PublicApiInfo> GetList()
        {
            return list;
        }

        public static List<Models.PublicApiInfo> GetList(List<Assembly> assemblies)
        {
            //first, get all platform-specific public APIs
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("Saber, Version"))
                {
                    list.AddRange(GetFromAssembly(assembly, "Saber.Services"));
                }
            }

            //next get all Vendor public APIs from assemblies
            foreach(var vendor in Core.Vendors.Details)
            {
                vendor.PublicApis = GetFromType(vendor.Type);
                list.AddRange(vendor.PublicApis);
            }

            return list;
        }

        public static List<Models.PublicApiInfo> GetFromAssembly(Assembly assembly, string nameSpace = "")
        {
            var results = new List<Models.PublicApiInfo>();
            var types = assembly.GetTypes()
                           .Where(type => typeof(Service).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
            if(nameSpace != "")
            {
                //filter types by namespace
                types = types.Where(a => a.FullName.StartsWith(nameSpace)).ToList();
            }
            foreach (var type in types)
            {
                results.AddRange(GetFromType(type));
            }
            return results;
        }

        public static List<Models.PublicApiInfo> GetFromType(Type type)
        {
            var results = new List<Models.PublicApiInfo>();
            //get public api methods from type
            var methods = type.GetMethods()
                .Where(a => a.GetCustomAttributes(typeof(Vendor.PublicApiAttribute), false).Length > 0);
            foreach (var method in methods)
            {
                var i = 0;
                var attributes = Attribute.GetCustomAttributes(method);
                if (attributes.Any(a => a is Vendor.PublicApiAttribute))
                {
                    var attr = (Vendor.PublicApiAttribute)attributes.Where(a => a is Vendor.PublicApiAttribute).First();
                    results.Add(new Models.PublicApiInfo()
                    {
                        Path = type.Name + "/" + method.Name,
                        Description = attr.Description,
                        Parameters = method.GetParameters().Select(a => new Models.PublicApiArgumentInfo()
                        {
                            Name = a.Name,
                            Index = i++,
                            DataType = GetDataType(a.ParameterType)
                        }).ToList()
                    });
                }
            }
            return results;
        }

        public static string GetDataType(Type parameter)
        {
            if (parameter.Name != "String")
            {
                if (parameter == typeof(Int32) || parameter == typeof(Int64) || parameter == typeof(int) || parameter == typeof(short))
                {
                    return "Int";
                }
                else if (parameter == typeof(float) || parameter == typeof(Decimal) || parameter == typeof(Single))
                {
                    return "Decimal";
                }
                else if (parameter.FullName.Contains("DateTime"))
                {
                    return "DateTime";
                }
                else if (parameter.IsArray)
                {
                    if (parameter.FullName == "System.Int32[]")
                    {
                        return "Int[]";
                    }
                    if (parameter.FullName.StartsWith("System.String[]"))
                    {
                        return "String[]";
                    }
                }
                else
                {
                    return parameter.Name;
                }
            }
            return "String";
        }
    }
}
