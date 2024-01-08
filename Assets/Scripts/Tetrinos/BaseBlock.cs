using UnityEngine;

public class BaseBlock : MonoBehaviour
{
    public static int BLOCK_SIZE = 40;

    private int mXPos = -1;
    private int mYPos = -1;

    private GameObject mDestroyingBlock;

    public GameObject getDestroyingBlock()
    {
        return mDestroyingBlock;
    }

    public void setDestroyingBlock(GameObject destryingBlock)
    {
        mDestroyingBlock = destryingBlock;
    }

    public void destroy()
    {
        Destroy(mDestroyingBlock);
    }

    public int getX() { return mXPos; }
    public int getY() { return mYPos; }

    public void setX(int xPos)
    {
        mXPos = xPos;
    }

    public void setY(int yPos)
    {
        mYPos = yPos;
    }

    public bool isAtSameLocation(BaseBlock otherBlock)
    {
        if (otherBlock == null)
        {
            return false;
        }

        bool isAtSameLocation = false;

        if (mXPos == otherBlock.mXPos && mYPos == otherBlock.mYPos)
        {
            isAtSameLocation = true;
        }

        return isAtSameLocation;
    }
}
