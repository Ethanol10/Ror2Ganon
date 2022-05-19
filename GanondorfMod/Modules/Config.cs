using BepInEx.Configuration;
using System;
using UnityEngine;

namespace GanondorfMod.Modules
{
    public static class Config
    {
        public static ConfigEntry<bool> saturatedClassicEnabled;
        public static ConfigEntry<bool> purpleEnabled;
        public static ConfigEntry<bool> greenEnabled;
        public static ConfigEntry<bool> hulkingMaliceEnabled;
        public static ConfigEntry<bool> brownEnabled;

        public static void ReadConfig()
        {
            saturatedClassicEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Saturated Classic Enabled"), false, new ConfigDescription("Make Saturated Classic to appear in game as a selectable skin.", null, Array.Empty<object>()));
            purpleEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Purple Enabled"), false, new ConfigDescription("Make Purple to appear in game as a selectable skin.", null, Array.Empty<object>()));
            greenEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Green Enabled"), false, new ConfigDescription("Make Green to appear in game as a selectable skin.", null, Array.Empty<object>()));
            hulkingMaliceEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Hulking Malice Enabled"), false, new ConfigDescription("Make Hulking Malice to appear in game as a selectable skin.", null, Array.Empty<object>()));
            brownEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Brown Enabled"), false, new ConfigDescription("Make Brown to appear in game as a selectable skin.", null, Array.Empty<object>()));
        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<bool> CharacterEnableConfig(string characterName)
        {
            return GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition(characterName, "Enabled"), true, new ConfigDescription("Set to false to disable Ganondorf"));
        }
    }
}