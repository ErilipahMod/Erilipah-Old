using System.Collections.Generic;
using System.IO;
using Terraria;
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

        public override void ResetEffects()
        {
            extraItemReach = 0;
            canMove = true;
            canJump = true;
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
