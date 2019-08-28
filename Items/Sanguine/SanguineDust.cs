using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Sanguine
{
    public class SanguineDust : ModDust
    {
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y -= 0.09f;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R, lightColor.G, lightColor.B);
        }
    }
}