using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class VoidParticle : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is Vector3 position)
            {
                if (Vector2.Distance(dust.position, new Vector2(position.X, position.Y)) > position.Z)
                {
                    dust.velocity = Vector2.Zero;
                    dust.alpha += 6;
                    if (dust.alpha >= 255)
                        dust.active = false;
                }
            }
            else if (dust.customData is float gravity)
            {
                dust.noGravity = true;
                dust.velocity.X *= 0.9f;
                dust.velocity.Y += gravity;
                dust.alpha += 6;
                if (dust.alpha >= 255)
                    dust.active = false;
            }
            else if (dust.customData is int type)
            {
                switch (type)
                {
                    case 0:
                        dust.velocity = Vector2.Zero;
                        dust.scale -= 0.0175f;
                        if (dust.scale < 0.1f)
                            dust.active = false;
                        break;
                    case 1:
                        dust.velocity = Vector2.Zero;
                        dust.scale -= 0.030f;
                        if (dust.scale < 0.1f)
                            dust.active = false;
                        break;
                }
            }
            else
            {
                dust.alpha += 7;
                if (dust.alpha >= 255)
                    dust.active = false;
            }

            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(50, 50, 50, 255);
        }
    }
}
