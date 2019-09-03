using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome.Potions
{
    public class PurityPot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Purity Serum");
            Tooltip.SetDefault("Bloodborne and ambient infection are lowered by 30% each");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserRestorationPotion);

            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.NPCHit19.WithPitchVariance(-0.9f);
            item.useAnimation = item.useTime = 60;
            item.noUseGraphic = true;

            item.healLife = 0;
            item.healMana = 0;

            item.width = 32;
            item.height = 32;

            item.value = Item.sellPrice(0, 0, 1);
            item.rare = ItemRarityID.Blue;

            item.buffTime = (int)(3600 * 2.5f);
            item.buffType = mod.BuffType<PurityPotBuff>();
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness);
        }
        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffID.PotionSickness, item.buffTime);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<SlowingPot>());
            recipe.AddIngredient(mod.ItemType<ReductionPot>());
            recipe.AddIngredient(mod.ItemType<SoulRubble>());
            recipe.AddTile(TileID.Bottles);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        internal class PurityPotBuff : ModBuff
        {
            public override bool Autoload(ref string name, ref string texture)
            {
                texture = "Erilipah/Debuff";
                return true;
            }
            public override void SetDefaults()
            {
                DisplayName.SetDefault("Purity");
                Description.SetDefault("Slight resistance against all infection");
                Main.debuff[Type] = false;
                Main.buffNoSave[Type] = false;
            }
            public override void Update(Player player, ref int buffIndex)
            {
                player.I().reductionDmg *= 0.70f;
                player.I().reductionRate *= 0.70f;
                if (player.HasBuff(mod.BuffType<SlowingPot.SlowingPotBuff>()) || player.HasBuff(mod.BuffType<ReductionPot.ReductionPotBuff>()))
                {
                    player.DelBuff(buffIndex);
                    buffIndex--;
                }
            }
        }
    }
}
