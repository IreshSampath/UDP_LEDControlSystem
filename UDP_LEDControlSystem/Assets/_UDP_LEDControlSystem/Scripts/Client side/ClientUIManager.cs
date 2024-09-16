using System.Collections.Generic;
using UnityEngine;

namespace GAG.UDPLEDControlSystem
{
    public class ClientUIManager : UIManager
    {
        [SerializeField] GameObject _lEDLight;
        [SerializeField] GameObject _lEDLightsParent;

        public List<GameObject> LEDLights;
        readonly int _lEDLightsCount = 100;

        // Start is called before the first frame update
        void Start()
        {
            LEDLights = new List<GameObject>();

            for (int i = 0; i < _lEDLightsCount; i++)
            {
                GameObject initLED = Instantiate(_lEDLight, _lEDLightsParent.transform);
                initLED.name = i.ToString();
                initLED.SetActive(true);
                LEDLights.Add(initLED);
            }
        }
    }
}
