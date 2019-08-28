using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome
{
    public class Watched : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Watched");
            Description.SetDefault("Stay in the light");

            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            rare = ItemRarityID.Purple;
        }
    }
}
