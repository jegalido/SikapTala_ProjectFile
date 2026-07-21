using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu controller with full keyboard + gamepad navigation.
/// Left button list (Resume / Tutorial / Options / Exit): Up-Down to move,
/// Right or Submit to enter a detail page. Inside a page: Up-Down move rows,
/// Left-Right adjust sliders / toggles, Back (Esc / Backspace / Circle) returns.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private enum Zone { List, Settings, Tutorial }
    private enum Nav { None, Up, Down, Left, Right }

    [Header("Root Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject buttonsContainer;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Left Menu Buttons (top -> bottom)")]
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject tutorialButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject exitButton;

    [Header("Settings Controls")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private GameObject settingsBackButton;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialBackButton;

    [Header("Navigation Tuning")]
    [SerializeField] private float sliderStep = 4f;
    [SerializeField] private float navRepeatDelay = 0.35f;
    [SerializeField] private float navRepeatRate = 0.12f;

    [Header("Blur")]
    [SerializeField] private Material screenBlurMaterial;
    [SerializeField] private float maxBlurSize = 3f;
    [SerializeField] private float blurFadeDuration = 0.25f;
    private static readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");

    private Zone zone = Zone.List;
    private GameObject[] menuItems;
    private List<Selectable> settingsItems;
    private List<Selectable> tutorialItems;
    private int listIndex;
    private int detailIndex;

    private Nav activeDir = Nav.None;
    private float nextRepeatTime;
    private float navLockUntil;

    private Coroutine blurRoutine;

    private void Awake()
    {
        menuItems = new GameObject[] { resumeButton, tutorialButton, optionsButton, exitButton };

        settingsItems = new List<Selectable>();
        AddSel(settingsItems, masterSlider);
        AddSel(settingsItems, musicSlider);
        AddSel(settingsItems, sfxSlider);
        AddSel(settingsItems, fullscreenToggle);
        AddSel(settingsItems, settingsBackButton);

        tutorialItems = new List<Selectable>();
        AddSel(tutorialItems, tutorialBackButton);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private static void AddSel(List<Selectable> list, Component c)
    {
        if (c == null) return;
        var s = c.GetComponent<Selectable>();
        if (s != null) list.Add(s);
    }
    private static void AddSel(List<Selectable> list, GameObject g)
    {
        if (g == null) return;
        var s = g.GetComponent<Selectable>();
        if (s != null) list.Add(s);
    }


    private void Update()
    {
        Gamepad pad = Gamepad.current;
        Keyboard kb = Keyboard.current;

        bool startPressed = pad != null && pad.startButton.wasPressedThisFrame;
        bool escPressed = kb != null && kb.escapeKey.wasPressedThisFrame;
        bool cancelPressed = escPressed
            || (kb != null && kb.backspaceKey.wasPressedThisFrame)
            || (pad != null && pad.buttonEast.wasPressedThisFrame);

        if (startPressed)
        {
            TogglePause();
            return;
        }

        if (!IsPaused)
        {
            if (escPressed) TogglePause();
            return;
        }

        // Paused from here on.
        if (cancelPressed)
        {
            if (zone == Zone.List) TogglePause();
            else ExitToList();
            return;
        }

        HandleNavigation();
    }

    private void HandleNavigation()
    {
        Nav dir = ReadDirection();

        if (Time.unscaledTime < navLockUntil)
        {
            activeDir = dir;
            nextRepeatTime = navLockUntil + navRepeatDelay;
            return;
        }

        if (dir == Nav.None)
        {
            activeDir = Nav.None;
            return;
        }

        bool fire = false;
        if (dir != activeDir)
        {
            activeDir = dir;
            nextRepeatTime = Time.unscaledTime + navRepeatDelay;
            fire = true;
        }
        else if (Time.unscaledTime >= nextRepeatTime)
        {
            nextRepeatTime = Time.unscaledTime + navRepeatRate;
            fire = true;
        }

        if (!fire) return;

        switch (zone)
        {
            case Zone.List: NavigateList(dir); break;
            case Zone.Settings: NavigateDetail(settingsItems, dir, true); break;
            case Zone.Tutorial: NavigateDetail(tutorialItems, dir, false); break;
        }
    }

    private Nav ReadDirection()
    {
        float x = 0f, y = 0f;
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) y += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) y -= 1f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) x += 1f;
        }
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 d = pad.dpad.ReadValue();
            Vector2 s = pad.leftStick.ReadValue();
            if (d.y > 0.5f || s.y > 0.5f) y += 1f;
            if (d.y < -0.5f || s.y < -0.5f) y -= 1f;
            if (d.x < -0.5f || s.x < -0.5f) x -= 1f;
            if (d.x > 0.5f || s.x > 0.5f) x += 1f;
        }

        if (Mathf.Abs(x) < 0.5f && Mathf.Abs(y) < 0.5f) return Nav.None;
        if (Mathf.Abs(x) >= Mathf.Abs(y))
            return x > 0f ? Nav.Right : Nav.Left;
        return y > 0f ? Nav.Up : Nav.Down;
    }

    private void NavigateList(Nav dir)
    {
        if (dir == Nav.Up)
        {
            listIndex = (listIndex - 1 + menuItems.Length) % menuItems.Length;
            SelectList();
        }
        else if (dir == Nav.Down)
        {
            listIndex = (listIndex + 1) % menuItems.Length;
            SelectList();
        }
        else if (dir == Nav.Right)
        {
            GameObject cur = menuItems[listIndex];
            if (cur == optionsButton) EnterSettings();
            else if (cur == tutorialButton) EnterTutorial();
        }
    }

    private void SelectList()
    {
        if (menuItems[listIndex] == null) return;
        SetSelected(menuItems[listIndex]);
    }

