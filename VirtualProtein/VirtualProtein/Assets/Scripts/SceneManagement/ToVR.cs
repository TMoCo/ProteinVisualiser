using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ToVR : MonoBehaviour {
    
	public void Start () {
        StartCoroutine(ActivateVR("VR"));
	}

    public IEnumerator ActivateVR(string VR) {
        XRSettings.LoadDeviceByName(VR);
        yield return null;
        XRSettings.enabled = true;
    }
	
}
