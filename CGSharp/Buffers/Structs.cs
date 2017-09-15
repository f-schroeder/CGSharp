using System.Runtime.InteropServices;

namespace CGSharp.Buffers
{
    [StructLayout(LayoutKind.Sequential, Size = 4, Pack = 1)]
    public struct Int
    {
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential, Size = 4, Pack = 1)]
    public struct UInt
    {
        public uint Value;
    }

    [StructLayout(LayoutKind.Sequential, Size = 4, Pack = 1)]
    public struct Float
    {
        public float Value;
    }

    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 1)]
    public struct Double
    {
        public double Value;
    }

    /// <summary>Struct for defining the work group size of a <see cref="Shaders.ComputeShader"/>.</summary>
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
}
