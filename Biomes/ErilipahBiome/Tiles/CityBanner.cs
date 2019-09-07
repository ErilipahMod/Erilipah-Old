using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    public class CityBanner : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorValidTiles = new int[] { mod.TileType<TaintedBrick>() };
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.StyleWrapLimit = 111;
            TileObjectData.addTile(Type);
            dustType = DustID.PurpleCrystalShard;
            disableSmartCursor = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("City Banner");
            AddMapEntry(new Color(100, 20, 100), name);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.8f;
            g = 0.1f;
            b = 1.2f;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                Main.LocalPlayer.AddBuff(mod.BuffType<CityBannerBuff>(), 300);
            }
        }

        public override void SetSpriteEffects(int i, int j, ref Microsoft.Xna.Framework.Graphics.SpriteEffects spriteEffects)
        {
            Tile tile = Main.tile[i, j];
            if ((i - tile.frameX / 18) % 2 == 0)
                spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
        }
    }
    public class CityBannerBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("City Banner");
            Description.SetDefault("The Ark's warmth helps slow your infection");
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.I().reductionRate *= 0.50f;
        }
    }
}
