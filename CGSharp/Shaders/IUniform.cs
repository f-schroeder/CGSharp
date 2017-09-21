namespace CGSharp.Shaders
{
    public interface IUniform
    {
        void Update();
        void SetData<T>(T newData);
    }
}
