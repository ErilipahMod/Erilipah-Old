using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Erilipah
{
    public partial class ErilipahWorld : ModWorld
    {
        public static int erilipahTiles = 0;
        public static int lostCityTiles = 0;

        public static bool sanguineOreSpawned = false;
        public static bool downedLunaemia = false;
        public static bool downedTaintedSkull = false;

        public static Vector2 AltarPosition { get; private set; }
        public static Vector2 ChasmPosition { get; private set; }

        public override void Initialize()
        {
            sanguineOreSpawned = NPC.downedBoss1;
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                [nameof(downedLunaemia)] = downedLunaemia,
                [nameof(downedTaintedSkull)] = downedTaintedSkull,
                ["AltarX"] = AltarPosition.X,
                ["AltarY"] = AltarPosition.Y
            };
        }
        public override void Load(TagCompound tag)
        {
            downedLunaemia = tag.ContainsKey(nameof(downedLunaemia));
            downedTaintedSkull = tag.ContainsKey(nameof(downedTaintedSkull));

            AltarPosition = new Vector2(
                tag.GetFloat("AltarX"), tag.GetFloat("AltarY")
                );
        }

        public override void PostUpdate()
        {
            IfNoneSpawnAboryc();
        }

        private void IfNoneSpawnAboryc()
        {
            int aborycType = mod.ProjectileType<Items.ErilipahBiome.AbProj>();
            if (!Main.projectile.Any(p => p.active && p.type == aborycType))
            {
                Projectile.NewProjectile(AltarPosition, Vector2.Zero, aborycType, 0, 0, 255);
            }
        }

        public override void ResetNearbyTileEffects()
        {
            erilipahTiles = lostCityTiles = 0;
        }
        public override void TileCountsAvailable(int[] tileCounts)
        {
            lostCityTiles = tileCounts[mod.TileType<TaintedBrick>()] + tileCounts[mod.TileType<TaintedRubble>()];
            erilipahTiles = tileCounts[mod.TileType<InfectedClump>()] + tileCounts[mod.TileType<SpoiledClump>()] + lostCityTiles;
        }
    }
}
