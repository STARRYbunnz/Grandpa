using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SceneChangeOnCollision : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField, HideInInspector] private string sceneToLoad = "G_setting";

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI รูปภาพ")]
    [SerializeField] private Texture2D eKeyIcon;     
    [SerializeField] private float iconSize = 128f;   
    [SerializeField] private float bottomOffset = 100f;

    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInside = false;
    }

    private void Update()
    {
        if (playerInside && IsEPressed())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private bool IsEPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    
    private void OnGUI()
    {
        if (!playerInside || eKeyIcon == null) return;

        Rect rect = new Rect(
            (Screen.width - iconSize) / 2f,
            Screen.height - iconSize - bottomOffset,
            iconSize,
            iconSize
        );

       
        GUI.DrawTexture(rect, eKeyIcon, ScaleMode.ScaleToFit, true);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneToLoad = sceneAsset.name;
        }
    }
#endif
}