# VTuber 관리 게임 — 아트 제작 사양서 (아티스트용 완전 설명서)

버전: 1.0  
작성일: 2025-11-25  
대상: UI/아이콘/일러스트/애니 담당 아티스트 (2인 개발: 프로그래머 + 아티스트)

목적: 현재 개발 중인 VTuber 관리 시뮬레이션의 모든 아트 요구사항을 한 파일로 정리합니다. 각 에셋의 용도, 파일명 규칙, 해상도/포맷, 우선순위(프로토타입→완성) 및 씬/스크립트와의 매핑을 포함합니다. 이 문서를 기반으로 원본 PSD/AI + 내보낸 PNG를 전달해 주세요.

---

## 1. 프로젝트 개요 (한 줄)
플레이어가 VTuber의 주간 스케줄을 짜고(1주 = 7일), 각 일차에 이벤트가 발생하는 라이프/매니지먼트 시뮬레이터입니다. UI 중심의 소규모 프로토타입 목표.

---

## 2. 아트 파트 전체 요구 (요약 우선순위)
A. 필수(프로토타입에 반드시 필요)
- 스케줄 노드 아이콘 5종 (Streaming/Training/Rest/Social/Empty)
- 스케줄 슬롯(빈/채움/하이라이트) 아트
- 버튼 세트 (확정/Undo/Clear) — Normal/Hover/Pressed/Disabled
- 팝업(이벤트/선택) 레이아웃 + 2개의 CTA 버튼 스타일
- 캐릭터 초상(기본 표정, 기쁨, 화남, 슬픔) — 최소 4종
- 배경 이미지(스튜디오 또는 방) — 1종

B. 권장(프로토타입 이후)
- 이벤트 일러스트(중요 이벤트용) 2~3종
- 피드백 이펙트(성공 토스트, 실패 토스트)
- 아이콘(팔로워, 뷰, 민심 등)
- 슬롯 채움 애니메이션(스프라이트 시트 또는 간단한 스케일/알파 시퀀스)

---

## 3. 파일/폴더 구조 (권장)
- Assets/Art/
  - UI/
    - Icon/ (ui_icon_node_{type}_128.png)
    - Button/ (btn_confirm_normal.png ...)
    - Slot/ (slot_base.png, slot_filled_overlay.png, slot_highlight.png)
    - Popup/ (popup_frame.png, popup_cta_accept.png, popup_cta_decline.png)
  - Characters/
    - Portraits/ (vtuber_main_expr_default.png, vtuber_main_expr_happy.png ...)
    - Sources/ (PSD/AI 원본)
  - Backgrounds/ (bg_studio_1920x1080.png)
  - Effects/ (toast_success_*, particle_*)
  - Sources/ (원본 PSD, AI, 폰트, 아이콘 원본)

---

## 4. 에셋 상세 스펙 (파일명 규칙 포함)

1) 스케줄 노드 아이콘 (필수)
- 용도: ScheduleUI의 슬롯에 채워지는 아이콘
- 항목: Streaming, Training, Rest, Social, None(빈)
- 파일명 예: ui_icon_node_streaming_128.png
- 해상도/포맷: 128x128 PNG (알파 채널)
- 원본: PSD(레이어 유지)
- 스타일 가이드: 단순·식별 쉬움, 테두리 있는 원형/사각 베이스 권장

2) 스케줄 슬롯 (필수)
- 파일:
  - slot_base_256.png (빈 슬롯 배경)
  - slot_filled_overlay_256.png (아이콘 위에 겹치는 채움 효과)
  - slot_highlight_256.png (선택/호버 상태)
- 해상도: 256x256 PNG (알파)
- 사용처: 7개 슬롯 UI에 재사용

3) 버튼 (필수)
- 버튼별 상태 이미지: Normal / Hover / Pressed / Disabled
- 파일명 예: btn_confirm_normal_260x80.png
- 권장 해상도: 260x80 (타이틀/라벨 공간 고려)
- 텍스트 아닌 그래픽(아이콘+컬러)로도 가능 — 개발자에게 텍스트 폰트/색 알려주세요

4) 이벤트 팝업 / 모달 (필수)
- frame: popup_frame_1200x600.png (중앙 백그라운드)
- CTA: popup_cta_accept_200x64.png, popup_cta_decline_200x64.png
- 요구: 타이틀 영역, 본문 텍스트 영역, 2개의 버튼 영역이 명확히 구분되어야 함

5) 캐릭터 초상 (필수)
- 파일: vtuber_main_1024_expr_default.png, vtuber_main_1024_expr_happy.png, vtuber_main_1024_expr_angry.png, vtuber_main_1024_expr_sad.png
- 원본 PSD 필수(표정 레이어 분리)
- 해상도: 1024x1024 PNG(알파) + PSD(레이어)
- 사용처: 이벤트 팝업, 프로필, 슬롯 툴팁

6) 배경 (권장)
- bg_studio_1920x1080.png (메인 씬 배경)
- PNG 또는 JPG(무손실 권장 PNG)

7) 이벤트 일러스트 (권장)
- major_event_1600x900.png (논란/대형스폰서 등)
- minor_event_800x450.png (작은 이벤트)

8) 아이콘(통계 등)
- icon_followers_64.png, icon_views_64.png, icon_reputation_64.png
- 해상도: 64x64 PNG

---

