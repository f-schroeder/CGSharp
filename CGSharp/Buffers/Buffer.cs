using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using All = OpenTK.Graphics.OpenGL4.All;
using BufferRangeTarget = OpenTK.Graphics.OpenGL4.BufferRangeTarget;
using BufferStorageFlags = OpenTK.Graphics.OpenGL4.BufferStorageFlags;
using BufferTarget = OpenTK.Graphics.OpenGL4.BufferTarget;
using GL = OpenTK.Graphics.OpenGL4.GL;
using MemoryBarrierFlags = OpenTK.Graphics.OpenGL4.MemoryBarrierFlags;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.OpenGL4.PixelInternalFormat;

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
        protected static readonly Dictionary<BufferTarget, int> BoundBuffers = new Dictionary<BufferTarget, int>(); // Key: Buffer target, value: Buffer ID
        protected static readonly Dictionary<int, int> BaseBoundBuffers = new Dictionary<int, int>(); // Key: Binding point, value: Buffer ID
        private int _bufferID;
        private int _size;
        private ulong _gpuAddress;

        /// <summary>All created Buffers are registered in this Dictionary by their name strings.</summary>
        public static readonly Dictionary<string, Buffer> RegisteredBuffers = new Dictionary<string, Buffer>();

        /// <summary>
        /// The name of this buffer.
        /// This name should be identical with its name inside the shader.
        /// </summary>
        public string Name { get; set; }

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

        /// <summary>The address of this buffer on the GPU (for bindless buffers).</summary>
        public ulong GpuAddress => GetGpuAddress();

        /// <summary>
        /// Constructor for creating a Buffer for a given buffer target with a given size.
        /// </summary>
        /// <param name="bufferName">The name of the buffer. This name should be identical with the buffers name inside the shader.</param>
        /// <param name="bufferSize">The size of the GPU storage that will be allocated for this buffer in bytes. Default: 4.</param>
        /// <param name="bufferTarget">The target to which the buffer gets bound. Default: ShaderStorageBuffer.</param>
        public Buffer(string bufferName, int bufferSize = 4, BufferTarget bufferTarget = BufferTarget.ShaderStorageBuffer)
        {
            if(bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Negative buffer size not allowed!");

            Target = bufferTarget;
            _size = bufferSize;
            Name = bufferName;
            RegisteredBuffers[Name] = this;
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

            Delete();
            _size = newSize;
            CreateBuffer();
        }

        protected ulong GetGpuAddress()
        {
            //TODO: Test if all casts works properly!
            if (_gpuAddress == 0)
            {
                OpenTK.Graphics.OpenGL.GL.NV.GetNamedBufferParameter((uint)ID,
                    (BufferParameterName)NvShaderBufferLoad.BufferGpuAddressNv, out _gpuAddress);
                OpenTK.Graphics.OpenGL.GL.NV.MakeNamedBufferResident(ID, (NvShaderBufferLoad)BufferAccess.ReadWrite);
            }
            return _gpuAddress;
        }

        #region BindBase

        /// <summary>
        /// Binds the buffer object to a binding point in the the array of targets for this buffer.
        /// Note that this is only possible for the following buffer targets: Atomic counter, transform feedback, uniform and shader storage.
        /// </summary>
        /// <param name="index">The binding point.</param>
        public void BindBase(int index)
        {
            if (BaseBoundBuffers[index] != ID)
            {
                BaseBoundBuffers[index] = ID;
                GL.BindBufferBase((BufferRangeTarget) Target, index, ID);
            }
        }

        /// <summary>
        /// Binds the buffer object to a binding point in the the array of specified targets.
        /// Note that this is only possible for the following buffer targets: Atomic counter, transform feedback, uniform and shader storage.
        /// </summary>
        /// <param name="index">The binding point.</param>
        /// <param name="bindTarget">The binding target.</param>
        public void BindBase(int index, BufferRangeTarget bindTarget)
        {
            if (BaseBoundBuffers[index] != ID)
            {
                BaseBoundBuffers[index] = ID;
                GL.BindBufferBase(bindTarget, index, ID);
            }
        }

        #endregion

        #region Bind

        /// <summary>
        /// Binds the specified buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="bufferTarget">The buffer target to which the buffer gets bound.</param>
        /// <param name="bufferID">The OpenGL handle for the buffer that gets bound.</param>
        public static void Bind(BufferTarget bufferTarget, int bufferID)
        {
            if (BoundBuffers[bufferTarget] != bufferID)
            {
                BoundBuffers[bufferTarget] = bufferID;
                GL.BindBuffer(bufferTarget, bufferID);
            }
            BindWarning(bufferTarget);
        }

        /// <summary>
        /// Binds the specified buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="bufferTarget">The buffer target to which the buffer gets bound.</param>
        /// <param name="buffer">The buffer that gets bound.</param>
        public static void Bind(BufferTarget bufferTarget, Buffer buffer)
        {
            if (BoundBuffers[bufferTarget] != buffer.ID)
            {
                BoundBuffers[bufferTarget] = buffer.ID;
                GL.BindBuffer(bufferTarget, buffer.ID);
            }
            BindWarning(bufferTarget);
        }

        /// <summary>
        /// Binds this buffer to the specified target.
        /// Keeps track of already bound buffers to avoid re-binding the buffer.
        /// Warning: Do not use this function frequently as it requires state changes. Use BindBase() whenever possible.
        /// </summary>
        /// <param name="bufferTarget">The buffer target to which the buffer gets bound.</param>
        public void Bind(BufferTarget bufferTarget)
        {
            if (BoundBuffers[bufferTarget] != ID)
            {
                BoundBuffers[bufferTarget] = ID;
                GL.BindBuffer(bufferTarget, ID);
            }
            BindWarning(bufferTarget);
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
            BindWarning(Target);
        }

        [Conditional("DEBUG")]
        private static void BindWarning(BufferTarget bufferTarget)
        {
            if (bufferTarget == BufferTarget.ShaderStorageBuffer || bufferTarget == BufferTarget.AtomicCounterBuffer ||
                    bufferTarget == BufferTarget.UniformBuffer || bufferTarget == BufferTarget.TransformFeedbackBuffer)
                {
                    Debug.WriteLine("Used Buffer.Bind() with target " + bufferTarget + ". Use Buffer.BindBase() instead for this target!", "WARNING");
                }
        }

        #endregion

        public override string ToString()
        {
            return "Buffer " + Name + ": ID: " + _bufferID + ", Size: " + _size + ", Target: " + Target;
        }

        protected void Delete()
        {
            if (_gpuAddress != 0)
            {
                OpenTK.Graphics.OpenGL.GL.NV.MakeNamedBufferNonResident(ID);
                _gpuAddress = 0;
            }

            GL.DeleteBuffer(_bufferID);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RegisteredBuffers.Remove(Name);
                Delete();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
