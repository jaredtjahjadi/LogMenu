using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueLogger
{
    internal class ModConfig
    {
        public int LogLimit { get; set; } = 20; // Desired number of logged messages. Default = 20
        public SButton LogButton { get; set; } = SButton.L; // Desired key to open the dialogue list. Default = L
    }
}
