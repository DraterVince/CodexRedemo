# ?? GOOGLE OAUTH 403 ERROR - ITCH.IO URL FIX

## Problem Fixed

**403 Forbidden Error:**
```
GET https://accounts.google.com/...redirect_to=http://localhost:8000/ 403 (Forbidden)
```

**Root Cause:** 
- Code was using `http://localhost:8000/` as redirect URL
- Your game is actually hosted on `https://vinceisme.itch.io/codex`
- Google OAuth rejects mismatched URLs with 403 error

---

## ? Changes Applied

### File Modified:
`Assets/WebGLTemplates/SupabaseTemplate/index.html`

**Before:**
```javascript
const redirectUrl = 'http://localhost:8000/';
```

**After:**
```javascript
const redirectUrl = 'https://vinceisme.itch.io/codex';
```

---

## ?? Required: Update Google Cloud Console

You **MUST** also add this URL to your Google OAuth configuration:

### Step 1: Open Google Cloud Console

1. Go to: https://console.cloud.google.com/
2. Select your project (the one with your OAuth Client ID)
3. Navigate to: **APIs & Services** ? **Credentials**

### Step 2: Find Your OAuth Client

1. Look for **OAuth 2.0 Client IDs**
2. Find the client ID: `603686578977-6iu8nbbqhjlh2e65rv5s9npfbvdftfr0.apps.googleusercontent.com`
3. Click on it to edit

### Step 3: Add Authorized Redirect URI

**In the "Authorized redirect URIs" section, add:**

```
https://vinceisme.itch.io/codex
```

**Also make sure these are present:**
```
https://bpjyqsfggliwehnqcbhy.supabase.co/auth/v1/callback
http://localhost:8000/
```

**Your final list should include:**
- `https://bpjyqsfggliwehnqcbhy.supabase.co/auth/v1/callback` (Supabase callback)
- `https://vinceisme.itch.io/codex` (Your itch.io game)
- `http://localhost:8000/` (For local testing)

### Step 4: Save Changes

Click **"Save"** at the bottom of the page.

**?? Important:** Changes can take 5-10 minutes to propagate!

---

## ?? Update Supabase Site URL

You should also update your Supabase project settings:

### Step 1: Open Supabase Dashboard

1. Go to: https://supabase.com/dashboard
2. Select your project: `bpjyqsfggliwehnqcbhy`
3. Go to: **Authentication** ? **URL Configuration**

### Step 2: Update Site URL

**Set Site URL to:**
```
https://vinceisme.itch.io/codex
```

### Step 3: Add Redirect URLs

**In "Redirect URLs" section, add:**
```
https://vinceisme.itch.io/codex
http://localhost:8000/
```

### Step 4: Save Settings

Click **"Save"** at the bottom.

---

## ?? Testing Instructions

### Step 1: Rebuild WebGL

1. **File ? Build Settings**
2. **Select WebGL**
3. **Build** (to apply index.html changes)

### Step 2: Upload to Itch.io

1. Upload new build to itch.io
2. Wait for processing to complete

### Step 3: Clear Browser Cache

Before testing, clear cache:

**In browser console (F12):**
```javascript
localStorage.clear();
sessionStorage.clear();
location.reload();
```

### Step 4: Test Google Sign-In

1. Open game on itch.io: https://vinceisme.itch.io/codex
2. Click "Sign in with Google"
3. **Expected:** Google popup appears (no 403 error)
4. Complete authentication
5. **Expected:** Successfully redirected back to game
6. **Expected:** Game loads with your account

---

## ?? Verification Checklist

After making all changes, verify:

### Google Cloud Console:
- [ ] OAuth Client found
- [ ] `https://vinceisme.itch.io/codex` added to Authorized redirect URIs
- [ ] `https://bpjyqsfggliwehnqcbhy.supabase.co/auth/v1/callback` present
- [ ] Changes saved
- [ ] Waited 5-10 minutes for propagation

### Supabase Dashboard:
- [ ] Site URL set to `https://vinceisme.itch.io/codex`
- [ ] Redirect URLs include itch.io link
- [ ] Settings saved

### Code Changes:
- [ ] `index.html` updated to use itch.io URL
- [ ] WebGL rebuilt
- [ ] New build uploaded to itch.io

