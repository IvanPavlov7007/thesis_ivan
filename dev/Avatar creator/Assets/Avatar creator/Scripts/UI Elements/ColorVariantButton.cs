using UnityEngine.UI;
using UnityEngine;
using UMA;

/// <summary>
/// A button that changes specific sharedColor(UMA) of humanoids to a given color
/// </summary>
public class ColorVariantButton : MonoBehaviour
{
    [SerializeField]
    Button button;
    HumanoidAvatarManager humanoidAvatarManager;
    OverlayColorData color;
    string sharedColorName;

    public void Setup(string sharedColorName, OverlayColorData color, HumanoidAvatarManager humanoidAvatarManager)
    {
        this.color = color;
        this.sharedColorName = sharedColorName;
        this.humanoidAvatarManager = humanoidAvatarManager;

        var block = button.colors;
        block.normalColor = color.color;
        block.pressedColor = Color.Lerp(color.color, block.pressedColor, 0.5f);

        button.colors = block;

    }

    public void OnClick()
    {
        humanoidAvatarManager.SetColorAlbedo(sharedColorName, color);
    }


}
