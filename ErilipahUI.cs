using Erilipah.Items.Dracocide;
using Erilipah.Items.Templar;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Erilipah
{
    public partial class Erilipah : Mod
    {
        private UserInterface vitalityUI;
        private UserInterface shieldBrokenUI;
        private UserInterface infectUI;

        public static VitalityBar vitalityBar;
        public static ShieldBroken shieldBroken;
        public static InfectionUI infectionBar;

        public override void UpdateUI(GameTime gameTime)
        {
            if (vitalityUI != null && VitalityBar.Visible)
                vitalityUI.Update(gameTime);

            if (shieldBrokenUI != null && ShieldBroken.Visible)
                shieldBrokenUI.Update(gameTime);

            if (infectUI != null)
                infectUI.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBars = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBars == -1)
                return;

            layers.Insert(resourceBars + 2, new LegacyGameInterfaceLayer(
                "Erilipah: Infection",
                delegate
                {
                    infectUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }));

            layers.Insert(resourceBars, new LegacyGameInterfaceLayer(
                "Erilipah: Vitality",
                delegate
                {
                    if (VitalityBar.Visible)
                        vitalityUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }));

            layers.Insert(resourceBars, new LegacyGameInterfaceLayer(
                "Erilipah: Broken Shield",
                delegate
                {
                    if (ShieldBroken.Visible)
                        shieldBrokenUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }));
        }
    }
}
