using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class Startup : Datasilk.Startup {

    public override void Configured(IApplicationBuilder app, IHostingEnvironment env, IConfigurationRoot config)
    {
        base.Configured(app, env, config);
        var query = new Saber.Query.Users(server.sqlConnectionString);
        server.resetPass = query.HasPasswords();
        server.hasAdmin = query.HasAdmin();

        server.languages = new Dictionary<string, string>();
        var languages = new Saber.Query.Languages(server.sqlConnectionString);
        server.languages.Add("en", "English"); //english should be the default language
        languages.GetList().ForEach((lang) => {
            server.languages.Add(lang.langId, lang.language);
        });
    }
}
