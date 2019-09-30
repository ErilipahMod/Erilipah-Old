using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Hive : HazardTile
    {
        public override string MapName => "Light Hive";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style => TileObjectData.GetTileData(TileID.Hive, 0);

        public override void SetDefaults()
        {
            base.SetDefaults();
            Main.tileCut[Type] = true;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.netMode != 1)
            {
                Vector2 spawn = GetSpawn(i, j);

                NPC n = Main.npc[NPC.NewNPC((int)spawn.X, (int)spawn.Y, mod.NPCType<LightWisp>())];
                (n.modNPC as LightWisp).returnPos = spawn;
            }
        }

        public override bool KillSound(int i, int j)
        {
            Main.PlaySound(SoundID.NPCDeath1, new Microsoft.Xna.Framework.Vector2(i, j) * 2);
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode == 1)
                return;

            Vector2 spawn = GetSpawn(i, j);

            // Make all the wisps with this tile queued lost
            for (int n = 0; n < 200; n++)
            {
                NPC npc = Main.npc[n];
                LightWisp wisp = npc.modNPC as LightWisp;

                if (wisp.returnPos == spawn)
                {
                    wisp.returnPos = Vector2.Zero;
                }
            }

            // Spawn some wisps & guards
            for (int n = 0; n < 6; n++)
            {
                NPC.NewNPC((int)spawn.X, (int)spawn.Y, mod.NPCType<LightWisp>());
                if (n < 2)
                    NPC.NewNPC((int)spawn.X, (int)spawn.Y, mod.NPCType<LightWispGuard>(), Target: Helper.FindClosestPlayer(spawn, 300));
            }
        }

        private static Vector2 GetSpawn(int i, int j)
        {
            int x = i - Main.tile[i, j].frameX / 18;
            int y = j - Main.tile[i, j].frameY / 18;
            Vector2 spawn = new Vector2(x * 16 + 6, y * 16 + 24);
            return spawn;
        }
    }
}
