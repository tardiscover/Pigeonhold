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
        PositionMusicButtons();
        PlayMusicWithIntro();
    }

    private void PositionMusicButtons()
    {
        //Ensure buttons are in the same place, even though one one will be visible at a time
        playMusicButton.gameObject.transform.position = stopMusicButton.gameObject.transform.position;
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
