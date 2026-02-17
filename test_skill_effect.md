# 스킬 이펙트 디버깅 가이드

## 1️⃣  Unity Console 확인사항

**Unity에서 플레이 모드 실행 후 Console 창(Cmd+Shift+C)에서 다음 로그를 찾아주세요:**

### ✅ 있어야 할 로그:

```
[ManaBar] 💙 [Rare] Archer: 50% reached (50.0/100)
[ManaBar] 💙 [Rare] Archer: 100% FULL! (100.0/100)
[Unit] 🌟 Archer activated skill: 화살 비
[SimpleFloatingText] ✅ Created: '화살 비' at (x, y, z)
```

### ❌ 실제 보이는 로그:

```
[ManaBar] 💙 Phoenix: RESET 46%→0% (skill used!)
```
→ 이건 **게임 종료 시 리셋**이지 스킬 발동이 아닙니다!

---

## 2️⃣  문제: 마나가 100%까지 차지 않음!

**가능한 원인:**

1. **전투 시간이 너무 짧음** - 몬스터가 너무 빨리 끝남
2. **마나 재생 속도가 너무 느림**
3. **CombatTick이 호출되지 않음**

---

## 3️⃣  긴급 해결책: 마나 재생 속도 10배 증가

마나가 빠르게 차도록 임시로 속도를 올리겠습니다!

---

## 4️⃣  확인 요청

**Unity Console에서 다음 로그가 보이나요?**

- [ ] `[ManaBar] 💙 ... 50% reached`
- [ ] `[ManaBar] 💙 ... 100% FULL!`
- [ ] `[Unit] 🌟 ... activated skill`

**하나라도 보이면** → 마나 시스템은 작동 중, 스킬 이펙트만 수정하면 됨
**아무것도 안 보이면** → 마나가 차지 않음, 재생 속도를 높여야 함
