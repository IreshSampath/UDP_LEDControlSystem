using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text _consolTxt;

    public void PrintConsole(string msg)
    {
        System.DateTime currentTime = System.DateTime.Now;
        string prevMsg = _consolTxt.text;
        _consolTxt.text = prevMsg + "\n" + currentTime.TimeOfDay + " : " + msg;
    }
}
