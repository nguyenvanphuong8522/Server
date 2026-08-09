using Shared.Network;
using System;

namespace Shared.Network
{


    public static class PacketUtility
    {
        public static byte[] BuildPacket(MyMessageType type, byte[] payload)
        {
            byte[] packet = new byte[13 + payload.Length];

            Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, packet, 0, 4);

            // bytes 4..11 để dành (timestamp/sequence/checksum...)
            packet[12] = (byte)type;

            Buffer.BlockCopy(payload, 0, packet, 13, payload.Length);

            return packet;
        }
    }
}