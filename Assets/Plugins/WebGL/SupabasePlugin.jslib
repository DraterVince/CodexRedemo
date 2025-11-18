mergeInto(LibraryManager.library, {
    // Helper function to convert C# string to JS string
    _ConvertString: function(ptr) {
        return UTF8ToString(ptr);
    },

    // =====================
    // PlayerPrefs Sync (CRITICAL FOR WEBGL SAVE PERSISTENCE)
    // =====================
    ForcePlayerPrefsSyncJS: function() {
        console.log("[SupabasePlugin] Forcing PlayerPrefs sync from IndexedDB...");
  
        // Unity WebGL stores PlayerPrefs in IndexedDB under "/idbfs"
        if (typeof Module !== 'undefined' && Module.FS && Module.FS.syncfs) {
    console.log("[SupabasePlugin] Using Module.FS.syncfs...");
            Module.FS.syncfs(true, function(err) {
                if (err) {
      console.error("[SupabasePlugin] IndexedDB sync error:", err);
          } else {
    console.log("[SupabasePlugin] ? IndexedDB sync complete!");
       }
    });
        } else {
         console.warn("[SupabasePlugin] FS.syncfs not available, forcing manual reload...");
      
   // Manual reload: Trigger Unity to re-read PlayerPrefs file
      // Unity stores PlayerPrefs in a file, we need to force it to reload
   if (typeof _UnityConfig !== 'undefined') {
  console.log("[SupabasePlugin] Attempting to trigger PlayerPrefs reload...");
      
    // Force PlayerPrefs.Save() to flush memory cache
                // This will cause Unity to re-read from IndexedDB on next access
          try {
          if (typeof FS !== 'undefined' && FS.syncfs) {
            FS.syncfs(true, function(err) {
     if (!err) {
           console.log("[SupabasePlugin] ? FS.syncfs completed");
       } else {
   console.error("[SupabasePlugin] FS.syncfs error:", err);
         }
    });
      } else {
          console.warn("[SupabasePlugin] FS.syncfs not found in global scope");
  }
   } catch (e) {
   console.error("[SupabasePlugin] Error during sync:", e);
                }
         }
     
            console.log("[SupabasePlugin] Manual reload triggered - Unity should re-read PlayerPrefs");
  }
    },

    // =====================
    // LocalStorage Helpers
    // =====================
    SetLocalStorage: function(keyPtr, valuePtr) {
     const key = UTF8ToString(keyPtr);
     const value = UTF8ToString(valuePtr);
        window.SetLocalStorage(key, value);
    },

    GetLocalStorage: function(keyPtr) {
    const key = UTF8ToString(keyPtr);
    const value = window.GetLocalStorage(key);
        const bufferSize = lengthBytesUTF8(value) + 1;
        const buffer = _malloc(bufferSize);
        stringToUTF8(value, buffer, bufferSize);
        return buffer;
    },

    // =====================
    // Authentication Functions
    // =====================
    SupabaseRegister: function(emailPtr, passwordPtr) {
        const email = UTF8ToString(emailPtr);
        const password = UTF8ToString(passwordPtr);
        window.SupabaseRegister(email, password);
    },

    SupabaseLogin: function(emailPtr, passwordPtr) {
 const email = UTF8ToString(emailPtr);
     const password = UTF8ToString(passwordPtr);
        window.SupabaseLogin(email, password);
    },

    SupabaseLogout: function() {
  window.SupabaseLogout();
},
    
    // NEW: Google Sign-In
    SupabaseGoogleSignIn: function() {
  window.SupabaseGoogleSignIn();
    },

    // =====================
    // Player Data Functions
    // =====================
  createPlayerData: function(userIdPtr, emailPtr, usernamePtr) {
        const userId = UTF8ToString(userIdPtr);
        const email = UTF8ToString(emailPtr);
        const username = UTF8ToString(usernamePtr);
        window.createPlayerData(userId, email, username);
    },

  loadPlayerData: function(userIdPtr) {
        const userId = UTF8ToString(userIdPtr);
    window.loadPlayerData(userId);
    },

    updatePlayerData: function(userIdPtr, levelsUnlocked, currentMoney, unlockedCosmeticsPtr) {
   const userId = UTF8ToString(userIdPtr);
      const unlockedCosmetics = UTF8ToString(unlockedCosmeticsPtr);
        window.updatePlayerData(userId, levelsUnlocked, currentMoney, unlockedCosmetics);
    },

    getCurrentUser: function() {
   window.getCurrentUser();
    },

    // =====================
    // Game Save Functions
    // =====================
    LoadAllSaveSlotsJS: function() {
    window.LoadAllSaveSlotsJS();
    },

    SaveSlotToSupabaseJS: function(slotNumber, usernamePtr, levelsUnlocked, currentMoney, unlockedCosmeticsPtr) {
     const username = UTF8ToString(usernamePtr);
        const unlockedCosmetics = UTF8ToString(unlockedCosmeticsPtr);
    window.SaveSlotToSupabaseJS(slotNumber, username, levelsUnlocked, currentMoney, unlockedCosmetics);
    },

    DeleteSlotFromSupabaseJS: function(slotNumber) {
        window.DeleteSlotFromSupabaseJS(slotNumber);
  },
    
    // NEW: Check if save data is ready
    HasPendingSaveData: function() {
        if (typeof window.HasPendingSaveData === 'function') {
            return window.HasPendingSaveData() ? 1 : 0;
        }
     return 0;
    },
    
  // NEW: Get the pending save data
    GetPendingSaveData: function() {
        if (typeof window.GetPendingSaveData === 'function') {
            const data = window.GetPendingSaveData();
         const bufferSize = lengthBytesUTF8(data) + 1;
            const buffer = _malloc(bufferSize);
            stringToUTF8(data, buffer, bufferSize);
            return buffer;
        }
     // Return empty array if function doesn't exist
const emptyData = '[]';
    const bufferSize = lengthBytesUTF8(emptyData) + 1;
        const buffer = _malloc(bufferSize);
  stringToUTF8(emptyData, buffer, bufferSize);
        return buffer;
    }
});
