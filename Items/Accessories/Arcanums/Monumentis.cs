using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class Monumentis : ModItem
    {
        private new string DisplayName => "Arcanum " + GetType().Name;

        private new string Tooltip => "Saves you from death, with a six minute cooldown" +
            "\nRevival halves defense and nullifies healing for one minute" +
            "\n'Evigilare faciatis!'";

        private const int Rarity = ItemRarityID.Orange;
        private int[] Dimensions = new int[] { 28, 26 };

        private int[,] CraftingIngredients => new int[,] { { mod.ItemType("Gate"), 1 }, { mod.ItemType("SacraciteCore"), 4 } };

        public override bool CanEquipAccessory(Player player, int slot)
        {
            int[] arcanums = new int[] {
                mod.ItemType<Monumentis>(),
                mod.ItemType<Noctis>(),
                mod.ItemType<Sanguis>(),
                mod.ItemType<Subterrania>(),
                mod.ItemType<Tenebris>(),
                mod.ItemType<Terra>(),
                mod.ItemType<Veritas>(), };

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
    }
}
