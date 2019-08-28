using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Arcanums
{
    public class Veritas : ModItem
    {
        private new string DisplayName => "Veritas";

        private new string Tooltip => "Pressing Veritas Ability hotkey trades your max life for damage (toggleable)" +
            "\nSaves you from death, with an eight minute cooldown" +
            "\nCooldown is decreased to four minutes when underground or nighttime" +
            "\nRevival cripples you temporarily, less so if underground or nighttime" +
            "\n10% increased mining speed" +
            "\nVarious potent buffs given when underground or nighttime" +
            "\n'Nihil occultatum est!'";

        private const int Rarity = ItemRarityID.Orange;
        private int[] Dimensions = new int[] { 40, 40 };

        private int[,] CraftingIngredients
        {
            get
            {
                return new int[,] { { mod.ItemType("Subterrania"), 1 }, { mod.ItemType("Noctis"), 1 }, { mod.ItemType("Gate"), 2 } };
            }
        }

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
            item.value = Item.sellPrice(0, 1);

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
            Lighting.AddLight(player.Center, 0.15f, 0.15f, 0);

            player.pickSpeed *= 1.10f;

            if (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight || !Main.dayTime)
            {
                player.allDamage += 0.1f;
                player.statLifeMax2 = (int)(player.statLifeMax2 * 1.2f);
                player.maxRunSpeed *= 1.05f;
                player.accRunSpeed *= 1.10f;
                player.statDefense += 4;
            }
        }
    }
}