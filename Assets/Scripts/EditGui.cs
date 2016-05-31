using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditGui : MonoBehaviour {

    private Text display;

	// Use this for initialization
	void Start () {
        display = GetComponent<Text>();
	}

    public void ChangeText(string newText) {
        display.text = newText;
    }
}
