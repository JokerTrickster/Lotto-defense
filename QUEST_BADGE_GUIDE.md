# 퀘스트 알림 배지 가이드

## 개요

히든 퀘스트를 달성하면 퀘스트 버튼 우상단에 **빨간 숫자 배지**가 표시됩니다.

---

## 기능

### ✅ 자동 동작
```
1. 게임 중 히든 퀘스트 달성 → 배지에 "1" 표시
2. 또 다른 퀘스트 달성 → 배지에 "2" 표시
3. 퀘스트 버튼 클릭 → 배지 사라짐
4. 다시 퀘스트 달성 → 배지 다시 나타남
```

### 🎨 디자인
```
위치: 퀘스트 버튼 우상단 (-8, -8)
크기: 28x28 픽셀
배경: 빨간 원 (#F24040)
텍스트: 흰색, 굵게, 18pt
외곽선: 어두운 빨강
```

### 📊 표시 범위
```
0개: 배지 숨김
1-99개: 숫자 그대로 표시
100개 이상: "99+" 표시 (작은 폰트)
```

---

## 구현 세부사항

### 1. QuestNotificationBadge.cs

**싱글톤 패턴:**
```csharp
QuestNotificationBadge.Instance.IncrementCount();  // 카운트 +1
QuestNotificationBadge.Instance.ResetCount();      // 카운트 초기화
QuestNotificationBadge.Instance.SetCount(5);       // 특정 값 설정
```

**자동 이벤트 구독:**
```csharp
// QuestManager.OnQuestCompleted 자동 구독
private void OnQuestCompleted(QuestInstance quest)
{
    IncrementCount();  // 퀘스트 달성 시 자동으로 +1
}
```

**UI 생성:**
```csharp
public void CreateBadge(Transform parentButton)
{
    // 동그란 빨간 배지
    // 흰색 숫자 텍스트
    // 외곽선 효과
}
```

---

### 2. GameSceneBootstrapper.UI.cs

**퀘스트 버튼에 배지 추가:**
```csharp
private void EnsureQuestButton()
{
    // ... 버튼 생성 코드 ...

    // 배지 컴포넌트 추가
    QuestNotificationBadge badge = btnObj.AddComponent<QuestNotificationBadge>();
    badge.CreateBadge(btnObj.transform);

    // 버튼 클릭 시 배지 초기화
    button.onClick.AddListener(() => {
        QuestUI questUI = FindFirstObjectByType<QuestUI>();
        if (questUI != null)
        {
            questUI.Show();
            
            // 배지 초기화
            if (QuestNotificationBadge.Instance != null)
            {
                QuestNotificationBadge.Instance.ResetCount();
            }
        }
    });
}
```

---

### 3. QuestUI.cs

**퀘스트 창 열 때 배지 초기화:**
```csharp
public void Show()
{
    if (questPanel != null)
        questPanel.SetActive(true);

    RefreshQuestList();

    // 배지 초기화
    if (QuestNotificationBadge.Instance != null)
    {
        QuestNotificationBadge.Instance.ResetCount();
    }
}
```

---

## UI 계층 구조

```
QuestButton (60x60)
├── Image (배경)
├── Shadow (그림자)
├── Button (클릭)
├── Text "Q" (아이콘)
└── QuestBadge (28x28) ← NEW!
    ├── Image (빨간 원)
    ├── Outline (외곽선)
    └── BadgeText
        ├── Text (숫자)
        └── Outline (텍스트 외곽선)
```

---

## 테스트 방법

### 1. Unity Editor에서 테스트

**자동 테스트:**
1. Play 버튼 클릭
2. 게임 시작
3. 히든 퀘스트 달성 (예: 특정 유닛 소환)
4. 퀘스트 버튼 우상단에 "1" 배지 확인
5. 퀘스트 버튼 클릭
6. 배지 사라짐 확인

**수동 테스트 (Console에서):**
```csharp
// Console 창에서 실행
QuestNotificationBadge.Instance.SetCount(5);   // 배지에 "5" 표시
QuestNotificationBadge.Instance.ResetCount();  // 배지 숨김
```

---

### 2. 빌드 테스트

**모바일 (Android/iOS):**
1. 빌드 & 실행
2. 퀘스트 달성
3. 배지 터치 (퀘스트 버튼)
4. 퀘스트 창 열림 + 배지 사라짐

**Safe Area 테스트:**
- 노치가 있는 기기에서도 배지가 버튼 위에 올바르게 표시됨
- 배지 위치: 퀘스트 버튼 기준 상대 좌표

---

## 커스터마이징

### 배지 색상 변경

**QuestNotificationBadge.cs (91줄):**
```csharp
// 기본: 빨간색
badgeImage.color = new Color(0.95f, 0.25f, 0.25f, 1f);

// 파란색
badgeImage.color = new Color(0.25f, 0.5f, 0.95f, 1f);

// 초록색
badgeImage.color = new Color(0.25f, 0.85f, 0.35f, 1f);
```

---

### 배지 크기 변경

**QuestNotificationBadge.cs (82줄):**
```csharp
// 기본: 28x28
badgeRect.sizeDelta = new Vector2(28, 28);

// 크게: 36x36
badgeRect.sizeDelta = new Vector2(36, 36);

// 작게: 20x20
badgeRect.sizeDelta = new Vector2(20, 20);
```

---

### 배지 위치 변경

