using System.Collections.Generic;
using UnityEngine;

public class I_Block : Tetrino
{
    private float X_OFFSET = BaseBlock.BLOCK_SIZE / 2;

    public override void populateSegments()
    {
        mSegments = new List<GameObject>();

        GameObject newSegment;
        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, 0, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, BaseBlock.BLOCK_SIZE, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, BaseBlock.BLOCK_SIZE * 2, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, BaseBlock.BLOCK_SIZE * 3, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);
    }

    public override bool insertIntoModel(Vector3 initialPosition)
    {
        populateSegments();

        bool insertSuccessful = true;

        int baseX = (int)initialPosition.x / BaseBlock.BLOCK_SIZE;
        int baseY = (int)initialPosition.y / BaseBlock.BLOCK_SIZE;

        mGameBoardModel = getGameBoardModel();

        if (mGameBoardModel.getModel(baseX, baseY) != null ||
            mGameBoardModel.getModel(baseX, baseY + 1) != null ||
            mGameBoardModel.getModel(baseX, baseY + 2) != null ||
            mGameBoardModel.getModel(baseX, baseY + 3) != null)
        {
            return false;
        }


        if (!mGameBoardModel.setModel(mSegments[0], baseX, baseY))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[1], baseX, baseY + 1))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[2], baseX, baseY + 2))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[3], baseX, baseY + 3))
        {
            return false;
        }

        return insertSuccessful;
    }

    public override void rotateTetrinoUp()
    {
        Vector3 rotation = transform.eulerAngles;
        Vector3 desiredRotation = new Vector3(0, 0, -90);
        Vector3 positionOffset = new Vector3(0, BaseBlock.BLOCK_SIZE, 0);

        if (Mathf.Round(transform.eulerAngles.z) == 0)
        {
            transform.eulerAngles = rotation + desiredRotation;
            transform.localPosition += positionOffset;
            adjustSegments0();
        }
        else
        {
            transform.eulerAngles = rotation - desiredRotation;
            transform.localPosition -= positionOffset;
            adjustSegments90();
        }
    }

    public override void adjustSegments0()
    {
        BaseBlock baseBlock = mSegments[1].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 1);
        baseBlock.setY(baseBlock.getY() - 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 2);
        baseBlock.setY(baseBlock.getY() - 2);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 3);
        baseBlock.setY(baseBlock.getY() - 3);
    }

    public override void adjustSegments90()
    {
        BaseBlock baseBlock = mSegments[1].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);
        baseBlock.setY(baseBlock.getY() + 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 2);
        baseBlock.setY(baseBlock.getY() + 2);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 3);
        baseBlock.setY(baseBlock.getY() + 3);
    }

    public override void adjustForNextTetrinoPosition()
    {
        transform.localPosition += new Vector3(95f, 40f, 0);
    }

    public override void adjustForStatsTetrinoPosition()
    {
        transform.localPosition += new Vector3(60f, 410f, 0);
        transform.eulerAngles = new Vector3(0, 0, -90);
    }
}
