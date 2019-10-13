//using Microsoft.Xna.Framework;
//using System;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace Erilipah.NPCs.Earthen
//{
//    public class EarthenSlime : NewModNPC
//    {
//        protected override int NPCFrameCount => 2;
//        protected override int MaxLife => 70;
//        protected override int Defense => 9;
//        protected override int Damage => 12;
//        protected override float KnockbackResist => 0.1f;
//        protected override bool NoGravity => false;

//        public override void SetDefaults()
//        {
//            base.SetDefaults();
//            npc.aiStyle = 1;
//            aiType = 1;
//            animationType = NPCID.BlueSlime;
//            npc.value = 350;
//        }

//        private int firing;
//        public override void AI()
//        {
//            if (npc.velocity.Y > 0.5 && firing == 0)
//            {
//                firing = 1;
//            }
//            if (npc.velocity.Y == 0 && firing == 1 && Main.netMode != 1)
//            {
//                firing = 2;
//                FireInCircle();
//            }
//            else if (npc.velocity.Y < 0 && firing == 2)
//            {
//                firing = 0;
//            }
//        }

//        private void FireInCircle()
//        {
//            const float rotation = 180;
//            const float numProj1 = 10;
//            const float Speed1 = 2.35f;

//            for (int i = 0; i < numProj1; i++)
//            {
//                Vector2 perturbedSpeed = new Vector2(Speed1).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / Math.Max(numProj1 - 1, 1)));
//                Projectile.NewProjectileDirect(npc.Center, perturbedSpeed, mod.ProjectileType<EarthenShard>(), Damage / 2, 1);
//            }
//        }

//        public override float SpawnChance(NPCSpawnInfo spawnInfo)
//        {
//            if (SpawnCondition.OverworldNight.Active || SpawnCondition.Underground.Active)
//                return 0.02f;
//            if (SpawnCondition.OverworldDay.Active)
//                return 0.008f;
//            return 0;
//        }

//        public override void OnHitPlayer(Player target, int damage, bool crit)
//        {
//            target.AddBuff(BuffID.Poisoned, 240);
//        }

//        public override void NPCLoot()
//        {
//            base.NPCLoot();
//            Loot.DropItem(npc, mod.ItemType("EarthenClump"), 4, 6);
//        }
//    }
//    public class EarthenShard : NewModProjectile
//    {
//        protected override DamageTypes DamageType => DamageTypes.Hostile;
//        protected override int[] Dimensions => new int[] { 10, 26 };
//        protected override float Gravity => 0.085f;
//        protected override float? Rotation => projectile.velocity.ToRotation() + MathHelper.PiOver2;
//        protected override int DustType => -1;

//        public override void OnHitPlayer(Player target, int damage, bool crit)
//        {
//            target.AddBuff(BuffID.Poisoned, 120);
//        }
//    }
//}
