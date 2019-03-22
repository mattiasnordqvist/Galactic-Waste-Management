using System.IO;
using System.Security.Cryptography;

namespace GalacticWasteManagement.Utilities
{
        public static class Hashing
    {
        public static string CreateHash(string content)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(GenerateStreamFromString(content));
                return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}