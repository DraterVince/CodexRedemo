# Google OAuth Tester for Itch.io
# Tests if your Google OAuth redirect URI is properly configured

import webbrowser
import urllib.parse
from http.server import HTTPServer, BaseHTTPRequestHandler
import threading
import time

# Your configuration
CLIENT_ID = "603686578977-6iu8nbbqhjlh2e65rv5s9npfbvdftfr0.apps.googleusercontent.com"
REDIRECT_URI = "http://localhost:8000/callback"
SUPABASE_URL = "https://bpjyqsfggliwehnqcbhy.supabase.co"

# Test results
test_results = {
    "redirect_received": False,
    "auth_code": None,
    "error": None,
    "error_description": None
}

class OAuthCallbackHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        """Handle the OAuth callback"""
        global test_results
   
# Parse the URL and query parameters
        if self.path.startswith('/callback'):
 query_string = self.path.split('?')[1] if '?' in self.path else ''
        params = urllib.parse.parse_qs(query_string)
            
 test_results["redirect_received"] = True
       
  # Check for authorization code
     if 'code' in params:
              test_results["auth_code"] = params['code'][0]
       self.send_success_page()
    # Check for error
    elif 'error' in params:
         test_results["error"] = params['error'][0]
        test_results["error_description"] = params.get('error_description', ['Unknown error'])[0]
                self.send_error_page()
else:
      self.send_response(200)
       self.send_header('Content-type', 'text/html')
    self.end_headers()
          self.wfile.write(b"<h1>Unexpected Response</h1>")
        else:
      self.send_response(404)
   self.end_headers()
    
    def send_success_page(self):
        """Send success page"""
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
      self.end_headers()
        
     html = """
        <!DOCTYPE html>
  <html>
        <head>
     <title>OAuth Test - Success!</title>
    <style>
                body {
            font-family: Arial, sans-serif;
       max-width: 600px;
        margin: 50px auto;
 padding: 20px;
          background: #f0f0f0;
      }
 .success {
     background: #4CAF50;
    color: white;
       padding: 20px;
          border-radius: 5px;
                    text-align: center;
       }
      .info {
           background: white;
        padding: 20px;
  margin-top: 20px;
                border-radius: 5px;
        border-left: 4px solid #4CAF50;
       }
           pre {
     background: #f5f5f5;
        padding: 10px;
   overflow-x: auto;
         border-radius: 3px;
   }
    </style>
        </head>
   <body>
            <div class="success">
 <h1>? OAuth Test Successful!</h1>
        <p>Google OAuth is properly configured</p>
  </div>
 <div class="info">
          <h2>What This Means:</h2>
                <ul>
                    <li>? Google Cloud Console redirect URI is correct</li>
         <li>? Authorization flow completed successfully</li>
       <li>? Google is not returning 403 errors</li>
        <li>? Your itch.io URL should work!</li>
     </ul>
     <p><strong>Next Steps:</strong></p>
                <ol>
                  <li>Wait 5-10 minutes for Google changes to propagate</li>
        <li>Clear browser cache on itch.io</li>
          <li>Test Google Sign-In on your game</li>
     </ol>
          <p>You can close this window and check the terminal for details.</p>
        </div>
        </body>
     </html>
      """
        self.wfile.write(html.encode())
    
    def send_error_page(self):
        """Send error page"""
        self.send_response(200)
   self.send_header('Content-type', 'text/html')
        self.end_headers()
        
     error_msg = test_results.get("error_description", "Unknown error")
        
     html = f"""
    <!DOCTYPE html>
        <html>
        <head>
      <title>OAuth Test - Error</title>
  <style>
         body {{
         font-family: Arial, sans-serif;
         max-width: 600px;
        margin: 50px auto;
         padding: 20px;
     background: #f0f0f0;
   }}
         .error {{
                    background: #f44336;
       color: white;
   padding: 20px;
border-radius: 5px;
         text-align: center;
}}
         .info {{
            background: white;
  padding: 20px;
       margin-top: 20px;
              border-radius: 5px;
                  border-left: 4px solid #f44336;
    }}
   pre {{
   background: #f5f5f5;
       padding: 10px;
   overflow-x: auto;
               border-radius: 3px;
      }}
                code {{
     background: #f5f5f5;
    padding: 2px 5px;
   border-radius: 3px;
    }}
            </style>
        </head>
        <body>
            <div class="error">
      <h1>? OAuth Test Failed</h1>
   <p>Google returned an error</p>
            </div>
     <div class="info">
                <h2>Error Details:</h2>
             <pre>{error_msg}</pre>
          
     <h2>Common Solutions:</h2>
                <ul>
         <li><strong>If error contains "redirect_uri_mismatch":</strong>
      <br>The redirect URI in Google Cloud Console doesn't match.
               <br>Add <code>http://localhost:8000/callback</code> to authorized URIs.
   </li>
            <li><strong>If error is "access_denied":</strong>
   <br>You cancelled the authorization. This is normal for testing.
      </li>
  <li><strong>If error is "invalid_client":</strong>
      <br>Client ID is incorrect or OAuth consent screen not configured.
        </li>
 </ul>
  
                <p>Check the terminal for more details. You can close this window.</p>
  </div>
        </body>
        </html>
        """
        self.wfile.write(html.encode())
    
    def log_message(self, format, *args):
        """Suppress default logging"""
      pass

