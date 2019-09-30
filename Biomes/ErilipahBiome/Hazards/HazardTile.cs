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

            TileObjectData.newTile.FullCopyFrom(Style);
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
            drawColor *= 2;
        }
    }
}
