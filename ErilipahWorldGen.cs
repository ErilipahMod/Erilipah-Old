using Erilipah.Biomes.ErilipahBiome.Hazards;
using Erilipah.Biomes.ErilipahBiome.Tiles;
using Erilipah.Items.Crystalline;
using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace Erilipah
{
    public partial class ErilipahWorld : ModWorld
    {
        /* int JRprogress = (int)(Main.dayLength / 1.5);
        void JunkRain()
        {
            JRprogress--;
            if (JRprogress <= 0)
            {
                Main.NewText("Junk has stopped raining from the sky.", ColorHelper.EventColor);
                JRprogress = (int)(Main.dayLength / 1.5);
                junkIsRaining = false;
            }
            if (Main.rand.NextBool(140))
            {
                //random player on the map (it will find one)
                Player plr = Main.player[Player.FindClosest(
                    new Vector2(Main.rand.Next(1, Main.maxTilesX), Main.rand.Next(1, Main.maxTilesY)),
                    Main.maxTilesX * 2, Main.maxTilesY * 2)];

                //projectile
                Projectile proj = Main.projectile[
                EntityHelper.FireAtTarget(new Vector2(plr.position.X + Main.rand.Next(-500, 501),
                    plr.position.Y - Main.rand.Next(Main.screenHeight, Main.screenHeight + 201)),
                    new Vector2(plr.position.X + Main.rand.Next(-500, 501),
                        plr.position.Y + Main.rand.Next(-50, 51)),
                    Main.rand.Next(Main.maxProjectileTypes),
                    plr.statDefense + 15, Main.rand.NextFloat(4, 17),
                    3)];

                if (proj.sentry || proj.minion)
                    proj.Kill();
                proj.friendly = false;
                proj.hostile = true;
                proj.tileCollide = plr.position.Y <= Main.worldSurface + 50;
                proj.timeLeft = System.Math.Min(proj.timeLeft, 1000);
            }
        }*/

        private int I(string s) => mod.ItemType(s);
        private ushort T(string n) => (ushort)mod.TileType(n);
        private static int GetHighestY(int x, int width, params int[] types)
        {
            int toReturn = 200;

            if (width > 0)
                for (int i = x; i < x + width; i++) // Increase/decrease according to if width is pos/neg
                {
                    int y = GetY(i, toReturn, types);
                    if (y > toReturn)
                        toReturn = y;

                    y = GetY(i, toReturn + 8, types);
                    if (y > toReturn + 8)
                        toReturn = y;
                }

            else
                for (int i = x; i > x + width; i--) // Increase/decrease according to if width is pos/neg
                {
                    int y = GetY(i, toReturn, types);
                    if (y > toReturn)
                        toReturn = y;

                    y = GetY(i, toReturn + 8, types);
                    if (y > toReturn + 8)
                        toReturn = y;
                }

            return toReturn;
        }
        private static int GetY(int x, int y = 350, params int[] types)
        {
            for (; y < Main.maxTilesY; y++)
            {
                // Go downwards, looking for one
                if (WorldGen.SolidOrSlopedTile(Main.tile[x, y]) && (
                    types.Length == 0 || types.Any(t => Main.tile[x, y] != null && t == Main.tile[x, y].type)))
                    break;
            }

            return y;
        }

        public override void ModifyHardmodeTasks(List<GenPass> list)
        {
            // Place one Erilipah block randomly in the world
            list.Add(new PassLegacy("Erilipah Seed", seed =>
            {
                int x = WorldGen.genRand.Next(Main.maxTilesX);
                int y = WorldGen.genRand.Next((int)(Main.maxTilesY * 0.6), Main.maxTilesY);

                // Keep searching if the current tile isn't stone
                while (Main.tile[x, y].type != TileID.Stone)
                {
                    x = WorldGen.genRand.Next(Main.maxTilesX); // Anywhere X
                    y = WorldGen.genRand.Next((int)WorldGen.rockLayerHigh, Main.maxTilesY - 300); // Below high caverns and above hell
                }
                WorldGen.TileRunner(x, y, 9, 4, T("InfectedClump"));
            }));
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            // Generate the Erilipah biome
            int index = tasks.FindIndex(genpass => genpass.Name == "Corruption") + 1;
            tasks.Insert(index, new PassLegacy("[Erilipah] Erilipah Biome", ErilipahBiomeGen));

            // Add the lost city chasm
            index = tasks.FindIndex(genpass => genpass.Name == "Smooth World") - 1;
            tasks.Insert(index, new PassLegacy("[Erilipah] Massive underground chasm", LostChasmGen));

            tasks.Insert(index + 1, new PassLegacy("[Erilipah] Crystalline", CrystallineOre));

            index = tasks.FindIndex(genpass => genpass.Name == "Final Cleanup");
            tasks.Insert(index, new PassLegacy("[Erilipah] The Lost City", LostCityGen));

            index = tasks.FindIndex(genpass => genpass.Name == "Shinies");
            tasks.Insert(index, new PassLegacy("[Erilipah] Sacracite", SacraciteOre));

            tasks.Add(new PassLegacy("[Erilipah] Hazards", HazardGen));
        }

        private bool GenLeft => WorldGen.dungeonX > Main.maxTilesX / 2;
        private int ChasmBottomY;
        private int BiomeCenterX
        {
            get
            {
                const int offset = 625;
                return GenLeft ? offset : Main.maxTilesX - offset;
            }
        }
        private Rectangle Chasm = new Rectangle();

        private void ErilipahBiomeGen(GenerationProgress progress)
        {
            progress.Message = "The Erilipah";
            progress.Set(0f);

            const float rateChangeMinLava = -7.25f;
            const float rateChangeMin = -5.0f;
            const float rateChangeMax = +5.2f;

            float change1 = Main.maxTilesX * 0.035f;
            float change2 = Main.maxTilesX * 0.035f;
            int j = 300;

            // Go downwards; stop before Hell
            while (j++ < Main.maxTilesY - 220)
            {
                // For a row
                for (int i = BiomeCenterX - (int)change1; i < BiomeCenterX + (int)change2; i++)
                {
                    // Make the block  an Erilipian block
                    Infect(i, j);
                    if (Main.tile[i, j].type == 21)
                        Main.tile[i, j].type = (ushort)TileType<ErilipahChest>();
                }

                bool atLava = j > Main.maxTilesY * 0.6;
                if (atLava)
                {
                    change1 += WorldGen.genRand.NextFloat(rateChangeMinLava, rateChangeMax - 1f);
                    change2 += WorldGen.genRand.NextFloat(rateChangeMinLava, rateChangeMax - 1f);
                }
                else
                {
                    change1 += WorldGen.genRand.NextFloat(rateChangeMin, rateChangeMax);
                    change2 += WorldGen.genRand.NextFloat(rateChangeMin, rateChangeMax);
                }
            }
            progress.Set(0.5f);

            PitGen();

            // Distance from the center
            const int outerDistance = 60;
            const int innerDistance = 35;

            // Left spikes
            MakeSpike(BiomeCenterX - outerDistance, -1, 15, 0.175f);
            MakeSpike(BiomeCenterX - innerDistance, -1, 18, 0.225f);
            // Right spikes
            MakeSpike(BiomeCenterX + innerDistance, +1, 18, 0.225f);
            MakeSpike(BiomeCenterX + outerDistance, +1, 15, 0.175f);

            void MakeSpike(int x, int dir, int width, float decWidthChance)
            {
                int y = GetHighestY(x, width * dir, T("InfectedClump"), T("SpoiledClump")) + 8;

                int iterations = 0;
                WorldGen.TileRunner(x + width / 2, y + 14, 6, 5, (ushort)TileType<CrystallineTileTile>());

                for (int i = 0; i < width - 1; i++)
                {
                    WorldGen.PlaceTile(x - i * dir + 1, y + 1, T("InfectedClump"), forced: true);
                }
                for (int i = 0; i < width - 2; i++)
                {
                    WorldGen.PlaceTile(x - i * dir + 2, y + 2, T("InfectedClump"), forced: true);
                }
                while (width > 0)
                {
                    for (int i = 0; i < width; i++)
                    {
                        WorldGen.PlaceTile(x - i * dir, y, T("InfectedClump"), forced: true);
                    }

                    if (WorldGen.genRand.NextFloat() < decWidthChance && iterations > 8)
                    {
                        width--;
                    }

                    if (WorldGen.genRand.NextFloat() < decWidthChance / 1.5)
                    {
                        x -= 1 * dir;
                    }
                    iterations++;
                    y--;
                }
            }
            progress.Set(0.75f);
        }
        private void PitGen()
        {
            int halfWidth = 13;
            const int depthDecayPoint = 70;
            const float widenChance = 0.2f;
            const float oreChance = 0.1f;

            int spotX = BiomeCenterX;
            int top = GetY(BiomeCenterX, 200, T("InfectedClump")) - 13;
            int j = top;
            while (halfWidth > 0)
            {
                j++; // Go down
                for (int i = spotX - halfWidth - WorldGen.genRand.Next(-1, 3); i < spotX + halfWidth + WorldGen.genRand.Next(-1, 3); i++)
                {
                    // Fill in the row with air
                    Tile tile = Main.tile[i, j];
                    if (tile == null) tile = new Tile();
                    tile.active(false);

                    if ((j - top) >= 20 || WorldGen.genRand.NextBool(Math.Max(1, 8 - (j - top))))
                        WorldGen.PlaceWall(i, j, (ushort)WallType<InfectedClump.InfectedClumpWall>());
                }

                // If {chance}, increase width of row
                if (WorldGen.genRand.NextFloat() < widenChance && j - top > 10)
                    halfWidth++;

                if (WorldGen.genRand.NextFloat() < oreChance)
                {
                    int x = WorldGen.genRand.NextBool() ? spotX - 14 : spotX + halfWidth * 2 + 14;
                    WorldGen.TileRunner(x, j, 4, 4, (ushort)TileType<CrystallineTileTile>(), false, 0, 0);
                }

                // If {chance*0.33}, randomize X pos of row a little
                if (WorldGen.genRand.NextFloat() < widenChance * 0.33f)
                    spotX += WorldGen.genRand.Next(-2, 3);

                int pastPoint = j - (top + depthDecayPoint); // How far past the decay point the chasm is
                if (pastPoint > 0 && WorldGen.genRand.NextFloat() / (0.2 * pastPoint) < widenChance)
                {
                    halfWidth--;
                }
            }

            ChasmBottomY = j;

            // When done, place the altar at the very bottom
            for (int i = 6; i < 10; i++)
            {
                WorldGen.PlaceTile(spotX, j - i, T("TaintedRubble"), forced: true);
                WorldGen.PlaceTile(spotX + 1, j - i, T("TaintedRubble"), forced: true);
            }

            // Blocks to either side of the crystal
            WorldGen.PlaceTile(spotX - 1, j - 9, T("TaintedRubble"), forced: true);
            WorldGen.PlaceTile(spotX + 2, j - 9, T("TaintedRubble"), forced: true);
            WorldGen.PlaceTile(spotX + 3, j - 9, T("TaintedRubble"), forced: true);

            // Supporting blocks for the side blocks
            WorldGen.PlaceTile(spotX - 1, j - 8, T("TaintedRubble"), forced: true);
            WorldGen.SlopeTile(spotX - 1, j - 8, 4);
            WorldGen.PlaceTile(spotX + 2, j - 8, T("TaintedRubble"), forced: true);
            WorldGen.PlaceTile(spotX + 2, j - 7, T("TaintedRubble"), forced: true);
            WorldGen.SlopeTile(spotX + 2, j - 7, 3);
            WorldGen.PlaceTile(spotX + 3, j - 8, T("TaintedRubble"), forced: true);
            WorldGen.SlopeTile(spotX + 3, j - 8, 3);

            WorldGen.PlaceTile(spotX + 1, j - 5, T("TaintedRubble"), forced: true);
            WorldGen.SlopeTile(spotX, j - 4, 4);

            int altarX = spotX + 1;
            int altarY = j - 10;

            WorldGen.Place3x2(altarX, altarY, T("Altar"));
            AltarPosition = new Vector2(altarX * 16 + 8, altarY * 16);
        }

        private void LostChasmGen(GenerationProgress progress)
        {
            progress.Message = "[Erilipah] Massive underground chasm";
            progress.Set(0f);

            Chasm.Width = (int)(0.07 * Main.maxTilesX);
            Chasm.Height = (int)(0.1215 * Main.maxTilesY);
            Chasm.X = BiomeCenterX - Chasm.Width / 2;
            Chasm.Y = ChasmBottomY + 95;

            int chasmX = Chasm.X;
            float chasmHeight = 1;

            int variation1 = 0;
            int variation2 = 0;

            int trueTop = int.MaxValue;

            // Make the main cavern
            while (chasmX < Chasm.X + Chasm.Width && chasmHeight > 0)
            {
                chasmX++;
                variation1 += WorldGen.genRand.Next(-4, 5);
                variation2 += WorldGen.genRand.Next(-1, 2);

                float proportionFromCenter = (Chasm.Width / 2f - Math.Abs(Chasm.Center.X - chasmX)) / (Chasm.Width / 2f);
                proportionFromCenter *= 1.5f;
                chasmHeight = MathHelper.SmoothStep(0, Chasm.Height, MathHelper.Clamp(proportionFromCenter, 0, 1));

                int chasmRoof = Chasm.Bottom - (int)chasmHeight + variation1;
                int chasmBase = Chasm.Bottom + variation2;

                for (int j = chasmRoof; j < chasmBase; j++)
                {
                    Tile tile = Main.tile[chasmX, j];

                    if (tile.type == TileID.LihzahrdBrick)
                        continue;

                    tile.active(false);
                    tile.wall = 0;
                }
                for (int j = chasmBase; j < chasmBase + WorldGen.genRand.Next(11, 15); j++)
                {
                    Tile tile = Main.tile[chasmX, j];

                    if (tile.type == TileID.LihzahrdBrick)
                        continue;

                    WorldGen.PlaceTile(chasmX, j, (ushort)TileType<TaintedRubble>(), forced: true);
                    tile.wall = 0;
                }

                if (trueTop > chasmRoof + 20)
                    trueTop = chasmRoof + 20;

                progress.Set(Chasm.Right - Chasm.X / (float)Chasm.Width);
            }
            Chasm.Width = chasmX - Chasm.X;
            ChasmPosition = new Vector2((Chasm.X + Chasm.Width / 2) * 16, trueTop * 16);
        }
        private void LostCityGen(GenerationProgress progress)
        {
            progress.Message = "[Erilipah] The Lost City";
            progress.Set(0f);

            // Delete any railways
            for (int i = Chasm.Left; i < Chasm.Right; i++)
            {
                for (int j = Chasm.Top; j < Chasm.Bottom; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile.type == TileID.MinecartTrack)
                        tile.active(false);
                }
            }

            // Make the houses and altar
            for (int passTheBeer = Chasm.Left + 5; passTheBeer < Chasm.Right - 25;)
            {
                int y = GetY(passTheBeer, Chasm.Top, TileType<TaintedRubble>()) + 2;
                float amount = Math.Abs(passTheBeer - Chasm.Center.X) / (Chasm.Width / 2f);

                float chasmHeight = 0.1215f * Main.maxTilesY;
                float aNumFloors = (1f - amount) * (chasmHeight / 15f);
                int numFloors = (int)Math.Ceiling(aNumFloors);

                MakeHouse(
                    passTheBeer,
                    passTheBeer + Main.rand.Next(14, 20),
                    y,
                    WorldGen.genRand.Next(12, 17),
                    numFloors + WorldGen.genRand.Next(3),
                    ref passTheBeer);

                progress.Set((passTheBeer - Chasm.Left) / (float)(Chasm.Right - Chasm.Left));
            }
        }
        private void MakeHouse(int leftX, int rightX, int baseY, int floorHeight, int floors, ref int passTheBeer)
        {
            const float chestChance = 0.085f;
            const float potChance = 0.75f;
            const float pustuleChance = 0.12f;
            const int potsPerFloor = 6;
            const float bannerChance = 0.175f;
            const float paintingChance = 0.35f;

            const int statueStyles = 2;
            const float statueChance = 0.35f;

            ushort type = T("TaintedBrick");
            ushort wall = (ushort)WallType<TaintedBrick.TaintedBrickWall>();

            if (rightX - leftX < 14)
                rightX = leftX + 14;
            passTheBeer = rightX + 8;
            int roofY = baseY - floors * floorHeight;

            // Clear the house area, clearing left-right columns of down-up
            for (int i = leftX + 1; i <= rightX - 1; i++)
            {
                for (int j = baseY - 1; j >= roofY + 1; j--)
                {
                    Main.tile[i, j].active(false);

                    WorldGen.PlaceWall(i, j, wall);
                    Main.tile[i, j].wall = wall;

                    if (WorldGen.genRand.Chance(0.1f))
                    {
                        Liquid.AddWater(i, j);
                    }
                }
            }

            // Walls. Go upwards from the chasm floor and make the walls
            for (int j = GetY(leftX, baseY) + 2; j >= roofY; j--)
            {
                WorldGen.PlaceTile(leftX, j, type, forced: true);
                WorldGen.PlaceTile(leftX - 1, j, type, forced: true);
            }

            for (int j = GetY(rightX, baseY) + 2; j >= roofY; j--)
            {
                WorldGen.PlaceTile(rightX, j, type, forced: true);
                WorldGen.PlaceTile(rightX + 1, j, type, forced: true);
            }

            // Floors. Go rightwards from left side and make each floor
            for (int floor = baseY; floor >= roofY; floor -= floorHeight)
            {
                int insertionPoint = WorldGen.genRand.Next(leftX + 1, rightX - 1);
                for (int i = leftX + 1; i < rightX; i++)
                {
                    if (!(floor == baseY || floor == roofY) &&
                        i - insertionPoint >= -1 && i - insertionPoint <= 1)
                    {
                        WorldGen.PlaceTile(i, floor, TileID.Platforms, forced: true, style: 6);
                        continue;
                    }

                    WorldGen.PlaceTile(i, floor, type, forced: true);
                }

                // Extra layer of thicc at the bottom ;)
                for (int i = leftX; i < rightX; i++)
                {
                    WorldGen.PlaceTile(i, baseY + 1, type, forced: true);
                }

                // Statues
                for (int i = 0; i < 2; i++)
                {
                    if (WorldGen.genRand.Chance(statueChance))
                    {
                        int x = WorldGen.genRand.Next(leftX + 1, rightX - 1);
                        WorldGen.Place2xX(x, floor - 1, (ushort)TileType<SoulStatue>(), WorldGen.genRand.Next(statueStyles));
                    }
                }

                // Banners
                if (floor != baseY && WorldGen.genRand.Chance(bannerChance))
                {
                    for (int banI = 0; banI < 2; banI++)
                    {
                        for (int banJ = 0; banJ < 3; banJ++)
                        {
                            Main.tile[leftX + 2 + banI, floor + 1 + banJ].active(true);
                            Main.tile[leftX + 2 + banI, floor + 1 + banJ].type = (ushort)TileType<CityBanner>();
                            Main.tile[leftX + 2 + banI, floor + 1 + banJ].frameX = (short)(banI * 18);
                            Main.tile[leftX + 2 + banI, floor + 1 + banJ].frameY = (short)(banJ * 18);

                            Main.tile[rightX - 3 + banI, floor + 1 + banJ].active(true);
                            Main.tile[rightX - 3 + banI, floor + 1 + banJ].type = (ushort)TileType<CityBanner>();
                            Main.tile[rightX - 3 + banI, floor + 1 + banJ].frameX = (short)(banI * 18);
                            Main.tile[rightX - 3 + banI, floor + 1 + banJ].frameY = (short)(banJ * 18);
                        }
                    }
                }

                // Paintings
                if (floor != baseY && WorldGen.genRand.Chance(paintingChance))
                {
                    if (WorldGen.genRand.NextBool())
                        WorldGen.Place6x4Wall(
                            WorldGen.genRand.Next(leftX + 3, rightX - 3),
                            WorldGen.genRand.Next(floor + 3, floor + 8),
                            TileID.Painting6X4, WorldGen.genRand.Next(17)
                            );
                    else
                        WorldGen.Place3x3Wall(
                            WorldGen.genRand.Next(leftX + 3, rightX - 3),
                            WorldGen.genRand.Next(floor + 3, floor + 8),
                            TileID.Painting3X3, WorldGen.genRand.Next(13, 21)
                            );
                }

                // Torches
                if (floor != baseY)
                {
                    WorldGen.PlaceTile(leftX + 2, floor + 2, TileType<ArkenTorchTile>());
                    WorldGen.PlaceTile(rightX - 2, floor + 2, TileType<ArkenTorchTile>());

                    Main.tile[leftX + 2, floor + 2].frameX += 66;
                    Main.tile[rightX - 2, floor + 2].frameX += 66;
                }

                // Don't do any of this on the roof
                if (floor == roofY) continue;

                // Pots
                for (int i = 0; i < potsPerFloor; i++)
                {
                    if (WorldGen.genRand.Chance(potChance))
                    {
                        int x = WorldGen.genRand.Next(leftX + 1, rightX - 1);
                        if (WorldGen.genRand.Chance(pustuleChance))
                            WorldGen.Place2x2(x, floor - 1, (ushort)TileType<ErilipahPot>(), 0);
                        else
                            WorldGen.PlacePot(x, floor - 1, 28, WorldGen.genRand.NextBool() ?
                                (ushort)WorldGen.genRand.Next(13, 16) : (ushort)WorldGen.genRand.Next(31, 34));
                    }
                }

                // Chests
                if (WorldGen.genRand.Chance(chestChance))
                {
                    int x = WorldGen.genRand.Next(leftX + 4, rightX - 4);
                    int index = WorldGen.PlaceChest(x, floor - 1, (ushort)TileType<ErilipahChest>(), false, 1);
                    if (index <= 0)
                        continue;

                    Chest chest = Main.chest[index];
                    MakeChest(chest.item);
                }
            }

            // 70% chance to extend to another house out to the right
            int parentWidth = rightX - leftX;
            if (rightX < Chasm.Right - parentWidth * 1.25f && roofY > Chasm.Top && baseY < Chasm.Bottom + 35 && WorldGen.genRand.Chance(0.70f))
            {
                int floorToExtendY = baseY - floorHeight * WorldGen.genRand.Next(0, floors);
                int extensionLength = (int)WorldGen.genRand.NextFloat(parentWidth * 0.75f, parentWidth * 1.25f);

                int newBase = GetY(rightX + extensionLength, floorToExtendY, TileType<TaintedRubble>());
                if (newBase > Chasm.Bottom + 35)
                    newBase = Chasm.Bottom + 35;

                if (newBase % floorHeight != 0)
                    newBase -= (newBase % floorHeight); // Round it to the nearest floor
                newBase = Math.Max(newBase, floorToExtendY + floorHeight);

                int minFloors = (int)Math.Ceiling(Math.Abs(newBase - floorToExtendY) / (float)floorHeight); // Ensure that it lines up and has enough floors to do so
                minFloors += 2;

                float amount = Math.Abs(passTheBeer + extensionLength - Chasm.Center.X) / (Chasm.Width / 2f);
                int numFloors = (int)MathHelper.Lerp(1, 11, 1f - amount);

                int newFloors = Math.Max(minFloors, numFloors + WorldGen.genRand.Next(3));

                MakeHouse(
                    rightX + extensionLength + 1,
                    rightX + parentWidth + WorldGen.genRand.Next(-3, 5) + extensionLength + 1,
                    newBase,
                    floorHeight,
                    newFloors,
                    ref passTheBeer);

                int extensions = 1;
                int withinFloors = Math.Min(floors, newFloors);
                if (withinFloors > 2 && WorldGen.genRand.Chance(0.5f)) extensions++;
                if (withinFloors > 4 && WorldGen.genRand.Chance(0.6f)) extensions++;
                if (withinFloors > 7 && WorldGen.genRand.Chance(0.7f)) extensions++;
                for (int Y = 0; Y < extensions; Y++)
                {
                    if (Y > 0)
                    {
                        int lastExtension = floorToExtendY;
                        while (lastExtension == floorToExtendY)
                        {   // Ensure that it isn't the same goddamn floor
                            floorToExtendY = baseY - floorHeight * WorldGen.genRand.Next(0, withinFloors);
                        }
                    }

                    // Clear the passage & place background walls
                    for (int i = rightX + 2; i < rightX + extensionLength; i++)
                    {
                        for (int j = floorToExtendY - 1; j > floorToExtendY - floorHeight; j--)
                        {
                            Main.tile[i, j].active(false);
                            Main.tile[i, j].wall = wall;
                        }
                    }

                    // Make the passage to the other house
                    for (int i = rightX; i < rightX + extensionLength; i++)
                    {
                        WorldGen.PlaceTile(i, floorToExtendY - floorHeight - 1, type, forced: true);
                        WorldGen.PlaceTile(i, floorToExtendY - floorHeight, type, forced: true);
                        WorldGen.PlaceTile(i, floorToExtendY, type, forced: true);
                        WorldGen.PlaceTile(i, floorToExtendY + 1, type, forced: true);
                    }

                    #region Lil details
                    // Supports below
                    WorldGen.PlaceTile(rightX + 2, floorToExtendY + 2, type, forced: true);
                    WorldGen.PlaceTile(rightX + 3, floorToExtendY + 2, type, forced: true);
                    WorldGen.SlopeTile(rightX + 3, floorToExtendY + 2, 3);
                    WorldGen.PlaceTile(rightX + 2, floorToExtendY + 3, type, forced: true);
                    WorldGen.PlaceTile(rightX + 2, floorToExtendY + 4, type, forced: true);
                    WorldGen.SlopeTile(rightX + 2, floorToExtendY + 4, 3);

                    WorldGen.PlaceTile(rightX + extensionLength - 1, floorToExtendY + 2, type, forced: true);
                    WorldGen.PlaceTile(rightX + extensionLength - 2, floorToExtendY + 2, type, forced: true);
                    WorldGen.SlopeTile(rightX + extensionLength - 2, floorToExtendY + 2, 4);
                    WorldGen.PlaceTile(rightX + extensionLength - 1, floorToExtendY + 3, type, forced: true);
                    WorldGen.PlaceTile(rightX + extensionLength - 1, floorToExtendY + 4, type, forced: true);
                    WorldGen.SlopeTile(rightX + extensionLength - 1, floorToExtendY + 4, 4);

                    // Things above; don't do this if there's wall there
                    if (Main.tile[rightX + extensionLength + 3, floorToExtendY - floorHeight - 1].wall != wall)
                    {
                        WorldGen.PlaceTile(rightX + 2, floorToExtendY - floorHeight - 2, type, forced: true);
                        WorldGen.SlopeTile(rightX + 2, floorToExtendY - floorHeight - 2, 1);

                        WorldGen.PlaceTile(rightX + extensionLength - 1, floorToExtendY - floorHeight - 2, type, forced: true);
                        WorldGen.SlopeTile(rightX + extensionLength - 1, floorToExtendY - floorHeight - 2, 2);

                        for (int i = rightX + 2; i < rightX + extensionLength - 1; i++)
                        {
                            for (int j = floorToExtendY - floorHeight - 6; j < floorToExtendY - floorHeight; j++)
                            {
                                Main.tile[i, j].liquid = 255;
                                Liquid.AddWater(i, j);
                            }
                        }
                    }
                    #endregion

                    // Clear a passage between the houses' walls; start 5 blocks above and go down to 1 above the floor
                    for (int j = floorToExtendY - 5; j < floorToExtendY; j++)
                    {
                        Main.tile[rightX, j].active(false);
                        Main.tile[rightX + extensionLength, j].active(false);

                        Main.tile[rightX, j].wall = wall;
                        Main.tile[rightX + extensionLength, j].wall = wall;

                        Main.tile[rightX + 1, j].active(false);
                        Main.tile[rightX + 1 + extensionLength, j].active(false);

                        Main.tile[rightX + 1, j].wall = wall;
                        Main.tile[rightX + 1 + extensionLength, j].wall = wall;
                    }

                    int extensionRoof = floorToExtendY - floorHeight;
                    int relLeft = rightX + 1;
                    int relRight = rightX + extensionLength;

                    // Statues
                    for (int i = 0; i < 2; i++)
                    {
                        if (WorldGen.genRand.Chance(statueChance))
                        {
                            int x = WorldGen.genRand.Next(relLeft + 1, relRight - 1);
                            WorldGen.Place2xX(x, floorToExtendY - 1, (ushort)TileType<SoulStatue>(), WorldGen.genRand.Next(statueStyles));
                        }
                    }

                    // Paintings
                    if (WorldGen.genRand.Chance(paintingChance))
                    {
                        if (WorldGen.genRand.NextBool())
                            WorldGen.Place6x4Wall(
                                WorldGen.genRand.Next(relLeft + 3, relRight - 3),
                                WorldGen.genRand.Next(extensionRoof + 3, extensionRoof + 8),
                                TileID.Painting6X4, WorldGen.genRand.Next(17)
                                );
                        else
                            WorldGen.Place3x3Wall(
                                WorldGen.genRand.Next(relLeft + 3, relRight - 3),
                                WorldGen.genRand.Next(extensionRoof + 3, extensionRoof + 8),
                                TileID.Painting3X3, WorldGen.genRand.Next(13, 21)
                                );
                    }

                    // Torches
                    WorldGen.PlaceTile(relLeft + 2, extensionRoof + 2, TileType<ArkenTorchTile>());
                    WorldGen.PlaceTile(relRight - 2, extensionRoof + 2, TileType<ArkenTorchTile>());

                    Main.tile[relLeft + 2, extensionRoof + 2].frameX += 66;
                    Main.tile[relRight - 2, extensionRoof + 2].frameX += 66;

                    // Pots
                    for (int i = 0; i < potsPerFloor; i++)
                    {
                        if (WorldGen.genRand.Chance(potChance))
                        {
                            int x = WorldGen.genRand.Next(relLeft + 1, relRight - 1);
                            if (WorldGen.genRand.Chance(pustuleChance))
                                WorldGen.Place2x2(x, floorToExtendY - 1, (ushort)TileType<ErilipahPot>(), 0);
                            else
                                WorldGen.PlacePot(x, floorToExtendY - 1, 28, WorldGen.genRand.NextBool() ?
                                    (ushort)WorldGen.genRand.Next(13, 16) : (ushort)WorldGen.genRand.Next(31, 34));
                        }
                    }
                }
            }

            Action[] roofs = new Action[5] { PillarRoof, TriangleRoof, UnevenRoof, FluidRoof, HornedRoof };
            WorldGen.genRand.Next(roofs)();

            void PillarRoof()
            {
                // Bottom two rows
                for (int i = leftX - 3; i <= rightX + 3; i++)
                {
                    WorldGen.PlaceTile(i, roofY, type, forced: true);

                    WorldGen.PlaceTile(i, roofY - 1, type, forced: true);
                    if (i < leftX - 1 || i > rightX + 1)
                        Main.tile[i, roofY - 1].halfBrick(true);
                }

                const int outerWidth = 6;
                const int outerHeight = 5;
                // Place columns

                // 0bWXXX:
                // W = 1/0 if walls or not
                // X = int for what slopes; 0 for no tile
                // Set the shape to the left one
                const byte none = 0, full = 1, half = 2, slbl = 3, slbr = 4, sltl = 5, sltr = 6, optw = 8;

                byte[] shape = new byte[30] {
                    half, none, none, none, none, none,
                    full, full, full, full, slbl, none,
                    sltr, full, full, full, full, none,
                    none, none, optw, full, full, none,
                    none, none, half+optw, full, full, half
                };
                for (int i = 0; i < outerWidth; i++)
                {
                    int posI = leftX - 1 + i;
                    for (int j = 0; j < outerHeight; j++)
                    {
                        int posJ = roofY - (outerHeight - j) - 1;
                        byte info = shape[i + j * outerWidth];
                        byte tileInfo = (byte)(info & 0b0111);
                        if (tileInfo > 0)
                        {
                            WorldGen.PlaceTile(posI, posJ, type, forced: true);
                            if (tileInfo == 2)
                                Main.tile[posI, posJ].halfBrick(true);
                            if (tileInfo > 2)
                                WorldGen.SlopeTile(posI, posJ, tileInfo - 2);
                        }
                        else
                        {
                            Main.tile[posI, posJ].liquid = 255;
                            Liquid.AddWater(posI, posJ);
                        }

                        if (info >> 3 == 1)
                        {
                            WorldGen.PlaceWall(posI, posJ, wall);
                        }
                    }
                }

                // Right side
                shape = new byte[30] {
                    none, none, none, none, none, half,
                    none, slbr, full, full, full, full,
                    none, full, full, full, full, sltl,
                    none, full, full, optw, none, none,
                    half, full, full, half+optw, none, none
                };
                for (int i = 0; i < outerWidth; i++)
                {
                    int posI = rightX - 4 + i;
                    for (int j = 0; j < outerHeight; j++)
                    {
                        int posJ = roofY - (outerHeight - j) - 1;
                        byte info = shape[i + j * outerWidth];
                        byte tileInfo = (byte)(info & 0b0111);
                        if (tileInfo > 0)
                        {
                            WorldGen.PlaceTile(posI, posJ, type, forced: true);
                            if (tileInfo == 2)
                                Main.tile[posI, posJ].halfBrick(true);
                            if (tileInfo > 2)
                                WorldGen.SlopeTile(posI, posJ, tileInfo - 2);
                        }
                        else
                        {
                            Main.tile[posI, posJ].liquid = 255;
                            Liquid.AddWater(posI, posJ);
                        }

                        if (info >> 3 == 1)
                        {
                            WorldGen.PlaceWall(posI, posJ, wall);
                        }
                    }
                }

                bool even = (rightX - leftX) % 2 != 0;
                int halfway = (rightX - leftX) / 2;
                const int innerWidth = 9;
                const int innerHeight = 10;

                // Set the shape to the middle
                shape = new byte[90] {
                    none, none, none, half, half, half, none, none, none,
                    half, half, full, full, full, full, full, half, half,
                    full, full, full, full, full, full, full, full, full,
                    sltr, full, full, full, full, full, full, full, sltl,
                    none, none, sltr, full, full, full, sltl, none, none,
                    none, none, none, full, full, full, none, none, none,
                    none, none, none, full, full, full, none, none, none,
                    none, none, none, full, full, full, none, none, none,
                    none, none, none, full, full, full, none, none, none,
                    none, none, half, full, full, full, half, none, none,
                };
                for (int i = 0; i < innerWidth; i++)
                {
                    if (even && i == 4)
                        continue;

                    int posI = leftX + halfway - (innerWidth / 2) + i;
                    if (even && i <= 4) posI++;

                    for (int j = 0; j < innerHeight; j++)
                    {
                        int posJ = roofY - (innerHeight - j) - 1;
                        byte info = shape[i + j * innerWidth];
                        byte tileInfo = (byte)(info & 0b0111);
                        if (tileInfo > 0)
                        {
                            WorldGen.PlaceTile(posI, posJ, type, forced: true);
                            if (tileInfo == 2)
                                Main.tile[posI, posJ].halfBrick(true);
                            if (tileInfo > 2)
                                WorldGen.SlopeTile(posI, posJ, tileInfo - 2);
                        }
                        else
                        {
                            Main.tile[posI, posJ].liquid = 255;
                            Liquid.AddWater(posI, posJ);
                        }

                        if (j > 3 && i > 1 && i < innerWidth - 2)
                        {
                            WorldGen.PlaceWall(posI, posJ, wall);
                        }
                    }
                }
            }
            void TriangleRoof()
            {
                int leftSide = leftX - 3;
                int rightSide = rightX + 3;
                float y = 0.5f;
                while (leftSide <= rightSide)
                {
                    int topRoof = roofY - (int)Math.Ceiling(y);
                    for (int j = topRoof; j < roofY; j++)
                    {
                        WorldGen.PlaceTile(leftSide, j, type, forced: true);
                        WorldGen.PlaceTile(rightSide, j, type, forced: true);
                    }
                    if (y % 0.5 == 0 && y % 1 != 0) // if half
                    {
                        // Make those tiles half-tiles
                        Main.tile[leftSide, topRoof].halfBrick(true);
                        Main.tile[rightSide, topRoof].halfBrick(true);
                    }

                    y += 0.5f;
                    leftSide++;
                    rightSide--;
                }
            }
            void UnevenRoof()
            {
                int stoppingOffset = WorldGen.genRand.Next(-2, 3);
                int midPoint = leftX + (rightX - leftX) / 2 + stoppingOffset;

                float y = 0.5f;
                const float deg = 0.25f;

                for (float leftSide = leftX - 3; leftSide < midPoint; leftSide += WorldGen.genRand.NextFloat())
                {
                    int topRoof = roofY - (int)Math.Ceiling(y);
                    for (int j = topRoof; j < roofY; j++)
                    {
                        WorldGen.PlaceTile((int)leftSide, j, type, forced: true);
                    }
                    if (y % 1 < 0.5) // if half
                    {
                        // Make those tiles half-tiles
                        Main.tile[(int)leftSide, topRoof].halfBrick(true);
                    }

                    y += WorldGen.genRand.NextFloat() * (stoppingOffset > 0 ? 1 - deg : 1 + deg);
                }

                y = 0.5f;
                for (float rightSide = rightX + 3; rightSide > midPoint; rightSide -= WorldGen.genRand.NextFloat())
                {
                    int topRoof = roofY - (int)Math.Ceiling(y);
                    for (int j = topRoof; j < roofY; j++)
                    {
                        WorldGen.PlaceTile((int)rightSide, j, type, forced: true);
                    }
                    if (y % 1 < 0.5) // if half
                    {
                        // Make those tiles half-tiles
                        Main.tile[(int)rightSide, topRoof].halfBrick(true);
                    }

                    y += WorldGen.genRand.NextFloat() * (stoppingOffset < 0 ? 1 - deg : 1 + deg);
                }
            }
            void FluidRoof()
            {
                const byte none = 0, full = 1, half = 2, watr = 3;
                const int width = 11, height = 8;

                bool even = (rightX - leftX) % 2 != 0;
                int halfway = (rightX - leftX) / 2;

                for (int i = leftX - 3; i <= rightX + 3; i++)
                    WorldGen.PlaceTile(i, roofY, type, forced: true);
                for (int i = leftX - 2 - (even ? 1 : 0); i <= rightX + 2; i++)
                {
                    WorldGen.PlaceTile(i, roofY - 1, type, forced: true);
                    if (i == leftX - 2 - (even ? 1 : 0) || i == rightX + 2)
                        Main.tile[i, roofY - 1].halfBrick(true);
                }

                byte[] shape = new byte[88] {
                    none, none, none, none, half, half, half, none, none, none, none,
                    none, half, half, full, full, full, full, full, half, half, none,
                    full, full, full, full, full, full, full, full, full, full, full,
                    half, watr, watr, watr, watr, watr, watr, watr, watr, watr, half,
                    full, watr, watr, watr, watr, watr, watr, watr, watr, watr, full,
                    full, full, full, full, full, full, full, full, full, full, full,
                    half, watr, watr, watr, watr, watr, watr, watr, watr, watr, half,
                    full, watr, watr, watr, watr, watr, watr, watr, watr, watr, full,
                };
                for (int i = 0; i < width; i++)
                {
                    if (even && i == width / 2)
                        continue;

                    int posI = leftX + halfway - (width / 2) + i;
                    if (even && i <= 4) posI++;

                    for (int j = 0; j < height; j++)
                    {
                        int posJ = roofY - (height - j) - 1;
                        byte info = shape[i + j * width];
                        byte tileInfo = (byte)(info & 0b0111);
                        if (tileInfo > 0 && tileInfo < 3)
                        {
                            WorldGen.PlaceTile(posI, posJ, type, forced: true);
                            if (tileInfo == 2)
                                Main.tile[posI, posJ].halfBrick(true);
                        }
                        if (tileInfo == 3)
                        {
                            Main.tile[posI, posJ].liquid = 255;
                            Liquid.AddWater(posI, posJ);
                            WorldGen.PlaceWall(posI, posJ, wall);
                        }
                    }
                }
                int extraI = leftX + halfway - (width / 2);
                extraI += even ? 1 : 0;
                WorldGen.PlaceTile(extraI - 1, roofY - 2, type, forced: true);
                WorldGen.PlaceTile(extraI - 2, roofY - 2, type, forced: true);
                Main.tile[extraI - 2, roofY - 2].halfBrick(true);

                extraI = leftX + halfway + (width / 2);
                WorldGen.PlaceTile(extraI + 1, roofY - 2, type, forced: true);
                WorldGen.PlaceTile(extraI + 2, roofY - 2, type, forced: true);
                Main.tile[extraI + 2, roofY - 2].halfBrick(true);
            }
            void HornedRoof()
            {
                // Tiles
                for (int i = leftX - 1; i <= rightX + 1; i++)
                {
                    WorldGen.PlaceTile(i, roofY - 2, type, forced: true);
                    if (i > leftX + 3 && i < rightX - 3)
                        Main.tile[i, roofY - 2].halfBrick(true);

                    WorldGen.PlaceTile(i, roofY - 1, type, forced: true);
                }
                WorldGen.PlaceTile(leftX - 1, roofY - 5, type, forced: true);
                WorldGen.PlaceTile(rightX + 1, roofY - 5, type, forced: true);

                WorldGen.PlaceTile(leftX - 1, roofY - 4, type, forced: true);
                WorldGen.PlaceTile(rightX + 1, roofY - 4, type, forced: true);

                WorldGen.PlaceTile(leftX - 1, roofY - 3, type, forced: true);
                WorldGen.PlaceTile(rightX + 1, roofY - 3, type, forced: true);

                WorldGen.PlaceTile(leftX, roofY - 3, type, forced: true);
                WorldGen.PlaceTile(rightX, roofY - 3, type, forced: true);

                WorldGen.PlaceTile(leftX + 1, roofY - 3, type, forced: true); Main.tile[leftX + 1, roofY - 3].halfBrick(true);
                WorldGen.PlaceTile(rightX - 1, roofY - 3, type, forced: true); Main.tile[rightX - 1, roofY - 3].halfBrick(true);

                // Walls
                for (int i = leftX; i <= rightX; i++)
                {
                    for (int j = roofY - 4; j < roofY; j++)
                    {
                        WorldGen.PlaceWall(i, j, wall);
                        Main.tile[i, j].wall = wall;
                    }
                }
            }
        }
        private void MakeChest(Item[] loot)
        {
            // All  One     Primary item: Nidorose, Dragon Peepee, Soul Hearth, Soul Splitter
            // 50%  1-3     Dynamite 
            // 70%  4-12    Greater healing pots
            // 30%  12-22   Mythril or Orch. bars
            // 50%  15-29   Cobalt or Palladium bars
            // 50%  65-100  Hellfire arrows, Phlogiston arrows, silver bullets
            // All  1-3     Effulgence, Inhibitor, Antihemic, or Purity potions
            // 50%  2-3     Shine, Water Walking, Lifeforce, Heartreach
            // 50%  1-2     Magic Power, Regeneration, Ironskin, Inferno
            // 75%  1-4     Arken Torch     or    12-20   Crystalline Torch
            // All  2-4     Gold Coin

            int index = 0;

            if (Main.expertMode)
                AddItem(1, 1, 1, ItemType<DragonPeepee>(), ItemType<Nidorose>(), ItemType<HeadSplitter>(), ItemType<SoulCleanser>());
            else
                AddItem(1, 1, 1, ItemType<DragonPeepee>(), ItemType<Nidorose>(), ItemType<HeadSplitter>());

            void AddItem(float chance, int minimum, int maximum, params int[] itemTypes)
            {
                if (chance >= 1f || WorldGen.genRand.Chance(chance))
                {
                    loot[index].SetDefaults(itemTypes.Length > 1 ? WorldGen.genRand.Next(itemTypes) : itemTypes[0]);
                    loot[index].stack = WorldGen.genRand.Next(minimum, maximum + 1);
                    index++;
                }
            }

            AddItem(0.5f, 1, 3, ItemID.Dynamite);
            AddItem(0.7f, 4, 12, ItemID.GreaterHealingPotion);

            int barType = WorldGen.genRand.Next(2) == 0 ? ItemID.MythrilBar : ItemID.OrichalcumBar;
            AddItem(0.3f, 4, 12, barType);
            barType = barType == ItemID.MythrilBar ? ItemID.CobaltBar : ItemID.PalladiumBar;
            AddItem(0.5f, 15, 29, barType);

            AddItem(0.5f, 65, 100, ItemID.HellfireArrow, ItemID.SilverBullet, ItemType<Items.Phlogiston.PhlogistonArrow>());

            AddItem(1.0f, 1, 3, ItemType<EffulgencePot>(), ItemType<SlowingPot>(), ItemType<ReductionPot>(), ItemType<PurityPot>());
            AddItem(0.50f, 1, 3, ItemID.ShinePotion, ItemID.WaterWalkingPotion, ItemID.LifeforcePotion, ItemID.HeartreachPotion);
            AddItem(0.50f, 1, 3, ItemID.MagicPowerPotion, ItemID.RegenerationPotion, ItemID.IronskinPotion, ItemID.EndurancePotion);

            if (WorldGen.genRand.NextBool())
                AddItem(0.75f, 1, 4, ItemType<ArkenTorch>());
            else
                AddItem(0.75f, 12, 20, ItemType<CrystallineTorch>());

            AddItem(1f, 2, 5, ItemID.GoldCoin);
        }

        private void HazardGen(GenerationProgress progress)
        {
            int halfWidth = (int)(Main.maxTilesX * 0.07f);
            int start = Math.Max(1, BiomeCenterX - halfWidth);
            int end = Math.Min(Main.maxTilesX - 1, BiomeCenterX + halfWidth);

            for (int i = start; i < end; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    Tile tile = Main.tile[i, j];

                    if (tile.IsErilipahTile() && WorldGen.genRand.Chance(0.1f))
                    {
                        PlaceHazard(i, j);
                    }
                    if (tile.IsErilipahTile() && WorldGen.genRand.Chance(0.07f))
                    {
                        Mushroom.TryPlace(i, j);
                    }
                }
            }
        }
        public static void PlaceHazard(int i, int j)
        {
            PlaceHazard(i, j, WorldGen.genRand.Next(7));
        }
        public static void PlaceHazard(int i, int j, int type)
        {
            /* 0= stalk
             * 1= bubble
             * 2= vine
             * 3= geyser
             * 4= giant pf
             * 5= vent
             * 6= hive */

            bool isLostCity = Main.tile[i, j].active() && Main.tile[i, j].type == TileType<TaintedBrick>();
            isLostCity |= Main.tile[i, j + 1].active() && Main.tile[i, j + 1].type == TileType<TaintedBrick>();
            if (isLostCity)
                return;

            switch (type)
            {
                default:
                    if (Stalk.IsValid(i, j) && Stalk.IsValid(i - 1, j) && Stalk.IsValid(i + 1, j))
                    {
                        short frameY = (short)(Main.rand.Next(5, 8) * 18);
                        for (int n = -1; n <= 1; n++)
                        {
                            Main.tile[i + n, j - 1].active(true);
                            Main.tile[i + n, j - 1].type = (ushort)TileType<Stalk>();
                            Main.tile[i + n, j - 1].frameX = (short)(72 + n * 18);
                            Main.tile[i + n, j - 1].frameY = frameY;
                        }
                    }
                    break;

                case 1:
                    WorldGen.Place2x1(i, j - 1, (ushort)TileType<Flower>()); break;

                case 2:
                    if (WorldGen.SolidOrSlopedTile(Main.tile[i, j]) && Main.tile[i, j].slope() == 0 &&
                        !Main.tile[i, j + 1].active() && !WorldGen.SolidOrSlopedTile(Main.tile[i, j + 2]))
                    {
                        Tile vine = Main.tile[i, j + 1];
                        vine.active(true);
                        vine.type = (ushort)TileType<Vine>();
                        vine.frameX = Main.rand.Next(new short[] { 0, 18, 36 });
                        vine.frameY = 0;
                    }
                    break;

                case 3:
                    WorldGen.Place3x1(i, j - 1, (ushort)TileType<GasGeyser>());
                    break;

                case 4:
                    if (WorldGen.genRand.Chance(0.5f))
                        goto default;
                    WorldGen.Place3x2(i, j - 1, (ushort)TileType<GiantPF>()); break;

                case 5:
                    for (int n = -10; n < 10; n++)
                    {
                        for (int m = -10; m < 10; m++)
                        {
                            if (Main.tile[i + n, j + m].type == TileType<Vent>())
                                return;
                        }
                    }
                    WorldGen.Place2xX(i, j - 1, (ushort)TileType<Vent>()); break;

                case 6:
                    bool hayBase =
                        Main.tile[i, j].active() && Main.tile[i + 1, j].active();
                    bool puede =
                        !Main.tile[i, j + 1].active() && !Main.tile[i, j + 1].active() &&
                        !Main.tile[i + 1, j + 1].active() && !Main.tile[i + 1, j + 1].active();

                    if (!hayBase || !puede)
                        break;

                    for (int n = 0; n < 2; n++)
                        for (int m = 0; m < 2; m++)
                        {
                            Main.tile[i + n, j + m + 1].active(true);
                            Main.tile[i + n, j + m + 1].type = (ushort)TileType<Hive>();
                            Main.tile[i + n, j + m + 1].frameX = (short)(n * 18);
                            Main.tile[i + n, j + m + 1].frameY = (short)(m * 18);
                        }
                    break;
            }
        }

        private void Infect(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile.type == TileID.LihzahrdAltar || tile.type == TileID.LihzahrdBrick || tile.wall == WallID.LihzahrdBrickUnsafe)
                return;

            if (tile.type == 21)
            {
                tile.type = (ushort)TileType<ErilipahChest>();
            }

            if (WorldGen.SolidOrSlopedTile(tile) || tile.wall == WallID.EbonstoneUnsafe || tile.wall == WallID.CrimstoneUnsafe)
            {
                bool organic = TileID.Sets.Mud[tile.type] || tile.type == TileID.Slush || tile.type == TileID.Silt;

                if (organic)
                    tile.type = T("SpoiledClump");
                else
                    tile.type = T("InfectedClump");
            }

            if (tile.wall > 0)
            {
                tile.wall = (ushort)WallType<InfectedClump.InfectedClumpWall>();

                WorldGen.SquareWallFrame(i, j);
            }
        }

        private void SacraciteOre(GenerationProgress progress)
        {
            // Sacracite
            progress.Message = "[Erilipah] Ores";
            int veins = 0;
            while (veins < 6 + WorldGen.genRand.Next(0, 6))
            {
                progress.Set(veins / 6f);
                int i = WorldGen.genRand.Next(WorldGen.UndergroundDesertLocation.X, WorldGen.UndergroundDesertLocation.Right);
                int j = WorldGen.genRand.Next(WorldGen.UndergroundDesertLocation.Y, WorldGen.UndergroundDesertLocation.Bottom);
                if (WorldGen.SolidOrSlopedTile(Main.tile[i, j]))
                {
                    veins++;
                    WorldGen.OreRunner(i: i, j: j, strength: 7, steps: 10, type: T("SacraciteTileTile"));
                }
            }
        }
        private void CrystallineOre(GenerationProgress progress)
        {
            progress.Message = "[Erilipah] Ores";
            int veins = 0;
            while (veins < 27 + WorldGen.genRand.Next(0, 6))
            {
                progress.Set(veins / 27f);
                int i = WorldGen.genRand.Next(0, Main.maxTilesX);
                int j = WorldGen.genRand.Next(0, Main.maxTilesY);
                if (WorldGen.SolidOrSlopedTile(Main.tile[i, j]) && Main.tile[i, j].type == T("InfectedClump"))
                {
                    veins++;
                    WorldGen.TileRunner(i: i, j: j, strength: 3, steps: 9, type: T("CrystallineTileTile"), false, 0, 0);
                }
            }
        }

        private struct ChestItem
        {
            internal readonly int itemType;
            internal readonly short chestType;
            internal readonly float chance;

            private readonly short minNum;
            private readonly short maxNum;

            internal int Amount()
            {
                return WorldGen.genRand.Next(minNum, maxNum + 1);
            }
            internal ChestItem(int itemType, short minNum, short maxNum, float chance, short chestType)
            {
                this.itemType = itemType;
                this.minNum = minNum;
                this.maxNum = maxNum;
                this.chance = chance;
                this.chestType = chestType;
            }
        }
        public override void PostWorldGen()
        {
            ChestItem[] possibleItems = new ChestItem[]
            {
                new ChestItem(
                    I("ForestsWrath"),
                    1,
                    1,
                    0.5f,
                    0), // Living tree chest
            };
            ChestItem[] addedItems = new ChestItem[]
            {
                new ChestItem(
                    I("NightsBane"),
                    1,
                    1,
                    0.16f,
                    2 // Locked gold chest (dungeon)
                )
            };

            for (int selectedItem = 0; selectedItem < possibleItems.Length + addedItems.Length; selectedItem++)
            {
                for (int i = 0; i < Main.chest.Length; i++)
                {
                    Chest chest = Main.chest[i];
                    if (chest != null && Main.tile[chest.x, chest.y].type == 21)
                    {
                        if (selectedItem < possibleItems.Length &&
                            Main.rand.NextFloat() < possibleItems[selectedItem].chance &&
                            Main.tile[chest.x, chest.y].frameX == possibleItems[selectedItem].chestType * 18)
                        {
                            if (chest.item[0].type != ItemID.GoldenKey &&
                                chest.item[0].type != ItemID.ShadowKey &&
                                chest.item[0].type != ItemID.Muramasa)
                            {
                                chest.item[0].SetDefaults(possibleItems[selectedItem].itemType, false);
                                chest.item[0].stack = possibleItems[selectedItem].Amount();
                                if (WorldGen.genRand.Chance(0.33f))
                                    break;
                            }
                        }
                        int addedItem = selectedItem - possibleItems.Length;
                        if (selectedItem >= possibleItems.Length &&
                            Main.rand.NextFloat() < addedItems[addedItem].chance &&
                            Main.tile[chest.x, chest.y].frameX == addedItems[addedItem].chestType * 18)
                        {
                            chest.item[1].SetDefaults(addedItems[addedItem].itemType, false);
                            chest.item[1].stack = addedItems[addedItem].Amount();
                            if (WorldGen.genRand.Chance(0.33f))
                                break;
                        }
                    }
                }
            }
        }
    }
}
