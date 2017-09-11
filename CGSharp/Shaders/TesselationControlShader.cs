using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>Base class for tesselation control shaders. See <see cref="Shader"/> for more information.</summary>
    public class TesselationControlShader : Shader
    {
        public TesselationControlShader(string shaderSource) : base(shaderSource)
        {
        }

        public TesselationControlShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
        }

        protected override ShaderType Type() => ShaderType.TessControlShader;
    }
}
