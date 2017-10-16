using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CGSharp.Utils
{
    /// <summary>
    /// Static class to find assets in the file system by name.
    /// Pre-added paths are:
    /// ./Shaders/
    /// ./Data/
    /// ./Assets/
    /// ./Textures/
    /// ./Models/
    /// </summary>
    public static class AssetManager
    {
        private static readonly List<string> SearchPaths = new List<string>
        {
            "./Shaders/",
            "./Data/",
            "./Assets/",
            "./Textures/",
            "./Models/"
        };

        /// <summary>
        /// Adds the specified path to the list of search paths used by the asset manager.
        /// </summary>
        /// <param name="path">The path to add to the list of search paths. Must be a valid directory.</param>
        public static void AddSearchPath(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                SearchPaths.Add(path);
                Debug.WriteLine("AssetManager: Added " + path + " to search paths.", "INFO");
            }
            else
            {
                Debug.WriteLine("AssetManager: " + path + "is not a valid diractory and/or could not be added to the search paths", "ERROR");
            }
        }

        /// <summary>
        /// Searches project related paths for the specified file.
        /// </summary>
        /// <param name="filename">The name of the file (not the path) to search for.</param>
        /// <returns>The entire path to the file with the specified filename.</returns>
        /// <exception cref="FileNotFoundException">Thrown if no file with the specified filename was found.</exception>
        public static string Find(string filename) 
        {
            foreach (var dir in SearchPaths)
            {
                string fullpath = ProcessDirectory(dir, filename);
                if (fullpath != null)
                    return fullpath;
            }

            throw new FileNotFoundException("AssetLoader could not find a file with the name " + filename);
        }

        private static string ProcessDirectory(string path, string filename)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                    if (file.Contains(filename))
                        return file;
                foreach (var dir in Directory.GetDirectories(path))
                    return ProcessDirectory(dir, filename);
            }
            return null;
        }

    }
}
