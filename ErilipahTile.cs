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
            if (Main.netMode != 1 && Main.tile[i, j].IsErilipahTile() && Main.rand.Chance(0.02f) && OffScreen(i, j))
                ErilipahWorld.PlaceHazard(i, j, mod);
        }

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            Tile tile = Main.tile[i, j];

            if (Main.netMode != 1 && Main.rand.Chance(ErilipahItem.LightSnuffRate))
                Snuff(i, j, type, tile);
        }

        private void Snuff(int i, int j, int type, Tile tile)
        {
            try
            {
                bool light = tile.type == TileID.Torches || tile.type == TileID.Campfire || TileLoader.IsTorch(type);
                light &= type != mod.TileType<ArkenTorchTile>();
                if (light)
                {
                    if (type == mod.TileType<Items.ErilipahBiome.CrystallineTorchTile>() && Main.rand.Chance(0.50f))
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
