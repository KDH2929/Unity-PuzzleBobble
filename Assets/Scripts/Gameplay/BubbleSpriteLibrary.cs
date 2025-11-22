using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BubbleSpriteLibrary", menuName = "Bubble/SpriteLibrary")]
public class BubbleSpriteLibrary : ScriptableObject
{
    public Sprite redSprite;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite purpleSprite;

    private Dictionary<BubbleColor, Sprite> dict;

    public Sprite GetSprite(BubbleColor color)
    {
        if (dict == null)
        {
            dict = new Dictionary<BubbleColor, Sprite>
            {
                { BubbleColor.Red, redSprite },
                { BubbleColor.Blue, blueSprite },
                { BubbleColor.Green, greenSprite },
                { BubbleColor.Yellow, yellowSprite },
                { BubbleColor.Purple, purpleSprite },
            };
        }

        return dict[color];
    }
}