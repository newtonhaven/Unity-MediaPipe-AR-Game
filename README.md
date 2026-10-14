# Unity MediaPipe AR Game

An interactive, gesture-controlled Augmented Reality action-defense game built in Unity. This project integrates **Google MediaPipe Hand Tracking (Vision Tasks API)** and **Unity AR Foundation** to enable touch-free, physical hand interactions with virtual 3D elements in an AR workspace.

---

## Key Features

*   **Real-time Hand Landmark Tracking**: Tracks the player's hand skeleton in real-time directly from the device's camera stream using Google MediaPipe.
*   **AR Foundation Camera Integration**: Redirection of the device's AR camera feed to MediaPipe's CPU image processing pipeline without disrupting standard AR visual overlays or camera tracking.
*   **Scale-based 3D Depth Estimation**: Estimates the hand's distance (Z-depth) from the lens by analyzing the 2D pixel distance between the Wrist and the Middle Finger MCP joint.
*   **Virtual Physics Interactions**: Maps a virtual 3D collider to the player's index finger tip, allowing physical collision triggers with virtual objects.
*   **AR-Anchored Targets**: Features interactive enemy targets (Golems) that auto-anchor to physical space, react to missile impacts, update scores, and respawn in randomized areas.
*   **Physical Buttons**: Spawns in-world triggers (Shoot Rockets, Deploy Force Shields) that are clicked by physically tapping them with your virtual finger.

---

## How It Works (The Bridge Solution)

Out-of-the-box, Google MediaPipe and Unity's AR Foundation conflict because both compete for control over the camera hardware. This project implements a custom bridge to resolve this:

1.  **Frame Acquisition**: [ARRawCameraSource.cs](Assets/Scripts/ARRawCameraSource.cs) registers to `ARCameraManager.frameReceived`, captures the raw CPU image buffer via `TryAcquireLatestCpuImage`, and formats it to a `Texture2D` in `RGBA32` with Y-mirroring applied.
2.  **Model Inference**: The frame is fed into MediaPipe's hand tracking model, which computes landmark coordinates asynchronously on a native C++ thread.
3.  **Main Thread Dispatching**: To prevent thread safety violations, [MainThreadDispatcher.cs](Assets/Scripts/MainThreadDispatcher.cs) enqueues MediaPipe callbacks and schedules them to execute on Unity's single-threaded main loop.
4.  **2D-to-3D Coordinate Mapping**: Hand coordinates ($x$, $y$) are normalized ($[0,1]$ space) and flipped to match Unity's viewport coordinates, combined with scale-based depth, and projected to 3D world space coordinates via `Camera.ViewportToWorldPoint()`.

For a deep dive into the technical details and formulas, check out the full [Technical Report (raport.md)](raport.md).

---

## Project Structure & Key Scripts

*   **[ARRawCameraSource.cs](Assets/Scripts/ARRawCameraSource.cs)**: Custom frame-tap bridge between Unity AR Foundation and MediaPipe.
*   **[HandLandmarkerRunner.cs](Assets/MediaPipeUnity/Samples/Scenes/Hand%20Landmark%20Detection/HandLandmarkerRunner.cs)**: Configures MediaPipe hand landmark tasks, feeds inputs, handles output callbacks, and moves the virtual finger.
*   **[HandSkeletonVisualizer.cs](Assets/Scripts/HandSkeletonVisualizer.cs)**: Renders the skeletal structure of the tracked hand joints in 3D.
*   **[ShootButton.cs](Assets/Scripts/ShootButton.cs)**: Logic for the virtual shoot button, instantiating and firing physical rockets.
*   **[ShieldButton.cs](Assets/Scripts/ShieldButton.cs)**: Activates a protective force field on a cooldown timer.
*   **[GolemTarget.cs](Assets/Scripts/GolemTarget.cs)**: Handles AR anchoring, damage collisions, score dispatching, and respawning.

---

## Installation & Setup

### Requirements
*   Unity **2022.3 LTS** (or higher)
*   An AR-compatible mobile device:
    *   **Android**: ARCore-supported device (USB debugging enabled)
    *   **iOS**: ARKit-supported device
*   *Alternatively*: A computer webcam can be used for in-editor testing (`UNITY_EDITOR`).

### APK File is in Releases

### Steps For Build
1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/newtonhaven/Unity-MediaPipe-AR-Game.git
    cd Unity-MediaPipe-AR-Game
    ```
2.  **Open in Unity**:
    *   Launch Unity Hub, click **Add**, and select the project folder.
    *   Ensure the project platform is switched to Android or iOS in **File > Build Settings**.
3.  **MediaPipe Models**:
    *   Confirm that `hand_landmarker.bytes` is present in the `Assets/StreamingAssets` folder. If missing, retrieve it from the MediaPipe website and place it there.
4.  **Build and Deploy**:
    *   Open `Assets/Scenes/Main.unity` (or the corresponding main AR Scene).
    *   Go to **File > Build Settings**, select your target platform, and click **Build and Run**.

---

## How to Play

1.  Hold your phone/device up to scan your environment.
2.  Bring your hand into the camera's view. You will see a virtual skeleton overlay mapped onto your joints.
3.  Move your hand to position your virtual index finger tip over the 3D buttons floating in the space.
4.  **Press the Shoot Button** to launch rockets at the Golem target.
5.  **Press the Shield Button** to activate a protective shield if targets fire back or to test the barrier.
6.  Look at the floating scoreboard to track your score as you destroy targets!

---

## License
This project is licensed under the MIT License - see the LICENSE file for details.
