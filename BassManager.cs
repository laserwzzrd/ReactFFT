using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Un4seen;
using Un4seen.Bass;


public class BassManager : MonoBehaviour 
{
	public List<string> streams;
	public Thread bassThread;
	public bool bassThreadRunning = false;
	public System.Random rand;
	public int bassChannel = -1;
	public int bassStreamQueue = -1;
	public float[] bassData;
	public float bassDataSum = 0.0f;
	public int bassDataSize = 256;
	public float bassFFTBass = 0.0f;
	public float bassFFTMid = 0.0f;
	public float bassFFTTreble = 0.0f;
	public BASSActive bassActive;
	public float musicVolume = 0.5f;
	public float baseMusicVolume = 0.5f;

	public bool triggerSongIndicator = false;
	public bool songIndicatorActive = false;
	public Transform songIndicator;
	public TMPro.TextMeshPro songIndicatorTitle;
	public Transform songIndicatorAlbumImage;
	public float songIndicatorSlideTimer = 0.0f;
	public float songIndicatorSlideDelay = 1.0f;
	public float songIndicatorDisplayTimer = 0.0f;
	public float songIndicatorDisplayDelay = 1.0f;
	public AnimationCurve songIndicatorSlideCurve;
	public bool songIndicatorSlidingIn = true;
	public bool songIndicatorDisplay = false;
	public float songIndicatorSlideLength = 4.0f;
	public Vector3 songIndicatorOriginalPos;
	public Vector3 genericVec = Vector3.zero;
	public Texture2D bitBurner;
	public Texture2D homeLogo;

	public string cachedStreamingPath;

	[System.Serializable]
	public class AudioStream
	{
		public string name = "";
		public string url;
		public string bitrate = "128k";
		public Transform streamButton;
	}
	public List<AudioStream> audioStreams;

	[System.Serializable]
	public class MusicArtist
	{
		public string name = "";
		public Texture2D artistLogo;
		public string url = "";
	}

	[System.Serializable]
	public class MusicTrack
	{
		public string filePath = "";
	}
	public MusicTrack currentTrack;
	public List<MusicTrack> safeZoneTracks;
	public string safeZoneTracksRoot = "";
	public List<MusicTrack> dangerZoneTracks;
	public string dangerZoneTracksRoot = "";
	public int lastPlayedIndex = -1;

	public List<string> localAudioFiles;
	public bool usingLocalAudio = false;


	// Use this for initialization
	void Start () 
	{
		songIndicatorOriginalPos = songIndicator.localPosition;
		songIndicator.gameObject.SetActive(false);
		cachedStreamingPath = Application.streamingAssetsPath;
		bassData = new float[bassDataSize];
		for(int i = 0; i < bassDataSize; i++)
		{
			bassData[i] = 0.0f;
		}
		bassThread = new Thread(new ThreadStart(BassThread));
		bassThread.Start();

		UpdateStreamButtons();
	}


	public void UpdateStreamButtons()
	{
		foreach(AudioStream stream in audioStreams)
		{
			//stream.streamButton.Find("Label").GetComponent<UILabel>().text = stream.name;
		}
	}


	public void BassThread()
	{
		bassThreadRunning = true;
		rand = new System.Random();

		Un4seen.Bass.BassNet.Registration("hamburgerbum@gmail.com", "2X3132222152222");
		bool bassInit = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero);
		//Bass.BASS_SetVolume(musicVolume);

