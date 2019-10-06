using Erilipah.Items.Phlogiston;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phlogiston Sludge");
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults()
        {
            if (npc.scale == 1)
                npc.scale = 1.25f;
            npc.lifeMax = (int)(70 * npc.scale);
            npc.life = npc.lifeMax;
            npc.defense = (int)(8 * npc.scale);
            npc.damage = npc.scale < 1 ? 20 : 32;
            npc.knockBackResist = 0.2f / npc.scale;

            aiType = NPCID.LavaSlime;
            animationType = NPCID.BlueSlime;
            npc.aiStyle = 1;
            npc.noGravity = false;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.LiquidsWaterLava;

            npc.width = 36;
            npc.height = 26;

            npc.value = 125 * npc.scale;

            npc.lavaImmune = true;
            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor.R = Math.Max(drawColor.R, (byte)50);
            drawColor.G = Math.Max(drawColor.G, (byte)50);
            drawColor.B = Math.Max(drawColor.B, (byte)50);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<StablePhlogiston>(), 1, 2, npc.scale * 8);

            if (npc.scale > 0.65f && Main.netMode != 1)
                for (int i = -1; i < 2; i += 2)
                {
                    NPC child = Main.npc[NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, npc.type, Target: npc.target)];
                    child.scale = npc.scale * 0.7f;
                    child.velocity = new Vector2(i * 3, -2);
                }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Math.Max(SpawnCondition.Underworld.Chance * 0.08f, SpawnCondition.Cavern.Chance * 0.07f);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life < 0)
                for (int i = 0; i < 3; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), npc.scale);
                }
            else
            {
                Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), Math.Min(npc.scale, (float)damage / npc.lifeMax));
            }
        }

        public override bool CheckDead()
        {
            return true;
        }

        private int timer = 0;
        public override void SendExtraAI(BinaryWriter writer) => writer.Write(timer);
        public override void ReceiveExtraAI(BinaryReader reader) => timer = reader.ReadInt32();

        public override void AI()
        {
            if (timer == 0)
            {
                SetDefaults();
            }

            timer++;
            if (npc.scale > 0.7f && timer > 240 && timer < 300)
            {
                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2CircularEdge(30, 30), mod.DustType<PhlogistonDust>(), Vector2.One).customData = npc.Center;
            }
            if (npc.scale > 0.7f && timer > 300)
            {
                if (timer % 5 == 0 && Main.netMode != 1)
                {
                    int damage = (int)(Main.expertMode ? npc.damage * 0.7 : npc.damage);
                    float radians = MathHelper.SmoothStep(0, MathHelper.Pi, (timer - 300) / 120f);
                    Vector2 velocity = (-3.2f * Vector2.UnitX).RotatedBy(radians);
                    Projectile.NewProjectile(npc.Center, velocity, mod.ProjectileType<PhlogistonStream>(), damage, 1);
                }
            }
            if (timer > 420)
            {
                npc.netUpdate = true;
                timer = 1;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }
    }

    public class PhlogistonStream : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center, mod.DustType<PhlogistonDust>(), projectile.velocity / 8).customData = 0.01f;
            projectile.velocity.Y += 0.042f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }
    }
}
