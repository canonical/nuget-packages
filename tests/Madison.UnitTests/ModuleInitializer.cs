using System.Runtime.CompilerServices;

namespace Canonical.Madison.UnitTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.InitializePlugins();
    }
}