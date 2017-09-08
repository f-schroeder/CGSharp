using OpenTK.Graphics.OpenGL;

namespace CGSharp.Shaders
{
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
