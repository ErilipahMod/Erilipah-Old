using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class Lunacrita : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Lunacrita");
            Description.SetDefault("The Moon Wasp will fight for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ErilipahPlayer modPlayer = (ErilipahPlayer)player.GetModPlayer(mod, "ErilipahPlayer");
            bool hasMinion = player.ownedProjectileCounts[mod.ProjectileType("LunacritaProj")] > 0;
            if (hasMinion)
            {
                player.buffTime[buffIndex] = 18000;
            }
            if (!hasMinion || player.dead)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}