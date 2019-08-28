using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories
{
    public class TheGrandGeode : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Gun;
        protected override int[] Dimensions => new int[] { 66, 32 };
        protected override int Rarity => 2;
        protected override string Tooltip => "Rapidly fires powerful energy blasts inaccurately\n" +
            "'It's so beautiful, it could melt you alive!'";
        protected override int Mana => 13;

        protected override int Damage => 28;
        protected override int[] UseSpeedArray => new int[] { 15, 15 };
        protected override int Crit => 15;
        protected override float Knockback => 3.5f;

        protected override float ShootSpeed => 10;
        protected override float ShootInaccuracy => 15;

        protected override float ShootDistanceOffset => 56;
        protected override Vector2? HoldoutOffSet => new Vector2(-10, 0);

        public override void SetDefaults()
        {
            base.SetDefaults();
            item.magic = true;
            item.ranged = false;
            item.useAmmo = AmmoID.None;
            item.UseSound = null;
        }
        public override void UpdateInventory(Player player)
        {
            bool hasGeode = player.armor.Take(10).Any(x => x.type == mod.ItemType<EnormousGeode>());
            if (hasGeode)
            {
                item.GetGlobalItem<ItemBuff>().NewBuff(ItemBuffID.Geode, 2, false);
                item.mana = (int)((Mana - 6) * player.manaCost);
                degreeSpread = ShootInaccuracy - 6;
                item.useTime = item.useAnimation = 10;
            }
            else
            {
                item.mana = (int)(16 * player.manaCost);
                degreeSpread = ShootInaccuracy;
                item.useTime = item.useAnimation = 15;
            }
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<EnormousGeode>());
            r.AddIngredient(mod.ItemType<Sacracite.SacraciteCore>(), 2);
            r.AddIngredient(ItemID.Diamond, 2);
            r.AddIngredient(ItemID.IllegalGunParts, 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            item.shoot = mod.ProjectileType<TheGrandGeodeProj>();
            base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);

            float pitchOffset = Main.rand.NextFloat() * (Main.rand.NextBool(2) ? 1 : -1) / 2;
            Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 125, 0.8f, pitchOffset);
            return true;
        }
    }
    public class TheGrandGeodeProj : NewModProjectile
    {
        protected override TextureTypes TextureType => TextureTypes.Invisible;
        protected override int[] Dimensions => new int[] { 10, 10 };
        protected override int DustType => DustID.t_Martian;

        protected override int ExtraUpdates => 4;
        protected override int Pierce => -1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;
        protected override float? Rotation => null;

        protected override DamageTypes DamageType => DamageTypes.Magic;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override int TrailThickness => 5;
        protected override float TrailScale => 1.125f;
        protected override bool NoDustLight => true;

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Main.dust[Dust.NewDust(projectile.Center, 0, 0, DustID.t_Martian, Scale: 1.4f)];
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
    }
    public class EnormousGeode : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 26, 30 };
        protected override int Rarity => 2;
        protected override string Tooltip => "Increases magic damage by 10%\n" +
            "Increases magic critical strike chance by 5%\n" +
            "Increases mana regeneration by 10%\n" +
            "Increases effectivity of The Grand Geode greatly";

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Amethyst, 3);
            r.AddIngredient(ItemID.Topaz, 3);
            r.AddIngredient(ItemID.Ruby, 2);
            r.AddIngredient(ItemID.Sapphire, 2);
            r.AddIngredient(ItemID.Emerald, 2);
            r.AddIngredient(ItemID.Diamond, 1);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
        public override void UpdateEquip(Player player)
        {
            player.magicDamage += 0.10f;
            player.magicCrit += 5;
            player.manaRegen = (int)(player.manaRegen * 1.10f);
        }
    }
}
