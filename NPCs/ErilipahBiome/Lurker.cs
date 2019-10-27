using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.ErilipahBiome
{
    class LurkerHead : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lurker");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 200;
            npc.defense = 16;
            npc.damage = 40;
            npc.knockBackResist = 0f;
            npc.SetInfecting(5f);

            npc.aiStyle = 0;
            npc.noGravity = false;

            npc.HitSound = SoundID.NPCHit49;
            npc.DeathSound = SoundID.NPCDeath52;

            npc.width = 32;
            npc.height = 32;

            npc.value = 100;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        private Player Target => Main.player[npc.target];
        
        private int BodyAlive { get => (int)npc.ai[0]; set => npc.ai[0] = value; }

        public override void AI()
        {
            npc.direction = npc.spriteDirection = (Target.Center.X > npc.Center.X) ? 1 : -1;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            Vector2 eye = npc.GetSpritePosition(12, 16);

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(eye, DustID.PurpleCrystalShard, new Vector2(4 * hitDirection, Main.rand.NextFloat(-3, 3))); 
            }

            if (npc.life <= 0)
            {
                Gore.NewGore(npc.Center, npc.velocity, mod.GetGoreSlot("Gores/ERBiome/LurkerHead"));
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Main.hardMode && spawnInfo.player.InErilipah() ? 0.07f : 0;
        }
    }
}
