using Erilipah.Items.Taranys;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahPlayer : ModPlayer
    {
        public bool ZoneErilipah { get; private set; } = false;
        public bool ZoneLostCity { get; private set; } = false;

        public int extraItemReach = 0;
        public bool canMove = true;
        public bool canJump = true;
        public bool healingSoulTorch = false;

        public override void ResetEffects()
        {
            extraItemReach = 0;
            canMove = true;
            canJump = true;
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            var i = player.FindEquip(mod.ItemType<TorchOfSoul>());
            if (i != null && i.modItem is TorchOfSoul equip && !healingSoulTorch)
            {
                int amount = Math.Min(1, target.lifeMax) / 500 * damage;
                equip.stored += amount;

                Rectangle loc = player.getRect();
                loc.Y -= 30;
                CombatText.NewText(loc, new Color(247, 202, 166), amount);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!healingSoulTorch && player.statLife < player.statLifeMax2 && player.FindEquip(mod.ItemType<TorchOfSoul>()) != null && triggersSet.QuickHeal && player.HasBuff(BuffID.PotionSickness))
            {
                healingSoulTorch = true;
            }
        }

        public override void SetControls()
        {
            if (!canMove)
            {
                player.controlDown = player.controlUp = player.controlLeft =
                    player.controlRight = player.controlJump = player.controlMount = false;
                player.mount.Dismount(player);
            }
            if (!canJump)
            {
                player.controlJump = false;
                player.mount.Dismount(player);
            }
        }

        public override void UpdateBiomes()
        {
            ZoneLostCity = ErilipahWorld.lostCityTiles > 35;
            ZoneErilipah = ErilipahWorld.erilipahTiles > 35 || ZoneLostCity;
        }
        public override bool CustomBiomesMatch(Player other)
        {
            var difModPlr = other.GetModPlayer<ErilipahPlayer>();
            return ZoneErilipah == difModPlr.ZoneErilipah && ZoneLostCity == difModPlr.ZoneLostCity;
        }
        public override void CopyCustomBiomesTo(Player other)
        {
            ErilipahPlayer difModPlr = other.GetModPlayer<ErilipahPlayer>();
            difModPlr.ZoneErilipah = ZoneErilipah;
            difModPlr.ZoneLostCity = ZoneLostCity;
        }
        public override void SendCustomBiomes(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = ZoneErilipah;
            flags[1] = ZoneLostCity;
            writer.Write(flags);
        }
        public override void ReceiveCustomBiomes(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            ZoneErilipah = flags[0];
            ZoneLostCity = flags[1];
        }
        public override void UpdateBiomeVisuals()
        {
            player.ManageSpecialBiomeVisuals("Erilipah:ErilipahBiome", ZoneErilipah, player.Center);
        }
        //public override Texture2D GetMapBackgroundImage()
        //{
        //    if (ZoneExample)
        //    {
        //        return mod.GetTexture("ExampleBiomeMapBackground");
        //    }
        //    return null;
        //}
    }
}
