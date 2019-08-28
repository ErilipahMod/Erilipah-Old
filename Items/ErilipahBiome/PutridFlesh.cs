using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    class PutridFlesh : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("'Looks...tasty?'");
        }
        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.width = 28;
            item.height = 22;

            item.value = 20;
            item.rare = ItemRarityID.White;
        }
    }
}
