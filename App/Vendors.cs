using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Saber.Vendor;
using System.Reflection;
using System.Text.Json;
using Saber.Core;

namespace Saber.Common
{

    public static class Vendors
    {
        private static List<string> DLLs { get; set; } = new List<string>();
        private static List<KeyValuePair<string, Assembly>> Assemblies { get; set; } = new List<KeyValuePair<string, Assembly>>();
        private static List<string> Uninstalled { get; set; } = new List<string>();

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
                DLLs.AddRange(dir.GetFiles("*.dll").Select(a => a.FullName).ToArray());
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
            
            var details = Core.Vendors.Details.Where(a => a.Assembly == assemblyName).FirstOrDefault();
            if(details == null)
            {
                details = new VendorInfo();
                details.Assembly = assemblyName;
                details.DLL = DLL;
                if (!assemblyName.Contains("Saber.Common"))
                {
                    Core.Vendors.Details.Add(details);
                }
            }
            return details;
        }

        public static List<string> MarkedForUninstall = null;

        public static List<string> GetVendorsMarkedForUninstall()
        {
            if(MarkedForUninstall != null) { return MarkedForUninstall; }
            if (!Directory.Exists(App.MapPath("/Vendors"))) {
                MarkedForUninstall = new List<string>();
            }
            else
            {
                var root = App.MapPath("/Vendors/");
                MarkedForUninstall = Directory.GetFiles(App.MapPath("/Vendors/"), "uninstall.sbr", SearchOption.AllDirectories).Select(a => a.Replace(root, "").Replace("uninstall.sbr", "").Replace("\\", "/").Replace("/", "")).ToList();
            }
            return MarkedForUninstall;
        }

