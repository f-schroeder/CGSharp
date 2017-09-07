
namespace CGSharp.Shaders
{
    public class ShaderException : System.Exception
    {
        public ShaderException() : base() { }
        public ShaderException(string message) : base(message) { }
        public ShaderException(string message, System.Exception inner) : base(message, inner) { }

    }
}
