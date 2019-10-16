using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Crystalline
{
    public class CrystallineDust : ModDust
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
            if (dust.customData is float timer)
            {
                Projectile sigil = Main.projectile.FirstOrDefault(p => p.active && p.type == ProjectileType<AbProj>());
                if (sigil == null)
                {
                    dust.active = false;
                    return true;
                }

                dust.customData = (float)dust.customData + 1;
                if ((float)dust.customData < 100)
                {
                    dust.velocity *= 0.965f;
                }
                else
                {
                    dust.velocity += dust.position.To(sigil.Center - Vector2.UnitY * 16, 1 / 8f);
                    dust.scale = Vector2.Distance(dust.position, sigil.Center) / 70f;
                    if (dust.scale > 1)
                        dust.scale = 1;
                }

                if (timer > 130 && Vector2.Distance(sigil.Center - Vector2.UnitY * 16, dust.position) < 16)
                    dust.active = false;

                dust.position += dust.velocity;
                dust.noGravity = true;
                return false;
            }
            if (dust.customData is double portion)
            {
                Projectile sigil = Main.projectile.FirstOrDefault(p => p.active && p.frame == 1 && p.type == ProjectileType<AbProj>());
                if (sigil == null)
                {
                    dust.active = false;
                    return true;
                }

                dust.position = Vector2.Lerp(Main.LocalPlayer.Center, ErilipahWorld.AltarPosition - new Vector2(0, 100), (float)portion);
                dust.rotation += 0.1f;

                if (Vector2.Distance(Main.LocalPlayer.Center, sigil.Center) < 150)
                {
                    dust.customData = 0; // Stop tracking and start falling bich
                }
                return false;
            }
            return true;
        }

        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity) dust.velocity.Y += 0.006f;
            else dust.velocity *= 0.88f;

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(lightColor.R + 200, lightColor.G, lightColor.B + 200, 20);
        }
    }
}