        public static void DeleteVendors()
        {
            //check all vendors to see if Saber has marked them for uninstallation
            if (!Directory.Exists(App.MapPath("/Vendors"))) { return; }
            var vendors = GetVendorsMarkedForUninstall();
            
            foreach(var vendor in vendors)
            {
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
                catch (Exception ex) {
                    Console.WriteLine(ex.InnerException.Message);
                }
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
            if (Directory.Exists(App.MapPath("/wwwroot/editor/vendors")))
            {
                Directory.CreateDirectory(App.MapPath("/wwwroot/editor/vendors"));
            }
            foreach (var detail in Core.Vendors.Details)
            {
                //check version of vendor
                var v = detail.Version;
                if(v == null || v == "") { continue; }
                var vparts = v.ToString().Split('.').Select(a => int.Parse(a)).ToArray();
                var isnew = false;
                var isupdated = false;
                var haserror = false;
                var versionIndex = versions.FindIndex(a => a.Assembly == detail.Assembly);
                var v2 = new int[] { };
                if (versionIndex >= 0)
                {
                    v2 = versions[versionIndex].Version.Split('.').Select(a => int.Parse(a)).ToArray();
                    if (Utility.Versions.Compare(vparts, v2))
                    {
                        isnew = true;
                        isupdated = true;
                        versionsChanged = true;
                    }
                }
                else
                {
                    isnew = true;
                    versionsChanged = true;
                }

                if (isnew)
                {
                    //copy any public vendor resource files to the wwwroot folder
                    var path = App.MapPath(detail.Path);
                    var relpath = detail.Key + "/";
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles().Where(Current => Regex.IsMatch(Current.Extension, "\\.(js|css|less|" + string.Join("|", Core.Image.Extensions).Replace(".", "") + ")", RegexOptions.IgnoreCase));
                    var vendorsPath = "/wwwroot/editor/vendors/";
                    var vendorPath = vendorsPath + relpath.ToLower();
                    if (files.Count() > 0)
                    {
                        //create wwwroot vendor path
                        if (!Directory.Exists(App.MapPath(vendorPath)))
                        {
                            Directory.CreateDirectory(App.MapPath(vendorPath));
                        }
                    }
                    foreach (var f in files)
                    {
                        //copy all required vendor resources
                        switch (f.Extension)
                        {
                            case ".js":
                                Utility.Compression.GzipCompress(f.OpenText().ReadToEnd(), vendorPath + Path.GetFileName(f.FullName));
                                break;
                            case ".css":
                                File.Copy(f.FullName, App.MapPath(vendorPath + Path.GetFileName(f.FullName)), true);
                                break;
                            case ".less":
                                Platform.Website.SaveLessFile(f.OpenText().ReadToEnd(), vendorPath + Path.GetFileName(f.FullName), f.FullName.Replace(f.Name, ""));
                                break;
                            default:
                                if (Core.Image.Extensions.Any(a => a == f.Extension))
                                {
                                    //images
                                    File.Copy(f.FullName, App.MapPath(vendorPath + Path.GetFileName(f.FullName)), true);
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
                                Console.WriteLine("Executed " + detail.Path + "Sql/install.sql with no errors");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Error executing " + detail.Path + "Sql/install.sql");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.InnerException.Message);
                                haserror = true;
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
                                Console.WriteLine("Executed " + detail.Path + "Sql/" + script.Value + " with no errors");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error executing " + detail.Path + "Sql/" + script.Value);
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.InnerException.Message);
                                haserror = true;
                            }
                        }
                    }

                    //check if there are any errors before updating the plugin version in Venders/versions.json
                    if (haserror == false)
                    {
                        if (isupdated)
                        {
                            versions[versionIndex] = new AssemblyInfo() { Assembly = versions[versionIndex].Assembly, Version = v };
                        }
                        else
                        {
                            versions.Add(new AssemblyInfo() { Assembly = detail.Assembly, Version = v });
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            foreach (var attr in attributes)
            {
                if (!Core.Vendors.ViewRenderers.ContainsKey(attr.Path))
                {
                    details.ViewRenderers.Add(attr.Path, new List<IVendorViewRenderer>());
                    Core.Vendors.ViewRenderers.Add(attr.Path, new List<IVendorViewRenderer>());
                }
                var instance = (IVendorViewRenderer)Activator.CreateInstance(type);
                details.ViewRenderers[attr.Path].Add(instance);
                Core.Vendors.ViewRenderers[attr.Path].Add(instance);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            foreach (var attr in attributes)
            {
                var instance = (IVendorContentField)Activator.CreateInstance(type);
                var info = new Models.VendorContentFieldInfo()
                {
                    ContentField = instance,
                    ReplaceRow = type.GetCustomAttributes<ReplaceRowAttribute>().Count() > 0
                };

                details.ContentFields.Add(attr.FieldName, info);
                Core.Vendors.ContentFields.Add(attr.FieldName, info);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            details.Controllers.Add(type.Name.ToLower(), type);
            Core.Vendors.Controllers.Add(type.Name.ToLower(), type);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            details.Services.Add(type.Name.ToLower(), type);
            Core.Vendors.Services.Add(type.Name.ToLower(), type);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var name = type.FullName;
            if (!Core.Vendors.Startups.ContainsKey(name))
            {
                details.Startups.Add(name, type);
                Core.Vendors.Startups.Add(name, type);
            }
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorKeys)Activator.CreateInstance(type);
            details.Keys.Add(instance);
            Core.Vendors.Keys.Add(instance);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorHtmlComponents)Activator.CreateInstance(type);
            var components = instance.Bind();
            foreach (var component in components) {
                if(component.Parameters.Count == 0)
                {
                    details.SpecialVars.Add(component.Key, component);
                    Core.Vendors.SpecialVars.Add(component.Key, component);
                }
                details.HtmlComponents.Add(component.Key, component);
                Core.Vendors.HtmlComponents.Add(component.Key, component);
            }
        }

        public static void GetHtmlComponentKeys()
        {
            Core.Vendors.HtmlComponentKeys = Core.Vendors.HtmlComponents.Select(a => a.Key).OrderBy(a => a).ToArray();
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorEmailClient)Activator.CreateInstance(type);
            if(instance.Key == "smtp") { return; } //skip internal email client
            details.EmailClients.Add(instance.Key, instance);
            Core.Vendors.EmailClients.Add(instance.Key, instance);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var emails = (IVendorEmails)Activator.CreateInstance(type);
            foreach(var email in emails.Types)
            {
                details.EmailTypes.Add(email.Key, email);
                Core.Vendors.EmailTypes.Add(email.Key, email);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorWebsiteSettings)Activator.CreateInstance(type);
            details.WebsiteSettings.Add(instance);
            Core.Vendors.WebsiteSettings.Add(instance);
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
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorInfo)Activator.CreateInstance(type);
            if (Uninstalled.Contains(instance.Key))
            {
                Core.Vendors.Details.Remove(details);
                return;
            }
            details.Type = type;
            details.Key = instance.Key;
            details.Name = instance.Name;
            details.Description = instance.Description;
            details.Icon = instance.Icon;
            details.Version = instance.Version;
            details.Path = "/Vendors/" + instance.Key + "/";
        }
        #endregion

        #region "Data Sources"
        public static void GetDataSourcesFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorDataSources")
                        {
                            GetDataSourcesFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetDataSourcesFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorDataSources))) { return; }
            var details = GetDetails(type);
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorDataSources)Activator.CreateInstance(type);
            foreach(var datasource in instance.List())
            {
                Core.Vendors.DataSources.Add(new DataSourceInfo() { Key = (string.IsNullOrEmpty(instance.Prefix) ? "" : instance.Prefix + "-") + datasource.Key, Name = datasource.Value, Helper = instance });
            }
        }

        public static void InitDataSources()
        {
            foreach(var datasource in Core.Vendors.DataSources)
            {
                datasource.Helper.Init();
            }
        }
        #endregion

        #region "Saber Events"
        public static void GetSaberEventsFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes.Where(a => a.GetTypeInfo().IsSubclassOf(typeof(SaberEvents))))
                {
                    GetSaberEventsFromType(type);
                }
            }
        }

        public static void GetSaberEventsFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(SaberEvents))) { return; }
            var details = GetDetails(type);
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (SaberEvents)Activator.CreateInstance(type);
            Core.Vendors.EventHandlers.Add(instance);
        }
        #endregion

        #region "Internal APIs"
        public static void GetInternalApisFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorInteralApis")
                        {
                            GetInternalApisFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetInternalApisFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorInteralApis))) { return; }
            var details = GetDetails(type);
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorInteralApis)Activator.CreateInstance(type);
            foreach(var api in instance.Apis)
            {
                Core.Vendors.InternalApis.Add(api);
            }
        }
        #endregion

        #region "SignalR"
        public static void GetSignalRFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorSignalR")
                        {
                            GetSignalRFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetSignalRFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorSignalR))) { return; }
            var details = GetDetails(type);
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorSignalR)Activator.CreateInstance(type);
            Core.Vendors.SignalR.Add(instance);
            details.SignalR.Add(instance);
        }
        #endregion

        #region "CORS Policies"
        public static void GetPageResponseFromFileSystem()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.Value.ExportedTypes)
                {
                    foreach (var i in type.GetInterfaces())
                    {
                        if (i.Name == "IVendorCorsPolicy")
                        {
                            GetPageResponseFromType(type);
                            break;
                        }
                    }
                }
            }
        }

        public static void GetPageResponseFromType(Type type)
        {
            if (type == null) { return; }
            if (type.Equals(typeof(IVendorPageResponse))) { return; }
            var details = GetDetails(type);
            if (MarkedForUninstall.Contains(details.Key)) { return; }
            var instance = (IVendorPageResponse)Activator.CreateInstance(type);
            Core.Vendors.PageResponses.Add(instance);
            details.PageResponses.Add(instance);
        }
        #endregion
    }
}
