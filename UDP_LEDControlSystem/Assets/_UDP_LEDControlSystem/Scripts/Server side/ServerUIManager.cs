using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GAG.UDPLEDControlSystem
{
    public class ServerUIManager : UIManager
    {
        public List<GameObject> SelectedLEDButtons;

        [SerializeField] GameObject _lEDButton;
        [SerializeField] GameObject _lEDButtonsParent;

        [SerializeField] Slider _red;
        [SerializeField] Slider _green;
        [SerializeField] Slider _blue;

        List<GameObject> _lEDButtons;
        List<GameObject> _tempSelectedLEDButtons;

        readonly int _lEDButtonsCount = 100;
        Color _selectedColor;

        // Call things before the first frame update
        void Start()
        {
            _lEDButtons = new List<GameObject>();
            SelectedLEDButtons = new List<GameObject>();
            _tempSelectedLEDButtons = new List<GameObject> ();
            _selectedColor = Color.white;

            for (int i = 0; i < _lEDButtonsCount; i++)
            {
                GameObject initLED = Instantiate(_lEDButton, _lEDButtonsParent.transform);
                initLED.name = i.ToString();
                initLED.SetActive(true);
                _lEDButtons.Add(initLED);
            }
        }

        // Select LED buttons individually
        public void SelectLEDButton(GameObject selectedLEDButton)
        {
            foreach (GameObject tmpLEDButton in _tempSelectedLEDButtons)
            {
                if (selectedLEDButton.name == tmpLEDButton.name)
                {
                    _tempSelectedLEDButtons.Remove(tmpLEDButton);
                    break;
                }
            }
            selectedLEDButton.transform.GetChild(0).GetComponent<Button>().enabled = false;
            selectedLEDButton.GetComponent<Image>().enabled = true;
            _tempSelectedLEDButtons.Add(selectedLEDButton);
        }

        // Select all LED buttons
        public void SelectAllButtons()
        {
            _tempSelectedLEDButtons.Clear();

            foreach (GameObject lEDButton in _lEDButtons)
            {
                lEDButton.transform.GetChild(0).GetComponent<Button>().enabled = false;
                lEDButton.GetComponent<Image>().enabled = true;
                _tempSelectedLEDButtons.Add(lEDButton);
            }
        }

        // Select a color using sliders
        public void SelectColor()
        {
            _selectedColor.r = _red.value;
            _selectedColor.g = _green.value;
            _selectedColor.b = _blue.value;

            AddColor();
        }

        // Add the color to the selected LED buttons
        void AddColor()
        {
            foreach (GameObject lEDButton in _tempSelectedLEDButtons)
            {
                lEDButton.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = _selectedColor;
            }
        }

        // Clear all references and reset all
        public void Clear()
        {
            _red.value = 1;
            _green.value = 1;
            _blue.value = 1;

            foreach (GameObject lEDButton in _tempSelectedLEDButtons)
            {
                lEDButton.GetComponent<Image>().enabled = false;
                lEDButton.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = Color.white;
                lEDButton.transform.GetChild(0).GetComponent<Button>().enabled = true;
            }
            _tempSelectedLEDButtons.Clear();

            foreach (GameObject lEDButton in SelectedLEDButtons)
            {
                lEDButton.GetComponent<Image>().enabled = false;
                lEDButton.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = Color.white;
                lEDButton.transform.GetChild(0).GetComponent<Button>().enabled = true;
            }
            SelectedLEDButtons.Clear();
        }

        // Save the current color pattern and let the new pattern
        public void ApplyColor()
        {
            foreach (GameObject lEDButton in _tempSelectedLEDButtons)
            {
                lEDButton.GetComponent<Image>().enabled = false;
                lEDButton.GetComponent<Transform>().GetChild(0).GetComponent<Image>().color = _selectedColor;
                lEDButton.transform.GetChild(0).GetComponent<Button>().enabled = true;

                foreach (GameObject tmpLEDButton in SelectedLEDButtons)
                {
                    if(lEDButton.name == tmpLEDButton.name)
                    {
                        SelectedLEDButtons.Remove(tmpLEDButton);
                        break;
                    }
                }
                SelectedLEDButtons.Add(lEDButton);
            }
            _tempSelectedLEDButtons.Clear();
        }
    }
}