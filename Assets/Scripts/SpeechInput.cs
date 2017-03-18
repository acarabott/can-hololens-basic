using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class SpeechInput : MonoBehaviour, ISpeechHandler {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

  public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData) {
    Debug.Log(eventData.RecognizedText.ToLower());
    if (GazeGestureManager.Instance.FocusedObject == this.gameObject) {
      Debug.Log("looking good");
    }
  }
}
