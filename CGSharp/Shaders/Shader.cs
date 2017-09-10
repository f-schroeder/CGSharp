using System;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using Boolean = OpenTK.Graphics.OpenGL4.Boolean;

//TODO: Documentation!

namespace CGSharp.Shaders
{
    public abstract class Shader : IDisposable
    {
        public int ID { get; protected set; }

        protected abstract ShaderType Type();

        protected Shader(string shaderSource)
        {
            CreateShader(shaderSource);
        }

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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteShader(ID);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
