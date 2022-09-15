# 크로노 아크 게임플레이 플러그인 :baguette_bread: :watch: :hedgehog:

한글번역을 해주신 [windypanda1](https://github.com/fwqefwqef) 감사합니다!

---

이 레파지토리에는 [크로노 아크](https://store.steampowered.com/app/1188930/Chrono_Ark/)의 여러 부분을 변경하는 플러그인들이 들어있습니다. 변경사항은 새로운 게임 모드, 새로 추가되거나 변경된 아이템이나 카드, 자잘한 게임플레이 향상 등이 있습니다.

기여는 환영이고 피드백도 주시면 감사할 것 같습니다!

[Harmony](https://github.com/pardeike/Harmony)와 [BepInEx](https://github.com/BepInEx/BepInEx)를 사용해서 만듬

---
현재플러그인 수는 많지 않지만 앞으로더많은플러그인이추가되기를기대합니다.


## 설치

Step 1. BepInEx를 다운로드하고 설정합니다. [공식 가이드](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_mono.html)를 참조하세요.

Step 2. [모드를 다운로드하고](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases) 선택한 .dll 파일을 `BepInEx/plugins` 폴더에 넣습니다.

### *또는*

BepInEx가 포함된 패키지를 다운받습니다. 64비트 버전 [[여기]](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases/download/1.1.1/allplugins_BepInEx_x64_included-13-09-2021.zip), 32비트 버전 [[여기]](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases/download/1.1.1/allplugins_BepInEx_x86_included-13-09-2021.zip). 64비트 버전의 경우 `<크로노 아크 디렉토리>\x64\Master` 또는 32 비트 버전의 경우 `<크로노 아크 디렉토리>\x86\Master` 에 압축을 풉니다 (BepInEx 폴더는 ChronoArk.exe가 있는 동일한 디렉토리에 있어야 함).

### 단계별 가이드:

1. [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21)를 다운로드합니다. 올바른 버전(32비트 또는 64비트)을 다운로드해야 합니다.
2. 로컬 크로노 아크 파일을 찾습니다. 아마도 `C:\Program Files (x86)\Steam\steamapps\common\Chrono Ark\` 에 있을 겁니다. 그렇지 않다면 Steam > 라이브러리 > 크로노 아크 > 관리(:gear: 아이콘) > 속성 > 로컬 파일 > 찾아보기
3. 64비트 버전을 사용하는 경우 `<크로노 아크 디렉토리>\x64\Master에 BepInEx` 를 압축 풀고 32비트 버전을 사용하는 경우 `<크로노 아크 디렉토리>\x86\Master` 에 압축을 풉니다.
4. Steam에서 크로노 아크를 실행하면 BepInEx가 `BepInEx\plugins` 폴더를 포함한 관련 파일을 자동생성합니다. 게임을 닫습니다.
5. [플러그인을 다운로드합니다](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases). 압축을 풀고 dll파일들을 plugins 폴더에 넣습니다.
6. (선택) 세이브 데이터를 백업합니다. 모드가 세이브를 망칠 걱정은 없어서 할 필요는 없습니다. 여전히 원하는 경우 [참조](https://steamcommunity.com/app/1188930/discussions/1/4917340730760337347/).
7. 게임을 즐깁니다!

*More cursed battles* 같은 일부 플러그인들은 처음 실행된 후 `<ChronoArk.exe가 있는 디렉토리>\BepInEx\config` 에 .cfg 파일을 생성합니다. 게임을 닫고 .cfg 파일을 수정하여 (txt 편집기 아무거나 사용가능) 플러그인을 구성할 수 있습니다.

## 플러그인 목록

* ### 저주 배틀 증가  :feelsgood:

각 스테이지 당 저주받은 몬스터가 증가합니다. 저주받은 적의 골드 보상이 감소합니다. 게임 시작시 식별된 저주해제 스크롤 2개가 주어집니다. 이제 성역에서 저주 배틀이 제대로 작동하고 원한다면 더 좋은 보상을 주도록 선택할 수 있습니다. 모든 변수값은 생성된 .cfg파일에서 구성할 수 있습니다. 게임 난이도와 상관없이 실행됩니다.

* ### 저주해제 카드 위치 변경 :scroll:

저주 배틀 중 받는 저주해제 카드를 손 위 대신 맨 아래에 생성합니다. 아자르의 패시브나 헬리아의 몇몇 카드를 초반 턴에 조금 좋게 만듭니다.

* ### 상향된 고통은 곧 행복 :carrot:

후즈의 고통은 곧 행복! 카드를 상향조정합니다. 고통 전환 버프의 지속기간을 1턴 증가시키고 아군에게서 고통피해를 입은 경우 추가회복을 받습니다.

* ### 변경된 그림자 장막 :dagger:
이제 트리샤의 그림자 장막은 일회성이지만 고정 스킬로 사용할 수 있습니다. 더 이상 환영복제와 사용하면 사기는 아니지만 1.56 그림자 장막보단 좋습니다.

* ### 원소 트리오 디버프 유지 삭제 :fire:

성역 원소 트리오는 교전 후 디버프를 남기지 않습니다.

## 삭제
`plugin` 폴더에서 .dll 파일들을 삭제하고 해당되는 경우 `config` 폴더에서 플러그인 구성 파일을 삭제합니다.
