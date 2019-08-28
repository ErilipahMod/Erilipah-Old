using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineBow : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 38;

            item.damage = 9;
            item.ranged = true;
            item.knockBack = 0.6f;

            item.useTime = 15;
            item.useAnimation = 15;
            item.useStyle = 5;
            item.UseSound = SoundID.Item5;
            item.autoReuse = true;

            item.rare = 2;
            item.shootSpeed = 6.5f;
            item.shoot = 3;
            item.useAmmo = AmmoID.Arrow;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-1, 2);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod, "InfectionModule", 5);
            r.AddTile(mod, "ShadaineCompressorTile");
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
