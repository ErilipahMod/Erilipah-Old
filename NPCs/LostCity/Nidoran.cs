using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Erilipah.Biomes.ErilipahBiome.Hazards;
using System.IO;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.LostCity
{
    class Nidoran : ModNPC
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Nidoran");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 300 : 220;
            npc.defense = Main.hardMode ? 26 : 18;
            npc.damage = Main.hardMode ? 58 : 38;
            npc.knockBackResist = Main.hardMode ? 0 : 0.02f;

            npc.aiStyle = 3;
            npc.noGravity = false;

            npc.HitSound = SoundID.NPCHit7;
            npc.DeathSound = SoundID.NPCDeath43;

            npc.width = 40;
            npc.height = 52;

            npc.value = 1666;

            npc.buffImmune[BuffID.OnFire] = true;

            attackTimer = 0;
        }

        private float attackTimer = 0;
        private float slamTimer = 0;
        private float blockTimer = 0;

        private bool Blocking { get => blockTimer > 0; }
        private bool Slamming { get => slamTimer  > 0; }

        private Player Target => Main.player[npc.target];
        private bool CanHitTarget => Collision.CanHit(npc.Center, 1, 1, Target.Center, 1, 1);

        public override void AI()
        {
            attackTimer++;
            npc.spriteDirection = npc.direction;

            // Play sound
            if (Main.rand.NextBool(800))
            {
                Main.PlaySound(3, (int)npc.Center.X, (int)npc.Center.Y, 58, 1, -0.2f);
            }

            if (!Blocking && !Slamming)
            {
                Walk();

                if (attackTimer > 300 && npc.velocity.Y == 0)
                {
                    StartBlocking();
                }
                if (attackTimer > 200 && npc.velocity.Y == 0 && !CanHitTarget)
                {
                    StartSlamming(); // make slam not rart
                    // TODO make nidoran a pre-hardmode enemy
                }
            }
            else if (Blocking)
            {
                Block();

                if (blockTimer > 150 && npc.velocity.Y == 0 && CanHitTarget)
                {
                    StartSlamming();
                }
                if (blockTimer > 200)
                {
                    Reset();
                }
            }
            else if (Slamming)
            {
                bool done = Slam();

                if (done)
                    Reset();
            }
        }

        private void StartBlocking()
        {
            attackTimer = 0;
            blockTimer = 1;
            npc.aiStyle = -1;
            npc.netUpdate = true;
        }
        private void StartSlamming()
        {
            npc.dontTakeDamage = false;
            blockTimer = 0;
            slamTimer = 1;
            npc.aiStyle = -1;
            npc.netUpdate = true;
            npc.direction = npc.position.X > Target.position.X ? -1 : 1;
        }
        private void Reset()
        {
            attackTimer = 0;
            blockTimer = 0;
            slamTimer = 0;
            npc.aiStyle = 3;
            npc.netUpdate = true;
        }

        private void Walk()
        {
            npc.FaceTarget();
        }
        private void Block()
        {
            blockTimer++;

            npc.velocity.X *= 0.85f;
            npc.dontTakeDamage = true;
            // TODO Block animation


        }
        private bool Slam()
        {
            // TODO standing frame
            const int channelTimer = 35;
            if (slamTimer < channelTimer)
            {
                slamTimer++;
                if (npc.velocity.Y != 0)
                {
                    slamTimer = 1;
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                        Dust.NewDustDirect(npc.BottomLeft, npc.width, 1, DustType<FlowerDust>(), -8 * npc.direction, 0.01f, Scale: 1.7f);
                }
            }
            else if (slamTimer == channelTimer)
            {
                slamTimer++;
                // TODO raised arms
                npc.velocity = new Vector2(npc.direction * 8, -5);
                npc.netUpdate = true;
            }
            else // to 41; slam timer > 40
            {
                if (npc.collideX || (npc.collideY && npc.velocity.Y > 0))
                {
                    npc.netUpdate = true;
                    npc.velocity = Vector2.Zero;
                    Main.PlaySound(SoundID.NPCDeath14, npc.Center);

                    if (npc.collideX)
                    {
                        const int numDust = 16;
                        for (int i = 0; i < numDust; i++)
                        {
                            Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustType<FlowerDust>(),
                                npc.velocity.X * 6, npc.velocity.Y * 6, Scale: 1.7f);
                            d.noGravity = true;
                            //d.velocity = npc.velocity;
                        }
                    }
                    if (npc.collideY && npc.velocity.Y > 0)
                    {
                        const int numDust = 22;
                        for (int i = 0; i < numDust; i++)
                        {
                            Dust d = Dust.NewDustDirect(npc.BottomLeft, npc.width, 2, DustType<FlowerDust>(), 0, -18, Scale: 2.2f);
                            
                            d.noGravity = true;
                            //d.velocity = new Vector2(0, -6);
                        }
                    }

                    for (int i = -3; i <= 3; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            Helper.HitTile((int)npc.Center.X / 16 + i, (int)npc.Center.Y / 16 + j, 55);
                            Helper.HitTile((int)npc.Center.X / 16 + i, (int)npc.Center.Y / 16 + j, 55);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackTimer);
            writer.Write(blockTimer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackTimer = reader.ReadSingle();
            blockTimer = reader.ReadSingle();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (!Blocking && !Slamming)
            {
                npc.netUpdate = true;
                attackTimer += (float)damage;
            }
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustType<FlowerDust>(), Scale: 1.2f);
                d.noGravity = true;
                d.velocity = new Vector2(hitDirection * 4, -2);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InLostCity() ? 0.08f : (
                spawnInfo.player.InErilipah() && Main.hardMode ? 0.035f : 0);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemType<Items.ErilipahBiome.PutridFlesh>(), 1, 3);
        }
    }
}
