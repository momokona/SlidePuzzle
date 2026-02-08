using SlidePuzzle.Common;
using System.Collections.Generic; //Listを使うために必要
using Unity.InferenceEngine.Tokenization.PostProcessors.Templating;
using Unity.VisualScripting;
using UnityEngine;


namespace piece
{
    // 構造体自体も public にする必要がある
    // 行と列の場所
    public struct Pos
    {
        public int row ; // public をつける
        public int col; // public をつける
        // 有効な値か
        public bool IsValid()
        {
            return row != Defs.INVALID_ID && col != Defs.INVALID_ID;
        }
        public Pos(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }
}
public class PieceManager : MonoBehaviour
{
    // メンバ変数
    [SerializeField] PuzzlePiece _piecePrefab;// 生成するピースの元データ（プレハブ）をInspectorで設定できるようにする
    PuzzlePiece[] _pieces; // パズルピースの配列

    [SerializeField] Sprite[] _pieceImages;    // 分割された画像リスト（正解順）pieces_に渡す。0は空。要素数はEditorで決定
    [SerializeField] int _width;    // エディター側で幅を設定
    [SerializeField] int _height;    // エディター側で高さを設定
    [SerializeField] float _padding = 0.05f; // ピース間の隙間

    int _selectArrayIndex = Defs.INVALID_ID; // 選択されたピースのID。-1は未選択


    piece.Pos GetPosFromArrayIndex(int arrayIndex)
    {
        piece.Pos pos;
        pos.col = arrayIndex % _width; // 列番号
        pos.row = arrayIndex / _width; // 行番号
        return pos;
    }

    int GetArrayIndexFromPos(int row, int col)
    {
        return row * _width + col;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 盤面の初期生成と配置
        int totalCellNum = GetTotalCellNum();

        // 画像の枚数が合っているかチェック。
        if (_pieceImages.Length != totalCellNum - 1)
        {
            Debug.Assert(false, "[u8]スプライトの数とパズルの数が合いません\n");
            return;
        }


        Vector2 oneCellSize = GetOneCellSize();
        // 盤面が画面中央になるように、左上のセルの位置を決定
        Vector2 topLeft = GetTopLeftPos(oneCellSize);
        _pieces = new PuzzlePiece[totalCellNum];   /// 空白の部分も含めて配列を確保
        for (int i = 0; i < totalCellNum; ++i)
        {
            // プレハブの内容をすべてコピーして新しいインスタンスを生成。(第二引数にtransformを渡しているため、親をManagerに設定)
            _pieces[i] = Instantiate(_piecePrefab, transform);
            SetCellPos(oneCellSize, topLeft, i);
            // 空白(0番目)のときは画像nullを渡す
            Sprite img;
            if (i == Defs.EMPTY_ID)
            {
                img = null;
            }
            else if (i < Defs.EMPTY_ID)
            {
                img = _pieceImages[i];
            }
            else
            {
                img = _pieceImages[i - 1];
            }
            _pieces[i].Initialize(i, img);
        }

        do
        {
            // ピースをシャッフル
            for (int i = 1; i < totalCellNum; ++i)
            {
                // ピースをランダムに入れ替え
                int swapIndex = Random.Range(i, totalCellNum);
                SwapPiece(i, swapIndex);
            }
        } while (!PuzzleSolver.IsSolvablePuzzle(_pieces));
    }

    private int GetTotalCellNum()
    {
        return _width * _height;
    }

    Vector2 GetOneCellSize()
    {
        // セルの大きさを取得(スプライトの大きさが見た目のセルの大きさになるのでスプライトから取得)
        Vector2 originalSize = _pieceImages[0].bounds.size; // bounds:AABBのサイズを取得
        Vector2 scale = _piecePrefab.transform.localScale; // 設定されたスケールを取得
        Vector2 oneCellSize = originalSize * scale + new Vector2(_padding, _padding);
        return oneCellSize;
    }

    Vector2 GetTopLeftPos(Vector2 oneCellSize)
    {
        // 端のピースの中心から反対側の端のピースの中心までの距離を計算し、その半分が中心からのオフセットになる(Y座標は行番号と逆になる)
        return new Vector2(-(_width - 1) / 2.0f * oneCellSize.x, (_height - 1) / 2.0f * oneCellSize.y);
    }

