using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    /// <summary>
    /// Typed Buffers use a generic field Data to store and handle the data of the buffer.
    /// The data is stored as array and can be modified.
    /// This class also keeps track of the number of elements inside the buffer.
    /// As only structs are accepted, consider using the helper structs <see cref="CGSharp.Buffers.Int"/> etc. if the buffer should only contain single numbers.
    /// </summary>
    /// <typeparam name="T">The type of the buffer (must be a struct).</typeparam>
    public class TypedBuffer<T> : Buffer where T : struct
    {

        private int _elementCount;

        /// <summary>
        /// Number of elements in the data array of this buffer.
        /// By setting this property, the buffer gets resized and the data inside it may get broken or even wiped.
        /// </summary>
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

        /// <summary>
        /// The data array contained in this buffer.
        /// </summary>
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

        /// <summary>
        /// Constructor for a typed buffer with one element.
        /// </summary>
        /// <param name="bufferTarget">The target to which the buffer gets bound.</param>
        public TypedBuffer(BufferTarget bufferTarget) : base(bufferTarget, Marshal.SizeOf(typeof(T)))
        {
            ElementCount = 1;
        }

        /// <summary>
        /// Constructor for a typed buffer with a given number of elements.
        /// </summary>
        /// <param name="bufferTarget">The target to which the buffer gets bound.</param>
        /// <param name="elementCount">The number of elements in the data array.</param>
        public TypedBuffer(BufferTarget bufferTarget, int elementCount) : base(bufferTarget, elementCount * Marshal.SizeOf(typeof(T)))
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Negative element count not allowed!");
            _elementCount = elementCount;
        }

        /// <summary>
        /// Clears the entire buffer.
        /// Warning: Not tested yet!
        /// </summary>
        /// <param name="clearVal">The value that gets written into the whole buffer.</param>
        public virtual void Clear(T clearVal)
        {
            GL.ClearNamedBufferData(ID, PixelInternalFormat.R32f, PixelFormat.Red, All.Float, ref clearVal);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }
    }
}
