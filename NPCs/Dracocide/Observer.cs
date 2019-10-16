using Erilipah.Items.Dracocide;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.Dracocide
{
    public class Observer : ModNPC
    {
        private int DeathCounter
        {
            get { return (int)npc.ai[2]; }
            set { npc.ai[2] = value; }
        }
        private float AttackTime
        {
            get { return npc.ai[1]; }
            set { npc.ai[1] = value; }
        }
        private bool FoundTarget
        {
            get { return npc.ai[0] == 1; }
            set { npc.ai[0] = value ? 1 : 0; }
        }

        private Entity target = null;
        private readonly List<Entity> ignore = new List<Entity>();

        private readonly int[] projectiles = new int[9];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Observer");
            Main.npcFrameCount[npc.type] = 8;
        }
        public override void SetDefaults()
        {
            npc.dontTakeDamageFromHostiles = false;
            npc.lifeMax = 250;
            npc.defense = 45;
            npc.damage = 0;
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;

            npc.width = 64;
            npc.height = 62;

            npc.value = Item.buyPrice(0, 0, 30, 0);

            npc.MakeBuffImmune(BuffID.OnFire, BuffID.ShadowFlame, BuffID.CursedInferno, BuffID.Frostburn, BuffID.Chilled);
        }

        private bool CanSee(Entity target)
        {
            return Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height);
        }
        private void Say(string s) => CombatText.NewText(npc.Hitbox, Color.Crimson, s, true);

        private Entity FindTarget()
        {
            // #1 look for players
            npc.target = Helper.FindClosestPlayer(npc, 1050);
            if (npc.target > -1 && CanSee(Main.player[npc.target]) && !ignore.Contains(Main.player[npc.target]))
            {
                npc.netUpdate = true;
                Say("?");
                return Main.player[npc.target];
            }

            // #2 look for town NPCs and zombies
            npc.target = npc.FindClosestNPC(750, x => x.townNPC || x.aiStyle == 3);

            if (npc.target > -1 && CanSee(Main.npc[npc.target]) && !ignore.Contains(Main.npc[npc.target]))
            {
                npc.netUpdate = true;
                Say("?");
                return Main.npc[npc.target];
            }

            return null;
        }

        // Handle death
        public override void HitEffect(int hitDirection, double damage)
        {
            AttackTime += (int)damage * 20; // Anggerrry
            ignore.Clear();

            // If the death code hasn't already been run, run it
            if (npc.life <= 0 && DeathCounter == 0)
            {
                npc.life = 1;
                npc.dontTakeDamage = true;
                DeathCounter = 1; // Ensure that its death has started and make this code not run again

                npc.velocity = new Vector2(hitDirection * 7.5f, -3);
                npc.noGravity = false;
                npc.netUpdate = true;
            }
        }

        private int direction = 0;
        public override void AI()
        {
            Lighting.AddLight(npc.Center, 0.5f, 0.2f, 0);
            npc.spriteDirection = 1;
            // If the NPC is dying, run its death code & return the function
            if (DeathCounter > 0)
            {
                DeathCounter++;
                npc.noGravity = false;
                npc.netUpdate = true;
                npc.noTileCollide = false;
                npc.rotation += Helper.RadiansPerTick(2);

                npc.buffImmune[BuffID.OnFire] = false;
                npc.onFire = true;

                if (DeathCounter >= 80 || npc.collideY || npc.collideX || (npc.velocity.Y == 0 && DeathCounter > 30))
                {
                    npc.life = 0;
                    HitEffect(0, 0);
                    Main.PlaySound(npc.DeathSound, npc.Center);

                    NPCLoot();
                }
                return;
            }

            foreach (var entity in ignore.ToArray())
            {
                if (!entity.active || (entity is Player plr && plr.dead))
                    ignore.Remove(entity);
            }

            // Rotate slightly towards the direction it's moving
            npc.rotation = 0.03f * npc.velocity.X;

            // If we've found a target and are hurt, bolt outta there.
            if (FoundTarget && npc.life < npc.lifeMax)
            {
                target = null;
                npc.velocity += new Vector2(0.15f * direction, -0.05f);
                npc.noTileCollide = true;
                return;
            }

            // Finding a new target if the current one is dead/nonexistent, and that we haven't already found a target.
            if (target == null || !target.active || ignore.Contains(target) || (target is Player && (target as Player).dead))
            {
                AttackTime = 0;
                target = FindTarget();
            }

            #region Movement [excluding FoundTarget]

            // Wander if there is no target in-sight.
            if (target == null)
            {
                if (direction == 0)
                    direction = Main.rand.NextBool(2) ? 1 : -1;

                // Go to the direction it's facing.
                npc.velocity.X += 0.2f * direction;
                npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -1.8f, 1.8f);

                // If it hits a tile, it'll simply switch directions and repeat.
                if (npc.collideX || !Collision.CanHit(npc.position, npc.width, npc.height, npc.position + new Vector2(74, -5) * direction, npc.width, npc.height + 5))
                {
                    direction *= -1;
                    npc.velocity.X *= -0.5f;
                }
                if (npc.collideY || !Collision.CanHit(npc.position, npc.width, npc.height, npc.position + new Vector2(40 * direction, 200), npc.width + 10, npc.height + 10))
                {
                    npc.velocity.Y -= 0.15f;
                    npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -2.5f, 2.5f);
                }
                else
                {
                    npc.velocity.Y += 0.01f;
                    npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -1.4f, 1.4f);
                }
            }
            else
            {
                Vector2 goTo = target.Center + new Vector2(0, -200);

                if (CanSee(target))
                {
                    npc.velocity = npc.GoTo(goTo, 0.3f, 10);
                    if (Vector2.Distance(npc.Center, goTo) < 150)
                        npc.velocity *= 0.9f;
                }
                else
                    npc.velocity *= 0.99f;
            }
            #endregion

            // If there is no target, don't run any more code.
            if (target == null)
                return;

            // If any projectiles' ai[0]s are 1, it found a target!
            for (int i = 0; i < projectiles.Length; i++)
            {
                Projectile proj = Main.projectile[projectiles[i]];
                if (proj.ai[0] == 1 && proj.type == ProjectileType<ObserverProj>() && proj.active)
                {
                    FoundTarget = true;
                    Say("!");

                    npc.netUpdate = true;
                    SpawnBackup();
                    ignore.Add(target);
                    return;
                }
            }

            #region Attacking
            // If AttackTime has passed a threshold, fire!
            if (AttackTime > 450)
            {
                AttackTime++;

                if (AttackTime > 600 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toTarget = npc.Center.To(target.Center);
                    for (int i = -4; i < 5; i++)
                    {
                        Vector2 direction = toTarget.RotatedByRandom(MathHelper.ToRadians(i * 5));
                        projectiles[i + 4] = Projectile.NewProjectile(
                            npc.Center,
                            direction * 6,
                            ProjectileType<ObserverProj>(),
                            20,
                            0.5f,
                            Main.myPlayer,
                            ai1: target.whoAmI);
                    }

                    // Don't spam-fire.
                    AttackTime = 100;
                }
                return;
            }

            // If the player has not moved, swung an item, or has stayed out-of-sight, disengage.
            if (AttackTime < -400)
            {
                Say("...");
                ignore.Add(target);
                target = null;
                return;
            }

            // If the player uses an item, drastically increase the attack.
            if (CanSee(target) && target is Player player && player.itemAnimation > 0)
                AttackTime += 10;

            // If the target is moving at a speed above a threshold, increase attack. If below that threshold, the attack will decrease.
            if (CanSee(target) && target is Player)
                AttackTime += target.velocity.Length() * 0.5f - 1f;

            // If the target isn't a player but can still be seen
            else if (CanSee(target))
                AttackTime += target.velocity.Length();

            // If the target is out of direct line of sight, decrease attack per tick greatly.
            else
                AttackTime -= 2.5f;
            #endregion
        }

        private void SpawnBackup()
        {
            // If this is a multiplayer client, just don't run this code.
            if (Main.netMode == 1)
                return;

            npc.netUpdate = true;

            bool plr = target is Player;
            float numNPCs = plr ? 2.5f : 1.5f;
            if (Main.hardMode || Main.expertMode && plr)
                numNPCs++;

            int[] validDrones = { NPCType<Swarmer>(), NPCType<MiniSwarmer>(), mod.NPCType<AssaultDrone>() };
            if (npc.FindClosestNPC(2000, validDrones) != -1)
            {
                var existentFriends = Main.npc.Where(x => validDrones.Contains(x.type));
                foreach (var drone in existentFriends)
                {
                    drone.ai[1] = target.whoAmI;
                    drone.ai[2] = target is Player ? 1 : 0;

                    if (drone.type != NPCType<MiniSwarmer>())
                        numNPCs -= drone.life / drone.lifeMax; // Decrease the number of needed drones by however strong the existent drones are
                }
            }

            List<int> types = new List<int>() {
                NPCType<ArcCaster>(),
                NPCType<AssaultDrone>(),
                NPCType<AssaultDrone>(),
                NPCType<Swarmer>(),
            };

            for (int i = 0; i < numNPCs; i++)
            {
                int type = Main.rand.Next(types);
                types.Remove(type);
            }
            if (!types.Contains(NPCType<ArcCaster>()))
                SpawnBackupNPC(NPCType<ArcCaster>());
        }

        private int SpawnBackupNPC(int type)
        {
            int spawn =
                NPC.NewNPC(
                    (int)npc.Center.X + Main.rand.Next(-100, 100),
                    (int)npc.Center.Y - 500,
                    type,
                    ai1: target.whoAmI,
                    ai2: target is Player ? 1 : 0);
            Main.npc[spawn].alpha = 255;

            return spawn;
        }

        public override bool CheckActive()
        {
            int playerIndex = npc.FindClosestPlayer(3000);
            if (playerIndex == -1)
                return true;

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (AttackTime > 300)
                npc.Animate(frameHeight, 5, 4, 4);
            else
                npc.Animate(frameHeight, 5, 4);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemType<Dracocell>(), 1, 1, 70);
            Loot.DropItem(npc, ItemID.SilverCoin, 30, 50, 100, 2);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.hardMode)
            {
                return 0;
            }

            if (spawnInfo.playerInTown)
            {
                return SpawnCondition.Overworld.Chance * 0.01f;
            }

            return SpawnCondition.Overworld.Chance * 0.02f;
        }
    }

    public class ObserverProj : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Observer's Eye");
        }
        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;

            projectile.tileCollide = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = 300;

            projectile.penetrate = -1;
            projectile.hostile = true;
            projectile.friendly = true;

            projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            projectile.owner = Main.myPlayer;
            projectile.hostile = true;
            projectile.friendly = true;
            if (projectile.ai[1]++ % 5 == 0)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<DracocideDust>(), Main.rand.NextVector2Unit() * 0.05f);
                Lighting.AddLight(projectile.Center, 0.20f, 0.10f, 0f);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            projectile.ai[0] = 1;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.ai[0] = 1;
        }

        public override bool? CanHitNPC(NPC target) => target.type != NPCType<Observer>();
    }
}
