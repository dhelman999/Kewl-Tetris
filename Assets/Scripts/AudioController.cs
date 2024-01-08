using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public List<AudioSource> mPlayList;
    private AudioSource mCurrentSong;

    public AudioSource mMetriodBrinstarMusic;
    public AudioSource mLittleFeugeMusic;
    public AudioSource mStartUpSound;
    public AudioSource mGameOverSound;
    public AudioSource mMoveLeftSound;
    public AudioSource mMoveRightSound;
    public AudioSource mMoveDownSound;
    public AudioSource mMoveRotate;
    public AudioSource mMoveDropSound;
    public AudioSource mRowDelete1;
    public AudioSource mRowDelete2;
    public AudioSource mRowDelete3;
    public AudioSource mRowDelete4;
    public AudioSource mLevelUp;

    private int LITTLE_FEUGUE_SEEK_TIME = 3;

    private bool mIsAppPaused = false;

    private void Awake()
    {
        mLittleFeugeMusic.time = LITTLE_FEUGUE_SEEK_TIME;
    }

    public void playStartup()
    {
        mStartUpSound.Play();
    }

    public void playGameOver()
    {
        mGameOverSound.Play();
    }

    public void playMove(Vector3 vector)
    {
        if (vector.Equals(Vector3.left))
        {
            mMoveLeftSound.Play();
        }
        else if (vector.Equals(Vector3.right))
        {
            mMoveRightSound.Play();
        }
        else if (vector.Equals(Vector3.down))
        {
            mMoveDownSound.Play();
        }
        else if (vector.Equals(Vector3.up))
        {
            mMoveDownSound.Play();
        }
    }

    public void playRotate()
    {
        mMoveRotate.Play();
    }

    public void playDropSound()
    {
        mMoveDropSound.Play();
    }

    public void playRowDelete(int numRows)
    {
        switch (numRows)
        {
            case 1:
                mRowDelete1.Play();
                break;
            case 2:
                mRowDelete2.Play();
                break;
            case 3:
                mRowDelete3.Play();
                break;
            case 4:
                //rowDelete4.Play();
                playLevelUp();
                break;
        }
    }

    public void playLevelUp()
    {
        if (!mLevelUp.isPlaying)
        {
            mLevelUp.Play();
        }
    }

    public void playSoundTrack()
    {
        StartCoroutine(playAudioSequentially());
    }

    private void OnApplicationPause(bool paused)
    {
        mIsAppPaused = paused;
    }

    public IEnumerator playAudioSequentially()
    {

        if (mPlayList == null || mPlayList.Count == 0)
        {
            yield return null;
        }

        if (mCurrentSong == null)
        {
            mCurrentSong = mPlayList[0];
            playNextSong();
        }
        else if (!mCurrentSong.isPlaying)
        {
            if (!mIsAppPaused)
            {
                playNextSong();
            }

        }
        else if (mCurrentSong.isPlaying)
        {
            foreach (AudioSource music in mPlayList)
            {
                if (music.Equals(mCurrentSong))
                {
                    continue;
                }

                music.Stop();
            }
        }

        yield return null;
    }

    public void playNextSong()
    {
        if (mPlayList == null || mPlayList.Count == 0)
        {
            return;
        }

        if (mCurrentSong != null && mCurrentSong.isPlaying)
        {
            mCurrentSong.Stop();
        }

        mCurrentSong = mPlayList[0];

        if (mCurrentSong == null)
        {
            return;
        }

        mPlayList.RemoveAt(0);
        mPlayList.Add(mCurrentSong);
        mCurrentSong.Play();
    }
}
