# CalbucciLib.SmartCookie

SmartCookie is the best thing since sliced bread (if sliced bread was a library to manage HTTP cookies). It wraps all your cookies into a incredibly easy to use POCO-like class so you can set & get the values of cookies just as a setting or getting a property of a class. Wait, you are actually just setting or getting a property of a class:

```csharp
myCookie.DateFirstVisit = DateTime.UtcNow;
...
var currentUserId = myCookie.UserId;
```

## Basic Implementation
The minimum you need to do to make usage of SmartCookie is to create a class to manage your cookies, with whatever properties you want, that inherits from SmartCookie and use the GetValue/SetValue for the properties, as:

```csharp
public class MySiteCookie : SmartCookie
{
    public MySiteCookie(HttpContextBase context)
        : base(context)
    { }

    public string Property1 { 
        get { return GetValue(); }
        set { SetValue(value); }
    }
}
```

What this class does is to save a cookie of name "Property1" using the default parameters. On your code, you can use it like:

```csharp
...
var myCookie = new MySiteCookie(Context);
...
// Reads the cookie value from the Request (or Response)
var cookiePropertyValue = myCookie.Property1;
...
// Set the HTTP Response cookie
myCookie.Property1 = "NewValue";
```

## Features & Configuration
- All properties of a class that inherit from SmartCookie are automatically serialized.
- You can add **```[SmartCookieSkip]```** to prevent a property from being saved to a cookie (think [JsonIgnore])
- You can add **```[SmartCookieSettings]```** to have fine control over the properties of the cookies (expiration, name, etc). See more below
- The properties getter & setter must call **```GetValueXXX()```** and **```SetValue()```**.
- Your properties can be of 6 different types: string, int, long, double, dateTime? and bool.
- The SetValue takes any one of the 6 types. GetValue has unique names (GetValueInt, GetValueLong, etc.)
- By default, types int, long & double return 0 if the cookie is not set.
- By default bool types return false if the cookie is not set.
- Multi-value cookies are supported inherting from the **```SmartCookieMultiValue```** (see below)
- If you don't set it, the cookies will expire in 5 years (you can use DefaultExpires for a different period or the ExpiresInDays in the SmartCookieSettings attribute).
- You can set the default domain for your cookie using DefaultDomain property

## Adding Multi-Value Cookies
You can have a two-level structure cookie by using multi-values cookies.

You can access the data like this:

```csharp
var entryPoint = Request.RawUrl;
var visitDate = DateTime.UtcNow;
var myCookie = new MyCookie(HttpContext); // From the Controller

if(Session.IsNewSession)
{
    if(myCookie.FirstVisit.EntryPointUrl == null)
    {
        myCookie.FirstVisit.EntryPointUrl = entryPoint;
        myCookie.FirstVisit.DaveVisit = visitDate;
    }

    myCookie.LastVisit.EntryPointUrl = entryPoint;
    myCookie.LastVisit.DateVisit = visitDate;
}

```

For that to work, you create your Cookies class like these:

```csharp
public class MyCookie : SmartCookie
{
    // ... constructor

    public VisitInfoCookie FirstVisit
	{
		get { return GetMultiValue<VisitInfoCookie>(); }
	}

	public VisitInfoCookie LastVisit
	{
		get { return GetMultiValue<VisitInfoCookie>(); }
	}
}

public class VisitInfoCookie : SmartCookieMultiValue
{
    public string EntryPointUrl { 
        get { return GetValue(); }
        set { SetValue(value); }
    }

    public DateTime? DateVisit
    {
        get { return GetValueDateTime(); }
        set { SetValue(value); }
    }
}

```



## Fine Tuning Your Cookies (i.e. [SmartCookieSettings])
By setting the attribute SmartCookieSettings to a property of your cookie class, you can define all the properties that you expect to have access through HttpCookie.

```csharp
public class MyCookie : SmartCookie
{
    // ... constructor

    [SmartCookieSettings("ab", 30, Domain = "domain.com", HttpOnly = true, MaxLength = 10, Path = "/a/", Secure = true)]
    public string ABTestGroup { 
        get { return GetValue(); }
        set { SetValue(value); }
    }
}
```

A few notes on SmartCookieSettings:
- It's recommended to always set the name of the cookie to something short.
- The values present on SmartCookieSettings override the DefaultDomain, DefaultExpires, DefaultSecure properties of SmartCookie.
- In addition to the standard HttpCookie properties, you can also set MaxLength which will automatically truncate the cookie value for you.
- SmartCookieSettings can be use on Multi-Value cookies, but only the Name and MaxLegnth property are observed.

