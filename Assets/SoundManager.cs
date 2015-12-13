using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{
	public AudioSource sfxSource;
	public AudioSource sfxSource2;
	public AudioSource sfxSource3;
	public AudioSource musicSource;
	public static SoundManager instance = null;			
	public float lowPitchRange = .95f;
	public float highPitchRange = 1.05f;


	void Awake ()
	{
		instance = this;
	}


	public void PlayMusic(AudioClip clip)
	{
		musicSource.clip = clip;
		musicSource.Play ();
	}

	public void PlaySingle(AudioClip clip)
	{
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);
		AudioSource pSource;
		if (!sfxSource.isPlaying)
			pSource = sfxSource;
		else if (!sfxSource2.isPlaying)
			pSource = sfxSource2;
		else
			pSource = sfxSource3;
		
		pSource.pitch = randomPitch;
		pSource.clip = clip;
		pSource.Play ();
	}


	//RandomizeSfx chooses randomly between various audio clips and slightly changes their pitch.
	public void RandomizeSfx (params AudioClip[] clips)
	{
		//Generate a random number between 0 and the length of our array of clips passed in.
		int randomIndex = Random.Range(0, clips.Length);

		//Choose a random pitch to play back our clip at between our high and low pitch ranges.
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);

		//Set the pitch of the audio source to the randomly chosen pitch.
		sfxSource.pitch = randomPitch;

		//Set the clip to the clip at our randomly chosen index.
		sfxSource.clip = clips[randomIndex];

		//Play the clip.
		sfxSource.Play();
	}
}