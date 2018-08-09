using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TilemapConverter : MonoBehaviour {

	public int Option1 = 0;

	public InputField TextResult;
	public Text FileDirectoryText;

	public Transform Panel;

	void Start () {
		Option1 = PlayerPrefs.GetInt("opt1");
		if(string.IsNullOrEmpty(PlayerPrefs.GetString("paletteDirectory"))) {
			FileDirectoryText.text = "File Directory:";
		} else {
			FileDirectoryText.text = PlayerPrefs.GetString("fileDirectory").Split(Path.DirectorySeparatorChar)[PlayerPrefs.GetString("fileDirectory").Split(Path.DirectorySeparatorChar).Length - 1];
		}
	}

	string ResultCode;

	IEnumerator ConvertActionEnum () {
		if(string.IsNullOrEmpty(PlayerPrefs.GetString("fileDirectory"))) {
			yield break;
		}
		string Result = "const uint8_t " + PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "[] {\n";

		string txt = System.IO.File.ReadAllText(PlayerPrefs.GetString("fileDirectory"));
		Debug.Log(txt);

		if(!string.IsNullOrEmpty(txt)) {
			if(Option1 == 0) {
				Result += "\t" + (txt.Split('\n')[0].Split(',').Length - 2).ToString() + ", " + (txt.Split('\n').Length - 2).ToString() + ", //Width, Height,";
				Result += "\t" + txt.Replace("-1","0").Replace("\n",",").Replace(" ","");
				if(Result.EndsWith(",",System.StringComparison.InvariantCulture)) {
					Result.Remove(Result.Length-1);
				}
				Result += "\n};";
				TextResult.text = Result;
				ResultCode = Result;
			} else {
				Result += "\t" + (txt.Split('\n')[0].Split(',').Length - 2).ToString() + ", " + (txt.Split('\n').Length - 2).ToString() + ", //Width, Height,";
				string by2 = txt.Replace("-1","0").Replace("\n",",").Replace(" ","");
				by2 = by2.Replace("A","10");
				by2 = by2.Replace("B","11");
				by2 = by2.Replace("C","12");
				by2 = by2.Replace("D","13");
				by2 = by2.Replace("E","14");
				by2 = by2.Replace("F","15");
				by2 = by2.Replace(",",string.Empty);
				for(int i = 2; i < by2.Length; i += 3) {
					by2 = by2.Insert(i,",");
				}
				for(int i = 0; i < by2.Length; i += 5) {
					by2 = by2.Insert(i,"0x");
				}
				Result += "\t" + by2 + "\n};";
				TextResult.text = Result;
				ResultCode = Result;
			}
		} else {
			Result += "\tthe file is empty or broken you dumb dumb\n};";
			TextResult.text = Result;
			ResultCode = Result;
		}

		yield return null;
	}

	public void CopyButton () {
		ClipboardHelper.clipBoard = ResultCode;
	}

	public void ConvertAction () {
		StartCoroutine(ConvertActionEnum());
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
		PlayerPrefs.SetString("lastLoadedScene",SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("FileSelector");
	}

	public void SwitchOption1 (int Value) {
		Option1 = Value;
		PlayerPrefs.SetInt("opt1",Option1);
	}

	public void ChangeProject () {
		PlayerPrefs.SetString("lastLoadedScene",SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("ProjectFolder");
	}

	public void ReturnToMenu () {
		PlayerPrefs.SetString("lastLoadedScene",SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("Main");
	}
}
