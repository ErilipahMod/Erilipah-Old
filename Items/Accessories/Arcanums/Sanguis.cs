using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class Sanguis : ModItem
    {
        private new string DisplayName => "Arcanum " + GetType().Name;

        private new string Tooltip => "Buffs your damage, speed, and defense" +
            "\nIn return, the arcanum lowers your max health" +
            "\n'Qui cruentatur!'";

        private const int Rarity = ItemRarityID.Orange;
        private int[] Dimensions = new int[] { 28, 26 };

        private int[,] CraftingIngredients => new int[,] { { mod.ItemType("Gate"), 1 }, { mod.ItemType("SanguineAlloy"), 8 } };

        public override bool CanEquipAccessory(Player player, int slot)
        {
            int[] arcanums = new int[] {
                ItemType<Monumentis>(),
                ItemType<Noctis>(),
                ItemType<Sanguis>(),
                ItemType<Subterrania>(),
                ItemType<Tenebris>(),
                ItemType<Terra>(),
                ItemType<Veritas>(), };

            bool ifCan = player.armor.Take(10).Any(x => !arcanums.Contains(x.type));
            return ifCan;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (CanEquipAccessory(Main.player[item.owner], 0))
                return;
            var insert = tooltips.FindIndex(x => x.Name == "ItemName") + 1;
            var tooltipLine = new TooltipLine(mod, "Cannot use", "An Arcanum is equipped")
            {
                overrideColor = new Microsoft.Xna.Framework.Color(255, 125, 40)
            };
            tooltips.Insert(insert, tooltipLine);
        }

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault(DisplayName);
            base.Tooltip.SetDefault(Tooltip);
        }
        public override void SetDefaults()
        {
            item.accessory = true;
            item.maxStack = 1;
            item.rare = Rarity;
            Item.sellPrice(0, 0, 12, 50);

            item.width = Dimensions[0];
            item.height = Dimensions[1];
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            for (int i = 0; i < CraftingIngredients.GetLength(0); i++)
            {
                r.AddIngredient(CraftingIngredients[i, 0], CraftingIngredients[i, 1]);
            }

            r.SetResult(this, 1);
            r.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            player.allDamage += 0.08f;
            player.accRunSpeed *= 1.10f;
            player.maxRunSpeed *= 1.10f;
            player.statDefense += 4;

            player.statLifeMax2 -= (int)(player.statLifeMax2 * 0.2f);
        }
    }
}
