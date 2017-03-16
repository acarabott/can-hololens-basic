using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {

  public string buttonValue;
  public GameObject textObj;

  // Use this for initialization
  void Start () {
    textObj = this.transform.Find("cube text").gameObject;
    if (textObj != null) {
      TextMesh textMesh = textObj.GetComponent( typeof(TextMesh) ) as TextMesh;
      textMesh.text = buttonValue;
    }
  }

  // Update is called once per frame
  void Update () {

  }

  // Called by GazeGestureManager when the user performs a Select gesture
  void OnSelect()
  {
    Debug.Log(buttonValue);
  }
}
