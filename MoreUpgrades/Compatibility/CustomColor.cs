using BepInEx.Bootstrap;

namespace MoreUpgrades.Compatibility
{
    internal static class CustomColors
    {
        internal const string modGUID = "x753.CustomColors";
        
        internal static bool IsLoaded() => Chainloader.PluginInfos.ContainsKey(modGUID);
    }
}
