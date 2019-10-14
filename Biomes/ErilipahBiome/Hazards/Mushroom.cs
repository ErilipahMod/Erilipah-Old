using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Mushroom : HazardTile
    {
        public override string MapName => "Mushroom";
        public override int DustType => mod.DustType<FlowerDust>();
        public override TileObjectData Style
        {
            get
            {
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
                TileObjectData.newTile.Height = 1;
                TileObjectData.newTile.CoordinateHeights = new[] { 16 };
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.AnchorValidTiles = new int[]
                { mod.TileType<InfectedClump>(), mod.TileType<SpoiledClump>() };
                TileObjectData.newTile.AnchorBottom = new AnchorData(
                    AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
                TileObjectData.newTile.AnchorBottom = new AnchorData(
                    AnchorType.SolidBottom | AnchorType.SolidTile, TileObjectData.newTile.Width, 0);

                return TileObjectData.newTile;
            }
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Main.tileCut[Type] = true;
            soundType = 3;
            soundStyle = 1;
            drop = mod.ItemType<MushroomItem>();
        }

        public static bool TryPlace(int i, int j)
        {
            int mushType = Erilipah.Instance.TileType<Mushroom>();
            int clumpType = Erilipah.Instance.TileType<InfectedClump>();
            int spoilType = Erilipah.Instance.TileType<SpoiledClump>();

            bool rightSlope = Main.tile[i, j + 1].slope() == 0 || Main.tile[i, j + 1].topSlope();
            rightSlope |= Main.tile[i, j - 1].slope() == 0 || Main.tile[i, j - 1].bottomSlope();

            bool rightType = Main.tile[i, j + 1].active() && (Main.tile[i, j + 1].type == clumpType || Main.tile[i, j + 1].type == spoilType);
            rightType |= Main.tile[i, j - 1].active() && (Main.tile[i, j - 1].type == clumpType || Main.tile[i, j - 1].type == spoilType);

            if (!Main.tile[i, j].active() && rightType && rightSlope)
            {
                Main.tile[i, j].type = (ushort)mushType;
                Main.tile[i, j].active(true);
                Main.tile[i, j].frameX = (short)(Main.rand.Next(5) * 18);
                Main.tile[i, j].frameY = 0;
                return true;
            }
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            for (int m = -1; m <= 1; m++)
            {
                bool left = Main.tile[i + 1, j + m].active();
                int n = left ? -1 : 1;

                if (TryPlace(i + n, j + m))
                    break;
            }
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Main.tile[i, j].frameX = (short)(Main.rand.Next(5) * 18);
            if (Main.netMode == 2)
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
        }

        public override bool KillSound(int i, int j)
        {
            Main.PlaySound(SoundID.NPCHit, i * 16, j * 16, 19, 1, 0.2f);
            return false;
        }

        public override void SetSpriteEffects(int i, int j, ref Microsoft.Xna.Framework.Graphics.SpriteEffects spriteEffects)
        {
            if ((i + j) % 2 == 0)
                spriteEffects |= Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;

            if (!Main.tile[i, j + 1].active() && Main.tile[i, j - 1].active())
            {
                spriteEffects |= Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            }
        }

        public override void DrawEffects(int i, int j, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 1.2f;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            for (int h = 0; h < 7; h++)
            {
                float rotation = h / 7f * MathHelper.Pi + MathHelper.Pi;
                Dust.NewDustPerfect(new Vector2(i * 16 + 16, j * 16), mod.DustType<FlowerDust>(), rotation.ToRotationVector2() * 5, Scale: 1.8f).noGravity = true;
            }
        }
    }

    public class MushroomItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infected Fungus");
            Tooltip.SetDefault("'It smells horrible...'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Mushroom);

            item.maxStack = 999;

            item.healLife = 0;
            item.potion = true;
            item.consumable = true;

            item.width = 26;
            item.height = 32;

            item.value = 10;
        }

        public override bool ConsumeItem(Player player)
        {
            item.consumable = true;

            player.immune = false;
            player.immuneTime = 0;
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " thought eating wild shrooms was a good idea."), 60, 0);
            player.I().Infect(5f);
            return true;
        }
    }
}
