# Securing Local Data in the Browser
## Introduction
Today, many modern web applications choose to store data within the browser to minimize round trips to a server and to off-load processing and storage requirements at the server.  This data may be something as straight forward as a user's name and address, to something more complicated such as billing information or shopping cart contents. Whatever data needs to be retained, it takes on one of these major classifications.
- Short term storage that is relevant for  part of a sessioin (*inMemory*).
- Cookie storage that is used by an application but is volitale (*cookie*).
- Temporary data only relevant during a specific session (*sessionStorage*).
- Sticky (semi-permanent) data that may be relevant in subsequent sessions for use by the application (*localStorage*).  

  
Unfortunately, the information stored in any of these classifications is vulnerable to snooping or hijacking by un-intended applications or other web sites. This article *https://www.wired.com/story/ways-facebook-tracks-you-limit-it/* highlights some of the ways that data is harvested by use in ways you may not intend.  
If your application uses data that would be of value to others (sensitive or not) and you want it to be off-limits to anything other than its intended use, you must take steps to protect it.  
Modern browsers provide the mechanism to manage this data, but the data is only stored in *key/value* pairs in plain text. You may wish to encrypt this data prior to storage, and decrypt it prior to processing, but this still requires that you manage the encryption keys which must still be in plain text.  Furthermore, if the data is *Sticky*, you must ensure that the encryption / decryption keys are kept someplace which also involves an attack surface that is easily compromised.  See the following from Microsoft's documentation for *.Net* *https://docs.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-6.0&pivots=webassembly#browser-storage-wasm*. The relative quote from this page follows:
```
Warning
Users may view or tamper with the data stored in localStorage and sessionStorage.
```
Fortunately, the patented, state-of-the-art technology available from *Eclypses Inc.* can resolve this issue.  This document describes how your application can protect your most valuable asset, your data.  
## Eclypses SDR (Secure Data Replacement)
Building on the FIPS 140-3 validated technology of *Eclypses MTE*, an add-on has been developed to protect the data within the browser storage. This add-on is named *SDR* for *Secure Data Replacement*.  The following design patterns illustrate the usage of this technology specifially for *sessionStorage* and *localStorage*. The *Eclypses SDR* can also be used to protect short term data *inMemory* and data stored in a *cookie*, and the developer's guide details this usage.
### SDR Creation
The following javascript function returns an instance of the *Eclypses SDR* for either *sessionStorage* or *localStorage* based on the *persist* value.  

```js
function initialize(category, persist, entropy, nonce) {
    // instantiate the Mte WASM (WebAssembly) object.
    let mteWasm = new MteWasm();
    await mteWasm.instantiate();
    // category is a run time parameter that determines the high level portion of the data key.
    // if persist is true, this will use localStorage, if false, it uses sessionStorage.
    const mteSDR = MteSDRStorage.fromdefault(category, persist);
    // entropy is a secure array of random bytes used to initialize the Mte.
    // nonce is a string representation of a long integer used to initialize the Mte.
    MteSDRTest.initSDR(entropy, nonce);
    return mteSDR;
}
```

