using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProjectFinderManager : MonoBehaviour {

	public InputField folderSearch;
	public Text SubTitle;
	public Button SearchButton;

	public Sprite[] SearchButtonIcon;
	bool NewFolderMode;

	public GameObject DirectoryLinePrefab;
	public RectTransform ContentBox;

	public string CurrentDirectory;
	string[] currentDirList;

	void Start() {
		if(!string.IsNullOrEmpty(PlayerPrefs.GetString("projectDirectory"))) {
			CurrentDirectory = PlayerPrefs.GetString("projectDirectory");
		} else {
			if(Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
				CurrentDirectory = "/Users/";
			} else if(Application.platform == RuntimePlatform.WindowsPlayer) {
				CurrentDirectory = @"C:\\";
			} else if(Application.platform == RuntimePlatform.LinuxPlayer){
				CurrentDirectory = "/home/";
			}
		}
		LoadNewDirectory(CurrentDirectory);
	}

	public void DirectoryReturn () {
		LoadNewDirectory(Directory.GetParent(CurrentDirectory).FullName);
	}

	public void SetDirectoryAsProject (string DirectoryName) {
		Debug.Log(DirectoryName);
		SubTitle.text = "Project folder Selected";
		PlayerPrefs.SetString("projectDirectory", DirectoryName);
	}

	public void CreateNewFolder () {
		if(NewFolderMode) {
			folderSearch.text = CurrentDirectory;
			NewFolderMode = false;
		} else {
			folderSearch.text = "";
			NewFolderMode = true;
		}
	}

	public void SearchForFolder () {
		if(NewFolderMode) {
			Directory.CreateDirectory(CurrentDirectory + Path.DirectorySeparatorChar + folderSearch.text.Replace('.','_'));
			CreateNewFolder();
		} else {
			if(Directory.Exists(folderSearch.text)) {
				LoadNewDirectory(folderSearch.text);
			} else {
				folderSearch.text = CurrentDirectory;
				SubTitle.text = "The folder dosen't exist";
			}
		}
	}

	public void LoadNewDirectory (string NewDirectory) {
		folderSearch.text = NewDirectory;
		CurrentDirectory = NewDirectory;

		List<GameObject> children = new List<GameObject>();
		foreach(Transform child in ContentBox) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));

		ContentBox.sizeDelta = new Vector2(ContentBox.sizeDelta.x,0);
		LayoutRebuilder.MarkLayoutForRebuild(ContentBox as RectTransform);

		string[] dirs = Directory.GetDirectories(CurrentDirectory);
		currentDirList = dirs;
		for(int i = 0; i < dirs.Length; i++) {
			GameObject DirectoryLine = (GameObject)Instantiate(DirectoryLinePrefab,ContentBox);
			RectTransform dlrt = DirectoryLine.GetComponent<RectTransform>();

			dlrt.anchoredPosition = new Vector2(-10,-(i+3)*dlrt.rect.height+80);
			string[] sp = dirs[i].Split(Path.DirectorySeparatorChar);
			dlrt.GetChild(0).GetComponent<Text>().text = sp[sp.Length-1];

			int DirectoryToCall = i;
			DirectoryLine.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => {
				LoadNewDirectory(dirs[DirectoryToCall]);
			});
			DirectoryLine.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
				SetDirectoryAsProject(dirs[DirectoryToCall]);
			});

		}
	}



	public void ExitButton () {
		SceneManager.LoadScene(PlayerPrefs.GetString("lastLoadedScene"));
	}

	void Update () {
		SearchButton.GetComponent<Image>().sprite = SearchButtonIcon[NewFolderMode?1:0];
		if(NewFolderMode) {
			folderSearch.transform.Find("Placeholder").GetComponent<Text>().text = "Enter the name of the new folder";
		} else {
			folderSearch.transform.Find("Placeholder").GetComponent<Text>().text = "Search a folder";
		}
	}
}
