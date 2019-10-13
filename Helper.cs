using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    internal static class Helper
    {
        internal static bool Chance(this Terraria.Utilities.UnifiedRandom random, float chance) => chance >= 1f || random.NextFloat() < chance;
        internal const string Invisible = "Terraria/Projectile_294";

        internal static void SimpleRecipe<Result>(this Mod mod, int tile, int ingredient, int ingredientCount) where Result : ModItem
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ingredient, ingredientCount);
            recipe.AddTile(tile);
            recipe.SetResult(mod.ItemType<Result>());
            recipe.AddRecipe();
        }

        /// <summary>
        /// Finds an ammo item in a player's inventory.
        /// </summary>
        /// <returns>Null if nothing is found; otherwise, the ammo item found.</returns>
        internal static Item FindAmmo(this Player player, Predicate<Item> conditions = null)
        {
            for (int i = 54; i < 58; i++)
            {
                Item ammo = player.inventory[i];
                if (ammo.ammo > 0 && !ammo.IsAir && (conditions == null || conditions(ammo)))
                    return ammo;
            }
            return player.inventory.FirstOrDefault(ammo => ammo.ammo > 0 && !ammo.IsAir && (conditions == null || conditions(ammo)));
        }
        internal static Item FindEquip(this Player player, int type)
        {
            for (int i = 0; i < player.armor.Length; i++)
            {
                if (player.armor[i].type == type)
                    return player.armor[i];
            }
            return null;
        }

        internal static float Brightness(this Player player)
        {
            Rectangle loc = new Rectangle(player.getRect().X / 16, player.getRect().Y / 16, 2, 3);
            return Lighting.BrightnessAverage(loc.X, loc.Y, loc.Width, loc.Height);
        }
        internal static bool InErilipah(this Player player) => player.GetModPlayer<ErilipahPlayer>().ZoneErilipah || player.GetModPlayer<ErilipahPlayer>().ZoneLostCity;
        internal static bool InLostCity(this Player player) => player.GetModPlayer<ErilipahPlayer>().ZoneLostCity;
        internal static bool IsErilipahTile(this Tile tile)
        {
            var mod = Erilipah.Instance;
            return tile.type == mod.TileType<InfectedClump>() ||
                tile.type == mod.TileType<SpoiledClump>() ||
                tile.type == mod.TileType<TaintedRubble>() ||
                tile.type == mod.TileType<TaintedBrick>() ||
                tile.type == mod.TileType<Items.Crystalline.CrystallineTileTile>();
        }
        internal static bool IsErilipahWall(this Tile tile)
        {
            var mod = Erilipah.Instance;
            return tile.wall == mod.WallType<InfectedClump.InfectedClumpWall>() ||
                tile.wall == mod.WallType<TaintedBrick.TaintedBrickWall>();
        }

        internal static InfectionPlr I(this Player player) => player.GetModPlayer<InfectionPlr>();

        internal static void SetInfecting(this NPC npc, float infecting) => npc.GetGlobalNPC<InfectionPlr.InfectionNPC>().infecting = infecting;
        internal static void SetInfecting(this Projectile npc, float infecting) => npc.GetGlobalProjectile<InfectionPlr.InfectionProj>().infecting = infecting;

        internal static int AutoValue(this Item item)
        {
            int useSpeed = (item.useTime > 0 ? item.useTime : 60);

            return (int)(Math.Max(item.damage, 1) * Math.Max(item.knockBack / 2, 1) * Math.Max(item.defense, 1) *
                    (60 / useSpeed) * item.rare * 5 + Math.Pow(MathHelper.Clamp(item.crit, 0, 96), 2));
        }
        internal static int AutoValue(this NPC npc)
        {
            return (int)(Math.Max(npc.damage, 1) * (2f - npc.knockBackResist) + (npc.defense / 5 + 1) *
                    (npc.lifeMax / 20));
        }
        internal static float RadiansPerTick(float rotationsPerSecond) => MathHelper.Pi / (60 / rotationsPerSecond) * 2;
        internal const float MPH = (44f / 225f);

        internal static bool BossPresent()
        {
            return Main.npc.Any(x => (x.boss || x.type == NPCID.EaterofWorldsHead) && x.active);
        }
        internal static bool InTiles(this Entity entity)
        {
            return Collision.SolidTiles(
                (int)entity.position.X / 16, (int)(entity.position.X + entity.width) / 16,
                (int)entity.position.Y / 16, (int)(entity.position.Y + entity.height) / 16);
        }

        internal static void Animate(this Projectile proj, int ticksPerFrame, int numFrames, int startingFrame = 0)
        {
            if (++proj.frameCounter % ticksPerFrame == 0)
                proj.frame = (proj.frame + 1) % (numFrames + startingFrame);
            if (proj.frame < startingFrame)
                proj.frame = startingFrame;
        }
        internal static void Animate(this NPC npc, int frameHeight, int frameDelay, int numFrames, int startingFrame = 0)
        {
            if (++npc.frameCounter % frameDelay == 0)
            {
                npc.frame.Y += frameHeight;
                npc.frame.Y %= numFrames * frameHeight;
                if (npc.frame.Y < startingFrame)
                    npc.frame.Y = startingFrame * frameHeight;
            }
        }
        internal static void Animate(this NPC npc, int frameHeight, int frameDelay)
        {
            if (++npc.frameCounter % frameDelay == 0)
            {
                npc.frame.Y += frameHeight;
                npc.frame.Y %= Main.npcFrameCount[npc.type] * frameHeight;
            }
        }

        internal static void DrawGlowmask(this ModNPC npc, SpriteBatch spriteBatch, Color color)
        {
            string texture = npc.Texture.Remove(0, 9) + "_Glow";
            DrawGlowmask(npc.npc, spriteBatch, texture, color);
        }
        internal static void DrawGlowmask(this NPC npc, SpriteBatch spriteBatch, string texture, Color color)
        {
            Texture2D texture2d = ModContent.GetTexture("Erilipah/" + texture);
            Vector2 drawOrigin = npc.frame.Size() / 2;
            Vector2 drawPos = npc.Center - Main.screenPosition;

            spriteBatch.Draw(
                texture2d,
                drawPos,
                npc.frame,
                color * ((255 - npc.alpha) / 255f),
                npc.rotation,
                drawOrigin,
                npc.scale,
                npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f);
        }
        internal static void DrawTrail(this NPC npc, SpriteBatch spriteBatch, int length, Color drawColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            Vector2 drawOrigin = npc.frame.Size() / 2;

            if (length > npc.oldPos.Length) length = npc.oldPos.Length;
            for (int i = 0; i < length; i++)
            {
                Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, npc.gfxOffY);
                Color color = npc.GetAlpha(drawColor) * ((length - i) / (float)length);
                SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                spriteBatch.Draw(
                    texture: texture, position: drawPos, sourceRectangle: npc.frame, color: color, rotation: npc.rotation,
                    origin: drawOrigin, scale: npc.scale, effects: effects, layerDepth: 0
                    );
            }
        }
        internal static void DrawNPC(this NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(Main.npcTexture[npc.type], npc.Center - Main.screenPosition,
                npc.frame, drawColor * ((255 - npc.alpha) / 255f), npc.rotation, npc.frame.Size() / 2, npc.scale,
                npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        internal static void MakeBuffImmune(this NPC npc, params int[] buffs)
        {
            for (int i = 0; i < buffs.Length; i++)
            {
                npc.buffImmune[i] = true;
            }
        }
        internal static void MakeDebuffImmune(this NPC npc)
        {
            for (int i = 0; i < npc.buffImmune.Length; i++)
            {
                if (Main.debuff[i])
                {
                    npc.buffImmune[i] = true;
                }
            }
        }
        internal static void Heal(this NPC npc, int healAmount)
        {
            if (healAmount >= 1 && npc.life < npc.lifeMax)
            {
                if (npc.life + healAmount <= npc.lifeMax)
                {
                    npc.HealEffect(healAmount);
                    npc.life += healAmount;
                }
                else if (npc.life < npc.lifeMax)
                {
                    npc.HealEffect(npc.lifeMax - npc.life);
                    npc.life += npc.lifeMax - npc.life;
                }
            }
        }
        internal static void Heal(this Player player, int healAmount)
        {
            if (healAmount >= 1 && player.statLife < player.statLifeMax2)
            {
                if (player.statLife + healAmount <= player.statLifeMax2)
                {
                    player.HealEffect(healAmount);
                    player.statLife += healAmount;
                }
                else
                {
                    player.HealEffect(player.statLifeMax2 - player.statLife);
                    player.statLife += player.statLifeMax2 - player.statLife;
                }
            }
        }

        public static bool ValidTop(this Tile t)
        {
            bool isSolid = Main.tileSolidTop[t.type] || Main.tileSolid[t.type];
            bool notSloped = (t.slope() == 0 || t.bottomSlope()) && !t.halfBrick();
            return t.active() && isSolid && notSloped;
        }

        public static bool ValidTop(int i, int j)
        {
            Tile t = Main.tile[i, j];
            bool isSolid = Main.tileSolidTop[t.type] || Main.tileSolid[t.type];
            bool notSloped = (t.slope() == 0 || t.topSlope()) && !t.halfBrick();
            return t.active() && isSolid && notSloped;
        }

        internal static Vector2 To(this Vector2 from, Vector2 to, float speed = 1)
        {
            Vector2 velocity;
            Vector2 direction = to - from;
            float distance = direction.Length();

            if (distance > speed)
                velocity = direction * (speed / distance);
            else
                velocity = direction;
            return velocity;
        }

        ///// <param name="accel">The speed, in pixels per tick, to accelerate towards the target.</param>
        ///// <param name="decel">The speed, as a multiplier, to decelerate while nearing the target.</param>
        ///// <param name="maxSpeed">The maximum speed the entity can reach.</param>
        ///// <param name="slowingDist">The distance at which the entity starts to slow down.</param>
        ///// <returns></returns>
        //internal static Vector2 GoTo(this Entity entity, Vector2 to, float accel, float decel, float maxSpeed, float slowingDist = 80f)
        //{
        //    Vector2 velocity = entity.velocity;

        //    float distance = Math.Min(80f, Vector2.Distance(entity.Center, to));
        //    float slowDown = MathHelper.Lerp(decel, 1f, distance / slowingDist);

        //    if (to.Y - entity.Center.Y > slowingDist)
        //        velocity.Y += accel;
        //    else if (entity.Center.Y - to.Y > slowingDist)
        //        velocity.Y -= accel;
        //    else
        //        velocity.Y *= slowDown;

        //    if (to.X - entity.Center.X > slowingDist)
        //        velocity.X += accel;
        //    else if (entity.Center.X + to.X > slowingDist)
        //        velocity.X -= accel;
        //    else
        //        velocity.X *= slowDown;

        //    velocity = Vector2.Clamp(velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);

        //    return velocity;
        //}
        internal static Vector2 GoTo(this Entity entity, Vector2 to, Vector2 accel, float maxSpeed)
        {
            Vector2 velocity = entity.velocity;

            Vector2 targetPos = (to - entity.Center).SafeNormalize(Vector2.Zero);
            velocity += targetPos * accel;

            velocity = Vector2.Clamp(velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);

            return velocity;
        }
        internal static Vector2 GoTo(this Entity entity, Vector2 to, float accel, float maxSpeed)
        {
            Vector2 velocity = entity.velocity;

            Vector2 targetPos = (to - entity.Center).SafeNormalize(Vector2.Zero);
            velocity += targetPos * accel;

            velocity = Vector2.Clamp(velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);

            return velocity;
        }
        internal static Vector2 GoTo(this Entity entity, Vector2 to, float accel)
        {
            Vector2 velocity = entity.velocity;

            Vector2 targetPos = (to - entity.Center).SafeNormalize(Vector2.Zero);
            velocity += targetPos * accel;

            return velocity;
        }

        internal static int FindClosestPlayer(Vector2 pos, float viewDistance)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player tart = Main.player[i];
                if (tart.active && !tart.dead)
                {
                    float distCurrent = Vector2.Distance(tart.Center, pos);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestPlayer(this Entity entity, float viewDistance)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player tart = Main.player[i];
                if (tart.active && !tart.dead && Main.player[i] != entity)
                {
                    float distCurrent = Vector2.Distance(tart.Center, entity.Center);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestPlayer(this Entity entity, float viewDistance, Predicate<Player> conditions)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player tart = Main.player[i];
                if (tart.active && !tart.dead && Main.player[i] != entity && conditions(tart))
                {
                    float distCurrent = Vector2.Distance(tart.Center, entity.Center);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestNPC(Vector2 pos, float viewDistance, Predicate<NPC> conditions)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC tart = Main.npc[i];
                if (tart.active && !(tart.immortal || tart.dontTakeDamage) && conditions(tart))
                {
                    float distCurrent = Vector2.Distance(tart.Center, pos);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestNPC(this Entity entity, float viewDistance, Predicate<NPC> conditions)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC tart = Main.npc[i];
                if (tart.active && tart != entity && !(tart.immortal || tart.dontTakeDamage) && conditions(tart))
                {
                    float distCurrent = Vector2.Distance(tart.Center, entity.Center);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestNPC(this Entity entity, float viewDistance, bool? targetHostiles = true, bool ignoreInvincible = true)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC tart = Main.npc[i];
                if (tart.active && tart != entity &&
                    (targetHostiles == null ? true : ((bool)targetHostiles ? !tart.friendly : tart.friendly)) &&
                    (ignoreInvincible && !(tart.immortal || tart.dontTakeDamage) || !ignoreInvincible))
                {
                    float distCurrent = Vector2.Distance(tart.Center, entity.Center);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }
        internal static int FindClosestNPC(this Entity entity, float viewDistance, params int[] npcTypes)
        {
            int closest = -1;
            float distClosest = viewDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC tart = Main.npc[i];
                if (tart.active && tart != entity && npcTypes.Contains(tart.type))
                {
                    float distCurrent = Vector2.Distance(tart.Center, entity.Center);
                    if (distCurrent < distClosest)
                    {
                        closest = i;
                        distClosest = distCurrent;
                    }
                }
            }
            return closest;
        }

        /// <summary>
        /// Reflects a projectile.
        /// </summary>
        /// <param name="proj">The projectile.</param>
        /// <param name="magnitude">How the speed of the reflected projectile is altered (multiplied by this).</param>
        /// <param name="onlyFriendly">True for only reflecting friendlies, false for hostiles, null for anything.</param>
        /// <param name="switchStats">0 for only switching friendly status, 1 for hostile, 2 for both, anything else for neither.</param>
        internal static bool Reflect(this Projectile proj, float magnitude, params bool[] conditions)
        {
            bool validType = proj.aiStyle == 0 || proj.aiStyle == 1 || proj.aiStyle == 2 || proj.aiStyle == 8 || proj.aiStyle == 21 || proj.aiStyle == 24 || proj.aiStyle == 28 || proj.aiStyle == 29 || proj.aiStyle == 131;
            validType &= !proj.WipableTurret && !proj.hide && !proj.minion;

            if (validType && proj.active && conditions.All(x => x))
            {
                if (!proj.friendly || !proj.hostile)
                {
                    proj.friendly = !proj.friendly;
                    proj.hostile = !proj.hostile;
                }

                proj.velocity *= -Math.Abs(magnitude);
                return true;
            }
            return false;
        }
        internal static bool Reflect(this NPC npc, float magnitude, params bool[] conditions)
        {
            if (npc.active && !npc.boss && !npc.netAlways && !npc.dontCountMe && conditions.All(x => x))
            {
                npc.velocity *= -Math.Abs(magnitude);
                return true;
            }
            return false;
        }

        internal static Projectile[] FireInCircle(Vector2 position, float numProj, int type, int damage, float speed = 6.5f, float kb = 1, float degreesHalved = 180, int owner = 255, float ai0 = 0, float ai1 = 0)
        {
            if (Main.netMode != 1)
            {
                float rotation = MathHelper.ToRadians(degreesHalved);
                float numProj1 = numProj + 1;
                Projectile[] proj = new Projectile[(int)numProj1];
                for (int i = 0; i < numProj1; i++)
                {
                    float Speed1 = speed;  //projectile speed
                    Vector2 perturbedSpeed = new Vector2(Speed1).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / Math.Max(numProj1 - 1, 1)));
                    proj[i] = Projectile.NewProjectileDirect(position, perturbedSpeed, type, damage, kb, owner, ai0, ai1);
                    proj[i].ai[0] = ai0;
                }
                return proj;
            }
            return new Projectile[0];
        }
    }
}
