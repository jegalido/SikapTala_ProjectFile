using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
   
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Gamepad First Selected")]
    [SerializeField] private GameObject mainMenuFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject creditsFirstSelected;

    

    [Header("Settings")]
    [SerializeField] private Toggle fullscreenToggle;
[SerializeField] private GameObject howToPlayFirstSelected;

    [Header("Blur")]
    [SerializeField] private Material screenBlurMaterial; // same M_ScreenBlur asset

    private static readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");

private void Start()
    {
        if (screenBlurMaterial != null)
            screenBlurMaterial.SetFloat(BlurSizeID, 0f);

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }

    private void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        // Circle = back, only if a sub-panel is open
        if (pad.buttonEast.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf) OnBackClicked(settingsPanel);
            else if (creditsPanel.activeSelf) OnBackClicked(creditsPanel);
           
            else if (howToPlayPanel.activeSelf) OnBackClicked(howToPlayPanel);
        }
    }

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked at time: " + Time.realtimeSinceStartup);
        SceneManager.LoadScene("MainGameIntroCutScene");
    }
public void OnSettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        EventSystem.current.SetSelectedGameObject(settingsFirstSelected);
    }

public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
    public void onCreditsClicked()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(creditsFirstSelected);
    }
    public void OnBackClicked(GameObject currentPanel)
    {
        Transform parent = currentPanel.transform.parent;
        currentPanel.SetActive(false);
        if (parent.name == "Main_Canvas")
        {
            mainMenuPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            return;
        }
        parent.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }
    /*public void OnContinueClicked()
    {
        mainMenuPanel.SetActive(false);
        continuePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(continueFirstSelected);
    } */
    public void OnHowToPlayClicked()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(howToPlayFirstSelected);
    }
}