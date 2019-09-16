using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Items.ErilipahBiome
{
    public class CrystallineTorch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystalline Stick");
            Tooltip.SetDefault("Provides significant light while in Erilipah\nTakes twice as long to burn out");
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

            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.consumable = true;
            item.createTile = mod.TileType<CrystallineTorchTile>();

            item.value = 120;
        }

        internal static readonly Color light = new Color(1.7f, 0.3f, 2f);
        public override void HoldItem(Player player)
        {
            player.itemLocation.X -= 6 * player.direction;
            player.itemLocation.Y += 4;

            if (Main.rand.Next(player.itemAnimation > 0 ? 20 : 40) == 0)
            {
                if (Main.LocalPlayer.InErilipah())
                    Dust.NewDust(new Vector2(player.itemLocation.X + 12f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, mod.DustType<Crystalline.CrystallineDust>());
                else
                    Dust.NewDust(new Vector2(player.itemLocation.X + 12f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, mod.DustType<NPCs.ErilipahBiome.VoidParticle>());
            }
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

            if (Main.LocalPlayer.InErilipah()) // if I am in Erilipah, then light er up
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Lighting.AddLight(position + new Vector2(i, j) * 16, light.ToVector3());
                    }
                }
        }

        public override void PostUpdate()
        {
            if (!item.wet && Main.LocalPlayer.InErilipah())
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Lighting.AddLight(item.Center + new Vector2(i, j) * 20, light.ToVector3());
                    }
                }
            }
        }

        public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
        {
            dryTorch = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-5, 5);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Torch, 6);
            recipe.AddIngredient(mod.ItemType<Crystalline.CrystallineTileItem>(), 5);
            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 1);
            recipe.AddTile(mod.TileType<Biomes.ErilipahBiome.Tiles.Altar>());
            recipe.SetResult(this, 6);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Torch, 6);
            recipe.AddIngredient(mod.ItemType<BioluminescentSinew>(), 1);
            recipe.AddTile(mod.TileType<Biomes.ErilipahBiome.Tiles.Altar>());
            recipe.SetResult(this, 6);
            recipe.AddRecipe();
        }
    }
    public class CrystallineTorchTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileWaterDeath[Type] = true;

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
            dustType = mod.DustType<Crystalline.CrystallineDust>();
            drop = mod.ItemType<CrystallineTorch>();
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Torches };
            torch = true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = Main.rand.Next(1, 4);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];

            if (!Main.LocalPlayer.InErilipah() && tile.frameX < 66)
            {
                tile.frameX += 66;
            }
            if (Main.LocalPlayer.InErilipah() && tile.frameX >= 66)
            {
                tile.frameX -= 66;
            }

            if (tile.frameX < 66)
            {
                r = CrystallineTorch.light.R / 255f;
                g = CrystallineTorch.light.G / 255f;
                b = CrystallineTorch.light.B / 255f;

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        Lighting.AddLight(new Vector2(i, j) * 16 + new Vector2(x, y) * 16, CrystallineTorch.light.ToVector3());
                    }
                }
            }
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

        //public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        //{
        //    ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
        //    Color color = new Color(100, 100, 100, 0);
        //    int frameX = Main.tile[i, j].frameX;
        //    int frameY = Main.tile[i, j].frameY;
        //    int width = 20;
        //    int offsetY = 0;
        //    int height = 20;
        //    if (WorldGen.SolidTile(i, j - 1))
        //    {
        //        offsetY = 2;
        //        if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
        //        {
        //            offsetY = 4;
        //        }
        //    }
        //    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
        //    if (Main.drawToScreen)
        //    {
        //        zero = Vector2.Zero;
        //    }
        //    for (int k = 0; k < 7; k++)
        //    {
        //        float x = (float)Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
        //        float y = (float)Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
        //        Main.spriteBatch.Draw(mod.GetTexture("Tiles/ExampleTorch_Flame"), new Vector2((float)(i * 16 - (int)Main.screenPosition.X) - (width - 16f) / 2f + x, (float)(j * 16 - (int)Main.screenPosition.Y + offsetY) + y) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
        //    }
        //}
    }
}
