using Erilipah.Biomes.ErilipahBiome.Tiles;
using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah
{
    internal class ErilipahTile : GlobalTile
    {
        public static bool OnScreen(int i, int j)
        {
            bool onScreenX = i < Main.screenPosition.X / 16 || i > (Main.screenPosition.X + Main.screenWidth) / 16;
            bool onScreenY = j < Main.screenPosition.Y / 16 || j > (Main.screenPosition.Y + Main.screenHeight) / 16;
            return onScreenX & onScreenY;
        }

        public override void RandomUpdate(int i, int j, int type)
        {
            // TODO after testing
            if (Main.netMode != 1 && Main.tile[i, j].IsErilipahTile() && Main.rand.Chance(0.08f) && !OnScreen(i, j))
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
