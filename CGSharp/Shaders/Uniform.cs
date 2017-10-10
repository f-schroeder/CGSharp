using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    /// <summary>
    /// Class for managing a OpenGL uniform variable inside a shader.
    /// Contains the name, shader program, location and data of the uniform.
    /// Supports the following number data types: float, double, int, uint, UInt64.
    /// Supports the following floating-point vector data types: <see cref="Vector2"/>, <see cref="Vector3"/>, <see cref="Vector4"/>.
    /// Supports the following floating-point matrix data types: <see cref="Matrix2"/>, <see cref="Matrix3"/>, <see cref="Matrix4"/>.
    /// For bindless textures and images use the GPU adress as UInt64.
    /// </summary>
    /// <typeparam name="T">The type of the data inside the uniform.</typeparam>
    public class Uniform<T> : IUniform
    {
        /// <summary>
        /// The literal name of the uniform inside the shader.
        /// </summary>
        public string Name;

        /// <summary>
        /// The corresponding shader program ID this uniform belongs to.
        /// </summary>
        public int ProgramID;

        /// <summary>
        /// The uniform location inside the shader program.
        /// </summary>
        public int Location;

        /// <summary>
        /// The data stored in the uniform and passed to the shader program.
        /// </summary>
        public T Data
        {
            get => _data;
            set
            {
                _dataChanged = true;
                _data = value;
            }
        }

        private readonly Action _glProgramUniformFunction;
        private bool _dataChanged;
        private T _data;

        /// <summary>
        /// Updates the uniform data in the shader program (i.e. pushes it to the GPU).
        /// Only does so if the data changed since the last call of Update().
        /// </summary>
        public void Update()
        {
            if (_dataChanged)
            {
                _glProgramUniformFunction.Invoke();
                _dataChanged = false;
            }
        }

        /// <summary>
        /// Sets the data contained in the uniform.
        /// Alternative to setting the Data property directly.
        /// Provided by interface, no cast required.
        /// </summary>
        /// <typeparam name="TData">Type of the data. Must be the same as the generic type of the Uniform object.</typeparam>
        /// <param name="newData">The new data for the uniform.</param>
        public void SetData<TData>(TData newData)
        {
            if (typeof(TData) == typeof(T))
                Data = (T) Convert.ChangeType(newData, typeof(T));
            else
                throw new ArgumentException("Error: Called Uniform.SetData with wrong Type.");
        }

        /// <summary>
        /// Constructor for a Uniform object.
        /// Chooses the right GL.ProgramUniform... function depending on the generic type of the Uniform.
        /// </summary>
        /// <param name="uniformName">The literal name of the uniform inside the shader.</param>
        /// <param name="programID">The shader program this uniform belongs to.</param>
        /// <param name="uniformLocation">The uniform location in the shader program.</param>
        public Uniform(string uniformName, int programID, int uniformLocation)
        {
            Name = uniformName;
            ProgramID = programID;
            Location = uniformLocation;

            if (typeof(T) == typeof(float))
                _glProgramUniformFunction = () => GL.ProgramUniform1(ProgramID, Location, Convert.ToSingle(Data));
            else if (typeof(T) == typeof(double))
                _glProgramUniformFunction = () => GL.ProgramUniform1(ProgramID, Location, Convert.ToDouble(Data));
            else if (typeof(T) == typeof(int))
                _glProgramUniformFunction = () => GL.ProgramUniform1(ProgramID, Location, Convert.ToInt32(Data));
            else if (typeof(T) == typeof(uint))
                _glProgramUniformFunction = () => GL.ProgramUniform1(ProgramID, Location, Convert.ToUInt32(Data));
            else if (typeof(T) == typeof(UInt64))
                _glProgramUniformFunction = () => GL.ProgramUniform1(ProgramID, Location, Convert.ToUInt64(Data));
            else if (typeof(T) == typeof(Vector2))
                _glProgramUniformFunction = () => GL.ProgramUniform2(ProgramID, Location,
                    (Vector2) (Convert.ChangeType(Data, typeof(Vector2)) ?? Vector2.Zero));
            else if (typeof(T) == typeof(Vector3))
                _glProgramUniformFunction = () => GL.ProgramUniform3(ProgramID, Location,
                    (Vector3) (Convert.ChangeType(Data, typeof(Vector3)) ?? Vector3.Zero));
            else if (typeof(T) == typeof(Vector4))
                _glProgramUniformFunction = () => GL.ProgramUniform4(ProgramID, Location,
                    (Vector4) (Convert.ChangeType(Data, typeof(Vector4)) ?? Vector4.Zero));
            else if (typeof(T) == typeof(Matrix2))
                _glProgramUniformFunction = () =>
                {
                    var mat = (Matrix2)(Convert.ChangeType(Data, typeof(Matrix2)) ?? Matrix2.Identity);
                    GL.ProgramUniformMatrix2(ProgramID, Location, false, ref mat);
                };
            else if (typeof(T) == typeof(Matrix3))
                _glProgramUniformFunction = () =>
                {
                    var mat = (Matrix3)(Convert.ChangeType(Data, typeof(Matrix3)) ?? Matrix3.Identity);
                    GL.ProgramUniformMatrix3(ProgramID, Location, false, ref mat);
                };
            else if (typeof(T) == typeof(Matrix4))
                _glProgramUniformFunction = () =>
                {
                    var mat = (Matrix4)(Convert.ChangeType(Data, typeof(Matrix4)) ?? Matrix4.Identity);
                    GL.ProgramUniformMatrix4(ProgramID, Location, false, ref mat);
                };
        }
    }

    /// <summary>
    /// Calss containing static factory methods for crating <see cref="IUniform"/> objects.
    /// </summary>
    public abstract class UniformFactory
    {
        private UniformFactory()
        {
        }

        /// <summary>
        /// Creates a <see cref="Uniform{T}"/> with a given type parameter provided by OpenGL introspection.
        /// If the type doesn't match the supported types in <see cref="Uniform{T}"/>, it will be created with the type <see cref="UInt64"/>.
        /// </summary>
        /// <param name="type">The OpenGL type representation of the uniform.</param>
        /// <param name="uniformName">The literal name of the uniform inside the shader.</param>
        /// <param name="programID">The shader program this uniform belongs to.</param>
        /// <param name="uniformLocation">The uniform location in the shader program.</param>
        /// <returns></returns>
        public static IUniform MakeUniformFromOpenGlType(ActiveUniformType type, string uniformName, int programID,
            int uniformLocation)
        {
            switch (type)
            {
                case ActiveUniformType.Float:
                    return new Uniform<float>(uniformName, programID, uniformLocation);
                case ActiveUniformType.Double:
                    return new Uniform<double>(uniformName, programID, uniformLocation);
                case ActiveUniformType.Int:
                    return new Uniform<int>(uniformName, programID, uniformLocation);
                case ActiveUniformType.UnsignedInt:
                    return new Uniform<uint>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatVec2:
                    return new Uniform<Vector2>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatVec3:
                    return new Uniform<Vector3>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatVec4:
                    return new Uniform<Vector4>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatMat2:
                    return new Uniform<Matrix2>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatMat3:
                    return new Uniform<Matrix3>(uniformName, programID, uniformLocation);
                case ActiveUniformType.FloatMat4:
                    return new Uniform<Matrix4>(uniformName, programID, uniformLocation);
                default:
                    Debug.WriteLine("Created uniform object [" + uniformName + "] of type UInt64 for type parameter " + type + ". " +
                                    "If you want to use samplers or images, pass their address as uniform UInt64 (i.e. use bindless OpenGL).", "INFO");
                    return new Uniform<UInt64>(uniformName, programID, uniformLocation);
            }
        }
    }
}
