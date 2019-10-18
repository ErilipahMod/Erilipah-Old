using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome
{
    public class StageI : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Stage I");
            Description.SetDefault("Infection slowly weakens you");

            Main.debuff[Type] = true;
            Main.persistentBuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed -= player.moveSpeed * 0.15f;
            player.maxRunSpeed -= player.maxRunSpeed * 0.25f;
            player.jumpSpeedBoost -= 1;
        }
    }

    public class StageII : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Stage II");
            Description.SetDefault("Infection rapidly consumes you");

            Main.debuff[Type] = true;
            Main.persistentBuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed -= player.moveSpeed * 0.30f;
            player.maxRunSpeed -= player.maxRunSpeed * 0.55f;
            player.jumpSpeedBoost -= 2;
        }
    }
}
