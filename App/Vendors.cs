﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Saber.Vendor;
using System.Reflection;
using System.Text.Json;

namespace Saber.Common
{

    public static class Vendors
    {
        private static List<string> DLLs { get; set; } = new List<string>();
        private static List<KeyValuePair<string, Assembly>> Assemblies { get; set; } = new List<KeyValuePair<string, Assembly>>();
        private static List<string> Uninstalled { get; set; } = new List<string>();
        public static List<VendorInfo> Details { get; set; } = new List<VendorInfo>();
        public static Dictionary<string, List<IVendorViewRenderer>> ViewRenderers { get; set; } = new Dictionary<string, List<IVendorViewRenderer>>();
        public static Dictionary<string, Models.VendorContentFieldInfo> ContentFields { get; set; } = new Dictionary<string, Models.VendorContentFieldInfo>();
        public static Dictionary<string, Type> Controllers { get; set; } = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Startups { get; set; } = new Dictionary<string, Type>();
        public static List<IVendorKeys> Keys { get; set; } = new List<IVendorKeys>();
        public static Dictionary<string, HtmlComponentModel> HtmlComponents { get; set; } = new Dictionary<string, HtmlComponentModel>();
        public static string[] HtmlComponentKeys { get; set; }
        public static Dictionary<string, HtmlComponentModel> SpecialVars { get; set; } = new Dictionary<string, HtmlComponentModel>();
        public static Dictionary<string, IVendorEmailClient> EmailClients { get; set; } = new Dictionary<string, IVendorEmailClient>();
        public static Dictionary<string, EmailType> EmailTypes { get; set; } = new Dictionary<string, EmailType>();
        public static List<IVendorWebsiteSettings> WebsiteSettings { get; set; } = new List<IVendorWebsiteSettings>();

        public class VendorInfo : IVendorInfo
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public Vendor.Version Version { get; set; }
            public string DLL { get; set; }
            public string Assembly { get; set; }
            public string Path { get; set; }
            public Dictionary<string, List<IVendorViewRenderer>> ViewRenderers { get; set; } = new Dictionary<string, List<IVendorViewRenderer>>();
            public Dictionary<string, Models.VendorContentFieldInfo> ContentFields { get; set; } = new Dictionary<string, Models.VendorContentFieldInfo>();
            public Dictionary<string, Type> Controllers { get; set; } = new Dictionary<string, Type>();
            public Dictionary<string, Type> Services { get; set; } = new Dictionary<string, Type>();
            public Dictionary<string, Type> Startups { get; set; } = new Dictionary<string, Type>();
            public List<IVendorKeys> Keys { get; set; } = new List<IVendorKeys>();
            public Dictionary<string, HtmlComponentModel> HtmlComponents { get; set; } = new Dictionary<string, HtmlComponentModel>();
            public string[] HtmlComponentKeys { get; set; }
            public Dictionary<string, HtmlComponentModel> SpecialVars { get; set; } = new Dictionary<string, HtmlComponentModel>();
            public Dictionary<string, IVendorEmailClient> EmailClients { get; set; } = new Dictionary<string, IVendorEmailClient>();
            public Dictionary<string, EmailType> EmailTypes { get; set; } = new Dictionary<string, EmailType>();
            public List<IVendorWebsiteSettings> WebsiteSettings { get; set; } = new List<IVendorWebsiteSettings>();
        }
        #region "Assemblies"
        private class AssemblyInfo
        {
            public string Assembly { get; set; }
            public string Version { get; set; }
        }

