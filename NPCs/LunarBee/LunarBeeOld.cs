// V1
/*using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Bosses.LunarBee
{
    [AutoloadBossHead]
    public class LunarBee : ModNPC
    {
        public override void SetDefaults()
        {
            npc.width = 124;
            npc.height = 100;
            npc.damage = 30;
            npc.defense = 18;
            npc.lifeMax = 1900;
            npc.value = 1000f;
            npc.knockBackResist = 0f;
            npc.npcSlots = 3;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.noTileCollide = true;
            //boss stuff below//
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            NPCHelper.ImmuneToDebuffs(npc);
            npc.timeLeft = 28000;
            bossBag = mod.ItemType("LunarBeeBag"); 
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/MoonLanderB");
        }

        public override void OnHitPlayer(Player player, int dmgDealt, bool crit)
        {
            player.AddBuff(mod.BuffType("LunarBreakdown"), 80);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunaemia");
            Main.npcFrameCount[npc.type] = 8; 
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int j = 0; j <= damage / 2; j++)
                Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType<Dusts.MoonFire>(), hitDirection * 3, -1f, 0, default(Color), 1.1f);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = 2500;
            npc.damage = 35;
            npc.defense = 26;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            if (!ErilipahWorld.downedLunarBee)
            {
                //World.LunarBiome.MakeLunarBiomes();
                ErilipahWorld.downedLunarBee = true;
                Main.NewText("Shattered chunks of the moon drift down to close-orbit...", 100, 200, 255);
            }
            potionType = ItemID.LesserHealingPotion;
            Player closePlr = Main.player[npc.FindClosestPlayer()];
            closePlr.QuickSpawnItem(mod.ItemType("CosmicArtifact"));
            if (Main.expertMode)
            {
                npc.DropBossBags();
            }
            else if (!Main.expertMode)
            {
                Loot.DropItem(npc, mod.ItemType("LunacritaStaff"), percent: 25);
            }
        }

        public override bool CheckActive()
        {
            if (npc.Distance(Main.player[npc.target].Center) > 2000) return true;
            return false;
        }
        bool chooseSide;
        public override void AI()
        {
            Player player = Main.player[npc.target];
            Lighting.AddLight(npc.Center, 0f, 1f, 1f);
            Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("MoonFire"));
            npc.TargetClosest(true);
            npc.ai[1]++; // inc timer
            //npc.ai[2] is charge timer
            if (Main.dayTime || player.dead || !player.active && player != null && npc.target >= 0)
            {
                npc.velocity.X = 0;
                npc.velocity.Y -= 0.1f;
                if (npc.timeLeft > 5) npc.timeLeft = 5;
            }
            else Movement();
            AttackPattern();
            if (npc.ai[1] % 560 == 0)
            {
                npc.ai[0]++;
                npc.ai[0] %= 4;
                chooseSide = Main.rand.NextBool(2);
            }
        }

        private void AttackPattern()
        {
            Player plr = Main.player[npc.target];
            int dmg = Main.rand.Next(4, 7);

            if (npc.ai[0] == 0 && npc.ai[1] % 60 == 0)
            {
                EntityHelper.FireAtTarget(npc.Center, plr.Center, mod.ProjectileType("CrystalChunk"), (int)(dmg * 1.3), 6, 2);
            }
            else if (npc.ai[0] == 1 && npc.ai[1] % 80 == 0 && NPC.CountNPCS(mod.NPCType("MoonWasp")) <= 4)
            {
                int succ = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType("MoonWasp"));
                Main.npc[succ].damage = dmg / 2;
            }

            else if (npc.ai[0] == 2 && npc.ai[1] % 110 == 0)
            {
                EntityHelper.FireAtTarget(npc.Center, plr.Center, mod.ProjectileType("CrystalShard"), (int)(dmg * 0.9), 4, 2);
            }
            else if (npc.ai[0] == 3 && npc.ai[1] % 38 == 0)
            {
                //Projectile[] projs = 
                EntityHelper.FireInCircle(npc.Center, Main.rand.Next(3, 25), mod.ProjectileType("CrystalShard"), (int)(dmg * 0.6), 4.5f, 1);
                //for (int a = 0; a < projs.Length; a++)
                //{
                //    projs[a].alpha = 255;
                //}/
            }
        }

        bool plrToTheRight;
        private void Movement()
        {
            Player plr = Main.player[npc.target];
            float angPlr = npc.AngleTo(plr.position);
            Vector2 findAboveTarget = new Vector2(plr.position.X + (chooseSide ? 50 : -50), plr.position.Y - 360);
            Vector2 findSideTarget = new Vector2(plr.position.X + (chooseSide ? 400 : -400), plr.position.Y - 100);

            if (npc.ai[0] == 0)
            {
                // if (Vector2.Distance(npc.position, findAboveTarget) > 15) //checking so the NPC doesn't spaz out
                //    npc.velocity = (findAboveTarget - npc.position) //the directional angle/Vector2 for the npc.velocity. endpos - startpos.
                //    .SafeNormalize //method that Normalizes (makes Magnitude = 0, Length = 1, and if dividing by zero returns defaultvalue.
                //    (Vector2.Zero) //default value.
                //    * 6; //new magnitude.
                else chooseSide = !chooseSide;
                Vector2 direction = (findAboveTarget - npc.position);
                float distance = direction.Length();
                float speed = 3;

                if (distance > speed)
                    npc.velocity = direction * (speed / distance);
                else
                    npc.velocity = direction;
                npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;
            }
            else if (npc.ai[0] == 1)
            {
                Vector2 direction = (findSideTarget - npc.position);
                float distance = direction.Length();
                float speed = 3;

                if (distance > speed)
                    npc.velocity = direction * (speed / distance);
                else
                    npc.velocity = direction;
                npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

                //spawns minions (moon wasps)
            }
            else if (npc.ai[0] == 2)
            {
                const float phase1accX = 0.5f;
                const float phase1accY = 0.7f;
                float targetposX = chooseSide ? plr.position.X + 200 : plr.position.X - 200;

                if (npc.position.X > targetposX) npc.velocity.Y -= phase1accX;
                else if (npc.position.X < targetposX) npc.velocity.Y += phase1accX;

                if (plr.position.Y < npc.position.Y) npc.velocity.Y -= phase1accY;
                else if (plr.position.Y > npc.position.Y) npc.velocity.Y += phase1accY;

                if (npc.ai[2] < 55) npc.ai[2]++; // charge cooldown timer
                if (npc.ai[2] >= 55 && Math.Abs(npc.Center.Y - plr.Center.Y) <= 70)
                {
                    if (npc.ai[3] == 0)
                    {
                        npc.ai[3] = 1;
                        plrToTheRight = plr.position.X > npc.position.X;
                    }
                    npc.velocity.X = plrToTheRight ? 18.5f : -18.5f;
                    npc.velocity.Y = 0;
                    if (++npc.ai[2] > 90)
                    {
                        npc.ai[2] = 0; npc.ai[3] = 0;
                    }
                }
                else { npc.ai[3] = 0; }

                //charges
            }
            else if (npc.ai[0] == 3)
            {
                npc.velocity *= 0.01f;
                npc.spriteDirection = plr.position.X > npc.position.X ? -1 : 1;
                
                //shoots mini stingers al around
            }
            if (npc.ai[0] != 2) npc.rotation = 0;
        }

        public override void FindFrame(int frameHeight)
        {
            const int ticksPerFrame = 7;
            ++npc.frameCounter;

            if (npc.frameCounter % ticksPerFrame == 0 && 
                npc.ai[2] >= 55 && Math.Abs(npc.Center.Y - Main.player[npc.target].Center.Y) <= 70)
            {
                npc.frame.Y = ((npc.frame.Y / frameHeight + 1) % 4 * frameHeight) + (4 * frameHeight);
            }

            else if (npc.frameCounter % ticksPerFrame == 0)
            {
                npc.frame.Y = (npc.frame.Y / frameHeight + 1) % 4 * frameHeight;
            }
        }
    }
}*/

