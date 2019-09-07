using Erilipah.NPCs.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

                if (WorldGen.SolidOrSlopedTile(tile) && !(
                    tile.type == TileID.LihzahrdBrick ||
                    tile.type == TileID.LihzahrdAltar ||
                    tile.type == mod.TileType<InfectedClump>() ||
                    tile.type == mod.TileType<SpoiledClump>() ||
                    tile.type == mod.TileType<TaintedRubble>() ||
                    tile.type == mod.TileType<TaintedBrick>() ||
                    tile.type == mod.TileType<Items.Crystalline.CrystallineTileTile>()))
                {
                    bool brick = tile.type == TileID.GrayBrick || tile.type == TileID.RedBrick;

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
                    tile.wall == (ushort)mod.WallType<InfectedClumpWall>() ||
                    tile.wall == (ushort)mod.WallType<SpoiledClump.SpoiledClumpWall>() ||
                    tile.wall == (ushort)mod.WallType<TaintedBrick.TaintedBrickWall>()))
                {
                    bool woodWall = tile.wall == WallID.Wood || tile.wall == WallID.BorealWood || tile.wall == WallID.RichMaogany ||
                        tile.wall == WallID.PalmWood || tile.wall == WallID.LivingWood ||
                        tile.wall == WallID.SpookyWood || tile.wall == WallID.Shadewood || tile.wall == WallID.Ebonwood ||
                        tile.wall == WallID.LivingLeaf || tile.wall == WallID.LivingWood;

                    if (woodWall || tile.wall == WallID.MudUnsafe || tile.wall == WallID.SnowWallUnsafe)
                        tile.wall = (ushort)mod.WallType<SpoiledClump.SpoiledClumpWall>();
                    else if (tile.wall == WallID.GrayBrick || tile.wall == WallID.RedBrick)
                        tile.wall = (ushort)mod.WallType<TaintedBrick.TaintedBrickWall>();
                    else
                        tile.wall = (ushort)mod.WallType<InfectedClumpWall>();

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
                item.createTile = mod.TileType<InfectedClump>();
            }
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][mod.TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][mod.TileType<TaintedBrick>()] = true;
            Main.tileMerge[Type][mod.TileType<TaintedRubble>()] = true;
            Main.tileStone[Type] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = mod.ItemType<InfectedClumpItem>();

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
            if (Main.tile[i, j].liquid + Main.tile[i, j - 1].liquid < 200)
                Main.tile[i, j].liquid += 20;
            return true;
        }

        public override void FloorVisuals(Player player)
        {
            Dust dust = Main.dust[Dust.NewDust(
                player.position + new Vector2(0, player.height), player.width, 0, mod.DustType<VoidParticle>(),
                player.direction * -0.5f, -0.8f)];
            dust.noGravity = true;
            dust.fadeIn = 0.8f;
        }
    }
    public class SpoiledClump : ModTile
    {
        public class SpoiledClumpWall : ModWall
        {
            public override void SetDefaults()
            {
                drop = 0;
                dustType = DustID.PurpleCrystalShard;
                AddMapEntry(new Color(65, 0, 45, 200));

                soundType = 0;
                soundStyle = 0;
            }

            public override void RandomUpdate(int i, int j)
            {
                InfectedClump.Infect(i, j, mod);
            }
            public override bool CanExplode(int i, int j)
            {
                return true;
            }

            public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 3;
        }
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
                item.createTile = mod.TileType<SpoiledClump>();
            }
        }


        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][mod.TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][mod.TileType<TaintedBrick>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = mod.ItemType<SpoiledClumpItem>();

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
            if (Main.tile[i, j].liquid < 200)
                Main.tile[i, j].liquid += 35;
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
                    item.createWall = mod.WallType<TaintedBrickWall>();
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
                item.createTile = mod.TileType<TaintedBrick>();
                item.rare = 1;
            }
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[Type][mod.TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][mod.TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][mod.TileType<TaintedRubble>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = mod.ItemType<TaintedBrickItem>();

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
    public class TaintedRubble : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;

            Main.tileMerge[Type][mod.TileType<InfectedClump>()] = true;
            Main.tileMerge[Type][mod.TileType<SpoiledClump>()] = true;
            Main.tileMerge[Type][mod.TileType<TaintedBrick>()] = true;

            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;

            dustType = DustID.PurpleCrystalShard;
            drop = mod.ItemType<InfectedClump.InfectedClumpItem>();

            //ModTranslation name = CreateMapEntryName();
            //name.SetDefault("Crystalline Shards");
            AddMapEntry(new Color(30, 18, 35, 235));

            mineResist = 3f;
            minPick = 201;
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
                    Main.tileMerge[type][mod.TileType<SpoiledClump>()] = true;

                if (Main.tileMerge[type][TileID.Stone])
                    Main.tileMerge[type][mod.TileType<InfectedClump>()] = true;

                if (Main.tileMerge[type][TileID.GrayBrick] || Main.tileMerge[type][TileID.Stone])
                {
                    Main.tileMerge[type][mod.TileType<TaintedBrick>()] = true;
                    Main.tileMerge[type][mod.TileType<TaintedRubble>()] = true;
                }
            }
        }

        public override bool PreHitWire(int i, int j, int type)
        {
            return Main.tile[i, j].type != mod.TileType<TaintedBrick>();
        }
    }
}
