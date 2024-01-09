using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Tetrino;


public class GameController : MonoBehaviour
{
    public GameObject mBackEnd;
    public GameObject mGameboardCanvas;
    public GameBoard mGameBoardModel;
    public TextMeshProUGUI mGameOverTextTMP;
    public TextMeshProUGUI mLevelTextTMP;
    public TextMeshProUGUI mHighScoreTMP;
    public TextMeshProUGUI mCurrentScoreTMP;
    public AudioController mAudioController;
    public TetrinoFactory mTetrinoFactory;
    public PostProcessVolume mPostProcessVolumes;
    public EffectsController mEffectsController;
    public GameObject mPauseMenu;

    public static string GLOBAL_POST_NAME = "GlobalPost";

    // Basic timer for when the Tetrinos move down
    public float mFetrinoFallTime = .74f;

    // Modifier to speed up the fall time with each level increase
    // The basic idea is to tweak these numbers so the maximum fall time is reached at level 10
    public float mFallTimerLevelModifier = .8f;

    // How many lines cleared it takes to increase the level
    public int mLineToLevelIncrementor = 5;

    // Used in the Tetrino falling workflow
    private float mRunningTetrinoFallTime = 1;
    private float mTetrinoFallTimeRemaining;
    private float mRunningTimerMultiplier = 1;
    private float mMinFallTimer = .05f;

    // Used in the devMode workflow
    private float mDevModeToggleCooldown = 1;
    private float mDevModeCooldownTimeRemaining = 1;

    // Used in the smooth movement of Tetrinos
    private float mKeyDownTime = .1f;
    private float mKeyDownTimeDelayMultiplier = 3;
    private float mKeyDownTimeRemaining = .1f;

    // The current active Tetrino the player is controlling
    private Tetrino mActiveTetrino;
    private Bloom mBloom;
    private int mCurrentLevel = 1;
    private int mBaseScoreIncrement = 100;
    private int mHighScore = 0;
    private int mCurrentScore = 0;
    private bool mIsGameOver = false;
    private bool mGameOverProcessed = false;
    private bool mPaused = false;

    private string HIGH_SCORE_PREF_NAME = "HighScore";
    private static string LEVEL_TEXT_HEADER = "LEVEL - ";

    // Whether or not we can do devMode special commands
    public bool mDevMode = false;

