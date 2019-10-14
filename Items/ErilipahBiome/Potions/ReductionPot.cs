using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome.Potions
{
    public class ReductionPot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Antihemic Serum");
            Tooltip.SetDefault("Halves infection from damage");
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

            //item.width = 32;
            //item.height = 32;

            item.value = Item.sellPrice(0, 0, 1);
            item.rare = ItemRarityID.Blue;

            item.buffTime = 3600 * 5;
            item.buffType = mod.BuffType<ReductionPotBuff>();
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

            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 4);
            recipe.AddIngredient(mod.ItemType<Biomes.ErilipahBiome.Hazards.MushroomItem>(), 1);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddTile(TileID.Bottles);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        internal class ReductionPotBuff : ModBuff
        {
            public override void SetDefaults()
            {
                DisplayName.SetDefault("Antihemic");
                Description.SetDefault("Reduced infection from damage");
                Main.debuff[Type] = false;
                Main.buffNoSave[Type] = false;
            }
            public override void Update(Player player, ref int buffIndex)
            {
                player.I().reductionDmg *= 0.50f;
                if (player.HasBuff(mod.BuffType<SlowingPot.SlowingPotBuff>()) || player.HasBuff(mod.BuffType<PurityPot.PurityPotBuff>()))
                {
                    player.DelBuff(buffIndex);
                    buffIndex--;
                }
            }
        }
    }
}
