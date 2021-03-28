using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Saber.Common.Platform
{
    public static class PublicApi
    {
        private static List<Models.PublicApiInfo> list { get; set; } = new List<Models.PublicApiInfo>();
        public static bool Enabled { get; set; } = false;
        public static string Name { get; set; } = "Saber";
        
        public static List<Models.PublicApiInfo> GetList()
        {
            return list;
        }

        public static List<Models.PublicApiInfo> GetList(List<Assembly> assemblies)
        {
            //first, get all public api settings from the database
            var apis = Query.PublicApis.GetList();
            if(apis != null && apis.Any(a => a.api == Name))
            {
                Enabled = apis.Where(a => a.api == Name).First().enabled;
            }
            else
            {
                Query.PublicApis.Update(Name, false);
            }

            //then, get all platform-specific public APIs
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("Saber, Version"))
                {
                    list.AddRange(GetFromAssembly(assembly, apis, "Saber.Services"));
                }
            }

            //next get all Vendor public APIs from assemblies
            foreach(var vendor in Core.Vendors.Details)
            {
                vendor.PublicApis = GetFromType(vendor.Type, apis);
                list.AddRange(vendor.PublicApis);
            }

            return list;
        }

        public static void Update(string api, bool enabled)
        {
            try
            {
                list.Where(a => a.Path == api).First().Enabled = enabled;
                Query.PublicApis.Update(api, enabled);
            }
            catch (Exception)
            {

            }
        }

        public static void Clear()
        {
            list = new List<Models.PublicApiInfo>();
        }

        public static List<Models.PublicApiInfo> GetFromAssembly(Assembly assembly, List<Query.Models.PublicApi> apis = null, string nameSpace = "")
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
                results.AddRange(GetFromType(type, apis));
            }
            return results;
        }

        public static List<Models.PublicApiInfo> GetFromType(Type type, List<Query.Models.PublicApi> apis = null)
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
                    var path = type.Name + "/" + method.Name;
                    results.Add(new Models.PublicApiInfo()
                    {
                        Path = path,
                        Description = attr.Description,
                        Parameters = method.GetParameters().Select(a => new Models.PublicApiArgumentInfo()
                        {
                            Name = a.Name,
                            Index = i++,
                            DataType = GetDataType(a.ParameterType),
                            Description = attr.Parameters != null && attr.Parameters.Length >= i ? attr.Parameters[i - 1] : ""
                        }).ToList(),
                        Enabled = apis != null && apis.Any(a => a.api == path) ? apis.Where(a => a.api == path).First().enabled : false
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
