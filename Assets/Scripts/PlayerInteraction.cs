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
        if (JudgeManager.Instance != null && JudgeManager.Instance.IsGameplayBlocked())
        {
            interactionPrompt.SetActive(false);
            return;
        }

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
            if (currentInteractable != null)
            {
                // 녹음기 시작 전에는 녹음기만 상호작용 가능
                if (!recorder.HasStarted())
                {
                    if (!(currentInteractable is Recorder))
                    {
                        currentInteractable = null;
                    }
                }
                // 새로 해금된 섹션을 아직 한 번도 다 듣지 않았으면 녹음기 외 상호작용 불가
                else if (JudgeManager.Instance != null
                         && JudgeManager.Instance.HasUnheardUnlocks()
                         && !(currentInteractable is Recorder))
                {
                    currentInteractable = null;
                }
                // Clue인 경우 선행 조건 체크
                else if (currentInteractable is Clue)
                {
                    Clue clue = (Clue)currentInteractable;
                    if (!clue.IsAvailable())
                    {
                        currentInteractable = null;
                    }
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