### Testing:
- [ ] Browser cache cleared
- [ ] Visited game on itch.io
- [ ] Clicked "Sign in with Google"
- [ ] **No 403 error** in console
- [ ] Google OAuth popup appeared
- [ ] Successfully authenticated
- [ ] Redirected back to game
- [ ] Game loaded correctly

---

## ?? Troubleshooting

### Still Getting 403 Error

**Possible Causes:**

1. **Google Cloud Console changes not propagated yet**
   - Wait 5-10 minutes
   - Try incognito window
   - Clear browser cache completely

2. **Wrong OAuth Client ID**
   - Verify you edited the correct client in Google Cloud Console
- Check that client ID matches: `603686578977-6iu8nbbqhjlh2e65rv5s9npfbvdftfr0`

3. **Old build cached**
   - Hard refresh on itch.io (Ctrl+F5)
   - Clear itch.io cache
   - Try different browser

4. **URL mismatch**
   - Verify URL in code exactly matches: `https://vinceisme.itch.io/codex`
   - No trailing slash differences
   - Exact case match

### Console Errors to Look For

**Before Fix:**
```
GET https://accounts.google.com/...redirect_to=http://localhost:8000/ 403 (Forbidden)
```

**After Fix:**
```
[Supabase] Starting Google Sign-In...
[Supabase] Redirect URL: https://vinceisme.itch.io/codex
[Supabase] Google Sign-In initiated - redirecting...
[Supabase] Auth state changed: SIGNED_IN
[Supabase] Google Sign-In successful!
```

---

## ?? For Local Testing

If you want to test locally (http://localhost:8000/), you have two options:

### Option 1: Dynamic URL Detection

Update the code to auto-detect environment:

```javascript
// Google Sign-In Function
window.SupabaseGoogleSignIn = async function() {
    try {
        console.log('[Supabase] Starting Google Sign-In...');
        
        // Auto-detect URL (production vs local)
        const isLocal = window.location.hostname === 'localhost' || 
        window.location.hostname === '127.0.0.1';
        const redirectUrl = isLocal 
 ? 'http://localhost:8000/'
   : 'https://vinceisme.itch.io/codex';
        
   console.log('[Supabase] Redirect URL:', redirectUrl);
        console.log('[Supabase] Environment:', isLocal ? 'Local' : 'Production');

        const { data, error } = await window.supabase.auth.signInWithOAuth({
   provider: 'google',
            options: {
     redirectTo: redirectUrl
         }
    });
        
        // ... rest of code ...
    } catch (error) {
        // ... error handling ...
    }
};
```

### Option 2: Manual Switch

Comment/uncomment based on where you're testing:

```javascript
// For PRODUCTION (itch.io):
const redirectUrl = 'https://vinceisme.itch.io/codex';

// For LOCAL testing (comment out production, uncomment this):
// const redirectUrl = 'http://localhost:8000/';
```

---

## ?? Summary

### What Was Changed:

| Item | Before | After |
|------|--------|-------|
| **Redirect URL** | `http://localhost:8000/` | `https://vinceisme.itch.io/codex` |
| **File Modified** | `index.html` | ? Updated |
| **Google Cloud Console** | ?? Needs manual update | Add itch.io URL |
| **Supabase Dashboard** | ?? Needs manual update | Add itch.io URL |

### Next Steps:

1. ? **Code updated** (done automatically)
2. ? **Update Google Cloud Console** (manual - see instructions above)
3. ? **Update Supabase Dashboard** (manual - see instructions above)
4. ? **Rebuild WebGL** (File ? Build Settings ? Build)
5. ? **Upload to itch.io**
6. ? **Test Google Sign-In**

### Expected Result:

- ? No more 403 errors
- ? Google OAuth works on itch.io
- ? Successful sign-in and redirect
- ? Game loads with authenticated user

---

## ?? Critical Action Required

**You MUST update Google Cloud Console** for this fix to work!

Without adding `https://vinceisme.itch.io/codex` to your OAuth authorized redirect URIs, Google will continue to block the request with a 403 error.

**Timeline:**
- Code changes: ? Done (instant)
- Google Cloud Console: ? 5-10 minutes after you update
- Supabase Dashboard: ? Instant after you update
- Testing: ? After all above complete

---

The code is fixed! Now follow the Google Cloud Console instructions above to complete the fix. ??
