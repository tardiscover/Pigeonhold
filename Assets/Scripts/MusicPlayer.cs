using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;
    public Button stopMusicButton;
    public Button playMusicButton;

    // Start is called before the first frame update
    void Start()
    {
        PlayMusicWithIntro();
    }

    public void PlayMusicWithIntro()
    {
        introSource.Play();
        loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
        stopMusicButton.gameObject.SetActive(true);
        playMusicButton.gameObject.SetActive(false);
    }

    public void PlayMusic()
    {
        loopSource.Play();
        stopMusicButton.gameObject.SetActive(true);
        playMusicButton.gameObject.SetActive(false);
    }

    public void StopMusic()
    {
        introSource.Stop();
        loopSource.Stop();
        playMusicButton.gameObject.SetActive(true);
        stopMusicButton.gameObject.SetActive(false);
    }
}
