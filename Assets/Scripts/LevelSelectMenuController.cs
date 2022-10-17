using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// This script and the Level Select Menu Scene was created by studying, referencing and adapting the resources:
// MAIN MENU in Unity | All-In-One Tutorial: https://www.youtube.com/watch?v=Cq_Nnw_LwnI
// Level selection in your Unity game | Unity tutorial: https://www.youtube.com/watch?v=YAHFnF2MRsE
// Unity SceneManager Documentation: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html
// Various ways of disabling/removing buttons: https://www.w3schools.blog/how-to-disable-buttons-in-unity
// Unlock Levels - Levels Menu - Unity (PlayerPrefs) - Tutorial: https://www.youtube.com/watch?v=HhAEwhKnJJA (referenced the idea of a Button array)
// Unity GameObject.GetComponent Scripting API documentation: https://docs.unity3d.com/ScriptReference/GameObject.GetComponent.html
public class LevelSelectMenuController : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] GameObject[] levelButtons;
    int curLevelsUnlocked;

    private const int DEFAULT_LEVELS_UNLOCKED = 0;
    private const int ZERO_OFFSET = 1;

    [Header("Level Select Menu UI Sounds")]
    [SerializeField] AudioSource levelSelectMenuUIAudio;
    [SerializeField] AudioClip clickButtonSound;

    void Start() {
        // If a player has some progress on unlocked levels, fetch the "save data"
        // otherwise create new "save data" in the player preferences through the
        // constant default which is just 1.
        if(PlayerPrefs.HasKey("Unlocked Levels")) {
            curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");
        } else {
            PlayerPrefs.SetInt("Unlocked Levels", DEFAULT_LEVELS_UNLOCKED);
            curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");
        }

        // For debugging. Resets player level progression.
        // PlayerPrefs.SetInt("Unlocked Levels", DEFAULT_LEVELS_UNLOCKED);
        // curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");

        // Depending on the levels that the player has unlocked based on the "save data"
        // in the PlayerPrefs File, set the buttons to be interactable.
        for(int i = 0; i < curLevelsUnlocked; i++) {
            levelButtons[i].GetComponent<Button>().interactable = true;
        }
    }

    // Navigates to a certain level. Called by the level buttons on button press.
    public void GoToLevel(string levelToNavigateTo) {
        SceneManager.LoadScene(levelToNavigateTo);
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene("StartScene");
    }

    public void PlayClickButtonSound() {
        levelSelectMenuUIAudio.PlayOneShot(clickButtonSound);
    }
}
