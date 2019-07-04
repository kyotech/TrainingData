using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using TrainingData.StructuredAccess.Discord;

namespace TrainingData.Storage.Binary
{
    public class DiscordBinary
    {
        public class UlongChunk : IByteChunk
        {
            public ulong ID => BitConverter.ToUInt64(bytes, 0);
            public ulong index => BitConverter.ToUInt64(bytes, 8);

            public UlongChunk(params byte[] chunk) => this.chunk = chunk;
            public static implicit operator UlongChunk(byte[] chunk) => new UlongChunk(chunk);

            public byte this[int index] { get => bytes[index]; set => bytes[index] = value; }
            public byte[] this[int index, int length]
            {
                get => bytes.Skip(index).Take(length).ToArray();
                set
                {
                    if (value.Length != length) throw new IndexOutOfRangeException("Length of bytes to write != length of bytes to change!");
                    Array.Copy(value, 0, bytes, index, length);
                }
            }

            private byte[] chunk;

            public byte[] bytes
            {
                get => chunk;
                set
                {
                    if(value.Length != 16)
                        throw new InvalidDataException("UlongChunk Chunk invalid length. Must be 16 bytes");
                    chunk = value;
                }
            }
        }

        public FileStream binary;

        public DiscordBinary(FileStream stream) => binary = stream;
        public static implicit operator DiscordBinary(FileStream stream) => new DiscordBinary(stream);

        public UlongChunk[] ServerList()
        {
            binary.Seek(0, SeekOrigin.Begin);
            byte[] listLength = new byte[2]; binary.Read(listLength);
            UlongChunk[] servers = new UlongChunk[BitConverter.ToUInt16(listLength)];
            byte[] buffer = new byte[16];
            for (int i = 0; i < servers.Length; i++)
            {
                binary.Read(buffer);
                servers[i] = buffer;
            }

            return servers;
        }

        public UlongChunk[] ChannelList(ulong serverID)
        {
            UlongChunk serverIndex = ServerList().FirstOrDefault(x => x.ID == serverID);
            if(serverIndex == null) return new UlongChunk[0];

            binary.Seek((long)serverIndex.index, SeekOrigin.Begin);
            byte[] listLength = new byte[2]; binary.Read(listLength);
            UlongChunk[] channels = new UlongChunk[BitConverter.ToUInt16(listLength)];
            byte[] buffer = new byte[16];
            for (int i = 0; i < channels.Length; i++)
            {
                binary.Read(buffer);
                channels[i] = buffer;
            }

            return channels;
        }

        public UlongChunk[] MessageList(ulong serverID, ulong channelID)
        {
            UlongChunk channelIndex = ChannelList(serverID).FirstOrDefault(x => x.ID == channelID);
            if(channelIndex == null) return new UlongChunk[0];

            binary.Seek((long) channelIndex.index, SeekOrigin.Begin);
            byte[] listLength = new byte[2]; binary.Read(listLength);
            UlongChunk[] messages = new UlongChunk[BitConverter.ToUInt16(listLength)];
            byte[] buffer = new byte[16];
            for (int i = 0; i < messages.Length; i++)
            {
                binary.Read(buffer);
                messages[i] = buffer;
            }

            return messages;
        }

        public IEnumerable<ServerMessage> ServerMessages(ulong serverID)
        {
            UlongChunk[] Channels = ChannelList(serverID);
            if (Channels.Length == 0) yield break;

            foreach (UlongChunk channel in Channels)
                foreach (ChannelMessage message in ChannelMessages(serverID, channel.ID))
                    yield return new ServerMessage(message, channel.ID);
        }

        public IEnumerable<ChannelMessage> ChannelMessages(ulong serverID, ulong channelID)
        {
            UlongChunk[] Messages = MessageList(serverID, channelID);
            if (Messages.Length == 0) yield break;

            binary.Seek((long) Messages[0].index, SeekOrigin.Begin);
            byte[] length = new byte[2];
            byte[] buffer;
            for (int i = 0; i < Messages.Length; i++)
            {
                binary.Read(length);
                buffer = new byte[BitConverter.ToUInt16(length)];
                binary.Read(buffer);
                yield return buffer;
            }
        }
    }
}
