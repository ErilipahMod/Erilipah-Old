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
                dust.scale = distance / 15f;
                dust.scale = MathHelper.Min(dust.scale, 0.82f);

                if (distance <= 1f + dust.velocity.Length())
                    dust.active = false;

                return false;
            }
            if (dust.customData is float timeLeft)
            {
                if (dust.scale > 1)
                    dust.scale = 1;
                dust.scale -= timeLeft;

                if (dust.scale < 0.1f)
                    dust.active = false;

                if (!dust.noGravity)
                    dust.velocity.Y += Main.rand.NextFloat(0.05f);
                dust.position += Collision.TileCollision(dust.position, dust.velocity, 2, 2);
                return false;
            }
            return true;
        }

        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y -= 0.09f;
            else dust.velocity *= 0.89f;
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 135, lightColor.G + 90, lightColor.B, 90);
        }
    }
}