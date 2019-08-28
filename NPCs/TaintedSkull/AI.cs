using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Erilipah.ErilipahBiome;
using Erilipah.Items.Crystalline;
using Erilipah.Items.ErilipahBiome;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.TaintedSkull
{
    public partial class TaintedSkull : ModNPC
    {
        /// <summary>
        /// Don't change. Pos if alive timer, neg for death timer.
        /// </summary>
        private int Timer { get => (int)npc.ai[0]; set => npc.ai[0] = value; }
        private int Phase { get => (int)npc.ai[1]; set => npc.ai[1] = value; }
        private int Attack { get => (int)npc.ai[2]; set => npc.ai[2] = value; }
        private int TempTimer { get => (int)npc.ai[3]; set => npc.ai[3] = value; }
        private Vector2 vector;

        private Player Target => Main.player[npc.target];
        private float SpeedMult => MathHelper.Lerp(Main.expertMode ? 0.35f : 0.50f, 1f, npc.life / (float)npc.lifeMax);

        private static float GetFloorY(Vector2 pos)
        {
            pos /= 16f;
            while (!WorldGen.SolidOrSlopedTile(Main.tile[(int)pos.X, (int)pos.Y]) && !Main.tileSolidTop[Main.tile[(int)pos.X, (int)pos.Y].type])
            {
                // Go downwards, looking for one
                pos.Y++;
                if (pos.Y >= Main.maxTilesY)
                    break;
            }
            return pos.Y * 16;
        }

        private void Roar()
        {
            Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 10, 1, 0.225f);
        }
        private void Hover()
        {
            npc.velocity = npc.GoTo(Target.Center - Vector2.UnitY * 200, new Vector2(0.2f, 0.08f), 5.5f);
            if (npc.Center.Y > Target.Center.Y - 150 && npc.Center.Y < Target.Center.Y)
            {
                npc.velocity.Y *= 0.8f;
            }
            Rotate();
        }
        private void Rotate(bool spin = false)
        {
            if (spin)
                npc.rotation += 0.27f * npc.direction;
            else
                npc.rotation = npc.velocity.X / 15f;
        }
        private void SpawnMinions(int number)
        {
            for (int i = 0; i < number; i++)
            {
                int mother = npc.whoAmI;
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, (float)i / number);
                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType<OrbitMinion>(), 0, mother, rotation, 0, i * -255);
            }
        }
        private void Pulse(float distance, float speed = 5, bool repel = false)
        {
            if (distance == 0 || distance == speed)
                Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0, 1f, -0.2f);

            for (int i = 0; i < distance * 0.2f; i++)
            {
                // Create dusts in an even ring around the NPC
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, i / (distance * 0.2f));
                Vector2 position = npc.Center + Vector2.UnitX.RotatedBy(rotation) * distance;

                Dust dust = Dust.NewDustPerfect(position, mod.DustType<VoidParticle>(), Vector2.Zero);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;

                dust = Dust.NewDustPerfect(position, mod.DustType<CrystallineDust>(), Vector2.Zero);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                float distanceToNPC = Vector2.Distance(proj.Center, npc.Center);

                if (proj.active && distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                {
                    proj.velocity = repel ? npc.Center.To(proj.Center, proj.velocity.Length()) : Vector2.Zero;

                    if (proj.type != mod.ProjectileType<SharpCrystal>())
                        proj.aiStyle = 1;
                    else
                    {
                        proj.ai[1] = 0;
                        proj.ai[0] = -120;
                    }

                    if (Main.expertMode && repel)
                    {
                        if (proj.friendly || proj.hostile)
                        {
                            proj.friendly = false;
                            proj.hostile = true;
                        }
                    }
                }
            }

            if (!repel)
                return;

            for (int i = 0; i < 255; i++)
            {
                float distanceToNPC = Vector2.Distance(Main.player[i].Center, npc.Center);
                if (distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                {
                    Main.player[i].immune = true;
                    Main.player[i].immuneTime = 30;

                    Main.player[i].velocity = npc.Center.To(Main.player[i].Center, 10);
                    Main.player[i].wingTime = 0;
                    Main.player[i].rocketTime = 0;
                }
            }
        }

        public override void AI()
        {
            // Run spawning code on the first tick
            npc.spriteDirection = 0; npc.direction = npc.velocity.X > 0 ? 1 : -1;
            
            void IncrementPhase()
            {
                npc.netUpdate = true;
                TempTimer = 0;
                Phase ++;
            }
            void SwitchAttacks()
            {
                npc.netUpdate = true;
                TempTimer = 0;

                if (npc.life < npc.lifeMax * 0.275)
                {
                    IncrementPhase();
                }
                Attack = Main.rand.Next(5);
            }

            if (Timer == 0)
            {
                npc.Center = Target.Center - new Vector2(0, 200);
                npc.alpha = 255;
                SpawnMinions(Main.expertMode ? 16 : 12);
            }

            if (Timer > 0)
                Timer++;
            if (Timer < 0)
            {
                Timer--;
                npc.velocity = Vector2.Zero;
                return;
            }

            if (Timer < 100)
            {
                Hover();
                npc.velocity *= (Timer / 100f);
                npc.alpha -= 10;
                return;
            }

            if (Timer > 0)
                npc.alpha = (int)MathHelper.SmoothStep(270, 0, (float)npc.life / npc.lifeMax);
            else
                npc.alpha = 0;

            switch (Phase)
            {
                // Phase one: spawn minions. Spit spew if they are
                case 0:
                    Rotate();
                    // If there are no more minions, DIO and switch phases
                    if (!NPC.AnyNPCs(mod.NPCType<OrbitMinion>()))
                    {
                        npc.dontTakeDamage = false;
                        npc.velocity *= 0.8f;
                        TempTimer++;

                        Pulse(TempTimer * 15 % (120 * 15), 15, true);
                        if (TempTimer > 240)
                        {
                            if (Attack != -1)
                            {
                                IncrementPhase(); // If we're coming from the Expert mode minion phase, skip to phase 3
                                SwitchAttacks();
                            }
                            else
                            {
                                npc.netUpdate = true;
                                Phase = 3;
                                TempTimer = 0;
                            }
                        }
                    }
                    // If there ARE minions, hover above the player and fire some spew bolts
                    else
                    {
                        Hover(); // replace all that code & finish the eat attack
                        npc.dontTakeDamage = true;

                        if (Main.expertMode && Main.netMode != 1 && Timer % 90 == 0)
                        {
                            Main.projectile[Projectile.NewProjectile(npc.Center, npc.Center.To(Main.player[npc.target].Center, 7f), mod.ProjectileType<Spew>(), npc.damage/2, 1)]
                                .ai[0] = 1;
                        }
                    }
                    break;

                case 1:
                    // Teleport & dash
                    if (Attack == 0)
                    {
                        TempTimer++;
                        if (TempTimer < 60) // Breathing time
                        {
                            npc.velocity *= 0.94f;
                            Rotate();
                        }
                        else if (TempTimer == 60) // Teleport
                        {
                            npc.netUpdate = true;
                            npc.Teleport(Target.Center + new Vector2(0, 370).RotatedByRandom(MathHelper.TwoPi), 1);
                            npc.velocity = Vector2.Zero;
                            npc.rotation = 0;
                        }
                        else if (TempTimer == 61) npc.velocity = -npc.Center.To(Target.Center, 3); // Wind back...
                        else if (TempTimer < 75) npc.velocity *= 0.95f; // Slow down...
                        else if (TempTimer == 75) // Set velocity and roar
                        {
                            vector = npc.Center.To(Target.Center, dashSpeed);
                            Roar();
                        }
                        else if (TempTimer < 125) // Z O O M
                        {
                            npc.velocity = vector;
                            Rotate(true);
                        }
                        else // Wind down
                        {
                            npc.netUpdate = true;
                            npc.velocity /= 2f;
                            TempTimer = (int)(49 * (1 - SpeedMult)); // Decrease breathing time as HP decreases
                            if (Main.rand.Chance(dashChange))
                                SwitchAttacks();
                        }
                    }
                    // Circle and dash
                    if (Attack == 1)
                    {
                        TempTimer++;
                        if (TempTimer < 60) // Breathing time
                        {
                            npc.velocity *= 0.94f;
                            Rotate();
                        }
                        else if (TempTimer < (int)(160 * SpeedMult))
                        {
                            Vector2 to = Target.Center + new Vector2(325, 0).RotatedBy(Timer / 26f);
                            if (Vector2.Distance(npc.Center, to) > 200)
                                npc.Teleport(to, 1);
                            else
                                npc.Center = to;

                            for (int i = 0; i < 20; i++)
                            {
                                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2CircularEdge(
                                    (320*SpeedMult) - Timer*2, (320*SpeedMult) - Timer*2),
                                    mod.DustType<VoidParticle>(), Vector2.Zero);
                            }

                            Rotate(true);
                        }
                        else if (TempTimer == (int)(160 * SpeedMult))
                        {
                            vector = npc.Center.To(Target.Center, dashSpeed);
                            Roar();
                        }
                        else if (TempTimer < 220 * SpeedMult)
                        {
                            npc.velocity = vector;
                            Rotate(true);
                        }
                        else // Wind down
                        {
                            npc.netUpdate = true;
                            npc.velocity /= 2f;
                            TempTimer = (int)(49 * (1 - SpeedMult)); // Decrease breathing time as HP decreases
                            if (Main.rand.Chance(dashChange))
                                SwitchAttacks();
                        }
                    }
                    // Fly to random spots and fire crystals
                    if (Attack == 2)
                    {
                        TempTimer++;
                        if (TempTimer < 25) // Slow down.
                        {
                            npc.velocity *= 0.8f;
                            Rotate();
                        }
                        else if (TempTimer == 25) // Decide where we're going
                        {
                            npc.netUpdate = true; // Lots of randomization, just sync 'em up.
                            if (Main.netMode != 1) // Don't run any of this on clients, it's not worth it lol.
                            {
                                Rectangle valid = new Rectangle((int)Target.Center.X - 300, (int)Target.Center.Y - 350, 600, 200);
                                Point validPosition = new Point();
                                for (int i = 0; i < 20; i++) // Check 20 times for a good position
                                {
                                    validPosition = new Point(Main.rand.Next(valid.Left, valid.Right), Main.rand.Next(valid.Top, valid.Bottom));

                                    // If there's no collision, i.e. this area is clear, you can move there.
                                    if (!Collision.SolidTiles(
                                        (validPosition.X - (int)npc.Size.X / 2) / 16, (int)npc.Size.X / 16,
                                        (validPosition.Y - (int)npc.Size.Y / 2) / 16, (int)npc.Size.Y / 16))
                                    {
                                        break;
                                    }
                                }

                                vector = validPosition.ToVector2(); // Save that kiddo,
                            }
                        }
                        else if (TempTimer < 30)
                        {
                            TempTimer = 26;
                            npc.alpha = 125;
                            npc.velocity = npc.Center.To(vector, dashSpeed * 1.75f); // and go to that kiddo
                            Rotate();

                            if (Vector2.Distance(npc.Center, vector) < dashSpeed * 2)
                                TempTimer = 30;
                        }
                        else if (TempTimer < 40) { npc.alpha = 255; npc.rotation = 0; npc.velocity = Vector2.Zero; } // 10 tick buffer to slow down
                        else if (TempTimer < 120)
                        {
                            // 7 deadly crystal projectiles; every 10 ticks through delta70 ticks
                            if (Timer % 10 == 0)
                            {
                                npc.netUpdate = true;
                                Vector2 velocity = npc.Center.To(Target.Center, Main.rand.NextFloat(3, 10f))
                                    .RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
                                if (Main.netMode != 1)
                                {
                                    Main.PlaySound(SoundID.Item17, npc.Center);
                                    Main.projectile[Projectile.NewProjectile(npc.Center, velocity, mod.ProjectileType<SharpCrystal>(), npc.damage / 3, 1f)]
                                        .ai[0] = 120 - TempTimer;
                                    // Spawn projs with major spread; the deadly homing crystals
                                }
                            }
                        }
                        else if (TempTimer > 140 * SpeedMult)
                        {
                            npc.netUpdate = true;
                            npc.velocity /= 2f;
                            TempTimer = (int)(25 * (1 - SpeedMult)); // Decrease breathing time as HP decreases
                            if (Main.rand.Chance(dashChange))
                                SwitchAttacks();
                        }
                    }
                    // Pillar telegraph then slam the ground at a location
                    if (Attack == 3)
                    {
                        TempTimer++;
                        if (TempTimer < 60) // Chill pill
                        {
                            npc.velocity = npc.GoTo(Target.Center - Vector2.UnitY * 200, new Vector2(0.2f, 0.08f), 5.5f);
                            if (npc.Center.Y > Target.Center.Y - 150 && npc.Center.Y < Target.Center.Y)
                            {
                                npc.velocity.Y *= 0.8f;
                            }
                            Rotate();
                        }
                        else if (TempTimer == 60 && Main.netMode != 1) // Set our place we wanna slam
                        {
                            npc.netUpdate = true;
                            Rectangle valid = new Rectangle((int)Target.Center.X - 7, (int)Target.Center.Y - 600, 14, 48);
                            Point validPosition = new Point();
                            for (int i = 0; i < 20; i++) // Check 20 times for a good position
                            {
                                validPosition = new Point(Main.rand.Next(valid.Left, valid.Right), Main.rand.Next(valid.Top, valid.Bottom));

                                // Check if a tall area for the boye is clear
                                if (!Collision.SolidTiles(
                                    (validPosition.X - (int)npc.Size.X / 2) / 16, (int)npc.Size.X / 16,
                                    (validPosition.Y - (int)npc.Size.Y / 2) / 16, (int)npc.Size.Y / 16 + 8))
                                {
                                    break;
                                }
                            }

                            vector = validPosition.ToVector2();
                        }
                        else if (TempTimer < 115) // Telegraph...
                        {
                            npc.velocity = npc.GoTo(vector, new Vector2(0.2f, 0.10f), 5.5f);
                            if (npc.Center.Y > Target.Center.Y - 150 && npc.Center.Y < Target.Center.Y)
                            {
                                npc.velocity.Y *= 0.8f;
                            }
                            Rotate();

                            float height = (TempTimer - 60) * 8;
                            float y = Math.Max(GetFloorY(vector), Target.Center.Y);
                            Vector2 dustPos = new Vector2(vector.X, y - height);

                            Dust.NewDustPerfect(dustPos, mod.DustType<CrystallineDust>(), Vector2.Zero).noGravity = true;
                            Dust.NewDustPerfect(dustPos, mod.DustType<VoidParticle>(), Vector2.Zero).customData = 0f;

                            Dust.NewDustPerfect(dustPos + new Vector2(npc.width / 2, 0), mod.DustType<CrystallineDust>(), Vector2.Zero).noGravity = true;
                            Dust.NewDustPerfect(dustPos + new Vector2(npc.width / 2, 0), mod.DustType<VoidParticle>(), Vector2.Zero).customData = 0f;

                            Dust.NewDustPerfect(dustPos + new Vector2(npc.width, 0), mod.DustType<CrystallineDust>(), Vector2.Zero).noGravity = true;
                            Dust.NewDustPerfect(dustPos + new Vector2(npc.width, 0), mod.DustType<VoidParticle>(), Vector2.Zero).customData = 0f;
                        }
                        else if (TempTimer == 115) // Zoop right there to ensure we're not too far away
                        {
                            if (npc.Distance(vector) > 100)
                                npc.Teleport(vector, 1);
                        }
                        else if (TempTimer < 120) // Falling, falling, falling,
                        {
                            TempTimer = 116;
                            npc.noTileCollide = true;
                            int off = 10;
                            bool fallThrough = npc.position.Y + npc.height + 2 < Target.position.Y + Target.height;
                            npc.velocity = Collision.TileCollision(
                                npc.position + new Vector2(off, off), new Vector2(0, dashSpeed),
                                npc.width - off, npc.height - off, fallThrough, fallThrough);
                            Rotate(true);

                            if (npc.velocity.Y < 4) // until we hit the ground.
                            {
                                npc.velocity = Vector2.Zero;
                                npc.noTileCollide = true;
                                TempTimer = 120;
                            }
                        }
                        else if (TempTimer < 180) // Create some dusts & painful sharp crystals that shoot up from the ground
                        {
                            float distance = (TempTimer - 120) * dashSpeed;
                            float speed = 11;

                            for (int i = -1; i < 2; i += 2)
                            {
                                float y = GetFloorY(npc.Center);
                                Vector2 position = new Vector2(npc.Center.X + distance * i, y);
                                Dust.NewDustPerfect(new Vector2(position.X, GetFloorY(position)), mod.DustType<VoidParticle>(), Vector2.Zero, Scale: 1.5f).customData = 0f;

                                if (Main.netMode != 1 && Main.rand.Chance(0.45f))
                                {
                                    Projectile p = Main.projectile[Projectile.NewProjectile(position, Vector2.UnitY * -speed, mod.ProjectileType<SharpCrystal>(), npc.damage / 2, 1)];
                                    p.ai[1] = 1;
                                }
                            }
                        }
                        else if (TempTimer < 240) // Pulse for 0.8s
                        {
                            Pulse((TempTimer - 180) * 20, 20);
                            npc.velocity = npc.GoTo(Target.Center - Vector2.UnitY * 200, new Vector2(0.2f, 0.08f), 5.5f);
                            if (npc.Center.Y > Target.Center.Y - 150 && npc.Center.Y < Target.Center.Y)
                            {
                                npc.velocity.Y *= 0.8f;
                            }
                            Rotate();
                        }
                        else // Wind down
                        {
                            npc.netUpdate = true;
                            TempTimer = (int)(60 * (1 - SpeedMult)); // Decrease breathing time as HP decreases
                            if (Main.rand.Chance(dashChange))
                                SwitchAttacks();
                        }
                    }
                    // Summon some bicthes
                    if (Attack == 4)
                    {
                        if (NPC.CountNPCS(mod.NPCType<Seeker>()) > (Main.expertMode ? 4 : 2))
                            SwitchAttacks();

                        TempTimer++;
                        if (TempTimer < 120)
                        {
                            if (npc.Distance(Target.Center) > 320)
                                npc.velocity = npc.Center.To(Target.Center, 2.5f);
                            if (npc.Distance(Target.Center) < 250)
                                npc.velocity = -npc.Center.To(Target.Center, 2.5f);
                            Rotate();
                            return;
                        }
                        else if (TempTimer % 45 == 0)
                        {
                            npc.netUpdate = true;
                            Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 13, 1f, -0.3f);
                            if (Main.netMode != 1)
                            {
                                NPC h = Main.npc[NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType<Seeker>(), 0, 0, 1)];
                                h.velocity = Main.rand.NextVector2CircularEdge(5, 5);
                            }
                        }
                        Rotate();

                        if (TempTimer > 240)
                        {
                            npc.netUpdate = true;
                            npc.velocity /= 4f;
                            SwitchAttacks();
                        }
                    }
                    break;

                case 2:
                    // If expert mode, skip this phase.
                    if (!Main.expertMode)
                    {
                        IncrementPhase();
                        goto case 3;
                    }
                    // Otherwise, spawn the bois once then run summoning AI as usual.
                    if (Attack != -1)
                    {
                        SpawnMinions(Main.expertMode ? 10 : 7);
                        Attack = -1; // Make sure that we don't loop back into attack phases
                    }
                    goto case 0;

                case 3:
                    TempTimer++;
                    if (TempTimer == 1)
                    {
                        npc.netUpdate = true;
                        Rectangle valid = new Rectangle((int)Target.Center.X - 300, (int)Target.Center.Y - 350, 600, 200);
                        Point validPosition = new Point();
                        for (int i = 0; i < 20; i++) // Check 20 times for a good position
                        {
                            validPosition = new Point(Main.rand.Next(valid.Left, valid.Right), Main.rand.Next(valid.Top, valid.Bottom));

                            // If there's no collision, i.e. this area is clear, you can move there.
                            if (!Collision.SolidTiles(
                                (validPosition.X - (int)npc.Size.X / 2) / 16, (int)npc.Size.X / 16,
                                (validPosition.Y - (int)npc.Size.Y / 2) / 16, (int)npc.Size.Y / 16))
                            {
                                break;
                            }
                        }

                        vector = validPosition.ToVector2();
                    }
                    else if (TempTimer < 120)
                    {
                        npc.velocity = npc.GoTo(vector, 0.3f, 4.5f);
                        if (npc.Center.Y > Target.Center.Y - 150 && npc.Center.Y < Target.Center.Y)
                        {
                            npc.velocity.Y *= 0.8f;
                        }
                        Rotate();

                        Pulse(TempTimer * 12, 12, true);
                    }
                    else if (npc.life > 0.1f)
                    {
                        if (npc.Distance(vector) > 102)
                        {
                            npc.Teleport(vector, 1);
                        }
                        npc.velocity = Vector2.Zero;

                        for (int i = 0; i < 255; i++)
                        {
                            Player p = Main.player[i];
                            p.position += p.Center.To(npc.Center, 2f);
                        }
                    }
                    else
                    {
                        IncrementPhase();
                        goto case 4;
                    }
                    break;

                case 4:
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // One frame size = 96 x 102
            npc.frame.Width = 96;
            npc.frame.Height = 102;

            if (Timer < 0)
            {
                // Go to the first death frame and stick there
                npc.frame.X = 96;

                // Don't move from that frame
                npc.frame.Y = 0;
            }
            if (Timer < 28)
            {
                // Second X frame
                npc.frame.X = 96;
            }
            else if (Timer < 100)
            {
                // Second X frame
                npc.frame.X = 96;

                // Animation of the y-frame
                npc.frame.Y = ((Timer - 28) / 6) * npc.frame.Height;
            }
            else
            {
                // Now stay in the first X frame
                npc.frame.X = 0;

                // Now cycle through frames 0-4 at 10 FPS
                npc.frame.Y = (Timer / 6 % 5) * npc.frame.Height;
            }
        }

        public override bool CheckDead()
        {
            if (Timer > 0)
            {
                Timer = -1;

                npc.dontTakeDamage = true;
                npc.life = 1;
                return false;
            }
            return Timer < -180;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                const string path = "Gores/Taranys/TaranysGore";
                Gore.NewGore(npc.position, new Vector2(-3, -3), mod.GetGoreSlot(path + "0"));
                Gore.NewGore(npc.position, new Vector2(3, -3), mod.GetGoreSlot(path + "1"));
                Gore.NewGore(npc.position, new Vector2(Main.rand.NextFloat(-1, 1), 2), mod.GetGoreSlot(path + "2"));
            }

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType<ErilipahBiome.VoidParticle>(), hitDirection * 4, -3);
                Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType<Items.Crystalline.CrystallineDust>(), hitDirection * 4, -3);
            }
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // If spinning
            if (Attack == 1 && TempTimer >= 60 && TempTimer < 160 * SpeedMult)
                if (projectile.friendly)
                {
                    projectile.Reflect(1f);
                    damage = 0;
                }
        }
    }

    public class OrbitMinion : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(TaintedSkull.Name + "'s Lessers");
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 80;
            npc.defense = 0;
            npc.damage = 20;
            npc.knockBackResist = 0f;
            npc.SetInfecting(1.5f);

            npc.aiStyle = 0;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit19;
            npc.DeathSound = SoundID.NPCDeath22;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 46;
            npc.height = 50;

            npc.value = 0;

            // npc.MakeBuffImmune(BuffID.OnFire);
        }

        private float ExtraRot { get => npc.ai[2]; set => npc.ai[2] = value; }
        private int Timer { get => (int)npc.localAI[0]; set => npc.localAI[0] = value; }
        private int PreTimer { get => (int)npc.ai[3]; set => npc.ai[3] = value; }
        private bool IsActive => PreTimer > 0;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => IsActive;
        public override void AI()
        {
            if (!NPC.AnyNPCs(mod.NPCType<TaintedSkull>()))
            {
                npc.velocity.Y -= 0.2f;
                npc.rotation += npc.rotation += 0.01f;
                return;
            }

            // Spawning fade in animation
            if (!IsActive)
            {
                npc.ai[3] += 7;
                if (npc.ai[3] > -255)
                    npc.alpha = (int)-npc.ai[3];
                else
                    npc.alpha = 255;
                npc.dontTakeDamage = true;
            }
            else
            {
                npc.alpha = 0;
                npc.dontTakeDamage = false;
            }

            Timer++;

            int time = (Main.expertMode ? 130 : 180);
            if (Timer > time)
                Timer = -time;

            NPC mother = Main.npc[(int)npc.ai[0]];
            float rotation = npc.ai[1] + (ExtraRot += 0.01f);
            float distance = MathHelper.SmoothStep(130, 800, Math.Abs(Timer) / (float)time);

            npc.Center = mother.Center + Vector2.UnitX.RotatedBy(rotation) * distance;
            npc.rotation = rotation + (npc.spriteDirection == 1 ? MathHelper.Pi : 0);
            npc.velocity = Vector2.Zero;

            Dust.NewDustPerfect(Vector2.Lerp(npc.Center, mother.Center, Math.Abs(Timer) / (float)time),
                mod.DustType<VoidParticle>(), Vector2.Zero).customData = 0;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<PureFlower>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.35f);
            return false;
        }
    }

    public class Spew : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spit");
        }
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.SetInfecting(2.2f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }
        public override void AI()
        {
            for (int i = 0; i < projectile.ai[0] + 1; i++)
            {
                if (projectile.ai[0] != 0)
                    Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, mod.DustType<VoidParticle>(), 0, 0).customData = (int)0;

                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, mod.DustType<CrystallineDust>(), 0, 0);
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
            }
        }
    }

    public class SharpCrystal : ModProjectile
    {
        public override string Texture => "Erilipah/Items/Crystalline/CrystallineDust";
        public override string GlowTexture => Texture;

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 3;
            DisplayName.SetDefault("Crystalline Shards");
        }
        public override void SetDefaults()
        {
            projectile.scale = 2f;
            projectile.width = 8;
            projectile.height = 8;
            projectile.SetInfecting(1.2f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.frame = Main.rand.Next(3);
            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }

        public override void AI()
        {
            projectile.ai[0]--;

            if (projectile.ai[1] != 0)
            {
                projectile.velocity.Y *= 0.985f;
                return;
            }

            if (projectile.ai[0] > 0)
            {
                projectile.tileCollide = true;
                projectile.velocity *= 0.95f;
            }
            else if (projectile.ai[0] > -120)
            {
                int player = projectile.FindClosestPlayer(500f);
                if (player == -1)
                    return;
                Player target = Main.player[player];
                projectile.velocity = projectile.GoTo(target.Center, 0.22f);
            }
            else
            {
                projectile.velocity.Y += 0.175f;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.PurpleCrystalShard);
            }
        }
    }
}
