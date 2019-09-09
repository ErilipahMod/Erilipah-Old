using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Tiles
{
    internal class Altar : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.addTile(Type);
            dustType = -1;
            disableSmartCursor = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Altar");
            AddMapEntry(new Color(70, 30, 60), name);

            soundType = -1;
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged)
        {
            return false;
        }
        public override bool CanExplode(int i, int j)
        {
            return false;
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            drawColor *= 3;

            if (drawColor.R < 100) drawColor.R = 100;
            if (drawColor.G < 100) drawColor.G = 100;
            if (drawColor.B < 100) drawColor.B = 100;
            if (drawColor.A < 100) drawColor.A = 100;
        }

        private float fluctuant = 0;
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            fluctuant += 0.025f;
            if (fluctuant > 1)
                fluctuant = -1;

            float absFluctuant = System.Math.Abs(fluctuant);
            r = MathHelper.SmoothStep(0, 2f, absFluctuant);
            g = MathHelper.SmoothStep(0, 1f, absFluctuant);
            b = MathHelper.SmoothStep(0, 3.5f, absFluctuant);
        }
    }
}
