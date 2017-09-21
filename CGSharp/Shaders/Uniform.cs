using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace CGSharp.Shaders
{
    public class Uniform<T> : IUniform
    {
        public string Name;
        public int ProgramID;
        public int Location;

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

        public void Update()
        {
            if (_dataChanged)
                _glProgramUniformFunction.Invoke();
        }

        public void SetData<TData>(TData newData)
        {
            if (typeof(TData) == typeof(T))
                Data = (T) Convert.ChangeType(newData, typeof(T));
            else
                throw new ArgumentException("Error: Called Uniform.SetData with wrong Type.");
        }

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

    public abstract class UniformFactory
    {
        private UniformFactory()
        {
        }

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
                    Debug.WriteLine("Created uniform object [" + uniformName + "] of type UInt64 for type parameter " + type + ".", "INFO");
                    return new Uniform<UInt64>(uniformName, programID, uniformLocation);
            }
        }
    }
}
