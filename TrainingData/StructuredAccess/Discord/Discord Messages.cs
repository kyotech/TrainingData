using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrainingData;

namespace TrainingData.StructuredAccess.Discord
{
    public class ChannelMessage : IByteChunk
    {
        public ulong messageID => BitConverter.ToUInt64(bytes, 0);
        public ulong userID => BitConverter.ToUInt64(bytes, 8);
        public ushort messageBytesLength => BitConverter.ToUInt16(bytes, 16);
        public string message => (messageBytesLength > 0) ? "" : BitConverter.ToString(bytes, 19, messageBytesLength);
        public byte attachmentCount => bytes[18];
        public string[] attachments => bytes.Skip(19 + messageBytesLength).ToArray().AsStringArray();

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
        public byte[] bytes { get; set; }

        public ChannelMessage(byte[] chunk) => bytes = chunk;

        public ChannelMessage(ulong messageID, ulong userID, string message, string[] attachments = default)
        {
            byte[] messageBytes = Encoding.Unicode.GetBytes(message);
            byte[][] attachmentsBytes = new byte[attachments.Length][];
            for(int i = 0; i < attachments.Length; i++)
                attachmentsBytes[i] = Encoding.Unicode.GetBytes(attachments[i]);

            int stringBytesLength = messageBytes.Length;
            foreach (byte[] bytes in attachmentsBytes)
                stringBytesLength += bytes.Length;

            bytes = new byte[stringBytesLength + 19 + (2 * attachments.Length)];

            Array.Copy(BitConverter.GetBytes(messageID), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(userID), 0, bytes, 8, 8);
            Array.Copy(BitConverter.GetBytes((ushort) messageBytes.Length), 0, bytes, 16, 2);
            bytes[18] = (byte) attachments.Length;
            Array.Copy(messageBytes, 0, bytes, 19, messageBytes.Length);

            int index = 19 + messageBytes.Length;
            for (int i = 0; i < attachmentsBytes.Length; i++)
            {
                bytes[index++] = (byte)attachmentsBytes[i].Length;
                Array.Copy(attachmentsBytes[i], 0, bytes, index, attachmentsBytes[i].Length);
                index += attachmentsBytes[i].Length;
            }
        }

        public static implicit operator ChannelMessage(byte[] chunk) => new ChannelMessage(chunk);
    }

    public class ServerMessage : IByteChunk
    {
        public ulong channelID => BitConverter.ToUInt64(bytes, 0);
        public ulong messageID => BitConverter.ToUInt64(bytes, 8);
        public ulong userID => BitConverter.ToUInt64(bytes, 16);
        public ushort messageBytesLength => BitConverter.ToUInt16(bytes, 24);
        public string message => (messageBytesLength > 0) ? "" : BitConverter.ToString(bytes, 27, messageBytesLength);
        public byte attachmentCount => bytes[26];
        public string[] attachments => bytes.Skip(27 + messageBytesLength).ToArray().AsStringArray();

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
        public byte[] bytes { get; set; }

        public ServerMessage(byte[] chunk) => bytes = chunk;

        public ServerMessage(ChannelMessage message, ulong channelID)
        {
            bytes = new byte[message.bytes.Length + 8];
            Array.Copy(BitConverter.GetBytes(channelID), bytes, 8);
            Array.Copy(message.bytes, 0, bytes, 8, message.bytes.Length);
        }

