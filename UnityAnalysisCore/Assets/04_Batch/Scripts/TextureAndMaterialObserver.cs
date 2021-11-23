using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TextureAndMaterialObserver : MonoBehaviour
{
    private Image img;
    private GUIStyle uIStyle;

    private void Awake()
    {
        this.img = GetComponent<Image>();
        uIStyle = new GUIStyle();
        uIStyle.fontSize = 32;
        uIStyle.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        var materialID = img.material.GetInstanceID();
        var textureID = img.mainTexture.GetInstanceID();
        var position = transform.position;
        Rect rect = new Rect(new Vector2(position.x - 50,1334 - position.y), new Vector2(100, 100));
        GUI.Label(rect, $"{materialID}_{textureID}", uIStyle);
    }
}
