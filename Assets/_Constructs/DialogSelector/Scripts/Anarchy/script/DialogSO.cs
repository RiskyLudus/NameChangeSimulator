using UnityEngine;

[CreateAssetMenu(fileName = "DialogSO_New", menuName = "Anarchy/DialogSO")]
public class DialogSO : ScriptableObject, IHasKeyword {
	public string Keyword;
	[TextArea]
	public string Dialog;

	string IHasKeyword.Keyword { get => Keyword; }
}
