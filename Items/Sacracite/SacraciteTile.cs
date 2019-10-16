using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sacracite
{
    public class SacraciteTileItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 26;
            item.height = 22;
            item.maxStack = 999;
            item.value = Terraria.Item.sellPrice(0, 0, 7);
            item.useTurn = true;
            item.autoReuse = true;

            item.useAnimation = 15;
            item.useTime = 10;

            item.useStyle = 1;
            item.rare = Terraria.ID.ItemRarityID.Green;
            item.consumable = true;
            item.createTile = mod.TileType("SacraciteTileTile"); //put your CustomBlock Tile name
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sacracite Ore");
            Tooltip.SetDefault("'Deposits of sandy crystals, coalesced'");
        }
    }
    public class SacraciteTileTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;

            dustType = DustType<GreenGemDust>();
            drop = mod.ItemType("SacraciteTileItem");

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Sacracite Ore");
            AddMapEntry(new Microsoft.Xna.Framework.Color(20, 180, 95), name);

            mineResist = 2f;
            minPick = 65;
            soundType = 21;
            soundStyle = 2;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => g = 0.1f;

        public override void NumDust(int i, int j, bool fail, ref int num)
            => num = fail ? 0 : 2;
    }
}
