; ============================================================
;  Healing Temple Ledger — NSIS Installer Script
;  Target: Windows 10/11 64-bit
;  Output: HealingTempleLedger_Setup_v1.0.0.exe
;
;  Build with: makensis HealingTempleLedger_Installer.nsi
;  Requires NSIS 3.x from https://nsis.sourceforge.io
; ============================================================

!define APP_NAME        "Healing Temple Ledger"
!define APP_VERSION     "1.0.0"
!define APP_PUBLISHER   "Healing Temple Ledger"
!define APP_URL         "https://redressright.me"
!define APP_EXE         "HealingTempleLedger.exe"
!define APP_DIR         "$PROGRAMFILES64\HealingTempleLedger"
!define UNINSTALL_KEY   "Software\Microsoft\Windows\CurrentVersion\Uninstall\HealingTempleLedger"
!define DATA_DIR        "$APPDATA\HealingTempleLedger"

Name "${APP_NAME} v${APP_VERSION}"
OutFile "HealingTempleLedger_Setup_v${APP_VERSION}.exe"
InstallDir "${APP_DIR}"
InstallDirRegKey HKLM "Software\HealingTempleLedger" "InstallDir"
RequestExecutionLevel admin
SetCompressor /SOLID lzma
BrandingText "${APP_PUBLISHER} — ${APP_URL}"

; ── Pages ────────────────────────────────────────────────────────────────────
!include "MUI2.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON         "Assets\icon.ico"
!define MUI_UNICON       "Assets\icon.ico"
!define MUI_HEADERIMAGE
!define MUI_BGCOLOR      "0D0F12"
!define MUI_TEXTCOLOR    "D4DBE8"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

; ── Installer ────────────────────────────────────────────────────────────────
Section "Healing Temple Ledger" SecMain
    SectionIn RO
    SetOutPath "$INSTDIR"

    ; Copy all application files
    File /r "publish\*.*"

    ; Create data directory
    CreateDirectory "${DATA_DIR}"

    ; Write install info to registry
    WriteRegStr HKLM "Software\HealingTempleLedger" "InstallDir" "$INSTDIR"
    WriteRegStr HKLM "Software\HealingTempleLedger" "Version"    "${APP_VERSION}"

    ; Add/Remove Programs entry
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "DisplayName"          "${APP_NAME}"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "DisplayVersion"       "${APP_VERSION}"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "Publisher"            "${APP_PUBLISHER}"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "URLInfoAbout"         "${APP_URL}"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "InstallLocation"      "$INSTDIR"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "UninstallString"      "$INSTDIR\Uninstall.exe"
    WriteRegStr   HKLM "${UNINSTALL_KEY}" "DisplayIcon"          "$INSTDIR\${APP_EXE}"
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoModify"             1
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoRepair"             1

    ; Estimate size (in KB)
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "EstimatedSize"        65536

    ; Write uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ; Desktop shortcut
    CreateShortcut "$DESKTOP\HealingTempleLedger.lnk" \
        "$INSTDIR\${APP_EXE}" "" \
        "$INSTDIR\${APP_EXE}" 0 \
        SW_SHOWNORMAL "" \
        "Healing Temple Ledger — Lawful Self-Governance & GAAPCLAW Suite"

    ; Start Menu
    CreateDirectory "$SMPROGRAMS\HealingTempleLedger"
    CreateShortcut "$SMPROGRAMS\HealingTempleLedger\HealingTempleLedger.lnk" \
        "$INSTDIR\${APP_EXE}" "" \
        "$INSTDIR\${APP_EXE}" 0 \
        SW_SHOWNORMAL "" \
        "Healing Temple Ledger"
    CreateShortcut "$SMPROGRAMS\HealingTempleLedger\Uninstall HealingTempleLedger.lnk" \
        "$INSTDIR\Uninstall.exe"

SectionEnd

; ── Uninstaller ──────────────────────────────────────────────────────────────
Section "Uninstall"
    ; Remove files
    RMDir /r "$INSTDIR"

    ; Remove Start Menu
    RMDir /r "$SMPROGRAMS\HealingTempleLedger"

    ; Remove Desktop shortcut
    Delete "$DESKTOP\HealingTempleLedger.lnk"

    ; Remove registry keys
    DeleteRegKey HKLM "${UNINSTALL_KEY}"
    DeleteRegKey HKLM "Software\HealingTempleLedger"

    ; Prompt about data
    MessageBox MB_YESNO "Remove saved data and settings from $APPDATA\\HealingTempleLedger?" IDNO SkipData IDNO SkipData
    RMDir /r "${DATA_DIR}"
    SkipData:

SectionEnd
