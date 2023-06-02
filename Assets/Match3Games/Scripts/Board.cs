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
    // Oyun tahtasýnýn geniþliði ve yüksekliði
    public int width;
    public int height;

    public int offSet;

    [Header("Prefab")]
    // Arka plan döþeme modelinin referansý
    public GameObject tilePrefab;

    [Header("Candy")]
    // Þekerlerin referanslarý
    public GameObject[] candies;
    // Tahtada her þekerin yerini tutan matris
    public GameObject[,] allCandies;

    void Start()
    {
        // Þeker matrisini tahtanýn geniþliði ve yüksekliði boyutunda oluþturur
        allCandies = new GameObject[width, height];
        // Tahtayý ilk oluþturur
        TileSetUp();
    }

    // Tahtayý oluþturan ve her döþemede rastgele bir þeker koyan fonksiyon
    private void TileSetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Yeni döþemenin konumunu belirler
                Vector2 pos = new Vector2(i, j + offSet);
                // Yeni bir döþeme oluþturur ve konumuna yerleþtirir
                GameObject backGroundTile = Instantiate(tilePrefab, pos, Quaternion.identity) as GameObject;
                // Oluþturulan döþemeyi bu Board objesinin altýna yerleþtirir
                backGroundTile.transform.parent = this.transform;
                // Oluþturulan döþemeye isim verir
                backGroundTile.name = "(" + i + ", " + j + ")";
                // Hangi þekerin kullanýlacaðýný rastgele belirler
                int candyToUse = Random.Range(0, candies.Length);
                // Baþlangýçta Üçlü eþleþme durumunu önlemek için kontrol eder
                int maxIterations = 0;
                while (MatchesAt(i, j, candies[candyToUse]) && maxIterations < 100)
                {
                    candyToUse = Random.Range(0, candies.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                // Þekeri oluþturur ve konumuna yerleþtirir
                GameObject candy = Instantiate(candies[candyToUse], pos, Quaternion.identity) as GameObject;

                candy.GetComponent<Candy>().row = j;
                candy.GetComponent<Candy>().column = i;

                // Oluþturulan þekeri bu Board objesinin altýna yerleþtirir
                candy.transform.parent = this.transform;
                // Oluþturulan þeker objesine isim verir
                candy.name = "(" + i + ", " + j + ")" + " " + "Candy";
                // Þekeri matristeki ilgili konuma atar
                allCandies[i, j] = candy;
            }
        }
    }

    // Belirli bir konumdaki þekerin yatay veya dikey olarak üçlü eþleþme oluþturup oluþturmadýðýný kontrol eder
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // Döþeme tahtanýn en az iki döþeme içeride olduðunda kontrol eder
        if (column > 1 && row > 1)
        {
            // Ayný sütundaki üstteki iki döþemenin ayný türde þeker içerip içermediðini kontrol eder
            if (allCandies[column - 1, row].tag == piece.tag && allCandies[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            // Ayný satýrdaki solundaki iki döþemenin ayný türde þeker içerip içermediðini kontrol eder
            if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        // Döþeme tahtanýn kenarýnda olduðunda kontrol eder
        else if (column <= 1 || row <= 1)
        {
            // Ayný sütundaki üstteki iki döþemenin ayný türde þeker içerip içermediðini kontrol eder
            if (row > 1)
            {
                if (allCandies[column, row - 1].tag == piece.tag && allCandies[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            // Ayný satýrdaki solundaki iki döþemenin ayný türde þeker içerip içermediðini kontrol eder
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

    // Belirli bir konumdaki eþleþen þekeri yok eder
    private void DestroyMatchesAt(int column, int row)
    {
        // Þeker eþleþtiyse onu yok eder
        if (allCandies[column, row].GetComponent<Candy>().isMatched)
        {
            Destroy(allCandies[column, row]);
            allCandies[column, row] = null;
        }
    }

    // Tahtada eþleþen tüm þekerleri yok eder
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
        // Þekerler yok edildikten sonra döþemeleri aþaðý indirir
        StartCoroutine(DecreaseRowCo());
    }

    // Eþleþen ve yok edilen þekerlerin yerine yenilerini oluþturur
    private IEnumerator DecreaseRowCo()
    {
        // Þekerin bulunmadýðý döþeme sayýsýný tutar
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Þeker yoksa nullCount arttýrýlýr
                if (allCandies[i, j] == null)
                {
                    nullCount++;
                }
                // Þeker varsa ve üstünde boþ döþeme varsa, þekeri aþaðýya doðru indirir
                else if (nullCount > 0)
                {
                    allCandies[i, j].GetComponent<Candy>().row -= nullCount;
                    allCandies[i, j] = null;
                }
            }
            // nullCount'ý sýfýrlar
            nullCount = 0;
        }
        // Ýndirme iþlemi tamamlandýktan sonra tahtayý doldurur
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    // Tahtadaki boþ döþemelere yeni þekerler koyar
    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Eðer döþeme boþsa yeni bir þeker oluþturur ve yerleþtirir
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

    // Tahtada eþleþen þeker olup olmadýðýný kontrol eder
    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCandies[i, j] != null)
                {
                    // Eþleþen bir þeker varsa true döner
                    if (allCandies[i, j].GetComponent<Candy>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        // Eþleþen þeker yoksa false döner
        return false;
    }

    // Tahtayý doldurur ve eþleþen tüm þekerleri yok eder
    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        // Tahtada eþleþme olduðu sürece þekerleri yok eder
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }
}
