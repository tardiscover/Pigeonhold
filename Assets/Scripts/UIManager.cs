using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Note: This script should be on the GameCanvas.
/// 
/// https://www.youtube.com/watch?v=JivuXdrIHK0 and https://www.youtube.com/watch?v=UGh4exLCFRg were combined and modified for this approach.
/// </summary>
public class UIManager : SingletonMonoBehavior<UIManager>
{
    public static bool gameIsPaused = false;    //!!! Move to GameManager?

    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;
    public GameObject pausedMenu;
    public GameObject playingMenu;

    public StatusTextbox statusTextBox;
    public Sprite winImageIcon;
    public Sprite loseImageIcon;
    public Button playGameButton;
    public Button restartButton;

    private Canvas gameCanvas;
    private PlayerInput[] playerInputs;

    protected override void Awake()
    {
        base.Awake();   //Remember this!

        gameCanvas = GetComponent<Canvas>();
        playerInputs = FindObjectsOfType<PlayerInput>();    //Currently only one player, but if ever more than one maps need switching for all
    }

    private void Start()
    {
        Resume();   //Ensure initialized so game playing, not paused.
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += CharacterHealed;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= CharacterTookDamage;
        CharacterEvents.characterHealed -= CharacterHealed;
    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        //Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = "-" + damageReceived.ToString();
    }

    public void CharacterHealed(GameObject character, int healthRestored)
    {
        //Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = "+" + healthRestored.ToString();
    }

    /// <summary>
    /// RestartGame called from RestartButton
    /// </summary>
    public void OnRestartButtonClick()
    {
        GameManager.Instance.RestartGame();
    }

    /// <summary>
    /// PauseMenuOpen called from Input (keyboard, gamepad, etc.)
    /// Hitting Esc on keyboard or Start button on gamepad for example.
    /// </summary>
    /// <param name="context"></param>
    public void OnPauseMenuOpen(InputAction.CallbackContext context)
    {
        if (!gameIsPaused)
        {
            Pause();

            //!!!GameManager.Instance.GameState = GameStateType.PausedMidGame;
        }
    }

    /// <summary>
    /// PauseMenuOpen called from Input (keyboard, gamepad, etc.)
    /// Hitting Esc on keyboard or Start button on gamepad for example.
    /// </summary>
    /// <param name="context"></param>
    public void OnPauseGameButtonClick()
    {
        if (!gameIsPaused)
        {
            Pause();

            //!!!GameManager.Instance.GameState = GameStateType.PausedMidGame;
        }
    }

    /// <summary>
    /// PauseMenuOpen called from Input (keyboard, gamepad, etc.)
    /// Hitting Esc on keyboard or Start button on gamepad for example.
    /// </summary>
    /// <param name="context"></param>
    public void OnPauseMenuClose(InputAction.CallbackContext context)
    {
        if (gameIsPaused)
        {
            Resume();

            //!!!GameManager.Instance.GameState = GameStateType.Playing;
        }
    }

    /// <summary>
    /// ExitGame called from ExitGameButton.
    /// </summary>
    public void OnPlayGameButtonClick()
    {
        if (gameIsPaused)
        {
            Resume();

            //!!!GameManager.Instance.GameState = GameStateType.Playing;
        }
    }

    /// <summary>
    /// ExitGame called from Input (keyboard, gamepad, etc.)
    /// Hitting X on keyboard for example.  //!!! Do we even want this?
    /// </summary>
    /// <param name="context"></param>
    public void OnExitGame(InputAction.CallbackContext context)
    {
        GameManager.Instance.ExitGame();
    }

    /// <summary>
    /// ExitGame called from ExitGameButton.
    /// </summary>
    public void OnExitGameButtonClick()
    {
        GameManager.Instance.ExitGame();
    }

    public void Pause()
    {
        if (GameManager.Instance.GameState == GameStateType.Playing)
        {
            GameManager.Instance.GameState = GameStateType.PausedMidGame;
        }

        pausedMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;

        foreach (PlayerInput playerInput in playerInputs)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
    }

    public void Resume()
    {
        GameManager.Instance.GameState = GameStateType.Playing;

        pausedMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;

        foreach (PlayerInput playerInput in playerInputs)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    public void SetUiForGameState(GameStateType gameState)
    {
        switch (gameState)
        {
            case GameStateType.GameOverWon:
                statusTextBox.smallText.text = "GAME OVER";
                statusTextBox.largeText.text = "Success!";
                statusTextBox.iconImage.sprite = winImageIcon;
                statusTextBox.iconImage.enabled = true;
                playGameButton.gameObject.SetActive(false);
                restartButton.gameObject.SetActive(true);
                SetPausedMenuVisible(true);
                break;

            case GameStateType.GameOverLost:
                statusTextBox.smallText.text = "GAME OVER";
                statusTextBox.largeText.text = "R.I.P.!";
                statusTextBox.iconImage.sprite = loseImageIcon;
                statusTextBox.iconImage.enabled = true;
                playGameButton.gameObject.SetActive(false);
                restartButton.gameObject.SetActive(true);
                SetPausedMenuVisible(true);
                break;

            case GameStateType.PausedMidGame:
                statusTextBox.smallText.text = "GAME";
                statusTextBox.largeText.text = "Paused";
                statusTextBox.iconImage.enabled = false;
                playGameButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(false);
                SetPausedMenuVisible(true);
                break;

            default:    //GameStateType.Playing
                //playGameButton.gameObject.SetActive(false);
                //restartButton.gameObject.SetActive(false);
                SetPausedMenuVisible(false);
                break;
        }
    }

    private void SetPausedMenuVisible(bool value)
    {
        pausedMenu.SetActive(value);
        playingMenu.SetActive(!value);
    }
}
