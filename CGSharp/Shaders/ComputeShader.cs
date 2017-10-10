using System;
using System.Diagnostics;
using CGSharp.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>
    /// Class for compute shaders.
    /// Additionally to the basic shader functionality, it handles a buffer for dispatching the shader indirectly.
    /// </summary>
    public class ComputeShader : Shader
    {
        private TypedBuffer<ComputeGroupSize> _indirectBuffer;

        /// <summary>
        /// The OpenGL handle of the dispatch indirect command buffer.
        /// By setting this property, the dispatch indirect command buffer gets updated automatically.
        /// </summary>
        public TypedBuffer<ComputeGroupSize> IndirectBuffer
        {
            get => _indirectBuffer;
            set
            {
                _indirectBuffer = value;
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
            _indirectBuffer = new TypedBuffer<ComputeGroupSize>(new[] { _workGroupSize }, BufferTarget.DispatchIndirectBuffer);

            Debug.WriteLine("ComputeShader: Created dispatch indirect buffer.", "INFO");
        }

        /// <summary>Updates the dispatch indirect buffer.</summary>
        protected void UpdateIndirectBuffer()
        {
            _indirectBuffer.Data = new[] {_workGroupSize};

            Debug.WriteLine("ComputeShader: Work group size is now " + _workGroupSize, "INFO");
        }

        /// <summary>
        /// Dispatches the compute shader using the work group size stored in the WorkGroupSize property.
        /// It is recommended to use the indirect dispatch functionality (<paramref name="indirect"/> = true).
        /// If you need to dispatch the compute shader directly, set <paramref name="indirect"/> to false.
        /// </summary>
        /// <param name="indirect">Specifies the way of dispatching the compute shader.</param>
        public void Dispatch(bool indirect = true)
        {
            if (indirect)
            {
                _indirectBuffer.Bind();
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
                _indirectBuffer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
