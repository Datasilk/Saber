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
        public static Dictionary<string, Type> Startups = new Dictionary<string, Type>();

        public static string[] LoadDLLs()
        {
            if (Directory.Exists(Server.MapPath("/Vendors")))
            {
                var dir = new DirectoryInfo(Server.MapPath("/Vendors"));
                return dir.GetFiles("*.dll").Select(a => a.FullName).ToArray();
            }
            return null;
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
                Type type = readerAssembly.ExportedTypes.FirstOrDefault(t => t.FullName == "IVendorViewRenderer");
                GetViewRendererFromType(type);
            }
        }

        public static void GetViewRendererFromType(Type type)
        {
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
            if (type.Equals(typeof(IVendorStartup))) { return; }
            Controllers.Add(type.Name.ToLower(), type);
        }
        #endregion
    }
}
