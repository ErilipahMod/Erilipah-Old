using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Taranys
{
    public class TorchOfSoul : ModItem
    {
        // TEST
        public int stored = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul Bank");
            Tooltip.SetDefault("Stores damage dealt to enemies for consumption\nPressing quick heal with Potion Sickness will use the item");
        }

        public override void SetDefaults()
        {
            item.accessory = true;
            item.maxStack = 1;

            item.width = 32;
            item.height = 44;

            item.value = 25000;
            item.rare = ItemRarityID.LightRed;
            stored = 0;
        }

        public override void UpdateEquip(Player player)
        {
            if (player.GetModPlayer<ErilipahPlayer>().healingSoulTorch)
            {
                if (stored <= 0 || player.statLife >= player.statLifeMax2)
                {
                    player.GetModPlayer<ErilipahPlayer>().healingSoulTorch = false;
                    return;
                }

                if (player.lifeRegenCount % 5 == 0)
                {
                    player.statLife++;
                    stored--;
                }
            }
        }

        public override void NetSend(BinaryWriter writer) => writer.Write(stored);
        public override void NetRecieve(BinaryReader reader) => stored = reader.ReadInt32();

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(mod, "Stored Dmg", "Use to heal " + stored + " life")
            {
                overrideColor = CombatText.HealLife
            });
        }
    }
}
