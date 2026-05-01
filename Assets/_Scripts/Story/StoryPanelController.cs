using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StoryPanelController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StoryPanelView view;
    [SerializeField] private TextTypingAudio typingAudio;
    [SerializeField] private StoryDatabase database;

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 35f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Input")]
    [SerializeField] private bool allowMouseClick = true;

    [Header("View")]
    [SerializeField] private bool firstSequenceStartsVisible = true;

    public bool IsPlaying => isPlaying;

    private StorySequenceData currentSequence;
    private int currentLineIndex;
    private Coroutine playRoutine;
    private bool isPlaying;
    private bool isLineFullyShown;
    private bool skipRequested;
    private bool nextRequested;
    private bool closeRequested;
    private bool hasPlayedAnySequence;
    private Action onSequenceFinished;

    private void Awake()
    {
        G.storyPanel = this;

        if (view != null)
        {
            if (firstSequenceStartsVisible)
            {
                view.gameObject.SetActive(true);
                view.SetInstantVisible();
            }
            else
            {
                view.SetInstantHidden();
                view.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (WasContinuePressed())
        {
            if (!isLineFullyShown)
                skipRequested = true;
            else
                nextRequested = true;
        }
    }

    public void PlaySequence(string sequenceId, Action onFinished = null)
    {
        if (database == null)
        {
            Debug.LogWarning("StoryDatabase is not assigned.", this);
            return;
        }

        StorySequenceData sequence = database.GetById(sequenceId);
        if (sequence == null)
        {
            Debug.LogWarning($"Story sequence not found by id: {sequenceId}", this);
            return;
        }

        PlaySequence(sequence, onFinished);
    }

    public void PlaySequence(StorySequenceData sequence, Action onFinished = null)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0)
            return;

        StopCurrentSequence(false);

        currentSequence = sequence;
        currentLineIndex = 0;
        onSequenceFinished = onFinished;
        playRoutine = StartCoroutine(PlaySequenceRoutine());
    }

    public void StopCurrentSequence(bool invokeCallback = false)
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        isPlaying = false;
        isLineFullyShown = false;
        skipRequested = false;
        nextRequested = false;
        closeRequested = false;

        currentSequence = null;
        currentLineIndex = 0;

        G.IsPaused = false;

        if (view != null)
            view.SetInstantHidden();

        if (invokeCallback)
        {
            Action callback = onSequenceFinished;
            onSequenceFinished = null;
            callback?.Invoke();
        }
        else
        {
            onSequenceFinished = null;
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        isPlaying = true;
        G.IsPaused = true;

        if (view != null)
            view.gameObject.SetActive(true);

        skipRequested = false;
        nextRequested = false;
        closeRequested = false;

        bool skipShowAnimation = firstSequenceStartsVisible && !hasPlayedAnySequence;

        if (view != null)
        {
            if (skipShowAnimation)
                view.SetInstantVisible();
            else
                yield return StartCoroutine(view.ShowRoutine());
        }

        while (currentSequence != null && currentLineIndex < currentSequence.lines.Length)
        {
            if (closeRequested)
            {
                StopCurrentSequence(false);
                yield break;
            }

            StoryLine line = currentSequence.lines[currentLineIndex];

            yield return StartCoroutine(PlaySingleLine(line));

            if (closeRequested)
            {
                StopCurrentSequence(false);
                yield break;
            }

            yield return StartCoroutine(WaitForContinue());

            if (closeRequested)
            {
                StopCurrentSequence(false);
                yield break;
            }

            currentLineIndex++;
        }

        hasPlayedAnySequence = true;
        isPlaying = false;
        G.IsPaused = false;

        if (view != null)
        {
            yield return StartCoroutine(view.HideRoutine());
            view.gameObject.SetActive(false);
        }

        Action callback = onSequenceFinished;
        onSequenceFinished = null;
        callback?.Invoke();
    }

    private IEnumerator PlaySingleLine(StoryLine line)
    {
        skipRequested = false;
        nextRequested = false;
        isLineFullyShown = false;

        if (view != null)
        {
            view.ClearBody();
            view.SetImage(line != null ? line.image : null);
        }

        string fullText = line != null ? line.text ?? string.Empty : string.Empty;

        if (string.IsNullOrEmpty(fullText))
        {
            isLineFullyShown = true;
            yield break;
        }

        float interval = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (closeRequested)
                yield break;

            if (skipRequested)
            {
                if (view != null)
                    view.SetBody(fullText);

                isLineFullyShown = true;
                yield break;
            }

            if (view != null)
                view.SetBody(fullText.Substring(0, i + 1));

            if (typingAudio != null)
                typingAudio.PlayChar(fullText[i]);

            if (interval > 0f)
            {
                if (useUnscaledTime)
                    yield return WaitUnscaled(interval);
                else
                    yield return new WaitForSeconds(interval);
            }
        }

        isLineFullyShown = true;
    }

    private IEnumerator WaitForContinue()
    {
        nextRequested = false;

        while (true)
        {
            if (closeRequested)
                yield break;

            if (nextRequested)
                yield break;

            yield return null;
        }
    }

    private bool WasContinuePressed()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;

        if (allowMouseClick && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        return false;
    }

    private bool WasClosePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private IEnumerator WaitUnscaled(float seconds)
    {
        float end = Time.unscaledTime + seconds;

        while (Time.unscaledTime < end)
        {
            if (closeRequested || skipRequested)
                yield break;

            yield return null;
        }
    }
}