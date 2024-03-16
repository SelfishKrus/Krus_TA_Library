using UnityEngine;

namespace Adobe.Substance
{
    [System.Serializable]
    public class SubstanceOutputTexture
    {
        [SerializeField]
        public int Index;

        [SerializeField]
        public int VirtualOutputIndex;

        [SerializeField]
        public SubstanceOutputDescription Description;

        [SerializeField]
        public Texture2D OutputTexture;

        [SerializeField]
        public bool sRGB;

        [SerializeField]
        public bool IsVirtual;

        [SerializeField]
        public bool IsAlphaAssignable;

        [SerializeField]
        public string AlphaChannel;

        [SerializeField]
        public bool InvertAssignedAlpha;

        [SerializeField]
        public uint Flags = 0;

        [SerializeField]
        public string MaterialTextureTarget;

        public SubstanceOutputTexture(SubstanceOutputDescription description, string unityTextureName)
        {
            Index = description.Index;
            Description = description;
            MaterialTextureTarget = unityTextureName;

            if (!string.IsNullOrEmpty(description.Channel))
                IsAlphaAssignable = !string.Equals(description.Channel, "normal", System.StringComparison.OrdinalIgnoreCase);
            else
                IsAlphaAssignable = false;

            IsVirtual = false;
            sRGB = false;
            OutputTexture = null;
            AlphaChannel = string.Empty;
        }
    }
}