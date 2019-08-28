using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class GreenGemDust : ModDust
    {
        private const float scaleDecrease = 0.005f;
        private const float scaleKill = 0.30f;
        public override bool MidUpdate(Dust dust)
        {
            dust.scale -= scaleDecrease;
            if (dust.scale < 0.50f)
            {
                dust.active = false;
            }
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.01f;
            }
            else
            {
                dust.velocity.Y -= 0.01f;
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R, lightColor.G, lightColor.B, 0);
        }
    }
}
