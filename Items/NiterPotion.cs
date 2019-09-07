using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items
{
    internal class NiterPotion : ModItem
    {
        public static void InsertNiterTooltip(List<TooltipLine> tooltips, Mod mod, int amount)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + amount + " life"));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Niter Potion");
            Tooltip.SetDefault("Induces a potent power surge");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;
            item.potion = false;

            //item.width = 32;
            //item.height = 32;

            item.value = Item.sellPrice(0, 0, 1);
            item.rare = ItemRarityID.Green;

            item.buffTime = 500;
            item.buffType = mod.BuffType<NiterPotionBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InsertNiterTooltip(tooltips, mod, 50);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness);
        }

        public override void OnConsumeItem(Player player)
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " bursted with energy."),
                (int)(50 + player.statDefense * (Main.expertMode ? 0.75 : 0.5)), 0);
            player.AddBuff(BuffID.PotionSickness, 900);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<ErilipahBiome.SoulRubble>(), 1);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 4);
            recipe.AddRecipe();
        }

        internal class NiterPotionBuff : ModBuff
        {
            public override bool Autoload(ref string name, ref string texture)
            {
                texture = "Erilipah/Debuff";
                return true;
            }
            public override void SetDefaults()
            {
                DisplayName.SetDefault("Niter");
                Description.SetDefault("Power at a cost");
                Main.debuff[Type] = false;
                Main.buffNoSave[Type] = false;
            }
            public override void Update(Player player, ref int buffIndex)
            {
                float proportion = (500 + player.buffTime[buffIndex]) / 500f;

                player.allDamage *= proportion;
                player.moveSpeed *= proportion;
                player.maxRunSpeed *= proportion;
                player.wellFed = true;
                player.statDefense += (int)(20 * proportion);
                player.endurance = 0.35f * proportion;
            }
        }
    }
}
