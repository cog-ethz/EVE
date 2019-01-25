using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson {
    public class JRD_FirstPersonController : MonoBehaviour
    {

        [SerializeField]
        public MouseLook m_MouseLook;

        private Camera m_Camera;

        // Use this for initialization
        private void Start()
        {
            m_Camera = Camera.main;
            m_MouseLook.Init(transform, m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
        }


        private void FixedUpdate()
        {
            m_MouseLook.UpdateCursorLock();
        }

        public void ResetRotation()
        {
            m_MouseLook.Init(transform, m_Camera.transform);
        }

        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


    } 
}
