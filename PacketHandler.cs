using System.IO;
using Terraria.ModLoader;

namespace Erilipah
{
    public abstract class PacketHandler
    {
        private readonly int ID = 0;
        public PacketHandler()
        {
            Erilipah.AddPacketHandler(this, out ID);
        }

        /// <summary>
        /// Send a packet out with specified info.
        /// </summary>
        /// <param name="info">Information.</param>
        public void SendPacket(params object[] info)
        {
            ModPacket packet = ModContent.GetInstance<Erilipah>().GetPacket();
            packet.Write(ID);

            WritePacket(packet, info);

            packet.Send();
        }

        /// <summary>
        /// Used to write (and <b>only</b> write) to the packet, using provided info.
        /// </summary>
        /// <param name="info">Information.</param>
        protected abstract void WritePacket(ModPacket packet, params object[] info);

        /// <summary>
        /// Used to handle packets.
        /// </summary>
        /// <param name="whoAmI">The whoAmI of the client who sent this ModPacket.</param>
        public abstract void HandlePacket(BinaryReader reader, int whoAmI);
    }
}