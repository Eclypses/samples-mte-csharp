using System;
using Eclypses.MTE;
using Eclypses.MTE.Interop;

namespace MteJailbreakTest
{
    class Cbs : MteJail
    {
        public Cbs(MteInterop interop) : base(interop) { }
        public byte[] GetMutated()
        {
            return myMutated;
        }

        public override void NonceCallback(int minLength,
            int maxLength,
            byte[] nonce,
            out int nBytes)
        {
            //--------
            // Super.
            //--------
            base.NonceCallback(minLength, maxLength, nonce, out nBytes);

            //------------------------------------
            // Retain a copy of the mutated nonce.
            //------------------------------------
            myMutated = new byte[nBytes];
            Array.Copy(nonce, myMutated, nBytes);
        }
        //----------------
        // Mutated nonce.
        //----------------
        byte[] myMutated;
    }
}
