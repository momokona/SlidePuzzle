using UnityEngine;
// パズルピース一つのクラス
// 自分の見た目を表示し、セルIDを保持する
public class PuzzlePiece : MonoBehaviour
{
    // スプライトを描画するためのコンポーネント
    [SerializeField] SpriteRenderer sprite_renderer_;    // SerializeFieldをつけておくことでUnityがロード時にsprite_rendrer_ にポインタを入れてくれる
    int id_;   // ID

    public int GetCellId()
    {
        return id_;
    }

    // 外部から呼ばれてスプライトとIDをセット(スプライトはセルごとに異なるので、セットしてもらう必要がある)
    public void Initialize(int id, Sprite sprite)
    {
        id_ = id;
        sprite_renderer_.sprite = sprite;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 必要に応じて _spriteRenderer を使用する処理をここに追加
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
