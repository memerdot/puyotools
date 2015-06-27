# Introduction #

Lz00 is an encrypted Lz77/Lz01 based compression format.

# Encryption #
All the compressed data is encrypted using the 32 bit key stored in the file at offset 0x34. This key also appears to be the date on which the data was compressed, but that appears to be irrelevant.

Before decrypting or encrypting a byte, the magic key/value must be updated with a new value. This must be done before every byte being encrypted/decrypted otherwise the new data will be incorrect. This function will generate a new magic key/value (note that the following code is written in C#):
```
// xValue = Magic Key/Value
uint x = (((((((xValue << 1) + xValue) << 5) - xValue) << 5) + xValue) << 7) - xValue;
x = (x << 6) - x;
x = (x << 4) - x;

xValue = ((x << 2) - x) + 12345;
```

After generating the new magic key/value, you can then encrypt or decrypt the byte. This function will do that (note that the following code is written in C#):
```
// xValue = Magic Key/Value
// value = Current byte to encrypt/decrypt
uint t0 = ((uint)xValue >> 16) & 0x7fff;
value ^= (byte)((uint)(((t0 << 8) - t0) >> 15));
```