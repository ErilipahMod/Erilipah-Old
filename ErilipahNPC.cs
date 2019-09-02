using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahNPC : GlobalNPC
    {
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(mod.BuffType<Items.Weapons.Wither>()))
                damage = 4;
        }
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Merchant && NPC.downedBoss1)
            {
                shop.item[nextSlot++].SetDefaults(mod.ItemType("ModelMoon"));
                shop.item[nextSlot++].SetDefaults(mod.ItemType("ConversionToken"));
                shop.item[nextSlot++].SetDefaults(mod.ItemType("UpgradeToken"));
                shop.item[nextSlot++].SetDefaults(mod.ItemType("DowngradeToken"));
            }
            if (type == NPCID.GoblinTinkerer)
            {
                shop.item[nextSlot++].SetDefaults(mod.ItemType("DuctTape"));
            }
            if (type == NPCID.ArmsDealer)
            {
                shop.item[nextSlot++].SetDefaults(mod.ItemType("GunMold"));
                shop.item[nextSlot++].SetDefaults(mod.ItemType("Bandolier"));
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.player.InErilipah())
                return;

            int[] erilipianNPCs = new int[] {
                mod.NPCType("ErilipahSludge"),
                mod.NPCType("Seeker")
            };

            foreach (var pair in pool.ToList())
            {
                if (!erilipianNPCs.Contains(pair.Key))
                {
                    pool[pair.Key] *= 0.65f;
                }
            }
        }
    }
}
