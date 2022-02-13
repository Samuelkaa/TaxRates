using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace TaxRates
{
    [Serializable]
    public class TRPluginConf : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool CrossWorld { get; set; }

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
