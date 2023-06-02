using System.Collections;
using UnityEngine;

public class Candy : MonoBehaviour
{

    [Header("Board Variables")]
    public int column; // �ekerin s�tun konumu
    public int row; // �ekerin sat�r konumu
    public int previousColumn; // �ekerin �nceki s�tun konumu
    public int previousRow; // �ekerin �nceki sat�r konumu
    public int targetX; // �ekerin hedef X konumu
    public int targetY; // �ekerin hedef Y konumu
    public bool isMatched = false; // �ekerin e�le�ip e�le�medi�ini kontrol eden boolean

    // Oyun tahtas�n� temsil eden bir referans
    private Board board;
    // Hareket s�ras�nda etkilenecek di�er �eker
    private GameObject otherCandy;
    // Ge�ici pozisyonlar� depolamak i�in kullan�lan Vector2
    private Vector2 tempPos;
    // Dokunma/Klik olaylar�n� takip etmek i�in kullan�lan Vector2
    private Vector2 firstTouchPos;
    private Vector2 finalTouchPos;
    // Kayd�rma a��s�n� belirleyen float
    public float swipeAngle = 0f;
    // Kayd�rman�n direncini belirleyen float
    public float swipeResist = 1f;

    // Bu, oyun ba�lad���nda �a�r�l�r.
    void Start()
    {
        // Oyun tahtas�n� bul
        board = FindObjectOfType<Board>();

        // �ekerin hedef X ve Y konumlar�n�, �u anki transform konumuna e�itle
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;

        // �ekerin sat�r ve s�tununu, hedef X ve Y konumlar�na e�itle
        //row = targetY;
        //column = targetX;

        // �ekerin �nceki sat�r ve s�tununu, �u anki sat�r ve s�tuna e�itle
        //previousRow = row;
        //previousColumn = column;
    }

    // Bu, her frame'de �a�r�l�r.
    void Update()
    {
        // E�le�meleri kontrol et
        FindMatches();

        // E�er �eker e�le�mi�se, �ekerin rengini de�i�tir
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, 2f);
        }

        // �ekerin hedef X ve Y konumlar�n�, �u anki s�tun ve sat�r konumlar�na e�itle
        targetX = column;
        targetY = row;

        // �ekerin hedef X konumu ile mevcut X konumu aras�ndaki fark� kontrol et ve konumu hedefe do�ru lerp et
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, .4f);
            if (board.allCandies[column, row] != this.gameObject)
            {
                board.allCandies[column, row] = this.gameObject;
            }
        }
        else
        {
            tempPos = new Vector2(targetX, transform.position.y);
            transform.position = tempPos;
        }
        // �ekerin hedef Y konumu ile mevcut Y konumu aras�ndaki fark� kontrol et ve konumu hedefe do�ru lerp et
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPos, .4f);
            if (board.allCandies[column, row] != this.gameObject)
            {
                board.allCandies[column, row] = this.gameObject;
            }
        }
        else
        {
            tempPos = new Vector2(transform.position.x, targetY);
            transform.position = tempPos;
        }
    }

    // Bu, fare ile t�klama ba�lad���nda �a�r�l�r.
    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    // Bu, fare ile t�klama bitti�inde �a�r�l�r.
    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

    }

    // Bu, t�klama olaylar� aras�ndaki a��y� hesaplar.
    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPos.y - firstTouchPos.y) > swipeResist || Mathf.Abs(finalTouchPos.x - firstTouchPos.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) * 180 / Mathf.PI;
            MovePiece();
            board.currentState = GameState.wait;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    // Bu, kayd�rma a��s�na g�re �ekeri hareket ettirir.
    void MovePiece()
    {
        // Kayd�rma a��s�na g�re �ekerlerin konumlar�n� g�ncelle
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            otherCandy = board.allCandies[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().column -= 1;
            column += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            otherCandy = board.allCandies[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            otherCandy = board.allCandies[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().column += 1;
            column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            otherCandy = board.allCandies[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherCandy.GetComponent<Candy>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    // Bu, oyun tahtas�nda e�le�meleri bulmaya �al���r.
    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftCandy1 = board.allCandies[column - 1, row];
            GameObject rightCandy1 = board.allCandies[column + 1, row];
            if (leftCandy1 != null && rightCandy1 != null)
            {
                if (leftCandy1.tag == this.gameObject.tag && rightCandy1.tag == this.gameObject.tag)
                {
                    leftCandy1.GetComponent<Candy>().isMatched = true;
                    rightCandy1.GetComponent<Candy>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upCandy1 = board.allCandies[column, row + 1];
            GameObject downCandy1 = board.allCandies[column, row - 1];
            if (upCandy1 != null && downCandy1 != null)
            {
                if (upCandy1.tag == this.gameObject.tag && downCandy1.tag == this.gameObject.tag)
                {
                    upCandy1.GetComponent<Candy>().isMatched = true;
                    downCandy1.GetComponent<Candy>().isMatched = true;
                    isMatched = true;
                }
            }

        }
    }

    // Bu, bir hareketin ge�erli olup olmad���n� kontrol eder.
    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);
        if (otherCandy != null)
        {
            if (!isMatched && !otherCandy.GetComponent<Candy>().isMatched)
            {
                otherCandy.GetComponent<Candy>().row = row;
                otherCandy.GetComponent<Candy>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            otherCandy = null;
        }
    }
}
