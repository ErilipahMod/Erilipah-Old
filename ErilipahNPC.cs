﻿using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int witherStack = 0;

        public override bool CheckDead(NPC npc)
        {
            if (witherStack > 0)
            {
                int closestNPC = npc.FindClosestNPC(1000);
                if (closestNPC == -1)
                    return true;

                Main.npc[closestNPC].GetGlobalNPC<ErilipahNPC>().witherStack += this.witherStack;
            }
            return true;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(mod.BuffType<Items.ErilipahBiome.Wither>()))
                damage = witherStack;
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

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.InErilipah())
            {
                spawnRate = (int)(spawnRate * 0.5);
                maxSpawns = (int)(maxSpawns * 0.8);
            }
        }
    }
}
