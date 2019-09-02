using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Erilipah.Items.ErilipahBiome.Aboryc;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineDust : ModDust
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is float timer)
            {
                Projectile sigil = Main.projectile.FirstOrDefault(p => p.active && p.type == mod.ProjectileType<AbProj>());
                if (sigil == null)
                {
                    dust.active = false;
                    return true;
                }

                dust.customData = (float)dust.customData + 1;
                if ((float)dust.customData < 120)
                    dust.velocity *= 0.96f;
                else
                    dust.velocity += dust.position.To(sigil.Center - Vector2.UnitY * 10, 1 / 10f);

                if (timer > 130 && Vector2.Distance(sigil.Center - Vector2.UnitY * 10, dust.position) < 16)
                    dust.active = false;

                dust.position += dust.velocity;
                dust.noGravity = true;
                return false;
            }
            return true;
        }

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
