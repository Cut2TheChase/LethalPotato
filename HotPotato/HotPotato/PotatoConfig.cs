using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace HotPotato
{
    [DataContract]
    public class PotatoConfig : SyncedConfig<PotatoConfig>
    {
        public ConfigEntry<float> DISPLAY_DEBUG_INFO { get; private set; }

        [DataMember] public SyncedEntry<int> SCRAP_RARITY { get; private set; }
        [DataMember] public SyncedEntry<float> BATTERY_USAGE { get; private set; }
        [DataMember] public SyncedEntry<bool> BATTERY_RANDOM { get; private set; }

        public PotatoConfig(ConfigFile cfg) : base("LethalPotato")
        {
            ConfigManager.Register(this);

            SCRAP_RARITY = cfg.BindSyncedEntry(
                    "General",                          // Config section
                    "Rarity",                     // Key of this config
                    25,                    // Default value
                    "How rare the scrap is, higher number means more likely to spawn"    // Description
            );

            BATTERY_USAGE = cfg.BindSyncedEntry(
                    "General",                  // Config subsection
                    "BatteryUsage",                  // Key of this config
                    165f,                               // Default value
                    "How quickly the battery drains, a lower number means faster"         // Description
            );

            BATTERY_RANDOM = cfg.BindSyncedEntry(
                    "General",                  // Config subsection
                    "BatteryUsageRandom",                  // Key of this config
                    false,                               // Default value
                    "If enabled, all Hot Potatoes will start at different charge values"         // Description
            );

            SyncComplete += DoSomethingAfterSync;
        }

        public void DoSomethingAfterSync(object sender, EventArgs args)
        {
            UnityEngine.Debug.Log("LETHAL POTATO CONFIG SETTINGS SYNCED AMONGST CLIENTS");
        }
    }
}
