# Camera Controller Setup Guide

## Quick Setup Instructions

### 1. Add the Script to Your Camera
1. Select your Main Camera in the scene
2. Add the `CameraController` component to it
3. The script will automatically find or create a center point for orbiting

### 2. Configure Settings (Optional)
The script comes with sensible defaults, but you can adjust these in the Inspector:

**Orbit Settings:**
- `Target`: Leave empty to auto-detect center, or drag a GameObject to orbit around
- `Base Distance`: Starting zoom distance (auto-calculated from current camera position)
- `Horizontal Speed`: How fast camera rotates left/right (default: 100)
- `Vertical Speed`: How fast camera rotates up/down (default: 80)

**Zoom Settings:**
- `Zoom Speed`: How fast zoom responds (default: 2)
- `Min Zoom Factor`: Closest zoom level (default: 0.5x)
- `Max Zoom Factor`: Farthest zoom level (default: 2x)

**Limits:**
- `Min/Max Vertical Angle`: Prevents camera from flipping upside down

**Input Keys:**
- `Zoom In Key`: Default Q
- `Zoom Out Key`: Default E

## Controls

### Rotation
- **W**: Rotate camera up
- **A**: Rotate camera left  
- **S**: Rotate camera down
- **D**: Rotate camera right

### Zoom
- **Mouse Scroll Wheel**: Zoom in/out
- **Q**: Zoom in
- **E**: Zoom out

## Features

### Automatic Center Detection
The script will try to find your game center by looking for objects named:
- "GameCenter"
- "Center" 
- "Map"

If none are found, it creates a "CameraTarget" at world origin (0,0,0).

### Game Integration
- Respects game pause state from GameManager
- Smooth interpolated movement
- Configurable speed and limits
- Debug visualization when camera is selected

### Public Methods
You can control the camera from other scripts:
```csharp
CameraController camController = Camera.main.GetComponent<CameraController>();

// Set new orbit target
camController.SetTarget(someTransform);

// Set zoom level programmatically
camController.SetZoom(1.5f); // 1.5x zoom

// Reset to default position
camController.ResetCamera();

// Focus on a specific position
camController.FocusOnPosition(Vector3.zero);
```

## Troubleshooting

**Camera doesn't move:**
- Make sure the script is attached to a GameObject with a Camera component
- Check that the game isn't paused

**Camera orbits around wrong point:**
- Manually assign the Target field in the inspector
- Or move the auto-created "CameraTarget" object to your desired center

**Movement too fast/slow:**
- Adjust Horizontal Speed and Vertical Speed values
- Adjust Movement Smoothing for more/less responsiveness

**Zoom range not right:**
- Adjust Min/Max Zoom Factor values
- The Base Distance is calculated from your camera's starting position
