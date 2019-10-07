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

            if (dust.scale > 0.8f)
                dust.scale -= 0.025f;
            else if (dust.alpha < 200)
                dust.alpha += 2;
            else
                dust.active = false;

            dust.position += dust.velocity;

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White * 0.55f * ((255 - dust.alpha) / 255f);
        }
    }
}