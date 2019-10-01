using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome
{
    public class LightWispDust : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.01f;
                dust.position += dust.velocity;
            }

            dust.scale -= 0.01f;
            if (dust.scale <= 0.1f)
                dust.active = false;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(255, 255, 255);
        }
    }
    public class LightWisp : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.hide = true;
            npc.lifeMax = 1;
            npc.defense = 0;
            npc.damage = 0;
            npc.knockBackResist = 1f;

            npc.aiStyle = 64;
            npc.noGravity = true;
            npc.DeathSound = SoundID.NPCDeath3;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 4;
            npc.height = 4;

            npc.timeLeft = 1200;
            npc.value = Item.buyPrice(0, 0, 75, 0);
            lightPulse = 0;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(lightPulse);
            writer.Write(returnTimer);
            writer.WriteVector2(returnPos);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            lightPulse = reader.ReadSingle();
            returnTimer = reader.ReadInt32();
            returnPos = reader.ReadVector2();
        }

        public Vector2 returnPos = Vector2.Zero;
        private int returnTimer = 0;
        private float lightPulse = 0;
        public override void AI()
        {
            Effects();

            if (npc.timeLeft < 180)
            {
                npc.netUpdate = true;
                npc.scale -= 1 / 200f;
                npc.aiStyle = 0;
                npc.velocity *= 0.9f;
            }

            if (returnPos != Vector2.Zero)
            {
                npc.timeLeft = 300;
                returnTimer++;

                if (!Collision.CanHit(npc.position, 4, 4, returnPos, 1, 1))
                {
                    returnTimer = 0;
                    returnPos = Vector2.Zero;
                    npc.netUpdate = true;
                }
            }

            if (returnTimer > 600)
            {
                npc.aiStyle = 0;
                npc.velocity *= 0.98f;
                npc.velocity += npc.Center.To(returnPos, 0.01f);
                npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -2.5f, Vector2.One * 2.5f);

                if (npc.Distance(returnPos) < 20)
                {
                    npc.active = false;
                }
            }
        }

        private void Effects()
        {
            lightPulse += 0.005f;
            if (lightPulse > 1)
                lightPulse = -1;

            Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(3, 3), mod.DustType<LightWispDust>(), Vector2.Zero, Scale: npc.scale).noGravity = true;
            Lighting.AddLight(npc.Center, new Vector3(0.89f, 0.58f, 0.76f) * Math.Abs(lightPulse * 1.3f) * npc.scale);

            if (Main.rand.NextBool(500))
            {
                Main.PlaySound(27, (int)npc.Center.X, (int)npc.Center.Y, 0, 0.5f, 0.4f);
            }
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor = Color.White;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InErilipah() ? 0.05f : 0;
        }
    }
    public class LightWispGuard : ModNPC
    {
        public override string Texture => base.Texture.Replace("Guard", "");
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 50;
            npc.defense = 6;
            npc.damage = 20;
            npc.knockBackResist = 0.3f;

            npc.aiStyle = 64;
            npc.noGravity = true;
            npc.DeathSound = SoundID.NPCDeath3;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.scale = 1.35f;
            npc.width = 4;
            npc.height = 4;

            npc.timeLeft = 1200;
            npc.value = Item.buyPrice(0, 0, 75, 0);
            lightPulse = 0;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(lightPulse);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            lightPulse = reader.ReadSingle();
        }

        private float lightPulse = 0;
        public override void AI()
        {
            Effects();

            if (npc.timeLeft < 180)
            {
                npc.netUpdate = true;
                npc.scale -= 1 / 200f;
                npc.aiStyle = 0;
                npc.velocity *= 0.9f;
            }

            if (npc.target != 255 && npc.target != -1)
            {
                npc.aiStyle = 5;
                aiType = NPCID.Bee;
            }
        }

        private void Effects()
        {
            lightPulse += 0.003f;
            if (lightPulse > 1)
                lightPulse = -1;

            for (int i = 0; i < 2; i++)
            {
                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(3, 3), mod.DustType<LightWispDust>(), Vector2.Zero, Scale: npc.scale);
            }
            Lighting.AddLight(npc.Center, new Vector3(227, 148, 190) * Math.Abs(lightPulse) * npc.scale);

            if (Main.rand.NextBool(200))
            {
                Main.PlaySound(27, (int)npc.Center.X, (int)npc.Center.Y, 0, 0.8f, 0.2f);
            }
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor = Color.White;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
        }
    }
}
