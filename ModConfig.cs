﻿using StardewModdingAPI;

namespace LogMenu
{
    internal class ModConfig
    {
        public bool StartFromBottom { get; set; } = true; // Option to have menu start from bottom or top. Default = true
        public bool OldestToNewest { get; set; } = true; // Option to have oldest messages at top of menu. Default = true
        public bool NonNPCDialogue { get; set; } = true; // Option to log non-NPC dialogue (e.g., when interacting with objects). Default = true
        public bool ToggleHUDMessages { get; set; } // Option to log HUD messages. Default = false
        public int LogLimit { get; set; } = 50; // Desired number of maixmum logged messages. Default = 50
        public SButton LogButton { get; set; } = SButton.L; // Desired key to open the dialogue list. Default = L
    }
}
