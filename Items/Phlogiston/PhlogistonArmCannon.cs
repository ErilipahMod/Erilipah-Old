using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonArmCannon : NewModItem
    {
        protected override int Damage => 15;
        protected override int[] UseSpeedArray => new int[] { 7, 7 };
        protected override float Knockback => 2;

        protected override float ShootSpeed => 15;
        protected override int ShootType => ProjectileType<PhlogistonArmCannonProjProj>();
        protected override float ShootDistanceOffset => 14;
        protected override bool FiresProjectile => true;

        protected override int[] Dimensions => new int[] { 28, 14 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.Gun;
        protected override string Tooltip => "Fires heat-seeking pelts";

        protected override int[,] CraftingIngredients => new int[,] { { mod.ItemType("StablePhlogiston"), 9 }, { ItemID.HellstoneBar, 8 } };
        protected override int CraftingTile => TileID.Anvils;

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (type == ProjectileID.Bullet)
            {
                type = ShootType;
                damage = 19;
                knockBack = 1;
                Vector2 speed = new Vector2(speedX, speedY);
                speed.Normalize();
                speed *= 7;
                speedX = speed.X;
                speedY = speed.Y;
            }
            if (player.ownedProjectileCounts[ProjectileType<PhlogistonArmCannonProj>()] < 1)
                Projectile.NewProjectile(position, Vector2.Zero, ProjectileType<PhlogistonArmCannonProj>(), 0, 0, player.whoAmI);
            position += Vector2.UnitY * 2;
            return true;
        }
    }
    public class PhlogistonArmCannonProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 28, 14 };
        protected override int DustType => 6;
        protected override int Pierce => InfinitePierceAndBounce;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override bool[] DamageTeam => new bool[] { false, false };

        protected override DamageTypes DamageType => DamageTypes.None;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.None;
        protected override TextureTypes TextureType => TextureTypes.ItemClone;
        protected override float? Rotation => (Main.MouseWorld - projectile.Center).ToRotation();

        public override void SetDefaults()
        {
            base.SetDefaults();
            drawHeldProjInFrontOfHeldItemAndArms = true;
        }
        public override void AI()
        {
            base.AI();
            Player plr = Main.player[projectile.owner];
            Vector2 mouse = (Main.MouseWorld - projectile.Center);
            mouse.Normalize();

            projectile.Center = plr.Center + mouse * 15;
            projectile.timeLeft = (plr.itemAnimation > 0) ? 2 : 0;
            plr.heldProj = projectile.whoAmI;
        }
    }
    public class PhlogistonArmCannonProjProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 14, 10 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType
        {
            get
            {
                return (++projectile.localAI[0] > 10) ? DustTrailTypes.PerfectNoGravity : DustTrailTypes.None;
            }
        }

        protected override float? Rotation => projectile.velocity.ToRotation() + MathHelper.ToRadians(180);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public override void AI()
        {
            base.AI();
            const int viewDist = 200;
            if (projectile.penetrate > 1 && projectile.FindClosestNPC(viewDist) >= 0)
            {
                NPC npc = Main.npc[projectile.FindClosestNPC(viewDist)];
                projectile.velocity = projectile.GoTo(npc.Center, 0.25f);
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.damage /= 4;
            const int useSpeed = 7;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * chance);
            }
        }
    }
}
