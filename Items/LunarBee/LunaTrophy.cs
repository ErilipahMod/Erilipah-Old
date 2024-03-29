﻿using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class LunaTrophy : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunaemia Trophy");
        }
        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 30;
            item.maxStack = 99;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.consumable = true;
            item.value = 50000;
            item.rare = 1;
            item.createTile = mod.TileType("BossTrophy");
            item.placeStyle = 0;
        }
    }
}
