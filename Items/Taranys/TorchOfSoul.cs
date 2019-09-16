using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class TorchOfSoul : ModItem
    {
        private int Banked => Main.player[item.owner].GetModPlayer<ErilipahPlayer>().bankedDamage;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spirit Trapper");
            Tooltip.SetDefault("Stores half of damage dealt as life in a bank\nCannot store more than 500 life\nCannot store life while withdrawing life\nUse the Soul Bank key to withdraw life");
        }

        public override void SetDefaults()
        {
            item.accessory = true;
            item.maxStack = 1;

            item.width = 32;
            item.height = 44;

            item.value = 25000;
            item.rare = ItemRarityID.LightRed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Banked > 0)
            tooltips.Add(new TooltipLine(mod, "Stored Dmg", "Use to heal " + Banked + " life")
            {
                overrideColor = CombatText.HealLife
            });
        }
    }
}
