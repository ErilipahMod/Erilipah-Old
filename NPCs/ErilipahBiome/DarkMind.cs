using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Erilipah.NPCs.ErilipahBiome
{
    class DarkMind : ModNPC
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("DarkMind");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 100;
            npc.defense = 5;
            npc.damage = 10;
            npc.knockBackResist = 1f;

            npc.aiStyle = 0;
            npc.noGravity = false;

            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            npc.width = 32;
            npc.height = 32;

            npc.value = 100;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void AI()
        {
            base.AI();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            base.HitEffect(hitDirection, damage);
        }

        public override void NPCLoot()
        {
            base.NPCLoot();
        }
    }
}
