using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Drone
{
    public class DeepFlames : ModDust
    {
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y -= 0.09f;
            else dust.velocity *= 0.89f;
            if (!dust.noLight)
            {
                float strength = dust.scale * 0.5f;
                if (strength > 2f)
                {
                    strength = 2f;
                }
                Lighting.AddLight(dust.position, strength, 0.82f * strength, 0);
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 100, lightColor.G + 80, lightColor.B, 30);
        }
    }
}