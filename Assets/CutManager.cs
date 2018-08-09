using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class CutManager : MonoBehaviour {

	public RectTransform ContentBox;
	public RawImage ImageDisplay;

	public GameObject SelectorRectPrefab;

	public float Scale = 3.125f;
	public float Rescaler = 1f;

	public Color colorNormal;
	public Color colorSelected;

	public List<SelectorRect> selectors;
	public SelectorRect mainSelector;

	public RectTransform SelectorInfo;
	public InputField SelectorInfoNameField;

	public InputField[] SelectorInfoFields;

	public RectTransform SelectorInfoSubPanel;
	public RectTransform AutoselectorSubPanel;

	void Start () {
		StartCoroutine(Init());
	}

	IEnumerator Init () {
		WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
		yield return new WaitUntil(() => www.isDone);
		Texture2D tx = www.texture;
		tx.filterMode = FilterMode.Point;

		ImageDisplay.texture = tx;

		ImageDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(ImageDisplay.texture.width*Scale,ImageDisplay.texture.height*Scale);
		ContentBox.GetComponent<LayoutElement>().minWidth = ImageDisplay.GetComponent<RectTransform>().sizeDelta.x;
		ContentBox.GetComponent<LayoutElement>().minHeight = ImageDisplay.GetComponent<RectTransform>().sizeDelta.y;
		ContentBox.sizeDelta = ImageDisplay.GetComponent<RectTransform>().sizeDelta;
		ContentBox.GetChild(0).GetComponent<RectTransform>().sizeDelta = ImageDisplay.GetComponent<RectTransform>().sizeDelta;

		if(PlayerPrefs.GetString("lastCuttedFile") == PlayerPrefs.GetString("fileDirectory")) {
			LoadCutData();
		}

		PlayerPrefs.SetString("lastCuttedFile", PlayerPrefs.GetString("fileDirectory"));
	}

	void LoadCutData () {
		string[] t = File.ReadAllLines(Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut");
		for(int i = 0; i < t.Length; i++) {
			AddNewSelectorRect();
			string[] data = t[i].Split(';');
			selectors[selectors.Count-1].SetPosition(int.Parse(data[0]),int.Parse(data[1]));
			selectors[selectors.Count-1].SetSize(int.Parse(data[2]),int.Parse(data[3]));
			selectors[selectors.Count-1].Name = data[4];
		}
	}

	public void SaveCut () {
		string cutdir = Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut";
		File.Create(cutdir).Dispose();
		using (StreamWriter sw = File.CreateText(cutdir)) {
			for(int i = 0; i < selectors.Count; i++) {
				if(i!=0) {
					sw.Write("\n");
				}
				sw.Write(Mathf.RoundToInt(selectors[i].GetComponent<RectTransform>().anchoredPosition.x/Scale)+";"+Mathf.RoundToInt(-selectors[i].GetComponent<RectTransform>().anchoredPosition.y/Scale)+";");
				sw.Write(Mathf.RoundToInt(selectors[i].GetComponent<RectTransform>().sizeDelta.x/Scale)+";"+Mathf.RoundToInt(selectors[i].GetComponent<RectTransform>().sizeDelta.y/Scale)+";");
				sw.Write(selectors[i].Name);
			}
		}
	}

	public void Exit () {
		SceneManager.LoadScene(PlayerPrefs.GetString("lastLoadedScene"));
	}

	public void RelocateSelector (SelectorRect sr) {
		foreach(SelectorRect r in selectors) {
			r.GetComponent<Image>().color = colorNormal;
		}
		mainSelector = sr;
		sr.transform.SetAsLastSibling();
	}

	bool SelectorInfoIsOpen = false;
	SelectorRect InfoSource;
	public void OpenSelectorInfo (SelectorRect sr) {
		InfoSource = sr;
		SelectorInfo.gameObject.SetActive(true);
		SelectorInfoNameField.text = sr.Name;

		SelectorInfoFields[0].text = (sr.GetComponent<RectTransform>().anchoredPosition.x/Scale).ToString();
		SelectorInfoFields[1].text = (-sr.GetComponent<RectTransform>().anchoredPosition.y/Scale).ToString();
		SelectorInfoFields[2].text = (sr.GetComponent<RectTransform>().sizeDelta.x/Scale).ToString();
		SelectorInfoFields[3].text = (sr.GetComponent<RectTransform>().sizeDelta.y/Scale).ToString();

		SelectorInfoIsOpen = true;
	}

	public void CloseSelectorInfo () {
		SelectorInfo.gameObject.SetActive(false);
		SelectorInfoNameField.text = "";

		InfoSource.SetPosition(int.Parse(SelectorInfoFields[0].text),int.Parse(SelectorInfoFields[1].text));
		InfoSource.SetSize(int.Parse(SelectorInfoFields[2].text),int.Parse(SelectorInfoFields[3].text));

		SelectorInfoIsOpen = false;
	}

	public void OpenAutoselectPanel () {
		SelectorInfo.gameObject.SetActive(true);
		SelectorInfoSubPanel.gameObject.SetActive(false);
		AutoselectorSubPanel.gameObject.SetActive(true);
	}

	public void CloseAutoselectPanel () {
		SelectorInfo.gameObject.SetActive(false);
		SelectorInfoSubPanel.gameObject.SetActive(true);
		AutoselectorSubPanel.gameObject.SetActive(false);
	}

	public void ValidAutoselectPanel () {
		Autoselect();
		CloseAutoselectPanel();
	}

	void Autoselect () {
		for(int x = 0; x < int.Parse(AutoselectorSubPanel.GetChild(10).GetComponent<InputField>().text)*int.Parse(AutoselectorSubPanel.GetChild(2).GetComponent<InputField>().text); x+=int.Parse(AutoselectorSubPanel.GetChild(2).GetComponent<InputField>().text)+int.Parse(AutoselectorSubPanel.GetChild(5).GetComponent<InputField>().text)) {
			for(int y = 0; y < int.Parse(AutoselectorSubPanel.GetChild(11).GetComponent<InputField>().text)*int.Parse(AutoselectorSubPanel.GetChild(3).GetComponent<InputField>().text); y+=int.Parse(AutoselectorSubPanel.GetChild(3).GetComponent<InputField>().text)+int.Parse(AutoselectorSubPanel.GetChild(6).GetComponent<InputField>().text)) {
				GameObject ns = (GameObject)Instantiate(SelectorRectPrefab, ContentBox);
				ns.GetComponent<SelectorRect>().cutManager = this;
				ns.GetComponent<SelectorRect>().Name = ImageConverter.PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_" + selectors.Count.ToString("D3");
				selectors.Add(ns.GetComponent<SelectorRect>());

				ns.GetComponent<RectTransform>().anchoredPosition = new Vector2(x*Scale,-y*Scale);
				ns.GetComponent<RectTransform>().sizeDelta = new Vector2(int.Parse(AutoselectorSubPanel.GetChild(2).GetComponent<InputField>().text)*Scale,int.Parse(AutoselectorSubPanel.GetChild(3).GetComponent<InputField>().text)*Scale);
			}
		}
	}

	void Update () {
		ContentBox.localScale = Vector3.one*Mathf.Clamp(ContentBox.localScale.x+Input.mouseScrollDelta.y*0.1f,0.1f,5f);

		if(SelectorInfoIsOpen) {
			InfoSource.Name = ImageConverter.PrepareNameForCode(SelectorInfoNameField.text);
		}

		if(Input.GetKey(KeyCode.Backspace) && mainSelector != null && !SelectorInfo.gameObject.activeInHierarchy) {
			DeleteMainSelector();
		}
		if(Input.GetKey(KeyCode.Slash) && Input.GetMouseButtonDown(0)) {
			GameObject ns = (GameObject)Instantiate(SelectorRectPrefab, ContentBox);
			ns.GetComponent<SelectorRect>().cutManager = this;
			ns.GetComponent<SelectorRect>().Name = ImageConverter.PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_" + selectors.Count;
			selectors.Add(ns.GetComponent<SelectorRect>());

			RelocateSelector(ns.GetComponent<SelectorRect>());
			ns.GetComponent<Image>().color = colorSelected;

			Vector3 cursorPos = Input.mousePosition;
			ns.transform.position = cursorPos;
			ns.GetComponent<RectTransform>().anchoredPosition = new Vector2(RoundStep(ns.GetComponent<RectTransform>().anchoredPosition.x,Scale)-Scale,RoundStep(ns.GetComponent<RectTransform>().anchoredPosition.y,Scale)+Scale);
			ns.GetComponent<RectTransform>().sizeDelta = new Vector2(int.Parse(AutoselectorSubPanel.GetChild(2).GetComponent<InputField>().text)*Scale,int.Parse(AutoselectorSubPanel.GetChild(3).GetComponent<InputField>().text)*Scale);
		}
	}

	public void DeleteMainSelector () {
		if(mainSelector != null) {
			selectors.Remove(mainSelector);
			Destroy(mainSelector.gameObject);
		}
	}

	public void AddNewSelectorRect () {
		GameObject ns = (GameObject)Instantiate(SelectorRectPrefab, ContentBox);
		ns.GetComponent<SelectorRect>().cutManager = this;
		ns.GetComponent<SelectorRect>().Name = ImageConverter.PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_" + selectors.Count.ToString("D3");
		selectors.Add(ns.GetComponent<SelectorRect>());
	}

	float FloorStep (float Value, float Step) {
		return Step*Mathf.Floor(Value/Step);
	}

	float RoundStep (float Value, float Step) {
		return Step*Mathf.Round(Value/Step);
	}
}
