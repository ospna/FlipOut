using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

    [Header("Set in Inspector")]
    /**
   
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
    */

    //Colors
    public Sprite blueCard;
    public Sprite greenCard;
    public Sprite purpleCard;
    public Sprite redCard;
    public Sprite yellowCard;

    public Sprite[] faceSprites;

    public GameObject prefabSprite;
    public GameObject prefabCard;


    /**
     * public GameObject prefabBlue;
    public GameObject prefabGreen;
    public GameObject prefabPurple;
    public GameObject prefabRed;
    public GameObject prefabYellow;
    */

    [Header("Set Dynamically")]

	public PT_XMLReader					xmlr;
	// add from p 569
	public List<string>					cardNames;
	public List<Card>					cards;
	public List<CardDefinition>			cardDefs;
	public Transform					deckAnchor;
	public Dictionary<string, Sprite>	dictSuits;


	// called by Prospector when it is ready
	public void InitDeck(string deckXMLText) {
		// from page 576
		if( GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		// init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"B", blueCard},
			{"G", greenCard},
			{"P", purpleCard},
			{"R", redCard},
            {"Y", yellowCard}
		};
		
		
		
		// -------- end from page 576
		//ReadDeck (deckXMLText);
		MakeCards();
	}


	// ReadDeck parses the XML file passed to it into Card Definitions
	public void ReadDeck(string deckXMLText)
	{
		xmlr = new PT_XMLReader ();
		xmlr.Parse (deckXMLText);

		// print a test line
		string s = "xml[0] decorator [0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		print (s);
		
        /**
		//Read decorators for all cards
		// these are the small numbers/suits in the corners
		decorators = new List<Decorator>();
		// grab all decorators from the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i=0; i<xDecos.Count; i++) {
			// for each decorator in the XML, copy attributes and set up location and flip if needed
			deco = new Decorator();
			deco.type = xDecos[i].att ("type");
			deco.flip = (xDecos[i].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			deco.scale = float.Parse (xDecos[i].att("scale"));
			deco.loc.x = float.Parse (xDecos[i].att("x"));
			deco.loc.y = float.Parse (xDecos[i].att("y"));
			deco.loc.z = float.Parse (xDecos[i].att("z"));
			decorators.Add (deco);
		}
        */
		
		// read pip locations for each card rank
		// read the card definitions, parse attribute values for pips
		cardDefs = new List<CardDefinition>();
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		
		for (int i=0; i<xCardDefs.Count; i++) {
			// for each carddef in the XML, copy attributes and set up in cDef
			CardDefinition cDef = new CardDefinition();
			//cDef.rank = int.Parse(xCardDefs[i].att("rank"));
			
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {			
				for (int j = 0; j < xPips.Count; j++) {
					//deco = new Decorator();
					deco.type = "pip";
					deco.flip = (xPips[j].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
					
					deco.loc.x = float.Parse (xPips[j].att("x"));
					deco.loc.y = float.Parse (xPips[j].att("y"));
					deco.loc.z = float.Parse (xPips[j].att("z"));
					if(xPips[j].HasAtt("scale") ) {
						deco.scale = float.Parse (xPips[j].att("scale"));
					}
				} // for j
			}// if xPips
			
			// if it's a colored card, map the proper sprite
			// foramt is ##A, where ## in 11, 12, 13 and A is letter indicating suit
			if (xCardDefs[i].HasAtt("colr")){
				cDef.color = xCardDefs[i].att ("color");
			}
			cardDefs.Add (cDef);
		} // for i < xCardDefs.Count
	} // ReadDeck
    
	/*
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank
    */

	public void MakeCards() {
		// stub Add the code from page 577 here
		cardNames = new List<string>();
		string[] letters = new string[] {"B","G","P","R","Y"};
		foreach (string s in letters) {
			for (int i = 0; i < 90; i++) {
				cardNames.Add(s + (i + 1));
			}
		}
		
		// list of all Cards
		cards = new List<Card>();
		
		// temp variables
		Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;  // so tempted to make a D&D ref here...
		
		for (int i = 0; i < cardNames.Count; i++) {
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			cgo.transform.parent = deckAnchor;
			Card card = cgo.GetComponent<Card>();
			
			cgo.transform.localPosition = new Vector3(i%13*3, i/13*4, 0);   // stacks the cards in a nice row
			
			card.name = cardNames[i];
			card.col = card.name[0].ToString();
			card.rank = int.Parse (card.name.Substring (1));
			
			if (card.col =="B") {
				card.colS = "Blue";
				card.color = Color.blue;
			}
            else if (card.col == "G")
            {
                card.colS = "Green";
                card.color = Color.green;
            }
            else if (card.col == "P")
            {
                card.colS = "Purple";
                card.color = Color.magenta;
            }
            else if (card.col == "R")
            {
                card.colS = "Red";
                card.color = Color.red;
            }
            else if (card.col == "Y")
            {
                card.colS = "Yellow";
                card.color = Color.yellow;
            }

			//Handle colored cards
			if (card.def.color != "") {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				
				tS = GetColor(card.def.color+card.col);
				tSR.sprite = tS;
				tSR.sortingOrder = 1;
				tGO.transform.parent=card.transform;
				tGO.transform.localPosition = Vector3.zero;  // slap it smack dab in the middle
				tGO.name = "color";
			}

			tGO = Instantiate(prefabSprite) as GameObject;
			tSR = tGO.GetComponent<SpriteRenderer>();
			//tSR.sprite = cardBack;
			tGO.transform.SetParent(card.transform);
			tGO.transform.localPosition=Vector3.zero;
			tSR.sortingOrder = 2;
			tGO.name = "back";
			card.back = tGO;
			card.faceUp = false;
			
			cards.Add (card);
		} // for all the Cardnames	
	} // makeCards
	
	//Find the proper face card
	public Sprite GetColor(string faceS) {
		foreach (Sprite tS in faceSprites) {
			if (tS.name == faceS) {
				return (tS);
			}
		}//foreach	
		return (null);  // couldn't find the sprite (should never reach this line)
	 }// getFace 

	 static public void Shuffle(ref List<Card> oCards)
	 {
	 	List<Card> tCards = new List<Card>();

	 	int ndx;   // which card to move

	 	while (oCards.Count > 0) 
	 	{
	 		// find a random card, add it to shuffled list and remove from original deck
	 		ndx = Random.Range(0,oCards.Count);
	 		tCards.Add(oCards[ndx]);
	 		oCards.RemoveAt(ndx);
	 	}

	 	oCards = tCards;

	 	//because oCards is a ref parameter, the changes made are propogated back
	 	//for ref paramters changes made in the function persist.


	 }


} // Deck class
