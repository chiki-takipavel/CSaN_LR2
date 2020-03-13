using System;
using System.Collections.Generic;
using System.Text;

namespace LR2_CSaN
{
    class ICMPPacket
    {
        private byte code;
        private UInt16 checksum;
        private int messageSize;
        private byte[] message = new byte[1024];

        public byte Type { get; set; }

        public int PacketSize { get; set; }

        public ICMPPacket(byte type, byte[] data)
        {
            Type = type;
            code = 0;
            checksum = 0;
            Buffer.BlockCopy(data, 0, message, 4, data.Length);
            messageSize = data.Length + 4;
            PacketSize = messageSize + 4;
            checksum = getChecksum();
        }

        public ICMPPacket(byte[] data, int size)
        {
            Type = data[20];
            code = data[21];
            checksum = BitConverter.ToUInt16(data, 22);
            PacketSize = size - 20;
            messageSize = size - 24;
            Buffer.BlockCopy(data, 24, message, 0, messageSize);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[PacketSize + 1];
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, data, 2, 2);
            Buffer.BlockCopy(message, 0, data, 4, messageSize);
            return data;
        }

        public UInt16 getChecksum()
        {
            UInt32 chcksm = 0;
            byte[] data = getBytes();
            int index = 0;

            while (index < PacketSize)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            return (UInt16)(~chcksm);
        }
    }
}
