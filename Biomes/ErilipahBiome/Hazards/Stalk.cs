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
    class Stalk : HazardTile
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
                TileObjectData.newTile.StyleMultiplier = 2;
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.LinkedAlternates = true;
                TileObjectData.newTile.AnchorAlternateTiles = new int[] { mod.TileType<Stalk>() };
                TileObjectData.newTile.AnchorValidTiles = new int[] 
                { mod.TileType<InfectedClump>(), mod.TileType<SpoiledClump>(), mod.TileType<TaintedBrick>(), mod.TileType<Vine>() };
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

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Helper.Invisible;
            return true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!Main.tile[i, j - 1].IsErilipahTile() && Main.tile[i, j + 1].type != Type)
            {
                WorldGen.KillTile(i, j, Main.rand.NextBool());
                WorldGen.TileFrame(i, j - 1);
                WorldGen.TileFrame(i + 1, j);
                WorldGen.TileFrame(i - 1, j);
            }
            resetFrame = false;
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return;

            Tile tile = Main.tile[i, j];
            Texture2D texture = ModContent.GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Crystalline_Glowmask");
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }
            Main.spriteBatch.Draw(
                texture, 
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, 
                new Rectangle(tile.frameX, tile.frameY + 2, 16, 16), Color.White * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (Main.netMode == 1 && tile.frameX % 36 != 0)
                return;

            if (tile.frameY == 54)
            {
                NPC.NewNPC(i * 16 + 8, j * 16 + 8, mod.NPCType<Bulb>(), ai1: i);
            }
            else if (Valid(i, j))
            { 
                bool endTile = Main.rand.Chance(0.12f);

                Tile vineEx = Main.tile[i, j + 1];
                vineEx.active(true);
                vineEx.type = Type;
                vineEx.frameX = (short)(Main.rand.Next(3) * 18);

                if (!endTile || tile.frameY == 0)
                    vineEx.frameY = Main.rand.NextBool() ? (short)18 : (short)36;
                else
                    vineEx.frameY = 54;
            }
        }

        bool Valid(int i, int j)
        {
            bool noRoof = !Collision.SolidTiles(i, i, j - 10, j);
            bool noWall = Main.tile[i, j - 1].wall == 0 && Main.tile[i, j - 1].wall == 0;
            return noRoof && noWall;
        }
    }
}
