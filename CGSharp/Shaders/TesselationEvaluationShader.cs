using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
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
