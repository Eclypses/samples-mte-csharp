# MteSDRTest Solution
## Introduction
This solution contains a *.net 6* set of projects that illustrate the use of the *Eclypses SDR*. It contains the following projects:
- *MteSDRTest.MteWrapper* -- this is the language interface for the *Eclypses MTE* in *C#*.
- *MteSDRTest.Client* -- This is a *Blazor C#* front end client application.
- *MteSDRTest.Common* -- This is a *.net 6* library of objects shared between the client and the server.
- *MteSDRTest.Server* -- This is a *.net 6* API Server that pairs with the client.  

Each of these projects is detailed below.
## MteSDRTest.MteWrapper
This is a direct copy of version 2.2.0 of the *C#* language interface from the *Eclypses* distribution site. It is referenced by the *MteSDRTest.Server*.
## MteSDRTest.Client
This is a *.net 6 Blazor* application with the following folders:
### wwwroot
The follwoing files are in this folder.
- appsettings.json -- this contains runtime settings for the client such as the url of the API server.
- index.html -- this is the page that *Blazor* actually loads. It does have references to a couple of java script files.
    - js/jsHelpers.js -- contains some simple javascript helpers that *Blazor* applications need.
    - js/jsCrypto.js -- this wraps some calls to the window.crypto javascript library that are not supported in *Blazor*.
- The following files are imported by the *.net jsInterop* and are not explicitly referenced in *index.html*.
  - js/ecdh.js -- this wraps javascript calls to manage Elliptical Curve Diffie-Hellman via *window.crypto.subtle*.
  - js/Mte.js -- this is the java script wrapper and the actual WASM for the *Eclypses MTE* library.
  - js/mte-helper.js -- this is a high level wrapper around *Mte.js*.
  - js/utils.js -- some miscellaneous java script helpers.
### Components
This contains some reusable components that can be embedded on a razor web page.
### Helpers
This contains some helper classes to handle low-level application functions. The files in this folder are as follows:
- BrowserStorageHelper -- this reads raw data from the local and session storage in the browser to display in the UI.
- MyAuthenticationStateProvider -- this manages the authentication state for the *Blazor* application. It implements abstract class (*AuthenticationStateProvider*) which is a *Blazor* framework class.
- WebHelper -- this handles http calls to the api server.
### Models
This contains some data-only models that are used only by the client.
- Alert -- this has properties used by the *AlertService*.
- DataDisplayModel -- the properties in this model are displayed in the UI.
- StateContainer -- this object is available in all classes and contains application level data such as the *UserPrincipal* of the logged in user.
### Pages
This contains the *razor* pages in this application.
- Login.razor -- the initial page that accepts and manages login.
- ShowData.razor -- the page that actually shows the functionality of the *Eclypses SDR*.  
### Services
This contains code that is referenced from the pages. These services are all injected at startup and can be used throughout the application.
- AlertService -- manages the UI for alerts.
- AuthService -- manages authorization with the API.
- ECDHService -- manages interaction with the Elliptical Curve Diffie-Hellman functions that are used for *Eclypses MTE* pairing.
- MteService -- manages calls to the WASM based *Eclyses MTE* methods.
- PayloadService -- provides a high-level interface to the *Eclypses MTE* for manipulating payloads to exchange with the API.
- SDRService -- exchanges data with the API to get consistent values to initialize the *Eclypses SDR*.  
### Shared
Contains the layouts for the *Blazor* pages.
## MteSDRTest.Common
This is a *.net 6* library that is shared amongst the client and the server with the following folders:
### Helpers
The following files are in this folder.
- CryptoHelper -- This is a wrapper around the *.net* cryptography methods (note - even though it is in *common* it is only used by the server since Blazor does not support all of these calls).
### Models
The following files are in this folder.
- ClientCredentials -- This contains the userid and password properties and is sent from the client to the server to authenticate.
- ClientPairModel --  This contains the properties that the client supplies on an *Eclypses MTE* pairing request.
- ClientUserModel -- This is the object that the authorization (login) call returns upon success.
- Constants -- This contains constant values shared by client and server.
- DataExchangeModel -- This contains arbitrary data that the client exchanges with the server - in this example, it is the vehicle for exchanging the *Entropy* and *Nonce* values needed for the consistent and persistent local storage *Eclypses SDR*.
- ServerPairModel -- This contains the properties that the server returns to the client for an *Eclypses MTE* pairing request.
- ServerSDRValues -- This contains the properties that are serialized into the *data* property of the *DataExchangeModel* and it contains the *Entropy* and *Nonce* values associated with a specific workstation.  
## MteSDRTest.Server
This is a *.net 6* web API that services the Client with the following folders:
### Controllers
The following files are in this folder.
- DataExchangeController -- this handles the route for exchanging the *DataExchangeModel*. It requires authorization and its incoming and outgoing payloads are protected with the *Eclypses MTE*.
- EchoController -- this returns an echo string to verify that the API is alive - it is not authorized, and not protected by the *Eclypses MTE*.
- LoginController -- this handles a login / authorization request - it is not authorized since that is what it does, but it *is* protected with the *Eclypses MTE*.
- MtePairController -- this handles the pairing request from the client to the server. Since pairing happens prior to authorization, it is not authorized and the payload is *not* protected with the *Eclypses MTE*.
### Helpers
The following files are in this folder.
- CacheHelper -- this manages reading and writing to the distributed cache.
- ECDHHelper -- this manages interactions with the *.net* cryptography library for Elliptical Curve Diffie-Hellman.
- JwtHelper -- this manages the creation and refreshing of the authorization token (the *jwt*).
- MteHelper -- this manages calls to the *.net* version of the *Eclypses MTE*.
- PayloadHelper -- this assists in concealing and revealing the incoming and outgoing payloads by calling into the *MteHelper*.
### Models
The following files are in this folder.
- DerivedKeysModel -- this is returned from the ECDHHelper upon a pair request.
- JwtIssuerOptions -- this wraps the values from *appsettings.json* that the *JwtHelper* uses.
### Services
The following files are in this folder.
- AuthService -- this verifies the incoming login request (in this example if only looks at the password and returns true or false).
- DataExchangeService -- this handles the request for the *ServerSDRValues* for a specific workstation.
### Other Files
Other files are in the root of this project.  They include the following:
- appsettings.json -- this contains run time settings for the api server.
- mte.dll - this is the *windows* version of the compiled *Eclypses MTE*.
- Program.cs -- This is the main entry point for the api server.
- Startup.cs -- This manages the dependency injection registration and pipeline configuration for the api server.
