using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PaintableSurface : MonoBehaviour
{
    private const float COLOR_IMPACT_ON_PIXEL_DRAW = 0.25f;  // Impact of color when drawing a pixel on the surface.
    private const int TEXTURE_SIZE_PER_UNIT = 96;  // Size of the texture per unit.

    [SerializeField] private Material _paintableMaterial;  // Material used for painting on the surface.
    [SerializeField] private Renderer _objectRenderer;  // Renderer component of the painted object.
    [SerializeField] private Color _emissionColor;  // Emission color of the painted object.

    private Texture _texture;  // Texture used for painting on the surface.
    private Texture2D _texture2D;  // Texture2D representation of the painted texture.

    private void Awake()
    {
        _objectRenderer.material = _paintableMaterial;  // Assign the paintable material to the object's renderer.

        _texture2D = new Texture2D(
            TEXTURE_SIZE_PER_UNIT * (int)transform.lossyScale.x,  // Create a new Texture2D with dimensions based on the scale of the object.
            TEXTURE_SIZE_PER_UNIT * (int)transform.lossyScale.y);

        NullifyTexture();  // Initialize the texture with black pixels.

        _objectRenderer.material.SetTexture("_PaintedTexture", _texture2D);  // Set the painted texture as a texture parameter in the material.
        _objectRenderer.material.SetColor("_EmissionColor", _emissionColor);  // Set the emission color in the material.
    }

    public void DrawPixelOnRaycastHit(RaycastHit hit)
    {
        var pixelUV = hit.textureCoord;  // UV coordinates of the hit point on the object's texture.
        var pixelPoint = new Vector2(pixelUV.x * _texture2D.width, pixelUV.y * _texture2D.height);  // Convert UV coordinates to pixel coordinates.
        var tiling = _objectRenderer.material.GetTextureScale("_PaintedTexture");  // Get the tiling value of the painted texture from the material.

        _texture2D.SetPixel((int)(pixelPoint.x * tiling.x), (int)(pixelPoint.y * tiling.y), Color.white);  // Draw a white pixel at the corresponding pixel coordinates on the texture.
    }

    public void ApplyTextureChanges()
    {
        _texture2D.Apply();  // Apply the changes made to the texture.
    }

    private void NullifyTexture()
    {
        for (var i = 0; i < _texture2D.width; i++)
        {
            for (var j = 0; j < _texture2D.height; j++)
            {
                _texture2D.SetPixel(i, j, Color.black);  // Set each pixel in the texture to black.
            }
        }

        _texture2D.Apply();  // Apply the changes made to the texture.
    }
}
