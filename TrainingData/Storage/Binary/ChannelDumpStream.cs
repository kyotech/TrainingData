using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrainingData.StructuredAccess.Discord;

namespace TrainingData.Storage.Binary
{
    public class ChannelDumpStream
    {
        public readonly FileStream stream;
        
        public ChannelDumpStream(FileStream stream) => this.stream = stream;
        public static implicit operator ChannelDumpStream(FileStream fileStream) => new ChannelDumpStream(fileStream);

        public void Initialize()
        {
            stream.Flush();
            stream.SetLength(0);
        }

        public void MoveToEnd() => stream.Seek(0, SeekOrigin.End);
        public void MoveToStart() => stream.Seek(0, SeekOrigin.Begin);
        public void MoveToMessages() => stream.Seek(16, SeekOrigin.Begin);

        public void WriteChannel(ulong serverID, ulong channelID)
        {
            stream.Write(BitConverter.GetBytes(serverID));
            stream.Write(BitConverter.GetBytes(channelID));
        }

        public (ulong serverID, ulong channelID) GetChannel()
        {
            if (stream.Length < 16) throw new EndOfStreamException("File too small for channel info!?");
            MoveToStart(); byte[] channelBlock = new byte[16]; stream.Read(channelBlock);
            return (BitConverter.ToUInt64(channelBlock, 0), BitConverter.ToUInt64(channelBlock, 8));
        }

        public void DumpMessage(ChannelMessage message)
        {
            stream.Write(BitConverter.GetBytes(message.bytes.Length));
            stream.Write(message.bytes);
        }

        public void DumpMessages(params ChannelMessage[] messages)
        {
            foreach (ChannelMessage message in messages)
            {
                stream.Write(BitConverter.GetBytes(message.bytes.Length));
                stream.Write(message.bytes);
            }
        }

        public ChannelMessage ReadMessage()
        {
            byte[] channelLength = new byte[4];
            stream.Read(channelLength);
            byte[] channelBlock = new byte[BitConverter.ToInt32(channelLength)];
            stream.Read(channelBlock);
            return new ChannelMessage(channelBlock);
        }

        public IEnumerator<ChannelMessage> ReadMessages()
        {
            if(stream.Length < 20)
                throw new EndOfStreamException("No Messages in binary dump!");

            MoveToMessages();
            byte[] channelLength, channelBlock;
            channelLength = new byte[4];
            stream.Read(channelLength);
            for (int length = BitConverter.ToInt32(channelLength); length != 0; length = BitConverter.ToInt32(channelLength))
            {
                channelBlock = new byte[length];
                stream.Read(channelBlock);
                yield return channelBlock;
                stream.Read(channelLength);
            }
        }

        public void SetChannel(ulong serverID, ulong channelID)
        {
            long position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(serverID));
            stream.Write(BitConverter.GetBytes(channelID));
            stream.Seek(position, SeekOrigin.Begin);
        }

        public void AppendMessage(ChannelMessage message)
        {
            long position = stream.Position;
            stream.Seek(0, SeekOrigin.End);
            stream.Write(message.bytes);
            stream.Seek(position, SeekOrigin.Begin);
        }
    }
}
