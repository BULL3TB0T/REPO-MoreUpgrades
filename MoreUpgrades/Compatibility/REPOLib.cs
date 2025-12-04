using BepInEx.Bootstrap;

namespace MoreUpgrades.Compatibility
{
    internal static class REPOLib
    {
        internal const string modGUID = "REPOLib";

        public static bool IsLoaded() => Chainloader.PluginInfos.ContainsKey(modGUID);

        public static void OnAwake() => Plugin.instance.PatchAll("REPOLibPatches");
    }
}
