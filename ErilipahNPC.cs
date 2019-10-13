using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;

namespace Erilipah
{
    public class ErilipahNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int witherStack = 0;
        private int crystalInfec = 0;
        public int CrystalInfectionDamage => Math.Min(12, crystalInfec / 90);

        public override bool CheckDead(NPC npc)
        {
            // Spread Wither to nearby NPCs
            if (witherStack > 0)
            {
                int closestNPC = npc.FindClosestNPC(1000);
                if (closestNPC == -1)
                    return true;

                int wither = mod.BuffType<Items.ErilipahBiome.Wither>();
                int myTime = npc.buffTime[npc.FindBuffIndex(wither)];
                int theirTime = Main.npc[closestNPC].buffTime[npc.FindBuffIndex(wither)];
                Main.npc[closestNPC].AddBuff(wither, myTime + theirTime);
            }
            return true;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff(mod.BuffType<Items.Taranys.CrystalInfection>()))
            {
                damage = crystalInfec;
                crystalInfec++;
            }
            else
            {
                crystalInfec = 0;
            }

            if (npc.HasBuff(mod.BuffType<Items.ErilipahBiome.Wither>()))
            {
                damage = witherStack;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(mod.BuffType<Items.Taranys.CrystalInfection>()))
            {
                int type = mod.BuffType<Items.Taranys.CrystalInfection>();
                int time = npc.buffTime[npc.FindBuffIndex(type)];
                float intensity = Math.Min(time / 300f, 0.35f);

                drawColor = Color.Lerp(drawColor, Color.MediumVioletRed, intensity);
            }
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
            if (spawnInfo.player.InErilipah())
            {
                if (pool.ContainsKey(NPCID.BlueSlime))
                    pool[NPCID.BlueSlime] *= 0.25f;
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.InErilipah())
            {
                spawnRate = (int)(spawnRate * 1.25);
                maxSpawns = (int)(maxSpawns * 1.75);
            }
        }
    }
}
