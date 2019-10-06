using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    internal class AshDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.customData = Main.rand.NextFloat(1, 5f);
            dust.scale = MathHelper.Lerp(0.6f, 1.5f, ((float)dust.customData - 1f) / 5f);
        }

        public override bool Update(Dust dust)
        {
            dust.scale -= 0.006f;
            dust.position += dust.velocity;
            if (dust.scale <= 0.1f)
                dust.active = false;
            return false;
        }

        public override bool MidUpdate(Dust dust)
        {
            if (dust.velocity.X < Math.Abs(Main.windSpeed) * 2)
                dust.velocity.X += Main.windSpeed / 30;
            if (dust.velocity.Y < (float)dust.customData)
                dust.velocity.Y += 0.0025f;

            if (dust.velocity.Y > 0 && (float)dust.customData > 2.35f)
                dust.velocity = Collision.TileCollision(dust.position, dust.velocity, 2, 2);

            if (dust.velocity.Y < 0.15f || Math.Abs(Main.windSpeed - dust.velocity.X) > 0.2f)
                dust.rotation += Main.windSpeed / 2f;
            else
                dust.rotation += Main.windSpeed;

            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(255, 255, 255, 255);
        }
    }
}
