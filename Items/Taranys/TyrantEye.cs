using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class TyrantEye : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eye of the Tyrant");
            Tooltip.SetDefault("Uses 15 mana per pulse\nHold left click to stare; stare strengthens over time\nRight click to pulse, damaging and repelling enemies");
        }

        public override void SetDefaults()
        {
            item.damage = 32;
            item.knockBack = 1.65f;
            item.crit = 10;
            item.magic = true;
            item.mana = 8;
            item.noMelee = true;
            item.autoReuse = false;
            item.channel = true;
            item.useTurn = true;

            item.UseSound = SoundID.Item103;

            item.maxStack = 1;
            item.useTime = 22;
            item.useAnimation = 10;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.holdStyle = 1;
            item.autoReuse = false;

            item.width = 30;
            item.height = 42;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.shoot = mod.ProjectileType<EyeProj>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                player.statMana -= 7;
                player.itemTime = 30;
                player.itemAnimation = 30;
                if (player.whoAmI == Main.myPlayer)
                {
                    position = Main.MouseWorld;
                    Main.PlaySound(2, (int)position.X, (int)position.Y, 125, 1, 0.5f);
                }
            }

            if (player.altFunctionUse == 2 || !player.ownedProjectileCounts.Contains(type))
                Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack, player.whoAmI, player.altFunctionUse == 2 ? -2 : 0);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation <= 0)
            {
                player.itemLocation.X -= 13 * player.direction;
                player.itemLocation.Y += 18;
            }

            if (player.whoAmI == Main.myPlayer)
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = Main.MouseWorld + Vector2.UnitX.RotatedBy(i / 30f * MathHelper.TwoPi) * 50;
                    float bright = Lighting.Brightness((int)(dustPos.X / 16), (int)(dustPos.Y / 16));

                    Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(1, 1), mod.DustType<VoidParticle>(), Vector2.Zero);
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

    public class EyeProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        private bool IsRightClick => projectile.ai[0] == -2;
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

            // First tick
            if (Pulse == 0)
            {
                Pulse = 50;
            }

            // Ensure the projectile dies
            if (player.statMana < 8 || (!player.channel && !IsRightClick) || (Pulse > 600 && IsRightClick))
            {
                projectile.Kill();
                return;
            }

            if (!IsRightClick)
            {
                Pulse++;
                player.itemTime = 40;
                player.itemAnimation = 40;

                if (Pulse % 5 == 0)
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
                    pos = Main.MouseWorld;
                if (projectile.ai[0] > -1)
                {
                    NPC target = Main.npc[(int)projectile.ai[0]];
                    pos = target.Center;
                    if (target.immune[player.whoAmI] <= 0)
                    {
                        target.netUpdate = true;
                        target.immune[player.whoAmI] = (int)(20 * (1 - Pulse / (300 + Pulse)));
                        player.ApplyDamageToNPC(target, projectile.damage + (int)Pulse / 35, 0, 0, false);
                        player.addDPS((int)((projectile.damage + Pulse / 35) / (60 / (20 * (1 - Pulse / (300 + Pulse))))));
                    }
                }
                else
                    Pulse = 0;

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
            }
            else // Pulse outward
            {
                const float effectiveDist = 20;
                Pulse += effectiveDist;

                #region Pulse
                for (int i = 0; i < Pulse * 0.2f; i++)
                {
                    // Create dusts in an even ring around the NPC
                    float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, i / (Pulse * 0.2f));
                    Vector2 position = projectile.Center + Vector2.UnitX.RotatedBy(rotation) * Pulse;
                    float bright = Lighting.Brightness((int)(position.X / 16), (int)(position.Y / 16));

                    Dust dust = Dust.NewDustPerfect(position, mod.DustType<VoidParticle>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = bright * 1.25f;

                    dust = Dust.NewDustPerfect(position, mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = (1 - bright) * 1.1f;
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    float distanceToNPC = Vector2.Distance(proj.Center, projectile.Center);

                    bool validType = proj.aiStyle == 0 || proj.aiStyle == 1 || proj.aiStyle == 2 || proj.aiStyle == 8 ||
                        proj.aiStyle == 21 || proj.aiStyle == 24 || proj.aiStyle == 28 || proj.aiStyle == 29 || proj.aiStyle == 131;
                    validType &= !proj.WipableTurret && !proj.hide && !proj.minion;

                    if (validType && proj.active && distanceToNPC > Pulse - effectiveDist && distanceToNPC < Pulse + effectiveDist && proj.hostile)
                    {
                        proj.velocity = projectile.Center.To(proj.Center, proj.velocity.Length());
                    }
                }

                for (int i = 0; i < 200; i++)
                {
                    NPC n = Main.npc[i];
                    float distanceToNPC = Vector2.Distance(n.Center, projectile.Center);
                    if (distanceToNPC > Pulse - effectiveDist && distanceToNPC < Pulse + effectiveDist)
                    {
                        if (n.immune[player.whoAmI] <= 0 && !n.friendly && !n.dontTakeDamage)
                        {
                            player.ApplyDamageToNPC(n, projectile.damage, projectile.knockBack, (projectile.Center.X > n.Center.X).ToDirectionInt(), false);
                            n.immune[player.whoAmI] = 20;
                        }

                        float knockbackReduction = 1f - n.life / (n.life + 15000);
                        if (n.boss && n.type != NPCID.CultistBoss && n.type != NPCID.WallofFlesh && n.type != NPCID.WallofFleshEye)
                            n.velocity += projectile.Center.To(n.Center, 4 * projectile.knockBack * n.knockBackResist + 2f * projectile.knockBack) * knockbackReduction;
                        else
                            n.velocity += projectile.Center.To(n.Center, 4 * projectile.knockBack * n.knockBackResist) * knockbackReduction;
                        n.netUpdate = true;
                    }
                }

                for (byte i = 1; i <= 4; i++)
                {
                    // If it's not active and we don't have a shader, take it
                    if (!Filters.Scene["TyrantEye" + i].IsActive() && Shader == 0)
                    {
                        Shader = i; // mark that it's taken with localAI1
                        Filters.Scene.Activate("TyrantEye" + i, projectile.Center).GetShader().
                            UseColor(1, 5, 20).UseTargetPosition(projectile.Center);
                    }
                }

                // If we have a shader
                if (Shader > 0)
                {
                    // Do the do
                    float progress = MathHelper.Lerp(0, 1, Pulse / 600);
                    Filters.Scene["TyrantEye" + Shader].GetShader().UseProgress(progress).
                        UseOpacity(30 * (1 - progress));
                }
                #endregion
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
