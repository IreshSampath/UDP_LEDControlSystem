using System;
using TMPro;
using UnityEngine;

namespace GAG.UDPLEDControlSystem
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] TMP_Text _consolTxt;

        public void PrintConsole(string msg)
        {
            DateTime now = DateTime.Now;
            string currentTime = now.ToString("hh:mm:ss");
            string prevMsg = _consolTxt.text;
            _consolTxt.text = prevMsg + "\n" + currentTime + " : " + msg;
        }
    }
}
