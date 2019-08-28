using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineDust : ModDust
    {
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y += 0.003f;
            else dust.velocity *= 0.7f;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 200, lightColor.G, lightColor.B + 200, 20);
        }
    }
}