// V2
//    #region Stats
//    protected override int MaxLife => 4200;
//    protected override int Damage => 16;
//    protected override int Defense => 3;
//    protected override int Width => 124;
//    protected override int Height => 100;
//    protected override int NPCFrameCount => 8;
//    protected override int? TreasureBag => mod.ItemType<Items.LunarBee.LunarBeeBag>();
//    protected override string TitleOfMusic => "Moon_Lander_B_Cut";
//    protected override string Title => "Lunaemia";

//    protected override bool CanDespawn => TDist > 2200 || Main.dayTime || !AnyPlayersAlive;
//    #endregion
//    private float rotateSpeed = 0;
//    protected override void WhenBossCanDespawn()
//    {
//        if (!Main.dayTime)
//            return;
//        npc.velocity.Y -= 0.15f;
//        npc.rotation += (rotateSpeed += Helper.RadiansPerTick(0.1f));
//        npc.dontTakeDamage = true;
//    }
//    public override void SetStaticDefaults()
//    {
//        base.SetStaticDefaults();
//        Main.npcFrameCount[npc.type] = 8;
//    }
//    public override void FindFrame(int frameHeight)
//    {
//        const int ticksPerFrame = 7;
//        ++npc.frameCounter;

//        if (dashing == 2 && Charging)
//        {
//            npc.spriteDirection = npc.direction = npc.velocity.X > 0 ? 1 : -1;
//            if (npc.frameCounter % ticksPerFrame == 0)
//            {
//                npc.frame.Y += frameHeight;
//                npc.frame.Y %= 4 * frameHeight;
//                npc.frame.Y += 4 * frameHeight;
//            }
//        }

