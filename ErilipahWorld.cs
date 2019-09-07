using Erilipah.Biomes.ErilipahBiome.Tiles;
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

        public override void Initialize()
        {
            sanguineOreSpawned = NPC.downedBoss1;
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                [nameof(downedLunaemia)] = downedLunaemia,
                [nameof(downedTaintedSkull)] = downedTaintedSkull
            };
        }
        public override void Load(TagCompound tag)
        {
            downedLunaemia = tag.ContainsKey(nameof(downedLunaemia));
            downedTaintedSkull = tag.ContainsKey(nameof(downedTaintedSkull));
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
