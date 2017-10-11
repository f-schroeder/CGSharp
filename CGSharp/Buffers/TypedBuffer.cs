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
        /// Constructor for a typed buffer with the given data array inside.
        /// </summary>
        /// <param name="bufferName">The name of the buffer. This name should be identical with the buffers name inside the shader.</param>
        /// <param name="bufferData">The data inside the buffer.</param>
        /// <param name="bufferTarget">The target to which the buffer gets bound. Default: ShaderStorageBuffer.</param>
        public TypedBuffer(string bufferName, T[] bufferData, BufferTarget bufferTarget = BufferTarget.ShaderStorageBuffer) : base(bufferName, bufferData.Length * Marshal.SizeOf(typeof(T)), bufferTarget)
        {
            Data = bufferData;
        }

        /// <summary>
        /// Constructor for a typed buffer with a given number of elements.
        /// </summary>
        /// <param name="bufferName">The name of the buffer. This name should be identical with the buffers name inside the shader.</param>
        /// <param name="elementCount">The number of elements in the data array. Default: 1.</param>
        /// <param name="bufferTarget">The target to which the buffer gets bound. Default: ShaderStorageBuffer.</param>
        public TypedBuffer(string bufferName, int elementCount = 1, BufferTarget bufferTarget = BufferTarget.ShaderStorageBuffer) : base(bufferName, elementCount * Marshal.SizeOf(typeof(T)), bufferTarget)
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Negative element count not allowed!");
            _elementCount = elementCount;
            Data = new T[elementCount];
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
