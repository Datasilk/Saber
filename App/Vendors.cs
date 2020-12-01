using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Saber.Vendor;
using System.Reflection;
using System.Text.Json;

namespace Saber
{
    public static class Vendors
    {
        private static List<string> DLLs { get; set; } = new List<string>();
        private static List<Assembly> Assemblies { get; set; } = new List<Assembly>();
        public static Dictionary<string, List<IVendorViewRenderer>> ViewRenderers { get; set; } = new Dictionary<string, List<IVendorViewRenderer>>();
        public static Dictionary<string, IVendorContentField> ContentFields { get; set; } = new Dictionary<string, IVendorContentField>();
        public static Dictionary<string, Type> Controllers { get; set; } = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Startups { get; set; } = new Dictionary<string, Type>();
        public static List<IVendorKeys> Keys { get; set; } = new List<IVendorKeys>();
        public static List<Type> HtmlComponents { get; set; } = new List<Type>();
        public static Dictionary<string, IVendorEmailClient> EmailClients { get; set; } = new Dictionary<string, IVendorEmailClient>();
        public static List<IVendorWebsiteSettings> WebsiteSettings { get; set; } = new List<IVendorWebsiteSettings>();

        private class AssemblyInfo
        {
            public string Assembly { get; set; }
            public string Version { get; set; }
        }

        public static string[] LoadDLLs()
        {
            //search Vendor folder for DLL files
            if (Directory.Exists(App.MapPath("/Vendors")))
            {
                RecurseDirectories(App.MapPath("/Vendors"));
            }
            //update JSON file with current versions of DLL files
            var versions = new List<AssemblyInfo>();
            var versionsChanged = false;
            if (File.Exists(App.MapPath("/Vendors/versions.json")))
            {
                versions = JsonSerializer.Deserialize<List<AssemblyInfo>>(File.ReadAllText(App.MapPath("/Vendors/versions.json")));
            }

            //load assemblies from DLL files
            foreach (var file in DLLs)
            {
                var context = new Common.Assemblies.AssemblyLoader(file);
                AssemblyName assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
                var assembly = context.LoadFromAssemblyName(assemblyName);
                Assemblies.Add(assembly);

                //check version of assembly
                assemblyName = assembly.GetName();
                var v = assemblyName.Version.ToString();
                var isnew = false;
                var i = versions.FindIndex(a => a.Assembly == assemblyName.Name);
                if (i >= 0)
                {
                    var v2 = versions[i].Version;
                    if(String.Compare(v, v2) < 0)
                    {
                        isnew = true;
                        versionsChanged = true;
                        versions[i] = new AssemblyInfo() { Assembly = versions[i].Assembly, Version = v };
                    }
                }
                else
                {
                    isnew = true;
                    versionsChanged = true;
                    versions.Add(new AssemblyInfo() { Assembly = assemblyName.Name, Version = v });
                }

                if (isnew)
                {
                    //copy any public vendor resource files to the wwwroot folder
                    var filename = Path.GetFileName(file);
                    var path = file.Replace(filename, "");
                    var relpath = path.Replace("\\", "/").Split("/Vendors/")[1];
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles().Where(Current => Regex.IsMatch(Current.Extension, "\\.(js|css)", RegexOptions.IgnoreCase));
                    var jsPath = "/wwwroot/editor/js/vendors/" + relpath.ToLower();
                    var cssPath = "/wwwroot/editor/css/vendors/" + relpath.ToLower();
                    var imagesPath = "/wwwroot/editor/images/vendors/" + relpath.ToLower();
                    if (files.Count() > 0)
                    {
                        //create wwwroot paths
                        if (!Directory.Exists(App.MapPath(jsPath)))
                        {
                            Directory.CreateDirectory(App.MapPath(jsPath));
                        }
                        if (!Directory.Exists(App.MapPath(cssPath)))
                        {
                            Directory.CreateDirectory(App.MapPath(cssPath));
                        }
                        if (!Directory.Exists(App.MapPath(imagesPath)))
                        {
                            Directory.CreateDirectory(App.MapPath(imagesPath));
                        }
                    }
                    foreach(var f in files)
                    {
                        //copy all required vendor resources
                        switch (f.Extension)
                        {
                            case ".js":
                                File.Copy(f.FullName, App.MapPath(jsPath + Path.GetFileName(f.FullName)), true);
                                break;
                            case ".css":
                                File.Copy(f.FullName, App.MapPath(cssPath + Path.GetFileName(f.FullName)), true);
                                break;
                            default:
                                if (Core.Image.Extensions.Any(a => a == f.Extension))
                                {
                                    //images
                                    File.Copy(f.FullName, App.MapPath(imagesPath + Path.GetFileName(f.FullName)), true);
                                }
                                break;
                        }
                    }
                }
            }
            if (versionsChanged)
            {
                //save versions to JSON
                File.WriteAllText(App.MapPath("/Vendors/versions.json"), JsonSerializer.Serialize(versions, new JsonSerializerOptions() { WriteIndented = true }));
            }

            return DLLs.ToArray();
        }

