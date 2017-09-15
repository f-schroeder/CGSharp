using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Buffers
{
    public class Buffer : IDisposable
    {
        protected static readonly Dictionary<BufferTarget, int> BoundBuffers = new Dictionary<BufferTarget, int>();
        private int _bufferID;
        private int _size;

        /// <summary>The Size of this buffer in bytes.</summary>
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

        public Buffer(BufferTarget bufferTarget, int bufferSize)
        {
            if(bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Negative buffer size not allowed!");

            Target = bufferTarget;
            _size = bufferSize;
            CreateBuffer();
        }

        public virtual void Clear(int clearVal = 0)
        {
            GL.ClearNamedBufferData(_bufferID, PixelInternalFormat.R32i, PixelFormat.RedInteger, All.Int, ref clearVal);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

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

        #region Bind

        /// <summary>
        /// Binds the specified buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
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
