using UnityEngine;
using UnityEngine.Events;

namespace SoundManager
{
	[CreateAssetMenu(fileName = "SoundController", menuName = "SO/SoundManager/SoundController")]
	public class SoundControllerSO : ScriptableObject
	{
		public UnityAction<AudioClip> OnEventPlayBGM;
		public UnityAction OnEventPauseBGM;
		public UnityAction OnEventRestartBGM;

		public UnityAction<AudioClip> OnEventPlaySFX;
		public UnityAction<AudioClip> OnEventPlayOneSFX;
		public UnityAction OnEventPauseSFX;
		public UnityAction OnEventRestartSFX;

		public UnityAction<int,float> OnEventChangeVolume;
		public UnityAction<int,bool> OnEventMuteVolume;

		public void PlayBGM(AudioClip BgmClip)
		{
			if (OnEventPlayBGM != null)
				OnEventPlayBGM.Invoke(BgmClip);
		}

		public void PauseBGM()
		{
			if (OnEventPauseBGM != null)
				OnEventPauseBGM.Invoke();
		}

		public void RestartBGM()
		{
			if (OnEventRestartBGM != null)
				OnEventRestartBGM.Invoke();
		}


		public void PlaySFX(AudioClip SfxClip)
		{
			if (OnEventPlaySFX != null)
				OnEventPlaySFX.Invoke(SfxClip);
		}

		public void PlaySFXOne(AudioClip SfxClip)
		{
			if (OnEventPlayOneSFX != null)
				OnEventPlayOneSFX.Invoke(SfxClip);
		}


		public void ChangeMasterVolume(float value)
		{
			if (OnEventChangeVolume != null)
				OnEventChangeVolume.Invoke(0, value);
		}

		public void ChangeBGMVolume(float value)
		{
			if (OnEventChangeVolume != null)
				OnEventChangeVolume.Invoke(1, value);
		}

		public void ChangeSFXVolume(float value)
		{
			if (OnEventChangeVolume != null)
				OnEventChangeVolume.Invoke(2, value);
		}


		public void MuteMaster(bool value)
		{
			if (OnEventMuteVolume != null)
				OnEventMuteVolume.Invoke(0, value);
		}

		public void MuteBGM(bool value)
		{
			if (OnEventMuteVolume != null)
				OnEventMuteVolume.Invoke(1, value);
		}

		public void MuteSFX(bool value)
		{
			if (OnEventMuteVolume != null)
				OnEventMuteVolume.Invoke(2, value);
		}

	}
}