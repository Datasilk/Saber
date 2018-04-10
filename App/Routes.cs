using Datasilk;

public class Routes : Datasilk.Routes
{
    public Routes(Core DatasilkCore) : base(DatasilkCore) { }

    public override Page FromPageRoutes(string name)
    {
        switch (name)
        {
            case "login": return new Saber.Pages.Login(S);
            case "upload": return new Saber.Pages.Upload(S);
            default: return new Saber.Pages.Editor(S);
        }

    }

    public override Service FromServiceRoutes(string name)
    {
        return null;
    }
}
