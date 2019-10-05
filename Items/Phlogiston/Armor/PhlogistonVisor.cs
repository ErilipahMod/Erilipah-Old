using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class PhlogistonVisor : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 14, 6 };
        protected override int Rarity => 3;
        protected override string Tooltip => "Increased vision\n" +
            "17% increased throwing and ranged critical strike chance";

        protected override int Defense => 9;
        public override void UpdateEquip(Player player)
        {
            player.nightVision = true;
            player.rangedCrit += 17;
            player.thrownCrit += 17;
        }
        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
        {
            drawHair = false;
            drawAltHair = true;
        }
        public override void UpdateArmorSet(Player player)
        {
            const int damageRadius = 250;
            player.setBonus =
                "Immune to On Fire!\n" +
                "You gain a short timespan of immunity to lava\n" +
                "Nearby enemies automatically inflicted with On Fire!";

            player.lavaMax += 90;
            player.buffImmune[Terraria.ID.BuffID.OnFire] = true;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.life > 5 && !npc.townNPC && !npc.dontTakeDamage && !npc.buffImmune[BuffID.OnFire] &&
                    npc.Distance(player.Center) < damageRadius)
                {
                    npc.AddBuff(Terraria.ID.BuffID.OnFire, 2);
                    npc.lifeRegen -= 6;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2CircularEdge(damageRadius, damageRadius), mod.DustType<NPCs.Drone.DeepFlames>());
                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.fadeIn = 0.2f;
                dust.noLight = true;
                Main.playerDrawDust.Add(dust.dustIndex);
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) =>
            body.type == mod.ItemType<PhlogistonBracing>() && legs.type == mod.ItemType<PhlogistonGreaves>();

        protected override int CraftingTile => TileID.Anvils;
        protected override int[,] CraftingIngredients => new int[,] {
            { mod.ItemType<StablePhlogiston>(), 9 },
            { Terraria.ID.ItemID.HellstoneBar, 4 }
        };
    }
}
