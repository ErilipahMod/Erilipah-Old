using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Erilipah.Items
{
    public abstract class NewModItem : ModItem
    {
        protected abstract UseTypes UseType { get; }
        protected abstract int[] Dimensions { get; }
        protected abstract int Rarity { get; }

        protected virtual int Damage => 0;
        /// <summary>
        /// UseTime, UseAnimation
        /// </summary>
        protected virtual int[] UseSpeedArray => new int[0];
        protected virtual int UseSpeed => 0;
        protected virtual float Knockback => 0;
        protected virtual int Crit => 4;
        protected virtual int Mana => 0;
        protected virtual int Defense => 0;

        protected virtual float ShootSpeed => 0;
        protected virtual int ShootType => mod.ProjectileType(GetType().Name + "Proj");
        protected virtual bool FiresProjectile => ShootSpeed > 0;

        protected virtual int Axe => 0;
        protected virtual int Pick => 0;
        protected virtual int Hammer => 0;

        /// <summary>
        /// Null for auto-calculate.
        /// Recommended for post Moon Lord items
        /// </summary>
        protected virtual int? Value => null;
        /// <summary>
        /// Null for auto-choose
        /// </summary>
        protected virtual int? MaxStack => null;
        /// <summary>
        /// Null for auto-choose
        /// </summary>
        protected virtual bool? Consumable => null;
        protected virtual bool Channel => false;
        protected virtual bool AutoReuse => true;
        protected virtual bool Accessory => false;

        protected virtual Vector2? HoldoutOffSet => null;
        public override Vector2? HoldoutOffset() => HoldoutOffSet;
        protected virtual Vector2 ShootPosOffset => Vector2.Zero;
        protected virtual float ShootDistanceOffset => 0;
        protected float degreeSpread = 0;
        protected virtual float ShootInaccuracy => degreeSpread;

        protected virtual LegacySoundStyle UseSound => null;

        protected enum UseTypes
        {
            None = -1, Material, Edible, Potion, HoldUp, Bullet, Arrow,
            Spear, SwordSwing, SwordStab, Swing, Bow, Gun, MagicStaff, Book, Summon, Thrown, Yoyo,
            Accessory, Armor, Placeable
        }

        protected new virtual string DisplayName => null;
        protected new virtual string Tooltip => null;
        protected virtual int PlaceTile => 0;

        protected virtual int[,] CraftingIngredients => new int[,] { };
        protected virtual int CraftingTile => -1;
        protected virtual int CraftingResultAmount => 1;

        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);

            Item.staff[item.type] = UseType == UseTypes.MagicStaff;
        }
        public override void SetDefaults()
        {
            // most important
            degreeSpread = ShootInaccuracy;
            item.accessory = Accessory;

            if (Dimensions.Length >= 2)
            {
                item.width = Dimensions[0];
                item.height = Dimensions[1];
            }
            else if (Dimensions.Length == 1)
                item.width = item.height = Dimensions[0];
            else if (Main.itemTexture[item.type] != null)
            {
                item.width = Main.itemTexture[item.type].Width;
                item.height = Main.itemTexture[item.type].Height;
            }

            item.crit = Crit - 4;
            item.defense = Defense;
            bool NotUsable =
                UseType == UseTypes.Accessory || UseType == UseTypes.Armor ||
                UseType == UseTypes.Material || UseType == UseTypes.Placeable;
            if (Damage > 0 && !NotUsable)
            {
                item.damage = Damage;
            }
            if (Knockback > 0 && !NotUsable)
            {
                item.knockBack = Knockback;
            }
            if (Mana > 0 && !NotUsable)
            {
                item.mana = Mana;
            }

            item.useTime = 0;
            item.useAnimation = 0;
            if (UseSpeedArray.Length == 2 && !NotUsable)
            {
                item.useTime = UseSpeedArray[0];
                item.useAnimation = UseSpeedArray[1];
            }
            else if (UseSpeed > 0 && !NotUsable)
            {
                item.useTime =
                    item.useAnimation = UseSpeed;
            }

            item.rare = Rarity;

            item.axe = Axe / 5;
            item.pick = Pick;
            item.hammer = Hammer;

            if (Value != null)
            {
                item.value = (int)Value;
            }
            else
            {
                int useSpeed = item.useTime > 0 ? item.useTime : 60;
                item.value = (int)(Math.Max(Damage, 1) * Math.Max(Knockback, 1) * Math.Max(Defense, 1) *
                    (60 / useSpeed) * Rarity * 5) / item.maxStack;
            }
            // maxStack and consumable at bottom

            item.autoReuse = AutoReuse;
            item.channel = Channel;

            if (FiresProjectile)
            {
                item.shoot = ShootType;
                item.shootSpeed = ShootSpeed;
            }

            item.useTurn = UseType == UseTypes.Swing || UseType == UseTypes.SwordStab;
            item.noUseGraphic = UseType == UseTypes.Thrown || UseType == UseTypes.Yoyo || UseType == UseTypes.Spear;
            item.noMelee = UseType != UseTypes.Swing && UseType != UseTypes.SwordSwing && UseType != UseTypes.SwordStab;

            item.melee = UseType == UseTypes.Swing || UseType == UseTypes.SwordSwing || UseType == UseTypes.Yoyo || UseType == UseTypes.Spear;
            item.ranged = UseType == UseTypes.Bow || UseType == UseTypes.Gun || UseType == UseTypes.Arrow || UseType == UseTypes.Bullet;
            item.magic = UseType == UseTypes.Book || UseType == UseTypes.MagicStaff;
            item.summon = UseType == UseTypes.Summon;
            item.thrown = UseType == UseTypes.Thrown;
            item.accessory = UseType == UseTypes.Accessory;

            item.maxStack = 1;
            item.consumable = false;

            item.UseSound = SoundID.Item1;
            if (UseType == UseTypes.Yoyo)
            {
                item.channel = true;
            }
            if (UseType == UseTypes.Bow)
            {
                item.useAmmo = AmmoID.Arrow;
                item.useStyle = ItemUseStyleID.HoldingOut;
                item.UseSound = SoundID.Item5;
                item.shoot = ProjectileID.WoodenArrowFriendly;
            }
            if (UseType == UseTypes.Gun)
            {
                item.useAmmo = AmmoID.Bullet;
                item.useStyle = ItemUseStyleID.HoldingOut;
                item.UseSound = SoundID.Item11;
                item.shoot = ProjectileID.Bullet;
            }
            if (UseType == UseTypes.Book || UseType == UseTypes.MagicStaff || UseType == UseTypes.Yoyo)
            {
                item.useStyle = ItemUseStyleID.HoldingOut;
                if (UseType != UseTypes.Yoyo)
                {
                    item.UseSound = SoundID.Item8;
                }
            }
            if (UseType == UseTypes.Summon || UseType == UseTypes.Swing ||
                UseType == UseTypes.SwordSwing || UseType == UseTypes.Thrown)
            {
                if (UseType == UseTypes.Summon)
                {
                    item.UseSound = SoundID.Item44;
                }
                else if (UseType == UseTypes.Thrown)
                {
                    item.UseSound = SoundID.Item19;
                }
                item.useStyle = ItemUseStyleID.SwingThrow;
            }
            if (UseType == UseTypes.Edible)
            {
                item.UseSound = SoundID.Item2;
                item.useStyle = ItemUseStyleID.EatingUsing;
                item.maxStack = 30;
                item.consumable = true;
            }
            if (UseType == UseTypes.Potion)
            {
                item.UseSound = SoundID.Item3;
                item.useStyle = 2;
                item.maxStack = 30;
                item.consumable = true;
                item.useTime = item.useAnimation = 17;
            }
            if (UseType == UseTypes.HoldUp)
            {
                item.useStyle = ItemUseStyleID.HoldingUp;
                item.maxStack = 30;
                item.consumable = true;
            }
            if (UseType == UseTypes.SwordStab)
            {
                item.UseSound = SoundID.Item7;
                item.useStyle = ItemUseStyleID.Stabbing;
            }
            if (UseType == UseTypes.Material || UseType == UseTypes.Thrown && Consumable == true)
            {
                item.maxStack = 999;
            }
            if (UseType == UseTypes.Spear)
            {
                item.useStyle = ItemUseStyleID.HoldingOut;
            }
            if (UseType == UseTypes.Placeable)
            {
                item.useTime = 10;
                item.useAnimation = 15;
                item.createTile = PlaceTile;
                item.consumable = true;
                item.useStyle = 1;
                item.maxStack = 999;
                item.useTurn = true;
                item.autoReuse = true;
            }
            if (UseType == UseTypes.Bullet || UseType == UseTypes.Arrow)
            {
                item.maxStack = 999;
                item.consumable = true;
                if (UseType == UseTypes.Bullet)
                    item.ammo = AmmoID.Bullet;
                else
                    item.ammo = AmmoID.Arrow;
            }

            if (UseSound != null)
            {
                item.UseSound = UseSound;
            }
            if (MaxStack != null)
            {
                item.maxStack = (int)MaxStack;
            }
            if (Consumable != null)
            {
                item.consumable = (bool)Consumable;
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (UseType == UseTypes.Spear && player.ownedProjectileCounts[item.shoot] < 1)
                return true;
            else if (UseType == UseTypes.Spear)
                return false;

            return true;
        }
        protected virtual int ShootCool => 0;

        private int shoot;

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem.type != item.type)
            {
                ++shoot;
            }
        }
        public override void HoldItem(Player player)
        {
            shoot++;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * ShootDistanceOffset;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            position += ShootPosOffset;

            if (degreeSpread > 0)
            {
                Vector2 speed = new Vector2(speedX, speedY);
                speed = speed.RotatedByRandom(MathHelper.ToRadians(Math.Abs(degreeSpread)));
                speedX = speed.X;
                speedY = speed.Y;
            }

            if (UseType == UseTypes.Gun && type == ProjectileID.Bullet)
            {
                type = item.shoot;
            }
            if (UseType == UseTypes.Bow && type == ProjectileID.WoodenArrowFriendly)
            {
                type = item.shoot;
            }
            if (shoot >= ShootCool)
            {
                shoot = 0;
                return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            if (CraftingTile >= 0 && 0 < CraftingIngredients.GetLength(0))
            {
                ModRecipe r = new ModRecipe(mod);
                for (int i = 0; i < CraftingIngredients.GetLength(0); i++)
                {
                    r.AddIngredient(CraftingIngredients[i, 0], CraftingIngredients[i, 1]);
                }
                if (CraftingTile >= 0)
                {
                    r.AddTile(CraftingTile);
                }
                r.SetResult(this, CraftingResultAmount);
                r.AddRecipe();
            }
        }
    }
}