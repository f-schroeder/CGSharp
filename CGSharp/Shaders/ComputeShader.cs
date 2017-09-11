using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>Struct for defining the work group size of a <see cref="ComputeShader"/>.</summary>
    [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
    public struct ComputeGroupSize
    {
        public int GroupsX;
        public int GroupsY;
        public int GroupsZ;

        public ComputeGroupSize(int sizeX = 1, int sizeY = 1, int sizeZ = 1)
        {
            GroupsX = sizeX;
            GroupsY = sizeY;
            GroupsZ = sizeZ;
        }
    }

    /// <summary>
    /// Class for compute shaders.
    /// Additionally to the basic shader functionality, it handles a buffer for dispatching the shader indirectly.
    /// </summary>
    public class ComputeShader : Shader
    {
        private int _indirectBufferID;

        /// <summary>
        /// The OpenGL handle of the dispatch indirect command buffer.
        /// By setting this property, the dispatch indirect command buffer gets updated automatically.
        /// </summary>
        public int IndirectBufferID
        {
            get => _indirectBufferID;
            set
            {
                _indirectBufferID = value;
                UpdateIndirectBuffer();
            }
        }

        private ComputeGroupSize _workGroupSize;

        /// <summary>
        /// The struct containing information about the work group size of this compute shader.
        /// By setting this property, the dispatch indirect command buffer gets updated automatically.
        /// </summary>
        public ComputeGroupSize WorkGroupSize
        {
            get => _workGroupSize;
            set
            {
                _workGroupSize = value;
                UpdateIndirectBuffer();
            }
        }

        /// <summary>See <see cref="Shader"/> for more information.</summary>
        public ComputeShader(string shaderSource) : base(shaderSource)
        {
            CreateIndirectBuffer();
        }

        /// <summary>See <see cref="Shader"/> for more information.</summary>
        public ComputeShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
            CreateIndirectBuffer();
        }

        /// <summary>See <see cref="Shader"/> for more information.</summary>
        protected override ShaderType Type() => ShaderType.ComputeShader;

        /// <summary>Creates the buffer for the dispatch indirect information.</summary>
        protected void CreateIndirectBuffer()
        {
            GL.CreateBuffers(1, out _indirectBufferID);

            _workGroupSize = new ComputeGroupSize();

            GL.NamedBufferStorage(_indirectBufferID, 12, new[] { _workGroupSize }, BufferStorageFlags.DynamicStorageBit);

            Debug.WriteLine("ComputeShader: Created dispatch indirect buffer.", "INFO");

        }

        /// <summary>Updates the dispatch indirect buffer.</summary>
        protected void UpdateIndirectBuffer()
        {
            GL.NamedBufferSubData(_indirectBufferID, IntPtr.Zero, 12, new[] { _workGroupSize });
            Debug.WriteLine("ComputeShader: Work group size is now " + _workGroupSize, "INFO");
        }

        /// <summary>
        /// Dispatches the compute shader.
        /// It is recommended to use the indirect dispatch functionality (<paramref name="indirect"/> = true).
        /// If you need to dispatch the compute shader directly, set <paramref name="indirect"/> to false.
        /// </summary>
        /// <param name="indirect">Specifies the way of dispatching the compute shader.</param>
        public void Dispatch(bool indirect = true)
        {
            if (indirect)
            {
                GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, _indirectBufferID);
                GL.DispatchComputeIndirect(IntPtr.Zero);
            }
            else
            {
                GL.DispatchCompute(_workGroupSize.GroupsX, _workGroupSize.GroupsY, _workGroupSize.GroupsZ);
            }
        }

        public override string ToString()
        {
            return base.ToString() + "\nComputeShader: Work group size " + _workGroupSize;
        }

        /// <summary>Deletes the buffer and calls base function.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteBuffers(1, ref _indirectBufferID);
            }
            base.Dispose(disposing);
        }
    }
}
