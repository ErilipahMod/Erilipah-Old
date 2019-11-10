using Microsoft.Xna.Framework;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.UI.Infection
{
    internal class InfectionBarDust : OverlayParticle
    {
        public InfectionBarDust(Vector2 pos, Vector2 vel, Vector2 accel, float scale, int biomeType) : base(GetTexture("Erilipah/Biomes/ErilipahBiome/Hazards/Gas"))
        {
            position = pos;
            velocity = vel;
            acceleration = accel;
            this.scale = scale;

#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (biomeType)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                default:
                    color = Color.Lerp(Color.White, Color.Pink, Main.rand.NextFloat());
                    break;
                case 1:
                    color = Color.Lerp(Color.White, Color.Lime, Main.rand.NextFloat());
                    break;
                case 2:
                    color = Color.Lerp(Color.White, Color.Crimson, Main.rand.NextFloat());
                    break;
            }

            rotation = Main.rand.NextFloat(6.24f);
        }

        public override void Update()
        {
            base.Update();

            scale -= 0.018f;
            color.A -= 7;
            if (color.A <= 5)
                Active = false;
        }
    }
}
