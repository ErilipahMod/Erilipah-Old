using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    public class CityPainting : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Painting6X4, 0));
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.AnchorValidWalls = new[] { mod.WallType<TaintedBrick.TaintedBrickWall>() };
            TileObjectData.newTile.AnchorWall = true;
            TileObjectData.addTile(Type);
            dustType = -1;
            disableSmartCursor = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Painting");
            AddMapEntry(new Color(100, 20, 125), name);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int style = frameX / 18 + 1;
            Item.NewItem(i * 16, j * 16, 16, 48, mod.ItemType("CityPaintingItem" + style));
        }
    }
    public class CityPaintingItem1 : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lurking Darkness");
            Tooltip.SetDefault("M. Burn");
        }
        public override void SetDefaults()
        {
            item.width = 10;
            item.height = 24;
            item.maxStack = 99;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.consumable = true;
            item.rare = 0;
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.createTile = mod.TileType("SoulStatue");
            item.placeStyle = 0;
        }
    }
}
