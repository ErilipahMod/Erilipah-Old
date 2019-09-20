using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonDust : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is Vector2 toPos)
            {
                dust.velocity = (toPos - dust.position).SafeNormalize(Vector2.Zero) * dust.velocity.Length();
                dust.position += dust.velocity;

                float distance = (toPos - dust.position).Length();
                dust.scale = distance / 10f;
                dust.scale = MathHelper.Min(dust.scale, 1f);

                if (distance <= 1f + dust.velocity.Length())
                    dust.active = false;

                return false;
            }
            return true;
        }

        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y -= 0.09f;
            else dust.velocity *= 0.9f;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 100, lightColor.G + 80, lightColor.B, lightColor.A + 30);
        }
    }
}