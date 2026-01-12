using UnityEngine;

public class PieceManager : MonoBehaviour
{
    // メンバ変数
    
    [SerializeField] PuzzlePiece _piecePrefab;// 生成するピースの元データ（プレハブ）をInspectorで設定できるようにする
    PuzzlePiece[] _pieces; // パズルピースの配列
    [SerializeField] Sprite[] _pieceImages;    // 分割された画像リスト（正解順）pieces_に渡す。0は空。要素数はEditorで決定
    [SerializeField] int _width;    // エディター側で幅を設定
    [SerializeField] int _height;    // エディター側で高さを設定
    [SerializeField] float _padding = 0.05f; // ピース間の隙間
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 盤面の初期生成と配置

        int totalCellNum = _width * _height;
        // 画像の枚数が合っているかチェック。
        if (_pieceImages.Length != totalCellNum - 1)
        {
            Debug.Assert(false, "[u8]スプライトの数とパズルの数が合いません\n");
            return;
        }

        // セルの大きさを取得(スプライトの大きさが見た目のセルの大きさになるのでスプライトから取得)
        Vector2 originalSize = _pieceImages[0].bounds.size; // bounds:AABBのサイズを取得
        Vector2 scale = _piecePrefab.transform.localScale; // 設定されたスケールを取得
        Vector2 oneCellSize = originalSize * scale + new Vector2(_padding, _padding);
        // 盤面が画面中央になるように、左上のセルの位置を決定
        // 端のピースの中心から反対側の端のピースの中心までの距離を計算し、その半分が中心からのオフセットになる
        Vector2 top_left = new Vector2(-(_width - 1) / 2.0f * oneCellSize.x, -(_height - 1) / 2.0f * oneCellSize.y);

        _pieces = new PuzzlePiece[totalCellNum];   /// 空白の部分も含めて配列を確保
        _pieces[0] = null;  // 最初のマスは空っぽ
        for (int i = 1; i < totalCellNum; ++i)
        {
            // プレハブの内容をすべてコピーして新しいインスタンスを生成。(第二引数にtransformを渡しているため、親をManagerに設定)
            _pieces[i] = Instantiate(_piecePrefab, transform);
            int col = i % _width; // 列番号
            int row = i / _width; // 行番号
            Vector2 pos = new Vector2(top_left.x + col * oneCellSize.x, top_left.y + row * oneCellSize.y);
            _pieces[i].transform.localPosition = pos; // ローカル座標系で位置を設定
            _pieces[i].Initialize(i, _pieceImages[i - 1]); // 初期化
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
