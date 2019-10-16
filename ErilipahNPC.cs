using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah
{
    public class ErilipahNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int witherStack = 0;
        private int crystalInfec = 0;
        public int CrystalInfectionDamage => (int)MathHelper.Clamp(crystalInfec / 60, 1, 15);

        public override bool CheckDead(NPC npc)
        {
            // Spread Wither to nearby NPCs
            if (witherStack > 0)
            {
                int dustT = DustType<NPCs.ErilipahBiome.VoidParticle>();
                for (int i = 0; i < 30; i++)
                {
                    float rot = i / 30f * MathHelper.TwoPi;
                    Dust.NewDustPerfect(npc.Center, dustT, rot.ToRotationVector2() * Main.rand.NextFloat(8, 11), newColor: Color.Black, Scale: 1.6f).noGravity = true;
                }

                int closestNPC = npc.FindClosestNPC(1000);
                if (closestNPC == -1)
                    return true;

                NPC other = Main.npc[closestNPC];

                int wither = BuffType<Items.ErilipahBiome.Wither>();
                int bIndex = npc.FindBuffIndex(wither);
                int myTime = npc.buffTime[bIndex];

                if (!other.HasBuff(wither))
                    other.AddBuff(wither, myTime + 30);
                else
                    BuffLoader.ReApply(wither, other, myTime + 30, bIndex);

                for (int i = 0; i < 30; i++)
                {
                    float rot = i / 30f * MathHelper.TwoPi;
                    Vector2 vel = rot.ToRotationVector2() * Main.rand.NextFloat(7.5f, 10);
                    Dust.NewDustPerfect(other.Center + vel * 10, dustT, -vel, newColor: Color.Black, Scale: 1.6f).noGravity = true;
                }

                for (int i = 0; i < 30; i++)
                {
                    Vector2 pos = Vector2.Lerp(npc.Center, other.Center, i / 30f);
                    Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6, 6), dustT, Vector2.Zero, newColor: Color.Black, Scale: 1.5f).noGravity = true;
                }
            }
            return true;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            bool wither = npc.HasBuff(BuffType<Items.ErilipahBiome.Wither>());
            bool crystal = npc.HasBuff(BuffType<Items.Taranys.CrystalInfection>());

            if (crystal)
            {
                damage = 3;
                crystalInfec++;
            }
            else
            {
                crystalInfec = 0;
            }

            if (wither)
            {
                if (witherStack > 22)
                    witherStack = 22;
                damage = witherStack;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(BuffType<Items.Taranys.CrystalInfection>()))
            {
                int type = BuffType<Items.Taranys.CrystalInfection>();
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
