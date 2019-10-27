using Erilipah.Biomes.ErilipahBiome.Tiles;
using Erilipah.Items.Crystalline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Stalk : HazardTile
    {
        public override string MapName => "Crystalline Stalk";
        public override int DustType => DustType<CrystallineDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinateHeights = new[] { 16 };
                TileObjectData.newTile.LinkedAlternates = true;
                TileObjectData.newTile.AnchorAlternateTiles = new int[] { TileType<Stalk>() };
                TileObjectData.newTile.AnchorValidTiles = new int[]
                { TileType<InfectedClump>(), TileType<SpoiledClump>(), TileType<Stalk>() };
                TileObjectData.newTile.AnchorBottom = new AnchorData(
                    AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
                TileObjectData.newTile.AnchorTop = AnchorData.Empty;

                return TileObjectData.newTile;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            soundType = 2;
            soundStyle = 27;
            drop = ItemType<CrystallineTileItem>();

            minPick = 65;
            mineResist = 2.5f;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tile = Main.tile[i, j];
            bool isBase = tile.frameX >= 54 && tile.frameY >= 90;

            if (!isBase && Main.tile[i, j + 1].type != Type)
            {
                WorldGen.KillTile(i, j, Main.rand.NextBool(3));
            }
            if (isBase)
            {
                int left = i - (tile.frameX - 54) / 18;

                bool notValidBase = !Main.tile[left, j].active() || !Main.tile[left + 1, j].active() || !Main.tile[left + 2, j].active();
                bool notValidFloor = !Main.tile[left, j + 1].ValidTop() || !Main.tile[left + 1, j + 1].ValidTop() || !Main.tile[left + 2, j + 1].ValidTop();

                if (!Main.tile[i, j + 1].IsErilipahTile() || notValidFloor || notValidBase)
                {
                    BreakBase(left, j);
                }

#pragma warning disable IDE0062 // Make local function 'static'
                void BreakBase(int xL, int nJ)
#pragma warning restore IDE0062 // Make local function 'static'
                {
                    WorldGen.KillTile(xL, nJ, Main.rand.NextBool());
                    WorldGen.KillTile(xL + 1, nJ, Main.rand.NextBool());
                    WorldGen.KillTile(xL + 2, nJ, Main.rand.NextBool());
                }
            }

            resetFrame = false;
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 4;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 2;
        }

        public override bool CanPlace(int i, int j) => IsValid(i, j);

        private void GrowMoreStalks(int i, int j)
        {
            for (int v = -1; v <= 1; v++)
            {
                bool left = Main.tile[i + 5, j + v].active();
                ErilipahWorld.PlaceHazard(i + (left ? -5 : 5), j + v, 0);
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            bool isTip = (tile.frameX == 54 && tile.frameY <= 18) || tile.frameY == 0;
            if (isTip)
                return;

            bool isBase = tile.frameX >= 54 && tile.frameY >= 90;
            bool isTop = tile.frameX >= 54 && tile.frameY < 90;
            bool isStalk = tile.frameX < 54 && tile.frameY >= 54;

            if (isBase)
            {
                // Grow a tile in the middle of the base
                int middleX = i - (tile.frameX - 54) / 18 + 1;
                if (!Main.tile[middleX, j - 1].active())
                {
                    Tile above = Main.tile[middleX, j - 1];
                    above.type = Type;
                    above.active(true);
                    above.frameX = (short)(Main.rand.Next(3) * 18);
                    above.frameY = 7 * 18;
                }

                GrowMoreStalks(i, j);
            }
            else if (isStalk)
            {
                Tile above = Main.tile[i, j - 1];
                if (above == null)
                {
                    above = new Tile();
                    above.active(false);
                }

                // Move up the stalk!
                if (above.active())
                {
                    if (above.type == Type)
                        Terraria.ModLoader.TileLoader.RandomUpdate(i, j - 1, Type);
                    return;
                }

                // % chance to start the tip of the stalk
                // Otherwise, continue growing the stalk
                if (Main.rand.Chance(0.05f) || Main.tile[i, j - 5].active())
                {
                    above.type = Type;
                    above.active(true);
                    above.frameX = (short)(Main.rand.Next(3, 6) * 18);
                    above.frameY = 72;
                }
                else
                {
                    above.type = Type;
                    above.active(true);
                    GetStalkFrame(out above.frameX, out above.frameY);
                }
            }
            else if (isTop)
            {
                Tile above = Main.tile[i, j - 1];

                // Move up the stalk!
                if (above.active())
                {
                    if (above.type == Type)
                        Terraria.ModLoader.TileLoader.RandomUpdate(i, j - 1, Type);
                    return;
                }
                else
                {
                    if (above == null)
                    {
                        above = new Tile();
                    }
                    above.type = Type;
                    above.active(true);
                    above.frameX = tile.frameX;
                    above.frameY = (short)(tile.frameY - 18);
                }
            }
#if DEBUG
            else
            {
                Main.LocalPlayer.Center = new Vector2(i, j) * 16;
                throw new Exception($"Invalid Crystalline Stalk frame: ({i}, {j})");
            }
#endif

            if (Main.netMode == 2 /*Sync to clients when run on the server*/)
                NetMessage.SendTileSquare(-1, i, j, 3);
        }

        private void GetStalkFrame(out short x, out short y)
        {
            x = (short)(Main.rand.Next(3) * 18);
            if (x == 0)
            {
                y = (short)(Main.rand.Next(3, 8) * 18);
            }
            else
            {
                y = (short)(Main.rand.Next(4, 8) * 18);
            }
        }

        public static bool IsValid(int i, int j)
        {
            // Check if the stalk area is clear
            for (int e = -1; e <= 1; e++)
                for (int f = -7; f < 0; f++)
                {
                    Tile t = Main.tile[i + e, j + f];
                    bool isActive = t.active() && t.type != (ushort)TileType<Mushroom>();
                    if (isActive || t.wall > 0)
                        return false;
                }

            // Check if the floor is valid
            for (int e = -1; e <= 1; e++)
            {
                Tile t = Main.tile[i + e, j];
                bool validSlope = t.slope() == 0 || t.bottomSlope();
                if (!t.active() || !validSlope || !Main.tileSolid[t.type])
                    return false;
            }

            return true;
        }
    }
}
