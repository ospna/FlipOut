using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
public class TurnLight : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
    {
        transform.position = Vector3.back * 3;

        if (FlipOut.CURRENT_PLAYER == null)
        {
            return;
        }

        transform.position += FlipOut.CURRENT_PLAYER.handSlotDef.pos;
	}
}
    */