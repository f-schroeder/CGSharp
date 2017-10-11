using System;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    public class VAO : Buffer
    {
        public VAO(string bufferName, int bufferSize = 4, BufferTarget bufferTarget = BufferTarget.ShaderStorageBuffer) : base(bufferName, bufferSize, bufferTarget)
        {
            throw new NotImplementedException();
        }
    }
}
