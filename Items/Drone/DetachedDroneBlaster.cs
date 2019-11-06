using Erilipah.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Erilipah.Items.Drone
{
    public class DetachedDroneBlaster : NewModItem
    {
        protected override string Tooltip => "'Still intact!'";
        protected override UseTypes UseType => UseTypes.Gun;
        protected override int[] Dimensions => new int[] { 34, 26 };
        protected override int Rarity => 2;
        protected override LegacySoundStyle UseSound => SoundID.NPCDeath14;

        protected override bool AutoReuse => false;
        protected override int Damage => 38;
        protected override int UseSpeed => 45;
        protected override int Crit => 26;
        protected override float Knockback => 3;
        protected override float ShootSpeed => 11;
        protected override float ShootDistanceOffset => 28;
        protected override Vector2? HoldoutOffSet => new Vector2(-8, 0);
        protected override Vector2 ShootPosOffset => new Vector2(0, -2);

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int dust = mod.DustType("DeepFlames");
            Vector2 speed = new Vector2(speedX, speedY).SafeNormalize(Vector2.Zero);
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(position, 0, 0, dust, speed.X * 6, speed.Y * 6);
            }

            player.velocity -= speed * 2.25f;
            type = ShootType;
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }
    public class DetachedDroneBlasterProj : NewModProjectile
    {
        public override string Texture => "Erilipah/NPCs/Drone/DroneGunnerProj";
        protected override int[] Dimensions => new int[] { 12, 22 };
        protected override int DustType => DustID.AmberBolt;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override bool NoDustLight => true;
        protected override float TrailScale => 0.8f;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation() + Degrees90;
    }
}
