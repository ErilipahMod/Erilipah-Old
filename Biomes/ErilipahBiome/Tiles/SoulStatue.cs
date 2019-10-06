using Erilipah.Items.ErilipahBiome;
using Erilipah.NPCs.LostCity;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    public class TESoulStatue : ModTileEntity
    {
        internal bool active = false;
        internal int Crack { get; private set; } = 0;
        internal bool Broken { get; private set; } = false;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            bool valid = tile.active() && tile.type == mod.TileType<SoulStatue>() && tile.frameX == 0 && tile.frameY == 0;
            return valid;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            active = false;
            Crack = 0;
            Broken = false;
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public override void Update()
        {
            if (Broken)
            {
                if (!active)
                    Crack++;
                if (Crack > 36000)
                {
                    Crack = 0;
                    Broken = false;
                }
                return;
            }
            if (Crack == 60 || Crack == 120)
            {
                Main.PlaySound(0, Position.ToVector2() * 16);
                Crack++;
            }
            if (Crack > SoulStatue.CrackingPoint * 2)
                if (!Broken && Main.netMode != 1)
                {
                    Main.PlaySound(4, Position.X * 16, Position.Y * 16, 23);
                    int target = Helper.FindClosestPlayer(new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8), 1000);
                    if (target == -1)
                        target = 255;
                    NPC.NewNPC(Position.X * 16 + 8, Position.Y * 16 + 32, mod.NPCType<Abomination>(), Target: target);
                    Loot.DropItem(new Rectangle(Position.X * 16, Position.Y * 16, 32, 48), mod.ItemType<SoulRubble>(), 1, 1, 50);

                    Broken = true;
                }
            if (!active)
            {
                if (Crack < SoulStatue.CrackingPoint && Crack > 0) Crack--;
                return;
            }

            int plrIndex = Helper.FindClosestPlayer(new Vector2(Position.X * 16 + 8, Position.Y * 16 + 8), 1000);
            if (plrIndex == -1 || Main.myPlayer != plrIndex)
                return;

            Player plr = Main.player[plrIndex];
            bool canSeePlayer = Collision.CanHitLine(plr.Center, 1, 1, new Vector2(Position.X * 16, Position.Y * 16) + new Vector2(10, 12), 1, 1);
            if (canSeePlayer)
                Crack++;
        }

        public override void NetSend(BinaryWriter writer, bool lightSend)
        {
            writer.Write(Crack);
            writer.Write(Broken);
        }

        public override void NetReceive(BinaryReader reader, bool lightReceive)
        {
            Crack = reader.ReadInt32();
            Broken = reader.ReadBoolean();
        }
    }

    public class SoulStatue : ModTile
    {
        private bool TryGetTE(int i, int j, out TESoulStatue statue)
        {
            Tile tile = Main.tile[i, j];
            int x = i - tile.frameX / 18;
            int y = j - tile.frameY / 18;
            if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out var tileEntity))
            {
                statue = tileEntity as TESoulStatue;
            }
            else
            {
                statue = TileEntity.ByID[mod.GetTileEntity<TESoulStatue>().Place(x, y)] as TESoulStatue;
                if (statue == null)
                    return false;
            }
            return true;
        }
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Statues, 12));
            TileObjectData.newTile.HookPlaceOverride =
                new PlacementHook(mod.GetTileEntity<TESoulStatue>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType<TaintedBrick>() };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.addTile(Type);
            dustType = -1;
            disableSmartCursor = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Soul Statue");
            AddMapEntry(new Color(80, 70, 100), name);

            soundType = -1;
        }

        internal const int CrackingPoint = 120;
        private int frameY = 0;

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (TryGetTE(i, j, out var statue))
                statue.active = true;
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            if (!TryGetTE(i, j, out var statue))
                return;

            // Set the frame
            if (statue.Crack <= 60)
                frameY = 0;
            if (statue.Crack > 60 && statue.Crack < 120)
                frameY = 1;
            if (statue.Crack >= 120 && statue.Crack < 240)
                frameY = 2;
            if (statue.Broken)
                frameY = 3;

            frameYOffset = frameY * 18 * 3;
        }

        public override bool CanExplode(int i, int j) => false;
        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            return false;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (frameX != 0 || frameY != 0)
            {
                return;
            }
            for (int index = 0; index < 7; index++)
            {
                Dust.NewDust(new Vector2(i * 16, j * 16), 32, 53, DustID.PurpleCrystalShard, Main.rand.NextFloat(), Main.rand.NextFloat(), Scale: 0.8f);
            }
            if (TryGetTE(i, j, out var statue)) statue.Kill(i, j);
        }
    }
}
