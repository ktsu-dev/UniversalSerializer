// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Security;

/// <summary>
/// Specifies the encryption algorithm to use for securing serialized data.
/// </summary>
public enum EncryptionType
{
	/// <summary>
	/// No encryption.
	/// </summary>
	None,

	/// <summary>
	/// AES encryption with CBC mode.
	/// </summary>
	AESCBC,

	/// <summary>
	/// AES encryption with GCM mode (authenticated encryption).
	/// </summary>
	AESGCM,

	/// <summary>
	/// ChaCha20-Poly1305 authenticated encryption.
	/// </summary>
	ChaCha20Poly1305
}
