namespace Adobe.Substance
{
    [System.Serializable]
    public class SubstanceOutputDescription
    {
        [UnityEngine.SerializeField]
        public string Identifier;

        [UnityEngine.SerializeField]
        public string Label;

        [UnityEngine.SerializeField]
        public int Index;

        [UnityEngine.SerializeField]
        public SubstanceValueType Type;

        [UnityEngine.SerializeField]
        public string Channel;

        public override string ToString()
        {
            return $"Identifier: {Identifier}\n" +
                   $"Label:{Label}\n" +
                   $"Index:{Index}\n" +
                   $"Type:{Type}\n" +
                   $"Channel{Channel}\n";
        }
    }
}