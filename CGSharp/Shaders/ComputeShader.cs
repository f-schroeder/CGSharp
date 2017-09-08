using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace CGSharp.Shaders
{

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

    public class ComputeShader : Shader
    {
        private int _indirectBufferID;
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
        public ComputeGroupSize WorkGroupSize
        {
            get => _workGroupSize;
            set
            {
                _workGroupSize = value;
                UpdateIndirectBuffer();
            }
        }

        public ComputeShader(string shaderSource) : base(shaderSource)
        {
            CreateIndirectBuffer();
        }

        public ComputeShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
            CreateIndirectBuffer();
        }

        protected override ShaderType Type()
        {
            return ShaderType.ComputeShader;
        }

        protected void CreateIndirectBuffer()
        {
            GL.CreateBuffers(1, out _indirectBufferID);

            _workGroupSize = new ComputeGroupSize();

            GL.NamedBufferStorage(_indirectBufferID, 12, new[] { _workGroupSize }, BufferStorageFlags.DynamicStorageBit);

            Debug.WriteLine("ComputeShader: Created dispatch indirect buffer.", "INFO");

        }

        protected void UpdateIndirectBuffer()
        {
            GL.NamedBufferSubData(_indirectBufferID, IntPtr.Zero, 12, new[] { _workGroupSize });
            Debug.WriteLine("ComputeShader: Work group size is now " + _workGroupSize, "INFO");
        }

        public void Dispatch()
        {
            GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, _indirectBufferID);
            GL.DispatchComputeIndirect(IntPtr.Zero);
        }

        public override string ToString()
        {
            return base.ToString() + "\nComputeShader: Work group size " + _workGroupSize;
        }

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
