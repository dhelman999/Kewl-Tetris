using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Tetrino;

public class GameBoard : MonoBehaviour
{
    // Play area size
    public static int MAX_X_SIZE = 10;
    public static int MAX_Y_SIZE = 30;

    // Don't change this unless you also change the name in the unity hierarchy
    public static string GAME_BOARD_NAME = "GameBoardModel";

    public RectTransform mPlayArea;
    public TetrinoFactory mTetrinoFactory;
    public GameController mGameController;
    public TextMeshProUGUI mLinesTMP;
    public EffectsController mEffectsController;

    // Backend model representation of the view
    private BaseBlock[,] mModelCells = new BaseBlock[MAX_X_SIZE, MAX_Y_SIZE];

    private static string LINES_TEXT_HEADER = "LINES - ";

    // Starting position for new Tetrinos
    private static int STARTING_X = 4;
    private static int STARTING_Y = 24;

    // Ease of use bounds variables, easier to do it this way than recalculate everytime
    private float mLeftMostBounds;
    private float mRightMostBounds;
    private float mBottomMostBounds;
    private float mTopMostBounds;

    private int mLineAmount = 0;
    private int mLinesRecentlyShifted = 0;

    private bool mIsDeletingRows = false;

    private void Start()
    {
        // Calculate bounds
        mLeftMostBounds = Mathf.Round(getLeftMostBounds());
        mRightMostBounds = Mathf.Round(getRightMostBounds());
        mBottomMostBounds = Mathf.Round(getBottomMostBounds());
        mTopMostBounds = Mathf.Round(getTopMostBounds());
    }

    public Tetrino spawnRandomTetrino()
    {
        Tetrino randomTetrino = mTetrinoFactory.createRandomTetrino();

        return spawnTetrino(TetrinoType.INVALID, randomTetrino);
    }


    public Tetrino spawnTetrino(TetrinoType type, Tetrino newTetrino)
    {
        if (newTetrino == null)
        {
            newTetrino = mTetrinoFactory.createTetrino(type, false);
        }

        Vector3 desiredMovement = new Vector3(BaseBlock.BLOCK_SIZE * STARTING_X, BaseBlock.BLOCK_SIZE * STARTING_Y, 0);
        bool insert = newTetrino.insertIntoModel(desiredMovement);

        // We can check if the model insert succeeded or not, a reason why it could fail is if there was already blocks
        // at that location, in this case, the game should end
        if (insert)
        {
            newTetrino.GetComponent<RectTransform>().localPosition += desiredMovement;
            updateShadow(newTetrino);
        }
        else
        {
            mGameController.setIsGameOver(true);
        }

        return newTetrino;
    }

    public void updateShadow(Tetrino tetrino)
    {
        if (tetrino == null)
        {
            return;
        }

        if (tetrino.getShadow())
        {
            tetrino.getShadow().destroy();
        }

        tetrino.setShadow(tetrino.Clone());

        while (moveTetrino(tetrino.getShadow(), Vector3.down)) ;
    }

    // Takes a base segment and moves it down 1 block
    private void shiftBaseSegmentDown(BaseBlock baseBlock, int x, int y)
    {
        if (baseBlock == null)
        {
            Debug.Log("Can't shift a null segment down.");
            return;
        }

        if (!validCellCoordinates(x, y))
        {
            Debug.Log("Can't shift a segment out of the play area.");
            return;
        }

        // Remove the block from its previous position
        setModel(null, x, y);

        // Shift it down by 1
        if (setModel(baseBlock.gameObject, x, y - 1))
        {
            Vector3 desiredMovement = new Vector3(0, BaseBlock.BLOCK_SIZE, 0);
            baseBlock.gameObject.transform.localPosition -= desiredMovement;
        }
        else
        {
            Debug.Log("Error while shifting the base block down.");
        }

    }

