# ?? AUTO GMAIL SIGN-IN FIX - PREVENT AUTOMATIC LOGIN

## Problem

When you visit the game, it automatically signs you in with Gmail without showing a choice/login screen.

**Root Cause:**
1. Google OAuth stores session cookies in your browser
2. `setupAuthListener()` detects existing session on page load
3. Automatically triggers `SIGNED_IN` event
4. Unity receives login callbacks immediately

---

## Solution Options

### Option 1: Clear Session on Page Load (Recommended)

Force users to explicitly sign in each time.

**Add this to index.html:**

```javascript
// Add RIGHT AFTER initSupabase() function, BEFORE setupAuthListener()

// Clear any existing session on page load (forces explicit login)
async function clearExistingSession() {
    try {
 const { data: { session } } = await window.supabase.auth.getSession();
     
        if (session) {
 console.log('[Supabase] Clearing existing session to force manual login');
     await window.supabase.auth.signOut();
    }
    } catch (error) {
        console.error('[Supabase] Error clearing session:', error);
    }
}
```

**Then update `setupAuthListener()` to call it first:**

```javascript
// Auth State Change Listener (handles OAuth redirects)
async function setupAuthListener() {
    // Clear existing session first (forces manual login)
    await clearExistingSession();
    
    window.supabase.auth.onAuthStateChange(async (event, session) => {
        console.log('[Supabase] Auth state changed:', event);
        
   // ... rest of existing code ...
    });
}
```

---

### Option 2: Only Auto-Login After Redirect

Only process sessions that come from OAuth redirects, not existing sessions.

**Replace the setupAuthListener function:**

```javascript
// Auth State Change Listener (handles OAuth redirects ONLY)
function setupAuthListener() {
    // Flag to track if we're processing a redirect
    const isOAuthRedirect = window.location.search.includes('code=') || 
        window.location.hash.includes('access_token=');
    
    window.supabase.auth.onAuthStateChange(async (event, session) => {
     console.log('[Supabase] Auth state changed:', event);
        
      // ONLY process SIGNED_IN if it's from an OAuth redirect
        if (event === 'SIGNED_IN' && session && isOAuthRedirect) {
            const authResponse = {
    user: {
       id: session.user.id,
        email: session.user.email
 },
    session: {
 access_token: session.access_token,
          refresh_token: session.refresh_token
        }
            };
  
     // Check if this is a Google Sign-In
  if (session.user.app_metadata.provider === 'google') {
        console.log('[Supabase] Google Sign-In successful from redirect!');
         
       // Load or create player data
    try {
            const { data: playerData, error: playerError } = await window.supabase
   .from('player_data')
 .select('*')
            .eq('user_id', session.user.id)
         .single();
  
         if (playerError && playerError.code === 'PGRST116') {
     // Create new player data
           const username = session.user.email.split('@')[0];
          
            const { data: newPlayerData, error: createError } = await window.supabase
         .from('player_data')
        .insert({
       user_id: session.user.id,
  email: session.user.email,
       username: username,
          levels_unlocked: 5,
  current_money: 0,
      unlocked_cosmetics: '[]'
         })
    .select()
             .single();
          
        if (!createError && newPlayerData) {
               console.log('[Supabase] Player data created, waiting for Unity...');
 await waitForUnityInstance();
       SendMessageToUnity('PlayerDataManager', 'OnPlayerDataCreated', JSON.stringify(newPlayerData));
           }
           } else if (!playerError && playerData) {
 console.log('[Supabase] Player data loaded, waiting for Unity...');
  await waitForUnityInstance();
          SendMessageToUnity('PlayerDataManager', 'OnPlayerDataLoaded', JSON.stringify(playerData));
             }
  } catch (playerLoadError) {
         console.error('[Supabase] Error loading player data:', playerLoadError);
     }
           
         console.log('[Supabase] Waiting for Unity before sending auth success...');
          await waitForUnityInstance();
   SendMessageToUnity('AuthManager', 'OnGoogleSignInSuccess', JSON.stringify(authResponse));
           console.log('[Supabase] Google Sign-In callbacks sent to Unity!');
    } else {
         // Regular email/password login
      await waitForUnityInstance();
        SendMessageToUnity('AuthManager', 'OnLoginSuccess', JSON.stringify(authResponse));
         }
        } else if (event === 'SIGNED_IN' && session && !isOAuthRedirect) {
      // Existing session detected, but NOT from redirect - ignore it
            console.log('[Supabase] Existing session detected, but ignoring (not from OAuth redirect)');
        }
    });
}
```

---

### Option 3: Add "Remember Me" Checkbox

Let users choose whether to stay signed in.

**HTML (add to your login UI):**
```html
<input type="checkbox" id="rememberMe" checked>
<label for="rememberMe">Remember me</label>
```

