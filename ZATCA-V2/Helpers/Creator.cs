using ZATCA_V2.Models;

namespace ZATCA_V2.Helpers;

public class Creator
{   public static void WriteConfigurationFile(string filePath, CertificateConfiguration certificateConfig)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            if (directory != null) Directory.CreateDirectory(directory);
        }
        using (StreamWriter writer = File.CreateText(filePath))
        {
            writer.WriteLine("oid_section= OIDS");
            writer.WriteLine("[ OIDS ]");
            writer.WriteLine("certificateTemplateName= 1.3.6.1.4.1.311.20.2");
            
            writer.WriteLine("[req]");
            writer.WriteLine("default_bits=2048");
            writer.WriteLine($"emailAddress=={certificateConfig.EmailAddress}");
            writer.WriteLine("req_extensions=v3_req");
            writer.WriteLine("x509_extensions=v3_Ca");
            writer.WriteLine("prompt=no");
            writer.WriteLine("default_md=sha256");
            writer.WriteLine("req_extensions=req_ext");
            writer.WriteLine("distinguished_name=req_distinguished_name");
            writer.WriteLine("");
            
            
            writer.WriteLine("[req_distinguished_name]");
            writer.WriteLine($"C={certificateConfig.C}");
            writer.WriteLine($"OU={certificateConfig.OU}");
            writer.WriteLine($"O={certificateConfig.O}");
            writer.WriteLine($"CN={certificateConfig.CN}");
            writer.WriteLine("");

            writer.WriteLine("[v3_req]");
            writer.WriteLine("basicConstraints = CA:FALSE");
            writer.WriteLine("keyUsage = nonRepudiation, digitalSignature, keyEncipherment");
            writer.WriteLine("");
            writer.WriteLine("");
            
            writer.WriteLine("[req_ext]");
            writer.WriteLine($"certificateTemplateName = ASN1:PRINTABLESTRING:{certificateConfig.CertificateTemplateName}");
            writer.WriteLine($"subjectAltName = dirName:alt_names");
            writer.WriteLine("");


            writer.WriteLine("[alt_names]");
            writer.WriteLine($"SN={certificateConfig.SN}");
            writer.WriteLine($"UID={certificateConfig.UID}");
            writer.WriteLine($"title={certificateConfig.Title}");
            writer.WriteLine($"registeredAddress={certificateConfig.RegisteredAddress}");
            writer.WriteLine($"businessCategory={certificateConfig.BusinessCategory}");
        }

        Console.WriteLine($"Generated configuration file: {filePath}");
    }

}