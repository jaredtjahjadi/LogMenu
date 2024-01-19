using StardewModdingAPI;

namespace DialogueLogger
{
    internal class ModConfig
    {
        public int LogLimit { get; set; } = 30; // Desired number of logged messages. Default = 30
        public bool RecentMessagesFirst { get; set; } = true; // Option to start from newest or oldest messages. Default = true (most recent)
        public SButton LogButton { get; set; } = SButton.L; // Desired key to open the dialogue list. Default = L
    }
}
