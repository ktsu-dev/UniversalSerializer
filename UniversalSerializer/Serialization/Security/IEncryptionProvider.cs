// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Security;

/// <summary>
/// Interface for encryption providers that can encrypt and decrypt data.
/// </summary>
public interface IEncryptionProvider
{
	/// <summary>
	/// Gets the encryption algorithm this provider supports.
	/// </summary>
	public EncryptionType EncryptionType { get; }

	/// <summary>
	/// Encrypts the specified data.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <param name="key">The encryption key.</param>
	/// <param name="iv">The initialization vector (optional).</param>
	/// <returns>The encrypted data.</returns>
	public byte[] Encrypt(byte[] data, byte[] key, byte[]? iv = null);

	/// <summary>
	/// Decrypts the specified encrypted data.
	/// </summary>
	/// <param name="encryptedData">The encrypted data to decrypt.</param>
	/// <param name="key">The decryption key.</param>
	/// <param name="iv">The initialization vector (optional).</param>
	/// <returns>The decrypted data.</returns>
	public byte[] Decrypt(byte[] encryptedData, byte[] key, byte[]? iv = null);

	/// <summary>
	/// Asynchronously encrypts the specified data.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <param name="key">The encryption key.</param>
	/// <param name="iv">The initialization vector (optional).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous encryption operation.</returns>
	public Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[]? iv = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously decrypts the specified encrypted data.
	/// </summary>
	/// <param name="encryptedData">The encrypted data to decrypt.</param>
	/// <param name="key">The decryption key.</param>
	/// <param name="iv">The initialization vector (optional).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous decryption operation.</returns>
	public Task<byte[]> DecryptAsync(byte[] encryptedData, byte[] key, byte[]? iv = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Generates a new encryption key.
	/// </summary>
	/// <returns>A new encryption key.</returns>
	public byte[] GenerateKey();

	/// <summary>
	/// Generates a new initialization vector.
	/// </summary>
	/// <returns>A new initialization vector.</returns>
	public byte[] GenerateIV();
}
