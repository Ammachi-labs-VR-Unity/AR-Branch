using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleControllerN : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public TMP_Text textToUpdate;
    public GameObject[] objectsToShow;
    public string[] toggleTexts = new string[9]; // Array to store text content for each toggle

    void Start()
    {
        // Get all toggles in the toggle group
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();

        // Add listeners to the onValueChanged events of all toggles
        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i; // Capture index in lambda
            toggles[i].onValueChanged.AddListener((isOn) => OnToggleValueChanged(index, isOn));
        }
    }

    void OnToggleValueChanged(int index, bool isOn)
    {
        if (isOn)
        {
            // Update the TMPro text content based on the specified text for the toggle
            if (index >= 0 && index < toggleTexts.Length)
            {
                textToUpdate.text = toggleTexts[index];
            }

            // Show the corresponding object and hide others
            for (int i = 0; i < objectsToShow.Length; i++)
            {
                objectsToShow[i].SetActive(i == index);
            }
        }
    }
}
