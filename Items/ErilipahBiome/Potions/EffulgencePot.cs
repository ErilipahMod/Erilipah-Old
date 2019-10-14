using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome.Potions
{
    public class EffulgencePot : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Effulgence Potion");
            Tooltip.SetDefault("Brightens the darkness of Erilipah\nMakes bloodborne infection much more severe");
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

            item.buffTime = (int)(3600 * 2.5f);
            item.buffType = mod.BuffType<EffulgencePotBuff>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.SoulofLight, 3);
            recipe.AddIngredient(mod.ItemType<Biomes.ErilipahBiome.Hazards.MushroomItem>(), 2);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddTile(TileID.Bottles);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        internal class EffulgencePotBuff : ModBuff
        {
            public override void SetDefaults()
            {
                DisplayName.SetDefault("Effulgence");
                Description.SetDefault("The constricting darkness relents");
                Main.debuff[Type] = false;
                Main.buffNoSave[Type] = false;
            }
            public override void Update(Player player, ref int buffIndex)
            {
                player.I().reductionDmg *= 1.50f;
                if (player.InErilipah())
                    Lighting.AddLight(player.Center, 0.85f, 0.7f, 1f);
                if (player.whoAmI == Main.myPlayer)
                    Erilipah.erilipahIsBright = true;
            }
        }
    }
}
