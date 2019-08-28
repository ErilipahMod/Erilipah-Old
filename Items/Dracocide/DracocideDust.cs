using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Dracocide
{
    public class DracocideDust : ModDust
    {

        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, Main.rand.Next(4) * 12, 10, 12);
            dust.customData = 180;
        }
        public override bool Update(Dust dust)
        {
            if (dust.fadeIn == 0)
                dust.fadeIn = 5;
            dust.customData = (int)dust.customData - 1;
            dust.rotation += (int)dust.customData / 500f;
            dust.position += dust.velocity;

            if ((int)dust.customData < 1)
                dust.alpha += (int)dust.fadeIn;

            if (dust.alpha >= 255)
            {
                dust.alpha = 255;
                dust.active = false;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position, 0.125f, 0.035f, 0);

            return false;
        }
    }
}
