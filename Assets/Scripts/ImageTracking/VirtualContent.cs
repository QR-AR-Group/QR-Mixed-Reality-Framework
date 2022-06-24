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
        public Vector3 ScaledOffset { get; private set; }

        public void Initialize(ContentParameters contentParameters)
        {
            Parameters = contentParameters ?? throw new ArgumentNullException(nameof(contentParameters));

            /* I guess as the object being scaled is a plane -> only x and z are important 
             * For different containers/objects all 3 params are important to set (with z=1f)
             */
            Scale = new Vector3(Parameters.Width, 1f, Parameters.Height);
            float scaledX = Parameters.Offset.x / Scale.x;
            float scaledY = Parameters.Offset.y / Scale.y;
            float scaledZ = Parameters.Offset.z / Scale.z;
            Vector3 scaledOffset = new Vector3(scaledX, scaledY, scaledZ);
            ScaledOffset = scaledOffset;
            
            // Do stuff
            //PrefabContainer = transform.GetChild(0).gameObject;
        }
    }
}