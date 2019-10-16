using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

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
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<TaintedBrick>() };
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
                Main.LocalPlayer.AddBuff(BuffType<CityBannerBuff>(), 300);
            }

            if (Main.rand.NextBool(16))
            {
                int dustInd = Dust.NewDust(new Vector2(i, j) * 16, 16, 16, DustType<NPCs.ErilipahBiome.VoidParticle>());

                Dust dust = Main.dust[dustInd];
                dust.noGravity = false;
                dust.velocity = Vector2.Zero;
                dust.customData = 0.05f;
            }
        }

        public override void SetSpriteEffects(int i, int j, ref Microsoft.Xna.Framework.Graphics.SpriteEffects spriteEffects)
        {
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
