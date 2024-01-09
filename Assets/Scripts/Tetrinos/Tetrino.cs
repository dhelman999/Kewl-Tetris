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
    private static float destroyingBlockAnimationLength = 2f;

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
            Vector3 segmentWorldPos = getBoundsForBlock(currentSegment, Vector3.left);

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

    public GameObject getBottomRightBlock()
    {
        float highestX = LOW_BOUNDS;
        float lowestY = HIGH_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getBoundsForBlock(currentSegment, Vector3.right);

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

    public GameObject getTopMostBlock()
    {
        float highestY = LOW_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getBoundsForBlock(currentSegment, Vector3.up);

            if (segmentWorldPos.y > highestY)
            {
                targetBlock = currentSegment;
                highestY = segmentWorldPos.y;
            }
        }

        return targetBlock;
    }

    public GameObject getBottomMostBlock()
    {
        float lowestY = HIGH_BOUNDS;
        GameObject targetBlock = null;

        foreach (GameObject currentSegment in mSegments)
        {
            Vector3 segmentWorldPos = getBoundsForBlock(currentSegment, Vector3.down);

            if (segmentWorldPos.y < lowestY)
            {
                targetBlock = currentSegment;
                lowestY = segmentWorldPos.y;
            }
        }

        return targetBlock;
    }

    public Vector3 getPointFromVector(Vector3 vector)
    {
        if (vector.Equals(Vector3.left))
        {
            return getBoundsForBlock(getBottomLeftBlock(), Vector3.left);
        }
        else if (vector.Equals(Vector3.right))
        {
            return getBoundsForBlock(getBottomRightBlock(), Vector3.right);
        }
        else if (vector.Equals(Vector3.up))
        {
            return getBoundsForBlock(getTopMostBlock(), Vector3.up);
        }
        else if (vector.Equals(Vector3.down))
        {
            return getBoundsForBlock(getBottomMostBlock(), Vector3.down);
        }

        return Vector3.zero;
    }

    public Vector3 getBoundsForBlock(GameObject block, Vector3 vector)
    {
        if (block == null)
        {
            if (vector.Equals(Vector3.left) || vector.Equals(Vector3.right))
            {
                return new Vector3(HIGH_BOUNDS, 0, 0);
            }
            else if (vector.Equals(Vector3.left) || vector.Equals(Vector3.right))
            {
                return new Vector3(LOW_BOUNDS, 0, 0);
            }
        }

        Vector3[] worldCorners = new Vector3[4];
        RectTransform rectTransform = block.GetComponent<RectTransform>();
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 boundedPoint = Vector3.zero;

        if (vector.Equals(Vector3.left))
        {
            new Vector3(HIGH_BOUNDS, 0, -1);
        }
        else if (vector.Equals(Vector3.right))
        {
            new Vector3(LOW_BOUNDS, 0, -1);
        }
        else if (vector.Equals(Vector3.up))
        {
            new Vector3(0, LOW_BOUNDS, -1);
        }
        else if (vector.Equals(Vector3.down))
        {
            new Vector3(0, HIGH_BOUNDS, -1);
        }

        foreach (Vector3 currentPoint in worldCorners)
        {
            Vector3 worldPoint = getGameBoardModel().getPlayArea().InverseTransformPoint(currentPoint);

            if (vector.Equals(Vector3.left))
            {
                if (worldPoint.x < boundedPoint.x)
                {
                    boundedPoint = worldPoint;
                }
            }
            else if (vector.Equals(Vector3.right))
            {
                if (worldPoint.x > boundedPoint.x)
                {
                    boundedPoint = worldPoint;
                }
            }
            else if (vector.Equals(Vector3.up))
            {
                if (worldPoint.y > boundedPoint.y)
                {
                    boundedPoint = worldPoint;
                }
            }
            else if (vector.Equals(Vector3.down))
            {
                if (worldPoint.y < boundedPoint.y)
                {
                    boundedPoint = worldPoint;
                }
            }
        }

        return boundedPoint;
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

    // Helper function to change a single base block to the block that has a destroy animation.
    public static BaseBlock changeBaseBlockToDestroyingBlock(GameObject baseSegment)
    {
        // Get the position of the original and its parent
        Vector3 exitingSegmentLocalPos = baseSegment.GetComponent<RectTransform>().localPosition;
        Vector3 existingSegmentLocalScale = baseSegment.GetComponent<RectTransform>().localScale;
        RectTransform parentTransform = baseSegment.gameObject.transform.parent.transform.GetComponent<RectTransform>();

        // We don't need the original anymore, destroy it
        Destroy(baseSegment);

        // Make a new destroying block and add a base block to it because only base blocks can enter the model
        GameObject destroyingBlock = Instantiate(mDestryoingBlock, new Vector3(0, 0, -1), Quaternion.identity);
        RectTransform destroyingBlockRectTransform = destroyingBlock.AddComponent<RectTransform>();
        destroyingBlock.AddComponent<BaseBlock>();
        BaseBlock baseBlock = destroyingBlock.GetComponent<BaseBlock>();
        baseBlock.setDestroyingBlock(destroyingBlock);

        // Set its parent and location to that of the original
        destroyingBlockRectTransform.SetParent(parentTransform, true);
        destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, parentTransform.rect.width);
        destroyingBlockRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, parentTransform.rect.height);
        destroyingBlock.transform.localPosition = exitingSegmentLocalPos;
        destroyingBlock.transform.localScale = existingSegmentLocalScale;

        // Change the animator speed as I don't see a great way to do this with the importer from Aesprite
        Animator destroyingBlockAnimator = destroyingBlock.GetComponent<Animator>();
        destroyingBlockAnimator.speed = destroyingBlockAnimationLength;

        return baseBlock;
    }

    // Change the entire Tetrino to a destroying blocks
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

    // *** Virtual methods, implement in children ***
    // Each child needs to know how to enter the model based on its composition
    public virtual bool insertIntoModel(Vector3 initialPosition) { return false; }

    // Each child needs to know how to rotate its blocks and adjust its position accordingly
    public virtual void rotateTetrinoUp() { }

    public virtual void populateSegments() { }

    public virtual void adjustSegments90() { }

    public virtual void adjustSegments180() { }

    public virtual void adjustSegments270() { }

    public virtual void adjustSegments0() { }

    // Each child also needs to know how to position itself in the next tetrino area with a fixed position
    public virtual void adjustForNextTetrinoPosition() { }

    public virtual void adjustForStatsTetrinoPosition() { }
}
