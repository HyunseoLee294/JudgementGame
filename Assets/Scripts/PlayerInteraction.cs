using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    public RecorderUI recorderUI;

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool lookingAtInteractable = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                lookingAtInteractable = true;
                interactionPrompt.SetActive(true);
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
        else
        {
            interactionPrompt.SetActive(false);
        }

        // E키 처리는 Raycast 바깥에서
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (recorderUI.recorderPanel.activeSelf)
            {
                recorderUI.Close();
            }
            else if (lookingAtInteractable)
            {
                recorderUI.Open();
            }
        }
    }
}
