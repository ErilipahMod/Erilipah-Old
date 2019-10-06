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

        private bool I(Player p, string s) => p.armor.Take(10).Any(i => i.type == mod.ItemType(s + "Medallion"));
        public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            speedX *= Stats(item, player, out _, out _, out _, out _);
            speedY *= Stats(item, player, out _, out _, out _, out _);
            return base.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        private float Stats(Item I, Player p, out float damage, out int crit, out float useTime, out float kb)
        {
            damage = 0;
            crit = 0;
            useTime = 1;
            kb = 1;

            int use = I.useStyle;
            if (I.melee)
            {
                bool notTool = I.pick == 0 && I.axe == 0 && I.hammer == 0;
                if (this.I(p, "Broadsword") && use == 1 && !I.noMelee && !I.noUseGraphic
                    && notTool)
                {
                    damage = 0.135f;
                    crit = 5;
                }
                if (this.I(p, "Shortsword") && use == 3
                    && notTool)
                {
                    damage = 0.175f;
                    useTime = 1.2f;
                    crit = 5;
                }
                if (this.I(p, "MeleeProjectile") && I.shoot > 0 && I.noMelee && I.noUseGraphic
                    && notTool)
                {
                    damage = 0.175f;
                    kb = 1.10f;
                }
                if (this.I(p, "Pickaxe") && use == 1
                    && I.pick > 0 && I.hammer == 0)
                {
                    useTime = 1.15f;
                }
                if (this.I(p, "Axe") && use == 1
                    && I.axe > 0 && I.hammer == 0)
                {
                    useTime = 1.15f;
                }
            }

            if (I.ranged)
            {
                if (this.I(p, "Bow") && use == 5 && I.useAmmo == AmmoID.Arrow && I.shoot > 0)
                {
                    damage = 0.15f;
                    return 1.10f;
                }
                if (this.I(p, "RocketLauncher") && use == 5 && I.useAmmo == AmmoID.Rocket && I.shoot > 0)
                {
                    crit = 10;
                    damage = 0.20f;
                }
                if (this.I(p, "Gun") && use == 5 && I.useAmmo == AmmoID.Bullet && I.shoot > 0)
                {
                    damage = 0.12f;
                    useTime = 1.1f;
                    crit = 5;
                    return 1.20f;
                }
            }

            if (I.magic)
            {
                if (this.I(p, "Staff") && use == 5 && Item.staff[I.type])
                {
                    damage = 0.135f;
                    return 1.20f;
                }
                if (this.I(p, "Tome") && use == 5 && I.width < 40 && I.height < 40 && !Item.staff[I.type])
                {
                    damage = 0.175f;
                    return 1.20f;
                }
            }

            if (I.sentry)
                if (this.I(p, "Sentry"))
                    damage = 0.2f;
            return 1;
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
        {
            Stats(item, player, out float c, out _, out _, out _);
            add += c;
        }
        public override void GetWeaponKnockback(Item item, Player player, ref float knockback)
        {
            Stats(item, player, out _, out _, out _, out float c);
            knockback *= c;
        }
        public override void GetWeaponCrit(Item item, Player player, ref int crit)
        {
            Stats(item, player, out _, out int c, out _, out _);
            crit += c;
        }
        public override float UseTimeMultiplier(Item item, Player player)
        {
            Stats(item, player, out _, out _, out float c, out _);
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

#pragma warning disable IDE1006 // Naming Styles
        protected abstract int i { get; }
        protected virtual int i2 => 0;
#pragma warning restore IDE1006 // Naming Styles
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
    { protected override int i => ItemID.GoldBroadsword; protected override int i2 => ItemID.PlatinumBroadsword; }
    public class ShortswordMedallion : Medallion
    { protected override int i => ItemID.GoldShortsword; protected override int i2 => ItemID.PlatinumShortsword; }
    public class MeleeProjectileMedallion : Medallion
    { protected override int i => ItemID.EnchantedBoomerang; }
    public class PickaxeMedallion : Medallion
    { protected override int i => ItemID.GoldPickaxe; protected override int i2 => ItemID.PlatinumPickaxe; }
    public class AxeMedallion : Medallion
    { protected override int i => ItemID.GoldAxe; protected override int i2 => ItemID.PlatinumAxe; }

    public class BowMedallion : Medallion
    { protected override int i => ItemID.GoldBow; protected override int i2 => ItemID.PlatinumBow; }
    public class RocketLauncherMedallion : Medallion
    { protected override int i => ItemID.RocketLauncher; protected override int i2 => ItemID.SnowmanCannon; }
    public class GunMedallion : Medallion
    { protected override int i => ItemID.Handgun; }

    public class StaffMedallion : Medallion
    { protected override int i => ItemID.AmethystStaff; protected override int i2 => ItemID.TopazStaff; }
    public class TomeMedallion : Medallion
    { protected override int i => ItemID.WaterBolt; }

    public class SentryMedallion : Medallion
    { protected override int i => ItemID.DD2FlameburstTowerT1Popper; }
    public class MinionMedallion : Medallion
    { protected override int i => ItemID.HornetStaff; }

    public class ThrownItemMedallion : Medallion
    { protected override int i => ItemID.BouncyGrenade; protected override int i2 => ItemID.BouncyDynamite; }
}