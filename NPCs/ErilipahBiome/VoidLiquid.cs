using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class VoidLiquid : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (!dust.noGravity)
                dust.velocity.Y += 0.06f;
            dust.position += Collision.TileCollision(dust.position, dust.velocity, 2, 2);

            if (dust.scale > 0.8f)
                dust.scale -= 0.01f;
            else if (dust.alpha < 200)
                dust.alpha++;
            else
                dust.active = false;

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White * 0.85f;
        }
    }
}