    public bool moveTetrino(Tetrino tetrino, Vector3 vector)
    {
        if (tetrino == null)
        {
            return false;
        }

        // Calculate desired movement
        Vector3 oldPosition = tetrino.transform.localPosition;
        Vector3 desiredMovement = new Vector3(vector.x * BaseBlock.BLOCK_SIZE, vector.y * BaseBlock.BLOCK_SIZE, 0);
        Vector3 desiredPosition = oldPosition + desiredMovement;

        // Get predicted position and bounds
        float desiredPoint = getGreatestPointBasedOnDesiredDirection(tetrino, vector);
        float targetBounds = getBoundsFromDesiredDirection(vector);

        bool moveSuccessful = false;

        // Check play area bounds based on requested direction, request the move accordingly
        if (vector.Equals(Vector3.left))
        {
            if ((int)desiredPoint >= targetBounds)
            {
                moveSuccessful = requestMove(tetrino, vector);
            }
        }
        else if (vector.Equals(Vector3.right))
        {
            if ((int)desiredPoint <= targetBounds)
            {
                moveSuccessful = requestMove(tetrino, vector);
            }
        }
        else if (vector.Equals(Vector3.up))
        {
            if ((int)desiredPoint <= targetBounds)
            {
                moveSuccessful = requestMove(tetrino, vector);
            }
        }
        else if (vector.Equals(Vector3.down))
        {
            if ((int)desiredPoint >= targetBounds)
            {
                moveSuccessful = requestMove(tetrino, vector);
            }
        }
        else
        {
            Debug.Log("Movemment not allowed for vector: " + vector.ToString());
        }

        if (moveSuccessful)
        {
            tetrino.transform.localPosition = desiredPosition;
        }

        return moveSuccessful;
    }

    public bool requestMove(Tetrino tetrino, Vector3 vector)
    {
        if (!vector.Equals(Vector3.left) &&
           !vector.Equals(Vector3.right) &&
           !vector.Equals(Vector3.down) &&
           !vector.Equals(Vector3.up))
        {
            return false;
        }

        bool requestGranted = true;

        // First remove the segments from the model so they don't collide with themselves.
        removeTetrinoFromModel(tetrino);
        List<GameObject> segments = tetrino.getSegments();

        if (tetrino.isEmpty())
        {
            // This can happen in dev mode, but it better not outside of it.
            if (!mGameController.getDevMode())
            {
                Debug.Log("Empty Tetrino requested move, probably a memory leak here.");
            }

            return false;
        }

        foreach (GameObject segment in segments)
        {
            if (segment == null)
            {
                continue;
            }

            BaseBlock baseBlock = segment.GetComponent<BaseBlock>();

            if (baseBlock == null)
            {
                continue;
            }

            BaseBlock existingBlock = null;

            if (vector.Equals(Vector3.left))
            {
                existingBlock = getModel(baseBlock.getX() - 1, baseBlock.getY());
            }
            else if (vector.Equals(Vector3.right))
            {
                existingBlock = getModel(baseBlock.getX() + 1, baseBlock.getY());
            }
            else if (vector.Equals(Vector3.down))
            {
                existingBlock = getModel(baseBlock.getX(), baseBlock.getY() - 1);
            }
            else if (vector.Equals(Vector3.up))
            {
                existingBlock = getModel(baseBlock.getX(), baseBlock.getY() + 1);
            }

            // See if there are any existing pieces in the model that would cause a collision.
            if (existingBlock != null)
            {
                if (!checkForParentBlock(tetrino, existingBlock))
                {
                    requestGranted = false;
                    break;
                }
            }
        }

        // The path is clear, we can do the move.
        if (requestGranted)
        {
            // Perform the actual model move.
            foreach (GameObject segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }

                BaseBlock baseBlock = segment.GetComponent<BaseBlock>();
                if (baseBlock == null)
                {
                    continue;
                }

                BaseBlock existingBlock = null;

                if (vector.Equals(Vector3.left))
                {
                    existingBlock = getModel(baseBlock.getX() - 1, baseBlock.getY());
                }
                else if (vector.Equals(Vector3.right))
                {
                    existingBlock = getModel(baseBlock.getX() + 1, baseBlock.getY());
                }
                else if (vector.Equals(Vector3.down))
                {
                    existingBlock = getModel(baseBlock.getX(), baseBlock.getY() - 1);
                }
                else if (vector.Equals(Vector3.up))
                {
                    existingBlock = getModel(baseBlock.getX(), baseBlock.getY() + 1);
                }

                if (requestGranted)
                {
                    if (!checkForParentBlock(tetrino, existingBlock))
                    {
                        requestGranted = (existingBlock == null);
                    }
                }

                // Shadow clones don't enter the model
                if (!tetrino.getIsShadowClone())
                {
                    setModel(segment, baseBlock.getX() + (int)vector.x, baseBlock.getY() + (int)vector.y);
                }
                else
                {
                    // Even if shadow clones don't enter the model, we still allow them to update their position
                    baseBlock.setX(baseBlock.getX() + (int)vector.x);
                    baseBlock.setY(baseBlock.getY() + (int)vector.y);
                }
            }
        }
        else
        {
            // If the move failed, we need to remember to add the Tetrino back to the model.
            if (!tetrino.getIsShadowClone())
            {
                addTetrinoToModel(tetrino);
            }
        }

