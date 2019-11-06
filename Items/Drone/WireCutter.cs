using Erilipah.Projectiles;
using Terraria;

namespace Erilipah.Items.Drone
{
    public class Wirecutter : NewModItem
    {
        protected override string Tooltip => "Ignores gravity and veers towards foes";
        protected override UseTypes UseType => UseTypes.Thrown;
        protected override int[] Dimensions => new int[] { 32, 28 };
        protected override int Rarity => 2;

        protected override int Damage => 19;
        protected override int[] UseSpeedArray => new int[] { 17, 17 };
        protected override float Knockback => 5;
        protected override bool AutoReuse => false;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 9.5f;

        protected override int[,] CraftingIngredients => new int[,] { { mod.ItemType("PowerCoupling"), 8 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
    }
    public class WirecutterProj : NewModProjectile
    {
        protected override int[] Dimensions => AutoDimensions;
        protected override int DustType => 5;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override DamageTypes DamageType => DamageTypes.Thrown;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.rotation + Helper.RadiansPerTick(2.35f) * RotateDirection;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Terraria.ID.ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void AI()
        {
            base.AI();
            int npc = projectile.FindClosestNPC(350, x =>
            {
                bool closest = Collision.CanHit(projectile.position, projectile.width, projectile.height, x.position, x.width, x.height);
                return closest;
            }); ;
            if (npc != -1 && projectile.penetrate > 1)
            {
                projectile.GoTo(Terraria.Main.npc[npc].Center, 0.35f);
            }
        }
    }
}