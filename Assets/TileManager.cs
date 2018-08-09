using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TileManager : MonoBehaviour {

	public InputField SizeX;
	public InputField SizeY;
	public InputField ImageSize;

	public Text DirectoryText;
	public Button ExportButton;

	public InputField NewParamName;
	public Dropdown NewParamType;
	public InputField NewParamValue;
	public Text NewParamByteId;
	public GameObject NewParamObject;

	public List<Parameter> ParametersLines;
	RectTransform SelectedParameter;
	public RectTransform Content;

	Texture image;

	void Start () {
		ParametersLines = new List<Parameter>();

		if(string.IsNullOrEmpty(PlayerPrefs.GetString("projectDirectory"))) {
			ExportButton.gameObject.SetActive(false);
		}
	}

	public void GetFile() {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("FileSelector");
	}

	public void ImportTile () {
		
	}

	public void AddParameter () {
		GameObject ParamLine = (GameObject)Instantiate(NewParamObject, Content);
		ParamLine.transform.GetChild(0).GetComponent<Text>().text = NewParamName.text;
		ParametersLines.Add(new Parameter(ParamLine.GetComponent<RectTransform>(),NewParamName.text,NewParamType.value,NewParamValue.text));
		RectTransform i = ParametersLines[ParametersLines.Count-1].TransformObject;

		ParamLine.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => {
			SelectedParameter = i;
		});
	}

	public void RemoveParameter () {
		if(ParametersLines.Count > 0 && SelectedParameter != null) {
			int deleteAt = 0;
			for(int i = 0; i < ParametersLines.Count; i++) {
				if(ParametersLines[i].TransformObject == SelectedParameter) {
					deleteAt = i;
					break;
				}
			}
			Destroy(ParametersLines[deleteAt].TransformObject.gameObject);
			ParametersLines.RemoveAt(deleteAt);
			SelectedParameter = null;
		}
	}

	public IEnumerator LoadImage () {
		WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
		yield return new WaitUntil(() => www.isDone);
		image = www.texture;

		ImageSize.text = (image.width*image.height).ToString();
		SizeX.text = image.width.ToString();
		SizeY.text = image.height.ToString();
	}

	public void ReturnToMenu () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("Main");
	}

	public void ChangeProject () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("ProjectFolder");
	}

	public void Import() {
		StartCoroutine(LoadImage());

		List<GameObject> children = new List<GameObject>();
		foreach(Transform child in Content) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));

		ParametersLines = new List<Parameter>();
	}

	public void Export() {
		//Open the export windows,
		//Select mode: Byte file Index/RGB565, Code Index/RGB565
	}

	public class Parameter {
		public RectTransform TransformObject;
		public string Name;
		public int Type;
		public string Value;

		public Parameter (
			RectTransform TransformObject,
			string Name,
			int Type,
			string Value) {
			this.TransformObject = TransformObject;
			this.Name = Name;
			this.Type = Type;
			this.Value = Value;
		}
	}
}