//        else
//        {
//            npc.spriteDirection = npc.direction = npc.Center.X < TCen.X ? 1 : -1;
//            if (npc.frameCounter % ticksPerFrame == 0)
//            {
//                npc.frame.Y += frameHeight;
//                npc.frame.Y %= 4 * frameHeight;
//            }
//        }
//    }
//    public override void BossLoot(ref string name, ref int potionType)
//    {
//        base.BossLoot(ref name, ref potionType);
//        if (!Main.expertMode)
//        {
//            Loot.DropItem(npc, mod.ItemType("SynthesizedLunaesia"), 10, 18);
//        }
//        else
//        {
//            npc.DropBossBags();
//        }
//    }

//    private const int
//        summonMax = 4,
//        numDashes = 3,
//        numRingFires = 7,
//        timeLargeBolts = (int)(7.5 * 60);
//    private const float
//        dashSpeed = 50f * MPH,
//        MPH = (44f / 255f);

//    private bool Charging => Attack == (int)AI0.Dash;

//    private bool RingFiring => Attack == (int)AI0.SmallChunks && npc.life < npc.lifeMax / 2f;

//    private bool Summoning => Attack == (int)AI0.Summon &&
//        NPC.CountNPCS(mod.NPCType("Lunacritas"))
//        < summonMax * ((npc.life < npc.lifeMax / 2) ? 2 : 1);

//    private int DustType => mod.DustType("MoonFire");

//    private int dashing = 0;

//    private enum AI0
//    {
//        Dash, LargeChunks, Summon, SmallChunks
//    }

//    private float Attack { get { return npc.ai[0]; } set { npc.ai[0] = value; } }

//    protected override void BossAI()
//    {
//        if (Timer % 60 * (15) == 0)
//        {
//            NewAttack();
//        }
//        if (Target.dead || !Target.active)
//        {
//            foreach (Player plr in Main.player)
//            {
//                if (!plr.dead && plr.active)
//                {
//                    Target = plr;
//                    npc.netUpdate = true;
//                    return;
//                }
//            }
//            Despawn(mod.DustType<Items.LunarBee.MoonFire>());
//        }
//        if (!Charging)
//        {
//            npc.damage = Damage;
//        }
//        else
//        {
//            npc.damage = Damage * 4;
//        }
//    }

