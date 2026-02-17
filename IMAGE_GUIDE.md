# 🎨 Lotto-Defense 이미지 가이드

## 📐 **유닛 이미지 규격**

### **필수 규격:**
```
형식: PNG
크기: 64 x 64 픽셀
컬러: RGBA (투명도 지원)
배경: 투명 (Alpha Channel)
해상도: 72 DPI 이상
```

### **권장 사항:**
- **정사각형 비율** (1:1)
- **중앙 정렬** - 캐릭터가 이미지 중앙에 위치
- **여백 확보** - 좌우 5~10px 여백 (게임에서 80% 크기로 렌더링)
- **선명한 외곽선** - 작은 크기에서도 식별 가능하게
- **밝은 색상** - 어두운 배경에서도 보이도록

### **파일명 규칙:**
```
영어로 작성 (공백 없이)
예시:
  - Warrior.png
  - Dragon_Knight.png (언더스코어 사용)
  - PhoenixMage.png (CamelCase)
```

---

## 📂 **파일 위치**

### **유닛 이미지:**
```
Assets/Resources/Sprites/Units/[유닛이름].png
```

예시:
```
Assets/Resources/Sprites/Units/
├── Warrior.png
├── Archer.png
├── Mage.png
├── DragonKnight.png
└── Phoenix.png
```

### **몬스터 이미지:**
```
Assets/Resources/Sprites/Monsters/[몬스터이름].png
```

예시:
```
Assets/Resources/Sprites/Monsters/
├── Goblin.png
├── Slime.png
├── ArmoredOgre.png
├── SpeedDemon.png
└── DragonBoss.png
```

---

## 🎨 **이미지 제작 가이드**

### **스타일:**
- **2D 픽셀아트** 또는 **벡터 아트**
- **귀여운/카툰** 스타일 권장
- **명확한 실루엣** - 멀리서도 구분 가능

### **레이어 구성 (권장):**
```
1. 배경 (투명)
2. 그림자 (선택사항)
3. 캐릭터 본체
4. 외곽선 (2~3px)
5. 하이라이트
```

### **색상:**
- **등급별 테마 색상:**
  - Normal (일반): 회색/갈색 톤
  - Rare (희귀): 파란색/청록색 톤
  - Epic (영웅): 보라색/자주색 톤
  - Legendary (전설): 황금색/오렌지색 톤

---

## 🛠️ **Unity에 이미지 추가하는 방법**

### **1단계: 이미지 준비**
```
✅ PNG 파일 (64x64)
✅ 투명 배경
✅ 파일명 영어로 작성
```

### **2단계: Unity에 드래그**
```
1. Unity 프로젝트 열기
2. Project 창 → Assets/Resources/Sprites/Units/ 폴더
3. 준비한 PNG 파일을 드래그 앤 드롭
```

### **3단계: Import 설정 (자동 적용됨)**
Unity가 자동으로 다음 설정을 적용합니다:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Multiple
Pixels Per Unit: 100
Filter Mode: Bilinear
Max Size: 2048
Compression: Normal Quality
```

### **4단계: UnitData에 연결**
```
1. Project 창 → Assets/Resources/Units/[유닛이름].asset
2. Inspector 창 → Sprite
3. 방금 추가한 이미지를 드래그해서 연결
```

---

## 🖼️ **이미지 예시**

### **Warrior (전사):**
```
- 크기: 64x64
- 모습: 검과 방패 든 기사
- 색상: 회색 갑옷, 빨간 망토
- 외곽선: 검정 2px
```

### **Archer (궁수):**
```
- 크기: 64x64
- 모습: 활을 든 엘프
- 색상: 녹색 옷, 갈색 활
- 외곽선: 검정 2px
```

### **Mage (마법사):**
```
- 크기: 64x64
- 모습: 지팡이 든 마법사
- 색상: 보라색 로브, 파란 모자
- 외곽선: 검정 2px
```

---

## 📦 **제공할 이미지 목록**

### **유닛 이미지 (최소 5개):**
1. **Warrior** - 전사 (근접)
2. **Archer** - 궁수 (원거리)
3. **Mage** - 마법사 (광역)
4. **DragonKnight** - 드래곤 기사 (전설)
5. **Phoenix** - 불사조 (전설)

### **몬스터 이미지 (최소 5개):**
1. **Goblin** - 고블린 (약함)
2. **Slime** - 슬라임 (약함)
3. **ArmoredOgre** - 방어 오우거 (중간)
4. **SpeedDemon** - 빠른 악마 (빠름)
5. **DragonBoss** - 드래곤 보스 (강함)

---

## 🎯 **빠른 체크리스트**

이미지를 주기 전에 확인하세요:

```
✅ PNG 형식
✅ 64x64 픽셀
✅ 투명 배경 (RGBA)
✅ 파일명 영어로
✅ 캐릭터가 중앙에 위치
✅ 외곽선 있음 (선명함)
✅ 밝은 색상 (어두운 배경에서 보임)
```

---

## 🚀 **고급 옵션 (선택사항)**

### **애니메이션 스프라이트:**
현재는 정적 이미지만 사용하지만, 나중에 애니메이션을 추가하려면:

```
크기: 64x64 x N프레임
예시: 256x64 (4프레임 애니메이션)

프레임 구성:
[대기][공격][피격][죽음]
```

### **해상도 옵션:**
- **기본:** 64x64 (현재)
- **고해상도:** 128x128 (나중에 업스케일)
- **4K 지원:** 256x256 (최고 품질)

Unity가 자동으로 스케일 조정하므로 큰 이미지로 주셔도 됩니다!

---

## 💡 **추천 도구**

### **이미지 제작:**
- **Aseprite** - 픽셀아트 전문
- **Photoshop** - 전문가용
- **GIMP** - 무료 대안
- **Krita** - 무료, 그림 특화
- **Procreate** - iPad용

### **AI 생성:**
- **Midjourney** - 고품질 캐릭터
- **DALL-E 3** - 일러스트 스타일
- **Stable Diffusion** - 커스터마이징 가능

---

## 📧 **이미지 전달 방법**

### **옵션 1: 직접 파일 제공**
```
1. ZIP 파일로 압축
2. 폴더 구조:
   Units/
     ├── Warrior.png
     ├── Archer.png
     └── ...
   Monsters/
     ├── Goblin.png
     └── ...
3. 카카오톡/텔레그램으로 전송
```

### **옵션 2: 클라우드 링크**
```
- Google Drive 링크
- Dropbox 링크
- WeTransfer 링크
```

---

## 🎨 **현재 프로젝트 이미지 상태**

```
✅ 있는 이미지:
  Units:
    - Warrior.png (64x64)
    - Archer.png (64x64)
    - Mage.png (64x64)
    - DragonKnight.png (64x64)
    
  Monsters:
    - Goblin.png (64x64)
    - Slime.png (64x64)
    - ArmoredOgre.png (64x64)
    - SpeedDemon.png (64x64)
    - DragonBoss.png (64x64)

❌ 없는 이미지:
  - Phoenix.png (전설 유닛)
```

---

**새 이미지를 주시면 바로 Unity에 추가하겠습니다!** 🎨✨
