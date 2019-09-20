using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class SoulCleanser : ModItem
    {
        public override string Texture => Helper.Invisible;
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 38;
            item.rare = 5;
            item.maxStack = 1;
            item.value = 0;
        }
    }
    public class SoulHearth : ModItem
    {
        public override string Texture => Helper.Invisible;
        private int Banked => Main.player[item.owner].GetModPlayer<ErilipahPlayer>().bankedDamage;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul Hearth");
            Tooltip.SetDefault("Stores 1/4 of damage dealt as life in a bank\nCannot store more than 500 life\nCannot store life while withdrawing life\n" +
                "Automatically withdraws life when needed\n'Giving the trapped souls a hearth and home has relieved them'");
        }

        public override void SetDefaults()
        {
            item.accessory = true;
            item.expert = true;
            item.maxStack = 1;

            item.width = 32;
            item.height = 44;

            item.value = 120000;
            item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<Taranys.TorchOfSoul>());
            r.AddIngredient(mod.ItemType<SoulCleanser>());
            r.AddTile(mod.TileType<Biomes.ErilipahBiome.Tiles.Altar>());
            r.SetResult(this);
            r.AddRecipe();
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
