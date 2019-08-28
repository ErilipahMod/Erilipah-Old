using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    class PureFlower : ModItem
    {
        public override string Texture => "Terraria/Item_" + ItemID.Daybloom;

        public override void SetDefaults()
        {
            item.color = Color.LawnGreen;

            item.maxStack = 1;
            item.width = 32;
            item.height = 32;
            item.rare = 0;
        }

        public override bool OnPickup(Player player)
        {
            Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, Main.rand.NextFloat(-0.1f, 0.3f));
            player.I().Infect(Main.expertMode ? -5f : -3f);
            item.TurnToAir();
            return false;
        }
    }
}
