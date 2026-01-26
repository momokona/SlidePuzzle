using System.Collections.Generic; //Listを使うために必要
using SlidePuzzle.Common;
using UnityEngine;


// TODO:C++で前に書いたものを移植しただけなので、計算方法あっているか確認する
public class PuzzleSolver
{
    // 転倒数を計算する
    private int GetInvNum(List<int> pieces)
    {
        int totalNum = pieces.Count;
        int invNum = 0;

        // 右のほうが小さい場合に転倒数をカウントする(EMPTY_IDは空白を意味するので抜かす)
        for (int index = 0; index < totalNum; ++index)
        {
            if (pieces[index] == Defs.EMPTY_ID)
            {
                continue;
            }

            for (int rightIndex = index + 1; rightIndex < totalNum; ++rightIndex)
            {
                if (pieces[rightIndex] == Defs.EMPTY_ID)
                {
                    continue;
                }

                if (pieces[index] > pieces[rightIndex])
                {
                    ++invNum;
                }
            }
        }
        return invNum;
    }

    // 空白の行番号を下から数えたものを返す(偶数×偶数のパズルで必要)
    private int GetEmptyRowFromBottomForEven(List<int> pieces)
    {
        // 仮で4×4のパズルとして実装
        // TODO:可変で対応できるようにする
        const int colNum = 4;
        const int rowNum = 4;

        // 空白のインデックスを取得
        int emptyIndex = pieces.IndexOf(Defs.EMPTY_ID);

        if (emptyIndex == Defs.INVALID_ID) // 見つからなかった場合
        {
            return Defs.INVALID_ID;
        }

        int lastRow = rowNum - 1;

        return lastRow - emptyIndex / colNum;
    }

    // パズルが解けるかどうかを判定する
    bool IsSolvablePuzzle(List<int> pieces)
    {
        int invNum = GetInvNum(pieces);

        if (pieces.Count % 2 != 0)
        {
            return (invNum % 2 == 0);  // 奇数×奇数のパズルであれば、転倒数が偶数なら解ける
        }

        // 転倒数＋空白の行番号(下から数えたもの)が偶数なら解ける
        return (invNum + GetEmptyRowFromBottomForEven(pieces)) % 2 == 0;
    }
}