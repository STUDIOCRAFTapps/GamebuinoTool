using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorRect : MonoBehaviour {

	public CutManager cutManager;

	public float Scale = 3.125f;
	public float Rescaler = 1f;

	Vector2 OldPos;
	Vector2 OldRectPos;
	Vector2 OldRectSize;

	RectTransform rt;

	public string Name;

	void Start () {
		rt = GetComponent<RectTransform>();
	}

	public void StartDrag () {
		cutManager.RelocateSelector(this);
		GetComponent<Image>().color = cutManager.colorSelected;

		OldPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		OldRectPos = rt.anchoredPosition;
		OldRectSize = rt.sizeDelta;
	}

	public void Drag (int Part) {
		switch(Part) {
		case 0:
			rt.anchoredPosition = new Vector2(OldRectPos.x+(Input.mousePosition.x-OldPos.x)/cutManager.ContentBox.localScale.x,OldRectPos.y+(Input.mousePosition.y-OldPos.y)/cutManager.ContentBox.localScale.y);
			break;
		case 1:
			rt.sizeDelta = new Vector2(OldRectSize.x+(Input.mousePosition.x-OldPos.x)/cutManager.ContentBox.localScale.x,OldRectSize.y-(Input.mousePosition.y-OldPos.y)/cutManager.ContentBox.localScale.y);
			break;
		}
		rt.anchoredPosition = VectorRound(rt.anchoredPosition,Scale*Rescaler);
		rt.sizeDelta = VectorRound(rt.sizeDelta,Scale);
		rt.anchoredPosition = new Vector2(Mathf.Clamp(rt.anchoredPosition.x,0f,cutManager.ImageDisplay.texture.width*Scale),Mathf.Clamp(rt.anchoredPosition.y,-cutManager.ImageDisplay.texture.height*Scale,0f));
		rt.sizeDelta = new Vector2(Mathf.Clamp(rt.sizeDelta.x,Scale,Mathf.Infinity),Mathf.Clamp(rt.sizeDelta.y,Scale,Mathf.Infinity));
	}

	public void SetPosition (int x, int y) {
		GetComponent<RectTransform>().anchoredPosition = new Vector2(x*Scale, -y*Scale);
	}

	public void SetSize (int width, int height) {
		GetComponent<RectTransform>().sizeDelta = new Vector2(width*Scale,height*Scale);
	}

	public void EndDrag () {
		
	}

	public void Click () {
		if(Input.GetMouseButtonUp(1)) {
			cutManager.OpenSelectorInfo(this);
		} else {
			cutManager.RelocateSelector(this);
			GetComponent<Image>().color = cutManager.colorSelected;
		}
	}

	Vector2 VectorRound(Vector2 Value, float Steps) {
		return new Vector2(SmartRound(Value.x,Steps),SmartRound(Value.y,Steps));
	}

	float SmartRound(float Value, float Steps) {
		return Steps*Mathf.Round(Value/Steps);
	}
}
