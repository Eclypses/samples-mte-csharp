import { arrayBufferToBase64, base64ToArrayBuffer } from "./utils.js";

/**
 * Elliptical Curve Diffie Helman options to use.
 * https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/deriveKey#supported_algorithms
 */
const algorithm = {
    name: "ECDH",
    namedCurve: "P-256",
};

//
// Variables to hold the two private keys
// so that the computeSharedSecret method can use them.
//
let encoderPrivateKeyB64;
let decoderPrivateKeyB64;
let arbitraryPrivateKeyB64;

/**
 * Generates ECDH keys, and returns a base64 representation of the public key.
 * @param flavor A string used to ensure the proper private key is preserved.
 * @returns A base64 representation of the public key.
 */
export const getEcdh = async function (flavor) {
    const keys = await window.crypto.subtle.generateKey(algorithm, true, [
        ["deriveKey"],
    ]);
    //
    // NOTE: NodeJS requires the public key in "raw" format, whereas .Net requires it in "spki" format.
    //
    const publicKeyData = await window.crypto.subtle.exportKey("spki", keys.publicKey);
    const publicKeyB64 = arrayBufferToBase64(publicKeyData);

    //
    // Save the private key for use in computeSharedSecret
    //
    const privateKeyData = await window.crypto.subtle.exportKey('pkcs8', keys.privateKey);
    if (flavor == "encoder") {
        encoderPrivateKeyB64 = arrayBufferToBase64(privateKeyData);
    } else if (flavor == "decoder") {
        decoderPrivateKeyB64 = arrayBufferToBase64(privateKeyData);
    } else {
        arbitraryPrivateKeyB64 = arrayBufferToBase64(privateKeyData);
    }
    return publicKeyB64;
}

/**
 * Take in a foreign public key as a base64 string, and return a shared secret as an array buffer.
 * @param flavor A string used to ensure the proper private key is hydrated.
 * @param serverPublicKey A base64 encoded foreign public key.
 * @returns A string with a base64 encoded shared secret.
 */
export const computeSharedSecret = async function (flavor, serverPublicKeyB64) {

    let privateKeyB64;
    if (flavor == "encoder") {
        privateKeyB64 = encoderPrivateKeyB64;
    }
    else if (flavor == "decoder") {
        privateKeyB64 = decoderPrivateKeyB64;
    }
    else {
        privateKeyB64 = arbitraryPrivateKeyB64;
    }

    const privateKey = await window.crypto.subtle.importKey(
        'pkcs8',
        new Uint8Array(base64ToArrayBuffer(privateKeyB64)),
        algorithm,
        true,
        ['deriveKey', 'deriveBits'],
    );
    const publicKey = await window.crypto.subtle.importKey(
        'spki',
        new Uint8Array(base64ToArrayBuffer(serverPublicKeyB64)),
        algorithm,
        true,
        [],
    );

    const sharedSecret = await window.crypto.subtle.deriveBits(
        { name: 'ECDH', public: publicKey },
        privateKey,
        256,
    );

    //
    // .Net generates a SHA-256 hashed result from the deriveBits
    // call, so for java script to work with it, we must do the
    // same here on the client so that entropy will properly match.
    //
    const sharedSecretHash = await crypto.subtle.digest('SHA-256', sharedSecret);
    const sharedSecretArray = new Uint8Array(sharedSecretHash);
    const sharedSecretHashB64 = btoa(String.fromCharCode.apply(null, sharedSecretArray));

    //
    // The following console logs will assist with debugging.
    // console.log("About to generate shared secret - " + flavor);
    // console.log("server PublicKeyB64 for derivation: " + flavor + " - " + serverPublicKeyB64);
    // console.log("client sharedSecret: " + flavor + " - " + sharedSecretHashB64);
    //

    return sharedSecretHashB64;
};