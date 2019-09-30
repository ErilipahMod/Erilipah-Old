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

    class Vent : HazardTile
    {
        public override string MapName => "Gas Geyser";
        public override int DustType => mod.DustType<AshDust>();
        public override TileObjectData Style => TileObjectData.Style2xX;

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
    }
}
