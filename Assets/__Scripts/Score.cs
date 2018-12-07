using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    /*
     * score
click on card color to score (can be any in run)
find number of adjacent cards with that color that includes card clicked on 
if less than 4 adjacent, do not allow
highlight cards
remove cards from array, move to scoring pile
update score
for each empty array slot, draw a new card

*/
	// Use this for initialization
	void Start () {
		
	}
	

    public void ScoreAction()
    {
        print("Score");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
