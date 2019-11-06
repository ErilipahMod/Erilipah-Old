using Erilipah.Projectiles;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonThrowingDagger : NewModItem
    {
        protected override int Damage => 34;
        protected override int[] UseSpeedArray => new int[] { 21, 21 };
        protected override float Knockback => 3;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9;

        protected override int[] Dimensions => new int[] { 28, 28 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Thrown;

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 9 }, { Terraria.ID.ItemID.HellstoneBar, 8 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
        protected override string Tooltip => "Bursts into six weak projectiles on impact";
    }
    public class PhlogistonThrowingDaggerProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 28, 28 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override int TimeLeft => 60;

        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(3) * RotateDirection;

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            Helper.FireInCircle(projectile.Center, 6, ProjectileType<PhlogistonThrowingDaggerProjProj>(), 7, 4, 2, owner: projectile.owner);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 21;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
    public class PhlogistonThrowingDaggerProjProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 10, 10 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override int TimeLeft => 40;

        protected override TextureTypes TextureType => TextureTypes.Invisible;
        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(3) * RotateDirection;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 10;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance / 2));
            }
        }
    }
}
