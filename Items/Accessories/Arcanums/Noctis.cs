using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class Noctis : ModItem
    {
        private new string DisplayName => "Admonitus " + GetType().Name;

        private new string Tooltip => "Decreases maximum health and increases other stats during night" +
            "\nIncreases maximum health and decreases other stats during day" +
            "\n'Timebis nocte!'";

        private const int Rarity = ItemRarityID.Orange;
        private int[] Dimensions = new int[] { 28, 28 };

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

        private int[,] CraftingIngredients
        {
            get
            {
                return new int[,] { { mod.ItemType("Sanguis"), 1 }, { mod.ItemType("Tenebris"), 1 }, { mod.ItemType("Gate"), 1 } };
            }
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
            Item.sellPrice(0, 0, 50);

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

            r.AddTile(TileID.DemonAltar);
            r.SetResult(this, 1);
            r.AddRecipe();
        }

        public override void UpdateEquip(Player player)
        {
            if (Main.dayTime)
            {
                player.allDamage -= 0.1f;
                player.statLifeMax2 = (int)(player.statLifeMax2 * 1.15f);
                player.accRunSpeed *= 0.90f;
                player.maxRunSpeed *= 0.90f;
                player.statDefense -= 3;
            }
            else
            {
                player.allDamage += 0.1f;
                player.statLifeMax2 = (int)(player.statLifeMax2 * 0.75f);
                player.accRunSpeed *= 1.15f;
                player.maxRunSpeed *= 1.15f;
                player.statDefense += 4;
            }
        }
    }
}
