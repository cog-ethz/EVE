using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//inspired by 3DBuzz, https://www.youtube.com/watch?v=QxRAIjXdfFU
//this class is a component given to menus to coordinate animation, interaction and state of menus
namespace Assets.EVE.Scripts.Menu
{
    public class BaseMenu : MonoBehaviour
    {

        public string menuName = "Menu";

        private Animator _animator;
        private CanvasGroup _canvasGroup;

        public bool isOpen {
            get { return _animator!=null && _animator.GetBool("IsOpen"); }
            set { if(_animator!=null)_animator.SetBool("IsOpen", value); }
        }

        public CanvasGroup getMenuCanvasGroup() {
            return _canvasGroup;
        }

        public void Awake() {
            _animator = GetComponent<Animator>();
            isOpen = false;
            _canvasGroup=GetComponent<CanvasGroup>();

            //no matter where windows are designed, they get moved to the middle of the screen
            var rect = GetComponent<RectTransform>();
            rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
        }

        public void Update() {
            //if animator is in the state open, the menu is interactable, toherwise it is not. prevents manipulating menus by sideffects or pressing tab  
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            {
                _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
            }
            else {
                _canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
            }
        }
        
    }
}