        private static void RecurseDirectories(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                DLLs.AddRange(dir.GetFiles(App.IsDocker ? "*.so" : "*.dll").Select(a => a.FullName).ToArray());
                foreach (var sub in dir.GetDirectories())
                {
                    RecurseDirectories(sub.FullName);
                }
            }
        }

        public static string[] LoadDLLs()
        {
            //search Vendor folder for DLL files
            if (Directory.Exists(App.MapPath("/Vendors")))
            {
                RecurseDirectories(App.MapPath("/Vendors"));
                DLLs = DLLs.OrderBy(a => a).ToList();
            }

            //load assemblies from DLL files
            foreach (var file in DLLs)
            {
                var context = new Assemblies.AssemblyLoader(file);
                AssemblyName assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
                var assembly = context.LoadFromAssemblyName(assemblyName);
                Assemblies.Add(new KeyValuePair<string, Assembly>(file, assembly));
            }

            return DLLs.ToArray();
        }

        private static VendorInfo GetDetails(Type type, string DLL = "")
        {
            var assemblyName = string.Join('.', type.FullName.Split('.').SkipLast(1));
            
            var details = Details.Where(a => a.Assembly == assemblyName).FirstOrDefault();
            if(details == null)
            {
                details = new VendorInfo();
                details.Assembly = assemblyName;
                details.DLL = DLL;
                if (!assemblyName.Contains("Saber.Common"))
                {
                    Details.Add(details);
                }
            }
            return details;
        }

        public static void DeleteVendors()
        {
            //check all vendors to see if Saber has marked them for uninstallation
            var files = Directory.GetFiles(App.MapPath("/Vendors/"), "uninstall.sbr", SearchOption.AllDirectories);
            var root = App.MapPath("/Vendors/");
            foreach(var file in files)
            {
                var vendor = file.Replace(root, "").Replace("uninstall.sbr", "").Replace("\\", "/").Replace("/", "");
                try
                {
                    Uninstalled.Add(vendor);
                    //execute uninstall.sql
                    if(File.Exists(App.MapPath("/Vendors/" + vendor + "/Sql/uninstall.sql")))
                    {
                        Query.Script.Execute(App.MapPath("/Vendors/" + vendor + "/Sql/uninstall.sql"));
                        Console.WriteLine("Executed /Vendors/" + vendor + "/Sql/uninstall.sql");
                    }
                    Directory.Delete(App.MapPath("/Vendors/" + vendor), true);
                    Console.WriteLine("Uninstalled Vendor " + vendor);
                }
                catch (Exception) { }
            }
        }
        #endregion

        #region "Versioning"
        public static void CheckVersions()
        {
            //update JSON file with current versions of DLL files
            var versions = new List<AssemblyInfo>();
            var versionsChanged = false;
            if (File.Exists(App.MapPath("/Vendors/versions.json")))
            {
                versions = JsonSerializer.Deserialize<List<AssemblyInfo>>(File.ReadAllText(App.MapPath("/Vendors/versions.json")));
            }
            foreach (var detail in Details)
            {
                //check version of vendor
                var v = detail.Version;
                if(v == null || v == "") { continue; }
                var vparts = v.ToString().Split('.').Select(a => int.Parse(a)).ToArray();
                var isnew = false;
                var isupdated = false;
                var i = versions.FindIndex(a => a.Assembly == detail.Assembly);
                var v2 = new int[] { };
                if (i >= 0)
                {
                    v2 = versions[i].Version.Split('.').Select(a => int.Parse(a)).ToArray();
                    if (Utility.Versions.Compare(vparts, v2))
                    {
                        isnew = true;
                        isupdated = true;
                        versionsChanged = true;
                        versions[i] = new AssemblyInfo() { Assembly = versions[i].Assembly, Version = v };
                    }
                }
                else
                {
                    isnew = true;
                    versionsChanged = true;
                    versions.Add(new AssemblyInfo() { Assembly = detail.Assembly, Version = v });
                }

                if (isnew)
                {
                    //copy any public vendor resource files to the wwwroot folder
                    var path = App.MapPath(detail.Path);
                    var relpath = detail.Key + "/";
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles().Where(Current => Regex.IsMatch(Current.Extension, "\\.(js|css|less|" + string.Join("|", Core.Image.Extensions).Replace(".", "") + ")", RegexOptions.IgnoreCase));
                    var jsPath = "/wwwroot/editor/vendors/" + relpath.ToLower();
                    var cssPath = "/wwwroot/editor/vendors/" + relpath.ToLower();
                    var imagesPath = "/wwwroot/editor/vendors/" + relpath.ToLower();
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
                    foreach (var f in files)
                    {
                        //copy all required vendor resources
                        switch (f.Extension)
                        {
                            case ".js":
                                Utility.Compression.GzipCompress(f.OpenText().ReadToEnd(), jsPath + Path.GetFileName(f.FullName));
                                break;
                            case ".css":
                                File.Copy(f.FullName, App.MapPath(cssPath + Path.GetFileName(f.FullName)), true);
                                break;
                            case ".less":
                                Platform.Website.SaveLessFile(f.OpenText().ReadToEnd(), cssPath + Path.GetFileName(f.FullName), f.FullName.Replace(f.Name, ""));
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

                    if(isupdated == false)
                    {
                        //check for Sql/install.sql script
                        if(File.Exists(App.MapPath(detail.Path + "Sql/install.sql")))
                        {
                            try
                            {
                                Query.Script.Execute(App.MapPath(detail.Path + "Sql/install.sql"));
                                Console.WriteLine("Executed " + detail.Path + "Sql/install.sql");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Error executing " + detail.Path + "Sql/install.sql");
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        //check for migration scripts
                        var scripts = Directory.GetFiles(App.MapPath(detail.Path + "Sql/"), "migrate-*.sql");
                        var migrations = new List<KeyValuePair<int[], string>>();
                        foreach(var script in scripts)
                        {
                            var f = script.Replace("\\", "/").Split("/")[^1];
                            var ver = f.Replace("migrate-", "").Replace(".sql", "");
                            var vrs = ver.Split(".").Select(a => int.Parse(a)).ToArray();
                            if(Utility.Versions.Compare(vrs, v2))
                            {
                                migrations.Add(new KeyValuePair<int[], string>(vrs, f));
                            }
                        }
                        migrations.Sort((a, b) => Utility.Versions.Compare(a.Key, b.Key) ? 1 : 0);
                        foreach (var script in migrations)
                        {
                            try
                            {
                                Query.Script.Execute(App.MapPath(detail.Path + "Sql/" + script.Value));
                                Console.WriteLine("Executed " + detail.Path + "Sql/" + script.Value);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error executing " + detail.Path + "Sql/" + script.Value);
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }

            //check for uninstalled vendor plugins
            if(Uninstalled.Count > 0)
            {
                foreach(var vendor in Uninstalled)
                {
                    if(versions.Any(a => a.Assembly == "Saber.Vendors." + vendor))
                    {
                        versions.Remove(versions.Where(a => a.Assembly == "Saber.Vendors." + vendor).FirstOrDefault());
                        versionsChanged = true;
                    }
                }
            }

            if (versionsChanged)
            {
                //save versions to JSON
                File.WriteAllText(App.MapPath("/Vendors/versions.json"), JsonSerializer.Serialize(versions, new JsonSerializerOptions() { WriteIndented = true }));

                //concat all editor.js files into "/wwwroot/editor/js/vendors-editor.js"
                ConcatVendorsEditorJs();
            }
        }

        public static void ConcatVendorsEditorJs()
        {
            var vendorsPath = new DirectoryInfo(App.MapPath("/Vendors/"));
            var files = vendorsPath.GetFiles("editor.js", SearchOption.AllDirectories);
            var jsparts = new StringBuilder();
            foreach (var f in files)
            {
                jsparts.Append(File.ReadAllText(f.FullName));
            }
            //gzip vendors-eitor.js
            Utility.Compression.GzipCompress(string.Join("\n", jsparts), "/wwwroot/editor/js/vendors-editor.js");
        }
        #endregion

        #region "View Renderers"
        public static void GetViewRenderersFromFileSystem()
        {
            foreach(var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorViewRenderer")
                        {
                            GetViewRendererFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetViewRendererFromType(Type type, string DLL = "")
        {
            if(type == null) { return; }
            if (type.Equals(typeof(IVendorViewRenderer))) { return; }
            var attributes = type.GetCustomAttributes<ViewPathAttribute>();
            var details = GetDetails(type, DLL);
            foreach (var attr in attributes)
            {
                if (!ViewRenderers.ContainsKey(attr.Path))
                {
                    details.ViewRenderers.Add(attr.Path, new List<IVendorViewRenderer>());
                    ViewRenderers.Add(attr.Path, new List<IVendorViewRenderer>());
                }
                var instance = (IVendorViewRenderer)Activator.CreateInstance(type);
                details.ViewRenderers[attr.Path].Add(instance);
                ViewRenderers[attr.Path].Add(instance);
            }
        }
        #endregion

        #region "Content Fields"
        public static void GetContentFieldsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorContentField")
                        {
                            GetContentFieldsFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetContentFieldsFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorContentField))) { return; }
            var attributes = type.GetCustomAttributes<ContentFieldAttribute>();
            var details = GetDetails(type, DLL);
            foreach (var attr in attributes)
            {
                var instance = (IVendorContentField)Activator.CreateInstance(type);
                var info = new Models.VendorContentFieldInfo()
                {
                    ContentField = instance,
                    ReplaceRow = type.GetCustomAttributes<ReplaceRowAttribute>().Count() > 0
                };

                details.ContentFields.Add(attr.FieldName, info);
                ContentFields.Add(attr.FieldName, info);
            }
        }
        #endregion

        #region "Controllers"
        public static void GetControllersFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorController")
                        {
                            GetControllerFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetControllerFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorController))) { return; }
            var details = GetDetails(type, DLL);
            details.Controllers.Add(type.Name.ToLower(), type);
            Controllers.Add(type.Name.ToLower(), type);
        }
        #endregion

        #region "Services"
        public static void GetServicesFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorService")
                        {
                            GetServiceFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetServiceFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorService))) { return; }
            var details = GetDetails(type, DLL);
            details.Services.Add(type.Name.ToLower(), type);
            Services.Add(type.Name.ToLower(), type);
        }
        #endregion

        #region "Startup"
        public static void GetStartupsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorStartup")
                        {
                            GetStartupFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetStartupFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorStartup))) { return; }
            var details = GetDetails(type, DLL);
            details.Startups.Add(type.Assembly.GetName().Name, type);
            Startups.Add(type.Assembly.GetName().Name, type);
        }
        #endregion

        #region "Security Keys"
        public static void GetSecurityKeysFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorKeys")
                        {
                            GetSecurityKeysFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetSecurityKeysFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorKeys))) { return; }
            var details = GetDetails(type, DLL);
            var instance = (IVendorKeys)Activator.CreateInstance(type);
            details.Keys.Add(instance);
            Keys.Add(instance);
        }
        #endregion

        #region "Html Components"
        public static void GetHtmlComponentsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorHtmlComponent")
                        {
                            GetHtmlComponentsFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetHtmlComponentsFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorHtmlComponents))) { return; }
            var details = GetDetails(type, DLL);
            var instance = (IVendorHtmlComponents)Activator.CreateInstance(type);
            var components = instance.Bind();
            foreach (var component in components) {
                if(component.Parameters.Count == 0)
                {
                    details.SpecialVars.Add(component.Key, component);
                    SpecialVars.Add(component.Key, component);
                }
                details.HtmlComponents.Add(component.Key, component);
                HtmlComponents.Add(component.Key, component);
            }
        }

        public static void GetHtmlComponentKeys()
        {
            HtmlComponentKeys = HtmlComponents.Select(a => a.Key).OrderBy(a => a).ToArray();
        }
        #endregion

        #region "Email Clients"
        public static void GetEmailClientsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorEmailClient")
                        {
                            GetEmailClientsFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetEmailClientsFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorEmailClient))) { return; }
            var details = GetDetails(type, DLL);
            var instance = (IVendorEmailClient)Activator.CreateInstance(type);
            if(instance.Key == "smtp") { return; } //skip internal email client
            details.EmailClients.Add(instance.Key, instance);
            EmailClients.Add(instance.Key, instance);
            instance.Init();
        }
        #endregion

        #region "Emails"
        public static void GetEmailsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorEmails")
                        {
                            GetEmailsFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetEmailsFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorEmails))) { return; }
            var details = GetDetails(type, DLL);
            var emails = (IVendorEmails)Activator.CreateInstance(type);
            foreach(var email in emails.Types)
            {
                details.EmailTypes.Add(email.Key, email);
                EmailTypes.Add(email.Key, email);
            }
        }
        #endregion

        #region "Website Settings"
        public static void GetWebsiteSettingsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorWebsiteSettings")
                        {
                            GetWebsiteSettingsFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetWebsiteSettingsFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorWebsiteSettings))) { return; }
            var details = GetDetails(type, DLL);
            var instance = (IVendorWebsiteSettings)Activator.CreateInstance(type);
            details.WebsiteSettings.Add(instance);
            WebsiteSettings.Add(instance);
        }
        #endregion

        #region "Vendor Info"
        public static void GetInfoFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorInfo")
                        {
                            GetInfoFromType(type, assembly.Key);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetInfoFromType(Type type, string DLL = "")
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorInfo))) { return; }
            var details = GetDetails(type, DLL);
            var instance = (IVendorInfo)Activator.CreateInstance(type);
            if (Uninstalled.Contains(instance.Key))
            {
                Details.Remove(details);
                return;
            }
            details.Key = instance.Key;
            details.Name = instance.Name;
            details.Description = instance.Description;
            details.Icon = instance.Icon;
            details.Version = instance.Version;
            details.Path = "/Vendors/" + instance.Key + "/";
        }
        #endregion
    }
}