**JavaScript (update Google Sign-In):**
```javascript
window.SupabaseGoogleSignIn = async function() {
    try {
        console.log('[Supabase] Starting Google Sign-In...');
        
   // Check if user wants to be remembered
        const rememberMe = document.getElementById('rememberMe')?.checked ?? true;
        
        const redirectUrl = 'http://localhost:8000/';
        console.log('[Supabase] Redirect URL:', redirectUrl);
        console.log('[Supabase] Remember me:', rememberMe);

   const { data, error } = await window.supabase.auth.signInWithOAuth({
            provider: 'google',
   options: {
     redirectTo: redirectUrl,
    // If remember me is false, use session storage instead of local storage
storageType: rememberMe ? 'local' : 'session'
         }
        });
   
        if (error) {
          console.error('[Supabase] Google Sign-In error:', error);
     SendMessageToUnity('AuthManager', 'OnGoogleSignInError', error.message);
   } else {
      console.log('[Supabase] Google Sign-In initiated - redirecting...');
 }
} catch (error) {
        console.error('[Supabase] Google Sign-In exception:', error);
        SendMessageToUnity('AuthManager', 'OnGoogleSignInError', error.message);
 }
};
```

---

## Quick Fix (Immediate Solution)

**For immediate testing, add this to the very start of `setupAuthListener()`:**

```javascript
function setupAuthListener() {
    // QUICK FIX: Sign out immediately on page load
    window.supabase.auth.signOut().then(() => {
        console.log('[Supabase] Cleared existing session');
    });
    
    window.supabase.auth.onAuthStateChange(async (event, session) => {
   // ... rest of existing code ...
    });
}
```

---

## Why This Happens

### Google OAuth Flow:
1. **First Sign-In:**
   - User clicks "Sign in with Google"
 - Redirects to Google ? back to your site
   - Supabase stores session in `localStorage`
   - Session includes refresh token (long-lived)

2. **Return Visit:**
   - Page loads ? Supabase checks `localStorage`
   - Finds existing session ? automatically restores it
   - Triggers `SIGNED_IN` event immediately
   - Unity receives login callbacks without user interaction

### Browser Storage:
```javascript
// These persist even after closing browser
localStorage.getItem('supabase.auth.token')
localStorage.getItem('supabase.auth.refreshToken')
```

---

## Implementation Guide

### Step 1: Choose Your Option

**Option 1** - Always require login (most secure)
**Option 2** - Only auto-login from OAuth redirects (recommended)
**Option 3** - Let users choose (best UX)

### Step 2: Update index.html

Find the `setupAuthListener()` function in `Assets/WebGLTemplates/SupabaseTemplate/index.html` (around line 107).

Replace it with code from your chosen option above.

### Step 3: Rebuild WebGL

1. File ? Build Settings
2. Build
3. Test in browser

### Step 4: Test

**Test Auto-Login Prevention:**
1. Sign in with Google
2. Close browser completely
3. Reopen and navigate to game
4. **Expected:** Should NOT auto-login, shows login screen

**Test OAuth Redirect:**
1. Click "Sign in with Google"
2. Complete Google auth
3. **Expected:** Should successfully sign in and load game

---

## Clear Existing Sessions (Manual)

If you want to test right now without rebuilding:

**In Browser Console (F12):**
```javascript
// Clear Supabase sessions
localStorage.removeItem('supabase.auth.token');
localStorage.removeItem('sb-bpjyqsfggliwehnqcbhy-auth-token');
localStorage.clear();

// Clear cookies
document.cookie.split(";").forEach(c => {
    document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
});

// Reload
location.reload();
```

---

## Additional: Revoke Google Access

If you want to completely disconnect your Google account:

1. Go to https://myaccount.google.com/permissions
2. Find your game/app
3. Click "Remove Access"
4. Next login will ask for permissions again

---

## Recommended Solution

**Use Option 2** (OAuth redirect detection) because:
- ? Prevents auto-login on page refresh
- ? Still allows successful Google OAuth
- ? Good balance of security and UX
- ? Doesn't require UI changes
- ? Works immediately

---

## Files to Modify

| File | Location | Change |
|------|----------|--------|
| `index.html` | `Assets/WebGLTemplates/SupabaseTemplate/` | Update `setupAuthListener()` |

---

## Summary

**Problem:** Google session persists, auto-logs you in  
**Cause:** `setupAuthListener()` processes all `SIGNED_IN` events  
**Solution:** Only process events from OAuth redirects, not existing sessions  
**Implementation:** Replace `setupAuthListener()` with Option 2 code  
**Result:** ? Manual login required, no auto-sign-in!

---

The fix is ready! Choose your option and I can help implement it. ??
