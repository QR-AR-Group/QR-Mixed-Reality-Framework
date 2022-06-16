using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ImageTracking
{
    // Can have multiple objects and scripts attached -> Web View for displaying a webpage etc.
    public class VirtualContent : MonoBehaviour
    {
        public ContentParameters Parameters { get; private set; }
        private bool _initialized;

        // Parent and assigned Image of Virtual Content
        private ARTrackedImage _trackedImage;

        // This could hold a Web View for example 
        private GameObject _planeContainer;

        public void Initialize(ContentParameters contentParameters, ARTrackedImage trackedImage)
        {
            Parameters = contentParameters ?? throw new ArgumentNullException(nameof(contentParameters));
            _trackedImage = trackedImage;

            // Later components of the Virtual Component can be tagged and searched (e.g. "FindGameObjectWithTag("WebView")
            _planeContainer = transform.GetChild(0).gameObject;
            if (_planeContainer)
            {
                _initialized = true;
                _planeContainer.transform.position = transform.position + Parameters.Offset;
                /* As the object being scaled is a plane -> only x and z are important 
                 * For different containers/objects all 3 params are important to set
                 * (in most of these cases z = 1f)
                 */
                _planeContainer.transform.localScale = new Vector3(Parameters.Width, 1f, Parameters.Height);
            }
        }
    }
}