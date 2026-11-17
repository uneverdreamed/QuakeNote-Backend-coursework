using System.Security.Cryptography;

Directory.CreateDirectory("keys");
using var rsa = RSA.Create(2048);
File.WriteAllText("keys/jwt-private.pem", rsa.ExportPkcs8PrivateKeyPem());
File.WriteAllText("keys/jwt-public.pem", rsa.ExportSubjectPublicKeyInfoPem());
Console.WriteLine("Keys generated in ./keys");
