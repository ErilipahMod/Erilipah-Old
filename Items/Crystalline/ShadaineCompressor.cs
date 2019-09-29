using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Items.Crystalline
{
    public class ShadaineCompressor : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Void Compressor");
            Tooltip.SetDefault("Forges Erilipian items");
        }

        public override void SetDefaults()
        {
            item.width = 48;
            item.height = 34;
            item.maxStack = 99;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.consumable = true;
            item.value = 150;
            item.createTile = mod.TileType("ShadaineCompressorTile");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddTile(TileID.WorkBenches);
            recipe.AddRecipeGroup("Erilipah:EvilBars", 6);
            recipe.AddIngredient(mod, "CrystallineTileItem", 6);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class ShadaineCompressorTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolidTop[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.Height = 2;
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Void Compressor");
            AddMapEntry(new Color(100, 0, 200), name);

            disableSmartCursor = true;
            animationFrameHeight = 36;
            dustType = mod.DustType<CrystallineDust>();
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.73f;
            g = 0.01f;
            b = 0.92f;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) =>
            Item.NewItem(i * 16, j * 16, 32, 16, mod.ItemType("ShadaineCompressor"));

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter % 5 == 0)
            {
                frame += 1;
                frame %= 4;
            }
        }
        /*public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            // Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting.
            int uniqueAnimationFrame = Main.tileFrame[Type];

            frameXOffset = uniqueAnimationFrame * 48;
        }*/
    }
}