using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineTileItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 18;
            item.maxStack = 999;
            item.value = Terraria.Item.sellPrice(0, 0, 3);
            item.useTurn = true;
            item.autoReuse = true;

            item.useAnimation = 15;
            item.useTime = 10;

            item.useStyle = 1;
            item.rare = Terraria.ID.ItemRarityID.Green;
            item.consumable = true;
            item.createTile = TileType<CrystallineTileTileFromItem>();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystalline Shards");
            Tooltip.SetDefault("'Beautiful, but they hurt to look at.'");
        }
    }
    public class CrystallineTileTileFromItem : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;

            dustType = DustType<CrystallineDust>();
            drop = ItemType<CrystallineTileItem>();

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Crystalline Shards");
            AddMapEntry(new Microsoft.Xna.Framework.Color(120, 20, 90), name);

            mineResist = 1;
            minPick = 65;
            soundType = 2;
            soundStyle = 27;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => g = 0.1f;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 2;
    }
    public class CrystallineTileTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;

            Main.tileMerge[Type][TileType<Biomes.ErilipahBiome.Tiles.InfectedClump>()] = true;

            dustType = DustType<CrystallineDust>();
            drop = ItemType<CrystallineTileItem>();

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Crystalline Shards");
            AddMapEntry(new Microsoft.Xna.Framework.Color(120, 20, 90), name);

            mineResist = 1;
            minPick = 65;
            soundType = 2;
            soundStyle = 27;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => g = 0.1f;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 0 : 2;

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Loot.DropItem(new Rectangle(i * 16, j * 16, 16, 16), ItemType<CrystallineTileItem>(), 1, 3);
        }
    }
}