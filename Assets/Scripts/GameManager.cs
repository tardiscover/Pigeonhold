
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Note that this is a SingletonMonoBehavior.
/// </summary>
public class GameManager : SingletonMonoBehavior<GameManager>
{
    [SerializeField] int foesLeftMax;
    [SerializeField] int foesLeftCurrent;
    public FoesLeftBar foesLeftBar;
    public StatusTextbox statusTextBox;
    public Sprite winImageIcon;
    public Sprite loseImageIcon;
    public Button restartButton;
    private GameObject player;

    public enum GameStateType
    {
        Playing,
        GameOverWon,
        GameOverLost
    }

    private GameStateType _gameState;
    public GameStateType GameState
    {
        get
        {
            return _gameState;
        }
        set
        {
            _gameState = value;
            switch (_gameState)
            {
                case GameStateType.GameOverWon:
                    statusTextBox.largeText.text = "Success!";
                    statusTextBox.iconImage.sprite = winImageIcon;
                    statusTextBox.gameObject.SetActive(true);
                    restartButton.gameObject.SetActive(true);
                    break;

                case GameStateType.GameOverLost:
                    statusTextBox.largeText.text = "R.I.P.!";
                    statusTextBox.iconImage.sprite = loseImageIcon;
                    statusTextBox.gameObject.SetActive(true);
                    restartButton.gameObject.SetActive(true);
                    break;

                default:    //GameStateType.Playing
                    statusTextBox.gameObject.SetActive(false);
                    restartButton.gameObject.SetActive(false);
                    break;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();   //Remember this!

        //things to do on Awake in the child, if anything...
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        GameState = GameStateType.Playing;
    }

    public void IncrementFoesLeftMax()
    {
        foesLeftMax++;
        foesLeftCurrent++;
        foesLeftBar.SetFoesLeft(foesLeftCurrent, foesLeftMax);
    }

    public void DecrementFoesLeftMax()
    {
        foesLeftCurrent--;
        foesLeftBar.SetFoesLeft(foesLeftCurrent, foesLeftMax);

        if (foesLeftCurrent <= 0)
        {
            Win();
        }
    }

    private void Win()
    {
        GameState = GameStateType.GameOverWon;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log($"{this.name} : {this.GetType()} : {System.Reflection.MethodBase.GetCurrentMethod().Name}");
#endif

#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (Unity_STANDALONE)
        Application.Quit();
#elif (UNITY_WEBGL)
        SceneManager.LoadScene("QuitScene");
#endif
    }
}
