# Image Encryption and Compression

## Overview
This repository offers a robust solution for image encryption and decryption using Linear Feedback Shift Register (LFSR) along with compression and decompression via Huffman Coding and Run-Length Encoding (RLE). Combining these techniques ensures secure transmission and storage of image data while minimizing storage space.

## Features

### Encryption & Decryption with Linear Feedback Shift Register (LFSR)
The encryption process utilizes LFSR to generate a pseudo-random sequence, which scrambles the pixel values of the image. Decryption reverses this process by applying the inverse of the LFSR-generated sequence to retrieve the original image data, ensuring confidentiality and integrity during transmission and storage.

### Enhanced Password Encryption
Passwords for encryption are enhanced using alphanumeric characters instead of binary, increasing the complexity and security of the encryption process.

### Compression & Decompression
This solution implements two compression techniques:
- **Huffman Coding**: Efficiently represents the image data by assigning shorter codes to frequently occurring pixel values and longer codes to less frequent ones, reducing the file size without compromising image quality.
- **Run-Length Encoding (RLE)**: Compresses the image data by representing consecutive runs of the same value with a single value and count.

### Password Cracking
Includes a feature for attempting to crack passwords on encrypted images, providing a way to test and evaluate the strength of the encryption.

### Automatic Test Script
An automatic test script is included to allow users to test any image and compute metrics such as compression ratio, encryption and decryption time, and data integrity verification.

## Usage
1. **Encryption**: Execute the encryption algorithm to encrypt the desired image using LFSR and an alphanumeric password.
2. **Decryption**: Use the same encryption key to decrypt and retrieve the original image.
3. **Compression**: Apply the compression algorithm to compress the image using Huffman Coding or Run-Length Encoding.
4. **Decompression**: Decompress the compressed image to restore the original image data.
5. **Password Cracking**: Utilize the password cracking tool to test the robustness of the encrypted images.
6. **Automatic Testing**: Run the test script to evaluate the metrics of the encryption and compression processes on any image.

## Security Considerations
- **Key Management**: Ensure secure generation, storage, and transmission of encryption keys to prevent unauthorized access.
- **Algorithm Strength**: Regularly assess the robustness of the encryption algorithm to withstand potential cryptographic attacks.
- **Data Integrity**: Implement integrity checks, such as hash functions, to verify the image data during encryption, transmission, and decryption.

## Dependencies
Ensure the following dependencies are installed:
- .NET Framework
- Additional libraries required for image processing and manipulation in C#
