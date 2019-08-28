using Terraria;
using Terraria.ID;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineArrow : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Arrow;
        protected override int[] Dimensions => new int[] { 10, 32 };
        protected override int Rarity => 2;
        protected override int Damage => 8;
        protected override float ShootSpeed => 8;

        protected override int[,] CraftingIngredients => new int[,]
        {
            { ItemID.WoodenArrow, 111 },
            { mod.ItemType<InfectionModule>(), 1 }
        };
        protected override int CraftingTile => mod.TileType<ShadaineCompressorTile>();
    }
    public class CrystallineArrowProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => mod.DustType("CrystallineDust");

        protected override int Pierce => -1;
        protected override int Bounce => -1;
        protected override float Gravity => 0;
        protected override int TimeLeft => 120;
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.extraUpdates = 2;
        }

        protected override int TrailThickness => 2;
        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.Perfect;

        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.ShadowFlame, 60);
            target.AddBuff(BuffID.Weak, 180);
        }

        public override bool OnTileCollide(Microsoft.Xna.Framework.Vector2 oldVelocity)
        {
            projectile.ai[0]++;
            if (projectile.ai[0] > 1)
            {
                projectile.Kill();
                return false;
            }
            if (projectile.velocity.X != oldVelocity.X)
                projectile.velocity.X = -oldVelocity.X;
            if (projectile.velocity.Y != oldVelocity.Y)
                projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Helper.FireInCircle(projectile.Center, 6, mod.ProjectileType<CrystallineArrowProjProj>(), projectile.damage / 2, 10);
        }
    }
    public class CrystallineArrowProjProj : NewModProjectile
    {
        protected override TextureTypes TextureType => TextureTypes.Invisible;
        protected override int[] Dimensions => new int[] { 12, 12 };
        protected override int DustType => mod.DustType("CrystallineDust");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0.05f;
        public override void AI()
        {
            base.AI();
            if (projectile.timeLeft > 160)
                projectile.friendly = false;
            else
                projectile.friendly = true;
        }

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees180;
    }
}
