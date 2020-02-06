using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StopVR : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(DeactivateVR("VR"));
	}

    IEnumerator DeactivateVR(string VR)
    {
        XRSettings.LoadDeviceByName(VR);
        yield return null;
        XRSettings.enabled = false;
    }
	
}
