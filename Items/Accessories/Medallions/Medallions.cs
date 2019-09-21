using Microsoft.Xna.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories.Medallions
{
    public class Medallions : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;

        private bool i(Player p, string s) => p.armor.Take(10).Any(i => i.type == mod.ItemType(s + "Medallion"));
        public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            speedX *= stats(item, player, out a, out b, out a, out a);
            speedY *= stats(item, player, out a, out b, out a, out a);
            return base.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        private float stats(Item I, Player p, out float damage, out int crit, out float useTime, out float kb)
        {
            damage = 0;
            crit = 0;
            useTime = 1;
            kb = 1;

            int use = I.useStyle;
            if (I.melee)
            {
                bool notTool = I.pick == 0 && I.axe == 0 && I.hammer == 0;
                if (i(p, "Broadsword") && use == 1 && !I.noMelee && !I.noUseGraphic
                    && notTool)
                {
                    damage = 0.135f;
                    crit = 5;
                }
                if (i(p, "Shortsword") && use == 3
                    && notTool)
                {
                    damage = 0.175f;
                    useTime = 1.2f;
                    crit = 5;
                }
                if (i(p, "MeleeProjectile") && I.shoot > 0 && I.noMelee && I.noUseGraphic
                    && notTool)
                {
                    damage = 0.175f;
                    kb = 1.10f;
                }
                if (i(p, "Pickaxe") && use == 1
                    && I.pick > 0 && I.hammer == 0)
                {
                    useTime = 1.15f;
                }
                if (i(p, "Axe") && use == 1
                    && I.axe > 0 && I.hammer == 0)
                {
                    useTime = 1.15f;
                }
            }

            if (I.ranged)
            {
                if (i(p, "Bow") && use == 5 && I.useAmmo == AmmoID.Arrow && I.shoot > 0)
                {
                    damage = 0.15f;
                    return 1.10f;
                }
                if (i(p, "RocketLauncher") && use == 5 && I.useAmmo == AmmoID.Rocket && I.shoot > 0)
                {
                    crit = 10;
                    damage = 0.20f;
                }
                if (i(p, "Gun") && use == 5 && I.useAmmo == AmmoID.Bullet && I.shoot > 0)
                {
                    damage = 0.12f;
                    useTime = 1.1f;
                    crit = 5;
                    return 1.20f;
                }
            }

            if (I.magic)
            {
                if (i(p, "Staff") && use == 5 && Item.staff[I.type])
                {
                    damage = 0.135f;
                    return 1.20f;
                }
                if (i(p, "Tome") && use == 5 && I.width < 40 && I.height < 40 && !Item.staff[I.type])
                {
                    damage = 0.175f;
                    return 1.20f;
                }
            }

            if (I.sentry)
                if (i(p, "Sentry"))
                    damage = 0.2f;
            return 1;
        }

        private float a;
        private int b;
        public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
        {
            float c;
            stats(item, player, out c, out b, out a, out a);
            add += c;
        }
        public override void GetWeaponKnockback(Item item, Player player, ref float knockback)
        {
            float c;
            stats(item, player, out a, out b, out a, out c);
            knockback *= c;
        }
        public override void GetWeaponCrit(Item item, Player player, ref int crit)
        {
            int c;
            stats(item, player, out a, out c, out a, out a);
            crit += c;
        }
        public override float UseTimeMultiplier(Item item, Player player)
        {
            float c;
            stats(item, player, out a, out b, out c, out a);
            return c;
        }

        public override void UpdateEquip(Item item, Player player)
        {
            if (item.type == mod.ItemType<SentryMedallion>())
            {
                player.maxTurrets += 1;
            }
            if (item.type == mod.ItemType<MinionMedallion>())
            {
                player.maxMinions += 2;
                player.minionDamage += 0.10f;
            }
            if (item.type == mod.ItemType<ThrownItemMedallion>())
            {
                player.thrownDamage += 1.10f;
                player.thrownCrit += 10;
            }
        }
    }
    public class BlankMedallion : NewModItem
    {
        protected override string Tooltip => "'It's missing something...'";
        protected override UseTypes UseType => UseTypes.Material;
        protected override int[] Dimensions => new int[] { 30, 30 };
        protected override int Rarity => 2;
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Wood, 8);
            r.AddRecipeGroup("IronBar", 2);
            r.AddRecipeGroup("Erilipah:GoldBars", 1);
            r.AddTile(TileID.WorkBenches);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public abstract class Medallion : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Accessory;
        protected override int[] Dimensions => new int[] { 30, 30 };
        protected override int Rarity => 2;
        protected override string Tooltip
        {
            get
            {
                if (item.type == mod.ItemType<SentryMedallion>())
                    return "Empowers Sentries";
                return "Empowers " + Regex.Replace(GetType().Name.Replace("Medallion", "") + 's', "([A-Z])", " $1").Trim();
            }
        }

        protected abstract int i { get; }
        protected virtual int i2 => 0;
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<BlankMedallion>());
            r.AddIngredient(i, i == ItemID.BouncyDynamite ? 25 : 1);
            r.AddTile(TileID.TinkerersWorkbench);
            r.SetResult(item.type);
            r.AddRecipe();

            if (i2 != 0)
            {
                r = new ModRecipe(mod);
                r.AddIngredient(mod.ItemType<BlankMedallion>());
                r.AddIngredient(i2, i2 == ItemID.BouncyGrenade ? 25 : 1);
                r.AddTile(TileID.TinkerersWorkbench);
                r.SetResult(item.type);
                r.AddRecipe();
            }
        }
    }

    public class BroadswordMedallion : Medallion
    { override protected int i => ItemID.GoldBroadsword; override protected int i2 => ItemID.PlatinumBroadsword; }
    public class ShortswordMedallion : Medallion
    { override protected int i => ItemID.GoldShortsword; override protected int i2 => ItemID.PlatinumShortsword; }
    public class MeleeProjectileMedallion : Medallion
    { override protected int i => ItemID.EnchantedBoomerang; }
    public class PickaxeMedallion : Medallion
    { override protected int i => ItemID.GoldPickaxe; override protected int i2 => ItemID.PlatinumPickaxe; }
    public class AxeMedallion : Medallion
    { override protected int i => ItemID.GoldAxe; override protected int i2 => ItemID.PlatinumAxe; }

    public class BowMedallion : Medallion
    { override protected int i => ItemID.GoldBow; override protected int i2 => ItemID.PlatinumBow; }
    public class RocketLauncherMedallion : Medallion
    { override protected int i => ItemID.RocketLauncher; override protected int i2 => ItemID.SnowmanCannon; }
    public class GunMedallion : Medallion
    { override protected int i => ItemID.Handgun; }

    public class StaffMedallion : Medallion
    { override protected int i => ItemID.AmethystStaff; override protected int i2 => ItemID.TopazStaff; }
    public class TomeMedallion : Medallion
    { override protected int i => ItemID.WaterBolt; }

    public class SentryMedallion : Medallion
    { override protected int i => ItemID.DD2FlameburstTowerT1Popper; }
    public class MinionMedallion : Medallion
    { override protected int i => ItemID.HornetStaff; }

    public class ThrownItemMedallion : Medallion
    { override protected int i => ItemID.BouncyGrenade; override protected int i2 => ItemID.BouncyDynamite; }
}