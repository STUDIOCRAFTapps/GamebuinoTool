using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FileFinderManager : MonoBehaviour {

	public InputField folderSearch;
	public Text SubTitle;
	public Button SearchButton;

	public Sprite[] SearchButtonIcon;
	bool NewFolderMode;

	public Sprite[] FileIcons;
	public GameObject DirectoryLinePrefab;
	public RectTransform ContentBox;

	public string CurrentDirectory;
	string[] currentDirList;
	string[] currentFileList;

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

	public void SetDirectoryAsFile (string DirectoryName) {
		SubTitle.text = "File Selected";
		PlayerPrefs.SetString("fileDirectory", DirectoryName);
	}

	public void SearchForFolder () {
		if(Directory.Exists(folderSearch.text)) {
			LoadNewDirectory(folderSearch.text);
		} else {
			folderSearch.text = CurrentDirectory;
			SubTitle.text = "The folder dosen't exist";
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
			DirectoryLine.transform.GetChild(2).GetComponent<Button>().interactable = false;
			/*DirectoryLine.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
				SetDirectoryAsFile(dirs[DirectoryToCall]);
			});*/

		}
			
		string[] files = Directory.GetFiles(CurrentDirectory);
		currentFileList = files;
		for(int i = 0; i < files.Length; i++) {
			GameObject DirectoryLine = (GameObject)Instantiate(DirectoryLinePrefab,ContentBox);
			RectTransform dlrt = DirectoryLine.GetComponent<RectTransform>();

			dlrt.anchoredPosition = new Vector2(-10,-((i+files.Length)+3)*dlrt.rect.height+80);
			string[] sp = files[i].Split(Path.DirectorySeparatorChar);
			dlrt.GetChild(0).GetComponent<Text>().text = sp[sp.Length-1];

			int spriteID = 1;

			if(sp[sp.Length-1].EndsWith(".txt")) {
				spriteID = 1;
			}
			if(sp[sp.Length-1].EndsWith(".html") || sp[sp.Length-1].EndsWith(".gif") || sp[sp.Length-1].EndsWith(".pdf") || sp[sp.Length-1].EndsWith(".zip") && sp[sp.Length-1].EndsWith(".jar") && sp[sp.Length-1].EndsWith(".shader") || sp[sp.Length-1].EndsWith(".exe") || sp[sp.Length-1].EndsWith(".app") || sp[sp.Length - 1].EndsWith(".csv")) {
				spriteID = 2;
			}
			if(sp[sp.Length-1].EndsWith(".png") || sp[sp.Length-1].EndsWith(".bmp") || sp[sp.Length-1].EndsWith(".jpg")) {
				spriteID = 3;
			}
			DirectoryLine.transform.GetChild(1).GetComponent<Image>().sprite = FileIcons[spriteID];

			if(spriteID == 3 || sp[sp.Length - 1].EndsWith(".csv")) {
				int FileToCall = i;
				DirectoryLine.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
					SetDirectoryAsFile(files[FileToCall]);
				});
			} else {
				DirectoryLine.transform.GetChild(2).GetComponent<Button>().interactable = false;
			}
		}
	}



	public void ExitButton () {
		SceneManager.LoadScene(PlayerPrefs.GetString("lastLoadedScene"));
	}

	void Update () {
		
	}
}
