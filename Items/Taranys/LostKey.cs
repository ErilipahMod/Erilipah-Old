using Microsoft.Xna.Framework;
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
            if (player.InLostCity() || !player.InErilipah())
                return false;
            if (ErilipahWorld.ChasmPosition == Vector2.Zero)
            {
                Console.WriteLine("Lost City position not set");
                return false;
            }

            Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 6, 1, -0.6f);
            player.itemAnimation = 0;
            player.itemTime = 0;
            player.Center = ErilipahWorld.ChasmPosition;

            // Carve out an area for the player to spawn
            for (int i = -5; i <= 5; i++)
            {
                int x = (int)player.position.X / 16;
                for (int j = -5; j <= 5; j++)
                {
                    int y = (int)player.position.Y / 16;
                    Tile tile = Main.tile[x + i, y + j];

                    if (tile.type != mod.TileType<Biomes.ErilipahBiome.Tiles.TaintedBrick>())
                        WorldGen.KillTile(x + i, y + j);
                }
            }

            // Make sure the player doesn't instantly fucking die
            player.AddBuff(BuffID.Featherfall, 300);
            player.AddBuff(BuffID.Shine, 300);
            player.immune = true;
            player.immuneTime = 300;
            return true;
        }
    }
}
