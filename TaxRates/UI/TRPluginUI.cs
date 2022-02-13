using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using Dalamud.Game.Text;
using ImGuiScene;
using System.IO;
using TaxRates.Serialized;
using System.Threading.Tasks;
using TaxRates.Universalis;
using System.Threading;

namespace TaxRates
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class TRPluginUI : IDisposable
    {
        private TRPluginConf config;

        private List<TextureWrap> icons = new List<TextureWrap>();

        private RatesResponse rates;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public TRPluginUI(TRPluginConf configuration, List<TextureWrap> icons)
        {
            this.config = configuration;
            this.icons = icons;
            
            TRPlugin.Framework.Update += this.HandleFrameworkUpdateEvent;
        }

        private readonly List<(string, string)> worldList = new List<(string, string)>();
        private int selectedWorld = -1;
        private string selectedWorldName = string.Empty;
        private uint currentCharWorld = 0;

        List<int> results = new List<int>();

        private ulong playerId;

        public void Dispose()
        {
            TRPlugin.Framework.Update -= this.HandleFrameworkUpdateEvent;
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(465, 50), ImGuiCond.Appearing);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(200, 200), new Vector2(float.MaxValue, float.MaxValue));

            var localPlayer = TRPlugin.ClientState.LocalPlayer;

            if (ImGui.Begin("Current taxes rates", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text("Choose world: ");
                ImGui.SameLine();
                if (ImGui.BeginCombo("##world", this.selectedWorld > -1 ? this.worldList[this.selectedWorld].Item2 : string.Empty))
                {
                    foreach (var world in this.worldList)
                    {
                        var isSelected = this.selectedWorld == this.worldList.IndexOf(world);
                        if (ImGui.Selectable(world.Item2, isSelected))
                        {
                            this.selectedWorld = this.worldList.IndexOf(world);
                            this.selectedWorldName = world.Item1;
                            GetRates();
                        }

                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }

                    ImGui.EndCombo();
                }
            }

            ImGui.Separator();

            ImGui.BeginChild("Limsa", new Vector2(60, 70));
            ImGui.Image(icons[0].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.LimsaRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Gridania", new Vector2(60, 70));
            ImGui.Image(icons[1].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.GridaniaRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Ul'dah", new Vector2(60, 70));
            ImGui.Image(icons[2].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.UldahRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Ishgard", new Vector2(60, 70));
            ImGui.Image(icons[3].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.IshgardRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Kugane", new Vector2(60, 70));
            ImGui.Image(icons[4].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.KuganeRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Crystarium", new Vector2(60, 70));
            ImGui.Image(icons[5].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.CrystariumRate}%%");
            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("Old Sharlayan", new Vector2(60, 70));
            ImGui.Image(icons[6].ImGuiHandle, new Vector2(40, 40));
            ImGui.SetCursorPosX(30 - ImGui.GetFontSize());
            ImGui.Text($"{this.rates.SharlayanRate}%%");
            ImGui.EndChild();

            ImGui.SetCursorPosY(ImGui.GetWindowContentRegionMax().Y - ImGui.GetTextLineHeightWithSpacing() + 10);
            ImGui.Text("Data provided by Universalis (https://universalis.app/)");

            ImGui.End();
        }

        private void HandleFrameworkUpdateEvent(Framework framework)
        {
            if ((TRPlugin.ClientState.LocalContentId != 0 && this.playerId != TRPlugin.ClientState.LocalContentId) || (TRPlugin.ClientState.LocalPlayer.CurrentWorld.Id != currentCharWorld))
            {
                var localPlayer = TRPlugin.ClientState.LocalPlayer;
                if (localPlayer == null)
                {
                    return;
                }

                var currentDc = localPlayer.CurrentWorld.GameData.DataCenter;
                var dcWorlds = TRPlugin.Data.GetExcelSheet<World>()
                  .Where(w => w.DataCenter.Row == currentDc.Row && w.IsPublic)
                  .OrderBy(w => w.Name.ToString())
                  .Select(w =>
                  {
                      string displayName = w.Name;

                      if (localPlayer.CurrentWorld.Id == w.RowId)
                      {
                          displayName += $" {SeIconChar.Hyadelyn.ToIconChar()}";
                          currentCharWorld = localPlayer.CurrentWorld.Id;
                          selectedWorldName = localPlayer.CurrentWorld.GameData.Name;
                          GetRates();
                      }

                      return (w.Name.ToString(), displayName);
                  });

                this.worldList.Clear();
                this.worldList.AddRange(dcWorlds);

                this.selectedWorld = this.config.CrossWorld ? 0 : this.worldList.FindIndex(w => w.Item1 == localPlayer.CurrentWorld.GameData.Name);
                if (this.worldList.Count > 1)
                {
                    this.playerId = TRPlugin.ClientState.LocalContentId;
                }
            }

            if (TRPlugin.ClientState.LocalContentId == 0)
            {
                this.playerId = 0;
            }
        }

        private void GetRates()
        {
            Task.Run(async () =>
            {
                this.rates = await RequestClient
                .GetTaxRates(selectedWorldName, CancellationToken.None)
                .ConfigureAwait(false);
            });
        }
    }
}
