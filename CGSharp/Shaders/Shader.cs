using System;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using Boolean = OpenTK.Graphics.OpenGL4.Boolean;

namespace CGSharp.Shaders
{
    /// <summary>
    /// Base class for all shaders. 
    /// Provides basic functionality for loading and compiling shaders. 
    /// Also checks for compilation errors. 
    /// As it allocates GPU resources, you should dispose it once it's no longer needed.
    /// 
    /// It also supports the ARB_shading_language_include extension to use '#include' in your shader.
    /// 
    /// Note that this class only handles loading and compilation of shaders. 
    /// For shader program creation and linking use the <see cref="ShaderProgram"/> class.
    /// </summary>
    public abstract class Shader : IDisposable
    {
        /// <summary>The OpenGL handle for this shader.</summary> 
        public int ID { get; protected set; }

        /// <summary>
        /// Specifies the actual type of the shader (i.e. Vertex, Fragment, Compute, etc.)
        /// Derived classes have to specify their type.
        /// </summary>
        /// <returns> The shader type.</returns>
        protected abstract ShaderType Type();

        /// <summary>
        /// Constructor for creating a shader for the specified source code.
        /// The source code can be either provided directly as string or by passing the path to the file containing the source code.
        /// </summary>
        /// <param name="shaderSource">EITHER The shader source code OR The path to the shader source file.</param>
        protected Shader(string shaderSource)
        {
            CreateShader(shaderSource);
        }

        /// <summary>
        /// Constructor for creating a shader for the specified source code.
        /// The source code can be either provided directly as string or by passing the path to the file containing the source code.
        /// You can pass additional paths to shader-files which will be treated as shader-includes.
        /// </summary>
        /// <param name="shaderSource">EITHER The shader source code OR The path to the shader source file. This should contain the main method of the shader.</param>
        /// <param name="shaderIncludePaths">The paths to the additional shader source files to be included.</param>
        protected Shader(string shaderSource, string[] shaderIncludePaths)
        {
            foreach (var includePath in shaderIncludePaths)
            {
                string includeSource = File.ReadAllText(includePath);
                string includeName = "/" + Path.GetFileName(includePath);

                GL.Arb.NamedString(All.ShaderIncludeArb, includeName.Length, includeName, includeSource.Length, includeSource);
                if (!GL.Arb.IsNamedString(includeName.Length, includeName))
                    throw new ShaderException("Error: Coud not create named string for shader includes.");

                Debug.WriteLine("Shader: Successfully loaded shader inlude: [" + includeName + "]", "INFO");
            }

            CreateShader(shaderSource);
        }

        /// <summary>Loads and compiles the OpenGL shader.</summary>
        /// <param name="shaderSource">Source string or path to source file.</param>
        protected void CreateShader(string shaderSource)
        {
            string source = Path.HasExtension(shaderSource) ? File.ReadAllText(shaderSource) : shaderSource;

            ID = GL.CreateShader(Type());
            GL.ShaderSource(ID, source);

            //Compile the shader
            GL.CompileShader(ID);
            CheckCompilationErrors();

            Debug.WriteLine("Shader: Successfully created shader from " + (Path.HasExtension(shaderSource) ? "file [" + Path.GetFileName(shaderSource) + "]." : "source string."), "INFO");
        }

        /// <summary>Checks if shader compilation was succesful.</summary>
        /// <exception cref="ShaderException">Thrown if compilation failed.</exception>
        protected void CheckCompilationErrors()
        {
            int isCompiled;
            GL.GetShader(ID, ShaderParameter.CompileStatus, out isCompiled);
            if (isCompiled.Equals(Boolean.False))
            {
                string infoLog = GL.GetShaderInfoLog(ID);

                GL.DeleteShader(ID);

                throw new ShaderException("Error: Shader compilation failed. \n" + infoLog);
            }
        }

        /// <summary>
        /// Override this function if you have to destroy other OpenGL resources.
        /// Called from public Dispose() function.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteShader(ID);
            }
        }

        /// <summary>Call Dispose() to free/delete allocated GPU and/or OpenGL resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
