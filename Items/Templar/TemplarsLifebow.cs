using Erilipah.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Templar
{
    public class TemplarsLifebow : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Bow;
        protected override int[] Dimensions => new int[] { 28, 46 };
        protected override int Rarity => 2;
        protected override Vector2? HoldoutOffSet => new Vector2(-3, 0);

        protected override int Damage => 17;
        protected override int UseSpeed => 25;
        protected override float Knockback => 3;
        protected override float ShootSpeed => 11;
        protected override int ShootType => ProjectileID.WoodenArrowFriendly;
        protected override float ShootInaccuracy => 3;

        protected override string DisplayName => "Templar's Lifebow";
        protected override string Tooltip => "Right click to fire a burst of healing bolts\n" +
            "Uses " + vitality + " vitality";

        private const int vitality = 30;

        private int Proj2 => ProjectileType<TemplarsLifebowProj>();

        public override bool AltFunctionUse(Player player) => player.GetModPlayer<Vitality>().CurrentVitality >= vitality;
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                type = Proj2;
                Vitality modPlayer = player.GetModPlayer<Vitality>();
                modPlayer.SubVitality(vitality);

                for (float i = position.Y - 10; i <= position.Y + 10; i += 10)
                {
                    Projectile.NewProjectile(position.X, i, speedX, speedY, type, damage, knockBack, player.whoAmI);
                }
                return false;
            }
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            return true;
        }
    }
    public class TemplarsLifebowProj : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 10, 16 };
        protected override int DustType => 74;

        protected override int Pierce => 0;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;

        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;

        private const int heal = 11;

        private Player CheckCollision()
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead && Main.player[i] != Main.player[projectile.owner] &&
                    projectile.Colliding(projectile.Hitbox, Main.player[i].Hitbox))
                    return Main.player[i];
            }
            return null;
        }

        private void CheckHeal()
        {
            Player player = CheckCollision();
            if (player != null)
            {
                player.Heal(heal);
                projectile.Kill();
            }
        }
        public override void AI()
        {
            base.AI();
            CheckHeal();
        }
        protected override bool[] DamageTeam => new bool[] { false, false };
    }
}