Here is a better implementation of the VisitInfo cookie pattern:

```csharp
public class MyCookie : SmartCookie
{
    // ... constructor

    [SmartCookieSettings("fv")]
    public VisitInfoCookie FirstVisit
	{
		get { return GetMultiValue<VisitInfoCookie>(); }
	}

    [SmartCookieSettings("lv")]
	public VisitInfoCookie LastVisit
	{
		get { return GetMultiValue<VisitInfoCookie>(); }
	}
}

public class VisitInfoCookie : SmartCookieMultiValue
{
    [SmartCookieSettings("e", MaxLength=128)]
    public string EntryPointUrl { 
        get { return GetValue(); }
        set { SetValue(value); }
    }

    [SmartCookieSettings("d")]
    public DateTime? DateVisit
    {
        get { return GetValueDateTime(); }
        set { SetValue(value); }
    }
}

```

The result will be two cookies, one named "fv" and another "lv", each of which with a value for "e" (entry pont url) and for "d" (date of the visit).


## WARNING
NEVER TRUST THE CONTENT OF A COOKIE FOR SECURITY & AUTHENTICATION WITHOUT ENCRYPTION OR A DIGITAL SIGNATURE.
Remember that users can open and modify the cookie on a computer. Most of the time they might just impact their own experience, but it's a bad idea to save the payment information, email addresses, or any non-encrypted PII in a cookie.


## Encrypted Cookie Content
Since we are talking about security & authentication, sometimes you need to store some sensitive information. On that case you can do one of two things: 1) You can encrypt the content of the cookie using a Symmetric-key encryption (using a single private-key), or 2) You can use a digital signature using a private-key to validate there have been no tampering with the cookie value.

### Example of Encrypted Cookie Value
Imagine that you want to save the UserId of the authenticated user (The "Remember me" checkbox on your sign in form). You also want to make it trivial for you to set/retrieve that UserId and isolate the complexities of encryption from the rest of your code, so that you can do it like this:

```csharp
public class MyController : Controller
{
     public MyCookie UserCookie { get; set; }

    protected override void Initialize(RequestContext requestContext)
	{
		base.Initialize(requestContext);
        UserCookie = new MyCookie(HttpContext);
	}

    public ActionResult Signin(SigninViewModel signinViewModel)
    {
        // Do sign in 
        if(signinSuccessful)
        {
            UserCookie.UserId = myUser.UserId;
        }    
    }
    // ...
    public ActionResult MyAccount()
    {
        var userId = UserCookie.UserId;
        // ...
    }
}

And the SmartCookie to support this pattern looks like this:

```csharp
public class MyCookie : SmartCookie
{
    private static byte[] _EncryptionKey;
        
    public TestCookie4(HttpContextBase context)
        : base(context)
    {
    }

    static public void SetEncryptionKey(byte []encrytpionKey)
    {
        _EncryptionKey = encrytpionKey;
    }

    [SmartCookieSkip]
    public int UserId
    {
        get
        {
            if (string.IsNullOrEmpty(IV) || string.IsNullOrEmpty(UserIdEncrypted))
                return 0;

            var ivBytes = Convert.FromBase64String(IV);
            var encryptedBytes = Convert.FromBase64String(UserIdEncrypted);

            using (var aes = new AesManaged())
            using(var decryptor = aes.CreateDecryptor(_EncryptionKey, ivBytes))
            using(var ms = new MemoryStream(encryptedBytes))
            using(var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            {
                var decrypted = new byte[4];
                cs.Read(decrypted, 0, 4);
                return BitConverter.ToInt32(decrypted, 0);
            }
        }

        set
        {
            if (value <= 0)
            {
                IV = null;
                UserIdEncrypted = null;
                return;
            }

            using (var aes = new AesManaged())
            {
                aes.GenerateIV();
                IV = Convert.ToBase64String(aes.IV);

                using(var encryptor = aes.CreateEncryptor(_EncryptionKey, aes.IV))
                using(var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    var bytes = BitConverter.GetBytes(value);
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();

                    byte[] encryptedBytes = ms.ToArray();
                    UserIdEncrypted = Convert.ToBase64String(encryptedBytes);
                }
            }
        }
    }

    [SmartCookieSettings("i")]
    protected string IV { get { return GetValue(); } set { SetValue(value); } }
        
    [SmartCookieSettings("u")]
    protected string UserIdEncrypted { get { return GetValue(); } set { SetValue(value); } }
}





## Contributors

- SmartCookie was originally created by *Marcelo Calbucci* ([blog.calbucci.com](http://blog.calbucci.com) | [@calbucci](http://twitter.com/calbucci))