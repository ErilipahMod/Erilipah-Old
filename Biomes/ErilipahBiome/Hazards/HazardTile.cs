using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    public abstract class HazardTile : ModTile
    {
        public abstract string MapName { get; }
        public abstract int DustType { get; }
        public abstract TileObjectData Style { get; }
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(Style);
            TileObjectData.addTile(Type);

            minPick = 101;
            dustType = DustType;
            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault(MapName);
            AddMapEntry(new Color(60, 30, 70), name);
        }

        public override bool CanExplode(int i, int j) => false;
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            const int min = 100;
            drawColor *= 2;

            if (drawColor.R < min) drawColor.R = min;
            if (drawColor.G < min) drawColor.G = min;
            if (drawColor.B < min) drawColor.B = min;
            if (drawColor.A < min) drawColor.A = min;
        }

        private float fluctuant = 0;
        protected void Light(float intensity, ref float r, ref float g, ref float b)
        {
            fluctuant += 0.0225f;
            if (fluctuant > 1)
                fluctuant = -1;

            float absFluctuant = Math.Abs(fluctuant);
            r = MathHelper.SmoothStep(-1f, 1f * intensity, absFluctuant);
            g = MathHelper.SmoothStep(-0.8f, 0.8f * intensity, absFluctuant);
            b = MathHelper.SmoothStep(-1f, 1f * intensity, absFluctuant);
        }
    }
}