    private void Start()
    {
        loadHighScore();

        mTetrinoFallTimeRemaining = mFetrinoFallTime;
        mRunningTetrinoFallTime = mFetrinoFallTime;
        mRunningTimerMultiplier = mFallTimerLevelModifier;
        mAudioController.playStartup();

        mPostProcessVolumes.profile.TryGetSettings(out mBloom);
        mBloom.enabled.Override(false);
        mBloom.intensity.value = 5f;
        PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, mBloom);
    }

    private void loadHighScore()
    {
        mHighScoreTMP.text = PlayerPrefs.GetInt(HIGH_SCORE_PREF_NAME, 0).ToString();
    }

    private void spawnRandomTetrino()
    {
        if (mPaused)
        {
            return;
        }

        mActiveTetrino = mGameBoardModel.spawnRandomTetrino();
    }

    // Spawns a specific Tetrino
    private void spawnTetrino(TetrinoType tetrinoType)
    {
        if (mPaused)
        {
            return;
        }

        mActiveTetrino = mGameBoardModel.spawnTetrino(tetrinoType, null);
        mAudioController.playDropSound();
    }

    // Deswpanws a tetrino
    private void despawnTetrino()
    {
        if (mActiveTetrino != null)
        {
            mActiveTetrino.destroy();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (mPaused)
        {
            return;
        }

        if (mIsGameOver)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.X))
        {
            if (mDevModeCooldownTimeRemaining > 0)
            {
                mDevModeCooldownTimeRemaining -= Time.deltaTime;
            }
            else
            {
                if (mDevMode)
                {
                    Debug.Log("Dev mode disabled.");
                    mDevMode = false;
                }
                else
                {
                    Debug.Log("Dev mode enabled.");
                    mDevMode = true;
                }

                mDevModeCooldownTimeRemaining = mDevModeToggleCooldown;
            }
        }
    }

    public void Update()
    {
        if (mPaused)
        {
            return;
        }

        mPauseMenu.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            mPauseMenu.SetActive(true);
            mGameboardCanvas.SetActive(false);
            mBackEnd.SetActive(false);
        }

        if (mIsGameOver)
        {
            processGameOver();

            if (Input.GetKeyDown(KeyCode.R))
            {
                restart();
            }

            return;
        }

        mAudioController.playSoundTrack();

        if (mGameBoardModel.getLineAmount() != 0)
        {
            int actualLevel = (int)Mathf.Floor(mGameBoardModel.getLineAmount() / mLineToLevelIncrementor) + 1;

            if (actualLevel > mCurrentLevel)
            {
                while (mCurrentLevel < actualLevel)
                {
                    incrementLevel();
                }
            }
        }

        devModeChecks();

        processMovementKeyPress();
    }

    private void updateHighScore()
    {
        if (mCurrentScore > mHighScore)
        {
            mHighScoreTMP.text = mCurrentScore.ToString();
        }
    }

    private void processGameOver()
    {
        if (mGameOverProcessed)
        {
            return;
        }

        mGameOverTextTMP.gameObject.SetActive(true);

        if (mActiveTetrino != null)
        {
            mActiveTetrino.destroy();
            mActiveTetrino = null;
        }

        updateHighScore();

        mAudioController.playGameOver();

        mGameOverProcessed = true;
    }

    private void moveTetrino(Vector3 vector)
    {
        moveTetrino(vector, true);
    }

    private void moveTetrino(Vector3 vector, bool playSounds)
    {
        if (mActiveTetrino == null)
        {
            return;
        }

        bool moveSuccessful = mGameBoardModel.moveTetrino(mActiveTetrino, vector);
        mGameBoardModel.updateShadow(mActiveTetrino);

        if (playSounds && moveSuccessful)
        {
            mAudioController.playMove(vector);
        }
    }

    private void rotateTetrino()
    {
        if (mActiveTetrino == null)
        {
            return;
        }

        mGameBoardModel.requestToRotate(mActiveTetrino);

        mAudioController.playRotate();

        mGameBoardModel.updateShadow(mActiveTetrino);
    }

    private void processMovementKeyPress()
    {
        if (mActiveTetrino != null)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveTetrino(Vector3.left);
                mKeyDownTimeRemaining = mKeyDownTime * mKeyDownTimeDelayMultiplier;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                if (mKeyDownTimeRemaining > 0)
                {
                    mKeyDownTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    moveTetrino(Vector3.left, false);
                    mKeyDownTimeRemaining = mKeyDownTime;
                }
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveTetrino(Vector3.right);
                mKeyDownTimeRemaining = mKeyDownTime * mKeyDownTimeDelayMultiplier;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                if (mKeyDownTimeRemaining > 0)
                {
                    mKeyDownTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    moveTetrino(Vector3.right, false);
                    mKeyDownTimeRemaining = mKeyDownTime;
                }
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveTetrino(Vector3.down);
                mKeyDownTimeRemaining = mKeyDownTime * mKeyDownTimeDelayMultiplier;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                if (mKeyDownTimeRemaining > 0)
                {
                    mKeyDownTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    moveTetrino(Vector3.down, false);
                    mKeyDownTimeRemaining = mKeyDownTime;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                rotateTetrino();
            }

            else if (Input.GetKeyDown(KeyCode.Space))
            {
                mAudioController.playDropSound();

                while (mGameBoardModel.moveTetrino(mActiveTetrino, Vector3.down)) ;

                mGameBoardModel.updateShadow(mActiveTetrino);

                if (!mDevMode)
                {
                    mTetrinoFallTimeRemaining = -1;
                    checkFallTimer();
                }
            }
        }
    }

    private void devModeChecks()
    {
        if (mDevMode)
        {
            if (mActiveTetrino != null)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    moveTetrino(Vector3.up);
                    mKeyDownTimeRemaining = mKeyDownTime * mKeyDownTimeDelayMultiplier;
                }
                else if (Input.GetKey(KeyCode.W))
                {
                    if (mKeyDownTimeRemaining > 0)
                    {
                        mKeyDownTimeRemaining -= Time.deltaTime;
                    }
                    else
                    {
                        moveTetrino(Vector3.up, false);
                        mKeyDownTimeRemaining = mKeyDownTime;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    relinquishTetrino();
                }
                else if (Input.GetKeyDown(KeyCode.Delete))
                {
                    despawnTetrino();
                }
                if (Input.GetKey(KeyCode.Alpha0))
                {
                    mActiveTetrino.destroy(KeyCode.Alpha0);
                }
                if (Input.GetKey(KeyCode.Alpha1))
                {
                    mActiveTetrino.destroy(KeyCode.Alpha1);
                }
                if (Input.GetKey(KeyCode.Alpha2))
                {
                    mActiveTetrino.destroy(KeyCode.Alpha2);
                }
                if (Input.GetKey(KeyCode.Alpha3))
                {
                    mActiveTetrino.destroy(KeyCode.Alpha3);
                }
            }

            // ** end of Tetrino specific commands **

            if (Input.GetKeyDown(KeyCode.P))
            {
                mGameBoardModel.printGameBoard();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                devRestart();
            }
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                incrementLevel();
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                mAudioController.playNextSong();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.L_Block);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.J_Block);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.I_Block);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.O_Block);
            }
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                if (mActiveTetrino != null)
                {
                    Tetrino shadow = mActiveTetrino.getShadow();

                    if (shadow != null)
                    {
                        shadow.changeTetrinoToDestroyingBlocks();

                        foreach (GameObject currentSegment in shadow.getSegments())
                        {
                            if (currentSegment == null)
                            {
                                continue;
                            }

                            Animator animator = currentSegment.GetComponent<Animator>();

                            if (animator != null)
                            {
                                animator.enabled = true;
                            }
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.T_Block);
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.S_Block);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                relinquishTetrino();
                spawnTetrino(TetrinoType.Z_Block);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                relinquishTetrino();
                spawnRandomTetrino();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (mActiveTetrino != null)
                {
                    if (mActiveTetrino.getShadow())
                    {
                        mActiveTetrino.getShadow().destroy();
                    }

                    mActiveTetrino.setShadow(mActiveTetrino.Clone());

                    while (mGameBoardModel.moveTetrino(mActiveTetrino.getShadow(), Vector3.down)) ;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                rotateTetrino();
            }

            checkFallTimer();
        }
    }

    private void devRestart()
    {
        relinquishTetrino();
        updateHighScore();
        restart();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (PlayStateNotifier.ApplicationIsAboutToExitPlayMode() == true)
        {
            OnApplicationQuit();
        }
#endif
    }

    public void OnApplicationQuit()
    {
        updateHighScore();

        int highScore;

        if (int.TryParse(mHighScoreTMP.text, out highScore))
        {
            PlayerPrefs.SetInt(HIGH_SCORE_PREF_NAME, highScore);
        }
        else
        {
            Debug.Log("Error parsing high score.");
        }

        PlayerPrefs.Save();
    }

    private void checkFallTimer()
    {
        if (mTetrinoFallTimeRemaining > 0)
        {
            mTetrinoFallTimeRemaining -= Time.deltaTime;
        }
        else
        {
            mTetrinoFallTimeRemaining = mRunningTetrinoFallTime;

            if (mActiveTetrino != null)
            {
                if (!mGameBoardModel.moveTetrino(mActiveTetrino, Vector3.down))
                {
                    relinquishTetrino();
                    spawnRandomTetrino();
                }
            }
            else
            {
                spawnRandomTetrino();
            }
        }
    }

    private void relinquishTetrino()
    {
        mPaused = true;

        mGameBoardModel.absorbTetrino(mActiveTetrino);
        mActiveTetrino = null;
        mGameBoardModel.checkForFullRowsWrapper();
        updateScore();
    }

    public bool getIsGameOver()
    {
        return mIsGameOver;
    }

    public void setIsGameOver(bool gameOver)
    {
        mIsGameOver = gameOver;
    }

    public void toggleShadow()
    {
        if (mActiveTetrino == null)
        {
            Tetrino.setIsShadowEnabled(!Tetrino.getIsShadowEnabled());
            return;
        }

        mActiveTetrino.toggleShadow();
    }

    public void incrementLevel()
    {
        // Increase the level
        mCurrentLevel++;
        mLevelTextTMP.text = LEVEL_TEXT_HEADER + mCurrentLevel.ToString();

        // Respect the minimum fall time
        if (mRunningTetrinoFallTime < mMinFallTimer)
        {
            mRunningTetrinoFallTime = mMinFallTimer;
        }

        // Reset to the new timers
        mRunningTetrinoFallTime = mFetrinoFallTime * mRunningTimerMultiplier;
        mTetrinoFallTimeRemaining = mRunningTetrinoFallTime;

        // lower the fall time by the level modifier
        mRunningTimerMultiplier *= mFallTimerLevelModifier;

        // Respect the minimum fall time
        if (mRunningTetrinoFallTime < mMinFallTimer)
        {
            mRunningTetrinoFallTime = mMinFallTimer;
        }

        mEffectsController.triggerLevelUpEffects();
    }

    public void updateScore()
    {
        int linesRecentlyShifted = mGameBoardModel.getLinesRecentlyShifted();
        int linesMultiplier = linesRecentlyShifted * linesRecentlyShifted;

        if (linesRecentlyShifted == 4)
        {
            mEffectsController.triggerShakeEffects();
        }

        mAudioController.playRowDelete(linesRecentlyShifted);

        mCurrentScore += (mBaseScoreIncrement * linesMultiplier * mCurrentLevel);
        mCurrentScoreTMP.text = mCurrentScore.ToString();
        mGameBoardModel.resetLinesRecentlyShifted();
    }

    private void restart()
    {
        mGameBoardModel.restart();
        mEffectsController.restart();

        mKeyDownTimeRemaining = mKeyDownTime;
        mTetrinoFallTimeRemaining = mFetrinoFallTime;
        mRunningTetrinoFallTime = mFetrinoFallTime;
        mRunningTimerMultiplier = mFallTimerLevelModifier;
        mCurrentLevel = 1;
        mLevelTextTMP.text = LEVEL_TEXT_HEADER + mCurrentLevel.ToString();
        mCurrentScore = 0;
        mCurrentScoreTMP.text = "0";
        mIsGameOver = false;
        mGameOverTextTMP.gameObject.SetActive(false);
        mGameOverProcessed = false;

        mAudioController.playStartup();
    }

    public bool getDevMode()
    {
        return mDevMode;
    }

    public bool getPaused()
    {
        return mPaused;
    }

    public void setPaused(bool paused)
    {
        mPaused = paused;
    }

    void OnDestroy()
    {
        if (mPostProcessVolumes != null)
        {
            RuntimeUtilities.DestroyVolume(mPostProcessVolumes, true, true);
        }
    }
}