## 5. UI ↔ 아트 매핑 (어디에 어떤 이미지가 들어가는지)
- Schedule 화면
  - 7개의 슬롯: slot_base_256.png (배경) + ui_icon_node_{type}_128.png (채움)
  - 슬롯 하이라이트: slot_highlight_256.png (호버/선택)
  - 슬롯 채움 애니메이션: Effects/slot_fill_* (선택사항)
  - 버튼 바(하단): btn_confirm_*, btn_undo_*, btn_clear_*
- 이벤트 팝업
  - 팝업 프레임: popup_frame_1200x600.png
  - 좌측: 캐릭터 초상(표정 전환)
  - 중앙: 이벤트 일러스트(대형 이벤트)
  - 하단: popup_cta_accept / popup_cta_decline
- HUD / 상단 스테이터스
  - 아이콘: icon_followers_64.png 등
  - 폰트/색: 아래 스타일 가이드 참고

---

## 6. 스타일 가이드 (컬러, 폰트, 아이콘 톤)
- 기본 색상
  - Primary Accent: #FF6B6B (강조/Confirm 버튼)
  - Secondary: #4D9DE0 (보조)
  - Neutral BG: #FFFFFF / #F4F4F6
  - Disabled Gray: #BDBDBD
- 폰트
  - UI 폰트: Noto Sans KR (TTF 제공 필요)
  - 본문 크기 예시: Title 24pt / Button 18pt / Body 14pt
- 아이콘 톤
  - 단순하고 식별하기 쉬울 것
  - 라인 + 플랫 컬러(그라데이션 최소화)
- 일러스트 톤
  - 밝고 친근한 카툰계열(프로토타입 성격 고려)
  - 표정은 명확하게 읽히도록 과장 표현 권장

---

## 7. 애니메이션/이펙트 요구
- 슬롯 채움 피드백: 아이콘 등장 시 0.15s scale(0.8→1.05→1.0) 애니메이션
- 토스트(확정 성공): 상단에서 내려오는 작은 배너(0.5s in/out)
- 팝업 등장: 알파 + scale(0.9→1.0) 0.2s
- 이벤트 중요도별: major_event 일러스트는 간단한 줌/페이드 효과 적용

(애니메이션 구현은 개발자가 코드로 처리, 아티스트는 스프라이트 시트나 연속 프레임, 혹은 단일 이미지 + 에셋 가이드 제공)

---

## 8. 익스포트/유니티 임포트 설정 (아티스트에게)
- 포맷: PNG-24 (알파 포함)
- 스프라이트 설정: Texture Type = Sprite (2D and UI)
- Max Size: 아이콘 128→256, 포트레이트 1024, 배경 2048/4096 필요시
- Compression: None(프로토타입). 이후 LZ4/ETC2 팀 합의
- Filter Mode: Bilinear (UI), Point(픽셀아트)
- PPU(Pixels Per Unit): 100 (팀 표준)

---

## 9. 네이밍 규칙 (반드시 지켜주세요)
- ui_icon_node_{type}_128.png
- slot_base_256.png / slot_filled_overlay_256.png / slot_highlight_256.png
- btn_{name}_{state}_{w}x{h}.png (예: btn_confirm_normal_260x80.png)
- vtuber_{role}_expr_{mood}_1024.png (예: vtuber_main_expr_happy_1024.png)
- major_event_{id}_1600x900.png

---

## 10. 우선순위 및 전달 일정 제안
Sprint 0 (3일) — 프로토타입 가능
1. ui_icon_node(5종) + slot_base + slot_filled_overlay (필수)
2. btn_confirm / btn_undo / btn_clear (각 normal + pressed)
3. popup_frame 기본 + 2 CTA 버튼
4. vtuber_main 기본 표정

Sprint 1 (다음 주)
1. 추가 표정 3종
2. 이벤트 일러스트 2종
3. 배경 1종
4. 간단한 효과(토스트, 슬롯 채움)

---

## 11. 전달물 체크리스트 (아티스트가 전달할 때)
- [ ] PNG 내보내기(요청 해상도) 전부
- [ ] PSD/AI 원본(레이어 유지)
- [ ] 사용 폰트 파일 또는 라이선스 정보
- [ ] 간단한 사용 예시(어떤 파일을 어떤 UI에 쓸지 메모)
- [ ] (선택) 애니메이션 프레임 시트 또는 Gif 데모

---

## 12. 개발자와 협업시 주의/팁
- 파일명 변경/폴더 이동 시 반드시 알려주세요(자동화 스크립트 없음).
- 색상/폰트 변경은 사전에 합의하면 개발 코드(색상 상수/폰트 설정)를 같이 업데이트합니다.
- 애니메이션이 필요하면 프레임 시트(열/행) 또는 각 프레임을 분리한 PNG로 제공하면 개발자가 쉽게 연결합니다.
- 라이브2D/Spine 이용 시 원본 파일(.moc3 /.json 등)과 빌드용 에셋을 함께 전달하세요.

---

## 13. 예시 메시지 템플릿 (아티스트에게 보낼 메일)
안녕하세요, 아래 에셋이 필요합니다. PSD 원본과 PNG(요청 해상도) 함께 부탁드립니다.
- ui_icon_node_streaming_128.png … (중략, 상단의 '필수' 리스트 붙여넣기)
파일은 Assets/Art/ 심폴더로 올려주시고, 파일명 규칙 지켜주세요. 질문 있으시면 바로 DM 주세요.

---

감사합니다. 이 문서를 기반으로 파일을 제작해 전달해 주시면, 개발자(프로그래머)가 Unity에 바로 임포트해 연결하겠습니다. 추가로 각각의 슬롯 UI에 적용되는 '시각적 레퍼런스(목업 이미지)'를 원하시면 예시 PNG(모형)도 만들어 드리겠습니다.