If you are using the *sessionStorage*, your value for *entropy* can be created within your application. with code similar to this:
``` js
function getEntropy(size) {
    var entropy = new Uint8Array(size);
    window.crypto.getRandomValues(entropy);
    var value = btoa(entropy);
    return value;
}
```
Your value for *nonce* can be the current timestamp expressed as an integer.
If you are using the *localStorage*, your value for *entropy* and *nonce* must be consistent. This is most easily obtained from an outside API call that receives these values from your server. An appendix to this document outlines secure ways to obtain these values.  
### SDR Usage
Once you have created your *SDR* object, you can manage data within that object by using the *writeString*, *readString*, and *remove* javascript functions. The *name* parameter identifies the specific piece of data you wish to manage. It (along with the *category* you used when you initialized) make up the actual key in your storage.
``` js
MteSDRTest.writeString(name, value);
return MteSDRTest.readString(name);
MteSDRTest.remove(name);
```
The mteSDR you originally initialized is the object that these functions work with.  If you wish to view the actual raw data within your browser, you will find the data has a key of *category/name* so if you initialized your *mteSDR* with a category of *secretstuff* and the *name* you provided was *accountnumber*, the data will be stored in either *sessionStorage* or *localStorage* with the key of *secretstuff/accountnumber*.  
An actual example could be for the account number of **12345678**, the actual data stored in the *Eclypses SDR* is
```
AAAAAAAAAAAAAAGCY8IXLAB2/P5cCFpq0uX/YeX4RNEEw8UqhurVXaq4MBpxUsTE2s6lbcdlj26sAV0EIbGgkEEtdtYiQa/eYQ/eniftocmaxu1Wum0ip38pWR6+OtsOZm8WXoMdLG+kAgTM8ANTnz9vdYSFsBM/gHyKbZo70UQ+VCmLOW5dvT35uhCW/YWLrEu6DoL+QbuFFAlVFumzTJrJOAJJ8g9UfrkWJdZFK7l4/k25YFnuiGmUboRRF94YQgC+SUsOJTBvTDcJKlg+U9RU+heE4kogMlZkWPKrV8gwtNefWVUcDJJNGo34WI74FDKX57af99Q3xxrweB9tZoJpypL7kncxyInsdypHduENX5So1oO+VHKYOOgH34WcmStCjF53EGbNOWykzpAyXQ2H5ErgpVACjrw2ugCzUUmsJtKQlf3BD9mVTlrighoZIoEpCKhZTVUhYGiVdHxi8JSS8ZBlVD8fYmUEYJxUGvDLluYzAwWSeTZGg3fCfOthX6IysaqffulQlDZ8Cdm0EeWbnlteL22rIXbZyEq74gVZxGTzyEOz1GuYwlpnC8B2HlNScjmZsyCi+d+SGWZr6gfMFiz/oB3q/tYJlA9RGJJm0BRzuJovW0Mg6Dks6ctIDZx8gazyfLvEutH5fLovyWqRVke/GItinHj8aaJLNxXJE0XbFmvQra17EnImgRFU8jxBM8Ge+blsuPEIWPqVTng57N1N4kHRhSTFxIIIauBWMnkPS7z/7lcOlBGo+HCgoLzZrhKGnwge3pbfB4TKjTIIf2XnBOL1xC9IckXGN/4RY4eyTB9nbB+eBhht0vqgXd+XZa+3S4xQeOjA3cebvw4SZjo23b3GCkEAEBGGznKnUk3azFzwO/HO2prx0D+VLNSYCDOA1XiJhzGS+WFtu1k+vF3C881xsHwaygWxtvInXjEWJdg97PlkLGJjlX/0KQJWIVZDepeqAAAAAA==
```
and even if you store the same value a second time, it will be completely different due to the instant obsolescense of the *Eclypses MTE* technology.  
## Conclusion
When your application needs to absolutely, positively ensure the security of data within your web page, the *Eclypses SDR* (based on the *Eclypses MTE*) is the only way to protect this important information.

