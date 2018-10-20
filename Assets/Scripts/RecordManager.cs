using UnityEngine;

///-----------------------------------------------------------
/// <summary>録音管理</summary>
///-----------------------------------------------------------
public class RecordManager : MonoBehaviour
{
    //============================================================
    //公開変数
    //============================================================

    #region PUBLIC_PARAMETERS

    //最大の録音時間
    public int maxDuration;

    //音声データ
    public AudioClip audioClip;

    #endregion

    //============================================================
    //非公開変数
    //============================================================

    #region PRIVATE_PARAMETERS

    //録音のサンプリングレート
    private const int sampleRate = 16000;

    //マイクのデバイス名
    private string mic;

    //再生用オーディオソース
    private AudioSource audioSource;

    #endregion

    //============================================================
    //公開メソッド
    //============================================================

    #region PUBLIC_METHODS

    ///-----------------------------------------------------------
    /// <summary>初期化処理</summary>
    ///-----------------------------------------------------------
    void Start()
    {
        //再生用オーディオソース
        this.audioSource = GetComponent<AudioSource>();
    }

    ///-----------------------------------------------------------
    /// <summary>録音開始</summary>
    ///-----------------------------------------------------------
    public void StartRecord()
    {
        //マイク存在確認
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("マイクが見つかりません");
            return;
        }

        //マイク名
        mic = Microphone.devices[0];

        //録音開始。audioClipに音声データを格納。
        audioClip = Microphone.Start(mic, false, maxDuration, sampleRate);
    }

    ///-----------------------------------------------------------
    /// <summary>録音終了</summary>
    ///-----------------------------------------------------------
    public void StopRecord()
    {
        //マイクの録音位置を取得
        int position = Microphone.GetPosition(mic);

        //マイクの録音を強制的に終了
        Microphone.End(mic);

        //シーク位置を検査
        if (position > 0)
        {
            //再生時間を確認すると、停止した時間に関わらず、maxDurationの値になっている。これは無音を含んでいる？
            Debug.Log("修正前の録音時間: " + audioClip.length);

            //音声データ一時退避用の領域を確保し、audioClipからのデータを格納
            float[] soundData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(soundData, 0);

            //新しい音声データ領域を確保し、positonの分だけ格納できるサイズにする。
            float[] newData = new float[position * audioClip.channels];

            //positionの分だけデータをコピー
            for (int i = 0; i < newData.Length; i++)
            {
                newData[i] = soundData[i];
            }

            //新しいAudioClipのインスタンスを生成し、音声データをセット
            AudioClip newClip = AudioClip.Create(audioClip.name, position, audioClip.channels, audioClip.frequency, false);
            newClip.SetData(newData, 0);

            //audioClipを新しいものに差し替え
            AudioClip.Destroy(audioClip);
            audioClip = newClip;

            //再生時間
            Debug.Log("修正後の録音時間: " + newClip.length);
        }
    }

    ///-----------------------------------------------------------
    /// <summary>再生</summary>
    ///-----------------------------------------------------------
    public void Play()
    {
        //音声データ存在確認
        if (audioClip == null)
        {
            Debug.Log("音声データが見つかりません。");
            return;
        }

        //再生
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    #endregion
}
