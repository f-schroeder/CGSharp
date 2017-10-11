using System;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    public class AtomicCounterBuffer : Buffer
    {
        public AtomicCounterBuffer(string bufferName, int bufferSize = 4, BufferTarget bufferTarget = BufferTarget.AtomicCounterBuffer) : base(bufferName, bufferSize, bufferTarget)
        {
            throw new NotImplementedException();
        }
    }
}
