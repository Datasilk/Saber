using Datasilk;

public class Routes : Datasilk.Routes
{
    public Routes(Core DatasilkCore) : base(DatasilkCore) { }

    public override Page FromPageRoutes(string name)
    {
        switch (name)
        {
            case "": case "home": return new Saber.Pages.Home(S);
            case "login": return new Saber.Pages.Login(S);
            default: return new Saber.Pages.Editor(S);
        }

    }

    public override Service FromServiceRoutes(string name)
    {
        return null;
    }
}
