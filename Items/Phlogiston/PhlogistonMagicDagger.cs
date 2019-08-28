using Microsoft.Xna.Framework;
using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonMagicDagger : NewModItem
    {
        protected override int Damage => 92;
        protected override int[] UseSpeedArray => new int[] { 35, 35 };
        protected override float Knockback => 4;
        protected override int Crit => 4;
        protected override int Mana => 19;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 10;
        protected override int ShootType => mod.ProjectileType<PhlogistonMagicDaggerProj>();

        protected override int[] Dimensions => new int[] { 18, 40 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override string Tooltip => "Leaves a trail of damaging orbs in its wake";

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 8 }, { Terraria.ID.ItemID.HellstoneBar, 7 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.magic = true;
            item.thrown = false;
        }
    }
    public class PhlogistonMagicDaggerProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 18, 40 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override int TrailThickness => 2;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;

        public override void AI()
        {
            base.AI();
            if (projectile.penetrate == 1)
            {
                projectile.damage = 41;
            }

            if (++projectile.ai[0] % 2 == 0)
            {
                Terraria.Projectile.NewProjectile(projectile.Center, projectile.velocity * 0,
                    mod.ProjectileType<PhlogistonMagicDaggerProjProj>(), (int)(projectile.damage / 4f), 0, projectile.owner);
            }
        }
    }
    public class PhlogistonMagicDaggerProjProj : NewModProjectile
    {
        public override Color? GetAlpha(Color lightColor)
        {
            if (projectile.ai[0] == -1)
            {
                projectile.ai[0] = Terraria.Main.rand.Next(3);
            }

            if (projectile.ai[0] == 1)
            {
                return new Color(219, 63, 39);
            }
            if (projectile.ai[0] == 2)
            {
                return new Color(255, 203, 79);
            }
            return base.GetAlpha(lightColor);
        }
        protected override int[] Dimensions => new int[] { 8, 8 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 3;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override int TimeLeft => 63;
        protected override int ImmuneFrames => 20;

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(3) * RotateDirection;
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.ai[0] = -1;
        }
    }
}
