using Erilipah.Items.Phlogiston;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults()
        {
            if (npc.scale == 1)
                npc.scale = 1.25f;
            npc.lifeMax = (int)(50 * npc.scale);
            npc.defense = (int)(8 * npc.scale);
            npc.damage = npc.scale < 1 ? 12 : 17;
            npc.knockBackResist = 0.2f / npc.scale;

            npc.aiStyle = 1;
            npc.noGravity = false;
            npc.HitSound = new LegacySoundStyle(39, 2).WithPitchVariance(0.15f);
            npc.DeathSound = SoundID.LiquidsWaterLava;

            npc.width = 36;
            npc.height = 26;

            npc.value = 125 * npc.scale;

            npc.MakeBuffImmune(BuffID.OnFire);
            npc.lavaImmune = true;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<StablePhlogiston>(), 1, 2, npc.scale * 30);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (SpawnCondition.Underworld.Active)
                return SpawnCondition.Underworld.Chance * 0.08f;
            else if (SpawnCondition.Cavern.Active)
                return SpawnCondition.Cavern.Chance * 0.08f;
            return 0;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life < 0)
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), npc.scale);
            }

            Gore.NewGore(npc.Center, Main.rand.NextVector2Unit() * 2, GoreID.ChimneySmoke1 + Main.rand.Next(3), Math.Min(npc.scale, (float)damage / npc.lifeMax));
        }

        public override bool CheckDead()
        {
            if (npc.scale <= 0.2f)
                return true;

            if (Main.netMode != 1)
                for (int i = -1; i < 2; i += 2)
                {
                    NPC child = Main.npc[NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, npc.type, Target: npc.target)];
                    child.scale = npc.scale * 0.65f;
                    child.velocity = new Vector2(i * 3, -2);
                }
            return true;
        }

        int timer = 0;
        public override void SendExtraAI(BinaryWriter writer) => writer.Write(timer);
        public override void ReceiveExtraAI(BinaryReader reader) => timer = reader.ReadInt32();

        public override void AI()
        {
            timer++;
            if (npc.scale > 0.7f && timer > 300)
            {
                if (timer % 5 == 0 && Main.netMode != 1)
                {
                    float radians = MathHelper.SmoothStep(0, MathHelper.Pi, (timer - 300) / 120f);
                    Vector2 velocity = (-3 * Vector2.UnitX).RotatedBy(radians);
                    Projectile.NewProjectile(npc.Center, velocity, mod.ProjectileType<PhlogistonStream>(), npc.damage, 1);
                }

                npc.scale -= 0.0015f;
            }
            if (timer > 420)
            {
                npc.netUpdate = true;
                timer = 0;
            }
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
            Dust.NewDustPerfect(projectile.Center, mod.DustType<PhlogistonDust>()).noGravity = true;
            projectile.velocity.Y += 0.04f;
        }
    }
}
