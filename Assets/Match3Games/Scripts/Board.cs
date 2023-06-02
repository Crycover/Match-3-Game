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
    // Oyun tahtas�n�n geni�li�i ve y�ksekli�i
    public int width;
    public int height;

    public int offSet;

    [Header("Prefab")]
    // Arka plan d��eme modelinin referans�
    public GameObject tilePrefab;

    [Header("Candy")]
    // �ekerlerin referanslar�
    public GameObject[] candies;
    // Tahtada her �ekerin yerini tutan matris
    public GameObject[,] allCandies;

    void Start()
    {
        // �eker matrisini tahtan�n geni�li�i ve y�ksekli�i boyutunda olu�turur
        allCandies = new GameObject[width, height];
        // Tahtay� ilk olu�turur
        TileSetUp();
    }

    // Tahtay� olu�turan ve her d��emede rastgele bir �eker koyan fonksiyon
    private void TileSetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Yeni d��emenin konumunu belirler
                Vector2 pos = new Vector2(i, j + offSet);
                // Yeni bir d��eme olu�turur ve konumuna yerle�tirir
                GameObject backGroundTile = Instantiate(tilePrefab, pos, Quaternion.identity) as GameObject;
                // Olu�turulan d��emeyi bu Board objesinin alt�na yerle�tirir
                backGroundTile.transform.parent = this.transform;
                // Olu�turulan d��emeye isim verir
                backGroundTile.name = "(" + i + ", " + j + ")";
                // Hangi �ekerin kullan�laca��n� rastgele belirler
                int candyToUse = Random.Range(0, candies.Length);
                // Ba�lang��ta ��l� e�le�me durumunu �nlemek i�in kontrol eder
                int maxIterations = 0;
                while (MatchesAt(i, j, candies[candyToUse]) && maxIterations < 100)
                {
                    candyToUse = Random.Range(0, candies.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                // �ekeri olu�turur ve konumuna yerle�tirir
                GameObject candy = Instantiate(candies[candyToUse], pos, Quaternion.identity) as GameObject;

                candy.GetComponent<Candy>().row = j;
                candy.GetComponent<Candy>().column = i;

                // Olu�turulan �ekeri bu Board objesinin alt�na yerle�tirir
                candy.transform.parent = this.transform;
                // Olu�turulan �eker objesine isim verir
                candy.name = "(" + i + ", " + j + ")" + " " + "Candy";
                // �ekeri matristeki ilgili konuma atar
                allCandies[i, j] = candy;
            }
        }
    }

    // Belirli bir konumdaki �ekerin yatay veya dikey olarak ��l� e�le�me olu�turup olu�turmad���n� kontrol eder
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // D��eme tahtan�n en az iki d��eme i�eride oldu�unda kontrol eder
        if (column > 1 && row > 1)
        {
            // Ayn� s�tundaki �stteki iki d��emenin ayn� t�rde �eker i�erip i�ermedi�ini kontrol eder
            if (allCandies[column - 1, row].tag == piece.tag && allCandies[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            // Ayn� sat�rdaki solundaki iki d��emenin ayn� t�rde �eker i�erip i�ermedi�ini kontrol eder
            if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        // D��eme tahtan�n kenar�nda oldu�unda kontrol eder
        else if (column <= 1 || row <= 1)
        {
            // Ayn� s�tundaki �stteki iki d��emenin ayn� t�rde �eker i�erip i�ermedi�ini kontrol eder
            if (row > 1)
            {
                if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            // Ayn� sat�rdaki solundaki iki d��emenin ayn� t�rde �eker i�erip i�ermedi�ini kontrol eder
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
        // �eker e�le�tiyse onu yok eder
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
        // �ekerler yok edildikten sonra d��emeleri a�a�� indirir
        StartCoroutine(DecreaseRowCo());
    }

    // E�le�en ve yok edilen �ekerlerin yerine yenilerini olu�turur
    private IEnumerator DecreaseRowCo()
    {
        // �ekerin bulunmad��� d��eme say�s�n� tutar
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // �eker yoksa nullCount artt�r�l�r
                if (allCandies[i, j] == null)
                {
                    nullCount++;
                }
                // �eker varsa ve �st�nde bo� d��eme varsa, �ekeri a�a��ya do�ru indirir
                else if (nullCount > 0)
                {
                    allCandies[i, j].GetComponent<Candy>().row -= nullCount;
                    allCandies[i, j] = null;
                }
            }
            // nullCount'� s�f�rlar
            nullCount = 0;
        }
        // �ndirme i�lemi tamamland�ktan sonra tahtay� doldurur
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
                // E�er d��eme bo�sa yeni bir �eker olu�turur ve yerle�tirir
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
                    // E�le�en bir �eker varsa true d�ner
                    if (allCandies[i, j].GetComponent<Candy>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        // E�le�en �eker yoksa false d�ner
        return false;
    }

    // Tahtay� doldurur ve e�le�en t�m �ekerleri yok eder
    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        // Tahtada e�le�me oldu�u s�rece �ekerleri yok eder
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