        private static void RecurseDirectories(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                DLLs.AddRange(dir.GetFiles(App.IsDocker ? "*.so" : "*.dll").Select(a => a.FullName).ToArray());
                foreach(var sub in dir.GetDirectories())
                {
                    RecurseDirectories(sub.FullName);
                }
            }
        }

        #region "View Renderers"
        public static void GetViewRenderersFromFileSystem()
        {
            foreach(var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorViewRenderer")
                        {
                            GetViewRendererFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetViewRendererFromType(Type type)
        {
            if(type == null) { return; }
            if (type.Equals(typeof(IVendorViewRenderer))) { return; }
            var attributes = type.GetCustomAttributes<ViewPathAttribute>();
            foreach (var attr in attributes)
            {
                if (!ViewRenderers.ContainsKey(attr.Path))
                {
                    ViewRenderers.Add(attr.Path, new List<IVendorViewRenderer>());
                }
                ViewRenderers[attr.Path].Add((IVendorViewRenderer)Activator.CreateInstance(type));
            }
        }
        #endregion

        #region "Content Fields"
        public static void GetContentFieldsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorContentField")
                        {
                            GetContentFieldsFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetContentFieldsFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorContentField))) { return; }
            var attributes = type.GetCustomAttributes<ContentFieldAttribute>();
            foreach (var attr in attributes)
            {
                ContentFields.Add(attr.FieldName, (IVendorContentField)Activator.CreateInstance(type));
            }
        }
        #endregion

        #region "Controllers"
        public static void GetControllersFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorController")
                        {
                            GetControllerFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetControllerFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorController))) { return; }
            Controllers.Add(type.Name.ToLower(), type);
        }
        #endregion

        #region "Startup"
        public static void GetStartupsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorStartup")
                        {
                            GetStartupFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetStartupFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorStartup))) { return; }
            Startups.Add(type.Assembly.GetName().Name, type);
        }
        #endregion

        #region "Security Keys"
        public static void GetSecurityKeysFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorKeys")
                        {
                            GetSecurityKeysFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetSecurityKeysFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorKeys))) { return; }
            Keys.Add((IVendorKeys)Activator.CreateInstance(type));
        }
        #endregion

        #region "Html Components"
        public static void GetHtmlComponentsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorHtmlComponent")
                        {
                            GetHtmlComponentsFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetHtmlComponentsFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorHtmlComponent))) { return; }
            HtmlComponents.Add(type);
        }
        #endregion

        #region "Email Clients"
        public static void GetEmailClientsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorEmailClient")
                        {
                            GetEmailClientsFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetEmailClientsFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorEmailClient))) { return; }
            var instance = (IVendorEmailClient)Activator.CreateInstance(type);
            EmailClients.Add(instance.Id, instance);
            instance.Init();
        }
        #endregion

        #region "Website Settings"
        public static void GetWebsiteSettingsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorWebsiteSettings")
                        {
                            GetWebsiteSettingsFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetWebsiteSettingsFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorWebsiteSettings))) { return; }
            var instance = (IVendorWebsiteSettings)Activator.CreateInstance(type);
            WebsiteSettings.Add(instance);
        }
        #endregion
    }
}