        return requestGranted;
    }

    // Gets the play area bounds based on the requested direction.
    private float getBoundsFromDesiredDirection(Vector3 desiredDirection)
    {
        float targetBounds = -1;

        if (desiredDirection.Equals(Vector3.left))
        {
            targetBounds = mLeftMostBounds;
        }
        else if (desiredDirection.Equals(Vector3.right))
        {
            targetBounds = mRightMostBounds;
        }
        else if (desiredDirection.Equals(Vector3.down))
        {
            targetBounds = mBottomMostBounds;
        }
        else if (desiredDirection.Equals(Vector3.up))
        {
            targetBounds = mTopMostBounds;
        }

        return targetBounds;
    }

    // Helper function to get the greatest magnitude point from all of the segments in a tetrino based on the requested direction
    private float getGreatestPointBasedOnDesiredDirection(Tetrino tetrino, Vector3 desiredDirection)
    {
        float targetPoint = -1;

        if (desiredDirection.Equals(Vector3.left))
        {
            targetPoint = tetrino.getLeftMostPoint().x - BaseBlock.BLOCK_SIZE;
        }
        else if (desiredDirection.Equals(Vector3.right))
        {
            targetPoint = tetrino.getRightMostPoint().x + BaseBlock.BLOCK_SIZE;
        }
        else if (desiredDirection.Equals(Vector3.down))
        {
            targetPoint = tetrino.getBottomMostPoint().y - BaseBlock.BLOCK_SIZE;
        }
        else if (desiredDirection.Equals(Vector3.up))
        {
            targetPoint = tetrino.getTopMostPoint().y + BaseBlock.BLOCK_SIZE;
        }

        return targetPoint;
    }

    public float getLeftMostBounds()
    {
        Vector3[] worldCorners = new Vector3[4];
        mPlayArea.GetWorldCorners(worldCorners);
        float leftMostX = mPlayArea.transform.InverseTransformPoint(worldCorners[0]).x;

        return leftMostX;
    }

    public float getRightMostBounds()
    {
        Vector3[] worldCorners = new Vector3[4];
        mPlayArea.GetWorldCorners(worldCorners);
        float rightMostX = mPlayArea.transform.InverseTransformPoint(worldCorners[3]).x;

        return rightMostX;
    }

    public float getBottomMostBounds()
    {
        Vector3[] worldCorners = new Vector3[4];
        mPlayArea.GetWorldCorners(worldCorners);
        float bottomMostY = mPlayArea.transform.InverseTransformPoint(worldCorners[0]).y;

        return bottomMostY;
    }

    public float getTopMostBounds()
    {
        Vector3[] worldCorners = new Vector3[4];
        mPlayArea.GetWorldCorners(worldCorners);
        float bottomMostY = mPlayArea.transform.InverseTransformPoint(worldCorners[1]).y;

        return bottomMostY;
    }

    public bool isWithinRightBounds(Tetrino tetrino)
    {
        float tetrinoRightMostPoint = tetrino.getRightMostPoint().x;
        bool isWithinBounds = (int)tetrinoRightMostPoint <= (int)mRightMostBounds;

        return isWithinBounds;
    }

    public bool isWithinLeftBounds(Tetrino tetrino)
    {
        float tetrinoLeftMostPoint = tetrino.getLeftMostPoint().x;
        bool isWithinBounds = (int)tetrinoLeftMostPoint >= (int)mLeftMostBounds;

        return isWithinBounds;
    }

    public BaseBlock getModel(int x, int y)
    {
        if (!validCellCoordinates(x, y))
        {
            return null;
        }

        return mModelCells[x, y];
    }

    public bool setModel(GameObject block, int x, int y)
    {
        BaseBlock baseBlock = null;

        // Get the BaseBlock if applicable
        if (block != null)
        {
            baseBlock = block.GetComponent<BaseBlock>();
        }

        // Set the coordinates of the base block, note that we do allow the cooridnates to be
        // outside of the actual model play area because we do bounds checking and adjustment
        // later in the movement workflow.
        if (baseBlock != null)
        {
            baseBlock.setX(x);
            baseBlock.setY(y);
        }

        bool validCoordinates = true;

        // Validate the coordinates in the model.
        if (!validCellCoordinates(x, y))
        {
            validCoordinates = false;
        }

        // Only set the model with valid coordinates, note that from above, the position may be invalid for the actual
        // base blocks as they could rotate outside the game play area.
        if (validCoordinates)
        {
            mModelCells[x, y] = baseBlock;
        }

        return true;
    }

    // Helper function to remove each of the Tetrino segments from the model.
    private void removeTetrinoFromModel(Tetrino tetrino)
    {
        List<GameObject> segments = tetrino.getSegments();

        foreach (GameObject segment in segments)
        {
            if (segment == null)
            {
                continue;
            }

            BaseBlock baseBlock = segment.GetComponent<BaseBlock>();

            if (baseBlock != null)
            {
                // Don't remove blocks that belong to the parent from the model
                if (!checkForParentBlock(tetrino, baseBlock))
                {
                    setModel(null, baseBlock.getX(), baseBlock.getY());
                }
            }
            else
            {
                Debug.Log("Invalid model removal for Tetrino: " + tetrino.ToString());
            }

        }
    }

    private bool checkForParentBlock(Tetrino childTetrino, BaseBlock baseBlock)
    {
        if (childTetrino == null || baseBlock == null)
        {
            return false;
        }

        Tetrino parent = childTetrino.getParent();

        if (parent == null)
        {
            return false;
        }

        foreach (GameObject segment in parent.getSegments())
        {
            if (segment == null)
            {
                continue;
            }

            BaseBlock currentParentBaseBlock = segment.GetComponent<BaseBlock>();

            if (currentParentBaseBlock != null)
            {
                if (baseBlock.isAtSameLocation(currentParentBaseBlock))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Helper function to add each of the Tetrino segments from the model.
    private void addTetrinoToModel(Tetrino tetrino)
    {
        List<GameObject> segments = tetrino.getSegments();

        foreach (GameObject segment in segments)
        {
            BaseBlock baseBlock = segment.GetComponent<BaseBlock>();

            setModel(segment, baseBlock.getX(), baseBlock.getY());
        }
    }

    // Helper function to rotate and adjust the Tetrino if it rotated out of bounds.
    private void safeRotateTetrino(Tetrino tetrino)
    {
        if (tetrino == null)
        {
            return;
        }

        tetrino.rotateTetrinoUp();

        adjustTetrinoBounds(tetrino);
    }

    // Attempts to perform a rotation.
    public bool requestToRotate(Tetrino tetrino)
    {
        bool requestGranted = true;

        // First remove the segments from the model so they don't collide with themselves.
        removeTetrinoFromModel(tetrino);

        // Create a shadow clone to do a test rotate to see if it collides with anything.
        Tetrino tetrinoClone = tetrino.Clone();
        safeRotateTetrino(tetrinoClone);

        List<GameObject> segments = tetrinoClone.getSegments();

        foreach (GameObject segment in segments)
        {
            BaseBlock baseBlock = segment.GetComponent<BaseBlock>();
            BaseBlock existingBlock = getModel(baseBlock.getX(), baseBlock.getY());

            // See if there are any existing pieces in the model that would cause a collision.
            if (existingBlock != null)
            {
                requestGranted = false;
                break;
            }
        }

        if (requestGranted)
        {
            // The shadow clone rotation was successful, now rotate the actual Tetrino
            safeRotateTetrino(tetrino);
            segments = tetrino.getSegments();

            // Rotation is complete, add it to the model
            foreach (GameObject segment in segments)
            {
                BaseBlock baseBlock = segment.GetComponent<BaseBlock>();
                setModel(segment, baseBlock.getX(), baseBlock.getY());
            }
        }
        else
        {
            // If the move failed, we need to remember to add the Tetrino back to the model.
            addTetrinoToModel(tetrino);
        }

        // Don't forget to destroy the clone so we don't have a memory leak.
        tetrinoClone.destroy();

        return requestGranted;
    }

    // Check if a Tetrino rotated out of bounds and if so, push it back in.
    private void adjustTetrinoBounds(Tetrino tetrino)
    {
        // Safety loop to not accidentally go on forever
        for (int i = 0; i < 5; i++)
        {
            if (!isWithinLeftBounds(tetrino))
            {
                moveTetrino(tetrino, Vector3.right);
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (!isWithinRightBounds(tetrino))
            {
                moveTetrino(tetrino, Vector3.left);
            }
            else
            {
                break;
            }
        }
    }

    public void checkForFullRowsWrapper()
    {
        List<int> rowDeleteIndexes = new List<int>();

        if (mIsDeletingRows)
        {
            return;
        }

        checkForFullRows(rowDeleteIndexes);

        if (rowDeleteIndexes.Count == 0)
        {
            mGameController.setPaused(false);
            return;
        }

        updateLinesText(rowDeleteIndexes.Count);

        foreach (int currentRowIndex in rowDeleteIndexes)
        {
            changeRowToDestroyingRow(currentRowIndex);
        }

        StartCoroutine(delayedDeleteAndShiftGameRowDown(rowDeleteIndexes));
    }

    public void checkForFullRows(List<int> rowDeleteIndexes)
    {
        for (int rowIndex = 0; rowIndex < MAX_Y_SIZE; rowIndex++)
        {
            bool shouldDeleteAndShiftRows = true;

            for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
            {
                if (mModelCells[columnIndex, rowIndex] == null)
                {
                    shouldDeleteAndShiftRows = false;
                    break;
                }
            }

            if (shouldDeleteAndShiftRows)
            {
                rowDeleteIndexes.Add(rowIndex);
            }
        }
    }

    public IEnumerator delayedDeleteAndShiftGameRowDown(List<int> rowDeleteIndexes)
    {
        mIsDeletingRows = true;
        float clipLength = mEffectsController.getDestroyingBlockAnimationLength();

        yield return new WaitForSeconds(clipLength);

        foreach (int currentRowIndex in rowDeleteIndexes)
        {
            deleteRow(currentRowIndex);
        }

        int rowShifts = 0;

        foreach (int currentRowIndex in rowDeleteIndexes)
        {
            shiftGameBordDown(currentRowIndex - rowShifts++);
        }

        mGameController.setPaused(false);
        mIsDeletingRows = false;
    }

    private void changeRowToDestroyingRow(int rowChangeIndex)
    {
        for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
        {
            BaseBlock currentBaseBlock = mModelCells[columnIndex, rowChangeIndex];
            BaseBlock destroyingBlock = changeBaseBlockToDestroyingBlock(currentBaseBlock.gameObject);

            mModelCells[columnIndex, rowChangeIndex] = destroyingBlock;
        }
    }

    // Helper function to delete a row.
    private void deleteRow(int rowDeleteIndex)
    {
        for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
        {
            BaseBlock currentBaseBlock = mModelCells[columnIndex, rowDeleteIndex];

            currentBaseBlock.destroy();
            mModelCells[columnIndex, rowDeleteIndex] = null;
        }
    }

    private void updateLinesText(int numLines)
    {
        mLineAmount += numLines;
        mLinesRecentlyShifted += numLines;
        mLinesTMP.text = LINES_TEXT_HEADER + mLineAmount.ToString();
    }

    // Helper function to shift all rows down from the requested index.
    private void shiftGameBordDown(int rowDeleteIndex)
    {
        for (int rowIndex = rowDeleteIndex + 1; rowIndex < MAX_Y_SIZE; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
            {
                BaseBlock currentBaseBlock = mModelCells[columnIndex, rowIndex];

                if (currentBaseBlock != null)
                {
                    shiftBaseSegmentDown(currentBaseBlock, columnIndex, rowIndex);
                }
            }
        }
    }

    // Take all of the Tetrinos individual segments and add them to the model as individual objects. This disassociates
    // the segments with the parent Tetrino so we are free to destroy them and manipulate them without having to
    // worry about the Tetrino as it is useless now.
    public void absorbTetrino(Tetrino tetrino)
    {
        if (tetrino == null)
        {
            return;
        }

        // We move the parent of the segments from the Tetrino to the play area.
        foreach (GameObject currentSegment in tetrino.getSegments())
        {
            if (currentSegment == null)
            {
                continue;
            }

            BaseBlock currentBaseBlock = currentSegment.GetComponent<BaseBlock>();

            if (currentBaseBlock == null)
            {
                continue;
            }

            currentSegment.gameObject.transform.SetParent(tetrino.transform.parent);
        }

        tetrino.destroyShadow();

        // Don't forget to destroy the parent Tetrino.
        Destroy(tetrino.gameObject);
    }

    // Debug print function
    public void printGameBoard()
    {

        for (int rowIndex = MAX_Y_SIZE - 1; rowIndex >= 0; rowIndex--)
        {
            string logLine = "Row " + rowIndex + ": ";

            for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
            {
                BaseBlock currentBlock = mModelCells[columnIndex, rowIndex];

                if (currentBlock == null)
                {
                    logLine += " - ";
                }
                else
                {
                    logLine += " X ";
                }
            }

            Debug.Log(logLine);
        }
    }

    // Helper function to validate model coordinates
    private bool validCellCoordinates(int x, int y)
    {
        if (x < 0 || x >= MAX_X_SIZE ||
            y < 0 || y >= MAX_Y_SIZE)
        {
            return false;
        }

        return true;
    }

    public RectTransform getPlayArea()
    {
        return mPlayArea;
    }

    public int getLineAmount()
    {
        return mLineAmount;
    }

    public int getLinesRecentlyShifted()
    {
        return mLinesRecentlyShifted;
    }

    public void resetLinesRecentlyShifted()
    {
        mLinesRecentlyShifted = 0;
    }

    public void restart()
    {
        for (int rowIndex = 0; rowIndex < MAX_Y_SIZE; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < MAX_X_SIZE; columnIndex++)
            {
                BaseBlock currentBlock = mModelCells[columnIndex, rowIndex];

                if (currentBlock != null)
                {
                    Destroy(currentBlock.gameObject);
                    mModelCells[columnIndex, rowIndex] = null;
                }
            }
        }

        mLineAmount = 0;
        mLinesTMP.text = LINES_TEXT_HEADER + mLineAmount.ToString();
        resetLinesRecentlyShifted();
        mTetrinoFactory.restart();
    }
}
