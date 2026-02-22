using UnityEngine;
using System.Collections;

public class ClearAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip firstClip;
    public AudioClip secondClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayAudioSequence());
    }
    IEnumerator PlayAudioSequence()
    {
        // 1. 最初のクリップを再生
        audioSource.clip = firstClip;
        audioSource.loop = false; // 1曲目はループさせない
        audioSource.Play();

        // 2. イントロが終わるまで待機
        yield return new WaitForSeconds(firstClip.length);

        // 3. 2曲目をセットしてループ再生を開始
        audioSource.clip = secondClip;
        audioSource.loop = true;  // ここでループをONにする
        audioSource.Play();
    }
}
