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
            npc.lifeMax = 120;
            npc.defense = 2;
            npc.damage = 15;
            npc.knockBackResist = 0f;

            npc.aiStyle = -1;
            npc.noGravity = true;

            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            npc.width = 30;
            npc.height = 36;

            npc.value = 1000;

            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            this.DrawGlowmask(spriteBatch, Color.White * 0.5f);
        }

        public override void FindFrame(int frameHeight)
        {
            npc.Animate(frameHeight, 5);
        }

        // ai0 for torch X
        // ai1 for torch Y
        // ai2 for timer

        // ai2 > 150 for "is disappearing"

        private const float speed = 3;
        private const int chooseLeave = 120;
        private const int finishSnuff = 160;

        private Player Target => Main.player[npc.target];
        private Point Torch { get { return new Point((int)npc.ai[0], (int)npc.ai[1]); } set { npc.ai[0] = value.X; npc.ai[1] = value.Y; } }
        private Rectangle NPCTileRect => new Rectangle((int) npc.position.X / 16, (int) npc.position.Y / 16, npc.width / 16, npc.height / 16);

        public override void AI()
        {
            base.AI();

            SpewDust();

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
                if (npc.Distance(Torch.ToWorldCoordinates()) > 5)
                {
                    npc.velocity = npc.DirectionTo(Torch.ToWorldCoordinates()) * speed;
                }
                else
                {
                    if (npc.ai[2] < 0)
                        npc.ai[2] = 0;
                    npc.ai[2]++;

                    npc.velocity = Vector2.Zero;

                    if (npc.ai[2] > finishSnuff)
                    {
                        npc.ai[2] = 0;
                        ErilipahTile.Snuff(Torch.X, Torch.Y, true);
                    }
                }
            }
        }

        private void SpewDust()
        {
            Dust.NewDustPerfect(
                npc.Bottom,
                mod.DustType<FlowerDust>(),
                new Vector2(
                    Main.rand.NextFloat(-0.3f, 0.3f),
                    6
                    ),
                Scale: 1.2f
                );
        }

        private void Leave()
        {
            npc.velocity += npc.DirectionTo(Target.Center) * 0.05f;
            if (npc.velocity.Length() > 3)
                npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 3;

            if (npc.alpha >= 250 || Lighting.BrightnessAverage(NPCTileRect.X, NPCTileRect.Y, NPCTileRect.Width, NPCTileRect.Height) < 0.1f)
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

            for (int i = tilePos.X - 20; i < tilePos.X + 20; i++)
            {
                for (int j = tilePos.Y - 20; j < tilePos.Y + 20; j++)
                {
                    float currentDistance = Vector2.Distance(new Vector2(i, j), tilePos.ToVector2());
                    bool light = IsLight(i, j);
                    if (currentDistance < closestDistance && light)
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
            Tile candidate = Main.tile[i, j];
            bool light = candidate.type == TileID.Torches || candidate.type == TileID.Campfire || TileLoader.IsTorch(candidate.type);
            return candidate.active() && light;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                for (int i = 0; i <= 3; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(2, 2) + Vector2.UnitX * hitDirection * 2.5f,
                        mod.GetGoreSlot("Gores/ERBiome/Shell" + i));
                }
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
            return spawnInfo.player.InErilipah() ? 0.045f : 0;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<PutridFlesh>(), 1, 1, 18);
        }
    }
}
