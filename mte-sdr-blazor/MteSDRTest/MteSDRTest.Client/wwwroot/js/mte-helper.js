import {
    MteBase,
    MteWasm,
    MteSdrStorage,
    MteStatus,
    MteMkeEnc,
    MteMkeDec,
} from "./Mte.js";
/**
 * Declare some variables that are global to this module.
 */
let mteWasm = null;
let mteBase = null;
let mtePersistedSdr = null;
let mteSessionSdr = null;

let encoderState = null;
let decoderState = null;
const licenseCompany = "Eclypses Inc.";
const licenseKey = "Eclypses123";
/**
 * An asynchronous function that instantiates MteWasm, then sets up MteBase for future use.
 * This MUST be called before any other MTE methods can be used, usually as soon as the website loads.
 */
export async function instantiateMteWasm() {
    // assign mteWasm variable, and instantiate wasm
    mteWasm = new MteWasm();
    await mteWasm.instantiate();

    // assign mteBase variable
    mteBase = new MteBase(mteWasm);

    // If applicable, Initialize MTE license
    if (!mteBase.initLicense(licenseCompany, licenseKey)) {
        const licenseStatus = MteStatus.mte_status_license_error;
        const status = mteBase.getStatusName(licenseStatus);
        const message = mteBase.getStatusDescription(licenseStatus);
        throw new Error(`Error with MTE License.\n${status}: ${message}`);
    }

    return true;
}

/**
 * Initializes a local MteSdr
 * to use for securely storing and retrieving
 * data items from the browser storage.
 * @param {any} category This allows for specifying what kind of data you with to manage.
 * @param {any} entropy This is a byte array of secure values to use for entropy.
 * @param {any} nonce This is a ulong used to instantiate the MTE.
 * @param {any} persist If this is true, the data is stored in LocalStorage, otherwise it is kept in Session Storage.
 */
function initializeSdr(category, entropy, nonce, persist) {
    const mteSdr = MteSdrStorage.fromdefault(mteWasm, category, persist);
    mteSdr.initSdr(entropy, nonce);
    return mteSdr;
}

/**
 * Initializes a LocalStorage SDR for persisted data between sessions.
 * @param {any} category This allows for specifying what kind of data you with to manage.
 * @param {any} entropy This is a byte array of secure values to use for entropy.
 * @param {any} nonce This is a ulong used to instantiate the MTE.
 */
export async function initializePersistentSdr(category, entropy, nonce) {
    mtePersistedSdr = initializeSdr(category, entropy, nonce, true);
}

/**
 * Initializes a SessionStorage SDR for data only kept within this browser session.
 * @param {any} category This allows for specifying what kind of data you with to manage.
 * @param {any} entropy This is a byte array of secure values to use for entropy.
 * @param {any} nonce This is a ulong used to instantiate the MTE.
 */
export async function initializeSessionSdr(category, entropy, nonce) {
    mteSessionSdr = initializeSdr(category, entropy, nonce, false);
}

/**
 * Retrieves, reveals, and returns the requested data item.
 * @param {any} name The name associated with the item you with to retrieve.
 * @param {any} persistent If this is true, the data is retrieved from LocalStorage, otherwise it is retrieved from Session Storage.
 */
export async function read(name, persistent) {
    try {
        if (persistent) {
            return mtePersistedSdr.readString(name);
        } else {
            return mteSessionSdr.readString(name);
        }
    } catch (err) {
        return null;
    }
}

/**
 * Conceals and stores the requested data item.
 * @param {any} name The name associated with the item you with to store.
 * @param {any} persistent If this is true, the data is stored in LocalStorage, otherwise it is kept in Session Storage.
 */
export async function write(name, data, persistent) {
    if (persistent) {
        mtePersistedSdr.writeString(name, data);
    } else {
        mteSessionSdr.writeString(name, data);
    }
}

/**
 * Removes the requested data item from storage.
 * @param {any} name The name associated with the item you with to remove.
 * @param {any} persistent If this is true, the data is removed from LocalStorage, otherwise it is removed from Session Storage.
 */
export async function remove(name, persistent) {
    if (persistent) {
        mtePersistedSdr.remove(name);
    } else {
        mteSessionSdr.remove(name);
    }
}

/**
 * Initialize MTE encoder/decoder and saves their states
 * @param {string} personalization The personalization string
 * @param {string} nonce A string of integers
 * @param {Uint8Array} encoderEntropy A Uint8Array for the encoder entropy
 * @param {Uint8Array} decoderEntropy A Uint8Array for the decoder entropy
 */
