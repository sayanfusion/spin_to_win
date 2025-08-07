using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelController : MonoBehaviour
{
    public WheelSegment segmentPrefab;
    public Transform parentTransform;

    public List<WheelSegment> allSegments;
    public float radius = 2f;
    public int segmentCount = 8;

    public int stopSegment = 0;
    public Tween currentTween;
    public Button spinBtn;
    public bool clockWise;
    public bool isSpinning = false;
    Tween spinTween;
    public float spinSpeed = 72f;
    public GameObject winPopUp;
    public TMP_Text balanceText;
    public int balance = 1000;

    public List<int> prizeValues = new List<int>() { 0, 200, 0, 50, 0, 100, 10, 0 };
    private void Start()
    {
        ArrangeSegments();

        spinBtn.onClick.AddListener(() =>
        {
            StartCoroutine(SpinRoutine());
        });
    }

    void ArrangeSegments()
    {
        int segmentCount = 8;
        float angleStep = 360f / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = -i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            WheelSegment segment = Instantiate(segmentPrefab, parentTransform);
            segment.segmentIndex = i;
            segment.baseImage.color = Color.HSVToRGB(i / (float)segmentCount, 1f, 1f);
            segment.transform.localPosition = Vector2.zero;
            segment.transform.localRotation = Quaternion.Euler(0, 0, angle);
            segment.text.text = prizeValues[i].ToString();
            segment.prize = prizeValues[i];
            allSegments[i]=segment;
        }
    }

    IEnumerator SpinRoutine()
    {

        if (balance <= 0) yield break;

        stopSegment = Random.Range(0, segmentCount);
        StartSpinning();
        balance -= 10;
        balanceText.text = balance.ToString();
        yield return new WaitForSeconds(2f);
        SpinToSegment();
        yield return new WaitUntil(() => !isSpinning);
        if (allSegments[stopSegment].prize <= 0)
        {
            winPopUp.SetActive(false);
            spinBtn.interactable = true;
            yield break;
        }
        OnWin();
        yield return new WaitForSeconds(3f);
        winPopUp.SetActive(false);
        parentTransform.DOKill();
        spinBtn.interactable = true;
    }

    public void StartSpinning()
    {
        if (isSpinning) return;

        isSpinning = true;

        float angle = clockWise ? -360f : 360f;
        spinBtn.interactable = false;
        spinTween = parentTransform
            .DORotate(new Vector3(0, 0, angle), spinSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart).SetSpeedBased(true);
    }
    public void SpinToSegment()
    {
        if (!isSpinning) return;

        stopSegment %= segmentCount;

        float segmentAngle = 360f / segmentCount;
        float targetAngle = 360f - (stopSegment * segmentAngle);
        float totalAngle = 2 * 360f;

        if (clockWise)
        {
            targetAngle *= -1;
            totalAngle *= -1;
        }

        float finalRotation = totalAngle + targetAngle;
        if (spinTween != null && spinTween.IsActive()) spinTween.Kill();
        isSpinning = true;
        float finalSpeed = (spinSpeed /4f);
        if (finalSpeed < 0)
        {
            finalSpeed = -finalSpeed;
        }
        parentTransform
            .DORotate(new Vector3(0, 0, finalRotation), finalSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart).SetSpeedBased(true).OnComplete(() =>
            {
                isSpinning = false;
            });
    }

    void OnWin()
    {
        int prizeValue = allSegments[stopSegment].prize;
        balance += prizeValue;
        balanceText.text = balance.ToString();
        winPopUp.SetActive(true);
        winPopUp.transform.GetChild(0).localScale = Vector2.zero;
        winPopUp.transform.GetChild(0).DOScale(Vector2.one, 1f).SetEase(Ease.OutBack);
        winPopUp.GetComponentInChildren<TMP_Text>().text = "You won: " + prizeValue.ToString();
    }
}

