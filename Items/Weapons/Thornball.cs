using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class Thornball : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 50 };
        protected override int Rarity => 2;

        protected override int Damage => 20;
        protected override int UseSpeed => 20;
        protected override float Knockback => 4;
        protected override float ShootSpeed => 11;
        protected override Vector2 ShootPosOffset => new Vector2(0, -8);

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.JungleSpores, 3);
            r.AddIngredient(ItemID.Stinger, 4);
            r.AddIngredient(ItemID.Vine, 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class ThornballProj : NewModProjectile
    {
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 35;
            height = 35;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => DustID.Grass;

        protected override int Pierce => 1;
        protected override int Bounce => 1;
        protected override float Gravity => 0.15f;
        protected override float? Rotation => Rotate(2);

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
    }
}
