using Terraria.ID;
using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Erilipah.Items.Weapons;

namespace Erilipah.Items.Taranys
{
    public class ScepterOfEternalAbyss : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Fires powerful ethereal knives");
            Item.staff[item.type] = true;
        }
        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 46;

            item.damage = 41;
            item.knockBack = 4;
            item.crit = 8;
            item.magic = true;

            item.reuseDelay = 5;
            item.useTime = 12;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;

            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = true;

            item.UseSound = SoundID.Item4;

            item.maxStack = 1;
            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<ScepterProj1>();
            item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position.X += Main.rand.NextFloat(-15, 15);
            position.Y += Main.rand.NextFloat(-7, 5);

            Vector2 to = position.To(Main.MouseWorld, new Vector2(speedX, speedY).Length());

            if (Main.rand.NextBool())
                type = mod.ProjectileType<ScepterProj2>();

            return base.Shoot(player, ref position, ref to.X, ref to.Y, ref type, ref damage, ref knockBack);
        }
    }

    public class ScepterProj2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scepter");
        }
        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.magic = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType<Wither>(), 300);
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, DustID.PurpleCrystalShard, 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
            }
        }
    }

    public class ScepterProj1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scepter");
        }
        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.magic = true;
            projectile.maxPenetrate = 6;
            projectile.penetrate = 6;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void AI()
        {
            projectile.rotation += projectile.velocity.X > 0 ? 0.2f : -0.2f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.velocity *= 0.885f;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath14, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, DustID.PurpleCrystalShard, 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
            }
        }
    }
}
