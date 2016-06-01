using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditGui : MonoBehaviour {

    private Text display;

	void Start () {
        display = GetComponent<Text>();
	}

    // PUBLIC FUNCTIONS
    public void ChangeText(string newText) {
        display.text = newText;
    }

    public void AddText(string text) {
        display.text += text;
    }
}
