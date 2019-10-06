using Erilipah.Biomes.ErilipahBiome.Tiles;
using Erilipah.Items.Crystalline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Stalk : HazardTile
    {
        public override string MapName => "Crystalline Stalk";
        public override int DustType => mod.DustType<CrystallineDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinateHeights = new[] { 16 };
                TileObjectData.newTile.LinkedAlternates = true;
                TileObjectData.newTile.AnchorAlternateTiles = new int[] { mod.TileType<Stalk>() };
                TileObjectData.newTile.AnchorValidTiles = new int[]
                { mod.TileType<InfectedClump>(), mod.TileType<SpoiledClump>(), mod.TileType<Stalk>() };
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
            drop = mod.ItemType<CrystallineTileItem>();
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tile = Main.tile[i, j];
            bool isBase = tile.frameX >= 54 && tile.frameY >= 90;

            if (!isBase && Main.tile[i, j + 1].type != Type)
            {
                WorldGen.KillTile(i, j, Main.rand.NextBool());
            }
            if (isBase)
            {
                int left = i - (tile.frameX - 54) / 18;

                if (!Main.tile[i, j + 1].IsErilipahTile())
                {
                    BreakBase(left, j);
                }

                if (!Main.tile[left, j].active() || !Main.tile[left + 1, j].active() || !Main.tile[left + 2, j].active())
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

        public override void RandomUpdate(int i, int j)
        {
            if (Main.netMode == 1)
                return;

            Tile tile = Main.tile[i, j];

            bool isTip = (tile.frameX == 54 && tile.frameY <= 18) || tile.frameY == 0;
            if (isTip)
                return;

            bool isBase = tile.frameX >= 54 && tile.frameY >= 90;
            bool isTop = tile.frameX >= 54 && tile.frameY <= 72;
            bool isStalk = (tile.frameX == 00 && tile.frameY >= 54) || (tile.frameX <= 36 && tile.frameY >= 72);

            if (isBase)
            {
                // Grow a tile in the middle of the base
                int middleX = i - (tile.frameX - 54) / 18 + 1;
                if (!Main.tile[middleX, j - 1].active())
                {
                    Tile above = Main.tile[middleX, j - 1];
                    above.type = Type;
                    above.active(true);
                    GetStalkFrame(out above.frameX, out above.frameY);
                }
            }
            else if (isStalk)
            {
                Tile above = Main.tile[i, j - 1];

                // 8% chance to start the tip of the cane. Also starts the tip of there's a tile that will prevent it
                // Otherwise, continue growing the stalk
                if (Main.rand.Chance(0.08f) || Main.tile[i, j - 6].active())
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

                if (!above.active())
                {
                    above.type = Type;
                    above.active(true);
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
            for (int e = -1; e <= 1; e++)
                for (int f = -7; f < 0; f++)
                {
                    Tile testing = Main.tile[i + e, j + f];
                    if (testing.active() || testing.wall > 0)
                        return false;
                }

            for (int e = -1; e <= 1; e++)
                if (!Main.tile[i + e, j].active())
                    return false;

            return true;
        }
    }
}
