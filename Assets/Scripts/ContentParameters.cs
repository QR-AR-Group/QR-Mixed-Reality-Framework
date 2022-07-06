using System;
using UnityEngine;

public class ContentParameters
{
    public string URL;
    public Vector3 Offset;
    public float Width;
    public float Height;
    public bool Transparency;

    public ContentParameters()
    {
    }

    public ContentParameters(string url, Vector3 offset, float width, float height, bool transparency)
    {
        URL = url ?? throw new ArgumentNullException(nameof(url));
        Offset = offset;
        Width = width;
        Height = height;
        Transparency = transparency;
    }

    public override string ToString()
    {
        return $"URL: {URL}, Offset: {Offset}, Width: {Width}, Height: {Height}, Transparency?: {Transparency}";
    }
}