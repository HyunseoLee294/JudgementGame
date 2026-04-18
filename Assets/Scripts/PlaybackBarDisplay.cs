using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaybackBarDisplay : MonoBehaviour
{
    public RectTransform hatchContainer;
    public AudioSource recorderAudio;

    private List<GameObject> hatchImages = new List<GameObject>();
    private Sprite hatchSprite;

    void Start()
    {
        // 빗금 텍스처 생성 → 스프라이트로 변환
        Texture2D tex = HatchTextureGenerator.CreateHatchTexture();
        hatchSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    public void RefreshHatchMarks()
    {
        // 기존 빗금 삭제
        foreach (var obj in hatchImages)
        {
            Destroy(obj);
        }
        hatchImages.Clear();

        if (recorderAudio.clip == null) return;

        float totalLength = recorderAudio.clip.length;
        float containerWidth = hatchContainer.rect.width;

        foreach (var section in GameManager.Instance.subtitleData.sections)
        {
            // 해금된 구간은 건너뛰기
            if (GameManager.Instance.IsSectionUnlocked(section.sectionId)) continue;

            // 구간의 시작/끝 위치를 슬라이더 비율로 계산
            float startRatio = section.startTime / totalLength;
            float endTime = section.endTime < 0 ? totalLength : section.endTime;
            float endRatio = endTime / totalLength;

            // 빗금 이미지 생성
            GameObject hatchObj = new GameObject("Hatch_" + section.sectionId);
            hatchObj.transform.SetParent(hatchContainer, false);

            // RectTransform 설정
            RectTransform rt = hatchObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(startRatio, 0);
            rt.anchorMax = new Vector2(endRatio, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Image 설정
            Image img = hatchObj.AddComponent<Image>();
            img.sprite = hatchSprite;
            img.type = Image.Type.Tiled;
            img.raycastTarget = false;  // 슬라이더 드래그 방해 안 함

            hatchImages.Add(hatchObj);
        }
    }
}