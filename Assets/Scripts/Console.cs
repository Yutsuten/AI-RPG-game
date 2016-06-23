using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour {

    EditGui consoleGui;
    System.String[] log = new System.String[4];

	void Start () {
        consoleGui = this.GetComponent<EditGui>();
        
        // initialize with empty messages
        for (int i = 0; i < log.Length; i++) {
            log[i] = "";
        }
	}

    public void AddMessage(System.String message) {
        // put older messages on the end of the array
        for (int i = log.Length - 1; i >= 1; i--) {
            log[i] = log[i-1];
        }
        // put the newer message on the first index
        log[0] = message;

        System.String logText = log[0] + System.Environment.NewLine +
            log[1] + System.Environment.NewLine +
            log[2] + System.Environment.NewLine +
            log[3] + System.Environment.NewLine;
        consoleGui.ChangeText(logText);
    }

    public void ClearMessages() {
        for (int i = 0; i < log.Length; i++)
            log[i] = System.String.Empty;
        consoleGui.ChangeText(System.String.Empty);
    }

}
