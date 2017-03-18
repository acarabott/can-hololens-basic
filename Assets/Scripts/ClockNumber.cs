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

    var gpRotation = 360.0f - grandParent.transform.eulerAngles.z;
    var angle = angles.z + 90.0f + gpRotation;
    var x = Mathf.Cos((Mathf.PI / 180.0f) * angle) * radius;
    var y = Mathf.Sin((Mathf.PI / 180.0f) * angle) * radius;
    x += grandParent.transform.position.x;
    y += grandParent.transform.position.y;

    var position = transform.position;
    position.x = x;
    position.y = y;
    transform.position = position;

	}

	// Update is called once per frame
	void Update () {

	}
}
