using Erilipah.Items.Crystalline;
using Erilipah.Items.Taranys;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
            projectile.width = 24;
            projectile.height = 20;
            projectile.tileCollide = false;
            projectile.netImportant = true;

            projectile.owner = 255;
            projectile.frame = 0;

            Timer = 0;
            ScaleTimer = 0;
            FloatTimer = 0;
        }

        private static Vector2 AboveAltar => ErilipahWorld.AltarPosition - Vector2.UnitY * 100;
        private bool Taken => projectile.owner != 255;
        private bool SummonComplete => Timer > 1330 + 600;

        private float Timer { get => projectile.ai[0]; set => projectile.ai[0] = value; }
        private float ShockTimer { get => projectile.ai[1]; set => projectile.ai[1] = value; }

        private float ScaleTimer { get => projectile.localAI[0]; set => projectile.localAI[0] = value; }
        private float FloatTimer { get => projectile.localAI[1]; set => projectile.localAI[1] = value; }

        public override void AI()
        {
            // Never die!
            projectile.timeLeft = 60;

            if (projectile.frame > 0)
            {
                Lighting.AddLight(projectile.Center, new Vector3(1.2f, 1.0f, 1.2f * projectile.scale) * projectile.scale);
            }

            // Run shockwave. To start a shockwave set ShockTimer = 1
            if (ShockTimer > 0)
            {
                ShockTimer++;
                if (!Filters.Scene["AborycTake"].IsActive())
                {
                    Filters.Scene.Activate("AborycTake", projectile.Center).GetShader().UseColor(1, 5, 10).UseTargetPosition(projectile.Center);
                }

                float progress = MathHelper.Lerp(0, 1, ShockTimer / 1500f);
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
                Player player = Main.player[projectile.owner];

                int taranys = NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>());
                bool taranysIsDying = taranys != -1 && Main.npc[taranys].ai[0] < 0;

                if (SummonComplete && taranys == -1 || Timer < 0)
                {
                    // Shrink back down then go to the altar
                    if (projectile.scale > 1.2f)
                        projectile.scale -= 0.035f;
                    else
                    {
                        AlAltar();
                    }
                }
                else if (SummonComplete && taranys > -1 && taranysIsDying)
                {
                    projectile.velocity = Vector2.Zero;
                    projectile.Center = Vector2.Lerp(projectile.Center, Main.npc[taranys].Center, 0.1f);
                    projectile.scale += 0.015f;
                    // Grow overtop Taranys as if to kill him
                }
                else 
                {
                    Timer++;
                    Effects(new Vector2(player.Center.X, player.Center.Y - 100));

                    if (Timer > 600)
                        Ritual(player, (int)Timer - 600);
                }
            }
            else
            {
                Effects(AboveAltar);
                projectile.owner = CheckForDash();
            }
        }

        private int CheckForDash()
        {
            for (int i = 0; i < 255; i++)
            {
                Player player = Main.player[i];
                bool playerCollision = Collision.CheckAABBvAABBCollision(projectile.position, projectile.Size, player.position, player.Size);
                if (playerCollision)
                {
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

            projectile.velocity = Vector2.Zero;

            if (ScaleTimer < scaleTime)
                ScaleTimer++;
            else
                ScaleTimer = -scaleTime;

            if (scaleOnly) return;

            if (FloatTimer < hoverTime)
                FloatTimer++;
            else
                FloatTimer = -hoverTime;

            if (Timer < 500 && (Timer < 300 || Vector2.Distance(projectile.Center, pos) > 3))
            {
                Vector2 hoverPos = new Vector2(pos.X, MathHelper.SmoothStep(pos.Y - hoverDist, pos.Y + hoverDist, Math.Abs(FloatTimer) / hoverTime));
                float distance = Vector2.Distance(projectile.Center, hoverPos);

                if (distance > 10)
                    projectile.Center = Vector2.Lerp(projectile.Center, hoverPos, 0.1f);
                else
                    projectile.Center = hoverPos;
            }
            else if (Timer < 500)
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

            if (time < 220) { }
            else if (time == 220)
            {
                Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 29, 1, 0.2f);
            }
            else if (time < 300)
            {
                Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 103, 0.2f, 0.3f);
                Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                    .customData = 0f;
            }
            else if (time == 300)
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10, mod.DustType<CrystallineDust>(), Main.rand.NextVector2CircularEdge(5, 5))
                        .customData = 0f;
                }
                projectile.frame = 1;
                Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, -0.4f);
                ShockTimer = 1;
            }

            if (time >= 220)
            {
                if (time < 650)
                {
                    // Lock the player in
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    player.heldProj = projectile.whoAmI;
                    player.GetModPlayer<ErilipahPlayer>().canMove = false;

                    player.immune = true;
                    player.immuneTime = 30;
                }
            }

            if (time > 730)
            {
                projectile.velocity = Vector2.Zero;
                projectile.Center = Vector2.Lerp(projectile.Center, new Vector2(player.Center.X + player.direction * 17.5f, player.Center.Y), 0.1f);
            }
            else if (time > 650)
            {
                projectile.velocity *= 0.92f;
            }
            else if (time == 650)
            {
                if (Main.netMode != 1)
                    NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType<NPCs.Taranys.Taranys>());

                NPC t = Main.npc[NPC.FindFirstNPC(mod.NPCType<NPCs.Taranys.Taranys>())];
                t.frame = new Rectangle(96, 0, 96, 102);
                t.Center = projectile.Center;

                projectile.velocity = new Vector2(0, -6);
            }
            else if (time >= 450 && time < 650)
            {
                Vector2 newPos = new Vector2(player.Center.X, player.Center.Y - 200);
                Vector2 distance = new Vector2(0, 200 - (time - 450)) / 2f; // 200 = the time of this phase, hence its existing
                float rotation = (float)Math.Pow(time - 450, 1.815f);
                rotation /= 100f;

                projectile.Center = newPos + distance.RotatedBy(rotation);
            }
        }

        private void AlAltar()
        {
            float distanceToAltar = Vector2.Distance(projectile.Center, AboveAltar);
            Vector2 dirToAltar = projectile.Center.To(AboveAltar);

            if (Timer < 0)
            {
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

                    // TODO: Finalize all the Taranys drops when they're all done
                    // TODO: Add the Lost Key, which teleports you down to the Lost City and can unlock the Throne Room to the huge abomination
                    Rectangle dropArea = new Rectangle((int)AboveAltar.X - 5, (int)AboveAltar.Y - 5, 10, 10);
                    DropItemInstanced(dropArea, mod.ItemType<TyrantEye>());
                    DropItemInstanced(dropArea, mod.ItemType<VoidSpike>());
                    DropItemInstanced(dropArea, mod.ItemType<TorchOfSoul>());
                    DropItemInstanced(dropArea, mod.ItemType<MadnessFocus>(), 20);

                    ShockTimer = 1;
                    Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 29, 1, -0.35f);
                    SetDefaults();
                    return;
                }

                if (Timer < -90)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center - Vector2.UnitY * 10 + Main.rand.NextVector2CircularEdge(50, 50), mod.DustType<CrystallineDust>(), Vector2.Zero);
                    dust.customData = 100; // Make it start funneling inward automatically
                }
            }
            else if (distanceToAltar < 10)
            {
                projectile.Center = AboveAltar;
                Timer = -1; // We're in the endgame now

                if (!ErilipahWorld.downedTaintedSkull)
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDustPerfect(Vector2.Zero, mod.DustType<CrystallineDust>(), Vector2.Zero, Scale: 1.35f)
                        .customData = i / 30d;
                }
            }
            else if (distanceToAltar < 200)
            {
                projectile.velocity = dirToAltar * 10 * (distanceToAltar / 200f);
            }
            else if (projectile.velocity.Length() < 10)
            {
                projectile.velocity += dirToAltar * 0.1f;
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

        float pulse = 1f;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float pulseIntensity = MathHelper.Lerp(0f, 1f, Timer / 600f);
            float pulseSpeed = (int)MathHelper.Lerp(0.01f, 0.15f, Timer / 600f);
            pulse += pulseSpeed;
            if (pulse > +1)
                pulse = -1 + (pulse - 1);

            float inputPulse;
            if (Timer > 600 || Timer < 0)
                inputPulse = 1;
            else
                inputPulse = Math.Max(pulseIntensity, Math.Abs(pulse));

            Texture2D texture = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
                texture.Frame(1, 2, 0, projectile.frame), Color.White * ((255 - projectile.alpha) / 255f) * inputPulse, 0f,
                Main.projectileTexture[projectile.type].Size() / 2, projectile.scale, 0, 1);
            return false;
        }
    }
}
