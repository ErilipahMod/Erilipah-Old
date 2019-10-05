using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class GiantPF : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSpelunker[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Dressers, 0));
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType<Tiles.InfectedClump>() };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 18, 18 };
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Giant Pure Flower");
            AddMapEntry(new Color(60, 30, 70), name);

            dustType = mod.DustType<FlowerDust>();
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 2;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 6;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
#pragma warning disable IDE0062 // Make local function 'static'
            Vector2 Vel() => Main.rand.NextVector2Circular(2, 2);
#pragma warning restore IDE0062 // Make local function 'static'

            int t1 = mod.GetGoreSlot("Gores/ERBiome/GiantPFGore0");
            int t2 = mod.GetGoreSlot("Gores/ERBiome/GiantPFGore1");

            Gore.NewGore(new Vector2(i, j + 1) * 16,     Vel(), t1);
            Gore.NewGore(new Vector2(i + 2, j + 1) * 16, Vel(), t1);

            Gore.NewGore(new Vector2(i, j) * 16,         Vel(), t2);
            Gore.NewGore(new Vector2(i + 2, j) * 16,     Vel(), t2);

            if (!WorldGen.gen && Main.netMode != 1)
            {
                Item.NewItem(i*16, j*16, 36, 54, mod.ItemType<PureFlower>(), Main.rand.Next(1, 5));
            }
        }
    }
}