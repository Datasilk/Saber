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
                    case "wwwroot":
                        return paths;
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
            Models.Page.Settings config;

            if (!string.IsNullOrEmpty(contents))
            {
                //deserialize page config file
                config = JsonSerializer.Deserialize<Models.Page.Settings>(contents);
                if(config != null)
                {
                    //update config properties
                    if (string.IsNullOrEmpty(config.Header))
                    {
                        config.Header = "header.html";
                    }
                    if (string.IsNullOrEmpty(config.Footer))
                    {
                        config.Footer = "footer.html";
                    }
                    config.Paths = paths;

                    var pathx = paths.Length - 1;
                    if (paths[^1] == "template")
                    {
                        //find template in parent folder (since this page is also a template)
                        pathx -= 1;
                    }

                    //see if page uses a template
                    for (var x = pathx; x > 1; x--)
                    {
                        var templatePath = string.Join('/', paths.Take(x).ToArray()) + "/template";
                        if (File.Exists(templatePath + ".html"))
                        {
                            config.TemplatePath = templatePath;
                            config.IsFromTemplate = true;
                            if(File.Exists(templatePath + ".json"))
                            {
                                //load template config
                                Models.Page.Settings tempconfig = null;
                                if (Configs.ContainsKey(templatePath + ".json"))
                                {
                                    //load from cache
                                    tempconfig = Configs[templatePath + ".json"];
                                }
                                else
                                {
                                    contents = File.ReadAllText(App.MapPath(templatePath + ".json"));
                                    if (!string.IsNullOrEmpty(contents))
                                    {
                                        tempconfig = JsonSerializer.Deserialize<Models.Page.Settings>(contents);
                                    }
                                }
                                if(tempconfig != null && tempconfig.IsLiveTemplate)
                                {
                                    //add stylesheets & scripts from live template into page config
                                    if(tempconfig.Stylesheets.Count > 0)
                                    {
                                        config.LiveStylesheets.AddRange(tempconfig.Stylesheets);
                                    }
                                    if (tempconfig.Scripts.Count > 0)
                                    {
                                        config.LiveScripts.AddRange(tempconfig.Scripts);
                                    }
                                    config.Header = tempconfig.Header;
                                    config.Footer = tempconfig.Footer;
                                }
                            }
                            break;
                        }
                    }

                    //add config to cache
                    Configs.Add(filename, config);
                    return config;
                }
            }

            //try to get the template config by traversing sub-folders backwards
            var foundTemplate = string.Empty;
            for(var x = paths.Length -1; x > 1; x--)
            {
                var templatePath = string.Join('/', paths.Take(x).ToArray()) + "/template";
                if (File.Exists(templatePath + ".html"))
                {
                    foundTemplate = templatePath;
                    if (Configs.ContainsKey(templatePath + ".json"))
                    {
                        //clone template config from cached object to use for new page config
                        var template = Configs[templatePath + ".json"];
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
                            FromLiveTemplate = template.IsLiveTemplate,
                            UsesLiveTemplate = template.IsLiveTemplate,
                            IsLiveTemplate = false,
                            Paths = paths,
                            TemplatePath = foundTemplate
                        };
                        if (template.IsLiveTemplate)
                        {
                            config.LiveStylesheets = template.Stylesheets.ToArray().ToList();
                            config.LiveScripts = template.Scripts.ToArray().ToList();
                        }
                        else
                        {
                            config.Stylesheets = template.Stylesheets.ToArray().ToList();
                            config.Scripts = template.Scripts.ToArray().ToList();
                        }
                        Configs.Add(filename, config);
                        return config;
                    }
                    else if(File.Exists(App.MapPath(templatePath + ".json")))
                    {
                        //load template config from file system to use for new page config
                        contents = File.ReadAllText(App.MapPath(templatePath + ".json"));
                        if (!string.IsNullOrEmpty(contents))
                        {
                            config = JsonSerializer.Deserialize<Models.Page.Settings>(contents);
                            if (config != null)
                            {
                                config.FromLiveTemplate = config.IsLiveTemplate;
                                config.UsesLiveTemplate = config.IsLiveTemplate;
                                config.IsFromTemplate = true;
                                if (config.IsLiveTemplate)
                                {
                                    config.LiveStylesheets = config.Stylesheets;
                                    config.LiveScripts = config.Scripts;
                                    config.Stylesheets = new List<string>();
                                    config.Scripts = new List<string>();
                                }
                                config.IsLiveTemplate = false; //set to false after using config.IsLiveTemplate above
                                config.Paths = paths;
                                config.TemplatePath = foundTemplate;
                                Configs.Add(filename, config);
                                return config;
                            }
                        }
                    }
                    break;
                }
            }

            //all else fails, generate a new page settings object
            var file = paths[^1];
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
                Paths = paths,
                IsFromTemplate = !string.IsNullOrEmpty(foundTemplate),
                TemplatePath = foundTemplate
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
