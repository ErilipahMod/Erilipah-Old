using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
        // TODO: Phlog loot & better dusts, spawns cavern to hell
        // TODO: Stats + Splits into two half-scale copies on death if in Expert and not already halved; explodes on contact with player dealing massive damage; smaller copies have normal slime AI
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
            npc.damage = npc.scale < 1 ? 12 : 20;
            npc.knockBackResist = 0.2f / npc.scale;

            npc.aiStyle = 1;
            npc.noGravity = false;
            npc.HitSound = new LegacySoundStyle(SoundID.Lavafall, 0);
            npc.DeathSound = SoundID.LiquidsWaterLava;

            npc.width = 36;
            npc.height = 26;

            npc.value = 125 * npc.scale;

            npc.MakeBuffImmune(BuffID.OnFire);
            npc.lavaImmune = true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (SpawnCondition.Underworld.Active)
                return SpawnCondition.Underworld.Chance * 0.08f;
            else if (SpawnCondition.Cavern.Active)
                return SpawnCondition.Cavern.Chance * 0.08f;
            return 0;
        }

        public override bool CheckDead()
        {
            if (npc.scale <= 0.2f)
                return true;

            for (int i = -1; i < 2; i += 2)
            {
                NPC child = Main.npc[NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, npc.type, Target: npc.target)];
                child.scale = npc.scale * 0.65f;
                child.velocity = new Vector2(i * 3, -2);
            }
            return true;
        }

        public override void AI()
        {
            base.AI();
        }
    }
}
