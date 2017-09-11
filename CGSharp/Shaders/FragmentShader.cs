using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>Base class for fragment shaders. See <see cref="Shader"/> for more information.</summary>
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