//    private void NewAttack()
//    {
//        Attack++;
//        Attack %= 4;

//        // Setup
//        npc.ai[2] = 0;
//        npc.ai[3] = 0;

//        if (npc.ai[1] <= 0)
//            npc.ai[1] = -1;
//        else
//            npc.ai[1] = 1;
//        npc.netUpdate = true;
//    }

//    void Clamp(float max)
//    {
//        npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -max, Vector2.One * max);
//    }
//    protected override void Movement()
//    {
//        Vector2 go;
//        if (CanDespawn)
//            return;
//        if (Charging)
//        {
//            Dash();
//        }

//        // Firing large chunks
//        else if (Attack == (int)AI0.LargeChunks)
//        {
//            go = new Vector2(
//                TCen.X + (250 * npc.ai[1]),
//                TCen.Y - 200
//                );

//            if (TDist > 1000)
//            {
//                npc.velocity = npc.GoTo(go, npc.Center, 0.2f);
//                Clamp(18 * Helper.MPH);
//            }
//            else
//            {
//                npc.velocity *= 0.9f;
//            }
//        }

//        // Summoning
//        else if (Summoning)
//        {
//            go = new Vector2(
//                TCen.X + (400 * -npc.ai[1]),
//                TCen.Y - 250
//                );

//            npc.velocity = npc.GoTo(go, npc.Center, 0.2f);
//            Clamp(15 * Helper.MPH);
//        }

//        // Firing rings of shards
//        else if (RingFiring)
//        {
//            if (TDist >= 1000)
//            {
//                go = TCen + new Vector2(100 * npc.ai[1], -320);
//            npc.velocity = npc.GoTo(go, npc.Center, 0.2f);
//            Clamp(15 * Helper.MPH);                }
//            else
//            {
//                npc.velocity *= 0.9f;
//            }
//        }

//        else
//        {
//            NewAttack();
//        }
//    }

//    private void Dash()
//    {
//        if (npc.ai[1] <= 0)
//            npc.ai[1] = -1;
//        else
//            npc.ai[1] = +1;

//        Vector2 go;
//        float xPos = 0;
//        const int dashXDist = 550;
//        // MoveTo dash position.
//        if (dashing == 0)
//        {
//            go = new Vector2(
//                TPos.X + (dashXDist * npc.ai[1]),
//                TPos.Y + 10
//                );

//            MoveTo(go);

//            if (Vector2.Distance(npc.Center, go) <= 10)
//            {
//                npc.ai[2] = 0;
//                dashing = 1;
//                npc.netUpdate = true;
//            }
//        }

//        // Channeling before dashing.
//        if (dashing == 1)
//        {
//            npc.velocity *= 0.9f;
//            for (int i = 0; i < 5; i++)
//            {
//                Dust.NewDust(npc.position, npc.width, npc.height, DustType, newColor: Color.Black);
//            }

//            if (++npc.ai[2] > 60)
//            {
//                npc.ai[2] = 0;
//                dashing = 2;
//                xPos = npc.position.X;
//                npc.netUpdate = true;
//            }
//        }

//        // Dashing.
//        if (dashing == 2)
//        {
//            npc.velocity.X = dashSpeed * -npc.ai[1];
//            npc.velocity.Y = 0;
//            ++npc.ai[2];

//            if (npc.ai[1] == 1)
//            {
//                if (npc.ai[2] > 120 && npc.Center.X < TCen.X)
//                {
//                    npc.ai[2] = 0;
//                    dashing = 3;
//                    npc.netUpdate = true;
//                }
//            }
//            else
//            {
//                if (npc.ai[2] > 120 && npc.Center.X > TCen.X)
//                {
//                    npc.ai[2] = 0;
//                    dashing = 3;
//                    npc.netUpdate = true;
//                }
//            }
//            if (npc.ai[2] % 20 == 0 && npc.ai[2] != 0 && Main.netMode != 1)
//            {
//                int proj = Projectile.NewProjectile(npc.Center, Vector2.Zero, mod.ProjectileType("LunaBubble"), 18, 3, 255, npc.ai[2]);
//                Main.projectile[proj].ai[0] = npc.ai[2];
//            }
//        }

