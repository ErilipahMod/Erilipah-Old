using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.ErilipahBiome.Potions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    public class ErilipahPot : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Pustule");
            AddMapEntry(new Color(50, 30, 60), name);

            soundType = 13;
            soundStyle = 0;
        }

        public override bool Dangersense(int i, int j, Player player) => true;
        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (!WorldGen.gen && Main.netMode != 1)
            {
                Rectangle area = new Rectangle(i * 16, j * 16, 32, 32);
                switch (Main.rand.Next(12))
                {
                    case 0:
                        Loot.DropItem(area, mod.ItemType<ReductionPot>());
                        break;
                    case 1:
                        Loot.DropItem(area, mod.ItemType<SlowingPot>());
                        break;
                    case 2:
                        Loot.DropItem(area, mod.ItemType<EffulgencePot>());
                        break;
                    case 3:
                        Loot.DropItem(area, ItemID.SilverCoin, 1, 12, 100, 2);
                        Loot.DropItem(area, ItemID.CopperCoin, 1, 100, 100, 1);
                        break;
                    case 4:
                        Loot.DropItem(area, ItemID.HealingPotion);
                        break;
                    case 5:
                        Loot.DropItem(area, ItemID.ShinePotion);
                        break;
                    case 6:
                        Loot.DropItem(area, ItemID.Heart);
                        break;
                    case 7: goto case 3;
                    case 8:
                        Loot.DropItem(area, ItemID.Heart, 2, 3, 100, 1);
                        break;
                    case 9: goto case 5;
                    case 10: goto case 3;
                    case 11:
                        Loot.DropItem(area, mod.ItemType<MadnessFocus>(), 2, 5, 100, 1.75f);
                        break;
                }
            }
        }
    }
}