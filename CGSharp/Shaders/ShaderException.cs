
namespace CGSharp.Shaders
{
    /// <summary>Class for exceptions regarding shader compilation and linking.</summary>
    public class ShaderException : System.Exception
    {
        public ShaderException() : base() { }
        public ShaderException(string message) : base(message) { }
        public ShaderException(string message, System.Exception inner) : base(message, inner) { }

    }
}
