using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguineTileItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.value = Terraria.Item.sellPrice(0, 0, 7);
            item.useTurn = true;
            item.autoReuse = true;

            item.useAnimation = 15;
            item.useTime = 10;

            item.useStyle = 1;
            item.rare = 3;
            item.consumable = true;
            item.createTile = mod.TileType("SanguineTileTile");
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanguine Chunk");
            Tooltip.SetDefault("'Pretty but brittle'");
        }
    }
    public class SanguineTileTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;

            dustType = mod.DustType("Sanguine");
            drop = mod.ItemType("SanguineTileItem");

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Sanguine Ore");
            AddMapEntry(new Microsoft.Xna.Framework.Color(200, 20, 0));

            mineResist = 2f;
            minPick = 60;
            soundType = 21;
            soundStyle = 2;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => r = 0.1f;

        public override void NumDust(int i, int j, bool fail, ref int num)
            => num = fail ? 0 : 2;
    }
}
