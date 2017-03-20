using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class SpeechInput : MonoBehaviour, ISpeechHandler {
    Dictionary<string, string> lookup = new Dictionary<string, string>();

	// Use this for initialization
	void Start () {
        lookup.Add("zero", "0");
        lookup.Add("o", "0");
        lookup.Add("oh", "0");
        lookup.Add("one", "1");
        lookup.Add("two", "2");
        lookup.Add("three", "3");
        lookup.Add("four", "4");
        lookup.Add("five", "5");
        lookup.Add("six", "6");
        lookup.Add("seven", "7");
        lookup.Add("eight", "8");
        lookup.Add("nine", "9");
        lookup.Add("dot", ".");
        lookup.Add("point", ".");
        lookup.Add("decimal", ".");
        lookup.Add("negative", "-");
        lookup.Add("minus", "-");
        lookup.Add("positive", "-");
        lookup.Add("clear", "clear");
	}

	// Update is called once per frame
	void Update () {

	}

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData) {
        var text = eventData.RecognizedText.ToLower();
        Debug.LogFormat("Recognized: {0}", text);
        if (lookup.ContainsKey(text)) {
            Debug.Log("key found");
            this.transform.SendMessageUpwards("OnInput", lookup[text]);
        }
    }
}
