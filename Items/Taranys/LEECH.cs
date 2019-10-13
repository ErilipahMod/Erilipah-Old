using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Erilipah.NPCs.ErilipahBiome;
using Erilipah.Items.ErilipahBiome;

namespace Erilipah.Items.Taranys
{
    class LEECH : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("L.E.E.C.H.");
            Tooltip.SetDefault(
                "Barrages your enemies with volleys of plague and death\n" +
                "'You get a cookie if you can guess what it stands for'"
                );

            //Item.staff[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 28;

            item.damage = 33;
            item.knockBack = 2f;
            item.crit = 6;

            item.ranged = true;
            item.noMelee = true;
            item.shoot = 0;
            item.shootSpeed = 0f;

            item.useTime =
            item.useAnimation = 20;

            item.useStyle = ItemUseStyleID.SwingThrow;
            item.autoReuse = true;
            item.useTurn = true;

            item.UseSound = SoundID.Item1;

            item.maxStack = 1;
            item.value = 1000;
            item.rare = ItemRarityID.Blue;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    public class VoidBolt : ModProjectile
    {
        public override string GlowTexture => Texture;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("LEECH");
        }
        public override void SetDefaults()
        {
            projectile.width = 6;
            projectile.height = 6;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 1;

            projectile.ranged = true;
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = true);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType<Wither>(), 80);
            projectile.Kill();
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.015f;
            Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(3, 3), mod.DustType<VoidParticle>(), Vector2.Zero)
                .noGravity = true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath9, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustPerfect(projectile.Center, mod.DustType<VoidParticle>(), 4 * Vector2.UnitX.RotatedBy(i / 10f * MathHelper.TwoPi))
                    .noGravity = true;
            }
        }
    }
}
