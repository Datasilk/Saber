﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Saber.Utility
{
    public class Page
    {
        private Core S;

        public Page(Core DatasilkCore)
        {
            S = DatasilkCore;
        }

        public static string[] GetRelativePath(string path)
        {
            var paths = path.Split('/');

            //translate root path to relative path
            switch (paths[0].ToLower())
            {
                case "root": paths[0] = ""; break;
                case "css": paths[0] = "/CSS"; break;
                case "pages": paths[0] = "/Pages"; break;
                case "partials": paths[0] = "/Partials"; break;
                case "scripts": paths[0] = "/Scripts"; break;
                case "services": paths[0] = "/Services"; break;
                case "content": paths[0] = "/Content/pages"; break;
                default: return new string[] { };
            }
            return paths;
        }

        public string ConfigFilePath(string path)
        {
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            return relpath.Replace(file, fileparts[0] + ".json");
        }

        public Models.Page.Settings GetPageConfig(string path)
        {
            var config = (Models.Page.Settings)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(ConfigFilePath(path), true), typeof(Models.Page.Settings));
            if (config != null) { return config; }

            //all else fails, generate a new page settings object
            var paths = GetRelativePath(path);
            var file = paths[paths.Length - 1];
            return new Models.Page.Settings()
            {
                title = new Models.Page.Title()
                {
                    prefix = "",
                    suffix = "",
                    body = S.Util.Str.Capitalize(file.Replace("-", " ").Replace("_", " ")),
                    prefixId = 0,
                    suffixId = 0
                },
                description = "This page was generated by the Saber web development platform",
                security = new Models.Page.Security()
                {
                    read = new int[] { },
                    write = new int[] { },
                    secure = false
                }
            };
        }

        public void SavePageConfig(string path, Models.Page.Settings config)
        {
            var filename = ConfigFilePath(path);
            S.Server.SaveFileFromCache(filename, S.Util.Serializer.WriteObjectToString(config, Newtonsoft.Json.Formatting.Indented));
        }
    }
}
