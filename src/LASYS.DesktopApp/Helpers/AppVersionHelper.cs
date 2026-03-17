using System.Reflection;

namespace LASYS.DesktopApp.Helpers
{
    public static class AppVersionHelper
    {
        public static string GetVersion()
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (version is null)
                return "vUnknown";

            // Remove +commit hash
            var cleanVersion = version.Split('+')[0];

            return $"v{cleanVersion}";
        }
    }
}
