using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonCaster : ModNPC
    {
        // TODO: check source code for caster AI
        // TODO: telegraphs with orange line, fires a fast, large fire proj (dust)
        // TODO: inferno ball explodes on contact, MASSIVE damage and sends you flying, small ripple
        public override void SetStaticDefaults()
        {
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
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 32;
            npc.height = 32;

            npc.value = Item.buyPrice(0, 0, 75, 0);

            // npc.MakeBuffImmune(BuffID.OnFire);
        }

        public override void AI()
        {
            base.AI();
        }
    }
}
