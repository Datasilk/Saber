﻿using System.Collections.Generic;
using System.Linq;
using Utility.Serialization;
using CommonMark;

namespace Saber.Common.Platform
{
    public class Render
    {
        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public static string Page(string path, Datasilk.Request request)
        {
            //translate root path to relative path
            var server = Server.Instance;
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }

            //check file path on drive for (estimated) OS folder structure limitations 
            if (Server.MapPath(relpath).Length > 180)
            {
                throw new ServiceErrorException("The URL path you are accessing is too long to handle for the web server");
            }
            var scaffold = new Scaffold(relpath, server.Scaffold);
            if (scaffold.elements.Count == 0)
            {
                if (request.User.userId == 0)
                {
                    scaffold.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
                else
                {
                    scaffold.HTML = "<p>Write content using HTML & CSS</p>";
                }
            }

            //load user content from json file, depending on selected language
            var config = PageInfo.GetPageConfig(path);
            var lang = request.User.language;

            //check security
            if (config.security.secure == true)
            {
                if (!request.CheckSecurity() || !config.security.read.Contains(request.User.userId))
                {
                    throw new ServiceDeniedException("You do not have read access for this page");
                }
            }

            var contentfile = ContentFields.ContentFile(path, lang);
            var data = (Dictionary<string, string>)Serializer.ReadObject(server.LoadFileFromCache(contentfile, true), typeof(Dictionary<string, string>));
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item.Value.IndexOf("\n") >= 0)
                    {
                        scaffold.Data[item.Key] = CommonMarkConverter.Convert(item.Value);
                    }
                    else
                    {
                        scaffold.Data[item.Key] = item.Value;
                    }
                }
            }
            return scaffold.Render();
        }
    }
}
