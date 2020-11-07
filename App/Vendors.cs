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
        public static Dictionary<string, List<IVendorViewRenderer>> ViewRenderers { get; set; } = new Dictionary<string, List<Vendor.IVendorViewRenderer>>();
        public static Dictionary<string, Type> Controllers { get; set; } = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Startups { get; set; } = new Dictionary<string, Type>();
        private static List<string> DLLs { get; set; } = new List<string>();
        private static List<Assembly> Assemblies { get; set; } = new List<Assembly>();
        private class AssemblyInfo
        {
            public string Assembly { get; set; }
            public string Version { get; set; }
        }

        public static string[] LoadDLLs()
        {
            //search Vendor folder for DLL files
            if (Directory.Exists(App.MapPath("/Vendor")))
            {
                RecurseDirectories(App.MapPath("/Vendor"));
            }
            //update JSON file with current versions of DLL files
            var versions = new List<AssemblyInfo>();
            var versionsChanged = false;
            if (File.Exists(App.MapPath("/Vendor/versions.json")))
            {
                versions = JsonSerializer.Deserialize<List<AssemblyInfo>>(File.ReadAllText(App.MapPath("/Vendor/versions.json")));
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
                    //copy any required JS & CSS files to the wwwroot folder
                    var filename = Path.GetFileName(file);
                    var path = file.Replace(filename, "");
                    var relpath = path.Replace("\\", "/").Split("/Vendor/")[1];
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles().Where(Current => Regex.IsMatch(Current.Extension, "\\.(js|css)", RegexOptions.IgnoreCase));
                    var jsPath = "/wwwroot/editor/js/vendor/" + relpath.ToLower();
                    var cssPath = "/wwwroot/editor/css/vendor/" + relpath.ToLower();
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
                    }
                    foreach(var f in files)
                    {
                        //copy all required JS & CSS files
                        switch (f.Extension)
                        {
                            case ".js":
                                File.Copy(f.FullName, App.MapPath(jsPath + Path.GetFileName(f.FullName)));
                                break;
                            case ".css":
                                File.Copy(f.FullName, App.MapPath(cssPath + Path.GetFileName(f.FullName)));
                                break;
                        }
                    }
                }
            }
            if (versionsChanged)
            {
                //save versions to JSON
                File.WriteAllText(App.MapPath("/Vendor/versions.json"), JsonSerializer.Serialize(versions, new JsonSerializerOptions() { WriteIndented = true }));
            }

            return DLLs.ToArray();
        }

        private static void RecurseDirectories(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                DLLs.AddRange(dir.GetFiles("*.dll").Select(a => a.FullName).ToArray());
                foreach(var sub in dir.GetDirectories())
                {
                    RecurseDirectories(sub.FullName);
                }
            }
        }

        #region "View Renderers"
        public static void GetViewRenderersFromFileSystem(string[] files)
        {
            if (files == null) { return; }
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
            var attributes = type.GetCustomAttributes<Vendor.ViewPathAttribute>();
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

        #region "Controllers"
        public static void GetControllersFromFileSystem(string[] files)
        {
            if (files == null) { return; }
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
        public static void GetStartupsFromFileSystem(string[] files)
        {
            if (files == null) { return; }
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
            Controllers.Add(type.Name.ToLower(), type);
        }
        #endregion
    }
}
