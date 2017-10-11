
using System;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    public class FBO : Buffer
    {
        public FBO(string bufferName, int bufferSize = 4, BufferTarget bufferTarget = BufferTarget.ShaderStorageBuffer) : base(bufferName, bufferSize, bufferTarget)
        {
            throw new NotImplementedException();
        }
    }
}
