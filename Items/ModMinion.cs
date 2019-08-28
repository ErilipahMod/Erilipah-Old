using Terraria.ModLoader;

namespace Erilipah.Items
{
    public abstract class ModMinion : ModProjectile
    {
        public override void AI()
        {
            CheckActive();
            Behavior();
        }

        public abstract void CheckActive();

        public abstract void Behavior();
    }
}