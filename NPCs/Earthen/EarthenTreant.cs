
//namespace Erilipah.NPCs.Earthen
//{
//    public class EarthenTreant : NewModNPC
//    {
//        protected override int NPCFrameCount => 7;
//        protected override int MaxLife => 100;
//        protected override int Damage => 8;
//        protected override int Defense => 8;
//        protected override float KnockbackResist => 0.10f;
//        protected override bool NoGravity => false;

//        public override void SetDefaults()
//        {
//            base.SetDefaults();
//            npc.aiStyle = 3;
//            npc.value = 400;
//        }

//        private int firing = 0;
//        public override void AI()
//        {
//            base.AI();
//            if (npc.velocity.X > 0)
//                npc.direction = npc.spriteDirection = 1;
//            else
//                npc.direction = npc.spriteDirection = -1;
//            if (++firing % 180 == 0 && Main.netMode != -1)
//            {
//                Vector2 velocity = npc.Center.To(TCen - new Vector2(0, 40), 5);
//                int type = ProjectileType<EarthenShard>();
//                Projectile.NewProjectile(npc.Center, velocity, type, 20, 1);
//            }
//        }

//        public override void FindFrame(int frameHeight)
//        {
//            if (Math.Abs(npc.velocity.Y) < 1)
//                npc.Animate(frameHeight, 4, 7, 0);
//        }

//        public override float SpawnChance(NPCSpawnInfo spawnInfo)
//        {
//            if (SpawnCondition.OverworldNight.Active || SpawnCondition.Underground.Active)
//                return 0.02f;
//            if (SpawnCondition.OverworldDay.Active)
//                return 0.008f;
//            return 0;
//        }

//        public override void NPCLoot()
//        {
//            base.NPCLoot();
//            Loot.DropItem(npc, mod.ItemType("EarthenClump"), 4, 6);
//        }

//        public override void OnHitPlayer(Player target, int damage, bool crit)
//        {
//            target.AddBuff(BuffID.Poisoned, 240);
//        }
//    }
//}