def start_oauth_server():
    """Start local server to receive OAuth callback"""
    server = HTTPServer(('localhost', 8000), OAuthCallbackHandler)
  server_thread = threading.Thread(target=server.serve_forever)
    server_thread.daemon = True
    server_thread.start()
    print("? Local OAuth callback server started on http://localhost:8000")
    return server

def build_oauth_url():
    """Build the Google OAuth URL"""
    params = {
        'client_id': CLIENT_ID,
      'redirect_uri': REDIRECT_URI,
        'response_type': 'code',
        'scope': 'email profile',
        'access_type': 'offline',
        'prompt': 'select_account'
    }
    
    query_string = urllib.parse.urlencode(params)
return f"https://accounts.google.com/o/oauth2/v2/auth?{query_string}"

def print_header():
    """Print test header"""
    print("\n" + "="*70)
    print("?? GOOGLE OAUTH CONFIGURATION TESTER")
    print("="*70)
    print()
    print("This script tests if your Google OAuth is properly configured.")
    print("It simulates what happens when a user clicks 'Sign in with Google'.")
    print()
    print("Configuration:")
    print(f"  Client ID: {CLIENT_ID[:50]}...")
    print(f"  Redirect URI: {REDIRECT_URI}")
    print()

def print_instructions():
    """Print test instructions"""
    print("?? INSTRUCTIONS:")
    print()
    print("1. A browser window will open with Google Sign-In")
    print("2. Sign in with your Google account")
    print("3. Grant permissions to the app")
  print("4. You'll be redirected back to this script")
    print("5. Results will be displayed below")
    print()
    print("??  If you get a 403 error, it means:")
    print("   - The redirect URI is not in Google Cloud Console")
    print("   - Or Google hasn't propagated the changes yet (wait 5-10 min)")
    print()
    input("Press ENTER to start the test...")

def print_results():
    """Print test results"""
    print("\n" + "="*70)
    print("?? TEST RESULTS")
    print("="*70)
    print()
    
    if test_results["redirect_received"]:
        if test_results["auth_code"]:
            print("? SUCCESS - OAuth is properly configured!")
            print()
     print("Details:")
  print(f"  ? Redirect received")
   print(f"  ? Authorization code obtained")
 print(f"  ? No 403 errors")
print()
    print("What this means:")
       print("  • Google Cloud Console redirect URI is correct")
    print("  • Authorization flow works")
  print("  • Your itch.io game should work after propagation")
 print()
          print("Next steps:")
            print("  1. Wait 5-10 minutes for Google to propagate changes")
            print("  2. Clear browser cache on itch.io")
            print("  3. Test Google Sign-In on your actual game")
          
 elif test_results["error"]:
            print("? FAILED - Google returned an error")
          print()
  print("Error Details:")
  print(f"  Error: {test_results['error']}")
   print(f"  Description: {test_results['error_description']}")
 print()
            
            if "redirect_uri_mismatch" in test_results.get("error", ""):
    print("?? Fix:")
      print("  1. Go to Google Cloud Console")
                print("  2. Find your OAuth Client")
          print("  3. Add this exact URI to Authorized redirect URIs:")
           print(f"   {REDIRECT_URI}")
    print("  4. Save and wait 5-10 minutes")
            elif test_results["error"] == "access_denied":
  print("??  You cancelled the authorization.")
          print("   This is normal for testing. Try again if needed.")
    else:
        print("? TIMEOUT - No redirect received")
      print()
        print("Possible reasons:")
        print("  • You closed the browser without completing")
 print("  • Google is blocking the request (403 error)")
        print("  • Network issue")
  print()
        print("Check the browser for error messages.")
    
    print()
    print("="*70)

def main():
    """Main test function"""
    print_header()
    print_instructions()
    
    # Start local server
    server = start_oauth_server()
    
    # Build OAuth URL
    oauth_url = build_oauth_url()
    
    print("\n?? Opening browser...")
    print(f"   URL: {oauth_url[:80]}...")
    print()
    
    # Open browser
    webbrowser.open(oauth_url)
    
    # Wait for callback (60 seconds timeout)
    print("? Waiting for OAuth callback...")
    print("   (You have 60 seconds to complete the flow)")
    print()
    
    timeout = 60
    start_time = time.time()
    
    while time.time() - start_time < timeout:
        if test_results["redirect_received"]:
  time.sleep(1)  # Give server time to send response
          break
     time.sleep(0.5)
 
    # Print results
    print_results()
    
 # Stop server
    server.shutdown()
    print("\n? Server stopped. Test complete.")
    print()

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n??  Test cancelled by user")
 except Exception as e:
    print(f"\n\n? Error: {e}")

