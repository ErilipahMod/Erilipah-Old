using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteArmCannon : NewModItem
    {
        protected override int Damage => 15;
        protected override int[] UseSpeedArray => new int[] { 20, 20 };
        protected override float Knockback => 3;

        protected override int ShootType => ProjectileType<SacraciteArmCannonFlak>();
        protected override float ShootSpeed => 9;
        protected override float ShootDistanceOffset => 14;
        protected override bool FiresProjectile => true;
        protected override float ShootInaccuracy => 3;

        protected override int[] Dimensions => new int[] { 42, 22 };
        protected override int Rarity => 2;
        protected override UseTypes UseType => UseTypes.Gun;
        protected override string Tooltip => "'Now with flaks!'";

        protected override int[,] CraftingIngredients => new int[,] { { mod.ItemType("SacraciteIngot"), 4 }, { ItemID.Sandgun, 1 } };
        protected override int CraftingTile => TileID.Anvils;

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            if (player.ownedProjectileCounts[ProjectileType<SacraciteArmCannonProj>()] < 1)
            {
                Projectile.NewProjectile(position, Vector2.Zero, ProjectileType<SacraciteArmCannonProj>(), 0, 0, player.whoAmI);
            }

            if (type == ItemID.MusketBall)
            {
                type = ProjectileType<SacraciteArmCannonFlak>();
            }
            position += Vector2.UnitY * 2;
            return true;
        }
    }
    public class SacraciteArmCannonProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 42, 22 };
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

            projectile.Center = plr.Center + mouse * 15 + (Vector2.UnitY * 4);
            projectile.timeLeft = (plr.itemAnimation > 0) ? 2 : 0;
            plr.heldProj = projectile.whoAmI;
        }
    }
    public class SacraciteArmCannonFlak : NewModProjectile
    {
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override int Pierce => 0;
        protected override float Gravity => 0;
        protected override int[] Dimensions => new int[] { 16, 22 };
        protected override int DustType => mod.DustType("GreenGemDust");

        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override TextureTypes TextureType => TextureTypes.Default;
        protected override float? Rotation => projectile.velocity.ToRotation() + MathHelper.PiOver2;
        protected override int TimeLeft => 90;

        public override void AI()
        {
            base.AI();
            projectile.velocity *= 0.9785f;
        }
        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            if (TimeLeft <= 1)
            {
                SpawnFlaks(28);
            }
            else
            {
                SpawnFlaks(10);
            }
        }

        private void SpawnFlaks(int damage)
        {
            if (Main.netMode != -1)
                for (int i = 0; i < 3; i++)
                {
                    float toRotateBy = MathHelper.PiOver4 / 5f;
                    float rotatedBy = -toRotateBy + (i * toRotateBy);
                    Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(rotatedBy) * 4.5f,
                        ProjectileType<SacraciteArmCannonFlakProj>(), damage, 0, projectile.owner)];
                    proj.friendly = true;
                    proj.damage = damage;
                }
        }
    }
    public class SacraciteArmCannonFlakProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetDefaults()
        {
            projectile.height = projectile.width = 10;
            projectile.friendly = projectile.ranged = true;
            projectile.alpha = 255;
            projectile.timeLeft = 180;
        }

        public override void AI()
        {
            base.AI();
            Dust.NewDustPerfect(projectile.Center, mod.DustType("GreenGemDust")).noGravity = true;
            Lighting.AddLight(projectile.Center, 0, 0.3f, 0.1f);
        }
    }
}