## Appendix A - Establishing consistent secure local storage with use of your paired *Eclypses MTE*
When your storage needs must survive browser restarts or data sharing between *tabs*, the *Eclypses SDR* must be initialized with consistent values. Since it is not secure to store these values within the browser, best practices dictate that you retrieve them from an external source. This presents a challange since the values may be transmitted from an *api* server and even though your connection may be protected by *TLS*, to be absolutely secure these values must be protected.  
If you have already authenticated and are using *Eclypses MTE* in your overall application to protect all of your payloads, the easiest way to protect this data is with your paired *Eclypses MTE* as outlined in the following design pattern.
### *Establish an authenticated session*
- Capture your authentication information from your browser application (typically userid and password).
- Use *Eclypses MTE* to protect your login object and transmit it to your *api* server.
- On the server, use the paired *Eclypses MTE* to reveal the login object and authenticate the request.
- Return user information to the browser.
- Once the application user is authenticated, use the *Eclypses SDR* to write the user information in secure *sessionStorage*.  
- Any time that your application needs the user information, read it using the *Eclypses SDR*.
### *Retrieve the entropy and nonce values*
- Obtain a *workstation identifier* that is consistent for this physical endpoint such as a serial number.
- Populate a *json* object with the *workstation identifier* and protect this object using your already established *Eclypses MTE*.
- Transmit this object to your server.
- Use your paired *Eclypses MTE* at the server to reveal the incoming *json* object from your authenticated endpoint.
- Use the *workstation identifier* to retrieve the *entropy* and *nonce* values from a secure storage facility such as a distributed cache or database.
- If no entry is found, this is the initial request, so create a secure byte array for *entropy* (it should be at least 32 bytes in length) and a long integer for nonce (the current time stamp works well for this) and store them using the *workstation identifier* as the lookup key.
- Populate a *json* object with the entry that you retrieved from cache (this entry contains the peristent *entropy* and *nonce* for the requested *workstation identifier*).
- Use the paired *Eclypses MTE* to protect this *json* object and return it to the browser.
### *Initialize the Eclypses SDR for localStorage at the browser*
- Use your paired *Eclypses MTE* to reveal the incoming *json* object.
- Use the *entropy* and *nonce* values from the *json* object to initialize the *localStorage* instance of the *Eclypses SDR*.
- Zeroize the returned *json* object to ensure security.
### *Use your secure localStorage*
- Any time your application needs to securely retrieve information from your persisted *localStorage* simply issue the *readString* method with the appropriate key.
- Your application now has a clear text version of the item that your previously issued a *writeString* for.
- If you wish to clear out that item, use the *remove* function and it will be removed from your browser's *localStorage*.

## Appendix B - Establishing consistent secure local storage without using your paired *Eclypses MTE*
Alternatively, you can utilize a key exchange methodology to generate a one-time use *encryption key* to protect your information.  
This presents a further challange in ensuring that the same *encryption key* is used at the *api* that the browser session has to decrypt the incoming values. Following is a design pattern that can ensure the protection of this data.  
### *Establish an authenticated session*
- Capture your authentication information from your browser application (typically userid and password).
- Transmit this to your server and authenticate the user creating some user information.
- Return that user information to the browser.
- Once the application user is authenticated, use the *Eclypses SDR* to write the user information in secure *sessionStorage*.  
- Any time that your application needs the user information, read it using the *Eclypses SDR*.
### *Establish a one-time use key pair*
- Obtain a *workstation identifier* that is consistent for this physical endpoint such as a serial number.
- Use a key algorithm such as *Elliptical Curve Diffie-Hellman* to generate a public / private key pair in the browser.
- Create a *json* object with the *workstation identifier* and the newly created *public key*.
- Transmit this information to your *api* server.
### *Retrieve the entropy and nonce values*
- At the server, use the *workstation identifier* to retrieve the *entropy* and *nonce* values from a secure storage facility such as a distributed cache or database.
- If no entry is found, this is the initial request, so create a secure byte array for *entropy* (it should be at least 32 bytes in length) and a long integer for nonce (the current time stamp works well for this) and store them using the *workstation identifier* as the lookup key.
- Use the same key algorithm that the browser used to generate a public / private key pair at the server.
- Use the browser's public key and the server's private key to generate a one-time use encryption key.
- Create a *json* object with the *entropy* and *nonce* associated with the incoming *workstation identifier*.
- Encrypt the *json* object with the one-time use encryption key.
- Zeroize the clear text values that you just encrypted.
- Return the server's public key and the encrypted *json* object to the browser.
### *Initialize the Eclypses SDR for localStorage at the browser*
- Use the server's public key and the browser's private key to generate the same one-time use decryption key that the server used.
- Decrypt the *json* object in the incoming payload.
- Use the *entropy* and *nonce* values from the decrypted *json* object to initialize the *localStorage* instance of the *Eclypses SDR*.
- Zeroize the returned *json* object to ensure security.
### *Use your secure localStorage*
- Any time your application needs to securely retrieve information from your persisted *localStorage* simply issue the *readString* method with the appropriate key.
- Your application now has a clear text version of the item that your previously issued a *writeString* for.
- If you wish to clear out that item, use the *remove* function and it will be removed from your browser's *localStorage*.

