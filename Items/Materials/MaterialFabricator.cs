using Erilipah.Items.LunarBee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Materials
{
    public class MaterialFabricatorItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 16;
            item.maxStack = 999;
            item.value = Terraria.Item.sellPrice(0, 0, 5);
            item.useTurn = true;
            item.autoReuse = true;

            item.useAnimation = 15;
            item.useTime = 10;

            item.useStyle = 1;
            item.rare = Terraria.ID.ItemRarityID.White;
            item.consumable = true;
            item.createTile = mod.TileType("MaterialFabricatorTile");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddTile(TileID.WorkBenches);
            recipe.AddIngredient(ItemID.Furnace, 1);
            recipe.AddRecipeGroup("IronBar", 10);
            recipe.AddRecipeGroup("Erilipah:SilverBars", 2);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Material Fabricator");
            Tooltip.SetDefault("Acts as a furnace" +
                "\nUsed for forging alloys, fabricating new materials, and transmuting minerals");
        }
    }
    public class MaterialFabricatorTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.Height = 2;
            TileObjectData.addTile(Type);
            adjTiles = new int[1] { TileID.Furnaces };
            dustType = DustType<MoonFire>();

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Material Fabricator");
            AddMapEntry(new Color(142, 255, 255), name);

            disableSmartCursor = true;
            animationFrameHeight = 36;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.55f;
            g = 1;
            b = 1;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) =>
            Item.NewItem(i * 16, j * 16, 32, 16, mod.ItemType("MaterialFabricatorItem"));

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter % 4 == 0)
            {
                frame += 1;
                frame %= 17;
            }
        }
        /*public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            // Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting.
            int uniqueAnimationFrame = Main.tileFrame[Type];

            frameXOffset = uniqueAnimationFrame * 48;
        }*/
    }
    public class ConversionToken : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.maxStack = 999;
            item.value = Terraria.Item.buyPrice(0, 0, 2);
            item.rare = 1;
        }
        // CHECK GLOBAL NPC FOR HOW TO OBTAIN
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Allows you to transmute minerals of the same tier" +
                "\nUse with a Material Fabricator and metal of your choice to transmute it" +
                "\nWorks with 'alternative minerals,' like Copper and Tin, or Iron and Lead");
        }

        public override void AddRecipes()
        {
            int[,] bars = { { ItemID.CopperOre, ItemID.TinOre }, { ItemID.IronOre, ItemID.LeadOre },
                { ItemID.SilverOre, ItemID.TungstenOre}, { ItemID.GoldOre, ItemID.PlatinumOre },
                { ItemID.DemoniteOre, ItemID.CrimtaneOre }, { ItemID.CobaltOre, ItemID.PalladiumOre },
                { ItemID.MythrilOre, ItemID.OrichalcumOre }, { ItemID.AdamantiteOre, ItemID.TitaniumOre },
                { ItemID.ShadowScale, ItemID.TissueSample } };

            for (int i = 0; i < bars.GetLength(0); i++)
            {
                ModRecipe r = new ModRecipe(mod);
                r.AddIngredient(itemID: bars[i, 0], stack: 3);
                r.AddIngredient(itemID: this.item.type, stack: 1);
                r.AddTile(mod.TileType("MaterialFabricatorTile"));
                r.SetResult(itemID: bars[i, 1], stack: 2);
                r.AddRecipe();

                r = new ModRecipe(mod);
                r.AddIngredient(itemID: bars[i, 1], stack: 2);
                r.AddIngredient(itemID: this.item.type, stack: 1);
                r.AddTile(mod.TileType("MaterialFabricatorTile"));
                r.SetResult(itemID: bars[i, 0], stack: 3);
                r.AddRecipe();
            }
        }
    }
    public class DowngradeToken : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.maxStack = 999;
            item.value = Terraria.Item.buyPrice(0, 0, 4);
            item.rare = 1;
        }
        // CHECK GLOBAL NPC FOR HOW TO OBTAIN
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Allows you to transmute minerals to a lower tier" +
                "\nUse with a Material Fabricator and metal of your choice to transmute it" +
                "\nWorks with ores of the same 'type', like Copper and Iron, or Tin and Lead");
        }

        public override void AddRecipes()
        {
            int[] bars =
            {
                ItemID.CopperOre, ItemID.IronOre, ItemID.SilverOre, ItemID.GoldOre,
                ItemID.CobaltOre, ItemID.MythrilOre, ItemID.AdamantiteOre
            };
            int[] altOres =
            {
                ItemID.TinOre, ItemID.LeadOre, ItemID.TungstenOre, ItemID.PlatinumOre,
                ItemID.PalladiumOre, ItemID.OrichalcumOre, ItemID.TitaniumOre
            };

            for (int i = 0; i < bars.Length - 1; i++)
            {
                ModRecipe r = new ModRecipe(mod);
                r.AddIngredient(itemID: bars[i + 1], stack: 1);
                r.AddIngredient(itemID: this.item.type, stack: 1);
                r.AddTile(mod.TileType("MaterialFabricatorTile"));
                r.SetResult(itemID: bars[i], stack: 4);
                r.AddRecipe();
            }

            for (int i = 0; i < altOres.Length - 1; i++)
            {
                ModRecipe r = new ModRecipe(mod);
                r.AddIngredient(itemID: altOres[i + 1], stack: 1);
                r.AddIngredient(itemID: this.item.type, stack: 1);
                r.AddTile(mod.TileType("MaterialFabricatorTile"));
                r.SetResult(itemID: altOres[i], stack: 4);
                r.AddRecipe();
            }
        }
    }
    public class UpgradeToken : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.maxStack = 999;
            item.value = Terraria.Item.buyPrice(0, 0, 4);
            item.rare = 1;
        }
        // CHECK GLOBAL NPC FOR HOW TO OBTAIN
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Allows you to transmute minerals to a higher tier" +
                "\nUse with a Material Fabricator and metal of your choice to transmute it" +
                "\nWorks with ores of the same 'type', like Copper and Iron, or Tin and Lead");
        }

        public override void AddRecipes()
        {
            int[] bars =
            {
                ItemID.CopperOre, ItemID.IronOre, ItemID.SilverOre, ItemID.GoldOre,
                ItemID.CobaltOre, ItemID.MythrilOre, ItemID.AdamantiteOre
            };
            int[] altOres =
            {
                ItemID.TinOre, ItemID.LeadOre, ItemID.TungstenOre, ItemID.PlatinumOre,
                ItemID.PalladiumOre, ItemID.OrichalcumOre, ItemID.TitaniumOre
            };

            for (int i = 0; i < bars.Length - 1; i++)
            {
                if (i != 4)
                {
                    ModRecipe r = new ModRecipe(mod);
                    r.AddIngredient(itemID: bars[i], stack: 4);
                    r.AddIngredient(itemID: this.item.type, stack: 1);
                    r.AddTile(mod.TileType("MaterialFabricatorTile"));
                    r.SetResult(itemID: bars[i + 1], stack: 1);
                    r.AddRecipe();
                }
            }
            for (int i = 0; i < altOres.Length - 1; i++)
            {
                if (i != 4)
                {
                    ModRecipe r = new ModRecipe(mod);
                    r.AddIngredient(itemID: altOres[i], stack: 4);
                    r.AddIngredient(itemID: this.item.type, stack: 1);
                    r.AddTile(mod.TileType("MaterialFabricatorTile"));
                    r.SetResult(itemID: altOres[i + 1], stack: 1);
                    r.AddRecipe();
                }
            }
        }
    }
}