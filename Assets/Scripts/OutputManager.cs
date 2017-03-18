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
        if (value == "clear")
        {
            outputText.text = "";
        }
        else if (value == "-") {
            outputText.text = outputText.text.StartsWith("-") ? outputText.text.Remove(0, 1) : "-" + outputText.text;
        }
        else if (value == "." && !outputText.text.Contains("."))
        {
            var needsZero = outputText.text == "-" || outputText.text.Length == 0;
            outputText.text = outputText.text + (needsZero ? "0." : ".");
        }
        else
        {
            outputText.text += value;
        }
    }
}
