using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Saber.Vendor;
using System.Reflection;

namespace Saber
{
    public static class Vendors
    {
        public static Dictionary<string, List<IVendorViewRenderer>> ViewRenderers { get; set; } = new Dictionary<string, List<Vendor.IVendorViewRenderer>>();
        public static Dictionary<string, Type> Controllers { get; set; } = new Dictionary<string, Type>();
        public static Dictionary<string, Type> Startups { get; set; } = new Dictionary<string, Type>();
        private static List<string> DLLs { get; set; } = new List<string>();

        public static string[] LoadDLLs()
        {
            if (Directory.Exists(App.MapPath("/Vendor")))
            {
                RecurseDirectories(App.MapPath("/Vendor"));
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
            var vendors = new Dictionary<string, List<IVendorViewRenderer>>();
            foreach (var file in files)
            {
                var context = new Common.Assemblies.AssemblyLoader(file);
                AssemblyName assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
                Assembly readerAssembly = context.LoadFromAssemblyName(assemblyName);
                var test = new List<string>();
                foreach(var type in readerAssembly.ExportedTypes)
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
            var vendors = new Dictionary<string, List<IVendorController>>();
            foreach (var file in files)
            {
                var context = new Common.Assemblies.AssemblyLoader(file);
                AssemblyName assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
                Assembly readerAssembly = context.LoadFromAssemblyName(assemblyName);
                Type type = readerAssembly.ExportedTypes.FirstOrDefault(t => t.FullName == "IVendorController");
                GetControllerFromType(type);
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
            var vendors = new Dictionary<string, List<IVendorStartup>>();
            foreach (var file in files)
            {
                var context = new Common.Assemblies.AssemblyLoader(file);
                AssemblyName assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(file));
                Assembly readerAssembly = context.LoadFromAssemblyName(assemblyName);
                Type type = readerAssembly.ExportedTypes.FirstOrDefault(t => t.FullName == "IVendorStartup");
                GetStartupFromType(type);
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
