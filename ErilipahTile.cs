using Erilipah.Biomes.ErilipahBiome.Tiles;
using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    internal class ErilipahTile : GlobalTile
    {
        public static bool OffScreen(int i, int j)
        {
            bool offScreenX = i < Main.screenPosition.X / 16 || i > (Main.screenPosition.X + Main.screenWidth) / 16;
            bool offScreenY = j < Main.screenPosition.Y / 16 || j > (Main.screenPosition.Y + Main.screenHeight) / 16;
            return offScreenX & offScreenY;
        }

        public override void RandomUpdate(int i, int j, int type)
        {
            if (Main.netMode != 1 && Main.tile[i, j].IsErilipahTile() && Main.rand.Chance(0.005f) && OffScreen(i, j)) 
            {
                /* 0= 2x    stalk
                 * 1= 1x    bubble
                 * 2= 3x    vine
                 * 3= 1x    geyser
                 * 4= 1.5x  giant pf
                 * 5= 0.85x vent
                 * 6= 1.25x hive */

                Terraria.Utilities.WeightedRandom<int> rand = new Terraria.Utilities.WeightedRandom<int>();
                rand.Add(0, 2);
                rand.Add(1, 1);
                rand.Add(2, 3);
                rand.Add(3, 1);
                rand.Add(4, 1.5);
                rand.Add(5, 0.85);
                rand.Add(6, 1.25);

                ErilipahWorld.PlaceHazard(i, j, rand, mod);
            }
        }

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            if (Main.netMode != 1 && Main.rand.Chance(ErilipahItem.LightSnuffRate))
                Snuff(i, j);
        }

        public static void Snuff(int i, int j, bool ignoreArken = false)
        {
            try
            {
                Tile tile = Main.tile[i, j];
                int type = tile.type;

                bool light = tile.type == TileID.Torches || tile.type == TileID.Campfire || TileLoader.IsTorch(type);
                if (ignoreArken)
                    light &= type != Erilipah.Instance.TileType<ArkenTorchTile>();
                if (light)
                {
                    if (!ignoreArken && type == Erilipah.Instance.TileType<CrystallineTorchTile>() && Main.rand.Chance(0.50f))
                    {
                        return;
                    }

                    if (Main.LocalPlayer.InErilipah())
                    {
                        ErilipahItem.SnuffFx(new Vector2(i * 16 + 8, j * 16 + 8));
                        Main.PlaySound(SoundID.LiquidsWaterLava.WithPitchVariance(-0.35f), new Vector2(i * 16 + 8, j * 16 + 8));

                        WorldGen.KillTile(i, j, false, noItem: true);
                        WorldGen.TileFrame(i, j);
                    }
                }
            }
            catch { Main.NewText("PEE"); }
        }

        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            Tile tileAbove = Main.tile[i, j - 1];
            bool tileAboveImportant = false;
            tileAboveImportant |= tileAbove.type == mod.TileType<SoulStatue>();
            tileAboveImportant |= tileAbove.type == mod.TileType<Altar>();
            return !tileAboveImportant;
        }
    }
}
