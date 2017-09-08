using OpenTK.Graphics.OpenGL;

namespace CGSharp.Shaders
{
    public class FragmentShader : Shader
    {
        public FragmentShader(string shaderSource) : base(shaderSource)
        {
        }
        public FragmentShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
        }

        protected override ShaderType Type() => ShaderType.FragmentShader;
    }
}
