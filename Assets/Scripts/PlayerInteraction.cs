using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                interactionPrompt.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("상호작용: " + hit.collider.name);
                }
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
    }
}
