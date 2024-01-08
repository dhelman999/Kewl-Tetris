using System.Collections.Generic;
using UnityEngine;

public class O_Block : Tetrino
{
    private float X_OFFSET = BaseBlock.BLOCK_SIZE / 2;

    public override void populateSegments()
    {
        mSegments = new List<GameObject>();

        GameObject newSegment;
        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, 0, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET + BaseBlock.BLOCK_SIZE, 0, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET, BaseBlock.BLOCK_SIZE, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET + BaseBlock.BLOCK_SIZE, BaseBlock.BLOCK_SIZE, -1), Quaternion.identity) as GameObject;
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
            mGameBoardModel.getModel(baseX + 1, baseY) != null ||
            mGameBoardModel.getModel(baseX, baseY + 1) != null ||
            mGameBoardModel.getModel(baseX + 1, baseY + 1) != null)
        {
            return false;
        }


        if (!mGameBoardModel.setModel(mSegments[0], baseX, baseY))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[1], baseX + 1, baseY))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[2], baseX, baseY + 1))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[3], baseX + 1, baseY + 1))
        {
            return false;
        }

        return insertSuccessful;
    }

    public override void rotateTetrinoUp() { }

    public override void adjustForNextTetrinoPosition()
    {
        transform.localPosition += new Vector3(70f, 90f, 0);
    }

    public override void adjustForStatsTetrinoPosition()
    {
        transform.localPosition += new Vector3(80f, 450f, 0);
    }
}
