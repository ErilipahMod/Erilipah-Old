using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah
{
    class ErilipahTile : GlobalTile
    {
        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            if (!Main.rand.Chance(ErilipahItem.LightSnuffRate))
                return;

            Tile tile = Main.tile[i, j];
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
                    Main.PlaySound(SoundID.LiquidsWaterLava.WithPitchVariance(-0.35f), new Vector2((float)(i * 16 + 8), (float)(j * 16 + 8)));

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
