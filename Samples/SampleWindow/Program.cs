using System;
using System.Drawing;
using CGSharp;

namespace Samples
{
    public class Program : CGProgram
    {

        [STAThread]
        public static void Main()
        {
            // The Main method should always look like this
            using (new Program()) { }
        }

        protected override void OnLoad()
        {
            // Do init stuff here (like window size, window title, loading shaders, textures, etc.)
            WindowSize = new Size(1920, 1080);
            WindowTitle = "CGSharp Sample Window";
        }

        protected override void OnUpdateFrame(double deltaTime)
        {
            // Do updates not related directly to rendering here (like camera, physics, etc.)
        }

        protected override void OnRenderFrame(double deltaTime)
        {
            // Do rendering updates here (like uniforms, draw calls, etc.)
        }

        protected override void OnUnload()
        {
            // Do cleanup stuff here (like deallocating GPU resources)
        }
    }
}
