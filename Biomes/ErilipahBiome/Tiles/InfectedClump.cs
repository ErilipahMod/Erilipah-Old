﻿using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    public class InfectedClump : ModTile
    {
        public static void Infect(int i, int j, Mod mod)
        {
            float chance = 0.01f;
            chance += 0.10f * Main.hardMode.ToInt();
            chance += 0.10f * NPC.downedPlantBoss.ToInt();
            chance += 0.15f * NPC.downedAncientCultist.ToInt();
            chance += 0.15f * NPC.downedMoonlord.ToInt();
            chance /= 2f;

            if (Main.rand.NextFloat() > chance)
                return;

            try
            {
                Vector2 tilePos = new Vector2(i, j) + Main.rand.NextVector2Circular(10, 10);
                Tile tile = Main.tile[(int)tilePos.X, (int)tilePos.Y];

                if (!Main.tileFrameImportant[tile.type] &&
                    WorldGen.SolidOrSlopedTile(tile) && !(
                    tile.type == TileID.LihzahrdBrick ||
                    tile.type == TileID.LihzahrdAltar ||
                    tile.type == TileType<InfectedClump>() ||
                    tile.type == TileType<SpoiledClump>() ||
                    tile.type == TileType<TaintedRubble>() ||
                    tile.type == TileType<TaintedBrick>() ||
                    tile.type == TileType<Items.Crystalline.CrystallineTileTile>()))
                {
                    bool brick = tile.type == TileID.GrayBrick || tile.type == TileID.RedBrick || tile.type == TileType<TaintedBrickSafe>();

                    bool organic = TileID.Sets.Mud[tile.type] || TileID.Sets.Snow[tile.type] || TileID.Sets.Conversion.Sand[tile.type] ||
                        tile.type == TileID.Slush || tile.type == TileID.Silt || tile.type == TileID.JungleGrass;
                    bool wood = tile.type == TileID.WoodBlock || tile.type == TileID.BorealWood || tile.type == TileID.RichMahogany ||
                        tile.type == TileID.PalmWood || tile.type == TileID.LivingWood || tile.type == TileID.DynastyWood ||
                        tile.type == TileID.SpookyWood || tile.type == TileID.Shadewood || tile.type == TileID.Ebonwood ||
                        tile.type == TileID.LeafBlock || tile.type == TileID.LivingMahogany || tile.type == TileID.LivingMahoganyLeaves;

                    if (brick)
                        tile.type = (ushort)mod.TileType("TaintedBrick");
                    else if (wood || organic)
                        tile.type = (ushort)mod.TileType("SpoiledWood");
                    else
                        tile.type = (ushort)mod.TileType("InfectedClump");

                    WorldGen.TileFrame((int)tilePos.X, (int)tilePos.Y);
                }

                if (tile.wall > 0 && !(
                    tile.wall == WallID.LihzahrdBrickUnsafe ||
                    tile.wall == (ushort)WallType<InfectedClumpWall>() ||
                    tile.wall == (ushort)WallType<TaintedBrick.TaintedBrickWall>()))
                {
                    bool woodWall = tile.wall == WallID.Wood || tile.wall == WallID.BorealWood || tile.wall == WallID.RichMaogany ||
                        tile.wall == WallID.PalmWood || tile.wall == WallID.LivingWood ||
                        tile.wall == WallID.SpookyWood || tile.wall == WallID.Shadewood || tile.wall == WallID.Ebonwood ||
                        tile.wall == WallID.LivingLeaf || tile.wall == WallID.LivingWood;

                    if (tile.wall == WallID.GrayBrick || tile.wall == WallID.RedBrick || tile.wall == WallType<TaintedBrickSafe.TaintedBrickWallSafe>())
                        tile.wall = (ushort)WallType<TaintedBrick.TaintedBrickWall>();
                    else
                        tile.wall = (ushort)WallType<InfectedClumpWall>();

                    WorldGen.SquareWallFrame((int)tilePos.X, (int)tilePos.Y);
                }
            }
            catch { }
        }

        public class InfectedClumpWall : ModWall
        {
            public override void SetDefaults()
            {
                drop = 0;
                dustType = DustID.PurpleCrystalShard;
                AddMapEntry(new Color(10, 8, 35, 155));

                soundType = 0;
                soundStyle = 0;
            }

            public override void RandomUpdate(int i, int j)
            {
                Infect(i, j, mod);
            }
            public override bool CanExplode(int i, int j)
            {
                return false;
            }

            public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
        }
        public class InfectedClumpItem : ModItem
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Infected Clump");
                Tooltip.SetDefault("'It pulses in perfect sync with the Erilipah'");
            }

            public override void SetDefaults()
            {
                item.CloneDefaults(ItemID.StoneBlock);
                item.width = 26;
                item.height = 20;
                item.createTile = TileType<InfectedClump>();
            }
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][TileType<TaintedBrick>()] = true;
            Main.tileMerge[Type][TileType<TaintedRubble>()] = true;
            Main.tileStone[Type] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = ItemType<InfectedClumpItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(2, 0, 25, 255));

            mineResist = 1.75f;
            minPick = 65;
            soundType = 21;
            soundStyle = 2;
        }

        public override void RandomUpdate(int i, int j)
        {
            Infect(i, j, mod);
        }

        public override bool CanExplode(int i, int j) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor.R = (byte)(drawColor.R * 0.75f);
            drawColor.G = (byte)(drawColor.G * 0.75f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;

        public override bool Drop(int i, int j)
        {
            Liquid.AddWater(i, j);
            return true;
        }

        public override void FloorVisuals(Player player)
        {
            if (Main.rand.NextFloat(System.Math.Abs(player.velocity.X)) < 0.5f)
                return;

            Dust dust = Main.dust[Dust.NewDust(
                player.position + new Vector2(0, player.height), player.width, 0, DustType<VoidParticle>(),
                player.direction * -0.5f, -0.8f)];
            dust.noGravity = true;
            dust.velocity /= 2f;
            dust.fadeIn = 1.15f;
        }
    }
    public class SpoiledClump : ModTile
    {
        public class SpoiledClumpItem : ModItem
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Spoiled Clump");
                Tooltip.SetDefault("'It pulses in perfect sync with the Erilipah'");
            }

            public override void SetDefaults()
            {
                item.CloneDefaults(ItemID.DirtBlock);
                item.width = 16;
                item.height = 16;
                item.createTile = TileType<SpoiledClump>();
            }
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][TileType<TaintedBrick>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = ItemType<SpoiledClumpItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(65, 0, 45, 255));

            mineResist = 1.65f;
            minPick = 65;
            soundType = 0;
            soundStyle = 2;
        }

        public override void RandomUpdate(int i, int j)
        {
            InfectedClump.Infect(i, j, mod);
        }

        public override bool CanExplode(int i, int j) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor.R = (byte)(drawColor.R * 0.75f);
            drawColor.G = (byte)(drawColor.G * 0.75f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;

        public override bool Drop(int i, int j)
        {
            Liquid.AddWater(i, j);
            Liquid.AddWater(i, j);
            return true;
        }
    }
    public class TaintedBrick : ModTile
    {
        public class TaintedBrickWall : ModWall
        {
            public class TaintedBrickWallItem : ModItem
            {
                public override void SetStaticDefaults()
                {
                    DisplayName.SetDefault("Tainted Brick Wall");
                    Tooltip.SetDefault("'Faded and worn, but still strong'");
                }

                public override void SetDefaults()
                {
                    item.CloneDefaults(ItemID.GrayBrickWall);
                    item.createWall = WallType<TaintedBrickSafe.TaintedBrickWallSafe>();
                    item.rare = 1;
                }
            }
            public override void SetDefaults()
            {
                drop = mod.ItemType("TaintedBrickWallItem");
                dustType = DustID.PurpleCrystalShard;
                AddMapEntry(new Color(15, 20, 45, 175));

                soundType = 0;
                soundStyle = 0;
            }

            public override void RandomUpdate(int i, int j)
            {
                InfectedClump.Infect(i, j, mod);
            }
            public override bool CanExplode(int i, int j)
            {
                return false;
            }

            public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
        }
        public class TaintedBrickItem : ModItem
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Tainted Brick");
                Tooltip.SetDefault("'Faded and worn, but still strong'");
            }

            public override void SetDefaults()
            {
                item.CloneDefaults(ItemID.GrayBrick);
                item.width = 16;
                item.height = 16;
                item.createTile = TileType<TaintedBrickSafe>();
                item.rare = 1;
            }
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[Type][TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][TileType<TaintedRubble>()] = true;
            Main.tileMerge[Type][TileType<TaintedBrickSafe>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = ItemType<TaintedBrickItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(15, 20, 65, 255));

            mineResist = 2.65f;
            minPick = 101;
            soundType = 21;
            soundStyle = 2;
        }

        public override void RandomUpdate(int i, int j)
        {
            InfectedClump.Infect(i, j, mod);
        }

        public override bool CanExplode(int i, int j) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor.R = (byte)(drawColor.R * 0.75f);
            drawColor.G = (byte)(drawColor.G * 0.75f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
    }
    public class TaintedBrickSafe : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "Erilipah/Biomes/ErilipahBiome/Tiles/TaintedBrick";
            return true;
        }
        public class TaintedBrickWallSafe : ModWall
        {
            public override bool Autoload(ref string name, ref string texture)
            {
                texture = "Erilipah/Biomes/ErilipahBiome/Tiles/TaintedBrickWall";
                return true;
            }
            public override void SetDefaults()
            {
                drop = mod.ItemType("TaintedBrickWallItem");
                dustType = DustID.PurpleCrystalShard;
                AddMapEntry(new Color(15, 20, 45, 175));

                soundType = 0;
                soundStyle = 0;
            }

            public override void RandomUpdate(int i, int j)
            {
                InfectedClump.Infect(i, j, mod);
            }
            public override bool CanExplode(int i, int j)
            {
                return false;
            }

            public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[Type][TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][TileType<TaintedRubble>()] = true;
            Main.tileMerge[Type][TileType<TaintedBrick>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = ItemType<TaintedBrick.TaintedBrickItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(15, 20, 65, 255));

            mineResist = 2.65f;
            minPick = 101;
            soundType = 21;
            soundStyle = 2;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor.R = (byte)(drawColor.R * 0.75f);
            drawColor.G = (byte)(drawColor.G * 0.75f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
    }
    public class TaintedRubble : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[Type][TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][TileType<TaintedBrick>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = ItemType<InfectedClump.InfectedClumpItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(30, 18, 35, 235));

            mineResist = 3f;
            minPick = int.MaxValue;
            soundType = 21;
            soundStyle = 2;
        }

        public override void RandomUpdate(int i, int j)
        {
            InfectedClump.Infect(i, j, mod);
        }

        public override bool CanExplode(int i, int j) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor.R = (byte)(drawColor.R * 0.75f);
            drawColor.G = (byte)(drawColor.G * 0.75f);
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
    }

    public class ErilipahGlobalTile : GlobalTile
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            for (int type = 0; type < TileID.Count; type++)
            {
                if (Main.tileMergeDirt[type])
                    Main.tileMerge[type][TileType<SpoiledClump>()] = true;

                if (Main.tileMerge[type][TileID.Stone])
                {
                    Main.tileMerge[type][TileType<InfectedClump>()] = true;
                }

                if (Main.tileMerge[type][TileID.GrayBrick] || Main.tileMerge[type][TileID.Stone])
                {
                    Main.tileMerge[type][TileType<TaintedBrick>()] = true;
                    Main.tileMerge[type][TileType<TaintedRubble>()] = true;
                }
            }
        }

        public override bool PreHitWire(int i, int j, int type)
        {
            return Main.tile[i, j].type != TileType<TaintedBrick>();
        }
    }
}
