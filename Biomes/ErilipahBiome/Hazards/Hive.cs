using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Hive : HazardTile
    {
        public override string MapName => "Light Hive";
        public override int DustType => DustType<FlowerDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
                TileObjectData.newTile.AnchorTop = new Terraria.DataStructures.AnchorData(Terraria.Enums.AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
                TileObjectData.newTile.AnchorBottom = Terraria.DataStructures.AnchorData.Empty;
                TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<Tiles.InfectedClump>(), TileType<Tiles.SpoiledClump>() };
                return TileObjectData.newTile;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Main.tileCut[Type] = true;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (NPC.CountNPCS(NPCType<LightWisp>()) < 15 && Main.netMode != 1)
            {
                Vector2 spawn = GetSpawn(i, j);

                NPC n = Main.npc[NPC.NewNPC((int)spawn.X, (int)spawn.Y, NPCType<LightWisp>())];
                (n.modNPC as LightWisp).returnPos = spawn;
            }
        }

        public override bool KillSound(int i, int j)
        {
            Main.PlaySound(SoundID.NPCDeath1, new Microsoft.Xna.Framework.Vector2(i, j) * 16);
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode == 1)
                return;

            Vector2 spawn = GetSpawn(i, j);

            // Spawn some wisps & guards
            for (int n = 0; n < 6; n++)
            {
                Main.npc[NPC.NewNPC((int)spawn.X, (int)spawn.Y, NPCType<LightWisp>())]
                    .timeLeft = 700;
                if (n < 2)
                    NPC.NewNPC((int)spawn.X, (int)spawn.Y, NPCType<LightWispGuard>(), Target: Helper.FindClosestPlayer(spawn, 300));
            }
        }

        private static Vector2 GetSpawn(int i, int j)
        {
            int x = i - Main.tile[i, j].frameX / 18;
            int y = j - Main.tile[i, j].frameY / 18;
            Vector2 spawn = new Vector2(x * 16 + 8, y * 16 + 28);
            return spawn;
        }
    }
}
