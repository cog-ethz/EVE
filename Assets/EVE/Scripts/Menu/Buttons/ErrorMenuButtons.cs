using UnityEngine;

namespace Assets.EVE.Scripts.Menu.Buttons
{
    public class ErrorMenuButtons : MonoBehaviour{

        private BaseMenu _originBaseMenu;
        private LaunchManager _launchManager;

        void Start()
        {
            _launchManager = GameObject.FindGameObjectWithTag("LaunchManager").GetComponent<LaunchManager>();
        }

        public void SetErrorText(string errorText)
        {
            var errorMessageObj=GameObject.Find("ErrorMessage");
            errorMessageObj.GetComponent<UnityEngine.UI.Text>().text = errorText;
        }

        public void SetErrorOriginMenu(BaseMenu originBaseMenu)
        {
            this._originBaseMenu = originBaseMenu;
        }

        public void ShowOriginMenu() {
            _launchManager.MenuManager.ShowMenu(_originBaseMenu);
        }
    }
}
