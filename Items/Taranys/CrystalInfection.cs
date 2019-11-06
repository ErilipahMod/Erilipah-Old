using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Taranys
{
    public class CrystalInfection : ModBuff
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

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
            npc.lifeRegen -= npc.GetGlobalNPC<ErilipahNPC>().CrystalInfectionDamage;

            if (Main.rand.NextBool())
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Scale: 1.5f);
                d.velocity = new Vector2(0, -5);
                d.noGravity = true;
            }

            int other = npc.FindClosestNPC(100);
            if (other != -1)
            {
                Main.npc[other].AddBuff(Type, npc.buffTime[buffIndex]);
            }
        }
    }
}
