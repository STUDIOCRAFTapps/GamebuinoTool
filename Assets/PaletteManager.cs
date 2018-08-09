using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PaletteManager : MonoBehaviour {

	public RectTransform ErrorMessage;

	public Transform[] ColorLines;
	public Text Subtitle;

	public GameObject PaletteSelectorPrefab;
	public RectTransform PaletteSelectorContent;

	public InputField NewPaletteName;
	string[] currentPaletteList;

	public InputField paletteCode;

	public Color[] CurrentPalette;

	void Start () {
		CurrentPalette = new Color[16];
		PlayerPrefs.SetString("paletteDirectory", "");

		SearchPalette();
		LoadPalette();
	}

	public void CreatePalette () {
		string maindir = Application.persistentDataPath + Path.DirectorySeparatorChar + "palettes";
		string paldir = maindir + Path.DirectorySeparatorChar + "palette_" + Directory.GetFiles(maindir).Length + ".gpl"; //gpl: Gamebuino Palette

		File.Create(paldir).Dispose();
		using (StreamWriter sw = File.CreateText(paldir)) 
		{
			sw.WriteLine(NewPaletteName.text);
		}
		SearchPalette();
	}

	public void DeletePalette () {
		Subtitle.text = "Palette Deleted";
		File.Delete(PlayerPrefs.GetString("paletteDirectory"));
		PlayerPrefs.SetString("paletteDirectory", "");

		SearchPalette();
	}

	public void SearchPalette () {
		List<GameObject> children = new List<GameObject>();
		foreach(Transform child in PaletteSelectorContent) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));

		PaletteSelectorContent.sizeDelta = new Vector2(PaletteSelectorContent.sizeDelta.x,0);
		LayoutRebuilder.MarkLayoutForRebuild(PaletteSelectorContent as RectTransform);

		if(!Directory.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "palettes")) {
			Directory.CreateDirectory(Application.persistentDataPath + Path.DirectorySeparatorChar + "palettes");
			return;
		}

		string[] files = Directory.GetFiles(Application.persistentDataPath + Path.DirectorySeparatorChar + "palettes");
		currentPaletteList = files;
		for(int i = 0; i < files.Length; i++) {
			GameObject DirectoryLine = (GameObject)Instantiate(PaletteSelectorPrefab,PaletteSelectorContent);
			RectTransform dlrt = DirectoryLine.GetComponent<RectTransform>();

			DirectoryLine.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = File.ReadAllText(files[i]).Split('\n')[0];
			DirectoryLine.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.HSVToRGB(1f/files.Length*i,1f,1f);

			int FileToCall = i;
			DirectoryLine.GetComponent<Button>().onClick.AddListener(() => {
				SelectPalette(FileToCall);
			});
		}
	}

	public void SelectPalette (int ID) {
		Subtitle.text = "Palette Selected";
		PlayerPrefs.SetString("paletteDirectory", currentPaletteList[ID]);

		LoadPalette();
	}

	void LoadPalette () {
		if(string.IsNullOrEmpty(PlayerPrefs.GetString("paletteDirectory"))) {
			for(int i = 0; i < ColorLines.Length; i++) {
				ColorLines[i].gameObject.SetActive(false);
			}
			return;
		} else {
			for(int i = 0; i < ColorLines.Length; i++) {
				ColorLines[i].gameObject.SetActive(true);
			}
		}

		CurrentPalette = new Color[16];
		string[] t = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
		for(int i = 0; i < Mathf.Min(t.Length-1,16); i++) {
			ColorUtility.TryParseHtmlString("#"+t[i+1], out CurrentPalette[i]);
		}

		for(int i = 0; i < ColorLines.Length; i++) {
			if(i < t.Length-1) {
				ColorLines[i].GetChild(0).GetChild(0).GetComponent<InputField>().text = t[i+1];
			} else {
				ColorLines[i].GetChild(0).GetChild(0).GetComponent<InputField>().text = "";
			}
			ColorLines[i].GetChild(0).GetChild(1).GetComponent<Image>().color = CurrentPalette[i];
			ColorLines[i].GetChild(0).GetChild(2).GetComponent<Text>().text = "RGB: " + (int)(CurrentPalette[i].r*255) + ", " + (int)(CurrentPalette[i].g*255) + ", " + (int)(CurrentPalette[i].b*255);
		}
	}

	public void SetColor1 (string Hex) {
		SetColor(Hex,1);
	}

	public void SetColor2 (string Hex) {
		SetColor(Hex,2);
	}

	public void SetColor3 (string Hex) {
		SetColor(Hex,3);
	}

	public void SetColor4 (string Hex) {
		SetColor(Hex,4);
	}

	public void SetColor5 (string Hex) {
		SetColor(Hex,5);
	}

	public void SetColor6 (string Hex) {
		SetColor(Hex,6);
	}

	public void SetColor7 (string Hex) {
		SetColor(Hex,7);
	}

	public void SetColor8 (string Hex) {
		SetColor(Hex,8);
	}

	public void SetColor9 (string Hex) {
		SetColor(Hex,9);
	}

	public void SetColor10 (string Hex) {
		SetColor(Hex,10);
	}

	public void SetColor11 (string Hex) {
		SetColor(Hex,11);
	}

	public void SetColor12 (string Hex) {
		SetColor(Hex,12);
	}

	public void SetColor13 (string Hex) {
		SetColor(Hex,13);
	}

	public void SetColor14 (string Hex) {
		SetColor(Hex,14);
	}

	public void SetColor15 (string Hex) {
		SetColor(Hex,15);
	}

	public void SetColor16 (string Hex) {
		SetColor(Hex,16);
	}

	void SetColor(string Hex, int ID) {
		ColorUtility.TryParseHtmlString("#" + Hex.Replace("0x","").ToUpper(), out CurrentPalette[ID-1]);

		for(int i = 0; i < ColorLines.Length; i++) {
			ColorLines[i].GetChild(0).GetChild(1).GetComponent<Image>().color = CurrentPalette[i];
			ColorLines[i].GetChild(0).GetChild(2).GetComponent<Text>().text = "RGB: " + (int)(CurrentPalette[i].r*255) + ", " + (int)(CurrentPalette[i].g*255) + ", " + (int)(CurrentPalette[i].b*255);
		}
	}

	//Color pico_8_palette[] = {
	//	(Color)0x0000,  // BLACK
	//	(Color)0x194a,  // DARK-BLUE
	//	(Color)0x792a,  // DARK-PURPLE
	//	(Color)0x042a,  // DARK-GREEN
	//	(Color)0xaa86,  // BROWN
	//	(Color)0x5aa9,  // DARK-GRAY
	//	(Color)0xc618,  // LIGHT-GRAY
	//	(Color)0xff9d,  // WHITE
	//	(Color)0xf809,  // RED
	//	(Color)0xfd00,  // ORANGE
	//	(Color)0xff64,  // YELLOW
	//	(Color)0x0726,  // GREEN
	//	(Color)0x2d7f,  // BLUE
	//	(Color)0x83b3,  // INDIGO
	//	(Color)0xfbb5,  // PINK
	//	(Color)0xfe75   // PEACH
	//};
	//[...]
	//gb.display.colorIndex = pico_8_palette;
	//[...]
	//myImage.setTransparentColor((ColorIndex)15);

	public void AutoDetect () {
		StartCoroutine(WaitToAutoDetect());
	}

	IEnumerator WaitToAutoDetect () {
		WWW www = new WWW("file://" +PlayerPrefs.GetString("fileDirectory"));
		yield return new WaitUntil(() => www.isDone);
		Texture2D tx = www.texture;

		Color32[] col = tx.GetPixels32();
		System.Array.Reverse(col);

		UnityEngine.Color[] Palette = new Color[16];
		int Taken = 0;

		for(int x = 0; x < col.Length; x++) {
			if(col[x].a >= 0.5f) {
				if(!Palette.ToList().Contains(col[x])) {
					if(Taken < 16) {
						Palette[Taken] = col[x];
						Taken++;
					} else {
						Taken++;
						break;
					}
				}
			}
		}

		if(Taken > 16) {
			ErrorMessage.gameObject.SetActive(true);
			ErrorMessage.GetChild(1).GetComponent<Text>().text = "More than 16 colors have been detected in the image. Only the first 16 were selected.";
		} else if(Taken < 16) {
			ErrorMessage.gameObject.SetActive(true);
			ErrorMessage.GetChild(1).GetComponent<Text>().text = "Less than 16 colors have been detected in the image. Insert manually the last " + (16-Taken).ToString() + " colors remaining.";
		}

		CurrentPalette = Palette;

		Save();
		LoadPalette();
	}

	public void Restart () {
		LoadPalette();
	}

	public void Save () {
		Subtitle.text = "Palette Saved";

		string[] t = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
		using (StreamWriter sw = File.CreateText(PlayerPrefs.GetString("paletteDirectory"))) 
		{
			sw.WriteLine(t[0]);
			for(int i = 0; i < 16; i++) {
				sw.WriteLine(ColorUtility.ToHtmlStringRGB(CurrentPalette[i]));
			}
		}

		paletteCode.text = "Color " + ImageConverter.PrepareNameForCode(t[0]) + "[] = {\n";
		for(int i = 0; i < 16; i++) {
			if(CurrentPalette[i] == new Color()) {
				continue;
			}

			paletteCode.text += "\t(Color)0x" + ImageConverter.RGB16((byte)(CurrentPalette[i].r*255),(byte)(CurrentPalette[i].g*255),(byte)(CurrentPalette[i].b*255)).ToString("x4") + ",\n";
		}
		paletteCode.text.Remove(paletteCode.text.Length-1);
		paletteCode.text += "};\ngb.display.colorIndex = newPalette;";
	}

	public void Quit () {
		SceneManager.LoadScene(PlayerPrefs.GetString("lastLoadedScene"));
	}
}
