using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class Mushroom : ModTile
    {
        public override void SetDefaults()
        {
            soundType = 3;
            soundStyle = 1;
            dustType = DustType<FlowerDust>();
            drop = ItemType<MushroomItem>();
            disableSmartCursor = true;

            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLavaDeath[Type] = true;

            AddMapEntry(new Color(60, 18, 80));

            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = -1;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = true;
            //TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.MushroomPlants, 0));
            TileObjectData.newTile.AnchorValidTiles = new[]
            {
                TileType<InfectedClump>(),
                TileType<SpoiledClump>()
            };

            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorLeft = TileObjectData.newTile.AnchorRight = AnchorData.Empty;

            TileObjectData.addTile(Type);
        }

        private static bool TileIsBase(int i, int j) 
        {
            Tile t = Main.tile[i, j];
            return Main.tileSolid[t.type] && t.type != TileType<TaintedBrick>() && t.active() && t.blockType() == 0;
        }

        public static bool TryPlace(int i, int j)
        {
            bool isTaken = Main.tile[i, j].active();

            if (isTaken || !TileIsBase(i, j - 1) && !TileIsBase(i, j + 1))
                return false;

            Main.tile[i, j].active(true);
            Main.tile[i, j].type = (ushort)TileType<Mushroom>();
            Main.tile[i, j].frameX = (short)(Main.rand.Next(5) * 18);
            Main.tile[i, j].frameY = 0;
            return true;
        }

        public override void RandomUpdate(int i, int j)
        {
            // Once in a while, spread a 'shroom
            if (Main.rand.Chance(0.10f))
                for (int n = -1; n <= 1; n++)
                    for (int m = -1; m <= 1; m++)
                    {
                        if (n == 0 && m == 0)
                            continue;
                        if (TryPlace(i + n, j + m))
                            return;
                    }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!Main.tile[i, j - 1].active() && !Main.tile[i, j + 1].active())
            {
                WorldGen.KillTile(i, j);
            }
            noBreak = true;
            resetFrame = false;
            return false;
        }

        public override bool Drop(int i, int j)
        {
            return Main.rand.NextBool(5);
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

            if (!TileIsBase(i, j + 1) && TileIsBase(i, j - 1))
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
            bool isUpsideDown = !TileIsBase(i, j + 1) && TileIsBase(i, j - 1);
            for (int h = 0; h <= 7; h++)
            {
                float rotation = h / 7f * MathHelper.Pi + (isUpsideDown ? 0 : MathHelper.Pi);
                Dust.NewDustPerfect(new Vector2(i * 16 + 16, j * 16), DustType<FlowerDust>(), rotation.ToRotationVector2() * 5, Scale: 1.8f).noGravity = true;
            }
        }
    }

    public class MushroomItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tainted Shroom");
            Tooltip.SetDefault("'It smells horrible...'\nBUFF SPRITE NEEDED @moonburn");
        }

        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.useStyle = ItemUseStyleID.EatingUsing;
            item.UseSound = SoundID.Item2;

            item.useTime = 60;
            item.useAnimation = 40;
            item.consumable = true;

            item.width = 26;
            item.height = 32;

            item.value = 10;

            item.buffType = BuffType<Hallucinating>();
            item.buffTime = 900;
        }

        public override void OnConsumeItem(Player player)
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " thought eating wild shrooms was a good idea."), player.statDefense / 2 + 60, 0);
            player.I().Infect(5f);
        }
    }

    public class Hallucinating : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "Erilipah/Debuff";
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Hallucinating");
            Description.SetDefault("Something is clearly wrong 🎵");
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen = System.Math.Min(player.lifeRegen, 0);
        }
    }
}
