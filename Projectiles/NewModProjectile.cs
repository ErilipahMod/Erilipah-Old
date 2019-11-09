using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Erilipah.Projectiles
{
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
        protected Player Player
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
            => DamageType == DamageTypes.Thrown || Dimensions == AutoDimensions ? TextureTypes.ItemClone : TextureTypes.Default;
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
                    Dust dust = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height,
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
                    Dust dust = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType, Scale: TrailScale)];
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
                        Dust dust = Dust.NewDustPerfect(projectile.Center, DustType, Scale: TrailScale);
                        dust.noGravity = true;
                        if (NoDustLight)
                            dust.noLight = true;
                    }
                    if (DustTrailType == DustTrailTypes.Perfect)
                    {
                        Dust dust = Dust.NewDustPerfect(projectile.Center, DustType, Scale: TrailScale);
                        if (NoDustLight)
                            dust.noLight = true;
                    }
                    if (DustTrailType == (int)DustTrailTypes.Normal)
                    {
                        Dust dust = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType, Scale: TrailScale)];
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

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[projectile.type] * 0.5f);
                if (MotionBlurLength > MaxMotionBlurLength)
                {
                    MotionBlurLength = MaxMotionBlurLength;
                }
                for (int i = 0; i < Math.Min(projectile.oldPos.Length, MotionBlurLength); i++)
                {
                    Vector2 drawPos = projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, projectile.gfxOffY);
                    Color color = projectile.GetAlpha(drawColor) * ((MotionBlurLength - i) / (float)MotionBlurLength) * 0.5f;
                    SpriteEffects effects = projectile.oldSpriteDirection[i] == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: rect, color: color * ((255 - projectile.alpha) / 255f), rotation: projectile.rotation,
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
}