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

namespace CGSharp
{
    /// <summary>
    /// This is the base class for all CGSharp programs.
    /// The class containing your Main() function should inherit from this class and implement the abstract functions for loading, updating, rendering and unloading.
    /// There are also several optional virtual functions which you can override if their functionality is needed.
    /// 
    /// This class manages the window for the OpenGL rendering and basic IO.
    /// It also contains some basic program states like camera, shader and scene.
    /// It implements the singleton pattern, so you can access it from anywhere by calling the Instance() function.
    /// </summary>
    /// <example>
    /// This example shows a typical main method used in a derived class of a CGProgram.
    /// <code>
    /// [STAThread]
    /// public static void Main()
    /// {
    ///     using (new Program()) { } //Program inherits from CGProgram.
    /// }
    /// </code>
    /// </example>
    public abstract class CGProgram : IDisposable
    {
        private static CGProgram _instance;
        private readonly GameWindow _window;

        /// <summary>Program state: The active camera for rendering.</summary>
        public Camera ActiveCamera { get; set; }
        /// <summary>Program state: The active shader for rendering or compute.</summary>
        public ShaderProgram ActiveShader { get; set; }
        /// <summary>Program state: The active scene for rendering.</summary>
        public Scene ActiveScene { get; set; }

        /// <summary>Singleton: Get the instance of the program to access its state variables.</summary>
        /// <exception cref="AccessViolationException">Thrown if no instance exists.</exception>
        public static CGProgram Instance
        {
            get
            {
                if (_instance == null)
                    throw new AccessViolationException("Error: There is no instance of a CGProgram.");
                
                return _instance;
            }
        }

        /// <summary>Sets the title of the window.</summary>
        public string WindowTitle
        {
            set => _window.Title = value;
        }

        /// <summary>Sets the size of the window</summary>
        public Size WindowSize
        {
            get => _window.Size;
            set => _window.Size = value;
        }

        /// <summary>
        /// Base constructor for a program.
        /// Creates a window and adds functions to events.
        /// Then runs the program.
        /// </summary>
        /// <exception cref="AccessViolationException">Thrown if there is already an instance of a CGProgram.</exception>
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

        /// <summary>
        /// Override this function in your program.
        /// Do all initialization in this function.
        /// This function is called once before rendering starts.
        /// By default, it disables VSync, enables the depth test, sets the ClearColor to black and the default viewport to the dimensions of the window.
        /// </summary>
        protected abstract void OnLoad();

        #endregion

        #region OnUpdateFrame

        private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
        {
            OnUpdateFrame(frameEventArgs.Time);

            if (_window.Keyboard[Key.Escape])
                _window.Exit();
        }

        /// <summary>
        /// Override this function in your program.
        /// Do all updates in this function (i.e. uniforms, physics, etc.).
        /// This function is called every frame if no update limit is set (by default there is no limit).
        /// </summary>
        protected abstract void OnUpdateFrame(double deltaTime);

        #endregion

        #region OnRenderFrame

        private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            OnRenderFrame(frameEventArgs.Time);

            _window.SwapBuffers();
        }

        /// <summary>
        /// Override this function in your program.
        /// Do all rendering stuff in this function (i.e. draw calls, dispatch compute, etc.).
        /// This function is called every frame if no render limit is set (by default there is no limit).
        /// By default, it calls GL.Clear() on the color and the depth buffer.
        /// </summary>
        protected abstract void OnRenderFrame(double deltaTime);

        #endregion

        #region OnUnload

        private void OnUnload(object sender, EventArgs e)
        {
            OnUnload();
        }

        /// <summary>
        /// Override this function in your program.
        /// Do all cleaning up in this function.
        /// This function is called once before the program ends.
        /// </summary>
        protected abstract void OnUnload();

        #endregion


        #region OnKeyPress

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e.KeyChar);
        }

        /// <summary>
        /// You may override this function in your program.
        /// This function is called when a key is pressed.
        /// </summary>
        /// <param name="key">The char representation of the key.</param>
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

        /// <summary>
        /// You may override this function in your program.
        /// This function is called when a mouse button is pressed.
        /// </summary>
        /// <param name="x">The x coordinate of the mouse pointer.</param>
        /// <param name="y">The y coordinate of the mouse pointer.</param>
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

        /// <summary>
        /// You may override this function in your program.
        /// This function is called when the window is resized.
        /// By default, the viewport (index 0) is set to the new dimensions.
        /// </summary>
        /// <param name="newWidth">The new width of the window.</param>
        /// <param name="newHeight">The new height of the window.</param>
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
