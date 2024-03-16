using UnityEngine;

namespace Adobe.Substance
{
	[PreferBinarySerialization]
	public class SubstanceFileRawData : ScriptableObject
	{
		[SerializeField]
		[HideInInspector]
		public byte[] FileContent = default;
	}
}