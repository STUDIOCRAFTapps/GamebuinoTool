using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Talker : MonoBehaviour {

	public AudioClip[] clips;
	bool[] clipsRemaining;
	AudioSource audios;

	public float Timer = 0.1f;

	// Use this for initialization
	void Start () {
		audios = GetComponent<AudioSource>();

		clipsRemaining = new bool[clips.Length];
		for(int i = 0; i < clipsRemaining.Length; i++) {
			clipsRemaining[i] = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!audios.isPlaying) {
			StartCoroutine(SoundPlayer());
		}
	}

	bool canCallPlayer = true;
	IEnumerator SoundPlayer () {
		if(canCallPlayer == false) {
			yield break;
		}
		canCallPlayer = false;

		if(!clipsRemaining.ToList().Contains(true)) {
			for(int i = 0; i < clipsRemaining.Length; i++) {
				clipsRemaining[i] = true;
			}

			int clipIndex = Random.Range(0,clips.Length);
			audios.PlayOneShot(clips[clipIndex]);
			clipsRemaining[clipIndex] = false;
		} else {
			while(true) {
				int clipIndex = Random.Range(0,clips.Length);
				if(clipsRemaining[clipIndex] == true) {
					audios.PlayOneShot(clips[clipIndex]);
					clipsRemaining[clipIndex] = false;

					yield return new WaitForSeconds(Timer);

					break;
				}
			}
		}

		canCallPlayer = true;
	}
}
