using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMenu : Menu {

    Menu originMenu = null;

    public void setErrorText(string errorText)
    {
        GameObject errorMessageObj=GameObject.Find("ErrorMessage");
        errorMessageObj.GetComponent<UnityEngine.UI.Text>().text = errorText;
    }

    public void setErrorOriginMenu(Menu originMenu)
    {
        this.originMenu = originMenu;
    }

    public void showOriginMenu() {
        GameObject.Find("Canvas").GetComponent<MenuManager>().ShowMenu(originMenu);
    }


}
