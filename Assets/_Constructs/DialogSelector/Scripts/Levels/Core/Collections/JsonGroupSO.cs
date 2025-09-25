
using UnityEngine;

[CreateAssetMenu(fileName = "NewJsonGroup", menuName = "Storage/Json Group")]
public class JsonGroupSO : ScriptableObject {
	public string groupName;

	[SerializeField]
	public AssetReferences.Json Asset;
}