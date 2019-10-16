using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Vent : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Statues, 12));
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<Tiles.InfectedClump>() };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.addTile(Type);

            dustType = DustType<AshDust>();
            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Vent");
            AddMapEntry(new Color(80, 70, 100), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                for (int c = 0; c < 3; c++)
                {
                    Vector2 pos = new Vector2(i * 16 + 16, j * 16 + 22);
                    Dust.NewDustPerfect(pos, DustType<AshDust>(), new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-8f, -4.5f)));
                }
            }
        }
    }
}
