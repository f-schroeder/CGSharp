using System;
using CGSharp;

namespace Samples
{
    public class Program : CGProgram
    {

        [STAThread]
        public static void Main()
        {
            using (new Program()) { }
        }

        protected override void OnLoad()
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdateFrame(double deltaTime)
        {
            throw new NotImplementedException();
        }

        protected override void OnRenderFrame(double deltaTime)
        {
            throw new NotImplementedException();
        }

        protected override void OnUnload()
        {
            throw new NotImplementedException();
        }
    }
}
