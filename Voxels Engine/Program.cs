

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

class Program
{
    static void Main()
    {
        NativeWindowSettings nws = NativeWindowSettings.Default;

        nws.APIVersion = new Version(4, 0);
        nws.Profile = ContextProfile.Core;
        nws.Flags = ContextFlags.ForwardCompatible;
        
        new Window(nws).Run();
    }
}