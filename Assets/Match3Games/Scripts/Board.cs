using System.Collections;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;

    [Header("Tile")]
    public int width;
    public int height;

    public int offSet;

    [Header("Prefab")]
    public GameObject tilePrefab;

    [Header("Candy")]
    public GameObject[] candies;
    public GameObject[,] allCandies;


    void Start()
    {
        allCandies = new GameObject[width, height];
        TileSetUp();
    }

    private void TileSetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 pos = new Vector2(i, j + offSet);
                GameObject backGroundTile = Instantiate(tilePrefab, pos, Quaternion.identity) as GameObject;
                backGroundTile.transform.parent = this.transform;
                backGroundTile.name = "(" + i + ", " + j + ")";
                int candyToUse = Random.Range(0, candies.Length);
                int maxIterations = 0;
                while (MatchesAt(i, j, candies[candyToUse]) && maxIterations < 100)
                {
                    candyToUse = Random.Range(0, candies.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                GameObject candy = Instantiate(candies[candyToUse], pos, Quaternion.identity) as GameObject;

                candy.GetComponent<Candy>().row = j;
                candy.GetComponent<Candy>().column = i;

                candy.transform.parent = this.transform;
                candy.name = "(" + i + ", " + j + ")" + " " + "Candy";
                allCandies[i, j] = candy;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // D��eme tahtan�n en az iki d��eme i�eride oldu�unda kontrol eder
        if (column > 1 && row > 1)
        {
            if (allCandies[column - 1, row].tag == piece.tag && allCandies[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        // D��eme tahtan�n kenar�nda oldu�unda kontrol eder
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allCandies[column - 1, row].tag == piece.tag && allCandies[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Belirli bir konumdaki e�le�en �ekeri yok eder
    private void DestroyMatchesAt(int column, int row)
    {
        if (allCandies[column, row].GetComponent<Candy>().isMatched)
        {
            Destroy(allCandies[column, row]);
            allCandies[column, row] = null;
        }
    }

    // Tahtada e�le�en t�m �ekerleri yok eder
    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCandies[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    // E�le�en ve yok edilen �ekerlerin yerine yenilerini olu�turur
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCandies[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allCandies[i, j].GetComponent<Candy>().row -= nullCount;
                    allCandies[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    // Tahtadaki bo� d��emelere yeni �ekerler koyar
    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCandies[i, j] == null)
                {
                    Vector2 tempPos = new Vector2(i, j + offSet);
                    int candyToUse = Random.Range(0, candies.Length);
                    GameObject piece = Instantiate(candies[candyToUse], tempPos, Quaternion.identity);
                    allCandies[i, j] = piece;
                    piece.GetComponent<Candy>().row = j;
                    piece.GetComponent<Candy>().column = i;
                }
            }
        }
    }

    // Tahtada e�le�en �eker olup olmad���n� kontrol eder
    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCandies[i, j] != null)
                {
                    if (allCandies[i, j].GetComponent<Candy>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Tahtay� doldurur ve e�le�en t�m �ekerleri yok eder
    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
