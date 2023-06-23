using System.Collections;
using UnityEngine;

public class Candy : MonoBehaviour
{

    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn; 
    public int previousRow; 
    public int targetX; 
    public int targetY; 
    public bool isMatched = false; 

    private Board board;
    private GameObject otherCandy;
    private Vector2 tempPos;
    private Vector2 firstTouchPos;
    private Vector2 finalTouchPos;
    public float swipeAngle = 0f;
    public float swipeResist = 1f;

    // Bu, oyun ba�lad���nda �a�r�l�r.
    void Start()
    {
        // Oyun tahtas�n� bul
        board = FindObjectOfType<Board>();

        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;

        //row = targetY;
        //column = targetX;

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
