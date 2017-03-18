using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockNumber : MonoBehaviour {
  public float radius = 0.18f;

	// Use this for initialization
	void Start () {
    Button parentButton = transform.parent.GetComponent<Button>();
    GameObject grandParent = transform.parent.transform.parent.gameObject;

    var angles = transform.parent.eulerAngles;
    transform.Rotate(new Vector3(0.0f, 0.0f, -angles.z));


    var position = transform.position;
    // var radius = 0.18f;
    var x = Mathf.Cos((Mathf.PI / 180.0f) * (angles.z + 90.0f)) * radius;
    var y = Mathf.Sin((Mathf.PI / 180.0f) * (angles.z + 90.0f)) * radius;
    x += grandParent.transform.position.x;
    y += grandParent.transform.position.y;

    position.x = x;
    position.y = y;
    transform.position = position;

	}

	// Update is called once per frame
	void Update () {

	}
}