//        // Changing dash direction before restarting Dash cycle.
//        if (dashing == 3)
//        {
//            npc.ai[1] *= -1;
//            npc.ai[2] = 0;
//            npc.ai[3]++;

//            dashing = 0;
//            if (npc.ai[3] >= numDashes)
//            {
//                npc.ai[3] = 0;
//                NewAttack();
//            }
//            npc.netUpdate = true;
//        }
//    }

//    private void MoveTo(Vector2 go)
//    {
//        npc.velocity = npc.GoTo(go, npc.Center, 45 * MPH);
//    }

//    protected override void Projectiles()
//    {
//        // Attack speed will reach double at 25% health. Then, it will stay.
//        if (CanDespawn)
//            return;
//        float attackSpeed = Math.Min(0.5f / (npc.life / (float)npc.lifeMax), 2f);
//        if (attackSpeed < 1)
//            attackSpeed = 1;

//        if (RingFiring && Timer % (int)(80 / attackSpeed) == 0)
//        {
//            npc.ai[3] = Main.rand.Next(4, 8);
//            Helper.FireInCircle(npc.Center, npc.ai[3] * 3, mod.ProjectileType<CrystalShard>(), npc.damage, 40 * MPH);

//            if (++npc.ai[2] > numRingFires)
//            {
//                npc.ai[2] = 0;
//                NewAttack();
//            }
//        }

//        if (Attack == (int)AI0.LargeChunks)
//        {
//            npc.ai[2]++;
//            npc.ai[3]++;
//            Vector2 pos = TCen + new Vector2(0, -300);

//            if (npc.ai[2] > 120)
//            {
//                NewProjectile(pos, new Vector2(0, 58 * MPH), mod.ProjectileType<CrystalChunk>(), npc.damage, 5);
//                npc.ai[2] = 0;
//            }
//            else if (npc.ai[2] > 48)
//            {
//                const int dimension = 32;
//                for (int i = 0; i < 5; i++)
//                {
//                    Dust.NewDust(pos - new Vector2(dimension / 2, dimension / 2),
//                        dimension, dimension, DustType);
//                }
//            }

//            if (npc.ai[3] >= timeLargeBolts)
//            {
//                NewAttack();
//            }
//        }
//        if (Attack == (int)AI0.LargeChunks && Timer % (int)(80f / attackSpeed) == 0)
//        {
//            FireAtTarget(npc.Center, TCen, mod.ProjectileType<CrystalShard>(),
//            npc.damage, 42 * MPH);
//        }

//        else if (Summoning && Timer % (int)(300f / attackSpeed) == 0)
//        {
//            NewNPC(npc.Center, mod.NPCType<MoonWasp>(), ai1: npc.whoAmI, target: npc.target);
//        }

//        else if (Attack == (int)AI0.Summon && Timer % (int)(70 / attackSpeed) == 0)
//        {
//            if (Main.netMode != 1)
//            {
//                Projectile proj = Main.projectile[
//                    FireAtTarget(npc.Center, TCen, mod.ProjectileType<CrystalShard>(),
//                    10, 43 * MPH)];

//                npc.ai[3] = Main.rand.Next(-4, 5);
//                proj.velocity = proj.velocity.RotatedBy(MathHelper.ToRadians(npc.ai[1]));
//            }
//        }
//    }

//    private int FireAtTarget(Vector2 shooter, Vector2 target, int type, int damage, float speed = 8f, float kb = 1, int owner = 255, float ai0 = 0, float ai1 = 0)
//    {
//        if (Main.netMode != 1)
//            return Projectile.NewProjectile(shooter, (target - shooter).SafeNormalize(-Vector2.UnitY) * speed, type, damage, kb, owner, ai0, ai1);
//        return 0;
//    }