export function createInitialMteState(
    encoderEntropy,
    decoderEntropy,
    nonce,
    personalization
) {
    // create encoder
    const mteEncoder = MteMkeEnc.fromdefault(mteWasm);
    mteEncoder.setEntropyStr(encoderEntropy);
    mteEncoder.setNonce(nonce);
    const encoderStatus = mteEncoder.instantiate(personalization);
    if (encoderStatus !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(encoderStatus);
        const message = mteBase.getStatusDescription(encoderStatus);
        throw new Error(`Error instantiating MTE Encoder.\n${status}: ${message}`);
    }
    // save mte encoder state
    encoderState = mteEncoder.saveStateB64();
    // destroy encoder
    mteEncoder.uninstantiate();
    mteEncoder.destruct();

    // create decoder
    const mteDecoder = MteMkeEnc.fromdefault(mteWasm);
    mteDecoder.setEntropyStr(decoderEntropy);
    mteDecoder.setNonce(nonce);
    const decoderStatus = mteDecoder.instantiate(personalization);
    if (decoderStatus !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(decoderStatus);
        const message = mteBase.getStatusDescription(decoderStatus);
        throw new Error(`Error instantiating MTE Decoder.\n${status}: ${message}`);
    }
    // save mte decoder state
    decoderState = mteDecoder.saveStateB64();
    // destroy decoder
    mteDecoder.uninstantiate();
    mteDecoder.destruct();
}

/**
 * Use MKE to encrypt a uint8Array
 * - create mte encoder, restore it from saved mte state
 * - encode the array
 * - save new encoder state, destroy encoder
 * - return encrypted result
 */
export function mkeEncryptUint8Array(arr) {
    const encoder = MteMkeEnc.fromdefault(mteWasm);
    encoder.restoreStateB64(encoderState);
    const encodeResult = encoder.encode(arr);
    if (encodeResult.status !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(encodeResult.status);
        const message = mteBase.getStatusDescription(encodeResult.status);
        throw new Error(`Error when MKE encrypting data.\n${status}: ${message}`);
    }
    encoderState = encoder.saveStateB64();
    encoder.uninstantiate();
    encoder.destruct();
    return encodeResult.arr;
}

/**
 * Uses the MkeEncoder to encode a string and return it as base64
 * @param str A string value to MTE encode.
 * @return An MTE encoded value as a base64 encoded string.
 */
export function mkeEncryptString(str) {
    const encoder = MteMkeEnc.fromdefault(mteWasm);
    encoder.restoreStateB64(encoderState);
    const encodeResult = encoder.encodeStrB64(str);
    // check if encode failed
    if (encodeResult.status !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(encodeResult.status);
        const message = mteBase.getStatusDescription(encodeResult.status);
        throw new Error(`Error when MTE encoding data.\n${status}: ${message}`);
    }
    encoderState = encoder.saveStateB64();
    encoder.uninstantiate();
    encoder.destruct();
    return encodeResult.str;
}

/**
 * Use MTE to decrypt a uint8Array
 * - create mke decoder, restore it from saved mte state
 * - decode the array
 * - save new decoder state, destroy decoder
 * - return decrypted result
 */
export function mkeDecryptUint8Array(arr) {
    const decoder = MteMkeDec.fromdefault(mteWasm);
    decoder.restoreStateB64(decoderState);
    const decodeResult = decoder.decode(arr);
    if (decodeResult.status !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(decodeResult.status);
        const message = mteBase.getStatusDescription(decodeResult.status);
        throw new Error(`Error when MKE decrypting data.\n${status}: ${message}`);
    }
    decoderState = decoder.saveStateB64();
    decoder.uninstantiate();
    decoder.destruct();
    return decodeResult.arr;
}

/**
 * Uses the MkeDecoder to decode a base64 encoded payload and return it as string
 * @param str An MKE encoded value as a base64 encoded string.
 * @return A string.
 */
export function mkeDecryptString(str) {

    const decoder = MteMkeDec.fromdefault(mteWasm);
    decoder.restoreStateB64(decoderState);

    const decodeResult = decoder.decodeStrB64(str);
    // check if decode failed
    if (decodeResult.status !== MteStatus.mte_status_success) {
        const status = mteBase.getStatusName(decodeResult.status);
        const message = mteBase.getStatusDescription(decodeResult.status);
        throw new Error(`Error when MTE decoding data.\n${status}: ${message}`);
    }
    decoderState = decoder.saveStateB64();
    decoder.uninstantiate();
    decoder.destruct();
    return decodeResult.str;
}