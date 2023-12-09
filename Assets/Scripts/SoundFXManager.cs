using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class SoundFXManager : MonoBehaviour
    {
        private AudioSource audioSource;
        public AudioClip rollSFX;


        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayRollSFX()
        {
            audioSource.PlayOneShot(rollSFX);
        }
    }
}
