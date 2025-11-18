if (window.opener && window.opener !== window) {
    
    const urlParams = new URLSearchParams(window.location.search);
    const hashParams = new URLSearchParams(window.location.hash.substring(1));
    
    const hasCode = urlParams.has('code');
    const hasAccessToken = hashParams.has('access_token');
    
    if (hasCode || hasAccessToken) {
        setTimeout(() => {
            window.close();
        }, 1000);
        throw new Error('OAuth popup - stopping page load');
    }
}

if (typeof window.SupabaseGoogleSignIn !== 'undefined') {
    window.OriginalSupabaseGoogleSignIn = window.SupabaseGoogleSignIn;
}
window.SupabaseGoogleSignIn = async function() {
    try {
  if (!window.supabase) {
SendMessageToUnity('AuthManager', 'OnGoogleSignInError', 'Supabase not initialized');
      return;
 }

        const redirectUrl = window.location.href;
        const { data, error } = await window.supabase.auth.signInWithOAuth({
            provider: 'google',
            options: {
       redirectTo: redirectUrl,
                skipBrowserRedirect: true
  }
        });

        if (error) {
   SendMessageToUnity('AuthManager', 'OnGoogleSignInError', error.message);
        return;
        }

        if (!data || !data.url) {
    SendMessageToUnity('AuthManager', 'OnGoogleSignInError', 'No OAuth URL');
  return;
        }

        
        const width = 500;
        const height = 600;
        const left = (screen.width / 2) - (width / 2);
const top = (screen.height / 2) - (height / 2);
        const popup = window.open(
   data.url,
        'GoogleSignIn',
            `width=${width},height=${height},left=${left},top=${top},toolbar=no,menubar=no,scrollbars=yes`
        );

      if (!popup || popup.closed || typeof popup.closed === 'undefined') {
          SendMessageToUnity('AuthManager', 'OnGoogleSignInError', 'Popup blocked - please allow popups');
            return;
        }

        let authSuccessful = false;
  let checkCount = 0;
        const maxChecks = 60;
      
   const authCheckInterval = setInterval(async () => {
  checkCount++;
  if (popup.closed) {
                clearInterval(authCheckInterval);
          if (!authSuccessful) {
  setTimeout(async () => {
             const { data: { session } } = await window.supabase.auth.getSession();
             if (session) {
           await handleGoogleAuthSuccess(session);
                 } else {
                SendMessageToUnity('AuthManager', 'OnGoogleSignInError', 'Sign-in cancelled');
             }
        }, 500);
 }
       return;
        }
     try {
           const { data: { session }, error: sessionError } = await window.supabase.auth.getSession();
     if (session && session.user) {
   authSuccessful = true;
  clearInterval(authCheckInterval);
        try {
            if (popup && !popup.closed) {
   popup.close();
          }
 } catch (e) {}
         await handleGoogleAuthSuccess(session);
            return;
        }
            } catch (e) {}
          if (checkCount >= maxChecks) {
    clearInterval(authCheckInterval);
     if (!authSuccessful) {
     try { popup.close(); } catch (e) {}
       SendMessageToUnity('AuthManager', 'OnGoogleSignInError', 'Sign-in timeout');
         }
            }
      }, 500);

    } catch (error) {
        SendMessageToUnity('AuthManager', 'OnGoogleSignInError', error.message || 'Unknown error');
    }
};

async function handleGoogleAuthSuccess(session) {

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

    try {
        const playerDataPromise = window.supabase
    .from('player_data')
            .select('*')
         .eq('user_id', session.user.id)
     .single();
      const timeoutPromise = new Promise(resolve => setTimeout(() => resolve(null), 2000));
 
        const result = await Promise.race([playerDataPromise, timeoutPromise]);
        
 if (result && result.data) {
   await waitForUnityInstance(3000);
            SendMessageToUnity('PlayerDataManager', 'OnPlayerDataLoaded', JSON.stringify(result.data));
        }
    } catch (error) {}

    await waitForUnityInstance(3000);
    SendMessageToUnity('AuthManager', 'OnGoogleSignInSuccess', JSON.stringify(authResponse));
}

async function waitForUnityInstance(maxWaitMs = 5000) {
    const startTime = Date.now();
    while (!window.unityInstance && (Date.now() - startTime) < maxWaitMs) {
        await new Promise(resolve => setTimeout(resolve, 50));
    }
}

function SendMessageToUnity(gameObject, method, message) {
    try {
        if (typeof unityInstance !== 'undefined' && unityInstance !== null) {
            unityInstance.SendMessage(gameObject, method, message || '');
        }
    } catch (e) {}
}
