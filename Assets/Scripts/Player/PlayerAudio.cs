using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip jumpSound;

    [Header("Skill Sounds")]
    [SerializeField] private AudioClip skillQSound;
    [SerializeField] private AudioClip skillESound;

    [Header("Settings")]
    [SerializeField] private float walkVolume = 0.6f;
    [SerializeField] private float runVolume = 0.8f;
    [SerializeField] private float jumpVolume = 1f;
    [SerializeField] private float skillVolume = 1f;

    private AudioClip currentMoveSound;

    // =========================
    // WALK
    // =========================
    public void PlayWalk()
    {
        PlayLoopMovement(walkSound, walkVolume);
    }

    // =========================
    // RUN
    // =========================
    public void PlayRun()
    {
        PlayLoopMovement(runSound, runVolume);
    }

    // =========================
    // STOP MOVEMENT SOUND
    // =========================
    public void StopMovement()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Stop();

        currentMoveSound = null;
    }

    // =========================
    // JUMP
    // =========================
    public void PlayJump()
    {
        PlayOneShot(jumpSound, jumpVolume);
    }

    // =========================
    // SKILL Q
    // =========================
    public void PlaySkillQ()
    {
        PlayOneShot(skillQSound, skillVolume);
    }

    // =========================
    // SKILL E
    // =========================
    public void PlaySkillE()
    {
        PlayOneShot(skillESound, skillVolume);
    }

    private void PlayLoopMovement(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
            return;

        // Nếu đang phát đúng âm thanh thì không phát lại
        if (currentMoveSound == clip && audioSource.isPlaying)
            return;

        currentMoveSound = clip;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}