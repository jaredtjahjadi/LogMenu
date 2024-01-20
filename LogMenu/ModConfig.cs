using StardewModdingAPI;

namespace LogMenu
{
    internal class ModConfig
    {
        public int LogLimit { get; set; } = 30; // Desired number of logged messages. Default = 30
        public bool StartFromBottom { get; set; } = true; // Option to have menu start from bottom or top. Default = true (most recent)
        public SButton LogButton { get; set; } = SButton.L; // Desired key to open the dialogue list. Default = L
    }
}
