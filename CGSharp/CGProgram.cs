using System;
using System.Diagnostics;
using System.Drawing;
using CGSharp.Scenes;
using CGSharp.Scenes.Cameras;
using CGSharp.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

//TODO: Documentation

namespace CGSharp
{
    public abstract class CGProgram : IDisposable
    {
        private static CGProgram _instance;
        private readonly GameWindow _window;

        public Camera ActiveCamera { get; set; }
        public ShaderProgram ActiveShader { get; set; }
        public Scene ActiveScene { get; set; }

        public static CGProgram Instance
        {
            get
            {
                if (_instance == null)
                    throw new AccessViolationException("Error: There is no instance of a CGProgram.");
                
                return _instance;
            }
        }

        public string WindowTitle
        {
            set => _window.Title = value;
        }

        public Size WindowSize
        {
            get => _window.Size;
            set => _window.Size = value;
        }

        protected CGProgram()
        {
            if (_instance != null)
                throw new AccessViolationException("Error: Tried to create multiple instances of a CGProgram.");

            _instance = this;

            _window = new GameWindow(1280, 720,
                GraphicsMode.Default, "CG Program", GameWindowFlags.Default,
                DisplayDevice.Default, 4, 4,
                GraphicsContextFlags.ForwardCompatible | (Debugger.IsAttached
                    ? GraphicsContextFlags.Debug
                    : 0));

            _window.Load += OnLoad;
            _window.UpdateFrame += OnUpdateFrame;
            _window.RenderFrame += OnRenderFrame;
            _window.Unload += OnUnload;

            _window.KeyPress += OnKeyPress;
            _window.MouseDown += OnMouseDown;
            _window.Resize += OnResize;

            _window.Run();

            Debug.WriteLine("Program started.", "INFO");

        }

        #region OnLoad

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            _window.VSync = VSyncMode.Off;

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Black);
            GL.ViewportIndexed(0, 0, 0, _window.Width, _window.Height);

            OnLoad();
        }
        protected abstract void OnLoad();

        #endregion

        #region OnUpdateFrame

        private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
        {
            OnUpdateFrame(frameEventArgs.Time);

            if (_window.Keyboard[Key.Escape])
                _window.Exit();
        }

        protected abstract void OnUpdateFrame(double deltaTime);

        #endregion

        #region OnRenderFrame

        private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            OnRenderFrame(frameEventArgs.Time);

            _window.SwapBuffers();
        }

        protected abstract void OnRenderFrame(double deltaTime);

        #endregion

        #region OnUnload

        private void OnUnload(object sender, EventArgs e)
        {
            OnUnload();
        }

        protected abstract void OnUnload();

        #endregion


        #region OnKeyPress

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e.KeyChar);
        }

        protected void OnKeyPress(char key)
        {
        }

        #endregion

        #region OnMouseDown

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if(mouseButtonEventArgs.IsPressed)
                OnMouseDown(mouseButtonEventArgs.X, mouseButtonEventArgs.Y);
        }
        protected void OnMouseDown(int x, int y)
        {
        }

        #endregion    

        #region OnResize

        private void OnResize(object sender, EventArgs eventArgs)
        {
            GL.ViewportIndexed(0, 0, 0, _window.Width, _window.Height);

            OnResize(_window.Width, _window.Height);
        }

        protected void OnResize(int newWidth, int newHeight)
        {
        }

        #endregion


        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _window?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
