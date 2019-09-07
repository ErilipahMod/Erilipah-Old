using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs
{
    internal class h : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults()
        {
            npc.CloneDefaults(NPCID.BlueSlime);
            npc.lifeMax = 1777;
            npc.defense = 30;
            npc.damage = 1;
            npc.knockBackResist = -2f;

            animationType = NPCID.BlueSlime;
            npc.aiStyle = 1;
            npc.noGravity = false;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 30;
            npc.height = 44;

            npc.value = Item.buyPrice(0, 10, 0, 0);
            npc.MakeDebuffImmune();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life > 0)
                Main.PlaySound(3, (int)npc.Center.X, (int)npc.Center.Y, Main.rand.Next(1, 58), 0.9f, Main.rand.NextFloat(-0.9f, 0.9f));
            else
                Main.PlaySound(4, (int)npc.Center.X, (int)npc.Center.Y, Main.rand.Next(1, 63), 0.9f, Main.rand.NextFloat(-0.9f, 0.9f));
            int coin = (int)(damage * 11.254924);
            if (coin >= 1000000)
                Loot.DropItem(npc.getRect(), ItemID.PlatinumCoin, coin / 1000000, coin / 1000000, 100, 2);
            if (coin >= 10000)
                Loot.DropItem(npc.getRect(), ItemID.GoldCoin, coin / 10000 % 1000000, coin / 10000 % 1000000, 100, 2);
            if (coin >= 100)
                Loot.DropItem(npc.getRect(), ItemID.SilverCoin, coin / 100 % 10000, coin / 100 % 10000, 100, 2);
            if (coin % 100 > 0)
                Loot.DropItem(npc.getRect(), ItemID.CopperCoin, coin % 100, coin % 100, 100, 2);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.OverworldDaySlime.Chance * 0.001f;
        }
    }
}
