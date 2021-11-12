using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundManager
{
    public class SFXPlayer : MonoBehaviour
    {
        private AudioSource audioSource = null;
        private IEnumerator isPlay = null;


        private void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
        }

        public void StartPlay(AudioClip clip)
        {
            this.gameObject.SetActive(true);
            isPlay = WaitForPlaying(clip);
            StartCoroutine(isPlay);
        }

        public void StopPlay()
        {
            if(isPlay != null)
            {
                StopCoroutine(isPlay);
                isPlay = null;
                this.gameObject.SetActive(false);
            }
        }

        private IEnumerator WaitForPlaying(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);

            isPlay = null;
            this.gameObject.SetActive(false);

            yield return null;
        }

        public AudioClip GetNowAudioClip()
        {
            return audioSource.clip;
        }
    }
}
