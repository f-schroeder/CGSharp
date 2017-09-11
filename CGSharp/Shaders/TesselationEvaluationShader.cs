using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>Base class for tesselation evaluation shaders. See <see cref="Shader"/> for more information.</summary>
    public class TesselationEvaluationShader : Shader
    {
        public TesselationEvaluationShader(string shaderSource) : base(shaderSource)
        {
        }

        public TesselationEvaluationShader(string shaderSource, string[] shaderIncludePaths) : base(shaderSource, shaderIncludePaths)
        {
        }

        protected override ShaderType Type() => ShaderType.TessEvaluationShader;
    }
}
