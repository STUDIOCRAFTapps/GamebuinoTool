using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageConverter : MonoBehaviour {

	public RectTransform ErrorMessage;

	public Toggle CreateATileFile;

	public int Option1 = 0;
	public int Option2 = 0;
	public UnityEngine.UI.Image PaletteWarning;

	public InputField[] TextResult;
	public UnityEngine.UI.Image[] CutWarning;
	public Text[] FileDirectoryText;

	public Transform[] Panels;

	public UnityEngine.Color[] JustAFuckingPalette;
	public UnityEngine.Color JustAFuckingColor;

	void Start () {
		Option1 = PlayerPrefs.GetInt("opt1");
		Option2 = PlayerPrefs.GetInt("opt2");
		foreach(Text t in FileDirectoryText) {
			t.text = "File Directory: ";
		}
		UpdatePanelType();
		//PlayerPrefs.SetString("fileDirectory","");
	}

	static public short RGB16(byte red, byte green, byte blue) {
		short b = (short)((blue >> 3) & 0x1f);
		short g = (short)(((green >> 2) & 0x3f) << 5);
		short r = (short)(((red >> 3) & 0x1f) << 11);

		return (short)(r | g | b);
	}

	string ResultCode;

	IEnumerator ConvertActionEnum () {
		switch(GetConversionType()) {
		case 0:
			if(Option2 == 0) {
				string Result = "const uint16_t " + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_Array[] = {\n\t";

				WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
				yield return new WaitUntil(() => www.isDone);
				Texture2D tx = www.texture;

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
						sw.WriteLine(PrepareNameForCode(PlayerPrefs.GetString("S_1"))); //Name
					}
				}

				Result += tx.width + ", " + tx.height + ", 1, 0, 0x0801, 0,\n\t";

				List<Color32> col = new List<Color32>();
				for(int y = tx.height-1; y >= 0; y--) {
					for(int x = 0; x < Mathf.CeilToInt(tx.width/2f)*2; x++) {
						col.Add(tx.GetPixel(x,y));
					}
				}

				for(int x = 0; x < col.Count; x++) {
					if(col[x].a < 127f) {
						col[x] = new Color32(8,0,8,0);
					}
					//Result += col[x].r*col[x].g*col[x].b;
					Result += "0x" + RGB16(col[x].r,col[x].g,col[x].b).ToString("x");
					Result += ",";
				}

				if(Result.EndsWith(",")) {
					Result.Remove(Result.Length-1);
				}

				Result += "\n};\n";
				Result += "Image " + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + " = Image(" + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_Array);";

				TextResult[0].text = Result;
				ResultCode = Result;
			} else if(Option2 == 1) {
				if(string.IsNullOrEmpty(PlayerPrefs.GetString("paletteDirectory"))) {
					yield break;
				}

				UnityEngine.Color[] CurrentPalette = new UnityEngine.Color[16];
				bool[] PaletteColorIsUsed = new bool[16];
					
				string[] t = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
				for(int i = 0; i < Mathf.Min(t.Length-1,16); i++) {
					PaletteColorIsUsed[i] = false;
					ColorUtility.TryParseHtmlString("#"+t[i+1], out CurrentPalette[i]);
				}

				string Result = "const uint8_t " + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_Array[] = {\n";

				WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
				yield return new WaitUntil(() => www.isDone);
				Texture2D tx = www.texture;

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
						sw.WriteLine(PrepareNameForCode(PlayerPrefs.GetString("S_1"))); //Name
					}
				}

				Result += "\t" + tx.width + ", " + tx.height + ", 1, 0, 0, transparent, 1,\n\t";

				List<Color32> col = new List<Color32>();
				for(int y = tx.height-1; y >= 0; y--) {
					for(int x = 0; x < Mathf.CeilToInt(tx.width/2f)*2; x++) {
						col.Add(tx.GetPixel(x,y));
					}
				}

				for(int x = 0; x < col.Count; x+=2) {
					Result += "0x";
					if(col[x].a < 127f) {
						Result += "transparent";
					} else {
						int closest = FindClosestColorInPalette(col[x],CurrentPalette);
						PaletteColorIsUsed[closest] = true;
						Result += closest.ToString("x1");
					}
					if(x+1 < col.Count) {
						if(col[x+1].a < 127f) {
							Result += "transparent";
						} else {
							int closest = FindClosestColorInPalette(col[x+1],CurrentPalette);
							PaletteColorIsUsed[closest] = true;
							Result += closest.ToString("x1");
						}
					} else {
						Result += "0";
					}
					//Result += "0x" + RGB16(col[x].r,col[x].g,col[x].b).ToString("x");
					Result += ",";
				}

				int toReplace = 0;

				if((Result.Length - Result.Replace("transparent",string.Empty).Length) / ("transparent".Length) > 1) {
					if(!PaletteColorIsUsed.ToList().Contains(false)) {
						ErrorMessage.gameObject.SetActive(true);
						ErrorMessage.GetChild(1).GetComponent<Text>().text = "All colors are used in one indexed image, leaving no room for transparency. Please, use only 15 colors per image.";
					} else {
						toReplace = PaletteColorIsUsed.ToList().IndexOf(false);
					}
				}
				Result = Result.Replace("transparent", toReplace.ToString("x1"));

				if(Result.EndsWith(",")) {
					Result.Remove(Result.Length-1);
				}

				Result += "\n};\n";
				Result += "Image " + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + " = Image(" + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "_Array);";

				TextResult[0].text = Result;
				ResultCode = Result;
			}
			break;
		case 2:
		case 4:
			string fdirectory = "";
			if(GetConversionType() == 2) {
				fdirectory = PlayerPrefs.GetString("projectDirectory")+Path.DirectorySeparatorChar+PrepareNameForCode(PlayerPrefs.GetString("S_1"))+".bmp";
			} else if(GetConversionType() == 4) {
				fdirectory = Directory.GetParent(PlayerPrefs.GetString("fileDirectory")).FullName+PrepareNameForCode(PlayerPrefs.GetString("S_1"))+".bmp";
			}

			if(Option2 == 2) {
				//SEMI BROKEN, FIX LATER

				Bitmap bitmap = new Bitmap(PlayerPrefs.GetString("fileDirectory"));

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
						sw.WriteLine(PrepareNameForCode(PlayerPrefs.GetString("S_1"))); //Name
					}
				}

				if(!string.IsNullOrEmpty(PlayerPrefs.GetString("projectDirectory"))) {
					UnityEngine.Color[] CurrentPalette = new UnityEngine.Color[16];
					bool[] PaletteColorIsUsed = new bool[16];

					string[] t = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
					for(int i = 0; i < Mathf.Min(t.Length-1,16); i++) {
						PaletteColorIsUsed[i] = false;
						ColorUtility.TryParseHtmlString("#"+t[i+1], out CurrentPalette[i]);
					}

					Bitmap clone = new Bitmap(bitmap.Width,bitmap.Height,System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

					System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
					System.Drawing.Color[] entries = palette.Entries;
					for(int i = 0; i < CurrentPalette.Length; i++) {
						entries[i] = System.Drawing.Color.FromArgb((byte)(CurrentPalette[i].r*255),(byte)(CurrentPalette[i].g*255),(byte)(CurrentPalette[i].b*255));
					}
					bitmap.Palette = palette;

					using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(clone)) {
						gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
					}
					clone.Save(fdirectory);
				}
			} else if(Option2 == 3) {
				Bitmap bitmap = new Bitmap(PlayerPrefs.GetString("fileDirectory"));

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
						sw.WriteLine(PrepareNameForCode(PlayerPrefs.GetString("S_1"))); //Name
					}
				}

				if(!string.IsNullOrEmpty(PlayerPrefs.GetString("projectDirectory"))) {
					Bitmap clone = new Bitmap(bitmap.Width,bitmap.Height,System.Drawing.Imaging.PixelFormat.Format16bppRgb565);

					using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(clone)) {
						gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
					}
					clone.Save(fdirectory);
				}
			}
			break;
		case 1:
			if(Option2 == 0) {
				string[] t = File.ReadAllLines(Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut");

				WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
				yield return new WaitUntil(() => www.isDone);
				Texture2D tx = www.texture;

				string Result = "";
				for(int i = 0; i < t.Length; i++) {
					string[] Data = t[i].Split(';');

					Result += "const uint16_t " + Data[4].ToString() + "_Array[] = {\n\t";
					Result += Data[2].ToString() + ", " + Data[3].ToString() + ", 1, 0, 0x0801, 0,\n\t";

					if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
						string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + Data[4] + ".gtl"; //Gamebuino Tile
						File.Create(tiledir).Dispose();
						using (StreamWriter sw = File.CreateText(tiledir)) {
							sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
							sw.WriteLine(Data[4]); 											//Name
							sw.WriteLine(t[i]);
						}
					}

					List<Color32> col = new List<Color32>();
					for(int y = int.Parse(Data[1])+1; y < int.Parse(Data[1])+int.Parse(Data[3])+1; y++) {
						for(int x = int.Parse(Data[0]); x < int.Parse(Data[0])+int.Parse(Data[2]); x++) {
							col.Add(tx.GetPixel(x,tx.height-y));
						}
					}

					for(int x = 0; x < col.Count; x++) {
						if(col[x].a < 256/2) {
							col[x] = new Color32(8,0,8,0);
						}
						//Result += col[x].r*col[x].g*col[x].b;
						Result += "0x" + RGB16((byte)(col[x].r),(byte)(col[x].g),(byte)(col[x].b)).ToString("x");
						Result += ",";
					}

					if(Result.EndsWith(",")) {
						Result.Remove(Result.Length-1);
					}

					Result += "\n};\n";
					Result += "Image " + Data[4] + " = Image(" + Data[4] + "_Array);\n\n";
				}
				TextResult[1].text = Result;
				ResultCode = Result;
			} else {
				if(string.IsNullOrEmpty(PlayerPrefs.GetString("paletteDirectory"))) {
					yield break;
				}
				string[] t = File.ReadAllLines(Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut");

				UnityEngine.Color[] CurrentPalette = new UnityEngine.Color[16];
				bool[] PaletteColorIsUsed = new bool[16];

				string[] p = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
				for(int i = 0; i < Mathf.Min(p.Length-1,16); i++) {
					PaletteColorIsUsed[i] = false;
					ColorUtility.TryParseHtmlString("#"+p[i+1], out CurrentPalette[i]);
				}
				JustAFuckingPalette = CurrentPalette;

				WWW www = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
				yield return new WaitUntil(() => www.isDone);
				Texture2D tx = www.texture;
				tx.wrapMode = TextureWrapMode.Repeat;

				string Result = "";
				for(int i = 0; i < t.Length; i++) {
					string[] Data = t[i].Split(';');

					Result += "const uint8_t " + Data[4] + "_Array[] = {\n";
					Result += "\t" + int.Parse(Data[2]) + ", " + int.Parse(Data[3]) + ", 1, 0, 0, transparent, 1,\n\t";

					if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
						string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + Data[4] + ".gtl"; //Gamebuino Tile
						File.Create(tiledir).Dispose();
						using (StreamWriter sw = File.CreateText(tiledir)) {
							sw.WriteLine(PlayerPrefs.GetString("fileDirectory")); 			//Image
							sw.WriteLine(Data[4]); 											//Name
							sw.WriteLine(t[i]);
						}
					}

					List<Color32> col = new List<Color32>();
					for(int y = int.Parse(Data[1])+1; y < int.Parse(Data[1])+int.Parse(Data[3])+1; y++) {
						for(int x = int.Parse(Data[0]); x < int.Parse(Data[0])+Mathf.CeilToInt(int.Parse(Data[2])/2f)*2; x++) {
							col.Add(tx.GetPixel(x,tx.height-y));
						}
					}

					for(int x = 0; x < col.Count; x+=2) {
						Result += "0x";
						if(col[x].a < 127f) {
							Result += "transparent";
						} else {
							int closest = FindClosestColorInPalette(col[x],CurrentPalette);
							PaletteColorIsUsed[closest] = true;
							Result += closest.ToString("x1");
						}
						if(x+1 < col.Count) {
							if(col[x+1].a < 127f) {
								Result += "transparent";
							} else {
								int closest = FindClosestColorInPalette(col[x+1],CurrentPalette);
								PaletteColorIsUsed[closest] = true;
								Result += closest.ToString("x1");
							}
						} else {
							Result += "0";
						}
						//Result += "0x" + RGB16(col[x].r,col[x].g,col[x].b).ToString("x");
						Result += ",";
					}

					int toReplace = 0;

					if((Result.Length - Result.Replace("transparent",string.Empty).Length) / ("transparent".Length) > 1) {
						if(!PaletteColorIsUsed.ToList().Contains(false)) {
							ErrorMessage.gameObject.SetActive(true);
							ErrorMessage.GetChild(1).GetComponent<Text>().text = "All colors are used in one indexed image, leaving no room for transparency. Please, use only 15 colors per image.";
						} else {
							toReplace = PaletteColorIsUsed.ToList().IndexOf(false);
						}
					}
					Result = Result.Replace("transparent", toReplace.ToString());

					if(Result.EndsWith(",")) {
						Result.Remove(Result.Length-1);
					}

					Result += "\n};\n";
					Result += "Image " + Data[4] + " = Image(" + Data[4] + "_Array);\n";
				}

				TextResult[1].text = Result;
				ResultCode = Result;
			}
			break;
		case 6:
			string[] tP = File.ReadAllLines(Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut");

			WWW wwwP = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
			yield return new WaitUntil(() => wwwP.isDone);
			Texture2D txP = wwwP.texture;

			string ResultP = "";
			string[] DataP = tP[0].Split(';');
			ResultP += "const uint16_t " + DataP[4].ToString() + "_Array[] = {\n\t";
			ResultP += DataP[2].ToString() + ", " + DataP[3].ToString() + ", " + tP.Length + ", 0, 0, 0x0801, 0,\n";

			for(int i = 0; i < tP.Length; i++) {
				ResultP += "\t";
				string[] Data = tP[i].Split(';');

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + Data[4] + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine("@"+PlayerPrefs.GetString("fileDirectory")); 		//Image
						sw.WriteLine(Data[4]); 											//Name
						sw.WriteLine(tP[i]);
					}
				}

				List<Color32> col = new List<Color32>();
				for(int y = int.Parse(Data[1])+1; y < int.Parse(Data[1])+int.Parse(Data[3])+1; y++) {
					for(int x = int.Parse(Data[0]); x < int.Parse(Data[0])+int.Parse(Data[2]); x++) {
						col.Add(txP.GetPixel(x,txP.height-y));
					}
				}

				for(int x = 0; x < col.Count; x++) {
					if(col[x].a < 256/2) {
						col[x] = new Color32(8,0,8,0);
					}
					//Result += col[x].r*col[x].g*col[x].b;
					ResultP += "0x" + RGB16((byte)(col[x].r),(byte)(col[x].g),(byte)(col[x].b)).ToString("x");
					ResultP += ",";
				}

				if(ResultP.EndsWith(",")) {
					ResultP.Remove(ResultP.Length-1);
				}
				ResultP += "\n";
			}
			ResultP += "};\n";
			ResultP += "Image " + DataP[4] + " = Image(" + DataP[4] + "_Array);\n\n";
			TextResult[1].text = ResultP;
			ResultCode = ResultP;
			break;
		case 7:
			if(string.IsNullOrEmpty(PlayerPrefs.GetString("paletteDirectory"))) {
				yield break;
			}
			string[] tS = File.ReadAllLines(Application.persistentDataPath + Path.DirectorySeparatorChar + "tempcut.cut");

			UnityEngine.Color[] CurrentPaletteS = new UnityEngine.Color[16];
			bool[] PaletteColorIsUsedS = new bool[16];

			string[] pS = File.ReadAllLines(PlayerPrefs.GetString("paletteDirectory"));
			for(int i = 0; i < Mathf.Min(pS.Length-1,16); i++) {
				PaletteColorIsUsedS[i] = false;
				ColorUtility.TryParseHtmlString("#"+pS[i+1], out CurrentPaletteS[i]);
			}
			JustAFuckingPalette = CurrentPaletteS;

			WWW wwwS = new WWW("file://" + PlayerPrefs.GetString("fileDirectory"));
			yield return new WaitUntil(() => wwwS.isDone);
			Texture2D txS = wwwS.texture;
			txS.wrapMode = TextureWrapMode.Repeat;

			string ResultS = "";
			string[] DataS = tS[0].Split(';');
			ResultS += "const uint8_t " + DataS[4] + "_Array[] = {\n";
			ResultS += "\t" + int.Parse(DataS[2]) + ", " + int.Parse(DataS[3]) + ", " + tS.Length + ", 0, transparent, 1,\n";

			for(int i = 0; i < tS.Length; i++) {
				ResultS += "\t";
				string[] Data = tS[i].Split(';');

				if(PlayerPrefs.GetInt("S_2") == 1 && CreateATileFile.isOn) {
					string tiledir = PlayerPrefs.GetString("projectDirectory") + Path.DirectorySeparatorChar + Data[4] + ".gtl"; //Gamebuino Tile
					File.Create(tiledir).Dispose();
					using (StreamWriter sw = File.CreateText(tiledir)) {
						sw.WriteLine(PlayerPrefs.GetString("fileDirectory")); 			//Image
						sw.WriteLine(Data[4]); 											//Name
						sw.WriteLine(tS[i]);
					}
				}

				List<Color32> col = new List<Color32>();
				for(int y = int.Parse(Data[1])+1; y < int.Parse(Data[1])+int.Parse(Data[3])+1; y++) {
					for(int x = int.Parse(Data[0]); x < int.Parse(Data[0])+Mathf.CeilToInt(int.Parse(Data[2])/2f)*2; x++) {
						col.Add(txS.GetPixel(x,txS.height-y));
					}
				}

				for(int x = 0; x < col.Count; x+=2) {
					ResultS += "0x";
					if(col[x].a < 127f) {
						ResultS += "transparent";
					} else {
						int closest = FindClosestColorInPalette(col[x],CurrentPaletteS);
						PaletteColorIsUsedS[closest] = true;
						ResultS += closest.ToString("x1");
					}
					if(x+1 < col.Count) {
						if(col[x+1].a < 127f) {
							ResultS += "transparent";
						} else {
							int closest = FindClosestColorInPalette(col[x+1],CurrentPaletteS);
							PaletteColorIsUsedS[closest] = true;
							ResultS += closest.ToString("x1");
						}
					} else {
						ResultS += "0";
					}
					//Result += "0x" + RGB16(col[x].r,col[x].g,col[x].b).ToString("x");
					ResultS += ",";
				}

				ResultS += "\n";
			}
			int toReplaceS = 0;

			if((ResultS.Length - ResultS.Replace("transparent",string.Empty).Length) / ("transparent".Length) > 1) {
				if(!PaletteColorIsUsedS.ToList().Contains(false)) {
					ErrorMessage.gameObject.SetActive(true);
					ErrorMessage.GetChild(1).GetComponent<Text>().text = "All colors are used in one indexed image, leaving no room for transparency. Please, use only 15 colors per image.";
				} else {
					toReplaceS = PaletteColorIsUsedS.ToList().IndexOf(false);
				}
			}
			ResultS = ResultS.Replace("transparent", toReplaceS.ToString());

			if(ResultS.EndsWith(",")) {
				ResultS.Remove(ResultS.Length-1);
			}

			ResultS += "};\n";
			ResultS += "Image " + DataS[4] + " = Image(" + DataS[4] + "_Array);\n";

			TextResult[1].text = ResultS;
			ResultCode = ResultS;
			break;
		default:
			break;
		}
	}

	public void CopyButton () {
		ClipboardHelper.clipBoard = ResultCode;
	}

	public void ConvertAction () {
		StartCoroutine(ConvertActionEnum());
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.J)) {
			Debug.Log(FindClosestColorInPalette(JustAFuckingColor,JustAFuckingPalette));
		}
	}

	public int FindClosestColorInPalette (UnityEngine.Color c, UnityEngine.Color[] palette) {
		float distance = Mathf.Infinity;
		int nearest_color = 0;
		for(int i = 0; i < palette.Length; i++) {
			float r = Mathf.Pow(palette[i].r - c.r, 2f);
			float g = Mathf.Pow(palette[i].g - c.g, 2f);
			float b = Mathf.Pow(palette[i].b - c.b, 2f);
			float temp = Mathf.Sqrt(r+b+g);
			if(temp == 0) {
				return i;
			} else if (temp < distance) {
				distance = temp;
				nearest_color = i;
			}
		}
		return nearest_color;
	}

	static string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890_";
	static public string PrepareNameForCode (string Value) {
		string Result = "";

		char[] VCharA = Value.ToCharArray();
		for(int i = 0; i < VCharA.Length; i++) {
			bool IsEqual = false;
			foreach(char c in allowedChars.ToCharArray()) {
				if(c == VCharA[i]) {
					IsEqual = true;
					break;
				}
			}
			if(IsEqual) {
				Result += VCharA[i];
			}
		}
		if(!string.IsNullOrEmpty(Result)) {
			if(char.IsNumber(Result.ToCharArray()[0])) {
				Result.Insert(0,"_");
			}
		}
		return Result;
	}

	public void GetFile () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("FileSelector");
	}

	public void SwitchOption1 (int Value) {
		Option1 = Value;
		PlayerPrefs.SetInt("opt1", Option1);
		UpdatePanelType();
	}

	public void SwitchOption2 (int Value) {
		Option2 = Value;
		PlayerPrefs.SetInt("opt2", Option2);
		UpdatePanelType();
	}

	int GetConversionType () {
		int Cut = 0; //0: No, 1: Yeah
		int Type = 0; //0: TextOutput, 1: ProjectOutput, 2: DropNextToFile

		Cut = Option1;

		switch(Option2) {
		case 0:
		case 1:
			Type = 0;
			break;
		default:
			Type = (byte)(1+(1-PlayerPrefs.GetInt("S_2")));
			break;
		}

		//0 no cut/text
		//1 cut/text
		//2 no cut/project
		//3 cut/project
		//4 no cut/drop
		//5 cut/drop
		if(Option1 == 2) {
			if(Option2 == 0) {
				return 6;
			} else {
				return 7;
			}
		} else {
			return Cut+(2*Type);
		}
	}

	public void UpdatePanelType () {
		int Cut = 0; //0: No, 1: Yeah
		int Type = 0; //0: TextOutput, 1: ProjectOutput, 2: DropNextToFile

		Cut = Option1;

		switch(Option2) {
		case 0:
		case 1:
			Type = 0;
			break;
		default:
			Type = (byte)(1+(1-PlayerPrefs.GetInt("S_2")));
			break;
		}

		int Result = Cut+(2*Type);
		if(Option1 == 2) {
			Result = 1+(2*0);
		}
		foreach(Transform tr in Panels) {
			tr.gameObject.SetActive(false);
		}
		Panels[Result].gameObject.SetActive(true);

		FileDirectoryText[Result].text = PlayerPrefs.GetString("fileDirectory").Split(Path.DirectorySeparatorChar)[PlayerPrefs.GetString("fileDirectory").Split(Path.DirectorySeparatorChar).Length-1];
	}

	public void ChangeProject () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("ProjectFolder");
	}

	public void ReturnToMenu () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("Main");
	}

	public void ChangePalette () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("PaletteSelector");
	}

	public void CropATile () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("SpriteCutter");
	}
}