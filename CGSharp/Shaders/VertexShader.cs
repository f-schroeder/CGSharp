using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    public class VertexShader : Shader
    {
        public VertexShader(string shaderSource) : base(shaderSource)
        {
        }

        public VertexShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
        }

        protected override ShaderType Type() => ShaderType.VertexShader;
    }
}