**QuestNotificationBadge.cs (84줄):**
```csharp
// 기본: 우상단 (-8, -8)
badgeRect.anchoredPosition = new Vector2(-8, -8);

// 더 안쪽: (-12, -12)
badgeRect.anchoredPosition = new Vector2(-12, -12);

// 좌상단: (8, -8)
badgeRect.anchorMin = new Vector2(0f, 1f);
badgeRect.anchorMax = new Vector2(0f, 1f);
badgeRect.anchoredPosition = new Vector2(8, -8);
```

---

### 텍스트 크기 변경

**QuestNotificationBadge.cs (133줄):**
```csharp
// 기본: 18pt
badgeText.fontSize = 18;

// 크게: 22pt
badgeText.fontSize = 22;

// 작게: 14pt
badgeText.fontSize = 14;
```

---

## 퀘스트 완료 조건 예시

**QuestManager.cs에서 퀘스트 달성 조건:**

```csharp
// 예시 1: 특정 유닛 5개 소환
if (quest.type == QuestType.SummonUnits && 
    quest.targetUnitId == "Archer" && 
    unitCount >= 5)
{
    quest.isCompleted = true;
    OnQuestCompleted?.Invoke(quest);  // ← 배지 +1
}

// 예시 2: 10라운드 생존
if (quest.type == QuestType.SurviveRounds && 
    currentRound >= 10)
{
    quest.isCompleted = true;
    OnQuestCompleted?.Invoke(quest);  // ← 배지 +1
}

// 예시 3: 100골드 모으기
if (quest.type == QuestType.CollectGold && 
    totalGold >= 100)
{
    quest.isCompleted = true;
    OnQuestCompleted?.Invoke(quest);  // ← 배지 +1
}
```

**OnQuestCompleted 이벤트 발생 시 자동으로 배지 카운트 증가!**

---

## 디버깅

### Console 로그 확인

```
[QuestNotificationBadge] Badge UI created
[QuestNotificationBadge] Count increased to 1
[QuestNotificationBadge] Count increased to 2
[QuestNotificationBadge] Count reset
```

### 배지가 안 보일 때

**체크리스트:**
1. QuestNotificationBadge.Instance != null 확인
2. CreateBadge()가 호출되었는지 확인
3. notificationCount > 0 인지 확인
4. badgeObj.activeSelf == true 인지 확인

**강제로 표시:**
```csharp
// Inspector에서 또는 Console에서
QuestNotificationBadge.Instance.SetCount(1);
```

---

### 배지가 초기화 안 될 때

**체크리스트:**
1. QuestUI.Show()가 호출되는지 확인
2. button.onClick에 ResetCount()가 등록되었는지 확인
3. Console에서 "Count reset" 로그 확인

---

## 성능 고려사항

### 최적화
- **배지 생성:** 게임 시작 시 1회만
- **카운트 업데이트:** O(1) 연산, 즉시 처리
- **텍스처 생성:** 32x32 원형 스프라이트 1회 생성
- **이벤트 구독:** Start()에서 1회만

### 메모리
- **텍스처:** 32x32 RGBA32 = 4KB
- **배지 UI:** 3개 GameObject (배지, 텍스트, 외곽선)
- **총 메모리:** < 10KB

---

## 향후 개선 아이디어

### 1. 애니메이션 추가
```csharp
// 배지 등장 시 펄스 효과
DOTween.Sequence()
    .Append(badgeObj.transform.DOScale(1.2f, 0.15f))
    .Append(badgeObj.transform.DOScale(1f, 0.15f));
```

### 2. 사운드 효과
```csharp
private void OnQuestCompleted(QuestInstance quest)
{
    IncrementCount();
    AudioManager.Instance?.PlaySound("quest_notification");
}
```

### 3. 다양한 배지 스타일
```csharp
// 중요도에 따라 색상 변경
if (quest.priority == QuestPriority.High)
    badgeImage.color = Color.red;     // 빨강
else if (quest.priority == QuestPriority.Medium)
    badgeImage.color = Color.yellow;  // 노랑
else
    badgeImage.color = Color.blue;    // 파랑
```

### 4. 배지 툴팁
```csharp
// 배지에 마우스 오버 시 "3개의 새 퀘스트" 표시
void OnPointerEnter()
{
    ShowTooltip($"{notificationCount}개의 새 퀘스트");
}
```

---

## 문제 해결

### Q: 배지가 퀘스트 버튼 밑에 숨겨집니다.
**A:** RectTransform의 siblingIndex를 마지막으로 설정:
```csharp
badgeObj.transform.SetAsLastSibling();
```

### Q: 배지가 너무 작아 보입니다.
**A:** sizeDelta를 36x36으로 증가:
```csharp
badgeRect.sizeDelta = new Vector2(36, 36);
```

### Q: 배지 숫자가 잘립니다.
**A:** 텍스트 overflow 설정 확인:
```csharp
badgeText.horizontalOverflow = HorizontalWrapMode.Overflow;
badgeText.verticalOverflow = VerticalWrapMode.Overflow;
```

### Q: 퀘스트 창을 닫았는데 배지가 다시 나타납니다.
**A:** 정상 동작입니다. 새로운 퀘스트를 달성하면 배지가 다시 표시됩니다.

---

## 참고

- **파일 위치:** `Assets/Scripts/UI/QuestNotificationBadge.cs`
- **의존성:** QuestManager, QuestUI
- **플랫폼:** Unity 2021.3+, 모든 플랫폼 호환
- **테스트:** Unity Editor, Android, iOS

---

**이제 플레이어가 퀘스트를 달성할 때마다 즉각적인 시각적 피드백을 받을 수 있습니다!** 🎉
