# Audio Volume Control Fix Plan

## Current Issues

### Issue #1: Master Volume Implementation
**Current:** Uses `AudioListener.volume = MasterVolume` (line 251)
**Problem:** AudioListener.volume affects ALL audio globally, but music/SFX volumes don't multiply by it
**Result:** Master slider changes global volume, but music/SFX sliders set absolute volumes (not relative to master)

### Issue #2: Volume Calculation
**Current:** `finalVolume = originalVolume * volumeSetting` (line 328)
**Problem:** Only multiplies by music OR SFX volume, NOT by master volume
**Result:** Master volume and music/SFX volumes compete instead of combining

### Issue #3: MusicManager Not Registering
**Possible Issue:** MusicManager's AudioSource might not be properly registered as a music source
**Result:** Music slider doesn't affect the background music

## Correct Volume System Design

### Proper Volume Hierarchy:
```
Final Volume = Original Volume × Category Volume × Master Volume
```

Where:
- **Original Volume**: Set in Unity Inspector (default 1.0)
- **Category Volume**: Music slider (for music) or SFX slider (for SFX)
- **Master Volume**: Master slider (affects everything)

### Example:
- Original Volume: 0.8 (set in prefab)
- Music Volume Slider: 0.7
- Master Volume Slider: 0.5
- **Final Volume**: 0.8 × 0.7 × 0.5 = 0.28

## The Fix

### Step 1: Remove AudioListener.volume Usage
AudioListener.volume is a global setting that affects everything. We should NOT use it because it doesn't allow individual control.

### Step 2: Update Volume Calculation
Change line 328 from:
```csharp
float finalVolume = volumeHelper.originalVolume * volumeSetting;
```

To:
```csharp
float finalVolume = volumeHelper.originalVolume * volumeSetting * MasterVolume;
```

### Step 3: Apply to Both Music and SFX
Both `UpdateRegisteredAudioSources` calls should multiply by master volume:
- Music sources: `originalVolume × MusicVolume × MasterVolume`
- SFX sources: `originalVolume × SFXVolume × MasterVolume`

### Step 4: Ensure MusicManager is Registered
Verify that MusicManager's AudioSource has the `MusicAudioSource` component attached.

## Implementation

The fix requires changing:
1. `ApplyVolumeSettings()` method - remove AudioListener.volume line
2. `UpdateRegisteredAudioSources()` method - add master volume multiplication
3. Verify MusicManager has proper registration

This ensures:
✅ Master slider affects ALL audio
✅ Music slider affects only music (relative to master)
✅ SFX slider affects only SFX (relative to master)
✅ All three sliders work together multiplicatively
