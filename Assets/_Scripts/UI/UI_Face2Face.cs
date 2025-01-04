using CLAPlus.Face2Face;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLAPlus.Face2Face
{
    public class UI_Face2Face : MonoBehaviour
    {
        [SerializeField] Face2Face faceTracking;
        [SerializeField] CaptureDeviceControl capture;

        [SerializeField] TMP_Dropdown dropdown;
        [SerializeField] Toggle toggle;

        void OnEnable()
        {
            LoadDropdown();
            if (capture.sourse == -1)
            {
                toggle.interactable = false;
            }
            else
            {
                toggle.interactable = true;
            }
        }

        void LoadDropdown()
        {
            dropdown.ClearOptions();
            var devices = WebCamTexture.devices;
            foreach (var device in devices)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(device.name));
            }
        }

        public void OnDropdownValueChanged(int value)
        {
            capture.sourse = value;
            toggle.interactable = true;
        }

        public void OnTogleFaceCapture(bool value)
        {
            if (value)
            {
                faceTracking.StartTracking();
            }
            else
            {
                faceTracking.StopTracking();
            }
        }

        public void UseFaceSync(bool value)
        {
            CLAPlus.Face2Face.FaceSync.UseFaceSync = value;
        }
    }
}