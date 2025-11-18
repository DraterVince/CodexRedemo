using System.Collections.Generic;
using Supabase.Gotrue;

internal class SignupOptions : SignUpOptions
{
    public new Dictionary<string, object> Data { get; set; }
}