		if(bassInit == true)
		{
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 1);
			if(usingLocalAudio == true)
			{
				PlayFile(cachedStreamingPath+"/"+localAudioFiles[rand.Next(0, localAudioFiles.Count)]);
			}
			else
			{
				PlayRandomStream();
			}
		}
		else
		{
			//bassErrorCode = Bass.BASS_ErrorGetCode();
		}

		Thread.Sleep(10);
		bassThreadRunning = false;

	}


	// Update is called once per frame
	void Update ()
	{
		if(1 == 1)//bassThreadRunning == true)
		{
			Bass.BASS_ChannelGetData(bassChannel, bassData, (int)BASSData.BASS_DATA_FFT256);// -2147483648);	// last param is magic number for FFT256, BassData won't let you convert?
			bassDataSum = 0;
			bassFFTBass = 0;
			bassFFTMid = 0;
			bassFFTTreble = 0;
			for(int i = 0; i < bassDataSize; i++)
			{
				bassDataSum += bassData[i];
				if(i < 25)
				{ 
					bassFFTBass += bassData[i];
					bassFFTMid += bassData[i] * 0.1f;
				}
				else if(i > 45)
				{
					bassFFTTreble += bassData[i];
					bassFFTMid += bassData[i] * 0.1f;
				}
				else
				{
					bassFFTMid += bassData[i] * 2.0f;
				}
			}

			//bassFFTBass += 1376256;
			//bassFFTBass *= 0.000000000003f;
			//bassFFTTreble += 1376256;
			//bassFFTTreble *= 0.000000000003f;
			//bassFFTMid += 1376256;
			//bassFFTMid *= 0.000000000003f;
			//Debug.Log("SUM: "+bassDataSum.ToString()+" DATA[0]: " + bassData[0].ToString() + " DATA[1]: " + bassData[1].ToString() + " DATA[2]: " + bassData[2].ToString());
		}

		bassActive = Bass.BASS_ChannelIsActive(bassChannel);
		if(bassActive == BASSActive.BASS_ACTIVE_STOPPED)
		{
			PlayStreamInThread(0);
		}
	}

	public void LateUpdate()
	{
		if(triggerSongIndicator == true)
		{
			triggerSongIndicator = false;
			ShowSongIndicator();
		}

		if(songIndicatorActive == true && songIndicator != null)
		{
			if(songIndicator.gameObject.activeInHierarchy == true)
			{
				songIndicatorSlideTimer += Time.deltaTime;
				if(songIndicatorSlideTimer < songIndicatorSlideDelay)
				{
					if(songIndicatorSlidingIn == true)
					{
						genericVec.x = songIndicatorSlideCurve.Evaluate(1.0f - (songIndicatorSlideTimer / songIndicatorSlideDelay)) * songIndicatorSlideLength;
						songIndicator.localPosition = songIndicatorOriginalPos + genericVec;
					}
					else
					{
						genericVec.x = songIndicatorSlideCurve.Evaluate(songIndicatorSlideTimer / songIndicatorSlideDelay) * songIndicatorSlideLength;
						songIndicator.localPosition = songIndicatorOriginalPos + genericVec;
					}
				}
				else
				{
					if(songIndicatorSlidingIn == false)
					{
						songIndicatorActive = false;
						songIndicatorSlidingIn = true;
						songIndicator.gameObject.SetActive(false);
					}

					songIndicatorDisplayTimer += Time.deltaTime;
					if(songIndicatorDisplayTimer < songIndicatorDisplayDelay)
					{
						songIndicator.localPosition = songIndicatorOriginalPos;
					}
					else
					{
						songIndicatorSlidingIn = false;
						songIndicatorSlideTimer = 0.0f;
					}
				}
			}
		}
	}


	public bool PlayRandomStream()
	{
		for(int i = 0; i < 10; i++)
		{
			bool playing = PlayStream(audioStreams[rand.Next(0, audioStreams.Count)].url);  
			if(playing == true) 
			{ 
				return true; 
			}
		}
		return false;
	}


	public bool PlayStream(string stream)
	{
		if(bassChannel != -1)
		{
			Bass.BASS_ChannelStop(bassChannel);
		}
		bassChannel = Bass.BASS_StreamCreateURL(stream, 0, BASSFlag.BASS_SAMPLE_FLOAT, null, System.IntPtr.Zero);
		Bass.BASS_ChannelSetAttribute(bassChannel, BASSAttribute.BASS_ATTRIB_VOL, musicVolume);
		return Bass.BASS_ChannelPlay(bassChannel, false);
	}


	public bool PlayFile(string filePath)
	{
		if(System.IO.File.Exists(filePath) == false)
		{
			return false;
		}

		if(bassChannel != -1)
		{
			Bass.BASS_ChannelStop(bassChannel);
		}
		bassChannel = Bass.BASS_StreamCreateFile(filePath, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);// System.IntPtr.Zero);
		Bass.BASS_ChannelSetAttribute(bassChannel, BASSAttribute.BASS_ATTRIB_VOL, musicVolume);
		return Bass.BASS_ChannelPlay(bassChannel, false);
	}


	public void SetMusicVolume(float volume)
	{
		Bass.BASS_ChannelSetAttribute(bassChannel, BASSAttribute.BASS_ATTRIB_VOL, baseMusicVolume * volume);
	}


	public void PlayStreamInThread(int stream)
	{
		bassStreamQueue = stream;
		if(bassThreadRunning == false)
		{
			bassThread = new Thread(new ThreadStart(PlayQueueThread));
			bassThread.Start();
		}
	}


	public void ShowSongIndicator()
	{
		songIndicatorActive = true;
		songIndicatorSlidingIn = true;
		songIndicatorDisplay = true;
		songIndicatorSlideTimer = 0.0f;
		songIndicatorDisplayTimer = 0.0f;
		songIndicator.gameObject.SetActive(true);
		songIndicatorTitle.text = Path.GetFileName(currentTrack.filePath).Replace(".mp3", ""); 
		songIndicatorAlbumImage.GetComponent<Renderer>().material.mainTexture = homeLogo;
	}


	public void PlayQueueThread() 
	{
		Debug.Log("[PlayQueueThread launched]");
		bassThreadRunning = true;
		if(usingLocalAudio == false)
		{
			PlayStream(audioStreams[bassStreamQueue].url);
		}
		else
		{
			int newPlayIndex = rand.Next(0, safeZoneTracks.Count);
			for(int i = 0; i < 100; i++)
			{
				if(newPlayIndex == lastPlayedIndex)
				{
					newPlayIndex = rand.Next(0, safeZoneTracks.Count);
				}
			}

			currentTrack = safeZoneTracks[rand.Next(0, safeZoneTracks.Count)];
			string trackStr = cachedStreamingPath+"/"+safeZoneTracksRoot+"/"+currentTrack.filePath;
			PlayFile(trackStr);
			lastPlayedIndex = newPlayIndex;
			Debug.Log("[PlayQueueThread]: safe zone track ("+trackStr+") started");

			triggerSongIndicator = true;
		}
		bassThreadRunning = false;
	}


	void OnDisable()
	{
		Debug.Log("BASS ERRROR CODE: " + Bass.BASS_ErrorGetCode().ToString());

		Bass.BASS_Stop();
		Bass.BASS_Free();
	}


	public void PlayStream01() { PlayStreamInThread(0); }
	public void PlayStream02() { PlayStreamInThread(1); }
	public void PlayStream03() { PlayStreamInThread(2); }
	public void PlayStream04() { PlayStreamInThread(3); }
	public void PlayStream05() { PlayStreamInThread(4); }
	public void PlayStream06() { PlayStreamInThread(5); }
	public void PlayStream07() { PlayStreamInThread(6); }
	public void PlayStream08() { PlayStreamInThread(7); }
	public void PlayStream09() { PlayStreamInThread(8); }
	public void PlayStream10() { PlayStreamInThread(9); }
}
