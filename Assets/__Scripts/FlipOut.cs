using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// enum contains different phases of a game turn
public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class FlipOut : MonoBehaviour
{
    static public FlipOut S;
    static public Player CURRENT_PLAYER;

    [Header("set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 0f;
    public int numStartingCards = 6;
    public float drawTimeStagger = 0.1f;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardFlipOut> drawPile;
    public List<CardFlipOut> discardPile;
    public List<Player> players;
    public CardFlipOut targetCard;
    public FloatingScore fsRun;
    public TurnPhase phase = TurnPhase.idle;

    private FlipOutLayout layout;
    private Transform layoutAnchor;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();   // Get the deck
        deck.InitDeck(deckXML.text);   // Pass DeckXML to it
        Deck.Shuffle(ref deck.cards);   // This shuffles the deck

        layout = GetComponent<FlipOutLayout>();      // get the layout
        layout.ReadLayout(layoutXML.text);          // pass LayoutXML to it

        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();
    }

    List<CardFlipOut> UpgradeCardsList(List<Card> lCD)
    {
        List<CardFlipOut> lCB = new List<CardFlipOut>();
        foreach (Card tCD in lCD)
        {
            lCB.Add(tCD as CardFlipOut);
        }
        return (lCB);
    }

    // position all the cards in the drawPile property
    public void ArrangeDrawPile()
    {
        CardFlipOut tCB;

        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;
            // Rotation should start at 0 
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4); // Order them front-to-back 
            tCB.state = CBState.drawpile;
        }
    }

    // Perform the initial game layout 
    void LayoutGame()
    {
        // Create an empty GameObject to serve as the tableau's anchor
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        // position the drawPile cards
        ArrangeDrawPile();

        // Set up the players
        Player p1;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            p1 = new Player();
            p1.handSlotDef = tSD;
            players.Add(p1);
            p1.playerNum = tSD.player;
        }
        players[0].type = PlayerType.human;         // make only the 0th player human

        CardFlipOut tCB;

        // deal 6 cards to each player
        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tCB = Draw();
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);

                players[(j + 1) % 4].AddCard(tCB);
            }
        }
        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }

    public void DrawFirstTarget()
    {
        // flip up the first card from the dP
        CardFlipOut tCB = MoveToTarget(Draw());

        // set the CardFlipOut to call cbcallback on this FlipOut when it's done
        tCB.reportFinishTo = this.gameObject;
    }

    // this callback is used by the last card to be dealt at the beginning
    public void CBCallback(CardFlipOut cb)
    {
        Utils.tr("FlipOut: CBCallback()", cb.name);
        StartGame();
    }

    public void StartGame()
    {
        // the player on the left of the human goes first
        PassTurn(1);
    }

    public void PassTurn(int num = -1)
    {
        // if no number was passed in, pick the next player
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }

        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;

            if (CheckForGameOver())
            {
                return;
            }
        }

        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;


        CURRENT_PLAYER.TakeTurn();

        Utils.tr("FlipOut: PassTurn()", "Old: " + lastPlayerNum,
                 "New: " + CURRENT_PLAYER.playerNum);
    }

    public bool CheckForGameOver()
    {
        // see if we need to reshuffle the disP into the draP
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardFlipOut cb in discardPile)
            {
                cards.Add(cb);
            }
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardsList(cards);
            ArrangeDrawPile();
        }

        // check to see if the current player has won
        if (CURRENT_PLAYER.hand.Count == 0)
        {
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);
            return (true);
        }
        return (false);
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__FlipOut_Scene_0");
    }

    // method veries that the card chosen can be played on the dP
    public bool ValidPlay(CardFlipOut cb)
    {
        // its valid if the rank is the same
        if (cb.rank == targetCard.rank)
        {
            return (true);
        }

        // its valid if the suit is the same
        if (cb.suit == targetCard.suit)
        {
            return (true);
        }

        // if neither
        return (false);
    }


    // this makes a new card the target
    public CardFlipOut MoveToTarget(CardFlipOut tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
        {
            MoveToDiscard(targetCard);
        }

        targetCard = tCB;

        return (tCB);
    }

    // this makes a new card the target
    public CardFlipOut MoveToDiscard(CardFlipOut tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

        return (tCB);
    }

    // the draw function will pull asingle card from the drawPile and return it
    public CardFlipOut Draw()
    {
        CardFlipOut cd = drawPile[0];

        if (drawPile.Count == 0)
        {
            ArrangeDrawPile();
            // show the cards moving to drawPile
            float t = Time.time;
            foreach (CardFlipOut tCB in drawPile)
            {
                tCB.transform.localPosition = layout.discardPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.02f;
                tCB.state = CBState.toDrawpile;
                tCB.eventualSortLayer = "0";
            }
        }

        drawPile.RemoveAt(0);
        return (cd);
    }

    public void CardClicked(CardFlipOut tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human)
        {
            return;
        }

        if (phase == TurnPhase.waiting)
        {
            return;
        }

        switch (tCB.state)
        {
            case CBState.drawpile:
                // draw the top card
                CardFlipOut cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr("FlipOut: CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;

            case CBState.hand:
                // check to see whether the card is valid
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("FlipOut:CardClicked()", "Play", tCB.name, targetCard.name + " is target");
                    phase = TurnPhase.waiting;
                }
                else
                {
                    // ignore but report when tried
                    Utils.tr("FlipOut.CardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
                }
                break;
        }
    }
}
