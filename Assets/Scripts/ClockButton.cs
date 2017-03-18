using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var angles = transform.eulerAngles;

    var position = transform.localPosition;
    // Debug.LogFormat("this: {0}", position);
    // var radius = transform.localScale;
    // var radius = 1.0f / transform.localScale.x;
    var radius = 0.09f;
    var x = Mathf.Cos((Mathf.PI / 180.0f) * (angles.z + 90.0f)) * radius;
    var y = Mathf.Sin((Mathf.PI / 180.0f) * (angles.z + 90.0f)) * radius;

    position.x = x;
    position.y = y;
    // transform.localPosition = position;
	}

	// Update is called once per frame
	void Update () {

	}
}
