using System;

namespace TrainingData
{
    public interface IByteChunk
    {
        byte[] bytes { get; set; }
        byte this[int index] { get; set; }
        byte[] this[int index, int length] { get; set; }
    }
}
