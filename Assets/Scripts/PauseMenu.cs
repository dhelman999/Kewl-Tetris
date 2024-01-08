using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject mPauseMenuCanvas;
    public GameObject mGameBoard;
    public GameController gameController;
    public GameObject mBackend;
    public Button mContinueButton;
    public Button mExitButton;
    public TextMeshProUGUI mContinueText;
    public AudioController mAudioController;

    private string CONTINUE_TEXT = "Continue";

    private bool mGameStarted = false;
    private SelectedButton mLastSelected = SelectedButton.NONE;

    public enum SelectedButton
    {
        CONTINUE,
        EXIT,
        NONE
    }

    private void Awake()
    {
        mGameBoard.SetActive(false);
        mBackend.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitGame();
        }

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.D))
            {
                selectButton(mContinueButton.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.S))
        {
            if (mLastSelected.Equals(SelectedButton.CONTINUE))
            {
                selectButton(mExitButton.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W))
        {
            if (mLastSelected.Equals(SelectedButton.EXIT))
            {
                selectButton(mContinueButton.gameObject);
            }
        }
    }

    private SelectedButton getSelected()
    {
        SelectedButton selectedButton = SelectedButton.NONE;
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null)
        {
            return SelectedButton.NONE;
        }

        if (currentSelected.Equals(mContinueButton.gameObject))
        {
            if (!mLastSelected.Equals(SelectedButton.CONTINUE))
            {
                selectButton(mContinueButton.gameObject);
            }

            mLastSelected = SelectedButton.CONTINUE;
        }
        if (currentSelected.Equals(mExitButton.gameObject))
        {
            if (!mLastSelected.Equals(SelectedButton.EXIT))
            {
                selectButton(mExitButton.gameObject);
            }

            mLastSelected = SelectedButton.EXIT;
        }

        return selectedButton;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        List<GameObject> hoveredList = eventData.hovered;

        foreach (GameObject currentGameObject in hoveredList)
        {
            selectButton(currentGameObject);
        }
    }

    private SelectedButton selectButton(GameObject button)
    {
        if (button == null)
        {
            return SelectedButton.NONE;
        }

        SelectedButton selectedButton = SelectedButton.NONE;

        if (button.Equals(mContinueButton.gameObject))
        {
            mContinueButton.Select();
            mAudioController.playMove(Vector3.left);
            selectedButton = mLastSelected = SelectedButton.CONTINUE;
        }
        else if (button.Equals(mExitButton.gameObject))
        {
            mExitButton.Select();
            mAudioController.playMove(Vector3.right);
            selectedButton = mLastSelected = SelectedButton.EXIT;
        }

        return selectedButton;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void continueClicked()
    {
        if (!mGameStarted)
        {
            mContinueText.text = CONTINUE_TEXT;
        }

        mGameBoard.SetActive(true);
        mBackend.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        mPauseMenuCanvas.SetActive(false);
        mGameStarted = true;
    }

    public void exitClicked()
    {
        quitGame();
    }

    private void quitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        mGameBoard.SetActive(true);
        gameController.OnApplicationQuit();
#endif

        Application.Quit();
    }
}
