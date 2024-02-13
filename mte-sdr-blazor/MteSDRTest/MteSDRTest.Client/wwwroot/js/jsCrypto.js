/**
 * Returns a base-64 encoded string
 * of a byte array consisting of
 * cryptographically secure random
 * values.
 * @param {any} size The size of the byte array you wish to receive.
 */
function getEntropy(size) {
    var entropy = new Uint8Array(size);
    window.crypto.getRandomValues(entropy);
    var value = btoa(entropy);
    return value;
}

/**
 * Returns a Uint8Array consisting of the
 * encrypted bytes of the incoming byte array.
 * This is intended for one time use, as it uses
 * a 'zero-byte' initialization vector.
 * @param {any} clearBytes The incoming byte array to encrypt.
 * @param {any} keyBytes A byte array of the encryption key - this is used to generate the proper key struct.
 */
async function encrypt(clearBytes, keyBytes) {
    iv = new Uint8Array(16);
    theKey = await generateKey(keyBytes);
    cipherBytes = await window.crypto.subtle.encrypt(
        {
            name: "AES-CBC",
            iv: iv
        },
        theKey,
        clearBytes
    );
    var value = new Uint8Array(cipherBytes);
    return value;

}

/**
 * Returns a Uint8Array consisting of the
 * decrypted bytes of the incoming byte array.
 * This is intended for one time use, as it uses
 * a 'zero-byte' initialization vector.
 * @param {any} cipherBytes The incoming byte array to decrypt.
 * @param {any} keyBytes A byte array of the encryption key - this is used to generate the proper key struct.
 */
async function decrypt(cipherBytes, keyBytes) {
    try {
        iv = new Uint8Array(16);
        theKey = await generateKey(keyBytes);
        clearBytes = await window.crypto.subtle.decrypt(
            {
                name: "AES-CBC",
                iv: iv
            },
            theKey,
            cipherBytes
        );
        var value = new Uint8Array(clearBytes);
        return value;
    } catch (err) {
        console.log(err);
    }
}

/**
 * Creates and imports the proper key struct
 * for use by window.crypto.subtle encrypt and decrypt.
 * @param {any} keyBytes The Uint8Array of raw bytes for the key.
 */
async function generateKey(keyBytes) {
    theKey = await window.crypto.subtle.importKey(
        "raw", //"pkcs8",// "spki", 
        keyBytes,
        "AES-CBC",
        true,
        ["encrypt", "decrypt"]
    );
    return theKey;
}

/**
 * Tests the encryption and decryption methods
 * by encrypting a byte array, decrypting it, and
 * finally returning the decrypted array which 
 * should match the incoming clear bytes.
 * @param {any} clearBytes The incoming Uin8Array of bytes to encrypt.
 * @param {any} keyBytes A byte array of the encryption key - this is used to generate the proper key struct.
 */
async function testCrypto(clearBytes, keyBytes) {
    cipherBytes = await encrypt(clearBytes, keyBytes);
    decrypted = await decrypt(cipherBytes, keyBytes);
    return decrypted;
}