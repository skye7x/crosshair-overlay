# Crosshair Overlay

- Runs in the system tray (look for the icon near the clock).
- **Ctrl+Alt+H** toggles the crosshair on/off.
- Right-click the tray icon for a toggle option and **Exit**.
- The overlay covers your primary monitor and is fully click-through —
  it won't intercept mouse clicks or interfere with games/apps underneath.

## How "streamproof" works

The app calls the Win32 `SetWindowDisplayAffinity` function with
`WDA_EXCLUDEFROMCAPTURE`. This is the same API Windows uses to hide things
like password fields from screenshots. It's effective against:

- OBS (Desktop Duplication / Windows Graphics Capture sources)
- Windows' built-in Snipping Tool / screenshots
- Most third-party screen recorders using standard Windows capture APIs

It will **not** hide the overlay from:

- A physical camera pointed at your screen
- Older/legacy captu
- re methods that bypass the Desktop Window Manager

## Adjusting:

```
        // ---- Crosshair appearance (tweak to taste) ----
        private const int ArmLength = 7;    // length of each arm, in pixels
        private const int GapSize = 0;      // empty gap around the center (0 = solid plus)
        private const int Thickness = 1;    // line thickness
        private const int DotRadius = 0;    // center dot radius (0 = no dot, just the plus)
```