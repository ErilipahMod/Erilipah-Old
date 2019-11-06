using Erilipah.Items.Drone;
using Erilipah.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Dracocide
{
    public class AssaultCannon : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Assault Cannon");
            Tooltip.SetDefault("Fires six powerful blasts\n" +
                "Consumes six ammo per shot\n" +
                "Will send you flying\n" +
                "'It's a pain to maintain this.'");
        }
        public override void SetDefaults()
        {
            item.damage = 60;
            item.knockBack = 3;
            item.crit = 8;
            item.ranged = true;
            item.noMelee = true;

            item.maxStack = 1;
            item.useTime = 17;
            item.useAnimation = 17;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.autoReuse = true;

            item.width = 34;
            item.height = 22;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;
            item.UseSound = SoundID.NPCDeath14;

            item.shoot = ProjectileType<FAssaultBolt>();
            item.shootSpeed = 11f;
            item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            // Rapid-fire in a small, 20-degree spread towards the player
            for (int i = -3; i <= 3; i++)
            {
                Vector2 dir = new Vector2(speedX, speedY).SafeNormalize(Vector2.Zero);
                Vector2 fireTo = dir.RotatedBy(
                    MathHelper.ToRadians(
                        i * 5
                        ));

                if (Main.netMode != 1)
                    Projectile.NewProjectile(
                        player.Center + fireTo * 28,
                        fireTo * item.shootSpeed,
                        ProjectileType<FAssaultBolt>(),
                        item.damage,
                        item.knockBack,
                        player.whoAmI);

                Dust.NewDust(player.Center + fireTo * 32, 0, 0, mod.DustType("DeepFlames"), fireTo.X * 3, fireTo.Y * 3);
            }

            float boost = MathHelper.Lerp(6, 0, player.velocity.Length() / 50f);
            player.velocity -= new Vector2(speedX, speedY).SafeNormalize(Vector2.Zero) * boost;
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-3, 3);
        }

        public override bool CanUseItem(Player player)
        {
            return player.FindAmmo(x => x.stack > 5 && x.ammo == AmmoID.Bullet) != null;
        }

        public override bool ConsumeAmmo(Player player)
        {
            player.FindAmmo(x => x.ammo == AmmoID.Bullet && x.stack > 5).stack -= 6;
            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemType<Dracocell>(), 6);
            recipe.AddIngredient(ItemType<DetachedDroneBlaster>(), 1);

            recipe.AddTile(TileID.MythrilAnvil);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class FAssaultBolt : NewModProjectile
    {
        public override string Texture => "Erilipah/NPCs/Dracocide/AssaultBolt";
        protected override int[] Dimensions => new int[] { 12, 22 };
        protected override int DustType => DustID.AmberBolt;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override bool NoDustLight => true;
        protected override float TrailScale => 0.8f;
        protected override DamageTypes DamageType => DamageTypes.Ranged;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation();
    }
}
