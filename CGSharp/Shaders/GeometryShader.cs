using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>Base class for geometry shaders. See <see cref="Shader"/> for more information.</summary>
    public class GeometryShader: Shader
    {
        public GeometryShader(string shaderSource) : base(shaderSource)
        {
        }

        public GeometryShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
        }

        protected override ShaderType Type() => ShaderType.GeometryShader;
    }
}
