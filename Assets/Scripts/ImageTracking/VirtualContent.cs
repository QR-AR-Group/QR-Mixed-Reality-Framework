using System;
using UnityEngine;

namespace ImageTracking
{
    // Can have multiple objects and scripts attached -> Web View for displaying a webpage etc.
    public class VirtualContent : MonoBehaviour
    {
        // Later components of the Virtual Component can be tagged and searched (e.g. "FindGameObjectWithTag("WebView")
        //public GameObject PrefabContainer { get; private set; }
        public ContentParameters Parameters { get; private set; }
        public Vector3 Scale { get; private set; }
        //public Vector3 ScaledOffset { get; private set; }

        public void Initialize(ContentParameters contentParameters)
        {
            Parameters = contentParameters ?? throw new ArgumentNullException(nameof(contentParameters));
            Scale = new Vector3(Parameters.Width, 1f, Parameters.Height);

            // Do stuff
            //PrefabContainer = transform.GetChild(0).gameObject;
        }
    }
}