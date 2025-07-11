using UnityEngine;

[CreateAssetMenu(fileName = "DialogSO_New", menuName = "Anarchy/DialogSO")]
public class DialogSO : ScriptableObject {
	public string Keyword;
	[TextArea]
	public string Dialog;
}
