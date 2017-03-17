using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputManager : MonoBehaviour {

    public TextMesh outputText;

	// Use this for initialization
	void Start () {
        outputText.text = "";
	}

	void OnInput(string value)
    {
        if (value == "-") {
            outputText.text = outputText.text.StartsWith("-") ? outputText.text.Remove(0, 1) : "-" + outputText.text;
        }
        else if (value == "." && !outputText.text.Contains("."))
        {
            outputText.text += value;
        }
    }
}
