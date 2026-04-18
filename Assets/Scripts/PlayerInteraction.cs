using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    public RecorderUI recorderUI;

    public Recorder recorder;

    // Update is called once per frame
    void Update()
    {
        if (recorderUI.recorderPanel.activeSelf)
        {
        // UI 열려있으면 상호작용 체크 안 함
        interactionPrompt.SetActive(false);
                // 패널 열려있을 때 E키로 닫기
        if (Input.GetKeyDown(KeyCode.E))
        {
            recorderUI.Close();
        }
        return;
        }
        
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        IInteractable currentInteractable = null;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // IInteractable 컴포넌트가 있는지 직접 확인
            currentInteractable = hit.collider.GetComponent<IInteractable>();
            // 녹음기 시작 전에는 녹음기만 상호작용 가능
            if (currentInteractable != null && !recorder.HasStarted())
            {
                // Recorder가 아니면 무시
                if (!(currentInteractable is Recorder))
                {
                    currentInteractable = null;
                }
            }
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
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }
}
