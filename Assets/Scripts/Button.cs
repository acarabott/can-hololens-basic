using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class Button : MonoBehaviour, IInputClickHandler {

  public string buttonValue;
  public GameObject textObj;

  // Use this for initialization
  void Start () {
    textObj = this.transform.Find("text").gameObject;
    if (textObj != null) {
      TextMesh textMesh = textObj.GetComponent( typeof(TextMesh) ) as TextMesh;
      textMesh.text = buttonValue;
    }
  }

  // Update is called once per frame
  void Update () {

  }

  public void OnInputClicked(InputEventData eventData)
  {
        this.transform.SendMessageUpwards("OnInput", buttonValue);
  }
}
