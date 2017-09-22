namespace CGSharp.Shaders
{
    /// <summary>
    /// Interface for <see cref="Uniform{T}"/>.
    /// Used to access <see cref="Uniform{T}"/> without having to cast the generic type.
    /// </summary>
    public interface IUniform
    {
        /// <summary>
        /// Updates the uniform data in the shader program (i.e. pushes it to the GPU).
        /// Only does so if the data changed since the last call of Update().
        /// </summary>
        void Update();

        /// <summary>
        /// Sets the data contained in the uniform.
        /// Alternative to setting the Data property directly.
        /// Provided by interface, no cast required.
        /// </summary>
        /// <typeparam name="TData">Type of the data. Must be the same as the generic type of the <see cref="Uniform{T}"/> object.</typeparam>
        /// <param name="newData">The new data for the uniform.</param>
        void SetData<TData>(TData newData);
    }
}
