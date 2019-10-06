using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Erilipah
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

            Terraria.Item.staff[item.type] = UseType == UseTypes.MagicStaff;
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
                int useSpeed = (item.useTime > 0 ? item.useTime : 60);
                item.value = (int)(Math.Max(Damage, 1) * Math.Max(Knockback, 1) * Math.Max(Defense, 1) *
                    (60 / useSpeed) * (Rarity) * 5) / item.maxStack;
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
            if (UseType == UseTypes.Material || (UseType == UseTypes.Thrown && Consumable == true))
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


    public abstract class NewModProjectile : ModProjectile
    {
        protected virtual int[] Dimensions => new int[0];
        protected virtual int DustType => 0;

        protected const int InfinitePierceAndBounce = -2;
        protected virtual int Pierce => 0;
        protected virtual int Bounce => 0;
        protected virtual int AIStyle => -1;
        protected virtual int ImmuneFrames => 0;

        protected virtual int FlightTime => 0;
        protected virtual float Gravity => 0;
        protected virtual float? Rotation => null;
        protected virtual float MaxFallSpeed => 10f;
        protected virtual bool TileCollide => true;
        protected virtual int ExtraUpdates => 0;
        protected virtual bool NoDustLight => false;
        protected bool DropItem = true;
        protected virtual Item ItemSource
        {
            get
            {
                string name = GetType().Name;
                try
                {
                    ModItem item = mod.GetItem(name.Replace("Proj", ""));
                    if (item != null && item.item != null)
                    {
                        return item.item;
                    }
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Friendly, hostile
        /// </summary>
        protected virtual bool[] DamageTeam => new bool[2] { true, false };
        protected abstract DamageTypes DamageType { get; }
        protected enum DamageTypes
        {
            None = -1, Hostile, ItemCopy, Melee, Ranged, Magic, Thrown, Minion, Sentry
        }

        protected virtual DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected enum DustTrailTypes
        {
            None = -2, NoTrail, Normal, Perfect, PerfectNoGravity
        }
        protected virtual int TrailThickness => 1;
        protected virtual float TrailScale => 1;
        protected virtual int TimeLeft => 180;
        protected virtual Color Light => new Color(0, 0, 0);

        protected float Speed => Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y);
        protected int RotateDirection => projectile.velocity.X > 0 ? 1 : -1;
        protected float Rotate(float rps) => projectile.rotation + Helper.RadiansPerTick(rps) * RotateDirection;
        protected float Degrees270 = MathHelper.ToRadians(270);
        protected float Degrees180 = MathHelper.ToRadians(180);
        protected float Degrees90 = MathHelper.ToRadians(90);
        protected static readonly int[] AutoDimensions = new int[] { -1, -1 };
        protected Player player
        {
            get
            {
                return Main.player[projectile.owner];
            }
            set
            {
                projectile.owner = value.whoAmI;
            }
        }

        protected bool MotionBlurActive = false;
        protected int MotionBlurLength = 0;
        protected virtual int MaxMotionBlurLength { get; } = 0;
        protected virtual int FrameCount => 1;

        protected virtual TextureTypes TextureType
            => (DamageType == DamageTypes.Thrown || Dimensions == AutoDimensions) ? TextureTypes.ItemClone : TextureTypes.Default;
        protected enum TextureTypes
        {
            Default = -1, Invisible, ItemClone
        }
        public override string Texture
        {
            get
            {
                if (TextureType == TextureTypes.ItemClone)
                {
                    string name = GetType().Name;
                    string Namespace = GetType().Namespace.Replace('.', '/') + '/';
                    return Namespace + name.Replace("Proj", "");
                }
                if (TextureType == (int)TextureTypes.Invisible)
                {
                    return Helper.Invisible;
                }
                string n = GetType().Name;
                string N = GetType().Namespace.Replace('.', '/') + '/';
                return N + n;
            }
        }

        public override void SetDefaults()
        {
            if (AIStyle > -1)
                projectile.aiStyle = AIStyle;
            if (projectile.Name != null && projectile.Name.Contains("Arrow"))
                projectile.arrow = true;
            projectile.extraUpdates = ExtraUpdates;

            if (ItemSource != null && TextureType == TextureTypes.ItemClone && Dimensions == AutoDimensions)
            {
                projectile.width = ItemSource.width;
                projectile.height = ItemSource.height;
            }
            else if (Dimensions.Length >= 2)
            {
                projectile.width = Dimensions[0];
                projectile.height = Dimensions[1];
            }
            else if (Dimensions.Length == 1)
            {
                projectile.width = projectile.height = Dimensions[0];
            }
            else if (Main.projectileTexture[projectile.type] != null)
            {
                int[] dimensions = {
                    Main.projectileTexture[projectile.type].Width,
                    Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type]
                };
                projectile.width = dimensions[0];
                projectile.height = dimensions[1];
            }
            projectile.tileCollide = TileCollide;

            if (DamageType == DamageTypes.Hostile)
            {
                projectile.hostile = true;
                projectile.friendly = false;
            }
            else
            {
                projectile.hostile = DamageTeam[1];
                projectile.friendly = DamageTeam[0];
            }

            if (ItemSource == null || DamageType != DamageTypes.ItemCopy)
            {
                projectile.melee = DamageType == DamageTypes.Melee;
                projectile.ranged = DamageType == DamageTypes.Ranged;
                projectile.magic = DamageType == DamageTypes.Magic;
                projectile.thrown = DamageType == DamageTypes.Thrown;
                projectile.minion = DamageType == DamageTypes.Minion;
                projectile.sentry = DamageType == DamageTypes.Sentry;
            }
            else if (DamageType == DamageTypes.ItemCopy)
            {
                projectile.melee = ItemSource.melee;
                projectile.ranged = ItemSource.ranged;
                projectile.magic = ItemSource.magic;
                projectile.thrown = ItemSource.thrown;
                projectile.minion = ItemSource.summon;
                projectile.sentry = ItemSource.sentry;
            }

            projectile.penetrate = Pierce >= 0 ? 1 + Pierce : -1;
            projectile.timeLeft = TimeLeft;
        }
        public override void SetStaticDefaults()
        {
            string name = GetType().Name;
            string completeName = name.Replace("Proj", "");
            DisplayName.SetDefault(Regex.Replace(completeName, "([A-Z])", " $1").Trim());
            if (MotionBlurLength > 0)
            {
                ProjectileID.Sets.TrailCacheLength[projectile.type] = Math.Max(MotionBlurLength, MaxMotionBlurLength);
                ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            }
            Main.projFrames[projectile.type] = FrameCount;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (++projectile.localAI[1] <= Bounce || Bounce < 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust dust = Main.dust[Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height,
                        DustType, projectile.velocity.X / 10, projectile.velocity.Y / 10, Scale: 0.92f)];
                    if (NoDustLight)
                        dust.noLight = true;
                }

                if (projectile.velocity.X != oldVelocity.X)
                    projectile.velocity.X = -oldVelocity.X;

                if (projectile.velocity.Y != oldVelocity.Y)
                    projectile.velocity.Y = -oldVelocity.Y;
            }
            else if (projectile.localAI[1] > Bounce)
                projectile.Kill();
            return false;
        }
        public override void Kill(int timeLeft)
        {
            if (DustTrailType != DustTrailTypes.None)
            {
                for (int i = 0; i < Math.Min(30, TrailThickness * 4); i++)
                {
                    Dust dust = Main.dust[Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType, Scale: TrailScale)];
                    if (NoDustLight)
                        dust.noLight = true;
                }
            }
            if (DropItem && ItemSource != null && ItemSource.consumable &&
                ItemSource.thrown && projectile.owner == Main.myPlayer && !projectile.noDropItem)
            {
                float chance = 1 - (projectile.width + projectile.height) / 100;
                float minChance = MathHelper.Clamp(chance, 0.0f, 0.2f);
                Loot.DropItem(projectile.getRect(), ItemSource.type, 1, 1, minChance * 100);
            }
        }

        private int fall = 0;
        protected bool Falling => ++fall > FlightTime;
        public override void AI()
        {
            if (Falling)
            {
                if (projectile.velocity.Y < MaxFallSpeed)
                    projectile.velocity.Y += Gravity;
            }
            if (Rotation != null)
            {
                projectile.rotation = (float)Rotation;
            }

            Lighting.AddLight(projectile.Center, Light.ToVector3());
            if (DustTrailType != DustTrailTypes.None)
            {
                for (int i = 0; i < TrailThickness; i++)
                {
                    if (DustTrailType == DustTrailTypes.PerfectNoGravity)
                    {
                        Dust dust = Terraria.Dust.NewDustPerfect(projectile.Center, DustType, Scale: TrailScale);
                        dust.noGravity = true;
                        if (NoDustLight)
                            dust.noLight = true;
                    }
                    if (DustTrailType == DustTrailTypes.Perfect)
                    {
                        Dust dust = Terraria.Dust.NewDustPerfect(projectile.Center, DustType, Scale: TrailScale);
                        if (NoDustLight)
                            dust.noLight = true;
                    }
                    if (DustTrailType == (int)DustTrailTypes.Normal)
                    {
                        Dust dust = Main.dust[Terraria.Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType, Scale: TrailScale)];
                        if (NoDustLight)
                            dust.noLight = true;
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (MotionBlurActive && projectile.velocity.Length() > 0)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];
                int frames = Main.projFrames[projectile.type];
                Rectangle rect = texture.Frame(1, frames, 0, projectile.frame);

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / Main.projFrames[projectile.type]) * 0.5f);
                if (MotionBlurLength > MaxMotionBlurLength)
                {
                    MotionBlurLength = MaxMotionBlurLength;
                }
                for (int i = 0; i < Math.Min(projectile.oldPos.Length, MotionBlurLength); i++)
                {
                    Vector2 drawPos = projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, projectile.gfxOffY);
                    Color color = projectile.GetAlpha(drawColor) * ((MotionBlurLength - i) / (float)MotionBlurLength);
                    SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: rect, color: color * ((255 - projectile.alpha) / 255f), rotation: projectile.oldRot[i],
                        origin: drawOrigin, scale: projectile.scale, effects: effects, layerDepth: 0
                        );
                }
            }
            return true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (ImmuneFrames > 0)
            {
                target.immune[projectile.owner] = ImmuneFrames;
            }
        }
    }


    public abstract class SpearProjectile : NewModProjectile
    {
        protected override int Pierce => -2;
        protected override bool TileCollide => false;
        protected override float Gravity => 0;
        protected override int Bounce => -2;
        protected override float? Rotation => null;
        protected override DamageTypes DamageType => DamageTypes.Melee;

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.aiStyle = 19;
            projectile.ownerHitCheck = true;
            projectile.hide = true;
        }

        protected abstract float MoveBackTimePercent { get; }
        protected abstract float MoveSpeed { get; }
        protected virtual float MoveBackSpeed => MoveSpeed;
        private float MovementFactor
        {
            get { return projectile.ai[0]; }
            set { projectile.ai[0] = value; }
        }
        public override void AI()
        {
            // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            // Sadly, Projectile/ModProjectile does not have its own
            Player plr = Main.player[projectile.owner];
            // Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
            Vector2 ownerMountedCenter = plr.RotatedRelativePoint(plr.MountedCenter, true);
            projectile.direction = plr.direction;
            plr.heldProj = projectile.whoAmI;
            plr.itemTime = plr.itemAnimation;
            projectile.position.X = ownerMountedCenter.X - projectile.width / 2;
            projectile.position.Y = ownerMountedCenter.Y - projectile.height / 2;
            // As long as the player isn't frozen, the spear can move
            if (!plr.frozen)
            {
                if (MovementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    MovementFactor = MoveSpeed; // Make sure the spear moves forward when initially thrown out
                    projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (plr.itemAnimation < plr.itemAnimationMax * MoveBackTimePercent) // Somewhere along the item animation, make sure the spear moves back
                {
                    MovementFactor -= MoveBackSpeed;
                }
                else // Otherwise, increase the movement factor
                {
                    MovementFactor += MoveSpeed;
                }
            }
            // Change the spear position based off of the velocity and the movementFactor
            projectile.position += projectile.velocity * MovementFactor;
            // When we reach the end of the animation, we can kill the spear projectile
            if (plr.itemAnimation == 0)
            {
                projectile.Kill();
            }
            // Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
            // MathHelper.ToRadians(xx degrees here)
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(135);
            // Offset by 90 degrees here
            if (projectile.spriteDirection == -1)
            {
                projectile.rotation -= MathHelper.ToRadians(90);
            }
        }
    }


    public abstract class ChargeProjectile : NewModProjectile
    {
        protected bool done = false;

        private bool AtMaxCharge => Charge == MaxCharge;
        protected bool Charging => !done && Charge < MaxCharge;
        protected virtual bool Dusts => true;

        protected float Charge
        {
            get { return projectile.localAI[0]; }
            set { projectile.localAI[0] = value; }
        }
        protected abstract bool Cancel { get; }
        protected abstract float MaxCharge { get; }
        protected abstract float MoveDistance { get; }
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.alpha = 255;
            projectile.friendly = projectile.hostile = false;
        }

        protected void ResumeVelocity(float speed = 10, LegacySoundStyle playSound = null)
        {
            if (playSound != null)
                Main.PlaySound(playSound, projectile.Center);

            projectile.friendly = DamageTeam[0];
            projectile.hostile = DamageTeam[1];
            projectile.alpha = 0;

            Vector2 velocity = Main.MouseWorld - player.Center;
            velocity.Normalize();
            velocity *= speed;

            projectile.velocity = velocity;
        }
        protected virtual void OnCancelCharge() { projectile.Kill(); }
        protected virtual void WhileCharging() { }
        protected virtual void OnFinishCharge() { ResumeVelocity(ItemSource != null ? ItemSource.shootSpeed : 10); }
        protected virtual void PostCharge() { }

        public override void AI()
        {
            base.AI();
            #region Set projectile position
            // Multiplayer support here, only run this code if the client running it is the owner of the projectile
            if (projectile.owner == Main.myPlayer && !AtMaxCharge && !done)
            {
                Vector2 diff = Main.MouseWorld - player.Center;
                diff.Normalize();
                projectile.velocity = diff;
                projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                projectile.netUpdate = true;
            }
            if (!AtMaxCharge && !done)
            {
                projectile.position = player.Center + projectile.velocity * MoveDistance;
                projectile.timeLeft = 300;
                int dir = projectile.direction;
                player.ChangeDir(dir);
                player.heldProj = projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.itemRotation = (float)Math.Atan2(projectile.velocity.Y * dir, projectile.velocity.X * dir);
            }
            #endregion
            #region Charging process
            // Kill the projectile if the player stops channeling
            if (Cancel && !AtMaxCharge && !done)
            {
                OnCancelCharge();
                done = true;
            }
            else if (!AtMaxCharge && !done)
            {
                if (Charge < MaxCharge)
                {
                    Charge++;
                }
                WhileCharging();

                if (!Dusts)
                    goto skip;

                Vector2 offset = projectile.velocity;
                offset *= MoveDistance - 20;
                Vector2 pos = player.Center + offset - new Vector2(10, 10);
                int chargeFact = (int)(Charge / 30f);
                Vector2 dustVelocity = Vector2.UnitX * 18f;
                dustVelocity = dustVelocity.RotatedBy(projectile.rotation - 1.57f, default);
                Vector2 spawnPos = projectile.Center + dustVelocity;
                for (int k = 0; k < chargeFact + 1; k++)
                {
                    Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f - (chargeFact * 2));
                    Dust dust = Main.dust[Dust.NewDust(pos, 17, 17, DustType, projectile.velocity.X / 2f,
                        projectile.velocity.Y / 2f, 0, default, 1f)];
                    dust.velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f - chargeFact * 2f) / 10f;
                    dust.noGravity = true;
                    dust.scale = Main.rand.Next(10, 20) * 0.05f;
                }
            skip:;
            }
            #endregion
            else if (AtMaxCharge && !done && projectile.owner == Main.myPlayer)
            {
                done = true;
                OnFinishCharge();
            }
            else if (done)
            {
                PostCharge();
            }
        }
    }


    public abstract class NewModNPC : ModNPC
    {
        protected int[] dimensions = new int[2];
        protected virtual int[] Dimensions => new int[0];
        protected virtual string Title => null;
        protected abstract int NPCFrameCount { get; }
        protected virtual int MaxLife { get; }
        protected virtual int Damage { get; }
        protected virtual int Defense { get; }
        protected virtual float KnockbackResist => 1;

        protected virtual int FrameDelay => 8;
        protected bool Client => Main.netMode == 1;
        protected virtual int[,] InflictBuffs => new int[0, 0];
        protected virtual int[] ImmuneToDebuff => new int[0];

        protected virtual Vector2 GoreVelocity => npc.velocity;
        protected virtual string GorePath => null;
        protected virtual int[] Gores => new int[0];

        protected virtual LegacySoundStyle HitSound => SoundID.NPCHit1;
        protected virtual LegacySoundStyle DeathSound => SoundID.NPCDeath1;
        protected virtual int AIType => 0;
        protected virtual NPCTypes NPCType => NPCTypes.Custom;
        protected enum NPCTypes
        {
            Custom, Fighter, Floater, Sentry
        }

        protected virtual bool LavaImmune => false;
        protected virtual bool NoTileCollide => false;
        protected virtual bool NoGravity => true;

        protected virtual float ScaleExpertHP => 0;
        protected virtual float ScaleExpertDmg => 2;
        protected virtual float ScaleExpertDef => 2;

        protected bool MotionBlurActive = false;
        protected virtual int MaxMotionBlurLength => 0;
        protected int MotionBlurLength = 0;

        protected virtual Player Target
        {
            get { return Main.player[npc.target]; }
            set { npc.target = value.whoAmI; }
        }
        protected Vector2 TCen => Target.Center;
        protected float TDist => Vector2.Distance(Target.Center, npc.Center);
        protected float TotalSpeed => Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y);

        public override void SetDefaults()
        {
            if (Main.npcTexture[npc.type] != null)
                dimensions = new int[] { Main.npcTexture[npc.type].Width, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] };
            if (Dimensions.Length > 0)
                dimensions = Dimensions;
            npc.value = MaxLife / 3 * (Defense / 15 + 1);

            npc.lifeMax = MaxLife;
            npc.damage = Damage;
            npc.defense = Defense;
            npc.width = dimensions[0];
            npc.height = dimensions[1];
            if (NPCType == NPCTypes.Fighter)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = false;
                npc.noGravity = false;
                npc.aiStyle = 3;
            }
            if (NPCType == NPCTypes.Floater)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = NoTileCollide;
                npc.noGravity = true;
            }
            if (NPCType == NPCTypes.Custom)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = NoTileCollide;
                npc.noGravity = NoGravity;
            }
            aiType = AIType;
            npc.knockBackResist = KnockbackResist;
            npc.friendly = false;

            npc.HitSound = HitSound;
            npc.DeathSound = DeathSound;
        }
        public override void SetStaticDefaults()
        {
            if (Title != null)
            {
                DisplayName.SetDefault(Title);
            }

            Main.npcFrameCount[npc.type] = NPCFrameCount;

            if (MaxMotionBlurLength > 0)
            {
                NPCID.Sets.TrailCacheLength[npc.type] = Math.Max(MotionBlurLength, MaxMotionBlurLength);
                NPCID.Sets.TrailingMode[npc.type] = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (MotionBlurActive && npc.velocity.Length() > 0)
            {
                Texture2D texture = Main.npcTexture[npc.type];

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / Main.npcFrameCount[npc.type]) * 0.5f);
                if (MotionBlurLength > MaxMotionBlurLength)
                {
                    MotionBlurLength = MaxMotionBlurLength;
                }
                for (int i = 0; i < Math.Min(MotionBlurLength, npc.oldPos.Length); i++)
                {
                    Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, npc.gfxOffY);
                    Color color = npc.GetAlpha(drawColor) * ((MotionBlurLength - i) / (float)MotionBlurLength);
                    SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: npc.frame, color: color, rotation: npc.rotation,
                        origin: drawOrigin, scale: npc.scale, effects: effects, layerDepth: 0
                        );
                }
            }
            return true;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (InflictBuffs != null)
            {
                for (int a = 0; a < InflictBuffs.GetLength(0); a++)
                {
                    target.AddBuff(InflictBuffs[a, 0], InflictBuffs[a, 1]);
                }
            }
        }
        public sealed override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            if (ScaleExpertHP <= 0)
                npc.lifeMax = (int)(npc.lifeMax * bossLifeScale) / 2;
            else
                npc.lifeMax = (int)(npc.lifeMax * ScaleExpertHP) / 2;
            npc.damage = (int)(npc.damage * ScaleExpertDmg) / 2;
            npc.defense = (int)(npc.defense * ScaleExpertDef) / 2;
        }

        protected enum AnimationTypes
        {
            AutoCycle, ReturnFrameNum
        }
        protected virtual int Animate(int frameHeight)
        {
            return -1;
        }
        public override void FindFrame(int frameHeight)
        {
            int a = Animate(frameHeight);
            if (a == (int)AnimationTypes.AutoCycle)
            {
                if (++npc.frameCounter % FrameDelay == 0)
                {
                    int frame = (npc.frame.Y / frameHeight + 1);
                    int Frame = frame % NPCFrameCount;
                    npc.frame.Y = Frame * frameHeight;
                }
            }
            if (a > (int)AnimationTypes.ReturnFrameNum)
            {
                npc.frame.Y = a * frameHeight;
            }
        }
        protected virtual void OnKill(int hitDirection, double damage)
        {
            SpawnGore();
            MotionBlurActive = false;
        }
        protected virtual void OnHit(int hitDirection, double damage, bool killed)
        {

        }
        protected void Kill(int hitDirection = 0, double dmg = 0)
        {
            npc.life = 0;
            HitEffect(hitDirection, dmg);
            Main.PlaySound(npc.DeathSound, npc.Center);
            if (PreNPCLoot() & !SpecialNPCLoot())
                NPCLoot();
        }
        protected virtual void SpawnGore()
        {
            if (GorePath != null)
            {
                for (int i = 0; i < Gores.Length; i++)
                {
                    for (int j = 0; j < Gores[i]; j++)
                    {
                        int goreNum = i + 1;
                        Gore.NewGore(npc.Center, GoreVelocity, mod.GetGoreSlot(GorePath + goreNum.ToString()));
                    }
                }
            }
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                OnKill(hitDirection, damage);
            }
            OnHit(hitDirection, damage, npc.life <= 0);
        }
    }
}