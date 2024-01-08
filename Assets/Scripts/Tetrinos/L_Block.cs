using System.Collections.Generic;
using UnityEngine;

public class L_Block : Tetrino
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

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET + (BaseBlock.BLOCK_SIZE * 2), 0, -1), Quaternion.identity) as GameObject;
        newSegment.transform.SetParent(transform, false);
        mSegments.Add(newSegment);

        newSegment = Instantiate(mBaseSegment, new Vector3(X_OFFSET + (BaseBlock.BLOCK_SIZE * 2), BaseBlock.BLOCK_SIZE, -1), Quaternion.identity) as GameObject;
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
            mGameBoardModel.getModel(baseX + 1, baseY) != null ||
            mGameBoardModel.getModel(baseX + 2, baseY) != null)
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

        if (!mGameBoardModel.setModel(mSegments[2], baseX + 2, baseY))
        {
            return false;
        }

        if (!mGameBoardModel.setModel(mSegments[3], baseX + 2, baseY + 1))
        {
            return false;
        }

        return insertSuccessful;
    }

    public override void rotateTetrinoUp()
    {
        Vector3 rotation = transform.eulerAngles;
        Vector3 desiredRotation = new Vector3(0, 0, -90);
        transform.eulerAngles = rotation + desiredRotation;

        if (Mathf.Round(transform.eulerAngles.z) == 90f)
        {
            transform.localPosition += new Vector3(-40, -80, 0);
            adjustSegments90();
        }
        else if (Mathf.Round(transform.eulerAngles.z) == 180f)
        {
            transform.localPosition += new Vector3(120, -40, 0);
            adjustSegments180();
        }
        else if (Mathf.Round(transform.eulerAngles.z) == 270f)
        {
            transform.localPosition += new Vector3(0, 120, 0);
            adjustSegments270();
        }
        else
        {
            transform.localPosition += new Vector3(-80, 0, 0);
            adjustSegments0();
        }
    }

    public override void adjustSegments90()
    {
        BaseBlock baseBlock = mSegments[0].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);
        baseBlock.setY(baseBlock.getY() - 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 1);
        baseBlock.setY(baseBlock.getY() + 1);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setY(baseBlock.getY() + 2);
    }

    public override void adjustSegments180()
    {
        BaseBlock baseBlock = mSegments[0].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 2);
        baseBlock.setY(baseBlock.getY() - 1);

        baseBlock = mSegments[1].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setY(baseBlock.getY() + 1);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);
    }

    public override void adjustSegments270()
    {
        BaseBlock baseBlock = mSegments[0].GetComponent<BaseBlock>();
        baseBlock.setY(baseBlock.getY() + 2);

        baseBlock = mSegments[1].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);
        baseBlock.setY(baseBlock.getY() + 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 2);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);
        baseBlock.setY(baseBlock.getY() - 1);
    }

    public override void adjustSegments0()
    {
        BaseBlock baseBlock = mSegments[0].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() - 1);

        baseBlock = mSegments[1].GetComponent<BaseBlock>();
        baseBlock.setY(baseBlock.getY() - 1);

        baseBlock = mSegments[2].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 1);
        baseBlock.setY(baseBlock.getY() - 2);

        baseBlock = mSegments[3].GetComponent<BaseBlock>();
        baseBlock.setX(baseBlock.getX() + 2);
        baseBlock.setY(baseBlock.getY() - 1);
    }

    public override void adjustForNextTetrinoPosition()
    {
        transform.localPosition += new Vector3(55f, 95f, 0);
    }

    public override void adjustForStatsTetrinoPosition()
    {
        transform.localPosition += new Vector3(80f, 570f, 0);
    }
}
