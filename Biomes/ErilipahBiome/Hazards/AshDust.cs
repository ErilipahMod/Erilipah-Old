using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Biomes.ErilipahBiome.Hazards
{
    class AshDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.customData = Main.rand.NextFloat(1, 5f);
            dust.scale = MathHelper.Lerp(0.6f, 1.5f, ((float)dust.customData - 1f) / 5f);
        }

        public override bool MidUpdate(Dust dust)
        {
            Vector2 velocity = new Vector2(Main.windSpeed * 2, (float)dust.customData);
            if ((float)dust.customData > 2.35f)
                velocity = Collision.TileCollision(dust.position, velocity, 2, 2);

            if (velocity.Y < 0.15f || MathHelper.Distance(Main.windSpeed, velocity.X) > 0.2f)
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
