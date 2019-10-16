using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Sanguine
{
    public class SanguineJavelin : ModItem
    {
        public override void SetDefaults()
        {
            // Alter any of these values as you see fit, but you should probably keep useStyle on 1, as well as the noUseGraphic and noMelee bools
            item.shootSpeed = 11;
            item.damage = 28;
            item.knockBack = 6;
            item.useStyle = 1;
            item.useAnimation = 23;
            item.useTime = 23;
            item.width =
                item.height = 48;
            item.maxStack = 999;
            item.rare = 3;

            item.consumable = true;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.autoReuse = true;
            item.thrown = true;

            item.UseSound = SoundID.Item1;
            item.value = Item.sellPrice(silver: 10);
            item.shoot = ProjectileType<SanguineJavelinProj>();
        }
        public override void SetStaticDefaults()
            => Tooltip.SetDefault("Heals you for 15% of damage dealt");

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.GetItem("SanguineAlloy"), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this, 125);
            recipe.AddRecipe();
        }
    }
    public class SanguineJavelinProj : ModProjectile
    {
        public override string Texture => mod.Name + "/Items/Sanguine/SanguineJavelinProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Javelin");
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = 4;
            projectile.hide = true;
        }

        // See ExampleBehindTilesProjectile. 
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            // If attached to an NPC, draw behind tiles (and the npc) if that NPC is behind tiles, otherwise just behind the NPC.
            if (projectile.ai[0] == 1f) // or if(isStickingToTarget) since we made that helper method.
            {
                int npcIndex = (int)projectile.ai[1];
                if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
                {
                    if (Main.npc[npcIndex].behindTiles)
                        drawCacheProjsBehindNPCsAndTiles.Add(index);
                    else
                        drawCacheProjsBehindNPCs.Add(index);
                    return;
                }
            }
            // Since we aren't attached, add to this list
            drawCacheProjsBehindProjectiles.Add(index);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            // For going through platforms and such, javelins use a tad smaller size
            width = height = 10; // notice we set the width to the height, the height to 10. so both are 10
            return true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Inflate some target hitboxes if they are beyond 8,8 size
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            // Return if the hitboxes intersects, which means the javelin collides or not
            return projHitbox.Intersects(targetHitbox);
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(0, (int)projectile.position.X, (int)projectile.position.Y); // Play a death sound
            Vector2 usePos = projectile.position; // Position to use for dusts
                                                  // Please note the usage of MathHelper, please use this! We subtract 90 degrees as radians to the rotation vector to offset the sprite as its default rotation in the sprite isn't aligned properly.
            Vector2 rotVector =
                (projectile.rotation - MathHelper.ToRadians(degreesRotate)).ToRotationVector2(); // rotation vector to use for dust velocity
            usePos += rotVector * 16f;

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(projectile.Center, 1, 1, mod.DustType("Sanguine"));
            }
            // Drop a javelin item, 1 in 18 chance (~5.5% chance)
            int item = Main.rand.NextBool(18) ?
                Item.NewItem(projectile.Hitbox, ItemType<SanguineJavelin>()) : 0;

            // Sync the drop for multiplayer
            // Note the usage of Terraria.ID.MessageID, please use this!
            if (Main.netMode == 1 && item >= 0)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
            }
        }

        // Here's an example on how you could make your AI even more readable, by giving AI fields more descriptive names
        // These are not used in AI, but it is good practice to apply some form like this to keep things organized

        // Are we sticking to a target?
        // WhoAmI of the current target
        public float targetWhoAmI
        {
            get { return projectile.ai[1]; }
            set { projectile.ai[1] = value; }
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            if (!target.immortal && !target.dontTakeDamage && !target.dontCountMe)
            {
                Player player = Main.player[projectile.owner];
                player.Heal((int)(damage * 0.15f));
            }
        }

        // Added these 2 constant to showcase how you could make AI code cleaner by doing this
        // Change this number if you want to alter how long the javelin can travel at a constant speed
        private const float maxTicks = 45f;

        // Change this number if you want to alter how the alpha changes
        private const int alphaReduction = 25;

        private const float degreesRotate = 90f;

        public override void AI()
        {
            // Slowly remove alpha as it is present
            if (projectile.alpha > 0)
            {
                projectile.alpha -= alphaReduction;
            }
            // If alpha gets lower than 0, set it to 0
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            // If ai0 is 0f, run this code. This is the 'movement' code for the javelin as long as it isn't sticking to a target
            targetWhoAmI += 1f;
            // For a little while, the javelin will travel with the same speed, but after this, the javelin drops velocity very quickly.
            if (targetWhoAmI >= maxTicks)
            {
                // Change these multiplication factors to alter the javelin's movement change after reaching maxTicks
                float velXmult = 0.98f; // x velocity factor, every AI update the x velocity will be 98% of the original speed
                float
                    velYmult = 0.35f; // y velocity factor, every AI update the y velocity will be be 0.35f bigger of the original speed, causing the javelin to drop to the ground
                targetWhoAmI = maxTicks; // set ai1 to maxTicks continuously
                projectile.velocity.X = projectile.velocity.X * velXmult;
                projectile.velocity.Y = projectile.velocity.Y + velYmult;
            }
            // Make sure to set the rotation accordingly to the velocity, and add some to work around the sprite's rotation
            projectile.rotation =
                projectile.velocity.ToRotation() +
                MathHelper.ToRadians(
                    degreesRotate); // Please notice the MathHelper usage, offset the rotation by 90 degrees (to radians because rotation uses radians) because the sprite's rotation is not aligned!
        }
    }
}
