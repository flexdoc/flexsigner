﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Principal;

using System.Security.Cryptography.Pkcs;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using iTextSharp.text.log;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace FlexSignerService
{
    public class SignX509
    {
        X509Store x509Store;
        IList<X509Certificate> chain;
        IOcspClient ocspClient;
        ITSAClient tsaClient;
        IList<ICrlClient> crlList;
        X509Certificate2 pk;

        private string certNum = "";

        public int NumberOfCertificatesFound = 0;
        private readonly Log _log = new Log();

        private string _chave = @"A4TyX3KUXrqK8weoUtQxJHrWEXPDmsTG";
        private byte[] _IV = new byte[16];

        public SignX509(string certNum)
        {
            this.certNum = certNum;
            bool x = PrepareCert(certNum, StoreLocation.LocalMachine);

            if(!x)
                x = PrepareCert(certNum, StoreLocation.CurrentUser);

        }

        private bool PrepareCert(string certNum, StoreLocation storeLocation)
        {
            NumberOfCertificatesFound = 0;
            x509Store = new X509Store(storeLocation);
            x509Store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = x509Store.Certificates;
            chain = new List<X509Certificate>();
            pk = null;

            bool found = false;

            if (certificates.Count > 0)
            {
                foreach(X509Certificate2 cert in certificates)
                {
                    pk = cert;
                    _log.Debug(pk.FriendlyName.ToString() + " - " + pk.NotAfter);

                    if (pk.Subject.Contains(certNum))
                    {
                        DateTime dt = pk.NotAfter;
                        if (dt > System.DateTime.UtcNow)
                        {
                            _log.Debug("Found valid certificate [" + this.certNum + "] valid :[" + dt + "]!");
                            NumberOfCertificatesFound = 1;
                            found = true;
                            break;
                        }                        
                    }
                }

                if (!found)
                {
                    _log.Debug("Certificado [" + this.certNum + "] não encontrado!");
                    return false;
                }

                X509Chain x509chain = new X509Chain();
                x509chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                x509chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                x509chain.Build(pk);

                foreach (X509ChainElement x509ChainElement in x509chain.ChainElements)
                    chain.Add(DotNetUtilities.FromX509Certificate(x509ChainElement.Certificate));
                
            }
            x509Store.Close();

            ocspClient = new OcspClientBouncyCastle();

            tsaClient = null;
            for (int i = 0; i < chain.Count; i++)
            {
                X509Certificate cert = chain[i];
                String tsaUrl = CertificateUtil.GetTSAURL(cert);
                if (tsaUrl != null)
                {
                    tsaClient = new TSAClientBouncyCastle(tsaUrl);
                    break;
                }
            }
            crlList = new List<ICrlClient>();
            crlList.Add(new CrlClientOnline(chain));

            crlList.Clear();

            return true;
        }

        private bool Sign(String src, String dest,
                         ICollection<X509Certificate> chain, X509Certificate2 pk,
                         String digestAlgorithm, CryptoStandard subfilter,
                         String reason, String location,
                         ICollection<ICrlClient> crlList,
                         IOcspClient ocspClient,
                         ITSAClient tsaClient,
                         int estimatedSize)
        {
            bool ret = false;

            // Creating the reader and the stamper
            PdfReader reader = null;
            PdfStamper stamper = null;
            FileStream os = null;
            bool isCripted = false;

            try
            {
                PdfReader.unethicalreading = true;

                // Verifica se é criptografado e decriptografa
                byte[] originalIn = File.ReadAllBytes(src);
                byte[] docIn = originalIn;

                string header = System.Text.Encoding.UTF8.GetString(originalIn, 0, 4);
                isCripted = header.StartsWith("%PDF") == false;

                if (isCripted)
                {
                    byte[] passwordBytes = new FlexCripto().GetAes256SecretKey(_chave);
                    docIn = new FlexCripto().FileDecrypt(originalIn, passwordBytes, _IV);
                    //File.WriteAllBytes(dest.Replace(".pdf", ".DecriptPortela.pdf"), docIn);
                }

                reader = new PdfReader(docIn);

                AcroFields fields = reader.AcroFields;
                int totSignatures = fields.TotalRevisions;

                int totPages = reader.NumberOfPages;

                os = new FileStream(dest, FileMode.Create);
                stamper = PdfStamper.CreateSignature(reader, os, '\0', null, true);

                //Seta para abrir com thumbnails
                stamper.Writer.ExtraCatalog.Put(PdfName.PAGEMODE, PdfName.USETHUMBS);

                // Creating the appearance
                PdfSignatureAppearance appearance = stamper.SignatureAppearance;

                appearance.CertificationLevel = PdfSignatureAppearance.NOT_CERTIFIED;
                
                // Creating the signature
                IExternalSignature pks = new X509Certificate2Signature(pk, digestAlgorithm);

                MakeSignature.SignDetached(appearance, pks, chain, crlList, ocspClient, tsaClient, 0, subfilter);

                ret = true;
            }
            catch( Exception ex )
            {
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (stamper != null)
                    stamper.Close();
                if (os != null)
                    os.Close();
            }

            // Gera arquivo na pasta TEMP (Criptografado ou não)
            byte[] docOut = File.ReadAllBytes(dest); //  ms.ToArray();

            if (isCripted)
            {
                byte[] passwordBytes = new FlexCripto().GetAes256SecretKey(_chave);
                docOut = new FlexCripto().FileEncrypt(docOut, passwordBytes, _IV);
            }

            File.WriteAllBytes(dest, docOut);

            return ret;
        }

        
        public byte[] ReadByteArrayFromFile(string fileName)
        {
            byte[] buff = null;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    long numBytes = new FileInfo(fileName).Length;
                    buff = br.ReadBytes((int)numBytes);
                }
            }

            return buff;
        }

        /// <summary>
        ///   Function to save byte array to a file
        /// </summary>
        /// <param name="_FileName"> File name to save byte array </param>
        /// <param name="_ByteArray"> Byte array to save to external file </param>
        /// <returns> Return true if byte array save successfully, if not return false </returns>
        public static bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                var _FileStream = new FileStream(_FileName, FileMode.Create, FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception);
            }

            // error occured, return false
            return false;
        }

        public bool CheckIfExistsSignature(string file)
        {
            PdfReader readerInitial = null;
            AcroFields fields = null;
            try
            {
                PdfReader.unethicalreading = true;

                readerInitial = new PdfReader(file);

                fields = readerInitial.AcroFields;
                int totSignatures = fields.TotalRevisions;

                foreach (var field in fields.Fields)
                {
                    string key = field.Key;
                    if (key.ToUpper().Contains("SIGNATURE"))
                        return true;
                }

            }
            catch (Exception e)
            {

            }
            finally
            {
                if (fields != null)
                    fields = null;
                if (readerInitial != null)
                    readerInitial.Close();

            }
            return false;
        }

        public bool SignPDF(string fileIn, string fileOut, string reason = "-", string location= "BR")
        {
            bool ret = Sign(fileIn, fileOut, chain, pk, DigestAlgorithms.SHA1, CryptoStandard.CMS, reason, location, crlList, ocspClient, tsaClient, 0);
            return ret;
        }
                
    }
}
