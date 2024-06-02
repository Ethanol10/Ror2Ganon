using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using System;
using UnityEngine;
using RiskOfOptions.OptionConfigs;

namespace GanondorfMod.Modules
{
    public static class Config
    {
        public static ConfigEntry<bool> saturatedClassicEnabled;
        public static ConfigEntry<bool> purpleEnabled;
        public static ConfigEntry<bool> greenEnabled;
        public static ConfigEntry<bool> hulkingMaliceEnabled;
        public static ConfigEntry<bool> brownEnabled;

        public static ConfigEntry<bool> disableSwordThrowParticleEffects;
        public static ConfigEntry<float> wizardsFootCooldown;
        public static ConfigEntry<float> flameChokeCooldown;

        public static ConfigEntry<float> secondaryFlameChokeCooldown;
        public static ConfigEntry<float> utilWizardsFootCooldown;

        public static ConfigEntry<float> voiceVolume;
        public static ConfigEntry<float> sfxVolume;

        public static void ReadConfig()
        {
            saturatedClassicEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Saturated Classic Enabled"), false, new ConfigDescription("Make Saturated Classic to appear in game as a selectable skin.", null, Array.Empty<object>()));
            purpleEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Purple Enabled"), false, new ConfigDescription("Make Purple to appear in game as a selectable skin.", null, Array.Empty<object>()));
            greenEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Green Enabled"), false, new ConfigDescription("Make Green to appear in game as a selectable skin.", null, Array.Empty<object>()));
            hulkingMaliceEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Hulking Malice Enabled"), false, new ConfigDescription("Make Hulking Malice to appear in game as a selectable skin.", null, Array.Empty<object>()));
            brownEnabled = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("01 - Skins", "Brown Enabled"), false, new ConfigDescription("Make Brown to appear in game as a selectable skin.", null, Array.Empty<object>()));

            disableSwordThrowParticleEffects = GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Miscellaneous", "Remove Serrated Whirlwind Effects"), false, new ConfigDescription("Disables the effect when the sword is thrown.", null, Array.Empty<object>()));

            wizardsFootCooldown = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("02 - Cooldowns", "Secondary Slot - Wizards Foot"),
                    6f,
                    new ConfigDescription("Sets the Cooldown for Wizards Foot on the Secondary slot. This requires a restart to take effect.", null, Array.Empty<object>())
                );
            flameChokeCooldown = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("02 - Cooldowns", "Utility Slot - Flame Choke"),
                    8f,
                    new ConfigDescription("Sets the Cooldown for Flame Choke on the Utility slot. This requires a restart to take effect.", null, Array.Empty<object>())
                );

            secondaryFlameChokeCooldown = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("02 - Cooldowns", "Secondary Slot - Flame Choke"),
                    6f,
                    new ConfigDescription("Sets the Cooldown for Flame Choke on the Secondary slot. This requires a restart to take effect.", null, Array.Empty<object>())
                );
            utilWizardsFootCooldown = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("02 - Cooldowns", "Utility Slot - Wizards Foot"),
                    8f,
                    new ConfigDescription("Sets the Cooldown for Wizards Foot on the Utility slot. This requires a restart to take effect.", null, Array.Empty<object>())
                );

            voiceVolume = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("03 - Volume", "Voice Volume"),
                    100f,
                    new ConfigDescription("Sets the Volume for voicelines, AFFECTED BY IN-GAME MASTER AND SFX VOLUME SLIDERS!", null, Array.Empty<object>())
                );

            sfxVolume = GanondorfPlugin.instance.Config.Bind<float>
                (
                    new ConfigDefinition("03 - Volume", "Ganondorf SFX Volume"),
                    100f,
                    new ConfigDescription("Sets the Volume for Swing sounds, hits and non-voice SFX, AFFECTED BY IN-GAME MASTER AND SFX VOLUME SLIDERS!", null, Array.Empty<object>())
                );
        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<bool> CharacterEnableConfig(string characterName)
        {
            return GanondorfPlugin.instance.Config.Bind<bool>(new ConfigDefinition(characterName, "Enabled"), true, new ConfigDescription("Set to false to disable Ganondorf"));
        }

        public static void SetupRiskOfOptions()
        {
            Sprite icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texGanondorfIcon");
            ModSettingsManager.SetModIcon(icon);

            ModSettingsManager.AddOption(new CheckBoxOption(disableSwordThrowParticleEffects));

            ModSettingsManager.AddOption(new CheckBoxOption(saturatedClassicEnabled));
            ModSettingsManager.AddOption(new CheckBoxOption(purpleEnabled));
            ModSettingsManager.AddOption(new CheckBoxOption(greenEnabled));
            ModSettingsManager.AddOption(new CheckBoxOption(hulkingMaliceEnabled));
            ModSettingsManager.AddOption(new CheckBoxOption(brownEnabled));

            ModSettingsManager.AddOption(new StepSliderOption(wizardsFootCooldown, new StepSliderConfig
            {
                min = 0.01f,
                max = 100f,
                increment = 0.01f
            }
            ));
            ModSettingsManager.AddOption(new StepSliderOption(flameChokeCooldown, new StepSliderConfig
            {
                min = 0.01f,
                max = 100f,
                increment = 0.01f
            }
            ));
            ModSettingsManager.AddOption(new StepSliderOption(secondaryFlameChokeCooldown, new StepSliderConfig
            {
                min = 0.01f,
                max = 100f,
                increment = 0.01f
            }
            ));
            ModSettingsManager.AddOption(new StepSliderOption(utilWizardsFootCooldown, new StepSliderConfig
            {
                min = 0.01f,
                max = 100f,
                increment = 0.01f
            }
            ));

            ModSettingsManager.AddOption(new StepSliderOption(voiceVolume, new StepSliderConfig
            {
                min = 0f,
                max = 100f,
                increment = 0.01f
            }));
            ModSettingsManager.AddOption(new StepSliderOption(sfxVolume, new StepSliderConfig
            {
                min = 0f,
                max = 100f,
                increment = 0.01f
            }));
        }

        public static void OnChangeHooks() 
        {
            disableSwordThrowParticleEffects.SettingChanged += DisableSwordThrowParticleEffects_SettingChanged;
            voiceVolume.SettingChanged += VoiceVolume_SettingChanged;
            sfxVolume.SettingChanged += SfxVolume_SettingChanged;
        }

        private static void DisableSwordThrowParticleEffects_SettingChanged(object sender, EventArgs e)
        {

            Modules.Assets.brawlSwordObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);
            Modules.Assets.brawlSwordObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);
            Modules.Assets.brawlSwordObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);

            Modules.Assets.swordObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);
            Modules.Assets.swordObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);
            Modules.Assets.swordObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(!disableSwordThrowParticleEffects.Value);
        }

        private static void SfxVolume_SettingChanged(object sender, EventArgs e)
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.SetRTPCValue("Volume_GanonSFX", sfxVolume.Value);
            }
        }

        private static void VoiceVolume_SettingChanged(object sender, EventArgs e)
        {
            if (AkSoundEngine.IsInitialized())
            {
                AkSoundEngine.SetRTPCValue("Volume_GanonVoice", voiceVolume.Value);
            }
        }
    }
}