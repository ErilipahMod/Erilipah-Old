using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class TEVent : ModTileEntity
    {
        internal int timer = 0;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            bool valid = tile.active() && tile.type == mod.TileType<Vent>() && tile.frameX == 0 && tile.frameY == 0;
            return valid;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public override void Update()
        {
            if (Vector2.Distance(Main.LocalPlayer.Center, Position.ToWorldCoordinates()) > Main.screenWidth + 200)
                return;

            Vector2 pos = new Vector2(Position.X * 16 + 16, Position.Y * 16 + 20);
            Dust.NewDustPerfect(pos, mod.DustType<AshDust>(), new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-6, -4)));
        }
    }

    class Vent : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Statues, 12));
            TileObjectData.newTile.HookPlaceOverride =
                new PlacementHook(mod.GetTileEntity<TEVent>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType<Tiles.InfectedClump>() };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.addTile(Type);

            dustType = mod.DustType<AshDust>();
            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Vent");
            AddMapEntry(new Color(80, 70, 100), name);
        }

        private void MakeVent(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int x = i - tile.frameX / 18;
            int y = j - tile.frameY / 18;

            if (!TileEntity.ByPosition.TryGetValue(new Point16(x, y), out _))
            {
                mod.GetTileEntity<TEVent>().Place(x, y);
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            MakeVent(i, j);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out var te))
                ((TEVent)te).Kill(i, j);
        }
    }
}
