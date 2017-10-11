using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using Boolean = OpenTK.Graphics.OpenGL4.Boolean;
using Buffer = CGSharp.Buffers.Buffer;

//TODO: Extract program inputs and outputs? --> automatic FBO and VBO/VAO creation?

namespace CGSharp.Shaders
{
    /// <summary>
    /// Base class for shader programs.
    /// Creates and handles a shader program from one or more shader stages (<see cref="Shader"/>).
    /// Extracts uniform and buffer variables from the shader program.
    /// Also checks for linking errors. 
    /// As it allocates GPU resources, you should dispose it once it's no longer needed.
    /// 
    /// To set uniform variables, use the extracted uniform objects.
    /// To bind buffers, use the extracted buffer bindings provided by this class.
    /// </summary>
    public class ShaderProgram : IDisposable
    {
        /// <summary>The OpenGL handle for this shader program</summary>
        public int ID { get; protected set; }

        /// <summary>
        /// The extracted uniforms from the shader program. 
        /// Use these instead of querying them separately with GL.GetUniformLocation() as they have less overhead.
        /// Key is the name of the uniform variable.
        /// Value is the Uniform object containing the location. 
        /// Use SetData() and Update() from <see cref="IUniform"/> to manipulate the data contained in the uniform.
        /// </summary>
        public ReadOnlyDictionary<string, IUniform> Uniforms { get; protected set; }

        /// <summary>
        /// The extracted buffers from the shader program.
        /// Key is the name of the buffer variable.
        /// Value is the binding point of the buffer.
        /// </summary>
        public ReadOnlyDictionary<string, int> Buffers { get; protected set; }

        /// <summary>Constructor for creating a shader program with the specified shaders.</summary> 
        /// <param name="shaders">Array of shaders representing the shader stages for the shader program.</param>
        public ShaderProgram(Shader[] shaders)
        {
            CreateProgram(shaders);
            ExtractUniforms();
            ExtractBuffers();
        }

        /// <summary>
        /// Updates the parameters of the shader program (i.e. uniforms and buffers).
        /// </summary>
        public virtual void Update()
        {
            UpdateUniforms();
            UpdateBuffers();
        }

        /// <summary>
        /// Updates the uniform variables of the shader program.
        /// </summary>
        public void UpdateUniforms()
        {
            foreach (var keyValuePair in Uniforms)
            {
                keyValuePair.Value.Update();
            }
        }

        /// <summary>
        /// Updates all buffer variables of the shader program (i.e. 'BindBase' for all buffers)
        /// </summary>
        public void UpdateBuffers()
        {
            foreach (var keyValuePair in Buffers)
            {
                Buffer buffer;
                if (Buffer.RegisteredBuffers.TryGetValue(keyValuePair.Key, out buffer))
                {
                    buffer.BindBase(keyValuePair.Value);
                }
            }
        }

        /// <summary>Performs introspection on the shader program to extract all defined uniform variables.</summary>
        protected void ExtractUniforms()
        {
            var uniforms = new Dictionary<string, IUniform>();

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

                uniforms[name.ToString()] = UniformFactory.MakeUniformFromOpenGlType((ActiveUniformType) values[1], name.ToString(), ID, values[3]);
            }

            Uniforms = new ReadOnlyDictionary<string, IUniform>(uniforms);
            Debug.WriteLine("ShaderProgram: Found " + uniforms.Count + " uniforms.", "INFO");
        }

        /// <summary>Performs introspection on the shader program to extract all defined buffer variables.</summary>
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

        /// <summary>Creates the OpenGL shader program.</summary>
        /// <param name="shaders">The shader stages to use for the shader program.</param>
        protected void CreateProgram(Shader[] shaders)
        {
            ID = GL.CreateProgram();

            Array.ForEach(shaders, shader => GL.AttachShader(ID, shader.ID));

            GL.LinkProgram(ID);
            CheckLinkingErrors(shaders);

            Array.ForEach(shaders, shader => GL.DetachShader(ID, shader.ID));

            Debug.WriteLine("ShaderProgram: Created shader program with " + shaders.Length + " shader stages.", "INFO");
        }

        /// <summary>Checks if shader program linking was successful.</summary>
        /// <param name="shaders">The used shader stages of the shader program.</param>
        /// <exception cref="ShaderException">Thrown if linking failed.</exception>
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

        /// <summary>
        /// Override this function if you have to destroy other OpenGL resources.
        /// Called from public Dispose() function.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteProgram(ID);
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

