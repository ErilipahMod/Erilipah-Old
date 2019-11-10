using Erilipah.UI.Infection;
using Erilipah.UI.Notes;
using Erilipah.UI.ShieldBroken;
using Erilipah.UI.VitalityBar;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Erilipah
{
    public partial class Erilipah : Mod
    {
        private UserInterface _interfaceVitality;
        private UserInterface _interfaceShieldBroken;
        private UserInterface _interfaceInfectionBar;
        private UserInterface _interfaceNoteUI;

        public static VitalityBar vitalityBar;
        public static ShieldBrokenUI shieldBrokenUI;
        public static InfectionBar infectionBar;
        public static NoteUI noteUI;

        private void LoadUI()
        {
            vitalityBar = new VitalityBar();
            vitalityBar.Activate();
            _interfaceVitality = new UserInterface();
            _interfaceVitality.SetState(vitalityBar);

            shieldBrokenUI = new ShieldBrokenUI();
            shieldBrokenUI.Activate();
            _interfaceShieldBroken = new UserInterface();
            _interfaceShieldBroken.SetState(shieldBrokenUI);

            infectionBar = new InfectionBar();
            infectionBar.Activate();
            _interfaceInfectionBar = new UserInterface();
            _interfaceInfectionBar.SetState(infectionBar);

            noteUI = new NoteUI();
            noteUI.Activate();
            _interfaceNoteUI = new UserInterface();
            _interfaceNoteUI.SetState(noteUI);
        }

        private void UnloadUI()
        {
            VeritasAbilityKey = null;
            Bandolier = null;
            SoulBank = null;

            VitalityAbilityKey = null;
            _interfaceVitality = null;
            vitalityBar = null;

            _interfaceShieldBroken = null;
            shieldBrokenUI = null;

            _interfaceInfectionBar = null;
            infectionBar = null;

            _interfaceNoteUI = null;
            noteUI = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (NoteUI.Visible)
                _interfaceNoteUI.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            #region already functional UI
            int resourceBars = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBars == -1)
                goto skip1;

            layers.Insert(resourceBars + 2, new LegacyGameInterfaceLayer(
                "Erilipah: Infection",
                delegate
                {
                    _interfaceInfectionBar.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }, 
                InterfaceScaleType.UI
                ));

            layers.Insert(resourceBars, new LegacyGameInterfaceLayer(
                "Erilipah: Vitality",
                delegate
                {
                    if (vitalityBar?.Visible ?? false)
                        _interfaceVitality.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }, 
                InterfaceScaleType.UI
                ));

            layers.Insert(resourceBars, new LegacyGameInterfaceLayer(
                "Erilipah: Broken Shield",
                delegate
                {
                    if (shieldBrokenUI?.Visible ?? false)
                        _interfaceShieldBroken.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }));
        #endregion
        skip1:
            int mouseIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseIndex != -1)
            {
                layers.Insert(mouseIndex, new LegacyGameInterfaceLayer(
                    "Erilipah: Note UI",
                    delegate {
                        if (NoteUI.Visible)
                            _interfaceNoteUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
