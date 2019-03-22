using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GalacticWasteManagement.Utilities
{
    public static class EmbeddedResourceReader
    {
        public static IEnumerable<ResourceFile> GetResourcesFrom(Assembly assembly, Func<string, bool> filter)
        {
            return assembly.GetManifestResourceNames()
                .Where(filter)
                .OrderBy(x => x)
                .Select(x => new ResourceFile(assembly, x))
                .ToList();
        }
    }

    public class ResourceFile
    {
        public ResourceFile(Assembly assembly, string resourceKey)
        {
            Assembly = assembly;
            ResourceKey = resourceKey;
            var resourceInfo = Assembly.GetManifestResourceInfo(resourceKey);
            ResourceLocation = resourceInfo.ResourceLocation;
        }

        private Assembly Assembly { get; set; }
        public string ResourceKey { get; protected set; }
        public ResourceLocation ResourceLocation { get; protected set; }

        public virtual string Read()
        {
            return Read(stream =>
            {
                string result;
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                return result;
            });
        }

        public virtual TResult Read<TResult>(Func<Stream, TResult> reader)
        {
            TResult result;
            using (var stream = Assembly.GetManifestResourceStream(ResourceKey))
            {
                result = reader(stream);
            }

            return result;
        }

        public string ResourceName
        {
            get
            {
                return string.Join(".", ResourceKey.Replace(Assembly.GetName().Name + ".", ""));
            }
        }
    }
}