    // 配列のインデックスに合わせて位置をセット
    void SetCellPos(Vector2 oneCellSize, Vector2 top_left, int i)
    {
        int col = i % _width; // 列番号
        int row = i / _width; // 行番号

        Vector2 pos = new Vector2(top_left.x + col * oneCellSize.x, top_left.y - row * oneCellSize.y);  // y座標は行が大きいほど下になる
        _pieces[i].transform.localPosition = pos; // ローカル座標系で位置を設定
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // マウスの左クリックが押されたとき
            DecidePiece();
        }

        else if (Input.GetMouseButtonUp(0))
        {
            // マウスの左クリックが離されたとき
            SwapPieceInGame();
        }
    }

    // ゲーム中でピースを入れ替える
    void SwapPieceInGame()
    {
        if (_selectArrayIndex == Defs.INVALID_ID)
        {
            return; // 有効なものを選択していなかった場合はreturn
        }
        _pieces[_selectArrayIndex].EndDragging();   // ドラッグ終了を伝える

        //piece.Pos nextPos = GetHitPiecePos();
        //if (!nextPos.IsValid())
        //{
        //    // 何も選択されていなかったら終了
        //    ResetSelect();
        //    return;
        //}
        //int nextIndex = GetArrayIndexFromPos(nextPos.row, nextPos.col);
        //if (_pieces[nextIndex].GetId() != Defs.EMPTY_ID)
        //{
        //    // 離された場所が空白でなかったら終了
        //    ResetSelect();
        //    return;
        //}

        List<int> surroundAreaList = GetSurroundAreaList(_selectArrayIndex);
        int emptyIndex = Defs.INVALID_ID;
        foreach (var surroundIndex in surroundAreaList)
        {
            if (_pieces[surroundIndex].GetId() == Defs.EMPTY_ID)
            {
                emptyIndex = surroundIndex;
                break;
            }
        }

        Vector2 oneCellSize = GetOneCellSize();
        Vector2 topLeft = GetTopLeftPos(oneCellSize);

        if (emptyIndex == Defs.INVALID_ID)
        {
            Debug.Assert(false, "[u8]空白マスが近くにありません\n");
            ResetSelect(oneCellSize, topLeft);
            return;
        }

        // マウス位置と「空白マスの中心座標」との距離を測る
        Vector2 emptyCellPos = GetWorldPosFromIndex(emptyIndex, oneCellSize, topLeft);  // 空白マスのワールド座標
        Vector2 mousePos = PuzzleSolver.GetMouseWorldPos(); // マウスのワールド座標
        // 距離が「セルの半分」より近ければドロップ成功とみなす
        float distance = Vector2.Distance(mousePos, emptyCellPos);
        float threshold = oneCellSize.x * 0.5f; // 半分の距離まで近づけば吸着
        if (distance > threshold)
        {
            // 空白マスで離されていない
            ResetSelect(oneCellSize, topLeft);
            return;
        }

        // 選択されていたピースと離された場所のピースを入れ替え
        SwapPiece(_selectArrayIndex, emptyIndex);

        _selectArrayIndex = Defs.INVALID_ID; // 選択解除
        if (IsClear())
        {
            // TODO：次のステップへ進む処理をここに書く
            Debug.Log("Clear!");
        }
        return;
    }

    private void ResetSelect(Vector2 oneCellSize, Vector2 topLeft)
    {
        SetCellPos(oneCellSize, topLeft, _selectArrayIndex);    // 元の位置に戻す
        _selectArrayIndex = Defs.INVALID_ID;
    }

    void SwapPiece(int index1, int index2)
    {
        (_pieces[index1], _pieces[index2]) = (_pieces[index2], _pieces[index1]);
        // 位置もセット
        Vector2 oneCellSize = GetOneCellSize();
        // 盤面が画面中央になるように、左上のセルの位置を決定
        Vector2 topLeft = GetTopLeftPos(oneCellSize);
        SetCellPos(oneCellSize, topLeft, index1);
        SetCellPos(oneCellSize, topLeft, index2);
    }

