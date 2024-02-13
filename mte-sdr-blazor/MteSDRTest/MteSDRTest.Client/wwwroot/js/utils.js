/**
 * Utility to convert a array buffer to a base64 string.
 * @param buffer A typed array.
 * @returns A base64 encoded string of the original array buffer data.
 */
export function arrayBufferToBase64(buffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
}

/**
 * Utility to convert a base64 string into an array buffer.
 * @param base64Str A base64 encoded string.
 * @returns An array buffer.
 */
export function base64ToArrayBuffer(base64Str) {
    const str = window.atob(base64Str);
    let i = 0;
    const iMax = str.length;
    const bytes = new Uint8Array(iMax);
    for (; i < iMax; ++i) {
        bytes[i] = str.charCodeAt(i);
    }
    return bytes.buffer;
}

/**
 * Use Javascript to create a v4 uuid.
 * Source: https://stackoverflow.com/a/2117523/4927236
 */
export function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, (c) =>
        (
            c ^
            (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (c / 4)))
        ).toString(16)
    );
}