using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class LoadingScreenText : MonoBehaviour
{
    public float timePassed = 0f;
    public int numberOfPeriods = 3;
    public int maxNumberOfPeriods = 3;
    public float lengthOfWait = 0.5f;
    public TextMeshProUGUI text;

    void Start()
    {
        text.text = string.Concat(text.text, new string(".".ToCharArray()[0], numberOfPeriods));
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= lengthOfWait)
        {
            numberOfPeriods %= maxNumberOfPeriods;
            numberOfPeriods++;
            text.text = string.Concat(text.text.TrimEnd(".".ToCharArray()[0]), new string(".".ToCharArray()[0], numberOfPeriods));
            timePassed = 0f;
        }
    }
}
