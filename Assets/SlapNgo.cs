using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlapNgo : MonoBehaviour {

	public string InputText;
	public string OutputText;

	public string[] ReplaceFromTo;

	public bool SeparateBy2 = false;

	// Use this for initialization
	void Start () {
		OutputText = InputText;
		for(int i = 0; i < Mathf.Floor(ReplaceFromTo.Length/2)*2; i+=2) {
			OutputText = OutputText.Replace(ReplaceFromTo[i],ReplaceFromTo[i+1]);
		}

		if(SeparateBy2) {
			for(int i = 2; i < OutputText.Length; i+=3) {
				OutputText = OutputText.Insert(i,",");
			}
			for(int i = 0; i < OutputText.Length; i+=5) {
				OutputText = OutputText.Insert(i,"0x");
			}
		}
	}
}
