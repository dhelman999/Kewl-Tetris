using System.Collections.Generic;
using UnityEngine;

public class Tetrino : MonoBehaviour
{
    private Tetrino mParent;
    private Tetrino mShadow;

    protected List<GameObject> mSegments = new List<GameObject>();
    protected bool mIsShadowClone = false;

    public GameBoard mGameBoardModel;
    public GameObject mBaseSegment;
    public TetrinoFactory mTetrinoFactory;

    private static string SHADOW_BLOCK_PATH = "Sprites/ShadowBlock";
    private GameObject mShadowBlock;
    private SpriteRenderer mShadowBlockSR;

    private static string DESTROYING_BLOCK_PATH = "Sprites/DestroyingBlock";
    private static GameObject mDestryoingBlock;


    private static float HIGH_BOUNDS = 9999;
    private static float LOW_BOUNDS = -9999;

    private static bool mIsShadowEnabled = false;

    private void Awake()
    {
        mShadowBlock = Resources.Load(SHADOW_BLOCK_PATH) as GameObject;
        mShadowBlockSR = mShadowBlock.GetComponent<SpriteRenderer>();
        mDestryoingBlock = Resources.Load(DESTROYING_BLOCK_PATH) as GameObject;
    }

    public enum TetrinoType
    {
        L_Block,
        J_Block,
        I_Block,
        O_Block,
        T_Block,
        S_Block,
        Z_Block,
        INVALID
    }

    public Tetrino getShadow()
    {
        return mShadow;
    }

    public void setShadow(Tetrino tetrino)
    {
        mShadow = tetrino;
        toggleShadow(false);
    }

    public void destroyShadow()
    {
        if (mShadow != null)
        {
            mShadow.destroy();
        }
    }

    protected GameBoard getGameBoardModel()
    {
        if (mGameBoardModel == null)
        {
            mGameBoardModel = GameObject.Find(GameBoard.GAME_BOARD_NAME).GetComponent<GameBoard>();
        }

        return mGameBoardModel;
    }

    protected TetrinoFactory getTetrinoFactory()
    {
        if (mTetrinoFactory == null)
        {
            mTetrinoFactory = GameObject.Find(TetrinoFactory.TETRINO_FACTORY_NAME).GetComponent<TetrinoFactory>();
        }

        return mTetrinoFactory;
    }

    private void safeDestroyShadow()
    {
        if (mShadow != null)
        {
            mShadow.destroy();
        }
    }

    public void destroy()
    {
        for (int currentIndex = 0; currentIndex < mSegments.Count; currentIndex++)
        {
            GameObject currentSegment = mSegments[currentIndex];

            if (currentSegment != null)
            {
                Destroy(currentSegment);
            }
        }

        safeDestroyShadow();

        Destroy(gameObject);
    }

    public void destroy(KeyCode keyCode)
    {
        if (mSegments == null)
        {
            return;
        }

        GameObject targetSegment = null;

        switch (keyCode)
        {
            case KeyCode.Alpha0:
                if (mSegments.Count >= 1)
                {
                    targetSegment = mSegments[0];
                    mSegments[0] = null;
                }
                break;
            case KeyCode.Alpha1:
                if (mSegments.Count >= 2)
                {
                    targetSegment = mSegments[1];
                    mSegments[1] = null;
                }
                break;
            case KeyCode.Alpha2:
                if (mSegments.Count >= 3)
                {
                    targetSegment = mSegments[2];
                    mSegments[2] = null;
                }
                break;
            case KeyCode.Alpha3:
                if (mSegments.Count >= 4)
                {
                    targetSegment = mSegments[3];
                    mSegments[3] = null;
                }
                break;
            case KeyCode.Alpha4:
                if (mSegments.Count >= 5)
                {
                    targetSegment = mSegments[4];
                    mSegments[4] = null;
                }
                break;
        }

        if (targetSegment != null)
        {
            Destroy(targetSegment);
        }

        if (isEmpty())
        {
            safeDestroyShadow();
            Destroy(gameObject);
        }
    }

    public bool isEmpty()
    {
        bool isParentEmpty = true;

        foreach (GameObject currentSegment in mSegments)
        {
            if (currentSegment != null)
            {
                isParentEmpty = false;
                break;
            }
        }

        return isParentEmpty;
    }

