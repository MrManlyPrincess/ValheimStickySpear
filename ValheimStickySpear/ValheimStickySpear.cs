using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.Events;
using ValheimStickySpear.Extensions;

namespace ValheimStickySpear
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class ValheimStickySpear : BaseUnityPlugin
    {
        public const string PluginGUID = "com.mrmanlyprincess.StickySpear";
        public const string PluginName = "Sticky Spear";
        public const string PluginVersion = "0.0.1";

        private static CustomLocalization ModLocalization = LocalizationManager.Instance.GetLocalization();
        private static bool HasEverBeenEnabled;

        #region Config

        public static ConfigEntry<bool> ModEnabled;

        #endregion

        private void Awake()
        {
            CreateConfigValues();

            // TODO: Handle enabled check everywhere else since we can toggle it mid-game now
            if (!ModEnabled.Value) return;
            if (HasEverBeenEnabled) return;

            AddLocalizations();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            HasEverBeenEnabled = true;
        }

        #region Setup

        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;
            Config.ConfigReloaded += ReloadConfig;
            Config.SettingChanged += UpdateSettings;

            ModEnabled = Config.Bind("General", "Enabled", true, "Enable this mod");
        }

        private void ReloadConfig(object sender, EventArgs args)
        {
            Jotunn.Logger.LogInfo($"ReloadConfig => {sender}, {args}");
        }

        private void UpdateSettings(object sender, SettingChangedEventArgs args)
        {
            Jotunn.Logger.LogInfo($"UpdateSettings => {sender}, {args}");
        }

        private void AddLocalizations()
        {
            // ModLocalization.AddTranslation("English", new Dictionary<string, string>
            // {
            //     { "offload_button_text", "Offload" }
            // });
        }

        #endregion
    }
}
