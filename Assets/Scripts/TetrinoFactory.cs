using TMPro;
using UnityEngine;
using static Tetrino;

public class TetrinoFactory : MonoBehaviour
{
    // Don't change this unless you also change the name in the unity hierarchy
    public static string TETRINO_FACTORY_NAME = "TetrinoFactory";

    public GameObject LBlockRef;
    public GameObject JBlockRef;
    public GameObject IBlockRef;
    public GameObject OBlockRef;
    public GameObject TBlockRef;
    public GameObject SBlockRef;
    public GameObject ZBlockRef;

    public RectTransform mPlayArea;
    public RectTransform mNextForeground;
    public RectTransform mStatsForeground;

    public TextMeshProUGUI TStats;
    public TextMeshProUGUI LStats;
    public TextMeshProUGUI OStats;
    public TextMeshProUGUI IStats;
    public TextMeshProUGUI SStats;
    public TextMeshProUGUI JStats;
    public TextMeshProUGUI ZStats;

    private int TStatsNum;
    private int LStatsNum;
    private int OStatsNum;
    private int IStatsNum;
    private int SStatsNum;
    private int JStatsNum;
    private int ZStatsNum;

    private Tetrino mNextTetrino;
    private TetrinoType mNextTetrinoType = TetrinoType.INVALID;

    private void Start()
    {
        createStatsTetrino(TetrinoType.T_Block);
        createStatsTetrino(TetrinoType.L_Block);
        createStatsTetrino(TetrinoType.O_Block);
        createStatsTetrino(TetrinoType.I_Block);
        createStatsTetrino(TetrinoType.S_Block);
        createStatsTetrino(TetrinoType.J_Block);
        createStatsTetrino(TetrinoType.Z_Block);

        mNextTetrinoType = (TetrinoType)Random.Range(0, 7);
        setNextTetrino();

        resetStats();
    }

    public Tetrino createRandomTetrino()
    {
        return createTetrino(mNextTetrinoType);
    }

    public Tetrino createTetrino(TetrinoType tetrinoType)
    {
        return createTetrino(tetrinoType, true);
    }

    public Tetrino createTetrino(TetrinoType tetrinoType, bool shouldSetNextTetrino)
    {
        return createTetrino(tetrinoType, shouldSetNextTetrino, true);
    }

    public Tetrino createTetrino(TetrinoType tetrinoType, bool shouldSetNextTetrino, bool shouldSetParentArea)
    {
        Tetrino newTetrino = null;

        switch (tetrinoType)
        {
            case TetrinoType.L_Block:
                newTetrino = Instantiate(LBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.J_Block:
                newTetrino = Instantiate(JBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.I_Block:
                newTetrino = Instantiate(IBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.O_Block:
                newTetrino = Instantiate(OBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.T_Block:
                newTetrino = Instantiate(TBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.S_Block:
                newTetrino = Instantiate(SBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;
            case TetrinoType.Z_Block:
                newTetrino = Instantiate(ZBlockRef, new Vector3(0, 0, -1), Quaternion.identity).GetComponent<Tetrino>();
                break;

            default:
                Debug.Log("Unknown tetrino type: " + tetrinoType.ToString());
                break;
        }

        if (newTetrino != null)
        {
            if (shouldSetParentArea)
            {
                setParentArea(newTetrino);
            }

            if (shouldSetNextTetrino)
            {
                incrementStats(tetrinoType);
                mNextTetrinoType = (TetrinoType)Random.Range(0, 7);
                setNextTetrino();
            }
        }

        return newTetrino;
    }

    public Tetrino createStatsTetrino(TetrinoType tetrinoType)
    {
        Tetrino newTetrino = createTetrino(tetrinoType, false, false);
        newTetrino.populateSegments();
        setParentArea(newTetrino, mStatsForeground);
        newTetrino.adjustForStatsTetrinoPosition();

        return newTetrino;
    }

    private void setNextTetrino()
    {
        if (mNextTetrino != null)
        {
            mNextTetrino.destroy();
        }

        mNextTetrino = createTetrino(mNextTetrinoType, false, false);
        mNextTetrino.populateSegments();
        setParentArea(mNextTetrino, mNextForeground);
        mNextTetrino.adjustForNextTetrinoPosition();
    }

    public void setParentArea(Tetrino tetrino)
    {
        setParentArea(tetrino, null);
    }

    public void setParentArea(Tetrino tetrino, RectTransform parent)
    {
        if (parent == null)
        {
            parent = mPlayArea;
        }

        // Set the parent to the play area
        RectTransform tetrinoTransform = tetrino.GetComponent<RectTransform>();

        // Set to the correct location of the original
        tetrinoTransform.SetParent(parent, false);
        tetrinoTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, tetrinoTransform.rect.width);
        tetrinoTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, tetrinoTransform.rect.height);
    }

    private void incrementStats(TetrinoType tetrinoType)
    {
        switch (tetrinoType)
        {
            case TetrinoType.L_Block:
                ++LStatsNum;
                LStats.text = LStatsNum.ToString();
                break;
            case TetrinoType.J_Block:
                ++JStatsNum;
                JStats.text = JStatsNum.ToString();
                break;
            case TetrinoType.I_Block:
                ++IStatsNum;
                IStats.text = IStatsNum.ToString();
                break;
            case TetrinoType.O_Block:
                ++OStatsNum;
                OStats.text = OStatsNum.ToString();
                break;
            case TetrinoType.T_Block:
                ++TStatsNum;
                TStats.text = TStatsNum.ToString();
                break;
            case TetrinoType.S_Block:
                ++SStatsNum;
                SStats.text = SStatsNum.ToString();
                break;
            case TetrinoType.Z_Block:
                ++ZStatsNum;
                ZStats.text = ZStatsNum.ToString();
                break;

            default:
                Debug.Log("Unknown tetrino type: " + tetrinoType.ToString());
                break;
        }
    }

    private void resetStats()
    {
        TStatsNum = 0;
        TStats.text = "0";
        LStatsNum = 0;
        LStats.text = "0";
        OStatsNum = 0;
        OStats.text = "0";
        IStatsNum = 0;
        IStats.text = "0";
        SStatsNum = 0;
        SStats.text = "0";
        JStatsNum = 0;
        JStats.text = "0";
        ZStatsNum = 0;
        ZStats.text = "0";
    }

    public void restart()
    {
        if (mNextTetrino != null)
        {
            mNextTetrino.destroy();
        }

        resetStats();
    }
}
