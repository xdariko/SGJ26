using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endPanel;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    [Header("End Panel Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button endExitButton;

    private void Awake()
    {
        G.ui = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (endExitButton != null)
            endExitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnContinueClicked()
    {
        if (G.main != null)
            G.main.ResumeGame();
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        G.IsPaused = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void OnExitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SetPausePanel(bool active)
    {
        if (pausePanel != null)
            pausePanel.SetActive(active);
    }

    public void ShowEndPanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (endExitButton != null)
            endExitButton.onClick.RemoveListener(OnExitClicked);
    }
}