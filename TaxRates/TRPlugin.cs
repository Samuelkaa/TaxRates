using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game;
using System.IO;
using Dalamud.Data;
using ImGuiScene;
using System.Collections.Generic;
using System;

namespace TaxRates
{
    public class TRPlugin : IDalamudPlugin
    {
        public string Name => "Tax Rates";

        private const string mainCommand = "/taxes";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private TRPluginConf Configuration { get; init; }
        private TRPluginUI PluginUi { get; init; }

        [PluginService]
        internal static Framework Framework { get; private set; } = null!;

        [PluginService]
        internal static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        internal static DataManager Data { get; private set; } = null!;

        List<TextureWrap> icons = new List<TextureWrap>();

        public TRPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as TRPluginConf ?? new TRPluginConf();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            IconsInit();

            this.PluginUi = new TRPluginUI(this.Configuration, icons);

            this.CommandManager.AddHandler(mainCommand, new CommandInfo(OnCommand)
            {
                HelpMessage = "Show retainer tax rates in cities."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            GC.SuppressFinalize(this);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.Visible = !this.PluginUi.Visible;
        }

        private void IconsInit()
        {
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Limsa.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Gridania.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Ul'dah.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Ishgard.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Kugane.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Crystarium.png")));
            icons.Add(this.PluginInterface.UiBuilder.LoadImage(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Icons", "Old Sharlayan.png")));
        }
    }
}
