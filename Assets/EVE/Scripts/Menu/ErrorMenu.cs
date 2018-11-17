using System.Collections;
using System.Collections.Generic;
using Assets.EVE.Scripts.Menu;
using UnityEngine;

public class ErrorMenu : BaseMenu {

    BaseMenu _originBaseMenu = null;

    public void setErrorText(string errorText)
    {
        GameObject errorMessageObj=GameObject.Find("ErrorMessage");
        errorMessageObj.GetComponent<UnityEngine.UI.Text>().text = errorText;
    }

    public void setErrorOriginMenu(BaseMenu originBaseMenu)
    {
        this._originBaseMenu = originBaseMenu;
    }

    public void showOriginMenu() {
        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(_originBaseMenu);
    }


}
