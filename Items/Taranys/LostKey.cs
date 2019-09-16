using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    class LostKey : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("One of the last keys to a place long forgotten\n'It bears the markings of a throne'");
        }
        public override void SetDefaults()
        {
            item.maxStack = 1;
            item.useTime = item.useAnimation = 80;

            item.useStyle = ItemUseStyleID.HoldingUp;

            item.width = 44;
            item.height = 24;

            item.value = 0;
            item.rare = ItemRarityID.LightRed;
        }

        public override bool UseItem(Player player)
        {
            Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 6, 1, -0.6f);
            if (player.itemAnimation < 20)
            {
                player.itemAnimation = 0;
                player.itemTime = 0;
                player.Center = ErilipahWorld.ChasmPosition;

                // Make sure the player doesn't instantly fucking die
                player.AddBuff(BuffID.Featherfall, 300);
                player.immune = true;
                player.immuneTime = 300;
            }
            return true;
        }
    }
}
