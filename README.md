# ZeldaPlatformer2D: Prototype Action-Platformer Game

This is my first videogame project. It's a basic but dynamic platformer based on Zelda with some HD-2D details. Hope you like it!

---

## Download & Installation

### Windows
1. Download the `.zip` file for Windows and extract it.
2. Open the extracted folder and run `ZeldaPlatformer2D.exe`.
3. If Microsoft Defender SmartScreen displays a warning (*"Windows protected your PC"*):
   * Click **"More info"**.
   * Click **"Run anyway"**.

### Linux
1. Download the `.zip` file for Linux and extract it.
2. Open the extracted folder and execute `ZeldaPlatformer2D-v<version>-Linux-x86_64`.

### macOS
Due to macOS security gatekeeper policies on unsigned apps, follow these steps to run the game:

1. Extract the `.zip` file so the `.app` file is visible.
2. Open **Terminal**.
3. Copy and paste the following command into Terminal, ensuring there is a **space at the very end**:
   ```bash
   chmod -R +x
4. Drag and drop the game's `.app` icon from Finder directly into the Terminal window (this automatically fills in the file path).
5. Press `Enter`.
> Note: If the game still errors out, try chmod a+x /PathToGame/GameName.app/Contents/MacOS/*
6. In the same Terminal window, copy and paste this command (leave a space at the end):
   ```bash
   xattr -cr
7. Drag and drop the game's `.app` icon into the Terminal window again.
8. Press Enter.
9. Right-click (or hold `Control` and click) the `.app` icon and select **Open**.
10. If a popup appears asking for confirmation, click **Open** or **Open Anyway**.

---

## Controls

### Keyboard & Mouse
* **Move:** `A` / `D` or `Left` / `Right` Arrow Keys
* **Jump:** `Spacebar`
* **Run:** `Hold Shift`
* **Attack:** `Left Mouse Button`
* **Pause Menu:** `Escape`
* **Skip Intro / Cutscene:** Any
* **Shoot Arrow:** `F` 


### Gamepad (Switch Layout)
* **Move:** `Left Stick`
* **Jump:** `B` Button
* **Run:** `Y` Button
* **Attack:** `A` Button
* **Pause Menu:** `+` Button (Start) or `-` Button (Select) 
* **Skip Intro / Cutscene:** Any
* **Shoot Arrow:** `ZR` / `RT` (Right Trigger / `R2`) 

### Advanced Jump Mechanics & Physics

The movement system features dynamic variable jumping mechanics:
- **Variable Jump Height**: Tapping the jump button triggers a short hop, while holding it down results in a full-height jump (similar to mechanics seen in _Super Smash Bros._).
- **Mid-Air Momentum Control**: Holding the Run button (Shift / Y) while in mid-air preserves horizontal velocity for maximum distance. Releasing it immediately dampens your forward momentum for precise landings.
- **Long Jump (Combined Tech)**: Executing a running jump while holding both Jump and Run optimizes mid-air velocity, allowing Link to cross wide gaps and reach distant platforms effortlessly.

---

## Credits & Acknowledgments

### Project & Creation
* **Game Developer & Animator:** Samuel Arosemena ([@Shack66](https://github.com/Shack66))
* **Inspired by Scene Arrangement:** u/slittle619 (Sam The Bard)
* **Learning & Base Engine:** "2D Platformer Crash Course in Unity 2022" by Chris' Tutorials

### Graphics & Art
* **Menu UI & Buttons:** Benay Daniel (ArtStation)
* **Link & Dark Link Sprites (Skyward Sword):** Made by GregarLink10, uploaded by GregarLink15
* **Darknut Sprites (Tales of the World-Style):** Uploaded by SmithyGCN
* **North Temple Assets:** Balladofwindfishes & Contributors
* **Health Bar UI:** rappenem (Reddit)
* **Game Logo Pixel Art:** Lachington (Reddit)
* **Heart Sprite & Item Custom Assets:** ChaosMiles07 (Base OoT sprites ripped by GaryCXJk)

*Archived / Unused Sprites:*
* **Ganondorf (OoT):** Made by GregarLink10, uploaded by GregarLink15
* **Stalfos Sprite:** Animated by Redhalberd

### Music & Audio
* **Zelda 2 Temple Theme:** Arranged by Sam The Bard | Original Composer: Akito Nakatsuka (Nintendo)
* **Final Fantasy VII - Victory Fanfare:** Uploaded by PadookieBruh | Original Composer: Nobuo Uematsu (Square Enix)
* **Menu Theme (The Minish Cap):** Arranged by Tatsuya | Original Composer: Mitsuhiko Takano (Nintendo)
* **Sound Effects (SFX):** Original Audio from *The Legend of Zelda: Ocarina of Time* & *Twilight Princess* (Nintendo) — Extracted via ZeldaSounds (noproblo.dayjo.org)

### Acknowledgments & Tools
* **AI Assistance:** Gemini (Google) was utilized as a collaborative coding partner for debugging UI navigation, slight image edition, structuring text formats, and refactoring C# scripts in Unity.

---

## Legal Disclaimer & Trademarks

> This is a non-profit fan-made project created solely for educational and portfolio purposes. It is not affiliated with, endorsed, sponsored, or specifically approved by Nintendo Co., Ltd. or Square Enix Co., Ltd.
> 
> All copyrighted materials, including *The Legend of Zelda* assets, character concepts, audio, and the *Final Fantasy VII Victory Fanfare*, are the exclusive intellectual property of their respective owners:
> 
> * **The Legend of Zelda** series, characters, sound effects, and music © Nintendo Co., Ltd.
> * **Final Fantasy VII Victory Fanfare** © Square Enix Co., Ltd. / Nobuo Uematsu.
> 
> No copyright infringement is intended. All rights belong to their original owners.

---

### Thank you for playing!