    // 動かすぴーすが決定されたときの処理
    void DecidePiece()
    {
        piece.Pos hitPos = GetHitPiecePos();
        if (!hitPos.IsValid())
        {
            // 何も選択されていなかったら終了
            return;
        }
        int arrayIndex = GetArrayIndexFromPos(hitPos.row, hitPos.col);
        if(arrayIndex == Defs.INVALID_ID || _pieces[arrayIndex].GetId() == Defs.EMPTY_ID)
        {
            // 無効なインデックス、または空白のピースが選択された場合は終了
            return;
        }
        
        // 動けるマスがあるか探す
        List<int> surroundAreaList = GetSurroundAreaList(arrayIndex);
        foreach (var piece in surroundAreaList)
        {
            if (_pieces[piece].GetId() == Defs.EMPTY_ID)
            {
                // 動けるマスが見つかった場合、選択されたピースの配列中のINDEXを保存して終了
                _selectArrayIndex = arrayIndex;
                _pieces[_selectArrayIndex].StartDragging(); // ドラッグされたことを伝える
                return;
            }
        }
        // 動けるマスがなかった場合は終了
        return;
    }

    // インデックスからワールド座標を算出するヘルパー関数
    // (SetCellPosのロジックを計算用に切り出し)
    Vector2 GetWorldPosFromIndex(int index, Vector2 oneCellSize, Vector2 topLeft)
    {
        int col = index % _width;
        int row = index / _width;
        return new Vector2(topLeft.x + col * oneCellSize.x, topLeft.y - row * oneCellSize.y);
    }

    private List<int> GetSurroundAreaList(int arrayIndex)
    {
        piece.Pos selectPos = GetPosFromArrayIndex(arrayIndex);
        List<int> surroundAreaList = new List<int>();   // 四方のマス
        if (selectPos.col != 0)
        {
            // 一列目ではない→左にマスがある
            surroundAreaList.Add(arrayIndex - 1);
        }
        if (selectPos.col != _width - 1)
        {
            // 最後の列ではない→右にマスがある
            surroundAreaList.Add(arrayIndex + 1);
        }
        if (selectPos.row != 0)
        {
            // 一行目ではない→上にマスがある
            surroundAreaList.Add(arrayIndex - _width);
        }
        if (selectPos.row != _height - 1)
        {
            // 最後の行ではない→下にマスがある
            surroundAreaList.Add(arrayIndex + _width);
        }

        return surroundAreaList;
    }

    // ピース配列のインデックスを取得
    piece.Pos GetHitPiecePos()
    {
        piece.Pos pos = new piece.Pos(Defs.INVALID_ID, Defs.INVALID_ID);

        // 画面上のマウスの位置を、ゲーム世界のワールド座標に変換
        Vector2 mousePos = PuzzleSolver.GetMouseWorldPos();
        // その位置にぴーむを飛ばしてコライダーにあたるか調べる
        // Physics2D.Raycast の引数は、通常 (発射地点, 発射方向)だが、Vector2.zero を指定すると、発射地点にいるオブジェクトを調べることができる
        // RaycastHit2Dは衝突結果を格納した構造体
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider == null)
        {
            return pos; // 何も当たらなかった場合は終了
        }

        // ヒットしたコライダーに所属しているゲームおぶじじぇくとのPuzzlePieceコンポーネントを取得
        PuzzlePiece piece = hit.collider.GetComponent<PuzzlePiece>();
        // 配列中から選択されたピースの位置を特定
        int arrayIndex = 0;
        foreach (var elem in _pieces)
        {
            if (_pieces[arrayIndex] == piece)
            {
                // 選択されたピースを見つけた
                return GetPosFromArrayIndex(arrayIndex);
            }
            ++arrayIndex;
        }
        return pos;
    }

    // パズルが完成しているかチェック
    private bool IsClear()
    {
        int right_index = 0;    // 正しいindex
        int totalCellNum = GetTotalCellNum();
        for (int cellIndex = 0; cellIndex < totalCellNum; ++cellIndex)
        {
            if (_pieces[cellIndex].GetId() != right_index)
            {
                return false;
            }
            ++right_index;
        }
            return true;
    }
}