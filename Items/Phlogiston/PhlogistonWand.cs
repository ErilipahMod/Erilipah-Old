using Erilipah.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;

namespace Erilipah.Items.Phlogiston
{
    public class PhlogistonWand : NewModItem
    {
        protected override int Damage => 52;
        protected override int[] UseSpeedArray => new int[] { 9, 25 };
        protected override float Knockback => 7;
        protected override int Mana => 12;

        protected override bool FiresProjectile => true;
        protected override float ShootSpeed => 10.5f;

        protected override int[] Dimensions => new int[] { 32, 32 };
        protected override int Rarity => 3;
        protected override UseTypes UseType => UseTypes.MagicStaff;
        protected override string Tooltip => "Fires three consecutive bolts";

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.reuseDelay = 2;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            int ai0 = 0;
            if (player.itemAnimation >= 24)
            {
                damage = 31;
            }
            else if (player.itemAnimation >= 16)
            {
                damage = 16;
                knockBack *= 0.5f;
                ai0 = 1;
            }
            else
            {
                damage = 5;
                knockBack *= 0.25f;
                ai0 = 2;
            }
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, ai0);
            return false;
        }

        protected override int[,] CraftingIngredients =>
            new int[,] { { mod.ItemType("StablePhlogiston"), 8 }, { Terraria.ID.ItemID.HellstoneBar, 7 } };
        protected override int CraftingTile => Terraria.ID.TileID.Anvils;
    }

    public class PhlogistonWandProj : NewModProjectile
    {
        protected override int[] Dimensions
        {
            get
            {
                switch ((int)projectile.ai[0])
                {
                    default:
                        return new int[] { 20, 24 };

                    case 1:
                        return new int[] { 16, 20 };

                    case 2:
                        return new int[] { 12, 16 };
                }
            }
        }
        protected override int FrameCount => 3;

        protected override int DustType => mod.DustType("DeepFlames");
        protected override int Pierce => 2 - (int)projectile.ai[0];
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override int ImmuneFrames => 10;

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees180;

        public override void AI()
        {
            base.AI();
            projectile.frame = (int)projectile.ai[0];
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            const int useSpeed = 20;
            const int chance = 60 / useSpeed;
            if (Terraria.Main.rand.NextBool(chance * 3))
            {
                target.AddBuff(Terraria.ID.BuffID.OnFire, 60 * (chance));
            }
        }
    }
}