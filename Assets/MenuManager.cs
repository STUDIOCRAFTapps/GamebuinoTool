using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public Image Warning;
	bool AssetSelected = false;

	// Use this for initialization
	void Start () {
		SetSaveInProject(true);
		PlayerPrefs.SetString("S_1",string.Empty);
	}

	public void CreateAsset () {
		if(!string.IsNullOrEmpty(PlayerPrefs.GetString("S_1")) && AssetSelected) {
			switch(PlayerPrefs.GetInt("S_0")) {
			case 0:
				SceneManager.LoadScene("Tile");
				break;
			case 1:
				SceneManager.LoadScene("Image");
				break;
			default:
				SceneManager.LoadScene("SlapNgo");
				break;
			}
		} else {
			StartCoroutine(WarningFlash());
		}
	}

	public void OpenProjectMenu () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("ProjectFolder");
	}

	public void SetAssetType (int ID) {
		AssetSelected = true;
		PlayerPrefs.SetInt("S_0",ID);
	}

	public void SetAssetName (string Name) {
		PlayerPrefs.SetString("S_1",Name);
	}

	public void SetSaveInProject(bool Status) {
		PlayerPrefs.SetInt("S_2",System.Convert.ToInt32(Status));
	}

	bool IsFlashing = false;
	IEnumerator WarningFlash () {
		if(IsFlashing) {
			yield break;
		}

		IsFlashing = true;
		Warning.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.08f);
		Warning.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.08f);
		Warning.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.08f);
		Warning.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.08f);
		Warning.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.08f);
		Warning.gameObject.SetActive(false);
		IsFlashing = false;
	}
}
