using UnityEngine;
using SlidePuzzle.Common;

// パズルピース一つのクラス
// 自分の見た目を表示し、セルIDを保持する
public class PuzzlePiece : MonoBehaviour
{
    // スプライトを描画するためのコンポーネント
    [SerializeField] SpriteRenderer _spriteRenderer;    // SerializeFieldをつけておくことでUnityがロード時にsprite_rendrer_ にポインタを入れてくれる
    int _id;   // ID
    bool _isDragging = false; // 移動中かどうか
    Vector3 _offset;        // マウス位置とピース中心のズレ

    public int GetId()
    {
        return _id;
    }

    // 外部から呼ばれてスプライトとIDをセット(スプライトはセルごとに異なるので、セットしてもらう必要がある)
    public void Initialize(int id, Sprite sprite)
    {
        _id = id;
        _spriteRenderer.sprite = sprite;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 必要に応じて _spriteRenderer を使用する処理をここに追加
    }



    // Update is called once per frame
    void Update()
    {
        if(!_isDragging)
        {
            return; // 移動中でなければ何もしない
        }
        // マウスの動きに追従させる
        Vector3 mousePos = PuzzleSolver.GetMouseWorldPos();
        Vector3 targetPos = mousePos + _offset;
        transform.position = targetPos;
    }

    public void StartDragging()
    {
        _isDragging = true;
        Vector3 mouseWorldPos = PuzzleSolver.GetMouseWorldPos();
        // ピースの中心とクリック位置のズレを計算
        _offset = transform.position - mouseWorldPos;
        // 描画順を一番手前にする（他のピースの裏に隠れないように）
        _spriteRenderer.sortingOrder = Defs.SELECT_CELL_SORT_ORDER;
    }

    public void EndDragging()
    {
        _isDragging = false;
        // 描画順を元に戻す
        _spriteRenderer.sortingOrder = 0;
    }

    public bool IsEmptyPiece()
    {
        return _id == 0;
    }
}
