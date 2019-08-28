using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class MoonStingerFriendly : ModProjectile
    {
        public override string Texture => mod.Name + "/NPCs/LunarBee/CrystalShardSmall";
        public override string GlowTexture => Texture;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = 2;
            Main.projFrames[projectile.type] = 1;
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moon Stinger");
        }

        public override void AI()
        {
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + MathHelper.PiOver2;
            projectile.localAI[0] += 1f;
            if (projectile.localAI[0] > 300f) //projectile time left before disappears
            {
                projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(0, projectile.position);
            for (int a = 0; a < 5; a++) Dust.NewDust(projectile.Center, projectile.width, projectile.height, mod.DustType("MoonFire"));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType("LunarBreakdown"), 240);
        }
    }
}