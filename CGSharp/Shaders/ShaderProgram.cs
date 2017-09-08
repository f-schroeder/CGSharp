using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Boolean = OpenTK.Graphics.OpenGL.Boolean;

//TODO: Documentation!
//TODO: Extract program inputs and outputs? --> automatic FBO and VBO/VAO creation?
//TODO: differentiate extracted uniforms between numbers, samplers ans images

namespace CGSharp.Shaders
{
    public class ShaderProgram : IDisposable
    {
        public int ID { get; protected set; }

        public ReadOnlyDictionary<string, int> Uniforms { get; protected set; }
        public ReadOnlyDictionary<string, int> Buffers { get; protected set; }

        public ShaderProgram(Shader[] shaders)
        {
            CreateProgram(shaders);
            ExtractUniforms();
            ExtractBuffers();
        }

        protected void ExtractUniforms()
        {
            var uniforms = new Dictionary<string, int>();

            int numUniforms;
            GL.GetProgramInterface(ID, ProgramInterface.Uniform, ProgramInterfaceParameter.ActiveResources, out numUniforms);
            ProgramProperty[] properties = { ProgramProperty.BlockIndex, ProgramProperty.Type, ProgramProperty.NameLength, ProgramProperty.Location };

            for (int uniformID = 0; uniformID < numUniforms; ++uniformID)
            {
                int[] values = new int[4];
                GL.GetProgramResource(ID, ProgramInterface.Uniform, uniformID, 4, properties, 4, out int _, values);

                //Skip any uniforms that are in a block or have no name.
                if (values[0] != -1 || values[2] < 1)
                    continue;

                var name = new StringBuilder();
                //Need to disable this warning because indeed it is the correct usage of the function.
                #pragma warning disable 618
                GL.GetProgramResourceName(ID, ProgramInterface.Uniform, uniformID, values[2], out int _, name);
                #pragma warning restore 618

                uniforms[name.ToString()] = values[3];
            }

            Uniforms = new ReadOnlyDictionary<string, int>(uniforms);
            Debug.WriteLine("ShaderProgram: Found " + uniforms.Count + " uniforms.", "INFO");
        }

        protected void ExtractBuffers()
        {
            var buffers = new Dictionary<string, int>();

            int numBuffers;
            GL.GetProgramInterface(ID, ProgramInterface.ShaderStorageBlock, ProgramInterfaceParameter.ActiveResources, out numBuffers);
            ProgramProperty[] properties = { ProgramProperty.NameLength, ProgramProperty.BufferBinding};

            for (int bufferID = 0; bufferID < numBuffers; ++bufferID)
            {
                int[] values = new int[2];
                GL.GetProgramResource(ID, ProgramInterface.ShaderStorageBlock, bufferID, 2, properties, 2, out int _, values);

                //Skip buffers without name (if that's even possible).
                if (values[0] < 1)
                    continue;

                var name = new StringBuilder();
                //Need to disable this warning because indeed it is the correct usage of the function.
                #pragma warning disable 618
                GL.GetProgramResourceName(ID, ProgramInterface.ShaderStorageBlock, bufferID, values[0], out int _, name);
                #pragma warning restore 618

                buffers[name.ToString()] = values[1];
            }

            Buffers = new ReadOnlyDictionary<string, int>(buffers);
            Debug.WriteLine("ShaderProgram: Found " + buffers.Count + " buffers.", "INFO");
        }

        protected void CreateProgram(Shader[] shaders)
        {
            ID = GL.CreateProgram();

            Array.ForEach(shaders, shader => GL.AttachShader(ID, shader.ID));

            GL.LinkProgram(ID);
            CheckLinkingErrors(shaders);

            Array.ForEach(shaders, shader => GL.DetachShader(ID, shader.ID));

            Debug.WriteLine("ShaderProgram: Created shader program with " + shaders.Length + " shader stages.", "INFO");
        }

        protected void CheckLinkingErrors(Shader[] shaders)
        {
            int isLinked;
            GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out isLinked);
            if (isLinked.Equals(Boolean.False))
            {
                string infoLog = GL.GetProgramInfoLog(ID);

                GL.DeleteProgram(ID);
                Array.ForEach(shaders, shader => GL.DeleteShader(shader.ID));

                throw new ShaderException("Error: Shader linking failed. \n" + infoLog);
            }
        }

        #region SetUniform expressions

        public void SetUniform(string name, int value) => GL.ProgramUniform1(ID, Uniforms[name], value);

        public void SetUniform(string name, float value) => GL.ProgramUniform1(ID, Uniforms[name], value);

        public void SetUniform(string name, ref Vector2 value) => GL.ProgramUniform2(ID, Uniforms[name], ref value);

        public void SetUniform(string name, ref Vector3 value) => GL.ProgramUniform3(ID, Uniforms[name], ref value);

        public void SetUniform(string name, ref Vector4 value) => GL.ProgramUniform4(ID, Uniforms[name], ref value);

        public void SetUniform(string name, ref Matrix3 value) => GL.ProgramUniformMatrix3(ID, Uniforms[name], false, ref value);

        public void SetUniform(string name, ref Matrix4 value) => GL.ProgramUniformMatrix4(ID, Uniforms[name], false, ref value);

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteProgram(ID);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

