using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class ErilipahSludge : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystallized Sludge");
            Main.npcFrameCount[npc.type] = 2;
            NPCID.Sets.TrailingMode[npc.type] = 0;
            NPCID.Sets.TrailCacheLength[npc.type] = 5;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 120;
            npc.defense = 12;
            npc.damage = 30;
            npc.SetInfecting(1.75f);
            npc.knockBackResist = 0f;

            animationType = NPCID.BlueSlime;
            npc.aiStyle = 1;
            npc.noGravity = false;
            npc.HitSound = SoundID.NPCHit1.WithPitchVariance(-0.4f);
            npc.DeathSound = SoundID.NPCDeath22.WithPitchVariance(-0.6f);
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 52;
            npc.height = 32;

            npc.value = Item.buyPrice(0, 0, 8, 0);

            // npc.MakeBuffImmune(BuffID.OnFire);
        }

        int timer = 0;
        public override void AI()
        {
            Player target = Main.player[npc.target];
            float distance = npc.Distance(target.Center);
            bool canSeeTarget = Collision.CanHitLine(target.position, target.width, target.height, npc.position, npc.width, npc.height);
            if (timer >= 300 || (canSeeTarget && distance < 1000))
                timer++;

            npc.aiStyle = 1;
            npc.damage = npc.defDamage;

            if (timer < 300)
                return;

            npc.damage *= 2;
            npc.aiStyle = 0;
            if (timer == 300)
                npc.velocity.Y -= 6;

            if (timer < 380)
            {
                npc.velocity = npc.Center.To(target.Center - new Vector2(0, 250), 8);
                Dust.NewDustPerfect(npc.position + new Vector2(4, 20), mod.DustType<VoidParticle>(), Vector2.Zero);
                Dust.NewDustPerfect(npc.TopRight - new Vector2(4, -20), mod.DustType<VoidParticle>(), Vector2.Zero);
            }
            else
            {
                npc.velocity = new Vector2(0, 14);
            }

            if (timer > 460 || npc.collideY)
            {
                npc.netUpdate = true;
                timer = 0;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            //npc.DrawGlowmask("Biomes/Erilipah/AbominationGlow", Color.White * 0.75f, spriteBatch);
            if (timer > 380)
            {
                const int blurLength = 5;

                Texture2D texture = Main.npcTexture[npc.type];
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / Main.npcFrameCount[npc.type]) * 0.5f);
                for (int i = 0; i < Math.Min(blurLength, npc.oldPos.Length); i++)
                {
                    Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, npc.gfxOffY);
                    Color color = npc.GetAlpha(drawColor) * ((blurLength - i) / (float)blurLength);
                    SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: npc.frame, color: color, rotation: npc.rotation,
                        origin: drawOrigin, scale: npc.scale, effects: effects, layerDepth: 0
                        );
                }
            }
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor *= 2f;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timer = reader.ReadInt32();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleCrystalShard, Scale: npc.scale);
            }
            if (npc.life <= 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.PurpleCrystalShard, Scale: npc.scale);
                }
            }
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<PutridFlesh>(), 1, 1, 18);
            Loot.DropItem(npc, ItemID.Heart, 1, 1, 50);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InErilipah() ? 0.09f : 0;
        }
    }
}
