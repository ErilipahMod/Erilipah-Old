using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Erilipah.Biomes.ErilipahBiome.Tiles;
using Terraria.Enums;
using Erilipah.NPCs.ErilipahBiome;
using Erilipah.Items.Crystalline;

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
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
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
            if (!Main.tile[i, j + 1].IsErilipahTile() && Main.tile[i, j + 1].type != Type)
            {
                WorldGen.KillTile(i, j, Main.rand.NextBool());
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
            const byte min = 100;

            if (drawColor.R < min)
                drawColor.R = min;
            if (drawColor.G < min)
                drawColor.G = min;
            if (drawColor.B < min)
                drawColor.B = min;
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

            bool isBase  =  tile.frameX >= 54 && tile.frameY >= 90;
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
            bool noRoof = !Collision.SolidTiles(i, i, j - 8, j - 1) && !Main.tile[i, j - 1].active();
            bool isBase = WorldGen.SolidTile(Main.tile[i, j + 1]) && WorldGen.SolidTile(Main.tile[i - 1, j + 1]) && WorldGen.SolidTile(Main.tile[i + 1, j + 1]);
            bool noObstruction = !Main.tile[i - 1, j].active() && !Main.tile[i, j].active() && !Main.tile[i + 1, j].active();

            return noRoof && noObstruction && isBase;
        }
    }
}
