using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class Nidorose : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Enemies around your cursor are damaged and weakened");
        }
        public override void SetDefaults()
        {
            item.width = 48;
            item.height = 38;

            item.damage = 45;
            item.knockBack = 0;
            item.crit = 6;
            item.magic = true;
            item.mana = 10;

            item.noMelee = true;
            item.autoReuse = false;
            item.channel = true;
            item.useTurn = true;

            item.maxStack = 1;
            item.holdStyle = 1;
            item.autoReuse = false;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.Pink;

            item.shoot = mod.ProjectileType<NidoroseProj>();
            item.shootSpeed = 0.01f;
        }

        public override void HoldItem(Player player)
        {
            int index = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == item.shoot)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, item.shoot, item.damage, item.knockBack, player.whoAmI);
            }
            else
            {
                Main.projectile[index].Center = player.Center;
            }

            if (player.itemAnimation <= 0)
            {
                player.itemLocation.X -= 13 * player.direction;
                player.itemLocation.Y += 18;
            }
            // Add projectile spawn here

            if (player.whoAmI == Main.myPlayer)
                for (int i = 0; i < 50; i++)
                {
                    Vector2 dustPos = Main.MouseWorld + Vector2.UnitX.RotatedBy(i / 30f * MathHelper.TwoPi) * 65;
                    float bright = Lighting.Brightness((int)(dustPos.X / 16), (int)(dustPos.Y / 16));

                    Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(1, 1), mod.DustType<NPCs.ErilipahBiome.VoidParticle>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = bright * 0.8f;

                    dust = Dust.NewDustPerfect(dustPos, mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = (1 - bright) * 0.8f;
                }
        }

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);
        public override bool AltFunctionUse(Player player) => player.statMana >= 15;
    }

    public class NidoroseProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        private float Pulse { get => projectile.ai[1]; set => projectile.ai[1] = value; }
        private byte Shader { get => (byte)projectile.localAI[1]; set => projectile.localAI[1] = value; }

        private Vector2 DustPos(Vector2 forward, float speed, float time, float frequency, float amplitude)
        {
            Vector2 up = new Vector2(-forward.Y, forward.X);
            float upspeed = (float)Math.Cos(time * frequency) * amplitude * frequency;
            return up * upspeed + forward * speed;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            projectile.netUpdate = true;
            projectile.timeLeft = 2;

            // Ensure the projectile dies
            if (player.statMana < 8 || !player.channel)
            {
                projectile.Kill();
                return;
            }

            Focus();
        }

        private void Focus()
        {
            Player player = Main.player[projectile.owner];

            Pulse++;
            player.itemTime = 40;
            player.itemAnimation = 40;

            if (Pulse % 4 == 0)
                player.statMana -= 1;

            // Using Pulse to sync it up
            projectile.ai[0] = -1;
            if (Main.myPlayer == player.whoAmI)
            {
                projectile.netUpdate = true;
                projectile.Center = Main.MouseWorld;
                projectile.ai[0] = projectile.FindClosestNPC(100, true, true);
            }

            Vector2 pos = Vector2.Zero;
            if (Main.myPlayer == player.whoAmI)
            {
                pos = Main.MouseWorld;
            }
            if (projectile.ai[0] > -1)
            {
                NPC target = Main.npc[(int)projectile.ai[0]];
                pos = target.Center;
                if (target.immune[player.whoAmI] <= 0)
                {
                    target.netUpdate = true;
                    target.immune[player.whoAmI] = (int)(25 * (1 - Pulse / (300 + Pulse)));
                    player.ApplyDamageToNPC(target, projectile.damage + (int)Pulse / 30, 0, 0, false);
                    player.addDPS((int)((projectile.damage + Pulse / 30) / (60 / (25 * (1 - Pulse / (300 + Pulse))))));
                }
            }
            else
            {
                Pulse = 0;
            }

            if (pos == Vector2.Zero)
                return;

            projectile.localAI[0]++; // Local dust timer
            for (int i = 0; i < 3; i++)
            {
                float speed = 100 * (1 - Pulse / (500 + Pulse)) + 40; // Accelerate over time
                float amount = projectile.localAI[0] % speed / speed % 1f;
                Vector2 dustPos = Vector2.SmoothStep(player.Center, pos, amount); // Lerp towards the end pos
                if (i != 0)
                {
                    Vector2 sine = DustPos((pos - dustPos).SafeNormalize(Vector2.Zero), 10, projectile.localAI[0], 3, 8);
                    dustPos += sine;
                } // Create a fancy effect
                float bright = Lighting.Brightness((int)(dustPos.X / 16), (int)(dustPos.Y / 16));

                if (i == 0 && projectile.localAI[0] % 15 == 0)
                    Main.PlaySound(2, (int)dustPos.X, (int)dustPos.Y, 9, 1, -0.3f + Math.Min(0.6f, Pulse / 500f));

                Dust dust = Dust.NewDustPerfect(dustPos, mod.DustType<VoidParticle>(), Vector2.Zero);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
                dust.scale = bright * 1.25f;

                dust = Dust.NewDustPerfect(dustPos, mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
                dust.scale = (1 - bright) * 1.25f;

                // Dusts & done
            }

            for (byte i = 1; i == 1 || i == 2; i++)
            {
                // If it's not active and we don't have a shader, take it
                if (!Filters.Scene["Nidorose" + i].IsActive() && Shader == 0)
                {
                    Shader = i; // mark that it's taken with localAI1
                    Filters.Scene.Activate("Nidorose" + i, projectile.Center).GetShader().
                        UseColor(1, 5, 3f).UseTargetPosition(projectile.Center);
                }
            }

            // If we have a shader
            if (Shader > 0)
            {
                // Do the do
                float progress = MathHelper.Lerp(0, 1, Pulse / 60f % 1);
                Filters.Scene["Nidorose" + Shader].GetShader().UseProgress(progress).
                    UseOpacity(15 * (1 - progress));
            }
        }

        public override void Kill(int timeLeft)
        {
            // If we have a shader, disable it
            if (Shader > 0)
            {
                Filters.Scene["TyrantEye" + Shader].Deactivate();
            }
        }
    }

}
