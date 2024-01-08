using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class EffectsController : MonoBehaviour
{
    public GameController mGameController;
    public PostProcessVolume mPostProcessVolume;
    public AudioController mAudioController;
    public Transform mGameBoardTransform;
    public Button mToggleShadowButton;
    public TextMeshProUGUI mUIOverlayText;

    private static string DESTROYING_BLOCK_PATH = "Sprites/DestroyingBlock";
    private static GameObject mDestryoingBlock;

    public float ENTRANCE_EFFECTS_PLAY_TIME = 2.5f;
    public float ENTRANCE_EFFECTS_BLOOM_INTENSITY = 50f;
    public float LEVEL_UP_PULSE_MULTIPLIER = 32f;
    public float LEVEL_UP_BLOOM_INTENSITY = 15f;
    public float INSTRUCTIONS_FADE_TIME = 5f;
    public float GAME_OVER_FADE_TIME = 3f;
    public float LEVEL_UP_PLAY_TIME = .75f;

    // Desired duration of the shake effect
    private float SHAKE_DURATION = 2f;
    private float mRunningShakeDuration = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    public float SHAKE_MAGNITUDE = 50;

    // A measure of how quickly the shake effect should evaporate
    private float SHAKE_DAMPING_SPEED = 1.0f;

    // The initial position of the GameObject
    Vector3 mShakeInitialPosition;

    private Bloom mBloom;
    private Grain mGrain;
    private ColorGrading mColorGrading;
    private static float MIN_HUE_SHIFT = 60f;

    private float mLevelUpPlayTimeRemaining;
    private bool mPlayLevelUpEffects = false;

    private float mEntranceEffectsTimeRemaining;

    private float mInstructionsTimeRemaining;
    private bool mInstructionsProcessed = false;

    private string UI_OVERLAY_TEXT_GAME_OVER = "Game Over!\r\n\r\nPress R to restart";
    private bool mGameOverProcessed = false;
    private float mGameOverTimeTimeRemaining;

    // Start is called before the first frame update
    void Start()
    {
        mInstructionsTimeRemaining = INSTRUCTIONS_FADE_TIME;
        mShakeInitialPosition = mGameBoardTransform.localPosition;
        mLevelUpPlayTimeRemaining = LEVEL_UP_PLAY_TIME;
        mGameOverTimeTimeRemaining = GAME_OVER_FADE_TIME;
        mEntranceEffectsTimeRemaining = ENTRANCE_EFFECTS_PLAY_TIME;

        mDestryoingBlock = Resources.Load(DESTROYING_BLOCK_PATH) as GameObject;

        mPostProcessVolume.profile.TryGetSettings(out mBloom);
        mBloom.enabled.Override(false);
        mPostProcessVolume.profile.TryGetSettings(out mGrain);
        mGrain.enabled.Override(false);
        mPostProcessVolume.profile.TryGetSettings(out mColorGrading);
        mColorGrading.enabled.Override(false);

        PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, mBloom);
        PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, mGrain);
        PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, mColorGrading);
    }

    // Update is called once per frame
    void Update()
    {
        processInstructionEffects();

        processEntranceBloomEffects();

        processLevelUpEffects();

        processShakeEffects();

        processGameOverEffects();
    }

    public void processInstructionEffects()
    {
        if (mGameController.getIsGameOver())
        {
            return;
        }

        if (mInstructionsProcessed || mEntranceEffectsTimeRemaining > 0)
        {
            return;
        }

        if (mInstructionsTimeRemaining > 0)
        {
            mInstructionsTimeRemaining -= Time.deltaTime;

            if (mInstructionsTimeRemaining < 0)
            {
                mInstructionsTimeRemaining = 0;
            }

            float desiredAlphaMultiplier = (mInstructionsTimeRemaining / INSTRUCTIONS_FADE_TIME);
            Color32 desiredColor = mUIOverlayText.color;
            int desiredAlpha = (int)Mathf.Floor(255 * desiredAlphaMultiplier);
            desiredColor = new Color32(desiredColor.r, desiredColor.g, desiredColor.b, (byte)desiredAlpha);

            mUIOverlayText.color = desiredColor;
        }
        else
        {
            mUIOverlayText.enabled = false;
            Color32 desiredColor = mUIOverlayText.color;
            mUIOverlayText.color = new Color32(desiredColor.r, desiredColor.g, desiredColor.b, 255);
            mInstructionsProcessed = true;
            mUIOverlayText.text = UI_OVERLAY_TEXT_GAME_OVER;
        }
    }

    private void processEntranceBloomEffects()
    {
        if (mEntranceEffectsTimeRemaining > 0)
        {
            mEntranceEffectsTimeRemaining -= Time.deltaTime;

            if (mEntranceEffectsTimeRemaining < 0)
            {
                mEntranceEffectsTimeRemaining = 0;
            }

            mBloom.enabled.Override(true);
            float effectsIntensity = (mEntranceEffectsTimeRemaining / ENTRANCE_EFFECTS_PLAY_TIME);
            mBloom.intensity.value = ENTRANCE_EFFECTS_BLOOM_INTENSITY * effectsIntensity;

            mGrain.enabled.Override(true);
            mGrain.intensity.value = effectsIntensity;
        }
        else
        {
            mBloom.intensity.value = 0;
            mBloom.enabled.Override(false);
        }
    }

    private void processLevelUpEffects()
    {
        if (mPlayLevelUpEffects)
        {
            float intensity = Mathf.Sin(Time.realtimeSinceStartup * LEVEL_UP_PULSE_MULTIPLIER);
            intensity = Mathf.Abs(intensity * LEVEL_UP_BLOOM_INTENSITY);
            mBloom.intensity.value = intensity;
            mBloom.enabled.Override(true);

            if (mLevelUpPlayTimeRemaining > 0)
            {
                mLevelUpPlayTimeRemaining -= Time.deltaTime;
            }
            else
            {
                mPlayLevelUpEffects = false;
                mLevelUpPlayTimeRemaining = LEVEL_UP_PLAY_TIME;
                mBloom.enabled.Override(false);
            }
        }
    }

    public void triggerLevelUpEffects()
    {
        mPlayLevelUpEffects = true;
        mAudioController.playLevelUp();
        mColorGrading.enabled.Override(true);

        float previousHue = mColorGrading.hueShift.value;
        float randomHue = Random.Range(-180, 180);

        // Ensure some minimum shift in hue so we aren't too close to our current colors.
        while ((Mathf.Abs(randomHue - previousHue)) < MIN_HUE_SHIFT)
        {
            randomHue = Random.Range(-180, 180);
        }

        mColorGrading.hueShift.value = Random.Range(-180, 180);
        triggerShakeEffects();
    }

    private void processShakeEffects()
    {
        if (mRunningShakeDuration > 0)
        {
            float zPos = mGameBoardTransform.localPosition.z;
            Vector3 desiredPos = mShakeInitialPosition + Random.insideUnitSphere * SHAKE_MAGNITUDE;
            mGameBoardTransform.localPosition = new Vector3(desiredPos.x, desiredPos.y, zPos);

            mRunningShakeDuration -= Time.deltaTime * SHAKE_DAMPING_SPEED;
        }
        else
        {
            mRunningShakeDuration = 0f;
            mGameBoardTransform.localPosition = mShakeInitialPosition;
        }
    }

    public void triggerShakeEffects()
    {
        mRunningShakeDuration = SHAKE_DURATION;
    }

    public void processGameOverEffects()
    {
        if (!mGameController.getIsGameOver())
        {
            return;
        }

        mToggleShadowButton.interactable = false;

        if (mGameOverProcessed)
        {
            return;
        }

        if (mGameOverTimeTimeRemaining > 0)
        {
            mGameOverTimeTimeRemaining -= Time.deltaTime;

            if (mGameOverTimeTimeRemaining < 0)
            {
                mGameOverTimeTimeRemaining = 0;
            }

            mUIOverlayText.enabled = true;
            mColorGrading.enabled.Override(true);
            float saturationAmount = 1 - (mGameOverTimeTimeRemaining / GAME_OVER_FADE_TIME);
            mColorGrading.saturation.value = -100f * saturationAmount;
        }
        else
        {
            mGameOverProcessed = true;
        }
    }

    public float getDestroyingBlockAnimationLength()
    {
        if (mDestryoingBlock == null)
        {
            mDestryoingBlock = Resources.Load(DESTROYING_BLOCK_PATH) as GameObject;
        }

        Animator animator = mDestryoingBlock.GetComponent<Animator>();

        if (animator == null)
        {
            return -1;
        }

        AnimationClip[] clipInfo = animator.runtimeAnimatorController.animationClips;

        if (clipInfo == null || clipInfo.Length == 0)
        {
            return -1;
        }

        return clipInfo[0].length;
    }

    public void restart()
    {
        mEntranceEffectsTimeRemaining = ENTRANCE_EFFECTS_PLAY_TIME;
        mLevelUpPlayTimeRemaining = LEVEL_UP_PLAY_TIME;
        mUIOverlayText.enabled = false;
        mGameOverProcessed = false;
        mGameOverTimeTimeRemaining = GAME_OVER_FADE_TIME;
        mBloom.enabled.Override(false);
        mColorGrading.saturation.value = 0;
        mColorGrading.enabled.Override(false);
        mToggleShadowButton.interactable = true;
    }
}
