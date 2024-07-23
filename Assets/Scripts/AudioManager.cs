using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get { return instance; }
    }

    private AudioSource audioSource;
    private AudioClip audioKang, audioPong, audioChao, audioWin;
    private AudioClip audioMovement, audioCollision;
    public void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();

        audioKang = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/kang-2.mp3");
        audioPong = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/pong-2.mp3");
        audioChao = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/chao-2.mp3");
        audioWin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/Thunder.mp3");

        audioMovement = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/Move 5.mp3");
        audioCollision = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Objects/Sounds/Move 2.mp3");
    }
    public void playKang()
    {
        audioSource.clip = audioKang;
        audioSource.Play();
    }
    
    public void playPong()
    {
        audioSource.clip = audioPong;
        audioSource.Play();
    }

    public void playChao()
    {
        audioSource.clip = audioChao;
        audioSource.Play();
    }

    public void playWin()
    {
        audioSource.clip = audioWin;
        audioSource.Play();
    }

    public void soundMovement()
    {
        if (audioSource.isPlaying && audioSource.clip != audioMovement)
        {
            return;
        }
        audioSource.clip = audioMovement;
        audioSource.Play();
    }

    public void soundCollision()
    {
        if (! audioSource.isPlaying)
        {
            audioSource.clip = audioCollision;
            audioSource.Play();
        }
    }
}
