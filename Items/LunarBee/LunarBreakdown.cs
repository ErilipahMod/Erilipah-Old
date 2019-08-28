using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class LunarBreakdown : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Lunar Breakdown");
            Description.SetDefault("Your neurons are frying");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen = System.Math.Min(player.lifeRegen, -9);
            player.lifeRegenTime = 0;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = System.Math.Min(npc.lifeRegen, -9);
        }

    }
}