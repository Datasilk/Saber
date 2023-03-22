using Saber.Core.Extensions.Strings;
using System.Text.Json;

namespace Saber.Common.Platform
{
    public static class PageInfo
    {
        public static Dictionary<string, Models.Page.Settings> Configs { get; set; } = new Dictionary<string, Models.Page.Settings>();

        public static string[] GetRelativePath(string path)
        {
            var paths = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            //translate root path to relative path
            if (paths.Length > 0 && paths[0].ToLower() == "content" && paths.Length > 1)
            {
                if (paths[1].ToLower() == "temp") { return new string[] { }; }
                if (paths[0] == "content") { paths[0] = "Content"; }
            }
            else if (paths.Length > 0 && paths[0].ToLower() != "content")
            {
                switch (paths[0].ToLower())
                {
                    case "root":
                        return new string[] { "" };
                    case "temp":
                        return Array.Empty<string>();
                    default:
                        path = "Content/" + path;
                        paths = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        break;
                }
            }
            else if (path == "")
            {
                path = "Content/";
                paths = new string[] { "Content" };
            }
            else
            {
                return Array.Empty<string>();
            }
            return paths;
        }

        public static string ConfigFilePath(string path)
        {
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            return relpath.Replace(file, fileparts[0] + ".json");
        }

        public static Models.Page.Settings GetPageConfig(string path)
        {
            var filename = ConfigFilePath(path);
            if (Configs.ContainsKey(filename))
            {
                //get config from cache
                return Configs[filename];
            }
            //get config from file system

            var contents = File.Exists(App.MapPath(filename)) ? File.ReadAllText(App.MapPath(filename)) : "";
            var paths = GetRelativePath(path);
            var templatePath = string.Join('/', paths.Take(paths.Length - 1).ToArray()) + "/template.json";
            Models.Page.Settings config;

            if (!string.IsNullOrEmpty(contents))
            {
                config = JsonSerializer.Deserialize<Models.Page.Settings>(contents);
                if(config != null)
                {
                    if (string.IsNullOrEmpty(config.Header))
                    {
                        config.Header = "header.html";
                    }
                    if (string.IsNullOrEmpty(config.Footer))
                    {
                        config.Footer = "footer.html";
                    }
                    if (File.Exists(App.MapPath(templatePath)))
                    {
                        config.IsFromTemplate = true;
                    }
                    config.Paths = paths;
                    Configs.Add(filename, config);
                    return config;
                }
            }

            //try to get the template config
            var file = paths[^1];
            if (Configs.ContainsKey(templatePath))
            {
                //clone template config from cached object
                var template = Configs[templatePath];
                config = new Models.Page.Settings()
                {
                    Title = template.Title,
                    Description = template.Description,
                    Thumbnail = template.Thumbnail,
                    DateCreated = template.DateCreated,
                    Security = new Models.Page.Security()
                    {
                        groups = template.Security.groups
                    },
                    Header = template.Header,
                    Footer = template.Footer,
                    Stylesheets = template.Stylesheets.ToArray().ToList(),
                    Scripts = template.Scripts.ToArray().ToList(),
                    FromLiveTemplate = template.IsLiveTemplate,
                    UsesLiveTemplate = template.IsLiveTemplate,
                    IsLiveTemplate = false,
                    Paths = paths
                };
                Configs.Add(filename, config);
                return config;
            }
            else
            {
                //load template config from file system
                contents = File.Exists(App.MapPath(templatePath)) ? File.ReadAllText(App.MapPath(templatePath)) : "";
                if (!string.IsNullOrEmpty(contents))
                {
                    config = JsonSerializer.Deserialize<Models.Page.Settings>(contents);
                    if (config != null)
                    {
                        config.FromLiveTemplate = config.IsLiveTemplate;
                        config.UsesLiveTemplate = config.IsLiveTemplate;
                        config.IsFromTemplate = true;
                        config.IsLiveTemplate = false; //set to false after using config.IsLiveTemplate above
                        config.Paths = paths;
                        Configs.Add(filename, config);
                        return config; 
                    }
                }
            }

            //all else fails, generate a new page settings object
            config = new Models.Page.Settings()
            {
                Title = new Models.Page.Title()
                {
                    prefix = "",
                    suffix = "",
                    body = file.Capitalize().Replace("-", " ").Replace("_", " ").Replace(".html", "")
                },
                Description = "This page was generated by the Saber web development platform",
                DateCreated = DateTime.Now,
                Security = new Models.Page.Security()
                {
                    groups = Array.Empty<int>()
                },
                Paths = paths
            };
            Configs.Add(filename, config);
            return config;
        }

        public static string NameFromFile(string filename)
        {
            if (filename.Contains('/'))
            {
                filename = filename.Split('/')[^1];
            }
            return filename.Replace(".html", "").Replace("-", " ").Replace("_", " ").Capitalize();
        }

        public static void SavePageConfig(string path, Models.Page.Settings config)
        {
            var filename = ConfigFilePath(path);
            if (Configs.ContainsKey(filename))
            {
                Configs[filename] = config;
            }
            File.WriteAllText(App.MapPath(filename), JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));
        }

        /// <summary>
        /// Removes all associated cache for a specific web page
        /// </summary>
        /// <param name="path">Relative path to the page (e.g. "/Content/pages/home.html")</param>
        /// <param name="language"></param>
        public static void ClearCache(string path, string language)
        {
            //remove config from cache
            var filename = ConfigFilePath(path);
            Configs.Remove(filename);
            //remove content fields from cache
            var lang = Core.ContentFields.ContentFile(path, language);
            Cache.Remove(lang);
            //remove View from cache
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            ViewCache.Remove(relpath);
        }
    }
}
