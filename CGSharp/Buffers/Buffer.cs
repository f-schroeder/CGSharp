using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    /// <summary>
    /// Base class for OpenGL buffers.
    /// Creates and handles OpenGL GPU buffer objects and keeps track of already bound buffers.
    /// Contains information about the size and the binding target of the buffer.
    /// As this buffer is not typed, you should consider using <see cref="TypedBuffer{T}"/> instead. 
    /// </summary>
    public class Buffer : IDisposable
    {
        protected static readonly Dictionary<BufferTarget, int> BoundBuffers = new Dictionary<BufferTarget, int>();
        private int _bufferID;
        private int _size;

        /// <summary>
        /// The Size of this buffer in bytes. 
        /// By setting this property, the buffer gets resized and the data inside it may get broken or even wiped.
        /// </summary>
        public int Size
        {
            get => _size;
            set => Resize(value);
        }

        /// <summary>The OpenGL handle for this buffer.</summary>
        public int ID
        {
            get => _bufferID;
            protected set => _bufferID = value;
        }

        /// <summary>The OpenGL binding target for this buffer.</summary>
        public BufferTarget Target { get; set; }

        /// <summary>
        /// Constructor for creating a Buffer for a given buffer target with a given size.
        /// </summary>
        /// <param name="bufferTarget">The target to which the buffer gets bound.</param>
        /// <param name="bufferSize">The size of the GPU storage that will be allocated for this buffer in bytes.</param>
        public Buffer(BufferTarget bufferTarget, int bufferSize)
        {
            if(bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Negative buffer size not allowed!");

            Target = bufferTarget;
            _size = bufferSize;
            CreateBuffer();
        }

        /// <summary>
        /// Clears the entire buffer.
        /// </summary>
        /// <param name="clearVal">Optional: The value that gets written into the whole buffer.</param>
        public virtual void Clear(int clearVal = 0)
        {
            GL.ClearNamedBufferData(_bufferID, PixelInternalFormat.R32i, PixelFormat.RedInteger, All.Int, ref clearVal);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        /// <summary>
        /// Clears the entire buffer.
        /// </summary>
        /// <param name="clearVal">Optional: The value that gets written into the whole buffer.</param>
        public virtual void Clear(float clearVal = 0.0f)
        {
            GL.ClearNamedBufferData(_bufferID, PixelInternalFormat.R32f, PixelFormat.Red, All.Float, ref clearVal);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        protected void CreateBuffer()
        {
            GL.CreateBuffers(1, out _bufferID);
            GL.NamedBufferStorage(_bufferID, _size, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        }

        protected void Resize(int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newSize), "Negative buffer size not allowed!");

            GL.DeleteBuffer(_bufferID);
            _size = newSize;
            CreateBuffer();
        }

        #region BindBase

        /// <summary>
        /// Binds the buffer object to a binding point in the the array of targets for this buffer.
        /// Note that this is only possible for the following buffer targets: Atomic counter, transform feedback, uniform and shader storage.
        /// </summary>
        /// <param name="index">The binding point.</param>
        public void BindBase(int index) => GL.BindBufferBase((BufferRangeTarget)Target, index, ID);

        /// <summary>
        /// Binds the buffer object to a binding point in the the array of specified targets.
        /// Note that this is only possible for the following buffer targets: Atomic counter, transform feedback, uniform and shader storage.
        /// </summary>
        /// <param name="index">The binding point.</param>
        /// <param name="bindTarget">The binding target.</param>
        public void BindBase(int index, BufferRangeTarget bindTarget) => GL.BindBufferBase(bindTarget, index, ID);

        #endregion

        #region Bind

        /// <summary>
        /// Binds the specified buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="target">The buffer target to which the buffer gets bound.</param>
        /// <param name="bufferID">The OpenGL handle for the buffer that gets bound.</param>
        public static void Bind(BufferTarget target, int bufferID)
        {
            if (BoundBuffers[target] != bufferID)
            {
                BoundBuffers[target] = bufferID;
                GL.BindBuffer(target, bufferID);
            }
        }

        /// <summary>
        /// Binds the specified buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="target">The buffer target to which the buffer gets bound.</param>
        /// <param name="buffer">The buffer that gets bound.</param>
        public static void Bind(BufferTarget target, Buffer buffer)
        {
            if (BoundBuffers[target] != buffer.ID)
            {
                BoundBuffers[target] = buffer.ID;
                GL.BindBuffer(target, buffer.ID);
            }
        }

        /// <summary>
        /// Binds this buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="target">The buffer target to which the buffer gets bound.</param>
        public void Bind(BufferTarget target)
        {
            if (BoundBuffers[target] != ID)
            {
                BoundBuffers[target] = ID;
                GL.BindBuffer(target, ID);
            }
        }

        /// <summary>
        /// Binds this buffer to this buffers target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        public void Bind()
        {
            if (BoundBuffers[Target] != ID)
            {
                BoundBuffers[Target] = ID;
                GL.BindBuffer(Target, ID);
            }
        }

        #endregion

        public override string ToString()
        {
            return "Buffer " + _bufferID + ": Size: " + _size + ", Target: " + Target;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteBuffer(_bufferID);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
