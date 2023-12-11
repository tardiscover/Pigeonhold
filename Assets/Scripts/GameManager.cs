
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
    private GameObject player;

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
            UIManager.Instance?.SetUiForGameState(_gameState);
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
