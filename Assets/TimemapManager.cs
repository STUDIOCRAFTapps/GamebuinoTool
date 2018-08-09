using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimemapManager : MonoBehaviour {

	public float TileScaling = 1.5f;
	public float TileSize = 32f;

	public GameObject TileSelectionPrefab;
	public GameObject TilePrefab;
	public InputField[] SizeField;
	public RectTransform ContentBox;
	public RectTransform ScrollRect;
	public RectTransform debugCursor;

	public RectTransform ContentBoxTileSelection;

	int SelectedTile = 0;
	List<Texture2D> TextureList;

	public byte[,] Tilemap;
	public RawImage[,] VisualMap;
	int SizeX = 32;
	int SizeY = 32;

	void Start () {
		VisualMap = new RawImage[0,0];
		LoadTiles();
	}

	void Update () {
		ContentBox.localScale = Vector3.one*Mathf.Clamp(ContentBox.localScale.x+Input.mouseScrollDelta.y*0.1f,0.1f,5f);
	}

	void LoadTiles ()  {
		StartCoroutine(LoadTilesIE());
	}

	IEnumerator LoadTilesIE () {
		//ImageDirectory\nName\nCutData
		List<GameObject> children = new List<GameObject>();
		foreach(Transform child in ContentBoxTileSelection) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));

		//Get all GTL files
		string[] files = Directory.GetFiles(PlayerPrefs.GetString("projectDirectory"));
		List<string> gtlfiles = new List<string>();
		for(int i = 0; i < files.Length; i++) {
			if(files[i].EndsWith(".gtl")) {
				gtlfiles.Add(files[i]);
			}
		}

		TextureList = new List<Texture2D>();

		for(int i = 0; i < gtlfiles.Count; i++) {
			string[] lines = File.ReadAllLines(gtlfiles[i]);

			WWW www = new WWW("file://" + lines[0]);
			yield return new WaitUntil(() => www.isDone);
			Texture2D tx = www.texture;
			Texture2D oldTx = tx;
			Color[] block = tx.GetPixels(
				int.Parse(lines[2].Split(';')[0]),
				oldTx.height-int.Parse(lines[2].Split(';')[1])-int.Parse(lines[2].Split(';')[3]),
				int.Parse(lines[2].Split(';')[2]),
				int.Parse(lines[2].Split(';')[3])
			);
			tx.Resize(
				int.Parse(lines[2].Split(';')[2]),
				int.Parse(lines[2].Split(';')[3])
			);
			tx.SetPixels(0,0,
				int.Parse(lines[2].Split(';')[2]),
				int.Parse(lines[2].Split(';')[3]),
				block
			);
			tx.Apply();
			tx.anisoLevel = 0;
			tx.filterMode = FilterMode.Point;

			TextureList.Add(tx);
			GameObject TileSelect = (GameObject)Instantiate(TileSelectionPrefab,ContentBoxTileSelection);
			TileSelect.transform.GetChild(0).GetComponent<RawImage>().texture = tx;
			int t = i;
			TileSelect.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
				SelectedTile = t;
			});
			TileSelect.transform.GetChild(1).GetComponent<Text>().text = lines[1];
		}

		UpdateVisualMap();
	}

	public void Save () {
		string MapResult = "";
		MapResult += "const byte " + ImageConverter.PrepareNameForCode(PlayerPrefs.GetString("S_1")) + "[] {\n\t"; //First line
		MapResult += SizeX.ToString() + ", " + SizeY.ToString() + ", "; //Header
		for(int i = 0; i < Mathf.Clamp(int.Parse(SizeField[2].text),0,255)-2; i++) {
			MapResult += "0, ";
		}
		MapResult += "\n\t";
		for(int y = 0; y < SizeY; y++) {
			for(int x = 0; x < SizeX; x++) {
				MapResult+=Tilemap[x,y].ToString()+",";
			}
		}
		if(MapResult.EndsWith(",")) {
			MapResult.Remove(MapResult.Length-1,1);
		}
		MapResult += "\n};";

		ClipboardHelper.clipBoard = MapResult;
	}

	public void ApplySize () {
		SizeX = Mathf.Clamp(int.Parse(SizeField[0].text),0,255);
		SizeY = Mathf.Clamp(int.Parse(SizeField[1].text),0,255);

		Tilemap = new byte[SizeX,SizeY];

		UpdateVisualMap();
	}

	public void SetTile () {
		debugCursor.transform.SetAsLastSibling();
		Vector3 cursorPos = Input.mousePosition;
		debugCursor.position = cursorPos;

		if(VisualMap.GetLength(0)*VisualMap.GetLength(1)==0) {
			return;
		}

		Vector2 pixPos = new Vector2(Mathf.Round(debugCursor.anchoredPosition.x/(TileSize/TileScaling)+0.5f)-1f,-Mathf.Round(debugCursor.anchoredPosition.y/(TileSize/TileScaling)+0.5f)+0f);
		VisualMap[Mathf.Clamp((int)pixPos.x,0,VisualMap.GetLength(0)-1),Mathf.Clamp((int)pixPos.y,0,VisualMap.GetLength(1)-1)].texture = 
			TextureList[SelectedTile];
		Tilemap[Mathf.Clamp((int)pixPos.x,0,VisualMap.GetLength(0)-1),Mathf.Clamp((int)pixPos.y,0,VisualMap.GetLength(1)-1)] = (byte)SelectedTile;
	}

	public void UpdateVisualMap () {
		Tilemap = new byte[SizeX,SizeY];
		ContentBox.GetComponent<LayoutElement>().minWidth = (SizeX-1)*TileSize/TileScaling;
		ContentBox.GetComponent<LayoutElement>().minHeight = (SizeY-1)*TileSize/TileScaling;

		foreach(RawImage obj in VisualMap) {
			Destroy(obj.gameObject);
		}

		VisualMap = new RawImage[SizeX,SizeY];
		for(int x = 0; x < SizeX; x++) {
			for(int y = 0; y < SizeY; y++) {
				GameObject Tile = (GameObject)Instantiate(TilePrefab,ContentBox);
				Tile.GetComponent<RectTransform>().anchoredPosition = new Vector2(x*(TileSize/TileScaling),y*-(TileSize/TileScaling));
				Tile.GetComponent<RectTransform>().sizeDelta = Vector2.one*TileSize;
				VisualMap[x,y] = Tile.GetComponent<RawImage>();
				VisualMap[x,y].texture = TextureList[0];
			}
		}
	}

	public void ChangeProject () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("ProjectFolder");
	}

	public void ReturnToMenu () {
		PlayerPrefs.SetString("lastLoadedScene", SceneManager.GetActiveScene().name);
		SceneManager.LoadScene("Main");
	}
}
