using System;

namespace ImageTracking
{
    public class ContentContainer
    {
        public ContentParameters Parameters { get; private set; }
        public VirtualContent ContentPrefab { get; private set; }

        private VirtualContent _instantiatedContent;

        public ContentContainer(ContentParameters parameters, VirtualContent prefab)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ContentPrefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
        }
    }
}