    public GameObject getBottomLeftBlock()
    {
        float lowestX = HIGH_BOUNDS;
        float lowestY = HIGH_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getLeftMostPoint(currentSegment);

            if (segmentWorldPos.x < lowestX)
            {
                targetBlock = currentSegment;
                lowestX = segmentWorldPos.x;
                lowestY = segmentWorldPos.y;
            }
            else if (segmentWorldPos.x == lowestX)
            {
                if (segmentWorldPos.y < lowestY)
                {
                    targetBlock = currentSegment;
                    lowestX = segmentWorldPos.x;
                    lowestY = segmentWorldPos.y;
                }
            }
        }

        return targetBlock;
    }

    private Vector3 getLeftMostXBounds(RectTransform rectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 leftMostPoint = new Vector3(HIGH_BOUNDS, 0, -1);

        foreach (Vector3 currentPoint in worldCorners)
        {
            Vector3 worldPoint = getGameBoardModel().getPlayArea().InverseTransformPoint(currentPoint);

            if (worldPoint.x < leftMostPoint.x)
            {
                leftMostPoint = worldPoint;
            }
        }

        return leftMostPoint;
    }

    private Vector3 getRightMostXBounds(RectTransform rectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 rightMostPoint = new Vector3(LOW_BOUNDS, 0, -1);

        foreach (Vector3 currentPoint in worldCorners)
        {
            Vector3 worldPoint = getGameBoardModel().getPlayArea().InverseTransformPoint(currentPoint);

            if (worldPoint.x > rightMostPoint.x)
            {
                rightMostPoint = worldPoint;
            }
        }

        return rightMostPoint;
    }

    public Vector3 getLeftMostPoint(GameObject targetBlock)
    {
        if (targetBlock == null)
        {
            return new Vector3(HIGH_BOUNDS, 0, 0);
        }

        RectTransform transform = targetBlock.GetComponent<RectTransform>();

        return getLeftMostXBounds(transform);
    }

    public Vector3 getLeftMostPoint()
    {
        return getLeftMostPoint(getBottomLeftBlock());
    }

    public Vector3 getRightMostPoint(GameObject targetBlock)
    {
        if (targetBlock == null)
        {
            return new Vector3(LOW_BOUNDS, 0, 0);
        }

        RectTransform rectTransform = targetBlock.GetComponent<RectTransform>();

        return getRightMostXBounds(rectTransform);
    }

    public Vector3 getRightMostPoint()
    {
        return getRightMostPoint(getBottomRightBlock());
    }

    public GameObject getBottomRightBlock()
    {
        float highestX = LOW_BOUNDS;
        float lowestY = HIGH_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getRightMostPoint(currentSegment);

            if (segmentWorldPos.x > highestX)
            {
                targetBlock = currentSegment;
                highestX = segmentWorldPos.x;
                lowestY = segmentWorldPos.y;
            }
            else if (segmentWorldPos.x == highestX)
            {
                if (segmentWorldPos.y < lowestY)
                {
                    targetBlock = currentSegment;
                    highestX = segmentWorldPos.x;
                    lowestY = segmentWorldPos.y;
                }
            }
        }

        return targetBlock;
    }

    public Vector3 getBottomMostPoint(GameObject targetBlock)
    {
        if (targetBlock == null)
        {
            return new Vector3(LOW_BOUNDS, 0, 0);
        }

        RectTransform rectTransform = targetBlock.GetComponent<RectTransform>();

        return getBottomMostBounds(rectTransform);
    }

    public Vector3 getBottomMostPoint()
    {
        return getBottomMostPoint(getBottomMostBlock());
    }

    public Vector3 getTopMostPoint(GameObject targetBlock)
    {
        if (targetBlock == null)
        {
            return new Vector3(HIGH_BOUNDS, 0, 0);
        }

        RectTransform rectTransform = targetBlock.GetComponent<RectTransform>();

        return getTopMostBounds(rectTransform);
    }

    public Vector3 getTopMostPoint()
    {
        return getTopMostPoint(getTopMostBlock());
    }

    private Vector3 getTopMostBounds(RectTransform segmentRectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        segmentRectTransform.GetWorldCorners(worldCorners);
        Vector3 topMostPoint = new Vector3(0, LOW_BOUNDS, -1);

        foreach (Vector3 currentPoint in worldCorners)
        {
            Vector3 worldPoint = getGameBoardModel().getPlayArea().InverseTransformPoint(currentPoint);

            if (worldPoint.y > topMostPoint.y)
            {
                topMostPoint = worldPoint;
            }
        }

        return topMostPoint;
    }

    public GameObject getTopMostBlock()
    {
        float highestY = LOW_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getTopMostPoint(currentSegment);

            if (segmentWorldPos.y > highestY)
            {
                targetBlock = currentSegment;
                highestY = segmentWorldPos.y;
            }
        }

        return targetBlock;
    }

    private Vector3 getBottomMostBounds(RectTransform segmentRectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        segmentRectTransform.GetWorldCorners(worldCorners);
        Vector3 bottomMostPoint = new Vector3(0, HIGH_BOUNDS, -1);

        foreach (Vector3 currentPoint in worldCorners)
        {
            Vector3 worldPoint = getGameBoardModel().getPlayArea().InverseTransformPoint(currentPoint);

            if (worldPoint.y < bottomMostPoint.y)
            {
                bottomMostPoint = worldPoint;
            }
        }

        return bottomMostPoint;
    }

    public GameObject getBottomMostBlock()
    {
        float lowestY = HIGH_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getBottomMostPoint(currentSegment);

            if (segmentWorldPos.y < lowestY)
            {
                targetBlock = currentSegment;
                lowestY = segmentWorldPos.y;
            }
        }

        return targetBlock;
    }

    public List<GameObject> getSegments()
    {
        return mSegments;
    }

    public void addSegment(BaseBlock segment)
    {
        if (segment == null)
        {
            return;
        }

        mSegments.Add(segment.gameObject);
    }

    public Tetrino Clone()
    {
        return Clone(true);
    }
    public Tetrino Clone(bool isShadowClone)
    {
        // Spawn a new base tetrino
        GameObject cloneGameObject = Instantiate(gameObject, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
        Tetrino clone = cloneGameObject.GetComponent<Tetrino>();

        // The above instantiate makes this workflow a bit strange. It clones the object and its children, meaning
        // all of the base block segments will be also be created and cloned. They will also set the parent/child
        // references as well, which is nice, however, it will not actually setup any of our internal data
        // structures that we use like our segments list, which means the clone will have children, but its
        // segments will be empty, so we have to account for this.
        BaseBlock[] cloneBaseBlocks = clone.GetComponentsInChildren<BaseBlock>();

        // Deep clone, make sure all child segments are also copied
        for (int currentIndex = 0; currentIndex < mSegments.Count; currentIndex++)
        {
            GameObject currentGO = mSegments[currentIndex];
            BaseBlock baseBlock = currentGO.GetComponent<BaseBlock>();
            BaseBlock baseBlockClone = cloneBaseBlocks[currentIndex];

            SpriteRenderer currentSpriteRenderer = baseBlockClone.gameObject.GetComponent<SpriteRenderer>();
            currentSpriteRenderer.sprite = mShadowBlockSR.sprite;

            baseBlockClone.setX(baseBlock.getX());
            baseBlockClone.setY(baseBlock.getY());

            clone.addSegment(baseBlockClone);
        }


        // Set the parent to the play area
        getTetrinoFactory().setParentArea(clone);

        // Set position to that of the original
        RectTransform cloneTransform = cloneGameObject.GetComponent<RectTransform>();
        cloneTransform.localPosition = transform.localPosition;
        cloneTransform.eulerAngles = transform.eulerAngles;

        // Shadow clones are clones that we don't add to the model, used for processing and various calculations
        clone.setIsShadowClone(isShadowClone);
        clone.mParent = this;

        return clone;
    }

    public Tetrino getParent()
    {
        return mParent;
    }

    public bool getIsShadowClone()
    {
        return mIsShadowClone;
    }

    public void setIsShadowClone(bool isShadowClone)
    {
        this.mIsShadowClone = isShadowClone;
    }

    public void setShadow()
    {

    }

    public static bool getIsShadowEnabled()
    {
        return mIsShadowEnabled;
    }

    public static void setIsShadowEnabled(bool isEnabled)
    {
        mIsShadowEnabled = isEnabled;
    }

    public void toggleShadow()
    {
        toggleShadow(true);
    }

    private void toggleShadow(bool toggleShadow)
    {
        if (mShadow == null)
        {
            return;
        }

        foreach (GameObject currentSegment in mShadow.getSegments())
        {
            if (currentSegment == null)
            {
                continue;
            }

            BaseBlock baseBlock = currentSegment.GetComponent<BaseBlock>();

            if (baseBlock == null)
            {
                continue;
            }

            SpriteRenderer shadowSpriteRenderer = currentSegment.GetComponent<SpriteRenderer>();

            if (shadowSpriteRenderer == null)
            {
                continue;
            }

            Color currentShadowColor = shadowSpriteRenderer.color;
            currentShadowColor.a = 0f;


            if (mIsShadowEnabled)
            {
                if (toggleShadow)
                {
                    currentShadowColor.a = 0f;
                }
                else
                {
                    currentShadowColor.a = 255f;
                }
            }
            else
            {
                if (toggleShadow)
                {
                    currentShadowColor.a = 255f;
                }
                else
                {
                    currentShadowColor.a = 0f;
                }
            }

            shadowSpriteRenderer.color = currentShadowColor;
        }

        if (toggleShadow)
        {
            mIsShadowEnabled = !mIsShadowEnabled;
        }
    }

    public static BaseBlock changeBaseBlockToDestroyingBlock(GameObject baseSegment)
    {
        Vector3 exitingSegmentLocalPos = baseSegment.GetComponent<RectTransform>().localPosition;
        Vector3 existingSegmentLocalScale = baseSegment.GetComponent<RectTransform>().localScale;
        RectTransform parentTransform = baseSegment.gameObject.transform.parent.transform.GetComponent<RectTransform>();

        Destroy(baseSegment);

        GameObject destroyingBlock = Instantiate(mDestryoingBlock, new Vector3(0, 0, -1), Quaternion.identity);
        RectTransform destroyingBlockRectTransform = destroyingBlock.AddComponent<RectTransform>();
        destroyingBlock.AddComponent<BaseBlock>();
        BaseBlock baseBlock = destroyingBlock.GetComponent<BaseBlock>();
        baseBlock.setDestroyingBlock(destroyingBlock);

        destroyingBlockRectTransform.SetParent(parentTransform, true);
        destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, parentTransform.rect.width);
        destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, parentTransform.rect.height);
        destroyingBlock.transform.localPosition = exitingSegmentLocalPos;
        destroyingBlock.transform.localScale = existingSegmentLocalScale;

        Animator destroyingBlockAnimator = destroyingBlock.GetComponent<Animator>();
        destroyingBlockAnimator.speed = 2f;

        return baseBlock;
    }


    public void changeTetrinoToDestroyingBlocks()
    {
        if (mSegments == null || mSegments.Count == 0)
        {
            return;
        }

        for (int i = 0; i < mSegments.Count; i++)
        {
            GameObject currentSegment = mSegments[i];
            Vector3 exitingSegmentLocalPos = currentSegment.GetComponent<RectTransform>().localPosition;
            Vector3 existingSegmentLocalScale = currentSegment.GetComponent<RectTransform>().localScale;
            RectTransform parentTransform = currentSegment.gameObject.transform.parent.transform.GetComponent<RectTransform>();

            Destroy(currentSegment);

            GameObject destroyingBlock = Instantiate(mDestryoingBlock, new Vector3(0, 0, -1), Quaternion.identity);
            RectTransform destroyingBlockRectTransform = destroyingBlock.AddComponent<RectTransform>();

            destroyingBlockRectTransform.SetParent(parentTransform, true);
            destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, parentTransform.rect.width);
            destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, parentTransform.rect.height);
            destroyingBlock.transform.localPosition = exitingSegmentLocalPos;
            destroyingBlock.transform.localScale = existingSegmentLocalScale;

            mSegments[i] = destroyingBlock;
        }

    }

    // *** Implement in children ***
    public virtual bool insertIntoModel(Vector3 initialPosition)
    {
        return false;
    }

    public virtual void rotateTetrinoUp() { }

    public virtual void populateSegments() { }

    public virtual void adjustSegments90() { }

    public virtual void adjustSegments180() { }

    public virtual void adjustSegments270() { }

    public virtual void adjustSegments0() { }

    public virtual void adjustForNextTetrinoPosition() { }

    public virtual void adjustForStatsTetrinoPosition() { }
}
