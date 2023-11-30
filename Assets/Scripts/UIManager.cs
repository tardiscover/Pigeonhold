using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
#if (UNITY_WEBGL)
using UnityEngine.SceneManagement;
#endif

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;

    private void Awake()
    {
        gameCanvas = FindObjectOfType<Canvas>();
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
}
