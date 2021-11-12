using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SoundManager
{
	enum GroupIndex 
	{ 
		Master, 
		BGM, 
		SFX
	};

	public class SoundManager : MonoBehaviour
	{
		[Header("SFX Player")]
		[SerializeField] private SFXPlayer sfxPlayer = default;
		[SerializeField] private int sfxPlayerInitialSize = default;
		[SerializeField] private Transform sfxPlayerGroup = default;
		private List<SFXPlayer> sfxPlayerList = null;

		[Header("Sound controller")]
		[SerializeField] private SoundControllerSO controller = default;

		[Space]
		[SerializeField] private AudioMixer audioMixer = default;
		[SerializeField] private AudioSource currentBGM = default;
		[SerializeField] private AudioSource previousBGM = default;

		[Header("Master, BGM, SFX")]
		[Range(0f, 1f)]
		[SerializeField] private float[] volumes = { 1f, 1f, 1f };
		private bool[] mutes = { false, false, false };

		private IEnumerator fadeCoroutine = null;

        private void Awake()
        {
			sfxPlayerList = new List<SFXPlayer>();

			for (int i = 0; i < sfxPlayerInitialSize; i++)
			{
				AddSFXPlayer();
			}
        }

        private void OnEnable()
		{
			controller.OnEventChangeVolume += ChangeVolume;
			controller.OnEventMuteVolume += MuteVolume;

			controller.OnEventPlaySFX += PlaySFX;
			controller.OnEventPlayOneSFX += PlayOneSFX;

			controller.OnEventPlayBGM += PlayBGM;
			controller.OnEventPauseBGM += PauseBGM;
			controller.OnEventRestartBGM += RestartBGM;

			
		}

		private void OnDestroy()
		{
			controller.OnEventChangeVolume -= ChangeVolume;
			controller.OnEventMuteVolume -= MuteVolume;

			controller.OnEventPlaySFX -= PlaySFX;
			controller.OnEventPlayOneSFX -= PlayOneSFX;

			controller.OnEventPlayBGM -= PlayBGM;
			controller.OnEventPauseBGM -= PauseBGM;
			controller.OnEventRestartBGM -= RestartBGM;
		}

        #region BGM

        private void StartFade()
        {
			fadeCoroutine = Fade();
			StartCoroutine(fadeCoroutine);
        }

		private void StopFade()
        {
			if(fadeCoroutine != null)
            {
				StopCoroutine(fadeCoroutine);
				fadeCoroutine = null;
			}
        }

		private IEnumerator Fade()
		{
			float nowVolume = currentBGM.volume;

			while(true)
            {
				nowVolume += Time.deltaTime;

				currentBGM.volume = nowVolume;
				previousBGM.volume = 1 - nowVolume;

				if(nowVolume > 1)
                {
					break;
				}
				yield return null;
			}

			previousBGM.Pause();
			fadeCoroutine = null;

			yield return null;
		}

		private void ChangeCurrentBGM()
		{
			AudioSource temp = currentBGM;
			currentBGM = previousBGM;
			previousBGM = temp;
		}

		private void PlayBGM(AudioClip bgmClip)
		{
			if (currentBGM.isPlaying)
			{
				if(currentBGM.clip != bgmClip)
                {
					ChangeCurrentBGM();

					if (currentBGM.clip != bgmClip)
					{
						currentBGM.clip = bgmClip;
						currentBGM.Play();
					}
					else
					{
						if (!currentBGM.isPlaying)
						{
							currentBGM.Play();
						}
					}

					StopFade();
					StartFade();
				}
			}
            else
			{
				currentBGM.clip = bgmClip;
				currentBGM.Play();

				StopFade();
				StartFade();
			}
		}

		private void PauseBGM()
		{
			if(currentBGM.isPlaying)
			{
				currentBGM.Pause();
			}
		}

		private void RestartBGM()
		{
			if (!currentBGM.isPlaying)
			{
				currentBGM.Play();
			}
		}

        #endregion

        #region Volume
        private void ChangeVolume(int index, float value)
		{
			volumes[index] = value;
			if (!mutes[index])
			{
				SetGroupVolume(((GroupIndex)index).ToString(), volumes[index]);
			}
		}

		private void MuteVolume(int index, bool value)
		{
			mutes[index] = value;
			if (value)
			{
				SetGroupVolume(((GroupIndex)index).ToString(), 0);
			}
			else
			{
				SetGroupVolume(((GroupIndex)index).ToString(), volumes[index]);
			}
		}

		public void SetGroupVolume(string groupName, float volumeValue)
		{
			bool volumeSet = audioMixer.SetFloat(groupName, NormalizedToMixerValue(volumeValue));
			if (!volumeSet)
				Debug.LogError("The AudioMixer parameter was not found");
		}

		#endregion

		#region SFX

		private void AddSFXPlayer()
		{
			var tempSfxPlayer = Instantiate(sfxPlayer, sfxPlayerGroup);
			sfxPlayerList.Add(tempSfxPlayer);
			tempSfxPlayer.gameObject.SetActive(false);
		}

		private void PlaySFX(AudioClip sfxClip)
		{
			bool isFull = true;
			for (int i = 0; i < sfxPlayerList.Count; i++)
            {
				if(!sfxPlayerList[i].gameObject.activeSelf)
                {
					sfxPlayerList[i].StartPlay(sfxClip);
					isFull = false;
					break;
                }
            }

			if(isFull)
            {
				AddSFXPlayer();
				sfxPlayerList[sfxPlayerList.Count-1].StartPlay(sfxClip);
			}
		}

		private void PlayOneSFX(AudioClip sfxClip)
		{
			bool isExist = true;
			for (int i = 0; i < sfxPlayerList.Count; i++)
			{
				if (sfxPlayerList[i].gameObject.activeSelf)
				{
					if(sfxPlayerList[i].GetNowAudioClip() == sfxClip)
                    {
						sfxPlayerList[i].StopPlay();
						sfxPlayerList[i].StartPlay(sfxClip);
						isExist = false;
						break;
					}
				}
			}

			if (isExist)
			{
				PlaySFX(sfxClip);
			}
		}

		private void ChangeListIndex(int index)
        {

        }

		#endregion

		private float NormalizedToMixerValue(float volumeValue)
		{
			// We're assuming the range [0 to 1] becomes [-80dB to 0dB]
			// This doesn't allow values over 0dB
			return (volumeValue - 1f) * 80f;
		}
	}
}