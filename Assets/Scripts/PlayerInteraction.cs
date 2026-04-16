using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    public RecorderUI recorderUI;

    // Update is called once per frame
    void Update()
    {
        if (recorderUI.recorderPanel.activeSelf)
        {
        // UI 열려있으면 상호작용 체크 안 함
        interactionPrompt.SetActive(false);
        return;
        }
        
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        IInteractable currentInteractable = null;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // IInteractable 컴포넌트가 있는지 직접 확인
            currentInteractable = hit.collider.GetComponent<IInteractable>();
        }

        if (currentInteractable != null)
        {
            interactionPrompt.SetActive(true);
        }
        else
        {
            interactionPrompt.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (recorderUI.recorderPanel.activeSelf)
            {
                recorderUI.Close();
            }
            else if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }
}
