using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.FilterReader
{
    /// <summary>
    /// Set of helper methods which hide FilterReader under nicer interface. Most usable for smaller files, since bigger ones require reading by chunks.
    /// </summary>
    public static class FilterHelper
    {
        /// <summary>
        /// Stream lines one by one.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadAllLines(string path)
        {
            using (var reader = new FilterReader(path))
            {
                while (reader.Peek() != -1)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Stream lines one by one.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadAllLines(Stream stream, string extension)
        {
            using (var reader = new FilterReader(stream, extension))
            {
                while (reader.Peek() != -1)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Check if IFilter is available for specific extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool IsFilterAvailable(string extension)
        {
            return FilterLoader.FilterIsInstalledFor(extension);
        }

        /// <summary>
        /// Read whole file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RealAll(string path)
        {
            using (var reader = new FilterReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Read whole file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string ReadAll(Stream stream, string extension)
        {
            using (var reader = new FilterReader(stream, extension))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
