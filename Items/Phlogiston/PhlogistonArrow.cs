using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonArrow : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Arrow;
        protected override int[] Dimensions => new int[] { 14, 40 };
        protected override int Rarity => 3;
        protected override int Damage => 7;
        protected override string Tooltip => "Splits into smaller bolts on impact";

        protected override float ShootSpeed => 9;

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.WoodenArrow, 333);
            r.AddIngredient(ItemType<StablePhlogiston>(), 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this, 333);
            r.AddRecipe();
        }
    }

    public class PhlogistonArrowProj : NewModProjectile
    {
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Width = 20;
            hitbox.Height = 30;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            width = 20;
            height = 30;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => -0.05f;
        protected override int AIStyle => 1;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;

        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;

        public override void Kill(int timeLeft)
        {
            Helper.FireInCircle(projectile.Center, 8, ProjectileType<PhlogistonArrowProjProj>(),
                projectile.damage - 7, 7, projectile.knockBack, owner: projectile.owner);
        }
    }

    public class PhlogistonArrowProjProj : NewModProjectile
    {
        protected override TextureTypes TextureType => TextureTypes.Invisible;
        protected override int[] Dimensions => new int[] { 12, 16 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        public override void AI()
        {
            base.AI();
            projectile.friendly = true;

            Vector2 pos = new Vector2(
                (float)System.Math.Sin(projectile.position.X),
                (float)System.Math.Cos(projectile.position.Y)) * 8;
            Dust.NewDustPerfect(projectile.Center + pos, mod.DustType("DeepFlames")).noGravity = true;
        }

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees180;
    }
}
