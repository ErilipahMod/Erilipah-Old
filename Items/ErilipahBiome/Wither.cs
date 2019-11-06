using Erilipah.NPCs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class Wither : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "Erilipah/Debuff";
            return true;
        }
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
        }

        private int Stack(NPC npc) => npc.GetGlobalNPC<ErilipahNPC>().witherStack;
        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            // The increase in time, decreases over time
            float inc = time / (npc.buffTime[buffIndex] / 90f);
            npc.buffTime[buffIndex] += (int)inc;

            int newStack = Math.Min(12, npc.buffTime[buffIndex] / 60);
            if (newStack > Stack(npc))
                npc.GetGlobalNPC<ErilipahNPC>().witherStack = newStack;
            return false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
            npc.lifeRegen -= Stack(npc) * 4 + 4;

            if (Main.rand.NextBool(3))
            {
                Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, 109, newColor: Color.Black)].noGravity = true;
            }

            if (npc.buffTime[buffIndex] <= 1)
            {
                if (Stack(npc) > 1)
                {
                    npc.buffTime[buffIndex] = 90;
                    npc.GetGlobalNPC<ErilipahNPC>().witherStack -= 1;
                }
            }
        }
    }
}
