using Erilipah.Items.Crystalline;
using Erilipah.Items.Taranys;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class AbProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 32;
            projectile.tileCollide = false;
            projectile.netImportant = true;

            Follow = 255;
            projectile.frame = 0;

            Timer = 0;
            ScaleTimer = 0;
            FloatTimer = 0;
            Follow = 255;
        }

        private static Vector2 AboveAltar => ErilipahWorld.AltarPosition - Vector2.UnitY * 100;
        private bool Taken => Follow != 255;
        private bool SummonComplete => Timer > 1110;

        private float Timer { get => projectile.ai[0]; set => projectile.ai[0] = value; }
        private float ShockTimer { get => projectile.ai[1]; set => projectile.ai[1] = value; }

        private float ScaleTimer { get => projectile.localAI[0]; set => projectile.localAI[0] = value; }
        private float FloatTimer { get => projectile.localAI[1]; set => projectile.localAI[1] = value; }

        private int Follow = 255;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Follow);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            reader.ReadInt32();
        }

        public override void AI()
        {
            // Never die!
            projectile.timeLeft = 180;
            projectile.aiStyle = 0;

            if (projectile.frame > 0)
            {
                Lighting.AddLight(projectile.Center, new Vector3(1.2f, 1.0f, 1.2f * projectile.scale) * projectile.scale * Math.Abs(pulse * 1.2f));
            }

            // Run shockwave. To start a shockwave set ShockTimer = 1
            if (ShockTimer > 0)
            {
                ShockTimer++;
                if (!Filters.Scene["AborycTake"].IsActive())
                {
                    Filters.Scene.Activate("AborycTake", projectile.Center).GetShader().UseColor(2, 3, 20).UseTargetPosition(projectile.Center);
                }

                float progress = MathHelper.Lerp(0, 1, ShockTimer / 60f);
                Filters.Scene["AborycTake"].GetShader().UseProgress(progress).UseOpacity(125 * (1 - progress));

                // Reset, eventually
                if (ShockTimer > 1500)
                {
                    Filters.Scene["AborycTake"].Deactivate();
                    ShockTimer = 0;
                }
            }

            if (Taken)
            {
                Player player = Main.player[Follow];

                int taranys = NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>());
                bool taranysIsDying = taranys != -1 && Main.npc[taranys].ai[0] < 0;
                bool taranysDespawn = taranys != -1 && Main.npc[taranys].ai[0] < -2000;

                if (SummonComplete && taranysDespawn || Timer >= 1e9f)
                {
                    Timer = 1e9f;
                    if (projectile.scale > 0.02f)
                        projectile.scale -= 0.01f;
                    else
                        projectile.Kill();
                }
                else if (SummonComplete && taranys == -1 || Timer < 0)
                {
                    // Shrink back down then go to the altar
                    Effects(Vector2.Zero, true);
                    AlAltar();
                }
                else if (SummonComplete && taranys > -1 && taranysIsDying)
                {
                    projectile.velocity = Vector2.Zero;
                    projectile.Center = Vector2.Lerp(projectile.Center, Main.npc[taranys].Center + new Vector2(0, 15), 0.08f);
                    projectile.scale += 0.005f;
                    // Grow overtop Taranys as if to kill him
                }
                else if (SummonComplete && taranys > -1)
                {
                    // Follow player during fight
                    projectile.velocity = Vector2.Zero;
                    projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X + player.direction * 17.5f, player.Center.Y), 0.1f);
                }
                else if (Timer >= 0)
                {
                    Timer++;
                    Effects(Vector2.Zero, true);
                    projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X, player.Center.Y - 100), 0.165f);

                    if (Timer > 600)
                        Ritual(player, (int)Timer - 600);
                }
            }
            else
            {
                Effects(AboveAltar);
                Follow = CheckForDash();
            }
        }

        private int CheckForDash()
        {
            if (ShockTimer > 0)
                return 255;
            for (int i = 0; i < 255; i++)
            {
                Player player = Main.player[i];
                bool playerCollision = Collision.CheckAABBvAABBCollision(projectile.position, projectile.Size, player.position, player.Size);
                if (playerCollision)
                {
                    player.AddBuff(BuffID.Featherfall, 600);
                    player.AddBuff(BuffID.Shine, 600);
                    player.velocity.Y -= 3;
                    return player.whoAmI;
                }
            }
            return 255;
        }

        private void Effects(Vector2 pos, bool scaleOnly = false)
        {
            const float hoverDist = 20;
            const float scaleTime = 107;
            const float hoverTime = 180;

            if (ScaleTimer < scaleTime)
                ScaleTimer++;
            else
                ScaleTimer = -scaleTime;

            projectile.scale = MathHelper.SmoothStep(0.92f, 1.08f, Math.Abs(ScaleTimer) / scaleTime);

            if (scaleOnly) return;

            projectile.velocity = Vector2.Zero;

            if (FloatTimer < hoverTime)
                FloatTimer++;
            else
                FloatTimer = -hoverTime;

            if (Timer < 1100)
            {
                Vector2 hoverPos = new Vector2(pos.X, MathHelper.SmoothStep(pos.Y - hoverDist, pos.Y + hoverDist, Math.Abs(FloatTimer) / hoverTime));
                float distance = Vector2.Distance(projectile.Center, hoverPos);

                if (distance > 8)
                    projectile.Center = Vector2.Lerp(projectile.Center, hoverPos, 0.1f);
                else
                    projectile.Center = hoverPos;
            }

            if (Timer > 780 && Vector2.Distance(projectile.Center, pos) < 6)
            {
                projectile.Center = pos;
            }
        }

        private void Ritual(Player player, int time)
        {
            player.GetModPlayer<InfectionPlr>().darknessCounter--;
            if (player.dead && NPC.AnyNPCs(mod.NPCType<NPCs.Taranys.Taranys>()))
            {
                Player nextPlayer = Main.player.FirstOrDefault(p => p.active && !p.dead && p.Distance(projectile.Center) < 3000);
                if (nextPlayer == null)
                {
                    // Go back to the altar and reset.
                    SetDefaults();
                    return;
                }
            }

            if (time == 1)
            {
                Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 29, 1, 0.2f);
            }
            else if (time < 80)
            {
                Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 103, 0.2f, 0.3f);
                Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                    .customData = 0f;
            }
            else if (time == 80)
            {
                ShockTimer = 3;
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                        .customData = 0f;
                }
                projectile.frame = 1;
                Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, -0.4f);
            }

            if (time < 430)
            {
                // Lock the player in
                player.GetModPlayer<ErilipahPlayer>().canMove = false;
                player.velocity = Vector2.Zero;
                player.maxFallSpeed = 0;

                player.immune = true;
                player.immuneTime = 30;
            }

            if (time > 430)
            {
                projectile.velocity *= 0.92f;
            }
            else if (time == 430)
            {
                if (Main.netMode != 1)
                    NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType<NPCs.Taranys.Taranys>());

                NPC t = Main.npc[NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>())];
                t.frame = new Rectangle(96, 0, 96, 102);
                t.Center = projectile.Center;

                projectile.velocity = new Vector2(0, -6);
            }
            else if (time >= 230 && time < 430)
            {
                Vector2 newPos = new Vector2(player.Center.X, player.Center.Y - 200);
                Vector2 distance = new Vector2(0, 200 - (time - 230)) / 2f; // 200 = the time of this phase, hence its existing
                float rotation = (float)Math.Pow(time - 230, 1.815f);
                rotation /= 100f;

                projectile.Center = newPos + distance.RotatedBy(rotation);
            }
        }

        private float inc = 0;
        private void AlAltar()
        {
            float distanceToAltar = Vector2.Distance(projectile.Center, AboveAltar);
            Vector2 dirToAltar = projectile.Center.To(AboveAltar);

            if (Timer < 0)
            {
                projectile.Center = AboveAltar;
                Timer--;

                // If there's a person dashing thru us, drop everything and complete the cycle.
                if (CheckForDash() < 255)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        const float speed = 8;
                        Vector2 dir = (i / 50f * MathHelper.TwoPi).ToRotationVector2();
                        Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<VoidParticle>(), dir * speed)
                            .noGravity = true;
                    }

                    Rectangle dropArea = new Rectangle((int)AboveAltar.X - 5, (int)AboveAltar.Y - 5, 10, 10);
                    DropItemInstanced(dropArea, mod.ItemType<LostKey>());

                    ShockTimer = 1;
                    Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 29, 1, -0.35f);
                    SetDefaults();

                    Follow = 255;
                    return;
                }

                if (Timer < -60)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 16 + Main.rand.NextVector2CircularEdge(55, 55), mod.DustType<CrystallineDust>(), Vector2.Zero);
                    dust.customData = 100f; // Make it start funneling inward automatically
                }

                float dist = Vector2.Distance(projectile.Center, Main.LocalPlayer.Center);
                // TODO: Add "if player can dash" check here
                if (!ErilipahWorld.downedTaintedSkull && dist > 300 && Main.LocalPlayer.InErilipah())
                {
                    float numDust = dist / 50f;
                    Vector2 pos = Vector2.Lerp(Main.LocalPlayer.Center, projectile.Center - Vector2.UnitY * 16, (inc += 0.25f / numDust) % 1);
                    Dust.NewDustPerfect(pos, mod.DustType<CrystallineDust>(), Vector2.Zero, Scale: 1.35f)
                        .noGravity = true;
                }
            }
            else if (distanceToAltar < 4)
            {
                projectile.velocity = Vector2.Zero;
                projectile.Center = AboveAltar;
                Timer = -1; // We're in the endgame now
            }
            else if (distanceToAltar < 200)
            {
                projectile.velocity = dirToAltar * 10 * (distanceToAltar / 200f);
            }
            else if (projectile.velocity.Length() < 10)
            {
                projectile.velocity += dirToAltar * 0.0225f;
            }
        }

        private void DropItemInstanced(Rectangle r, int itemType, int itemStack = 1)
        {
            if (Main.netMode == 2)
            {
                int itemIndex = Item.NewItem(r, itemType, itemStack, true, 0, false, false);
                Main.itemLockoutTime[itemIndex] = 54000;
                for (int remoteClient = 0; remoteClient < 255; remoteClient++)
                {
                    if (Main.player[remoteClient].active)
                        NetMessage.SendData(MessageID.InstancedItem, remoteClient, -1, null, itemIndex);
                }
                Main.item[itemIndex].active = false;
            }
            else if (Main.netMode == 0)
                Item.NewItem(r, itemType, itemStack, false, 0, false, false);
        }

        private float pulse = 1f;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float pulseSpeed;
            if (Timer > 600)
                pulseSpeed = 0f;
            else
                pulseSpeed = MathHelper.SmoothStep(0.005f, 0.20f, Timer / 600f);
            pulseSpeed = MathHelper.Clamp(pulseSpeed, 0.01f, 0.25f);

            pulse += pulseSpeed;
            if (pulse >= +1)
                pulse = -1 + (pulse - 1);
            pulse = MathHelper.Clamp(pulse, -1f, 1f);

            float inputPulse;
            if (Timer <= 0 || Timer > 600)
                inputPulse = 1;
            else
                inputPulse = Math.Abs(pulse);

            Texture2D texture = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
                texture.Frame(1, 2, 0, projectile.frame), Color.White * ((255 - projectile.alpha) / 255f) * inputPulse, 0f,
                Main.projectileTexture[projectile.type].Size() / 2, projectile.scale, 0, 1);
            return false;
        }
    }
}