        public ServerMessage(ulong serverID, ulong channelID, ulong messageID, ulong userID, string message, string[] attachments)
        {
            byte[] messageBytes = Encoding.Unicode.GetBytes(message);
            byte[][] attachmentsBytes = new byte[attachments.Length][];
            for (int i = 0; i < attachments.Length; i++)
                attachmentsBytes[i] = Encoding.Unicode.GetBytes(attachments[i]);

            int stringBytesLength = messageBytes.Length;
            foreach (byte[] bytes in attachmentsBytes)
                stringBytesLength += bytes.Length;

            bytes = new byte[stringBytesLength + 27 + (2 * attachments.Length)];

            Array.Copy(BitConverter.GetBytes(serverID), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(channelID), 0, bytes, 8, 8);
            Array.Copy(BitConverter.GetBytes(messageID), 0, bytes, 16, 8);
            Array.Copy(BitConverter.GetBytes(userID), 0, bytes, 24, 8);
            Array.Copy(BitConverter.GetBytes((ushort)messageBytes.Length), 0, bytes, 32, 2);
            bytes[34] = (byte)attachments.Length;
            Array.Copy(messageBytes, 0, bytes, 35, messageBytes.Length);

            int index = 27 + messageBytes.Length;
            for (int i = 0; i < attachmentsBytes.Length; i++)
            {
                bytes[index++] = (byte)attachmentsBytes[i].Length;
                Array.Copy(attachmentsBytes[i], 0, bytes, index, attachmentsBytes[i].Length);
                index += attachmentsBytes[i].Length;
            }
        }
    }

    public class Message : IByteChunk
    {
        public ulong serverID => BitConverter.ToUInt64(bytes, 0);
        public ulong channelID => BitConverter.ToUInt64(bytes, 8);
        public ulong messageID => BitConverter.ToUInt64(bytes, 16);
        public ulong userID => BitConverter.ToUInt64(bytes, 24);
        public ushort messageBytesLength => BitConverter.ToUInt16(bytes, 32);
        public string message => (messageBytesLength > 0) ? "" : BitConverter.ToString(bytes, 35, messageBytesLength);
        public byte attachmentCount => bytes[34];
        public string[] attachments => bytes.Skip(35 + messageBytesLength).ToArray().AsStringArray();

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
        public byte[] bytes { get; set; }

        public Message(byte[] chunk) => bytes = chunk;

        public Message(ServerMessage message, ulong serverID)
        {
            bytes = new byte[message.bytes.Length + 8];
            Array.Copy(BitConverter.GetBytes(serverID), bytes, 8);
            Array.Copy(message.bytes, 0, bytes, 8, message.bytes.Length);
        }

        public Message(ChannelMessage message, ulong serverID, ulong channelID)
        {
            bytes = new byte[message.bytes.Length + 16];
            Array.Copy(BitConverter.GetBytes(serverID), bytes, 8);
            Array.Copy(BitConverter.GetBytes(channelID), 0, bytes, 8, 8);
            Array.Copy(message.bytes, 0, bytes, 16, message.bytes.Length);
        }

        public Message(ulong serverID, ulong channelID, ulong messageID, ulong userID, string message, string[] attachments)
        {
            byte[] messageBytes = Encoding.Unicode.GetBytes(message);
            byte[][] attachmentsBytes = new byte[attachments.Length][];
            for (int i = 0; i < attachments.Length; i++)
                attachmentsBytes[i] = Encoding.Unicode.GetBytes(attachments[i]);

            int stringBytesLength = messageBytes.Length;
            foreach (byte[] bytes in attachmentsBytes)
                stringBytesLength += bytes.Length;

            bytes = new byte[stringBytesLength + 35 + (2 * attachments.Length)];

            Array.Copy(BitConverter.GetBytes(serverID), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(channelID), 0, bytes, 8, 8);
            Array.Copy(BitConverter.GetBytes(messageID), 0, bytes, 16, 8);
            Array.Copy(BitConverter.GetBytes(userID), 0, bytes, 24, 8);
            Array.Copy(BitConverter.GetBytes((ushort)messageBytes.Length), 0, bytes, 32, 2);
            bytes[34] = (byte)attachments.Length;
            Array.Copy(messageBytes, 0, bytes, 35, messageBytes.Length);

            int index = 35 + messageBytes.Length;
            for (int i = 0; i < attachmentsBytes.Length; i++)
            {
                bytes[index++] = (byte)attachmentsBytes[i].Length;
                Array.Copy(attachmentsBytes[i], 0, bytes, index, attachmentsBytes[i].Length);
                index += attachmentsBytes[i].Length;
            }
        }
    }
}