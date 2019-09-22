using Erilipah.Biomes.ErilipahBiome.Tiles;
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
        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            Tile tile = Main.tile[i, j];

            bool onScreenX = i < Main.screenPosition.X / 16 || i > (Main.screenPosition.X + Main.screenWidth) / 16;
            bool onScreenY = j < Main.screenPosition.Y / 16 || j > (Main.screenPosition.Y + Main.screenHeight) / 16;
            bool onScreen = onScreenX && onScreenY;

            if (Main.netMode != 1 && tile.IsErilipahTile() && Main.rand.Chance(0.007f) && !onScreen)
                ErilipahWorld.PlaceHazard(i, j, mod);

            if (Main.rand.Chance(ErilipahItem.LightSnuffRate) && type != mod.TileType<Items.ErilipahBiome.ArkenTorchTile>())
                Snuff(i, j, type, tile);
        }

        private void Snuff(int i, int j, int type, Tile tile)
        {
            if (TileID.Sets.RoomNeeds.CountsAsTorch.Any(t => t == type) ||
                TileObjectData.GetTileData(tile) == TileObjectData.GetTileData(TileID.Torches, 0) ||
                TileObjectData.GetTileData(tile) == TileObjectData.GetTileData(TileID.Campfire, 0))
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
