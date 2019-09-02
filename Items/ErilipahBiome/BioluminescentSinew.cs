using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    class BioluminescentSinew : ModItem
    {
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 28;
            item.height = 22;

            item.value = 100;
            item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 4);
            recipe.AddIngredient(mod.ItemType<Crystalline.CrystallineTileItem>(), 2);
            recipe.needWater = true;

            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
