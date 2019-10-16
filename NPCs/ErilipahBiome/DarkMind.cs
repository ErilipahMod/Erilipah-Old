using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Erilipah.Items.ErilipahBiome;
using Erilipah.Biomes.ErilipahBiome.Hazards;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class DarkMind : ModNPC
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("DarkMind");
            Main.npcFrameCount[npc.type] = 8;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 100 : 40;
            npc.defense = 7;
            npc.damage = 15;
            npc.knockBackResist = 0f;

            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit13;
            npc.DeathSound = SoundID.NPCDeath22;

            npc.width = 30;
            npc.height = 36;

            npc.value = 1000;

            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.5f);
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.Animate(frameHeight, 5);
        }

        // ai0 for torch X
        // ai1 for torch Y
        // ai2 for timer
        // ai3 for fade in

        // ai2 > 150 for "is disappearing"

        private Player Target => Main.player[npc.target];
        private Point Torch { get { return new Point((int)npc.ai[0], (int)npc.ai[1]); } set { npc.ai[0] = value.X; npc.ai[1] = value.Y; } }

        public override void AI()
        {
            const float speed = 5f;
            const int chooseLeave = 30;
            const int finishSnuff = 100;

            if (npc.target == -1 || npc.target == 255)
                npc.target = npc.FindClosestPlayer();

            bool invalidTorch = Torch == Point.Zero || !IsLight(Torch.X, Torch.Y);
            bool shouldLeave = npc.life < 30 || npc.ai[2] < -chooseLeave;
            if (shouldLeave || invalidTorch)
            {
                if (npc.ai[2] > 0)
                    npc.ai[2] = 0;
                npc.ai[2]--;

                if (shouldLeave)
                {
                    npc.netUpdate = true;
                    Leave();
                }
                else
                {
                    npc.netUpdate = true;
                    npc.velocity /= 2;
                    Torch = FindTorch();
                }
            }
            else
            {
                float dist = Vector2.Distance(npc.Center, Torch.ToWorldCoordinates());
                if (dist > 3)
                {
                    float distToSpeed = MathHelper.Lerp(speed, speed / 4, 1 - dist / 300f);
                    npc.velocity = npc.DirectionTo(Torch.ToWorldCoordinates()) * Math.Min(10, distToSpeed);
                }
                else
                {
                    if (npc.ai[2] < 0)
                        npc.ai[2] = 0;

                    // Increase faster the more there are focused on this boi
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == npc.type && Vector2.Distance(Main.npc[i].Center, npc.Center) < 6)
                            npc.ai[2]++;
                    }

                    npc.velocity = Vector2.Zero;
                    npc.Center = Torch.ToWorldCoordinates();

                    if (npc.ai[2] % 20 == 0)
                    {
                        Main.PlaySound(SoundID.LiquidsHoneyLava, npc.Center);
                    }

                    if (npc.ai[2] > finishSnuff)
                    {
                        npc.ai[2] = 0;
                        ErilipahTile.Snuff(Torch.X, Torch.Y, true);
                    }
                }
            }

            SpewDust();
        }

        private void SpewDust()
        {
            Dust.NewDustPerfect(
                npc.Bottom,
                mod.DustType<FlowerDust>(),
                new Vector2(
                    Main.rand.NextFloat(-0.6f, 0.6f),
                    5
                    ),
                Scale: 1.2f * ((255 - npc.alpha) / 255f)
                );
        }

        private void Leave()
        {
            npc.velocity += npc.DirectionTo(Target.Center) * -0.065f;
            if (npc.velocity.Length() > 3)
                npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 4;

            if (npc.alpha >= 250)
            {
                npc.netUpdate = true;
                npc.active = false;
            }

            npc.alpha++;
        }

        private Point FindTorch(bool centerPlayer = false)
        {
            Point tilePos = centerPlayer ? Target.Center.ToTileCoordinates() : npc.Center.ToTileCoordinates();
            Point closest = Point.Zero;
            float closestDistance = 1000;

            const int rad = 30;
            for (int i = tilePos.X - rad; i < tilePos.X + rad; i++)
            {
                for (int j = tilePos.Y - rad; j < tilePos.Y + rad; j++)
                {
                    float currentDistance = Vector2.Distance(new Vector2(i, j), tilePos.ToVector2());
                    bool light = IsLight(i, j);
                    bool canSee = centerPlayer ?
                        Collision.CanHitLine(Target.Top, 1, 1, new Vector2(i + 0.5f, j) * 16, 1, 1) : 
                        Collision.CanHitLine(npc.Top, 1, 1, new Vector2(i + 0.5f, j) * 16, 1, 1);
                    if (currentDistance < closestDistance && light && canSee)
                    {
                        closestDistance = currentDistance;
                        closest = new Point(i, j);
                    }
                }
            }

            if (!centerPlayer && closest == Point.Zero)
            {
                closest = FindTorch(true);
            }

            return closest;
        }

        private static bool IsLight(int i, int j)
        {
            if (i == 0 && j == 0)
                return false;

            Tile candidate = Main.tile[i, j];
            bool light = candidate.type == TileID.Torches || candidate.type == TileID.Campfire || TileLoader.IsTorch(candidate.type);
            return candidate.active() && light;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    float rot = i / 15f * MathHelper.TwoPi;
                    Dust.NewDustPerfect(npc.Center, mod.DustType<FlowerDust>(), rot.ToRotationVector2() * 5, Scale: 1.5f).noGravity = true;
                }

                for (int i = 0; i <= 3; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(2, 2) + Vector2.UnitX * hitDirection * 2.5f,
                        mod.GetGoreSlot("Gores/ERBiome/Shell" + i));
                }

                Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(2, 2) + Vector2.UnitX * hitDirection * 2.5f,
                    mod.GetGoreSlot("Gores/ERBiome/Eye0"));
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType<FlowerDust>());
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InErilipah() ? 0.065f : 0;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<PutridFlesh>(), 1, 1, 18);
        }
    }
}
