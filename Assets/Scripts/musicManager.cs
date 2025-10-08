using UnityEngine;

public class musicManager : MonoBehaviour
{
    [Header("Music Sources")]
    public float fadeSpeed = 2f;
    public AudioSource calmMusic;
    public AudioSource battleMusic;

    [Header("Base Volumes (per track)")]
    [Range(0f, 1f)] public float calmBaseVolume = 1f; // :)
    [Range(0f, 1f)] public float battleBaseVolume = 0.1f; // Kickstart my heart is loud!

    [Header("Settings")]
    [Range(0f, 1f)] public float globalMusicVolume = 1f; // global for player settings

    private AudioSource currentMusic;

    void Start(){
        calmMusic.Play();
        battleMusic.Play();
        calmMusic.volume = calmBaseVolume * globalMusicVolume;
        battleMusic.volume = 0;
        currentMusic = calmMusic;
    }

    public void beginPlay(string State){
        switch (State){
            case "Calm":
                CrossfadeTo(calmMusic);
                break;
            case "Battle":
                CrossfadeTo(battleMusic);
                break;
            default:
                Debug.LogWarning("Unknown music state: " + State);
                break;
        }
    }

    void CrossfadeTo(AudioSource target){
        if (target == currentMusic) return;

        StopAllCoroutines(); // stop any fades before fading
        StartCoroutine(FadeMusic(target));

        currentMusic = target;
    }

    System.Collections.IEnumerator FadeMusic(AudioSource target){
        float targetCalmVolume = (target == calmMusic) ? calmBaseVolume * globalMusicVolume : 0f;
        float targetBattleVolume = (target == battleMusic) ? battleBaseVolume * globalMusicVolume : 0f;

        while (true){
            calmMusic.volume = Mathf.MoveTowards(
                calmMusic.volume,
                targetCalmVolume,
                Time.deltaTime * fadeSpeed
            );

            battleMusic.volume = Mathf.MoveTowards(
                battleMusic.volume,
                targetBattleVolume,
                Time.deltaTime * fadeSpeed
            );

            // stop when both are close enough to targets
            if (Mathf.Approximately(calmMusic.volume, targetCalmVolume) &&
                Mathf.Approximately(battleMusic.volume, targetBattleVolume))
                yield break;

            yield return null;
        }
    }

    // call this when player changes volume in settings
    public void SetGlobalVolume(float volume){
        globalMusicVolume = Mathf.Clamp01(volume);
        // Update volumes instantly
        calmMusic.volume = calmBaseVolume * globalMusicVolume * (currentMusic == calmMusic ? 1f : 0f);
        battleMusic.volume = battleBaseVolume * globalMusicVolume * (currentMusic == battleMusic ? 1f : 0f);
    }
}
