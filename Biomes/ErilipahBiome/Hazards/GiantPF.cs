using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public class GiantPF : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSpelunker[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Giant Pure Flower");
            AddMapEntry(new Color(60, 30, 70), name);
        }

        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (!WorldGen.gen && Main.netMode != 1)
            {
                Item.NewItem(i - frameX / 18, j - frameY / 18, 36, 54, mod.ItemType<PureFlower>(), 2);
                Item.NewItem(i - frameX / 18, j - frameY / 18, 36, 54, mod.ItemType<PureFlower>(), 1);
                Item.NewItem(i - frameX / 18, j - frameY / 18, 36, 54, mod.ItemType<PureFlower>(), Main.rand.Next(3));
            }
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Helper.Invisible;
            return true;
        }
    }
}