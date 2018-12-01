using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();    // unused
    public float rot;
    public string type = "slot";
    public Vector2 stagger;
    public int player;      // player # of a hand
    public Vector3 pos;     // pos derived from x, y, & multiplier
}

public class FlipOutLayout : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;      // sets the spacing of the tableau's 

    // SlotDef refs
    public List<SlotDef> slotDefs;  // all the SlotDefs for Row0-Row3
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;

    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        // read in the multiplier, which sets card spacing
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        // read in the slots
        SlotDef tSD;
        // slotsX is used as a shortcut to all <slot>s
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();        // create a new SlotDef instance
            if (slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                tSD.type = "slot";
            }

            // various attributes are parsed in numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            // sorting layers
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch (tSD.type)
            {
                case "slot":
                    // ignore slots of type "slot"
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;

                case "discaredpile":
                    discardPile = tSD;
                    break;

                case "target":
                    target = tSD;
                    break;

                case "hand":
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}
