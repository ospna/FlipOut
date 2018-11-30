using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// an enum to track the possible states of a FloatingScore
public enum eFSState
{
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    // the score property sets both _score and scoreString
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");        // NO adds commas to the number
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;         // points for movement
    public List<float> fontSizes;            // points for font scaling
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;       // uses Easing in Utils.cs

    // the GO that will recieve the SendMessage when this is done moving
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private Text txt;

    // Set up the FloatingScore and movement
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        if(ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre;          // set it to the pre state, ready to start moving
    }

    public void FSCallback(FloatingScore fs)
    {
        // when this callback is called by SendMessage, add the score from the calling FloatingScore
        score += fs.score;
    }
	
	// Update is called once per frame
	void Update ()
    {
		// if its not moving, just return
        if(state == eFSState.idle)
        {
            return;
        }

        // Get u from the current time and duration. u ranges from 0 to 1
        float u = (Time.time - timeStart) / timeDuration;
        float uC = Easing.Ease(u, easingCurve);         // use easing class to curve the u value

        if(u < 0)                       // we should not move
        {
            state = eFSState.pre;
            txt.enabled = false;        // hide the score
        }
        else
        {
            if (u >= 1)                  // we are done moving
            {
                uC = 1;                 // set uC = 1 so we dont overshoot
                state = eFSState.post;

                if (reportFinishTo != null)      // if theres a callback GO
                {
                    // use SendMessage to call the FSCallback method with this as a parameter
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(gameObject);
                }
                else
                {
                    // if theres noting to callback. dont destroy just stay still.
                    state = eFSState.idle;
                }
            }
            else
            {
                // 0 <= u < 1, then it is active and moving
                state = eFSState.active;
                txt.enabled = true;         // show the score;
            }

            Vector2 pos = Utils.Bezier(uC, bezierPts);
            rectTrans.anchorMin = rectTrans.anchorMax = pos;    // position UI objs relative to total size of screen

            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
	}
}
