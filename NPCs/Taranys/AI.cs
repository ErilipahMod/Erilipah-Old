using Erilipah.Items.Crystalline;
using Erilipah.Items.ErilipahBiome;
using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Taranys
{
    public partial class Taranys : ModNPC
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
        private int AlphaOT => (int)MathHelper.SmoothStep(200, 0, npc.life / (float)npc.lifeMax);

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
            Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 10, 0.825f, Main.rand.NextFloat(0.1f, 0.225f));
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
            if (Main.netMode != 1)
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
                if (npc.life > npc.lifeMax * 0.1f)
                    Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 10, 1f, -0.35f);

            npc.alpha = (int)MathHelper.SmoothStep(0, AlphaOT, distance / (60 * speed));

            for (int i = 0; i < distance * 0.2f; i++)
            {
                // Create dusts in an even ring around the NPC
                float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, i / (distance * 0.2f));
                Vector2 position = npc.Center + Vector2.UnitX.RotatedBy(rotation) * distance;

                Dust dust = Dust.NewDustPerfect(position, mod.DustType<CrystallineDust>(), Vector2.Zero);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
                dust.scale = 1.3f;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                float distanceToNPC = Vector2.Distance(proj.Center, npc.Center);

                if (!Main.projHook[proj.type] && proj.active && distanceToNPC > distance - speed && distanceToNPC < distance + speed)
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
            {
                for (int i = 0; i < 255; i++)
                {
                    float distanceToNPC = Vector2.Distance(Main.player[i].Center, npc.Center);
                    if (distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                    {
                        Main.player[i].immune = true;
                        Main.player[i].immuneTime = 30;

                        Main.player[i].velocity *= -0.2f;
                        Main.player[i].wingTime = 0;
                        Main.player[i].rocketTime = 0;
                    }
                }
                return;
            }

            for (int i = 0; i < 255; i++)
            {
                float distanceToNPC = Vector2.Distance(Main.player[i].Center, npc.Center);
                if (distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                {
                    Main.player[i].immune = true;
                    Main.player[i].immuneTime = 30;

                    Main.player[i].velocity = npc.Center.To(Main.player[i].Center, 8.5f);
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
                Phase++;
            }
            void SwitchAttacks()
            {
                npc.netUpdate = true;
                TempTimer = 0;
                Attack = Main.rand.Next(5);

                if (npc.life < npc.lifeMax * 0.365)
                {
                    IncrementPhase();
                }
            }

            if (Timer <= -2000 || Target.dead && CheckActive())
            {
                Timer--;
                if (Timer > 0)
                {
                    // We're leaving. Don't do anything else. Bye bye.
                    Timer = -2000;
                    vector = npc.Center;
                }

                if (Phase == 0)
                {
                    npc.velocity.Y -= 0.1f;
                    return;
                }

                npc.rotation *= 0.9f;
                npc.velocity *= 0.5f;
                npc.dontTakeDamage = true;

                float dist = (float)Math.Pow(Math.Abs(Timer) - 2000, 1.2);
                npc.Center = vector + Main.rand.NextVector2Circular(dist, dist);
                if (dist > 2000)
                    npc.active = false;
                return;
            }

            // Force a switch if we're really low and aren't switching already
            if (npc.life < npc.lifeMax * 0.3 && Phase == 1)
                SwitchAttacks();

            if (!Target.InErilipah())
                npc.dontTakeDamage = true;
            else
                npc.dontTakeDamage = false;

            if (Timer == 0)
            {
                npc.Center = Target.Center - new Vector2(0, 200);
                npc.alpha = 255;
                SpawnMinions(Main.expertMode ? 16 : 12);
            }

            if (Timer >= 0)
            {
                Timer++;
                if (Timer < 100)
                {
                    Hover();
                    npc.velocity *= (Timer / 100f);
                    npc.alpha -= 7;
                    Timer++;
                    return;
                }
                npc.alpha = AlphaOT;
            }
            else
            {
                Timer--;
                npc.alpha--;
                npc.velocity = Vector2.Zero;
                npc.Center = vector + Main.rand.NextVector2Circular(2, 2) * ((-(200 + Timer) + 200) / 50f);
                npc.dontTakeDamage = true;

                if (Timer == -30)
                {
                    Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0, 1f, -0.5f);
                }

                if (Timer > -30)
                    return;

                const float speed = 20;
                float distance = (Math.Abs(Timer) - 29) * speed;

                for (int i = 0; i < distance * 0.25f; i++)
                {
                    // Create dusts in an even ring around the NPC
                    float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, i / (distance * 0.25f));
                    Vector2 position = npc.Center + Vector2.UnitX.RotatedBy(rotation) * distance;

                    Dust dust = Dust.NewDustPerfect(position, mod.DustType<CrystallineDust>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                    dust.scale = 1.6f;
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    float distanceToNPC = Vector2.Distance(proj.Center, npc.Center);

                    if (!Main.projHook[proj.type] && proj.active && distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                    {
                        proj.velocity = npc.Center.To(proj.Center, proj.velocity.Length());
                    }
                }

                for (int i = 0; i < 200; i++)
                {
                    if (Main.npc[i].active && i != npc.whoAmI)
                    {
                        Main.npc[i].life = 0;
                        Main.npc[i].HitEffect(0, Main.npc[i].life);
                    }
                }

                for (int i = 0; i < 255; i++)
                {
                    float distanceToNPC = Vector2.Distance(Main.player[i].Center, npc.Center);
                    if (distanceToNPC > distance - speed && distanceToNPC < distance + speed)
                    {
                        Main.player[i].immune = true;
                        Main.player[i].immuneTime = 30;

                        Main.player[i].velocity = npc.Center.To(Main.player[i].Center, 12.5f);
                        Main.player[i].wingTime = 0;
                        Main.player[i].rocketTime = 0;
                    }
                }
                return;
            }

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
                            Main.projectile[Projectile.NewProjectile(npc.Center, npc.Center.To(Main.player[npc.target].Center, 7f), mod.ProjectileType<Spew>(), npc.damage / 2, 1)]
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
                            if (Main.netMode != 1 && Main.rand.Chance(dashChange))
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
                                    (320 * SpeedMult) - Timer * 2, (320 * SpeedMult) - Timer * 2),
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
                            if (Main.netMode != 1 && Main.rand.Chance(dashChange))
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
                        else if (TempTimer < 40) { npc.rotation = 0; npc.velocity = Vector2.Zero; } // 10 tick buffer to slow down
                        else if (TempTimer < 120)
                        {
                            // 7 deadly crystal projectiles; every 10 ticks through delta70 ticks
                            if (Timer % 10 == 0)
                            {
                                npc.netUpdate = true;
                                Main.PlaySound(SoundID.Item17, npc.Center);
                                if (Main.netMode != 1)
                                {
                                    Vector2 velocity = npc.Center.To(Target.Center, Main.rand.NextFloat(3, 10f))
                                        .RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
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
                            if (Main.netMode != 1 && Main.rand.Chance(dashChange))
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
                        else if (TempTimer == 60) // Set our place we wanna slam
                        {
                            npc.netUpdate = true;

                            if (Main.netMode != 1)
                            {
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
                            Roar();
                        }
                        else if (TempTimer < 120) // Falling, falling, falling,
                        {
                            TempTimer = 116;
                            bool fallThrough = npc.position.Y + npc.height + 2 < Target.position.Y + Target.height;
                            npc.velocity = Collision.TileCollision(
                                npc.position + new Vector2(0, -10), new Vector2(0, dashSpeed),
                                npc.width, npc.height, fallThrough, fallThrough);
                            Rotate(true);

                            if (npc.velocity.Y < 4) // until we hit the ground.
                            {
                                Main.PlaySound(SoundID.NPCDeath14, npc.Center);
                                npc.velocity = Vector2.Zero;
                                TempTimer = 120;
                            }
                        }
                        else if (TempTimer < 180) // Create some dusts & painful sharp crystals that shoot up from the ground
                        {
                            float distance = (TempTimer - 120) * dashSpeed;

                            for (int i = -1; i < 2; i += 2)
                            {
                                float y = GetFloorY(npc.Center);

                                Vector2 position = new Vector2(npc.Center.X + distance * i, y);
                                Dust.NewDustPerfect(new Vector2(position.X, GetFloorY(position)), mod.DustType<VoidParticle>(), Vector2.Zero, Scale: 1.5f).customData = 0f;
                                Dust.NewDustPerfect(new Vector2(position.X, GetFloorY(position)), mod.DustType<CrystallineDust>(), Vector2.Zero, Scale: 1.2f).noGravity = true;

                                if (Main.netMode != 1 && Main.rand.Chance(0.45f))
                                {
                                    float speed = Main.rand.NextFloat(10, 13);
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
                            if (Main.netMode != 1 && Main.rand.Chance(dashChange))
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
                            Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 13, 1f, -0.5f);
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

                        if (Main.netMode != 1)
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

                            vector = validPosition.ToVector2();
                        }
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
                    else if (npc.life > 0.175f * npc.lifeMax)
                    {
                        npc.velocity = npc.GoTo(new Vector2(npc.Center.X, Target.Center.Y - 30), 0.1f, 3f);
                        npc.velocity *= 0.9f;

                        for (int i = 0; i < 255; i++)
                        {
                            Player p = Main.player[i];
                            float distance = p.Distance(npc.position + new Vector2(50, 78));
                            float speed = MathHelper.SmoothStep(8f, 0, distance / 1000f);

                            p.position.X += (p.Center.X < npc.Center.X ? speed : -speed);
                            if (MathHelper.Distance(p.position.X, npc.Center.X) < 10)
                                p.position.Y += (p.Center.Y < npc.Center.Y ? speed : -speed) / 2;
                        }

                        for (int i = 0; i < 30; i++)
                        {
                            float radius = MathHelper.SmoothStep(800f, 0f, TempTimer % 100f / 100f);
                            Dust.NewDustPerfect(npc.position + new Vector2(50, 78)
                                + new Vector2(0, 1).RotatedBy(i / 30f * TempTimer / 40f) * radius,
                                mod.DustType<CrystallineDust>(), Vector2.Zero)
                                .noGravity = true;
                        }

                        if (TempTimer % 150 == 0 && Main.netMode != 1)
                            Main.projectile[Projectile.NewProjectile(npc.Center, npc.Center.To(Main.player[npc.target].Center, 7f), mod.ProjectileType<Spew>(), npc.damage / 2, 1)]
                                .ai[0] = 1;

                    }
                    else
                    {
                        Roar();
                        IncrementPhase();
                        goto case 4;
                    }
                    break;

                case 4:
                    Hover();
                    npc.velocity /= 3f;

                    if (NPC.CountNPCS(mod.NPCType<OrbitMinion>()) < 13 && Timer % 600 * SpeedMult == 0)
                    {
                        SpawnMinions(Main.expertMode ? 4 : 2);
                        Roar();
                    }

                    Pulse(Timer % 100 * 15, 15, true);
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // One frame size = 96 x 102
            npc.frame.Width = 96;
            npc.frame.Height = 102;

            if (Timer > -2000 && Timer < 0)
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

                int Type(int t) => mod.GetGoreSlot("Gores/Taranys/Taranys" + t);
                Gore.NewGore(npc.position, new Vector2(-2, -3), Type(0));
                Gore.NewGore(npc.position + new Vector2(50, 0), new Vector2(2, -3), Type(1));
                Gore.NewGore(npc.position + new Vector2(32, 74), new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), 1), Type(2));

                vector = npc.Center;
                npc.dontTakeDamage = true;
                npc.life = 1;
                return false;
            }
            if (Timer < -300)
            {
                npc.life = 0;
                Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 13, 1f, -0.7f);
                Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, 13, 1f, 0.6f);
                return true;
            }
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
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
            DisplayName.SetDefault("Shattered Soul");
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 80;
            npc.defense = 0;
            npc.damage = 25;
            npc.knockBackResist = 0f;
            npc.SetInfecting(0.8f);

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
            if (!NPC.AnyNPCs(mod.NPCType<Taranys>()))
            {
                npc.life = 0;
                npc.HitEffect(0, 10);
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

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                int Type(int t) => mod.GetGoreSlot("Gores/Taranys/OrbitMinion" + t);

                for (int i = 0; i < 3; i++)
                    Gore.NewGore(npc.position, (npc.position - npc.oldPosition).SafeNormalize(Vector2.Zero) * 3, Type(0));
                for (int i = 1; i <= 3; i++)
                    Gore.NewGore(npc.position, (npc.position - npc.oldPosition).SafeNormalize(Vector2.Zero) * 3, Type(i));
            }
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
            projectile.SetInfecting(3.5f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }
        public override void AI()
        {
            projectile.ai[1]++;
            for (int i = 0; i < projectile.ai[0] + 1; i++)
            {
                Vector2 offset = DustPos(projectile.velocity.SafeNormalize(Vector2.Zero), projectile.velocity.Length(), projectile.ai[1], 3, 5);

                Dust dust = Dust.NewDustPerfect(projectile.Center + offset, mod.DustType<CrystallineDust>());
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
                dust.scale = 1.25f;
            }
        }
        private Vector2 DustPos(Vector2 forward, float speed, float time, float frequency, float amplitude)
        {
            Vector2 up = new Vector2(-forward.Y, forward.X);
            float upspeed = (float)Math.Cos(time * frequency) * amplitude * frequency;
            return up * upspeed + forward * speed;
        }
    }

    public class SharpCrystal : ModProjectile
    {
        public override string GlowTexture => "Erilipah/NPCs/Taranys/SharpCrystal";

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 2;
            DisplayName.SetDefault("Crystalline Shards");
        }
        public override void SetDefaults()
        {
            projectile.scale = 2f;
            projectile.width = 8;
            projectile.height = 8;
            projectile.SetInfecting(2.8f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }

        public override void AI()
        {
            projectile.ai[0]--;
            projectile.netUpdate = true;

            if (projectile.ai[1] != 0)
            {
                projectile.frame = 1;
                projectile.width = 10;
                projectile.height = 16;

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
