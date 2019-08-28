using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class NightsBane : ModItem
    {
        int Damage => 23;
        float Knockback => 5;
        int UseSpeed => 22;
        float ShootSpeed => 10.5f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Night's Bane");
            Tooltip.SetDefault("'Not to be confused with Light's Bane'");
        }

        public override void SetDefaults()
        {
            // most important
            item.width = 64;
            item.height = 64;
            item.useStyle = 1;
            item.maxStack = 1;

            // most changed
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            item.shootSpeed = ShootSpeed;

            // occasionally changed booleans
            item.noMelee = true;
            item.thrown = true;
            item.noUseGraphic = true;
            item.consumable = false;
            item.autoReuse = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = SoundID.Item1;
            item.shoot = mod.ProjectileType(GetType().Name.ToString() + "Proj");
        }
    }
    public partial class NightsBaneProj : ModProjectile
    {
        int Pierce => 2;
        int Dust => DustID.Electric;
        int FlightTime => 40;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.thrown = true;

            projectile.penetrate = 1 + Pierce;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            DisplayName.SetDefault("Night's Bane");
        }

        public override void AI()
        {
            int direction = projectile.velocity.X > 0 ? 1 : -1;

            if (++projectile.ai[0] > FlightTime)
            {
                projectile.velocity.Y += 0.165f;
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi / 2;
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, Dust, Scale: 0.92f);
            }
        }
    }
}
