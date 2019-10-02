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
    class Vine : HazardTile
    {
        public override string MapName => "Cursed Vine";
        public override int DustType => mod.DustType<CrystallineDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinateHeights = new[] { 16 };
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.LinkedAlternates = true;
                TileObjectData.newTile.AnchorAlternateTiles = new int[] { mod.TileType<Vine>() };
                TileObjectData.newTile.AnchorValidTiles = new int[] 
                { mod.TileType<InfectedClump>(), mod.TileType<SpoiledClump>(), mod.TileType<Vine>() };
                TileObjectData.newTile.AnchorTop = new AnchorData(
                    AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
                TileObjectData.newTile.AnchorBottom = AnchorData.Empty;

                return TileObjectData.newTile;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Main.tileCut[Type] = true;
            soundType = 3;
            soundStyle = 13;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = Main.rand.NextBool() ? mod.DustType<CrystallineDust>() : mod.DustType<FlowerDust>();
            return true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!Main.tile[i, j - 1].IsErilipahTile() && Main.tile[i, j - 1].type != Type)
            {
                WorldGen.KillTile(i, j, Main.rand.NextBool());
                WorldGen.TileFrame(i, j + 1);
            }
            resetFrame = false;
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D texture = ModContent.GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Vine_Glowmask");
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }

            Color color = Lighting.GetColor(i, j) * 4;
            Main.spriteBatch.Draw(
                texture, 
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, 
                new Rectangle(tile.frameX, tile.frameY + 2, 16, 16), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (Main.netMode == 1)
                return;

            bool anyBulb = Main.npc.Any(x => x.active && x.type == mod.NPCType<Bulb>() && x.ai[1] == i);
            if (tile.frameY == 54 && !anyBulb)
            {
                //FOR DEBUG
                //Main.LocalPlayer.position.X = i * 16;
                //Main.LocalPlayer.position.Y = j * 16;
                NPC.NewNPC(i * 16 + 8, j * 16 + 16, mod.NPCType<Bulb>(), ai1: i);
            }
            else if (!Main.tile[i, j + 1].active() && !Main.tile[i, j + 2].active())
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
    }

    public class Bulb : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 50;
            npc.defense = 500;
            npc.knockBackResist = 0;
            npc.noGravity = true;
            
            npc.aiStyle = 0;

            npc.width = 20;
            npc.height = 26;

            npc.noTileCollide = true;
            npc.timeLeft = 90;

            npc.damage = 0;
        }

        public override void DrawEffects(ref Color drawColor)
        {
            drawColor *= 2.5f;
        }

        public override void AI()
        {
            npc.ai[2] += 0.001f;
            if (npc.ai[2] > 0.15f)
                npc.ai[2] = -0.15f;

            float s = 0.925f + Math.Abs(npc.ai[2]);
            npc.scale = s;
            npc.timeLeft = 2;
            npc.velocity = Vector2.Zero;

            Lighting.AddLight(npc.Center, 1f * s, 0.8f * s, 1f * s);

            if (Main.rand.NextBool(10))
            {
                int dustInd = Dust.NewDust(npc.position, 20, 26, mod.DustType<FlowerDust>());

                Dust dust = Main.dust[dustInd];
                dust.noGravity = false;
                dust.velocity = Vector2.Zero;
                dust.customData = 0.1f;
            }
        }
    }
}
