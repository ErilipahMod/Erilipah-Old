using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Items.ErilipahBiome
{
    public class UnlitArkenTorch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unlit Arken Torch");
            Tooltip.SetDefault("Never snuffs in Erilipah\n'A lighter probably won't work'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Torch);

            item.flame = false;
            item.width = 20;
            item.height = 20;
            item.holdStyle = 1;

            item.noWet = true;
            item.useTurn = true;
            item.autoReuse = true;

            item.useStyle = 1;
            item.consumable = true;
            item.createTile = mod.TileType<ArkenTorchTile>();

            item.value = 0;
        }

        public override bool CanUseItem(Player player) => !player.wet;
    }

    public class ArkenTorch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arken Torch");
            Tooltip.SetDefault("Never snuffs in Erilipah\n'Don't stare at it, you'll go blind!'");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Torch);

            item.flame = true;
            item.width = 20;
            item.height = 20;
            item.holdStyle = 1;

            item.noWet = true;
            item.useTurn = true;
            item.autoReuse = true;

            item.useStyle = 1;
            item.consumable = true;
            item.createTile = mod.TileType<ArkenTorchTile>();

            item.value = 0;
        }

        public override bool CanUseItem(Player player) => !player.wet;

        internal static readonly Color light = new Color(2.5f, 1f, 2f);
        public override void HoldItem(Player player)
        {
            if (player.wet) return;

            player.itemLocation.X -= 4 * player.direction;
            player.itemLocation.Y += 4;

            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

            if (Main.LocalPlayer.InErilipah()) // if I am in Erilipah, then light er up
                for (int i = -2; i < 3; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Lighting.AddLight(position + new Vector2(i, j) * 20, light.ToVector3());
                    }
                }
            else
                Lighting.AddLight(position, light.ToVector3());
        }

        public override void PostUpdate()
        {
            if (item.wet) return;

            if (Main.LocalPlayer.InErilipah())
                for (int i = -2; i < 3; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Lighting.AddLight(item.Center + new Vector2(i, j) * 14, light.ToVector3());
                    }
                }
            else
                Lighting.AddLight(item.Center, light.ToVector3());
        }

        public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
        {
            dryTorch = true;
        }
    }
    public class ArkenTorchTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(2);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.addAlternate(0);
            TileObjectData.addTile(Type);

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Torch");
            AddMapEntry(new Color(200, 120, 215), name);
            drop = mod.ItemType<ArkenTorch>();
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Torches };
            torch = true;
        }

        private static bool IsLit(Tile tile) => tile.frameX < 66;

        public override bool CanPlace(int i, int j) => Main.tile[i, j].liquid == 0;
        public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(2, 4);
        public override void HitWire(int i, int j) { }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (item?.type == mod.ItemType<UnlitArkenTorch>())
            {
                Main.tile[i, j].frameX += 66;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            r = g = b = 0;

            if (IsLit(tile))
            {
                if (Main.LocalPlayer.InErilipah())
                    for (int x = -2; x < 3; x++)
                    {
                        for (int y = -2; y < 3; y++)
                        {
                            Lighting.AddLight(new Vector2(i, j) * 16 + new Vector2(x, y) * 14, ArkenTorch.light.ToVector3());
                        }
                    }
                else
                    Lighting.AddLight(new Vector2(i, j) * 16, ArkenTorch.light.ToVector3());
            }
        }


        public override bool Drop(int i, int j)
        {
            if (IsLit(Main.tile[i, j]))
            {
                drop = mod.ItemType<ArkenTorch>();
            }
            else
            {
                drop = mod.ItemType<UnlitArkenTorch>();
            }
            return true;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
        {
            offsetY = 0;
            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;
                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
            Color color = new Color(100, 100, 100, 0);
            int frameX = Main.tile[i, j].frameX;
            int frameY = Main.tile[i, j].frameY;
            int width = 20;
            int offsetY = 0;
            int height = 20;
            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;
                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }
            for (int k = 0; k < 7; k++)
            {
                float x = (float)Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                float y = (float)Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
                Main.spriteBatch.Draw(mod.GetTexture("Items/ErilipahBiome/ArkenTorchTile_Flame"), new Vector2((float)(i * 16 - (int)Main.screenPosition.X) - (width - 16f) / 2f + x, (float)(j * 16 - (int)Main.screenPosition.Y + offsetY) + y) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
