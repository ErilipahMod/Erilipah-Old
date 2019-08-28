using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    public class MoonFire : ModDust
    {
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y -= 0.25f;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(100, 100, 100, 100);
        }
    }
}