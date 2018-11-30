using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

    [Header("Set Dynamically")]
    public string    col;
	public int       rank;
	public Color     color = Color.black;
	public string    colS = "Black";  // or "Red"
	
	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	
	public GameObject back;  // back of card;
	public CardDefinition def;  // from DeckXML.xml		

    // List of the SpriteRenderer Components of this GO and its children
    public SpriteRenderer[] spriteRenderers;

	// Use this for initialization
	void Start ()
    {
        SetSortOrder(0);
	
	}

    // if spriteRenderers is not yet defined, this fucntions defines it
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // sets the sortingLayerName on all SpriteRenderer components
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    // sets the sortingOrder of all SpriteRenderer components
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        // iterate thru all the spriteRenderers as tSR
        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            if(tSR.gameObject == this.gameObject)
            {
                tSR.sortingOrder = sOrd;
                continue;
            }

            // each of the children of this Go are named, switch based on the names
            switch(tSR.gameObject.name)
            {
                case "back":
                    // set it to the highest layer to cover the other sprites
                    tSR.sortingOrder = sOrd + 2;
                    break;

                case "face":
                default:
                    // set it to the middle layer to be above the bg
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }

        set
        {
            back.SetActive(!value);
        }
    }

    virtual public void OnMouseUpAsButton()
    {
        print(name);
    }

    // Update is called once per frame
    void Update () {
	
	}
} // class Card

/**holds infomation about the decorators and pips described in the XML.
[System.Serializable]
public class Decorator{
	public string	type;			// For card pips, tyhpe = "pip"
	public Vector3	loc;			// location of sprite on the card
	public bool		flip = false;	//whether to flip vertically
	public float 	scale = 1.0f;
}
    */

// holds information about where the sprites arte to be positioned on each rank of card
[System.Serializable]
public class CardDefinition{
	public string	color;	//sprite to use for color cards
}
