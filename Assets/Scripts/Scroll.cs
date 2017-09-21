using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour {
    public int type = 0;
    public Sprite[] numbers;
    public int num;
    public int minNum;
    public int maxNum;

    void Start() {
        UpdateNumber(Random.Range(minNum, maxNum+1));
    }

    public void UpdateNumber(int input)
    {
        if (numbers.Length < num) return;
        num = input;
        transform.Find("NumberImage").GetComponent<SpriteRenderer>().sprite = numbers[num];
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
