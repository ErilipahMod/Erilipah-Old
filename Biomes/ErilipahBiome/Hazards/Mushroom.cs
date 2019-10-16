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
    public class Mushroom : ModTile
    {
        public override void SetDefaults()
        {
            soundType = 3;
            soundStyle = 1;
            dustType = mod.DustType<FlowerDust>();
            drop = mod.ItemType<MushroomItem>();
            disableSmartCursor = true;

            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLavaDeath[Type] = true;

            AddMapEntry(new Color(60, 18, 80));

            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[]{ 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = -1;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = true;
            //TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.MushroomPlants, 0));
            TileObjectData.newTile.AnchorValidTiles = new[]
            {
                mod.TileType<InfectedClump>(),
                mod.TileType<SpoiledClump>()
            };

            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorLeft = TileObjectData.newTile.AnchorRight = AnchorData.Empty;

            TileObjectData.addTile(Type);
        }

        public static bool TryPlace(int i, int j)
        {
            int typePrevious = Main.tile[i, j].type;
            int type = Erilipah.Instance.TileType<Mushroom>();
            WorldGen.Place1x1(i, j, type, 0);
            if (type != typePrevious)
            {
                Main.tile[i, j].frameX =(short)(Main.rand.Next(5) * 18);
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
            return Main.rand.NextBool(4);
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
            Tooltip.SetDefault("'It smells horrible...'\nBUFF SPRITE NEEDED @moonburn");
        }

        public override void SetDefaults()
        {
            item.maxStack = 999;

            item.useStyle = ItemUseStyleID.EatingUsing;
            item.UseSound = SoundID.Item2;

            item.useTime = 30;
            item.useAnimation = 40;
            item.consumable = true;
            item.potion = true;

            item.width = 26;
            item.height = 32;

            item.value = 10;

            item.buffType = mod.BuffType<Hallucinating>();
            item.buffTime = 900;
        }

        public override bool ConsumeItem(Player player)
        {
            player.immune = false;
            player.immuneTime = 0;
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " thought eating wild shrooms was a good idea."), 60, 0);
            player.I().Infect(5f);
            return true;
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
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen = System.Math.Min(player.lifeRegen, 0);
        }
    }
}
