using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    public class TypedBuffer<T> : Buffer where T : struct
    {

        private int _elementCount;

        public int ElementCount
        {
            get => _elementCount;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Negative element count not allowed!");
                _elementCount = value;
                Resize(_elementCount * Marshal.SizeOf(typeof(T)));
            }
        }

        public T[] Data
        {
            get
            {
                var result = new T[_elementCount];
                GL.GetNamedBufferSubData(ID, IntPtr.Zero, Size, result);
                return result;
            }
            set => GL.NamedBufferSubData(ID, IntPtr.Zero, Size, value);
        }

        public TypedBuffer(BufferTarget bufferTarget) : base(bufferTarget, Marshal.SizeOf(typeof(T)))
        {
            ElementCount = 1;
        }

        public TypedBuffer(BufferTarget bufferTarget, int elementCount) : base(bufferTarget, elementCount * Marshal.SizeOf(typeof(T)))
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Negative element count not allowed!");
            _elementCount = elementCount;
        }

        public virtual void Clear(T clearVal)
        {
            GL.ClearNamedBufferData(ID, PixelInternalFormat.R32f, PixelFormat.Red, All.Float, ref clearVal);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }
    }
}
