using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Heaton.WorldGen
{
    public class MapDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RawImage mapPreviewRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        public void DrawTexture(Texture2D texture)
        {
            mapPreviewRenderer.texture = texture;
            mapPreviewRenderer.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
        }

        public void DrawMesh(MeshData meshData, Texture2D texture)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }
}
