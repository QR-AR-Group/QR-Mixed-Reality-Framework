using System;
using UnityEngine;

namespace ImageTracking
{
    public class ContentParameters
    {
        public string URL { get; private set; }
        public Vector3 Offset { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public ContentParameters(string url, Vector3 offset, float width, float height)
        {
            URL = url ?? throw new ArgumentNullException(nameof(url));
            Offset = offset;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return $"URL: {URL}, Offset: {Offset.ToString()}, Width: {Width}, Height: {Height}";
        }
    }
}