private void NavigateDetail(List<Selectable> items, Nav dir, bool allowAdjust)
    {
        if (items == null || items.Count == 0) return;

        if (dir == Nav.Up)
        {
            detailIndex = (detailIndex - 1 + items.Count) % items.Count;
            SetSelected(items[detailIndex].gameObject);
        }
        else if (dir == Nav.Down)
        {
            detailIndex = (detailIndex + 1) % items.Count;
            SetSelected(items[detailIndex].gameObject);
        }
        else if (allowAdjust && (dir == Nav.Left || dir == Nav.Right))
        {
            Adjust(items[detailIndex].gameObject, dir == Nav.Right ? 1 : -1);
        }
    }

    private void Adjust(GameObject go, int sign)
    {
        Slider sl = go.GetComponent<Slider>();
        if (sl != null)
        {
            sl.value = Mathf.Clamp(sl.value + sliderStep * sign, sl.minValue, sl.maxValue);
            return;
        }
        Toggle tg = go.GetComponent<Toggle>();
        if (tg != null)
            tg.isOn = sign > 0;
    }

    // ---------------- Pause lifecycle ----------------
    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (buttonsContainer != null) buttonsContainer.SetActive(true);
        zone = Zone.List;
        listIndex = 0;
        SelectList();
        FadeBlur(maxBlurSize);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        zone = Zone.List;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        FadeBlur(0f);
    }

    // ------------- Zone transitions (also Button onClick targets) -------------
public void EnterSettings()
    {
        if (settingsPanel == null) return;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        settingsPanel.SetActive(true);
        zone = Zone.Settings;
        detailIndex = 0;
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        LockNav();
        if (settingsItems.Count > 0) SetSelected(settingsItems[0].gameObject);
    }

public void EnterTutorial()
    {
        if (tutorialPanel == null) return;
        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        tutorialPanel.SetActive(true);
        zone = Zone.Tutorial;
        detailIndex = 0;
        LockNav();
        if (tutorialItems.Count > 0) SetSelected(tutorialItems[0].gameObject);
    }

    public void ExitToList()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (buttonsContainer != null) buttonsContainer.SetActive(true);
        zone = Zone.List;
        LockNav();
        SelectList();
    }

    private void LockNav()
    {
        navLockUntil = Time.unscaledTime + 0.2f;
        activeDir = ReadDirection();
    }

    private void SetSelected(GameObject go)
    {
        if (EventSystem.current == null || go == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    // ---------------- Main menu ----------------
    public void onMainMenuClicked()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (screenBlurMaterial != null) screenBlurMaterial.SetFloat(BlurSizeID, 0f);
        SceneManager.LoadScene("MainMenu");
    }

    // Backwards-compatible aliases for any UI still pointing at old method names.
    public void onSettingsClicked() { EnterSettings(); }
    public void OnBackClicked(GameObject _) { ExitToList(); }

    // ---------------- Blur ----------------
    private void FadeBlur(float target)
    {
        if (screenBlurMaterial == null) return;
        if (blurRoutine != null) StopCoroutine(blurRoutine);
        blurRoutine = StartCoroutine(FadeBlurRoutine(target));
    }

    private IEnumerator FadeBlurRoutine(float target)
    {
        float start = screenBlurMaterial.GetFloat(BlurSizeID);
        float t = 0f;
        while (t < blurFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            screenBlurMaterial.SetFloat(BlurSizeID, Mathf.Lerp(start, target, t / blurFadeDuration));
            yield return null;
        }
        screenBlurMaterial.SetFloat(BlurSizeID, target);
    }

    private void OnDisable()
    {
        // Safety: never leave the game frozen if this object is disabled while paused.
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }
}
